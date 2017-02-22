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
    /// Interaction logic for Station3.xaml
    /// </summary>
    public partial class Station3 : Page
    {

        NetworkThread network;

        Thread workerThread;

        bool bTerminate;
        string filename;
        Logger log = LogManager.GetLogger("Station3FinishingLabelTrace");
        private Base.pageMainPanelDisplay pageMainPanelDisplay;



        public Station3(MainWindow mainWindow)
        {
            InitializeComponent();
            InitializeComponent();
            this.DataContext = mainWindow.DataContext;
            this.network = (NetworkThread)this.DataContext;
            //St3FLdataprovider.Document = network.networkmain.St3FLTrackingdoc;
        }




         public Station3(Base.pageMainPanelDisplay pageMainPanelDisplay)
        {
            // TODO: Complete member initialization
            this.pageMainPanelDisplay = pageMainPanelDisplay;
            InitializeComponent();
            this.DataContext = pageMainPanelDisplay.DataContext;
            this.network = (NetworkThread)this.DataContext;
            //St3FLdataprovider.Document = network.networkmain.St3FLTrackingdoc;
        }




        private void Button_Click_1(object sender, RoutedEventArgs e)
        {


            try
            {

                //network.St3Scanboxid = BoxID3.Text;

                //network.St3evt_FinishLabelRequest.Set();

            }


            catch (Exception ex)
            {
            }






        }

        private void MoveButton_Click(object sender, RoutedEventArgs e)
        {
            //network.networkmain.Client_sendBoxNumber_MOVESt3(this.MoveBoxID3.Text);


        }
    }
}
