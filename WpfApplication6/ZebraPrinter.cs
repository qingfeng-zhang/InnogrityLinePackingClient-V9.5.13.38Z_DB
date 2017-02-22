using NLog;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Web;
using InnogrityLinePackingClient;
using System.Net.NetworkInformation;
using System.Reflection;

namespace InnogrityLinePackingClient {
    class ZebraPrinter {
        Logger log;
        private string _ZebraFile = String.Empty;
        public string FileNameAndPath = String.Empty;
        public string Imagefolder = string.Empty;
        public ZebraPrinter(string[] args) {
            if (args.Length == 0) {
                log.Info("ZPLToImage IPAddress ZPLFileName");
                return;
            }
            string ipAddress = args[0];
            string zebraFile = args[2];

            if (zebraFile == "ClearDATA") {
                // SendToPrinter(ipAddress, "~JA");         



                SendToPrinter(ipAddress, "\r\n^XA\r\n" +
                             "~JA\r\n" +
                             "^XZ");

                return;

            }


            FileNameAndPath = args[1];
            Imagefolder = args[3];


            log = LogManager.GetLogger("PrinterLog" + ipAddress);

            zebraFile = FileNameAndPath + zebraFile;
            log.Info("Trying to connect to Printer IP Address: " + ipAddress);
            try
            {
                if (IsConnected(ipAddress))
                {
                    log.Info("Connected");
                }
                else
                {
                    log.Info("Not Connected");
                    throw new Exception("Not Connected");
                }
            }
            catch (Exception ex)
            {
                if (ex!=null) log.Error(ex.Message);
                log.Info("Unable to connect to the printer. IP Address: " + ipAddress);
                throw new Exception("Unable to connect to the printer. IP Address: " + ipAddress);
            }
            if (File.Exists(zebraFile) == false) {
                log.Info("ZPL is not found !");
                throw new Exception("ZPL is not found !");
            }
            int imageHeight = 0;
            var imageName = GetImageNameFromPrinter(ipAddress, zebraFile, out imageHeight);

            // Check if image returned...if no, error
            if (!String.IsNullOrEmpty(imageName)) {

                LoadImageFromPrinter(imageName, ipAddress, zebraFile, imageHeight);
                log.Info(imageName + " is created !");
            } else {
                log.Info("Unable to get image from printer !");
                throw new Exception("Unable to get image from printer !");
            }
        }

        private string GetImageNameFromPrinter(string printerIPAddress, string zebraFile, out int imageHeight) {
            //  log.Info("GetImageNameFromPrinter IP {0} ZebraFile {1}",printerIPAddress,zebraFile);
            imageHeight = 0;
            //  log.Info("GetImageNameFromPrinter ReadZPLFile Started IP {0} ZebraFile {1}",printerIPAddress,zebraFile);
            string zplFile = File.ReadAllText(zebraFile);
            //  log.Info("GetImageNameFromPrinter ReadZPLFile Finished IP {0} ZebraFile {1}",printerIPAddress,zebraFile);

            // log.Info("GetImageNameFromPrinter SendGraphics Started IP {0} ZebraFile {1}",printerIPAddress,zebraFile);
            if (!SendGraphics(printerIPAddress, zplFile)) return String.Empty;
            //  log.Info("GetImageNameFromPrinter SendGraphics Finished IP {0} ZebraFile {1}",printerIPAddress,zebraFile);
            // Get Label Length...length of label
            int index = zplFile.IndexOf("^LL");

            if (index >= 0) {
                int nextIndex = zplFile.IndexOf("\r\n", index + 1);

                if (nextIndex >= 0) {
                    string labelLength = zplFile.Substring(index, nextIndex - index).Replace("^LL", "");
                    Int32.TryParse(labelLength, out imageHeight);
                }
            } else {
                // Put default image height if no label length detected
                imageHeight = 200;
            }

            zplFile = HttpUtility.UrlEncode(zplFile);
            string response = null;
            // Setup the post parameters.
            StringBuilder sb = new StringBuilder();
            sb.Append("data=");
            sb.Append(zplFile);
            sb.Append("&dev=R");
            sb.Append("&oname=UNKNOWN");
            sb.Append("&otype=ZPL");
            sb.Append("&prev=Preview Label");
            sb.Append("&pw=");

            //string dataToSend = sb.ToString();
            // log.Info("GetImageNameFromPrinter HttpPost Started IP {0} ZebraFile {1}",printerIPAddress,zebraFile);
            response = HttpPost("http://" + printerIPAddress + "/printer/zpl", sb.ToString());

            // Failed to get image from printer
            if (String.IsNullOrEmpty(response)) {
                //   log.Error("GetImageNameFromPrinter HttpPost Failed IP {0} ZebraFile {1}",printerIPAddress,zebraFile);
                return String.Empty;
            }
            //   log.Info("GetImageNameFromPrinter HttpPost Successful IP {0} ZebraFile {1}",printerIPAddress,zebraFile);

            //   log.Info("GetImageNameFromPrinter ResponseParsing Started IP {0} ZebraFile {1}",printerIPAddress,zebraFile);
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(response);
            var imageNameXPath = "/html[1]/body[1]/div[1]/img[1]/@alt[1]";
            var imageAttributeValue = doc.DocumentNode.SelectSingleNode(imageNameXPath).GetAttributeValue("alt", "");
            // Take off the R: from the front and the .PNG from the back.
            var imageName = imageAttributeValue.Substring(2);
            imageName = imageName.Substring(0, imageName.Length - 4);

            //  log.Info("GetImageNameFromPrinter ResponseParsing Finished IP {0} ZebraFile {1}",printerIPAddress,zebraFile);
            return imageName;
        }
        private bool SendGraphics(string printerIPAddress, string zplFile) {
            //  log.Info("SendGraphics IP {0} ZPLFile {1}",printerIPAddress,zplFile);
            _ZebraFile = zplFile;
            bool sendOK = true;
            int startIndex = 0;

            //SendToPrinter(printerIPAddress, "~JA"); //To delete printer data first -Eugene 

            while (true) {
                int index = zplFile.IndexOf("~DG", startIndex);

                if (index == -1) break;
                int nextIndex = zplFile.IndexOf("~DG", index + 1);
                if (nextIndex == -1) {
                    nextIndex = zplFile.IndexOf("^XA", index + 1);
                }


                if (nextIndex >= 0) {
                    string graphicsData = zplFile.Substring(index, nextIndex - index);

                    _ZebraFile = _ZebraFile.Replace(graphicsData, "");
                    //   log.Info("SendGraphics SendToPrinter Started IP {0} ZPLFile {1}",printerIPAddress,zplFile);
                    if (!SendToPrinter(printerIPAddress, graphicsData)) {
                        //     log.Info("SendGraphics SendToPrinter Failed IP {0} ZPLFile {1}",printerIPAddress,zplFile);
                        sendOK = false;
                        break;
                    }
                    //     log.Info("SendGraphics SendToPrinter Successful IP {0} ZPLFile {1}",printerIPAddress,zplFile);
                }
                startIndex = index + 1;

            }
            return sendOK;
        }
     
        private bool SendToPrinter(string printerIPAddress, string message) {
            //   log.Info("SendToPrinter IP {0} Message {1}",printerIPAddress,message);
            bool sendOK = false;
            try {
                using (TcpClient client = new TcpClient()) {
                    //   log.Info("SendToPrinter BeginConnect IP {0}",printerIPAddress);
                    IAsyncResult ar = client.BeginConnect(printerIPAddress, 9100, null, null);
                    System.Threading.WaitHandle wh = ar.AsyncWaitHandle;
                    if (!ar.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(5), false)) {
                        //      log.Error("SendToPrinter BeginConnect Timeout IP {0}",printerIPAddress);
                        client.Close();
                        throw new TimeoutException();
                    }
                    //     log.Info("SendToPrinter BeginConnect Successful IP {0}",printerIPAddress);
                    client.EndConnect(ar);

                    using (NetworkStream ns = client.GetStream()) {
                        using (StreamWriter sw = new StreamWriter(ns)) {
                            sw.WriteLine(message);
                            sw.Close();
                            sw.Dispose();
                        }
                    }
                    sendOK = true;
                    //  log.Info("SendToPrinter Successful IP {0}",printerIPAddress);
                    client.Close();
                }
                Thread.Sleep(100);
            } catch {
                sendOK = false;
            }
            return sendOK;
        }
        //public  void LoadImageFromPrinter(string imageName, string printerIpAddress, string zebraFile, int imageHeight)
        //{
        //    string url = "http://" + printerIpAddress + "/printer/png?prev=Y&dev=R&oname=" + imageName + "&otype=PNG";
        //    string fileName = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), Path.GetFileNameWithoutExtension(zebraFile)) + ".png";

        //    using (WebClient wc = new WebClient())
        //    {
        //        wc.DownloadFile(url, fileName);
        //    }

        //    if (File.Exists(fileName) && imageHeight > 0)
        //    {
        //        Image image = Image.FromFile(fileName);

        //        using (Bitmap bmp = new Bitmap(image.Width, imageHeight == 0 ? image.Height : imageHeight))
        //        {

        //            using (Graphics gr = Graphics.FromImage(bmp))
        //            {
        //                //gr.DrawImage(
        //                gr.DrawImageUnscaledAndClipped(image, new Rectangle(0, 0, image.Width, imageHeight));
        //            }
        //            image.Dispose();
        //            ImageHandler img = new ImageHandler();
        //            //img.Save(bmp, image.Width * 2, image.Height * 2 ,100, FileNameAndPath);
        //            bmp.Save(fileName);
        //            bmp.Save(Imagefolder + "trainimage.bmp");
        //        }

        //        // Add this one to make sure all graphics are deleted.
        //        //_ZebraFile = _ZebraFile + 
        //        //                "\r\n^XA\r\n" +
        //        //                "^IDR:ID*.*\r\n" +
        //        //                "^XZ";
        //        //string s = File.ReadAllText(zebraFile);
        //        SendToPrinter(printerIpAddress, _ZebraFile);
        //    }
        //}
        public void LoadImageFromPrinter(string imageName, string printerIpAddress, string zebraFile, int imageHeight) {
            //    log.Info("LoadImageFromPrinter IP {0} ZebraFile {1}",printerIpAddress,zebraFile);
            string url = "http://" + printerIpAddress + "/printer/png?prev=Y&dev=R&oname=" + imageName + "&otype=PNG";

            string fileName = Imagefolder + Path.GetFileNameWithoutExtension(zebraFile) + ".png"; // Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), Path.GetFileNameWithoutExtension(zebraFile)) + ".png";

            using (WebClient wc = new WebClient()) {
                wc.DownloadFile(url, fileName);
                //    log.Info("LoadImageFromPrinter DownloadFile Successful IP {0} ZebraFile {1}",printerIpAddress,zebraFile);
            }

            if (File.Exists(fileName)) {
                //   log.Info("LoadImageFromPrinter BMPProcessing Started IP {0} ZebraFile {1}",printerIpAddress,zebraFile);
                Image image = Image.FromFile(fileName);
                try {
                    int imageWidth = 1016;
                    imageHeight = image.Height;

                    //int width = Convert.ToInt32(imageWidth / 0.498f);
                    //int height = Convert.ToInt32(imageHeight / 0.498f);

                    //int width = 1834;
                    //int height = 743;

                    using (Bitmap bmpActual = new Bitmap(imageWidth, imageHeight)) {
                        using (Graphics gr = Graphics.FromImage(bmpActual)) {
                            gr.Clear(Color.Black);
                            gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                            gr.DrawImageUnscaledAndClipped(image, new Rectangle(0, 0, imageWidth, imageHeight));
                        }
                        float ratio = 2040.0f / (imageWidth * 1.0f);

                        imageWidth = Convert.ToInt32(imageWidth * ratio);
                        imageHeight = Convert.ToInt32(imageHeight * ratio);

                        using (Bitmap bmp = new Bitmap(2040, 1088)) {

                            using (Graphics gr = Graphics.FromImage(bmp)) {
                                gr.Clear(Color.Black);
                                gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                                gr.FillRectangle(new SolidBrush(Color.White), new Rectangle(0, 0, 2040, 814));
                                gr.DrawImage(bmpActual, new Rectangle(0, 0, imageWidth, imageHeight));
                                //gr.DrawImageUnscaledAndClipped(image, new Rectangle(0, 0, imageWidth, imageHeight));
                                //gr.DrawRectangle(Pens.Red, new Rectangle(32, 234, 50, 50));
                            }

                            // Need to dispose first so can save with same name
                            image.Dispose();
                            image = null; // to prevent second time dispose at finally
                            using (Bitmap bmp1 = bmp.Clone(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.PixelFormat.Format8bppIndexed)) {
                                bmp1.Save(Imagefolder + Path.GetFileNameWithoutExtension(zebraFile) + ".bmp", System.Drawing.Imaging.ImageFormat.Bmp);
                                //   bmp1.Save(Imagefolder + "master0.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
                                //     log.Info("LoadImageFromPrinter IP {0} BMPSaved {1}",printerIpAddress,zebraFile);
                            }
                        }
                    }
                } finally {
                    if (image != null) image.Dispose();
                }
                // Add this one to make sure all graphics are deleted.
                //_ZebraFile = _ZebraFile + 
                //                "\r\n^XA\r\n" +
                //                "^IDR:ID*.*\r\n" +
                //                "^XZ";
                //string s = File.ReadAllText(zebraFile);
                //   log.Info("LoadImageFromPrinter SendToPrinter Started IP {0} ZebraFile {1}",printerIpAddress,zebraFile);
                SendToPrinter(printerIpAddress, _ZebraFile);
                //   log.Info("LoadImageFromPrinter SendToPrinter Completed IP {0} ZebraFile {1}",printerIpAddress,zebraFile);
            }

        }
        public string HttpPost(string URI, string Parameters) {
            try {
                System.Net.WebRequest req = System.Net.WebRequest.Create(URI);
                req.Proxy = new System.Net.WebProxy();

                //Add these, as we're doing a POST
                req.ContentType = "application/x-www-form-urlencoded";
                req.Method = "POST";

                //We need to count how many bytes we're sending. 
                //Post'ed Faked Forms should be name=value&


                byte[] bytes = System.Text.UnicodeEncoding.ASCII.GetBytes(Parameters);
                req.ContentLength = bytes.Length;

                //  log.Info("HttpPost Request Started URI {0} Parameters {1}",URI,Parameters);
                using (System.IO.Stream os = req.GetRequestStream()) {
                    os.Write(bytes, 0, bytes.Length); //Push it out there
                    os.Close();
                }
                //   log.Info("HttpPost Request Finished URI {0} Parameters {1}",URI,Parameters);

                //  log.Info("HttpPost ResponseReading Started URI {0} Parameters {1}",URI,Parameters);
                using (System.Net.WebResponse resp = req.GetResponse()) {
                    string returnedXML = String.Empty;
                    if (resp == null) return null;
                    using (System.IO.StreamReader sr =
                          new System.IO.StreamReader(resp.GetResponseStream())) {
                        returnedXML = sr.ReadToEnd().Trim();
                    }
                    resp.Close();
                    //  log.Info("HttpPost ResponseReading Finished URI {0} Parameters {1}",URI,Parameters);

                    return returnedXML;
                }
            } catch {
                return "";
            }
        }
        private bool IsConnected(string ipAddress) {
            bool connected = false;
            using (TcpClient client = new TcpClient()) {

                IAsyncResult ar = client.BeginConnect(ipAddress, 9100, null, null);
                System.Threading.WaitHandle wh = ar.AsyncWaitHandle;
                try {
                    if (!ar.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(1), false)) {
                        client.Close();
                        connected = false;
                        throw new TimeoutException();
                    }

                    client.EndConnect(ar);
                    connected = true;
                } catch (Exception ex) {
                    return false;
                } finally {
                    wh.Close();
                }

                client.Close();
            }

            return connected;
        }
    }

    /// <summary>
    /// helper Class contaning method to resize an image and save in JPEG format, converted to bmp format.
    /// </summary>
    public class ImageHandler {
        /// <summary>
        /// Method to resize, convert and save the image.
        /// </summary>
        /// <param name="image">Bitmap image.</param>
        /// <param name="maxWidth">resize width.</param>
        /// <param name="maxHeight">resize height.</param>
        /// <param name="quality">quality setting value.</param>
        /// <param name="filePath">file path.</param>      
        public void Save(Bitmap image, int maxWidth, int maxHeight, int quality, string filePath) {
            // Get the image's original width and height
            int originalWidth = image.Width;
            int originalHeight = image.Height;

            // To preserve the aspect ratio
            float ratioX = (float)maxWidth / (float)originalWidth;
            float ratioY = (float)maxHeight / (float)originalHeight;
            float ratio = Math.Min(ratioX, ratioY);

            // New width and height based on aspect ratio
            int newWidth = (int)(originalWidth * ratio);
            int newHeight = (int)(originalHeight * ratio);

            // Convert other formats (including CMYK) to RGB.
            Bitmap newImage = new Bitmap(newWidth, newHeight, PixelFormat.Format24bppRgb);

            // Draws the image in the specified size with quality mode set to HighQuality
            using (Graphics graphics = Graphics.FromImage(newImage)) {
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.DrawImage(image, 0, 0, newWidth, newHeight);
            }

            // Get an ImageCodecInfo object that represents the JPEG codec.
            ImageCodecInfo imageCodecInfo = this.GetEncoderInfo(ImageFormat.Bmp);

            // Create an Encoder object for the Quality parameter.
            System.Drawing.Imaging.Encoder encoder = System.Drawing.Imaging.Encoder.Quality;

            // Create an EncoderParameters object. 
            EncoderParameters encoderParameters = new EncoderParameters(1);

            // Save the image as a JPEG file with quality level.
            EncoderParameter encoderParameter = new EncoderParameter(encoder, quality);
            encoderParameters.Param[0] = encoderParameter;
            newImage.Save(filePath, imageCodecInfo, encoderParameters);
        }

        /// <summary>
        /// Method to get encoder infor for given image format.
        /// </summary>
        /// <param name="format">Image format</param>
        /// <returns>image codec info.</returns>
        private ImageCodecInfo GetEncoderInfo(ImageFormat format) {
            return ImageCodecInfo.GetImageDecoders().SingleOrDefault(c => c.FormatID == format.Guid);
        }
    }
    public class ZebraTestPrint
    {
        public bool ChecknLoadZPLForTestPrint(int printerNumber)
        {
            //TestPrint Function
            string outputFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            outputFolder = outputFolder + @"\resources\TestPrintFile.txt";
            string zplFile = File.ReadAllText(outputFolder);
            string IpAddr = "";
            Ping PingPrinter2 = new Ping();
            switch (printerNumber)
            {
                case 2:
                    IpAddr = "192.168.3.224";
                    break;
                case 4:
                    IpAddr = "192.168.3.225";
                    break;
                case 7:
                    IpAddr = "192.168.3.226";
                    break;
                default:
                    IpAddr = "192.168.3.224";
                    break;
            }
            PingReply PR2 = PingPrinter2.Send(IpAddr);

            if (PR2.Status == IPStatus.DestinationHostUnreachable)
            {
                // MyEventQ.AddQ("11;PrinterCommunicationBreak;Stationnumber;" + printerNumber.ToString());//Push message to stack
                return false;
            }
            else
            {
                try
                {
                    bool SendOK = SendToPrinter(IpAddr, zplFile);
                    //  if (SendOK)
                    //  {
                    //  MyEventQ.AddQ("82;PrinterTestPrint;PrinterNumber;" + printerNumber);//Push message to stack
                    //  EvtLog.Info("82;PrinterTestPrint;PrinterNumber;" + printerNumber);
                    //  }
                    return SendOK;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }
        private bool SendToPrinter(string printerIPAddress, string message)
        {
            //   log.Info("SendToPrinter IP {0} Message {1}",printerIPAddress,message);
            bool sendOK = false;
            try
            {
                using (TcpClient client = new TcpClient())
                {
                    //   log.Info("SendToPrinter BeginConnect IP {0}",printerIPAddress);
                    IAsyncResult ar = client.BeginConnect(printerIPAddress, 9100, null, null);
                    System.Threading.WaitHandle wh = ar.AsyncWaitHandle;
                    if (!ar.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(5), false))
                    {
                        //      log.Error("SendToPrinter BeginConnect Timeout IP {0}",printerIPAddress);
                        client.Close();
                        throw new TimeoutException();
                    }
                    //     log.Info("SendToPrinter BeginConnect Successful IP {0}",printerIPAddress);
                    client.EndConnect(ar);

                    using (NetworkStream ns = client.GetStream())
                    {
                        using (StreamWriter sw = new StreamWriter(ns))
                        {
                            sw.WriteLine(message);
                            sw.Close();
                            sw.Dispose();
                        }
                    }
                    sendOK = true;
                    //  log.Info("SendToPrinter Successful IP {0}",printerIPAddress);
                    client.Close();
                }
                Thread.Sleep(100);
            }
            catch
            {
                sendOK = false;
            }
            return sendOK;
        }
    }
}
