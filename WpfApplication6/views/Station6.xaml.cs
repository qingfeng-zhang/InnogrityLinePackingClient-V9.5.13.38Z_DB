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
    /// Interaction logic for Station6.xaml
    /// </summary>
    public partial class Station6 : Page
    {
       



         public Station6()
        {
            InitializeComponent();
        }

        NetworkThread network;
        
        Thread workerThread;

        bool bTerminate;
        string filename;
        Logger log = LogManager.GetLogger("Station6FinishingLabelTrace");
        private Base.pageMainPanelDisplay pageMainPanelDisplay;
     






        public Station6(MainWindow mainWindow)
        {
            InitializeComponent();
            this.DataContext = mainWindow.DataContext;
            this.network = (NetworkThread)this.DataContext;
            //St6FLdataprovider.Document = network.networkmain.St6FLTrackingdoc;
            
        }

        public Station6(Base.pageMainPanelDisplay pageMainPanelDisplay)
        {
            // TODO: Complete member initialization
            this.pageMainPanelDisplay = pageMainPanelDisplay;
            InitializeComponent();
            this.DataContext = pageMainPanelDisplay.DataContext;
            this.network = (NetworkThread)this.DataContext;
            //St6FLdataprovider.Document = network.networkmain.St6FLTrackingdoc;
        }

        

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try { 

            //network.St6Scanboxid = BoxID6.Text;

            //network.St6evt_FinishLabelRequest.Set();

        }


            catch (Exception ex)
            {
            }


        }

        

        private void MoveButton_Click(object sender, RoutedEventArgs e)
        {
            //network.networkmain.Client_sendBoxNumber_MOVESt6(this.MoveBoxID6.Text);
        }

       
        private void RequestFinishingLabel_Click(object sender, RoutedEventArgs e)
        {


            try
            {
               // network.Station6ForOP1Scanboxid = BoxID6.Text;//simulate actual situation send from PLC


                network.evnt_FindFinishingLabelForOperator.Set();
                network.evnt_ScannerForOperator.Set();
              network.evnt_TrackingLabelForOperator.Set();

               

            }
            catch (Exception ex)
            {
            }


        }

        private void RequestFinishingLabel_Click2(object sender, RoutedEventArgs e)
        {


            try
            {
               // network.Station6ForOP2Scanboxid = BoxID62.Text;//simulate actual situation send from PLC


                network.evnt_FindFinishingLabelForOperator2.Set();
                network.evnt_ScannerForOperator2.Set();

            }
            catch (Exception ex)
            {
            }


        }

        private void RequestTrackingLabel_Click(object sender, RoutedEventArgs e)
        {

          //  network.Station6ForOP1TrackingLabel = BoxID6track.Text;//simulate actual situation send from PLC


          

            network.evnt_TrackingLabelForOperator.Set();


        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {


            network.rescan();

            network.evnt_ScannerForOperator.Set();
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {

            network.rescan2();

            network.evnt_ScannerForOperator2.Set();
        }


























    }
}
