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
    /// Interaction logic for mypage.xaml
    /// </summary>
    public partial class Station1 : Page
    {
        public Station1()
        {
            InitializeComponent();
        }

        NetworkThread network;
        
        Thread workerThread;

        bool bTerminate;
        string filename;
        Logger log = LogManager.GetLogger("Station1FinishingLabelTrace");
        private Base.pageMainPanelDisplay pageMainPanelDisplay;
     






        public Station1(MainWindow mainWindow)
        {
            InitializeComponent();
            this.DataContext = mainWindow.DataContext;
            this.network = (NetworkThread)this.DataContext;
            //St1FLdataprovider.Document = network.networkmain.St1FLTrackingdoc;
            
        }

        public Station1(Base.pageMainPanelDisplay pageMainPanelDisplay)
        {
            // TODO: Complete member initialization
            this.pageMainPanelDisplay = pageMainPanelDisplay;
            InitializeComponent();
            this.DataContext = pageMainPanelDisplay.DataContext;
            this.network = (NetworkThread)this.DataContext;
            //St1FLdataprovider.Document = network.networkmain.St1FLTrackingdoc;
        }

        

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try { 

            //network.St1Scanboxid = BoxID1.Text;

            //network.St1evt_FinishLabelRequest.Set();

        }


            catch (Exception ex)
            {
            }


        }

        

        private void MoveButton_Click(object sender, RoutedEventArgs e)
        {
            //network.networkmain.Client_sendBoxNumber_MOVESt1(this.MoveBoxID1.Text);
        }




       


    }
}
