using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using IGTwpf;
using System.IO;
namespace InnogrityLinePackingClient.views
{
    /// <summary>
    /// Interaction logic for pageprinterfilelist.xaml
    /// </summary>
    public partial class pageprinterfilelist : Page
    {
        NetworkThread network;
        XmlDocument Fltrackingdoc;
        public pageprinterfilelist(MainWindow mainWindow)
        {
            InitializeComponent();
            this.DataContext = mainWindow.DataContext;
            this.network = (NetworkThread)this.DataContext;
            //Add in Finishing label information for tracking
            Fltrackingdoc = network.networkmain.FinishingLabelsInfo;
            XmlElement root1 = Fltrackingdoc.DocumentElement;
        }

        void FindPrinterfiles(string boxid)
        {

            try
            {
                network.networkmain.GetPrinterFilesTnR(boxid);
                network.networkmain.GetPrinterFilesMBB(boxid);
                network.networkmain.GetPrinterFilesBox(boxid);
                FLdataprovider.Document = network.networkmain.tnRdoc;
                FLdataprovider1.Document = network.networkmain.MBBdoc;
                FLdataprovider11.Document = network.networkmain.Boxdoc;
                FLdataprovider111.Document = null;
            }
            catch (Exception ex)
            {
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            FindPrinterfiles(BoxID.Text);
        }



        private void btnStation4print(object sender, RoutedEventArgs e)
        {
            Printfiles(network.networkmain.MBBdoc);
            ////find all files in btnStation2
            //XmlNodeList nodes = network.networkmain.MBBdoc.SelectNodes("//FILE/FILE_NAME");
            ////get filefolder
            //XmlDocument doc = new XmlDocument();
            //doc.Load(@"Config.xml");
            //XmlNode filefolder = doc.SelectSingleNode(@"/CONFIG/PRINTFILEDIR");
            //XmlNode ArchiveFolder = doc.SelectSingleNode(@"/CONFIG/PRINTARCHIVE");
            //string printer = network.networkmain.MBBdoc.SelectNodes("//BOXID/PRINTER")[0].InnerText;
            //XmlNode printerdetails = doc.DocumentElement.SelectSingleNode(@"./PRINTER[./PRINTERNAME='" + printer + "']");
            //foreach (XmlNode node in nodes)
            //{
            //    string filename = node.InnerText;
            //    try
            //    {
            //        bool exists = File.Exists(filefolder.InnerText + filename);
            //        using (StreamReader sr = new StreamReader(filefolder.InnerText + filename))
            //        {
            //            String ZPLString = sr.ReadToEnd();
            //            System.Net.Sockets.TcpClient Zebraclient = new System.Net.Sockets.TcpClient();
            //            Zebraclient.Connect(printerdetails.SelectSingleNode(@"IPADDRESS").InnerText, 9100);
            //            System.IO.StreamWriter writer = new System.IO.StreamWriter(Zebraclient.GetStream());
            //            writer.Write(ZPLString);
            //            writer.Flush();
            //            writer.Close();
            //            Zebraclient.Close();
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        MessageBox.Show(ex.ToString(), "error");

            //    }
            //    finally
            //    {
            //        try
            //        {
            //            //achive
            //            if (!Directory.Exists(ArchiveFolder.InnerText))
            //                Directory.CreateDirectory(ArchiveFolder.InnerText);
            //            File.Copy(filefolder.InnerText + filename, ArchiveFolder.InnerText + filename, true);
            //            File.Delete(filefolder.InnerText + filename);
            //        }
            //        catch (Exception ex)
            //        {
            //            MessageBox.Show(ex.ToString(), "error");
            //        }
            //    }
            //}

        }


        private void Printfiles(XmlDocument printdoc)
        {
            //find all files in btnStation2
            XmlNodeList nodes = printdoc.SelectNodes("//FILE/FILE_NAME");
            //get filefolder
            XmlDocument doc = new XmlDocument();
            doc.Load(@"Config.xml");
            XmlNode filefolder = doc.SelectSingleNode(@"/CONFIG/PRINTFILEDIR");
            XmlNode ArchiveFolder = doc.SelectSingleNode(@"/CONFIG/PRINTARCHIVE");
            string printer = printdoc.SelectNodes("//BOXID/PRINTER")[0].InnerText;
            XmlNode printerdetails = doc.DocumentElement.SelectSingleNode(@"./PRINTER[./PRINTERNAME='" + printer + "']");
            foreach (XmlNode node in nodes)
            {
                string filename = node.InnerText;
                try
                {
                    bool exists = File.Exists(filefolder.InnerText + filename);
                    if (!exists) throw new Exception(filefolder.InnerText + filename + "Not Found!");
                    using (StreamReader sr = new StreamReader(filefolder.InnerText + filename))
                    {
                        String ZPLString = sr.ReadToEnd();
                        System.Net.Sockets.TcpClient Zebraclient = new System.Net.Sockets.TcpClient();
                        Zebraclient.Connect(printerdetails.SelectSingleNode(@"IPADDRESS").InnerText, 9100);
                        if (Zebraclient.Connected == false) throw new Exception("Printer address " + printerdetails.SelectSingleNode(@"IPADDRESS").InnerText + "address not valid");
                        System.IO.StreamWriter writer = new System.IO.StreamWriter(Zebraclient.GetStream());
                        if (ZPLString.IndexOf("^XA") > 0)
                        {
                            writer.Write(ZPLString);
                            writer.Flush();
                            writer.Close();
                            Zebraclient.Close();
                        }
                        else
                        {
                            writer.Close();
                            Zebraclient.Close();
                            throw new Exception("File have no ^XA");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "error");
                }
                finally
                {
                    //achive
                    //archive folder 

                    //need to explicitly create the folder
                    //remember to do changes to xmllogger.. did  not use config xml information
                    try
                    {
                        if (!Directory.Exists(ArchiveFolder.InnerText))
                            Directory.CreateDirectory(ArchiveFolder.InnerText);
                        File.Copy(filefolder.InnerText + filename, ArchiveFolder.InnerText + filename, true);
                        File.Delete(filefolder.InnerText + filename);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString(), "error");
                    }
                }
            }
        }
        private void btnStation2print(object sender, RoutedEventArgs e)
        {
            Printfiles(network.networkmain.tnRdoc);
            //find all files in btnStation2
            //XmlNodeList nodes = network.networkmain.tnRdoc.SelectNodes("//FILE/FILE_NAME");
            ////get filefolder
            //XmlDocument doc = new XmlDocument();
            //doc.Load(@"Config.xml");
            //XmlNode filefolder = doc.SelectSingleNode(@"/CONFIG/PRINTFILEDIR");
            //XmlNode ArchiveFolder = doc.SelectSingleNode(@"/CONFIG/PRINTARCHIVE");
            //string printer = network.networkmain.tnRdoc.SelectNodes("//BOXID/PRINTER")[0].InnerText;
            //XmlNode printerdetails = doc.DocumentElement.SelectSingleNode(@"./PRINTER[./PRINTERNAME='" + printer + "']");
            //foreach (XmlNode node in nodes)
            //{
            //    string filename = node.InnerText;
            //    try
            //    {
            //        bool exists = File.Exists(filefolder.InnerText + filename);
            //        if (!exists) throw new Exception(filefolder.InnerText + filename + "Not Found!");
            //        using (StreamReader sr = new StreamReader(filefolder.InnerText + filename))
            //        {
            //            String ZPLString = sr.ReadToEnd();
            //            System.Net.Sockets.TcpClient Zebraclient = new System.Net.Sockets.TcpClient();
            //            Zebraclient.Connect(printerdetails.SelectSingleNode(@"IPADDRESS").InnerText, 9100);
            //            if (Zebraclient.Connected == false) throw new Exception("Printer address " + printerdetails.SelectSingleNode(@"IPADDRESS").InnerText + "address not valid");
            //            System.IO.StreamWriter writer = new System.IO.StreamWriter(Zebraclient.GetStream());
            //            if (ZPLString.IndexOf("^XA") > 0)
            //            {
            //                writer.Write(ZPLString);
            //                writer.Flush();
            //                writer.Close();
            //                Zebraclient.Close();
            //            }
            //            else
            //            {
            //                writer.Close();
            //                Zebraclient.Close();
            //                throw new Exception("File have no ^XA");
            //            }
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        MessageBox.Show(ex.ToString(), "error");
            //    }
            //    finally
            //    {
            //        //achive
            //        //archive folder 

            //        //need to explicitly create the folder
            //        //remember to do changes to xmllogger.. did  not use config xml information
            //        try
            //        {
            //            if (!Directory.Exists(ArchiveFolder.InnerText))
            //                Directory.CreateDirectory(ArchiveFolder.InnerText);
            //            File.Copy(filefolder.InnerText + filename, ArchiveFolder.InnerText + filename, true);
            //            File.Delete(filefolder.InnerText + filename);
            //        }
            //        catch (Exception ex)
            //        {
            //            MessageBox.Show(ex.ToString(),"error");
            //        }
            //    }
            //}
        }

        private void btnStation7print(object sender, RoutedEventArgs e)
        {
            Printfiles(network.networkmain.Boxdoc);

            ////find all files in btnStation2
            //XmlNodeList nodes = network.networkmain.Boxdoc.SelectNodes("//FILE/FILE_NAME");
            ////get filefolder
            //XmlDocument doc = new XmlDocument();
            //doc.Load(@"Config.xml");
            //XmlNode filefolder = doc.SelectSingleNode(@"/CONFIG/PRINTFILEDIR");
            //XmlNode ArchiveFolder = doc.SelectSingleNode(@"/CONFIG/PRINTARCHIVE");
            //string printer = network.networkmain.Boxdoc.SelectNodes("//BOXID/PRINTER")[0].InnerText;// get printer name
            //XmlNode printerdetails = doc.DocumentElement.SelectSingleNode(@"./PRINTER[./PRINTERNAME='" + printer + "']");//get printer IP Address
            //foreach (XmlNode node in nodes)
            //{
            //    string filename = node.InnerText;
            //    try
            //    {
            //        bool exists = File.Exists(filefolder.InnerText + filename);
            //        using (StreamReader sr = new StreamReader(filefolder.InnerText + filename))
            //        {
            //            String ZPLString = sr.ReadToEnd();
            //            System.Net.Sockets.TcpClient Zebraclient = new System.Net.Sockets.TcpClient();
            //            Zebraclient.Connect(printerdetails.SelectSingleNode(@"IPADDRESS").InnerText, 9100);
            //            System.IO.StreamWriter writer = new System.IO.StreamWriter(Zebraclient.GetStream());
            //            writer.Write(ZPLString);
            //            writer.Flush();
            //            writer.Close();
            //            Zebraclient.Close();
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        MessageBox.Show(ex.ToString(), "error");
            //    }
            //    finally
            //    {
            //        try
            //        {
            //            //achive
            //            if (!Directory.Exists(ArchiveFolder.InnerText))
            //                Directory.CreateDirectory(ArchiveFolder.InnerText);
            //            File.Copy(filefolder.InnerText + filename, ArchiveFolder.InnerText + filename, true);
            //            File.Delete(filefolder.InnerText + filename);
            //        }
            //        catch (Exception ex)
            //        {
            //            MessageBox.Show(ex.ToString(), "error");
            //        }
            //    }
            //}
        }
    }  
}
