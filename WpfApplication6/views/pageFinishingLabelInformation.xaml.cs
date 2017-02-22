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
using System.Threading;
using System.IO;
using NLog;
namespace InnogrityLinePackingClient.views
{

    /// <summary>
    /// Interaction logic for pageFinishingLabelInformation.xaml
    /// </summary>
    public partial class pageFinishingLabelInformation : Page
    {
        NetworkThread network;
        public string[] barcodeList;
        public string[] barcodeList1;
        Thread workerThread;
        bool bTerminate;
        string filename;
        public string ButtonName;
        Logger log = LogManager.GetLogger("FinishingLabelTrace");
        public pageFinishingLabelInformation(MainWindow mainWindow)
        {
            InitializeComponent();
            this.DataContext = mainWindow.DataContext;
            this.network = (NetworkThread)this.DataContext;
            FLdataprovider.Document = network.networkmain.FLTrackingdoc;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //send label request
            try
            {
                popup.IsOpen = true;

                ButtonName = "Request";




            }
            catch (Exception ex)
            {
            }
        }

        private void RESET(object sender, RoutedEventArgs e)
        {
            ButtonName = "RemoveAll";
            popup.IsOpen = true;

          
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            //open file
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".flf";
            dlg.Filter = "Finishing Label Files (*.flf)|*.flf";
            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();


            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 
                filename = dlg.FileName;
                string[] filenames = filename.Split('\\');
                //Get all files name
                this.labelfilename.Content = "File : " + filenames[filenames.Length - 1];
            }
        }

        private void contrunchecked_Checked_1(object sender, RoutedEventArgs e)
        {
            //if checked disable buttons and textbox

            BtnReqFL.IsEnabled = false;
            BoxID.IsEnabled = false;
            btnConRunStart.IsEnabled = true;

        }

        private void contrunchecked_Unchecked_1(object sender, RoutedEventArgs e)
        {
            btnConRunStart.IsEnabled = false;
            BtnReqFL.IsEnabled = true;
            BoxID.IsEnabled = true;
        }
        public void ContinuousSendBarcodeData()
        {
            while (!bTerminate)
            {
                try
                {
                    if (network.connected)
                    {
                        Thread.Sleep(100);
                        foreach (string barcode in barcodeList)
                        {
                            //send barcode
                            network.Scanboxid = barcode.Trim();//simulate actual situation send from PLC
                            if (network.Scanboxid == "")
                                continue;
                            log.Info("Finishing label send" + barcode.Trim());
                            network.evt_FinishLabelRequest.Set();

                            network.evt_FinishLabelRequestComplete.WaitOne(1000);//wait for label request to complete    
                            network.evt_FinishLabelRequestComplete.Reset();
                            //read barcode
                            if (!network.networkmain.ServerReplyEvt.WaitOne(20000))
                            {
                                //reply timeout
                                log.Info("Finishing Label " + barcode.Trim() + " no reply");
                            }
                            else
                            {
                                log.Info("Finishing Label " + barcode.Trim() + " replied");
                            }
                            network.networkmain.ServerReplyEvt.Reset();
                            //FG barcode
                            network.networkmain.Client_sendFG01_FG02_MOVE(barcode.Trim(), "FG01_FG02_MOVE");
                            log.Info("Sending FG01_FG02_MOVE for " + barcode);
                        }
                    }
                    else
                    {
                        log.Info("No Server Connection");
                        Thread.Sleep(1000);
                    }
                }
                catch (Exception ex)
                {
                    log.Error(ex.ToString);
                }
            }
        }
        private void btnConRunStart_Checked_1(object sender, RoutedEventArgs e)
        {
            //reading filename
            using (StreamReader sr = new StreamReader(this.filename, Encoding.Default))
            {
                string text = sr.ReadToEnd();
                barcodeList = text.Split(',');
            }

            //start thread
            // Create the thread object. This does not start the thread.
            workerThread = new Thread(new ThreadStart(ContinuousSendBarcodeData));
            bTerminate = false;
            // Start the worker thread.
            workerThread.Start();
            //disable checkbox
            contrunchecked.IsEnabled = false;

        }

        private void btnConRunStart_Unchecked_1(object sender, RoutedEventArgs e)
        {
            //end thread
            bTerminate = true;
            workerThread.Join(5000);
            workerThread.Abort();
            //enable checkbox
            contrunchecked.IsEnabled = true;
        }

        private void RemoveLabel(object sender, RoutedEventArgs e)
        {
            popup.IsOpen = true;
            ButtonName = "Remove";
        
        }

        private void ButtonLogin_Click(object sender, RoutedEventArgs e)
        {
            int micronpass =network.networkmain.Token3;
            string micPass = micronpass.ToString();
           // if ((passwordBox.Password == "Innogrity" || passwordBox.Password == micPass) && ButtonName=="Request")
            if ((passwordBox.Password == "" || passwordBox.Password == micPass) && ButtonName == "Request")
            {
            try
            {
                    popup.IsOpen = false;
                    passwordBox.Password = "";
                    ButtonName = "";
                    network.Scanboxid = BoxID.Text;//
                    network.evt_FinishLabelRequest.Set();
                }
                catch (Exception ex)
                {
                }

            }

      //     else  if ((passwordBox.Password == "Innogrity" || passwordBox.Password == micPass) && ButtonName == "Remove")
            else if ((passwordBox.Password == "" || passwordBox.Password == micPass) && ButtonName == "Remove")
            {
                popup.IsOpen = false;
                passwordBox.Password = "";
                ButtonName = "";

                //NEED TO SEND ACK TO MIDDLEWARE R996

                try
                {
                    string RJCODE = "996";
                    XmlDocument doc = new XmlDocument();
                    doc.Load(@"ConfigEvent.xml");
                    XmlNode node = doc.SelectSingleNode(@"/EVENT/R" + RJCODE);
                    string RJName = node.InnerText;

                    network.networkmain.Client_SendEventMessage("66", RJName, "BOX_ID", this.FGtxt.Text);


                    network.MyEventQ.AddQ("3;FinishingLabelDataCleared;Lotnumber;" + this.FGtxt.Text);
                    network.EvtLog.Info("3;FinishingLabelDataCleared;Lotnumber;" + this.FGtxt.Text);

                }
                catch (Exception ex)
                {

                }
                try
                {
                    network.DesiccantTimingMap.Remove(this.FGtxt.Text);
                    network.EvtLog.Info("delete HIC timer;Lotnumber;" + this.FGtxt.Text);
                }
                catch (Exception ex)
                {
                    network.EvtLog.Error("delete HIC timer;Lotnumber;" + this.FGtxt.Text+","+ ex.Message);
                }

                network.networkmain.Client_sendFG01_FG02_MOVE(this.FGtxt.Text, "FG01_FG02_MOVE,TECHNICIAN  MANUAL REJECT");
            }
       //     else if ((passwordBox.Password == "Innogrity" || passwordBox.Password == micPass) && ButtonName == "RemoveAll")
          else if ((passwordBox.Password == "" || passwordBox.Password == micPass) && ButtonName == "RemoveAll")
            {

                popup.IsOpen = false;
                passwordBox.Password = "";
                ButtonName = "";

                network.RoyalFlush(); //The Royal Flush Function

            }

            else
            {
                popup.IsOpen = false;
                MessageBox.Show("Wrong Password,try againg!");
                passwordBox.Password = "";
                ButtonName = "";



            }




        }

        private void ButtonCancel_Click_1(object sender, RoutedEventArgs e)
        {
            popup.IsOpen = false;
        }


    }
}
