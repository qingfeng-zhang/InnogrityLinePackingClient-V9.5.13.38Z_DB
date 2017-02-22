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
    /// Interaction logic for Station7.xaml
    /// </summary>
    public partial class Station7 : Page
    {
        
 public Station7()
        {
            InitializeComponent();
        }

        NetworkThread network;
        
        Thread workerThread;

        bool bTerminate;
        string filename;
        Logger log = LogManager.GetLogger("Station7FinishingLabelTrace");
        private Base.pageMainPanelDisplay pageMainPanelDisplay;
     






        public Station7(MainWindow mainWindow)
        {
            InitializeComponent();
            this.DataContext = mainWindow.DataContext;
            this.network = (NetworkThread)this.DataContext;
            //St7FLdataprovider.Document = network.networkmain.St7FLTrackingdoc;
            
        }

        public Station7(Base.pageMainPanelDisplay pageMainPanelDisplay)
        {
            // TODO: Complete member initialization
            this.pageMainPanelDisplay = pageMainPanelDisplay;
            InitializeComponent();
            this.DataContext = pageMainPanelDisplay.DataContext;
            this.network = (NetworkThread)this.DataContext;
            //St7FLdataprovider.Document = network.networkmain.St7FLTrackingdoc;
        }

        

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try { 

            //network.St7Scanboxid = BoxID7.Text;

            //network.St7evt_FinishLabelRequest.Set();

        }


            catch (Exception ex)
            {
            }


        }

        

        private void MoveButton_Click(object sender, RoutedEventArgs e)
        {
            //network.networkmain.Client_sendBoxNumber_MOVESt7(this.MoveBoxID7.Text);
        }

        private void RequestFinishingLabel_Click(object sender, RoutedEventArgs e)
        {
            //while (!network.networkmain.UpdateTrackingLabel(BoxID6.Text,
            //                                                  BoxID62.Text)) ;//assume PLC data moved before server reply

        }

        private void RequestTrackingLabel_Click(object sender, RoutedEventArgs e)
        {
            //int a = 263;
         //  network.CheckStringUpdateFor7(263,BoxID6track.Text);
          //  network.Station7ForTransferScanboxidFromPLC1 = BoxID6track.Text;
        }














    }
}
