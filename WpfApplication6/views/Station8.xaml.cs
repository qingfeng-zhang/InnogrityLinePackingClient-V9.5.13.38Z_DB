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
    /// Interaction logic for Station8.xaml
    /// </summary>
    public partial class Station8 : Page
    {
 public Station8()
        {
            InitializeComponent();
        }

        NetworkThread network;
        
        Thread workerThread;

        bool bTerminate;
        string filename;
        Logger log = LogManager.GetLogger("Station8FinishingLabelTrace");
        private Base.pageMainPanelDisplay pageMainPanelDisplay;
     






        public Station8(MainWindow mainWindow)
        {
            InitializeComponent();
            this.DataContext = mainWindow.DataContext;
            this.network = (NetworkThread)this.DataContext;
            //St8FLdataprovider.Document = network.networkmain.St8FLTrackingdoc;
            
        }

        public Station8(Base.pageMainPanelDisplay pageMainPanelDisplay)
        {
            // TODO: Complete member initialization
            this.pageMainPanelDisplay = pageMainPanelDisplay;
            InitializeComponent();
            this.DataContext = pageMainPanelDisplay.DataContext;
            this.network = (NetworkThread)this.DataContext;
            //St8FLdataprovider.Document = network.networkmain.St8FLTrackingdoc;
        }

        

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try { 

            //network.St8Scanboxid = BoxID8.Text;

            //network.St8evt_FinishLabelRequest.Set();

        }


            catch (Exception ex)
            {
            }


        }

        

        private void MoveButton_Click(object sender, RoutedEventArgs e)
        {
            //network.networkmain.Client_sendBoxNumber_MOVESt8(this.MoveBoxID8.Text);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void BoxID6track_Copy2_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

    }
}
