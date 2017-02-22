using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Threading;
using System.Xml;
using System.Net;
using NLog;
using System.Xml.Linq;

namespace IGTwpf.views
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    /// 
    public partial class SendMsgDialog : Page
    {
        public string[] barcodeList;
        public string[] barcodeList1;
        Thread workerThread;


        Logger log = LogManager.GetLogger("XMLTrace");
        //later call Log.Info("Message");
       
        MainNetworkClass network;
        public SendMsgDialog(MainWindow mainWindow)
        {
            
            InitializeComponent();
            this.DataContext = mainWindow.DataContext;
            this.network = (MainNetworkClass)this.DataContext;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //((MainNetworkClass)DataContext).SendScanBox(Box_Number.Text, Box_Number.Text);
            network.Client_SendScanBox(Box_Number.Text, Box_Number.Text);
            try
            {
                network.GetDataFromHost();
            }
            catch (Exception ex)
            { }
           
        }

        public void ContinuousSendBarcodeData()
        {
            string[] filePaths;
            try
            {
                log.Info("enter continous send");
                while (network.IsNonStop)
                {

                    if (!network.CheckHost())
                    {
                        //reconnects
                        try
                        {
                            network.ConnectToHost(network.ipaddresstohost);
                        }
                        catch (Exception ex)
                        {
                            log.Error("Error Connecting To Host @ " + network.ipaddresstohost.ToString());
                            throw ex;
                        }
                    }
         
                    int rate = 100/(barcodeList.Length);
                    for (int i = 0; i < barcodeList.Length; i++)
                    {
                        network.NumBarcode = (i+1)*rate;
                        if (i == (barcodeList.Length - 1))
                            network.NumBarcode = 100;
                        try
                        {
                            string s = barcodeList[i];
                            string s2 = barcodeList[i];                            
                            if (network.Client_SendScanBox(s.Trim(), s2.Trim()) == false)//wait for message is here..
                            {
                                log.Error("SendBox Data timeout Error 01");
                            }
                            log.Info("Sent bc1 (" + s + ") bc2 (" + s2 + ") complete");
                        }
                        catch (Exception ex)
                        {
                            log.Error("SendBox Data Exception Error 02");

                        }
                        //wait for server to reply
                        string str = network.GetDataFromHost();
                        XmlNodeList list = network.BoxInformationDocument.SelectNodes("MESSAGE/BODY/LABEL_PLACEMENT/LABEL/FILE_NAME");
                        filePaths = Directory.GetFiles(@"c:\printerfiles\", "*.ZPL");
                        bool fileNotExist = true;
                        foreach (XmlNode node in list)
                        {
                            log.Info("filename recieve from Middleware : "
                                        + node.InnerText.ToUpper()
                                        );                            
                        }
                        try
                        {
                            if (filePaths.Length == 0) log.Info("No Files Found on recieving Directory");
                            foreach (string file in filePaths)
                            {
                                //check file exist
                                string[] filename = file.Split('\\');
                                filename = filename[2].Split('.');
                                foreach (XmlNode node in list)
                                {

                                    if (filename[0].ToUpper() == node.InnerText.ToUpper())
                                    {
                                        fileNotExist = false;//file is found need not check further, log data
                                        log.Info("file : "
                                                + filename[0]
                                                + " matched to xml file recieved");
                                        break;
                                    }
                                }
                                if (fileNotExist)//file not there exits...
                                {
                                    //error log
                                    log.Error("File not exist file : " + filename[2]);
                                    //delete all files
                                    log.Info("deleting all files");
                                    try
                                    {
                                        foreach (string filedelete in filePaths)
                                        {

                                            File.Delete(file);
                                        }
                                    }
                                    catch (Exception ex) { }
                                    if (network.NoPrintCheck)
                                        continue;//still continue to check the rest of the file..
                                    else
                                        break;//exit since file does not exits
                                }

                                try
                                {
                                    //file exist... print file
                                    //file printing??

                                    //becareful not to use filename.. no path..
                                    //after print

                                    log.Info("file : " + filename[0] + " printed");
                                    //delete files

                                    File.Delete(file);
                                    log.Info("file : " + filename[0] + " deleted");
                                }
                                catch (Exception ex)
                                {
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            log.Info("Error: " + ex.ToString());
                        }
                        if (!network.IsNonStop)
                            throw new Exception();
                    }
                }
            }
            catch (Exception ex)
            {
                log.Info("Exit continuous Send data");
            }
            finally
            {
                MessageBox.Show("End of Sending Continuously");
                log.Info("Exit sending continuously");
            }

        }
        private void SendMultipleBtn_Click(object sender, RoutedEventArgs e)
        {

            //read code from files
            // Open the file to read from. 
            log.Info("do read from files for finishing labels");
            barcodeList = File.ReadAllLines(@"C:\barcodelist.txt");
            barcodeList1 = File.ReadAllLines(@"C:\barcodelist.txt");

            if (workerThread != null)
            {
                if (workerThread.IsAlive)
                {
                    MessageBox.Show("Data Still Sending");
                    return;
                }
            }
            // Create the thread object. This does not start the thread.
            workerThread = new Thread(new ThreadStart(ContinuousSendBarcodeData));

            // Start the worker thread.
            workerThread.Start();

            // Loop until worker thread activates. 
            while (!workerThread.IsAlive) ;


            //generate threads
            //disable Btn
        }

        private void CheckBox_Click_1(object sender, RoutedEventArgs e)
        {
            if (((CheckBox)sender).IsChecked == true)
            {
                SendSingleBtn.IsEnabled = false;
                SendMultipleBtn.IsEnabled = true;
            }
            else
            {
                SendMultipleBtn.IsEnabled = false;
                SendSingleBtn.IsEnabled = true;
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {



            XmlDocument doc = new XmlDocument();
            doc.Load(@"C:\boxlist.xml");


            // Create an XmlNamespaceManager to resolve the default namespace.
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("bk", "urn:boxlist-schema");

            // Select the first book written by an author whose last name is Atwood.
            XmlNode book;
            XmlElement root1 = doc.DocumentElement;
           // "descendant::bk:BOXLABEL[bk:MESSAGE/bk:BODY/bk:BOX_NUMBER='1212222.45']"
            book = root1.SelectSingleNode("descendant::bk:MESSAGE[bk:BODY/bk:BOX_NUMBER='1212222.45']", nsmgr);
            //book = root1.SelectSingleNode("descendant::bk:book[bk:author/bk:last-name='Kingsolver']", nsmgr);
            book = root1.SelectSingleNode("descendant::bk:MESSAGE[bk:BODY/bk:BOX_NUMBER='1234567.11']", nsmgr);
            //   XmlNode node =  doc.SelectSingleNode( "//@ISBN['0-201-63361-2']");
            //network.CheckHost();

            //test code for XML search and collections.

            string myxmlstring = @"<MESSAGE><HEADER><EQUIPMENT_ID>LINEPACK_AUTOMATION1</EQUIPMENT_ID><TIMESTAMP>2014/11/19 10:51:36.307</TIMESTAMP></HEADER>" +
               "<BODY><MESSAGE_TYPE>BOX_DATA</MESSAGE_TYPE><BOX_NUMBER>1234567.11</BOX_NUMBER><FRESH_LOT>TRUE</FRESH_LOT><SAP><INTENDED_CUST>IB</INTENDED_CUST><AQL>TRUE</AQL></SAP>" +
               "<MAM><MOISTURE_CURE_REQD /><CUST_SPECIFIC_FLOW /><PACKING_MEDIA /><BOX_TYPE>TR</BOX_TYPE><TRAY_DESIGN>14X18S7X14</TRAY_DESIGN><CURRENT_QTY>883</CURRENT_QTY><MOISTURE_SENSITIVITY>2</MOISTURE_SENSITIVITY><SHIPPING_MST_LEVEL>3</SHIPPING_MST_LEVEL><OUT_OF_PURGE_TIME /><OUT_OF_PURGE_LIMIT /><APO_MODULE_BOUND>YES</APO_MODULE_BOUND><MFR_COMPATIBLE>MICRON</MFR_COMPATIBLE><IMFT_PARTNER /><DESIGN_ID>L85A</DESIGN_ID><PACKAGE_WIDTH>18.000</PACKAGE_WIDTH><PACKAGE_LENGTH>14.000</PACKAGE_LENGTH><PRODUCT_ID>225225</PRODUCT_ID><NMX_MKTG_PART_NUMBER /><PACKAGE_TYPE>TBGA</PACKAGE_TYPE><LEAD_COUNT>152/221</LEAD_COUNT><NUMBER_OF_TRAY /><NUMBER_OF_TUBE /><REEL_WIDTH>44</REEL_WIDTH><TRAY_THICKNESS /></MAM>" +
               "<SEALER_RECIPE>0100</SEALER_RECIPE>" +
               "<LABEL_PLACEMENT>" +
               "<TAPE_REEL>" +
                   "<FILE>" +
                       "<FILE_NAME>TAPE_REEL_LABELFILENAME88.ZPL</FILE_NAME>" +
                           "<PRINTER_NUMBER>ZEBRA_001</PRINTER_NUMBER>" +
                   "</FILE>" +
               "</TAPE_REEL>" +
               "<MBB>" +
                   "<FILE>" +
                       "<FILE_NAME>MBB_MST_LABELFILENAME11.ZPL</FILE_NAME>" +
                       "<SURFACE>A</SURFACE>" +
                       "<COORDINATE_X>1000</COORDINATE_X>" +
                       "<COORDINATE_Y>500</COORDINATE_Y>" +
                       "<PRINTER_NUMBER>ZEBRA_002</PRINTER_NUMBER>" +
                   "</FILE>" +
                   "<FILE>" +
                       "<FILE_NAME>MBB_MST_LABELFILENAME12.ZPL</FILE_NAME>" +
                       "<SURFACE>A</SURFACE>" +
                       "<COORDINATE_X>1202</COORDINATE_X>" +
                       "<COORDINATE_Y>601</COORDINATE_Y>" +
                       "<PRINTER_NUMBER>ZEBRA_002</PRINTER_NUMBER>" +
                   "</FILE>" +
               "</MBB>" +
               "<BOX>" +
                   "<FILE>" +
                       "<FILE_NAME>BOX_LABELFILENAME11.ZPL</FILE_NAME>" +
                       "<SURFACE>A</SURFACE>" +
                       "<COORDINATE_X>1310</COORDINATE_X>" +
                       "<COORDINATE_Y>801</COORDINATE_Y>" +
                       "<PRINTER_NUMBER>ZEBRA_003</PRINTER_NUMBER>" +
                   "</FILE>" +
                   "<FILE>" +
                       "<FILE_NAME>BOX_LABELFILENAME12.ZPL</FILE_NAME>" +
                       "<SURFACE>B</SURFACE>" +
                       "<COORDINATE_X>1510</COORDINATE_X>" +
                       "<COORDINATE_Y>882</COORDINATE_Y>" +
                       "<PRINTER_NUMBER>ZEBRA_003</PRINTER_NUMBER>" +
                   "</FILE>" +
               "</BOX>" +

               "</LABEL_PLACEMENT></BODY></MESSAGE>";

            XmlDocument tmpdoc = new XmlDocument();
            tmpdoc.LoadXml(myxmlstring);
            XmlNodeList elemlist =  tmpdoc.GetElementsByTagName("MESSAGE");
            XmlNode node =  elemlist[0];
            XmlDocument FinishingLabelsInfo = new XmlDocument();
            XmlDocument tagdocument = new XmlDocument();
           
            
            FinishingLabelsInfo.LoadXml(@"<BOXLIST></BOXLIST>");
            XmlNode copiedNode = FinishingLabelsInfo.ImportNode(node, true);
            FinishingLabelsInfo.DocumentElement.AppendChild(copiedNode);                     
            //FinishingLabelsInfo.InnerXml = FinishingLabelsInfo.InnerXml.Replace(@"xmlns=""""" , "");            
            root1 = FinishingLabelsInfo.DocumentElement;
            myxmlstring = @"<MESSAGE><HEADER><EQUIPMENT_ID>LINEPACK_AUTOMATION1</EQUIPMENT_ID><TIMESTAMP>2014/11/19 10:51:36.307</TIMESTAMP></HEADER>"+
                "<BODY><MESSAGE_TYPE>BOX_DATA</MESSAGE_TYPE><BOX_NUMBER>1212222.45</BOX_NUMBER><FRESH_LOT>TRUE</FRESH_LOT><SAP><INTENDED_CUST>IB</INTENDED_CUST><AQL>TRUE</AQL></SAP>"+
                "<MAM><MOISTURE_CURE_REQD /><CUST_SPECIFIC_FLOW /><PACKING_MEDIA /><BOX_TYPE>TR</BOX_TYPE><TRAY_DESIGN>14X18S7X14</TRAY_DESIGN><CURRENT_QTY>883</CURRENT_QTY><MOISTURE_SENSITIVITY>2</MOISTURE_SENSITIVITY><SHIPPING_MST_LEVEL>3</SHIPPING_MST_LEVEL><OUT_OF_PURGE_TIME /><OUT_OF_PURGE_LIMIT /><APO_MODULE_BOUND>YES</APO_MODULE_BOUND><MFR_COMPATIBLE>MICRON</MFR_COMPATIBLE><IMFT_PARTNER /><DESIGN_ID>L85A</DESIGN_ID><PACKAGE_WIDTH>18.000</PACKAGE_WIDTH><PACKAGE_LENGTH>14.000</PACKAGE_LENGTH><PRODUCT_ID>225225</PRODUCT_ID><NMX_MKTG_PART_NUMBER /><PACKAGE_TYPE>TBGA</PACKAGE_TYPE><LEAD_COUNT>152/221</LEAD_COUNT><NUMBER_OF_TRAY /><NUMBER_OF_TUBE /><REEL_WIDTH>44</REEL_WIDTH><TRAY_THICKNESS /></MAM>"+
                "<SEALER_RECIPE>0100</SEALER_RECIPE>"+
                "<LABEL_PLACEMENT>"+                
                "<TAPE_REEL>"+
	                "<FILE>"+
                        "<FILE_NAME>TAPE_REEL_LABELFILENAME.ZPL</FILE_NAME>"+
                            "<PRINTER_NUMBER>ZEBRA_001</PRINTER_NUMBER>"+
		            "</FILE>"+
                "</TAPE_REEL>"+
                "<MBB>"+
                    "<FILE>"+
                        "<FILE_NAME>MBB_MST_LABELFILENAME01.ZPL</FILE_NAME>"+
                        "<SURFACE>A</SURFACE>"+
                        "<COORDINATE_X>1000</COORDINATE_X>"+
                        "<COORDINATE_Y>500</COORDINATE_Y>"+
                        "<PRINTER_NUMBER>ZEBRA_002</PRINTER_NUMBER>"+
                    "</FILE>"+
                    "<FILE>" +
                        "<FILE_NAME>MBB_MST_LABELFILENAME02.ZPL</FILE_NAME>" +
                        "<SURFACE>A</SURFACE>" +
                        "<COORDINATE_X>1200</COORDINATE_X>" +
                        "<COORDINATE_Y>600</COORDINATE_Y>" +
                        "<PRINTER_NUMBER>ZEBRA_002</PRINTER_NUMBER>" +
                    "</FILE>" +
                "</MBB>"+
                "<BOX>" +
                    "<FILE>" +
                        "<FILE_NAME>BOX_LABELFILENAME01.ZPL</FILE_NAME>" +
                        "<SURFACE>A</SURFACE>" +
                        "<COORDINATE_X>1300</COORDINATE_X>" +
                        "<COORDINATE_Y>800</COORDINATE_Y>" +
                        "<PRINTER_NUMBER>ZEBRA_003</PRINTER_NUMBER>" +
                    "</FILE>" +
                    "<FILE>" +
                        "<FILE_NAME>BOX_LABELFILENAME02.ZPL</FILE_NAME>" +
                        "<SURFACE>B</SURFACE>" +
                        "<COORDINATE_X>1500</COORDINATE_X>" +
                        "<COORDINATE_Y>888</COORDINATE_Y>" +
                        "<PRINTER_NUMBER>ZEBRA_003</PRINTER_NUMBER>" +
                    "</FILE>" +
                "</BOX>" +

                "</LABEL_PLACEMENT></BODY></MESSAGE>";
            tmpdoc.LoadXml(myxmlstring);
            elemlist = tmpdoc.GetElementsByTagName("MESSAGE");
            node = elemlist[0];
            copiedNode = FinishingLabelsInfo.ImportNode(node, true);
            FinishingLabelsInfo.DocumentElement.AppendChild(copiedNode); 
            //FinishingLabelsInfo.InnerXml = FinishingLabelsInfo.InnerXml.Replace(@"xmlns=""""", "");
            root1 = FinishingLabelsInfo.DocumentElement;
            book = root1.SelectSingleNode("descendant::MESSAGE[BODY/BOX_NUMBER='1212222.45']");
            XmlNodeList TapeAndReelLabel = book.SelectNodes("BODY/LABEL_PLACEMENT/TAPE_REEL/FILE");
            XmlNodeList MBBLabel = book.SelectNodes("BODY/LABEL_PLACEMENT/MBB/FILE");
            XmlNodeList BoxLabel = book.SelectNodes("BODY/LABEL_PLACEMENT/BOX/FILE");

            book = root1.SelectSingleNode("descendant::MESSAGE[BODY/BOX_NUMBER='1234567.11']");
            TapeAndReelLabel = book.SelectNodes("BODY/LABEL_PLACEMENT/TAPE_REEL/FILE");
            MBBLabel = book.SelectNodes("BODY/LABEL_PLACEMENT/MBB/FILE");
            foreach (XmlNode node1 in MBBLabel)
            {
                XmlElement ele = node1["FILE_NAME"];
                ele = node1["SURFACE"];
                ele = node1["COORDINATE_X"];
                ele = node1["COORDINATE_Y"];
                ele = node1["PRINTER_NUMBER"];
            }
            BoxLabel = book.SelectNodes("BODY/LABEL_PLACEMENT/BOX/FILE");
            
        }


    }
}
