﻿using System;
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
    /// Interaction logic for Station5.xaml
    /// </summary>
    public partial class Station5 : Page
    {
        NetworkThread network;

        Thread workerThread;

        bool bTerminate;
        string filename;
        Logger log = LogManager.GetLogger("Station5FinishingLabelTrace");
        private Base.pageMainPanelDisplay pageMainPanelDisplay;

        public Station5()
        {
            InitializeComponent();
        }

public Station5(MainWindow mainWindow)
        {
            InitializeComponent();
            this.DataContext = mainWindow.DataContext;
            this.network = (NetworkThread)this.DataContext;
            //St5FLdataprovider.Document = network.networkmain.St5FLTrackingdoc;
            
        }

        public Station5(Base.pageMainPanelDisplay pageMainPanelDisplay)
        {
            // TODO: Complete member initialization
            this.pageMainPanelDisplay = pageMainPanelDisplay;
            InitializeComponent();
            this.DataContext = pageMainPanelDisplay.DataContext;
            this.network = (NetworkThread)this.DataContext;
            //St5FLdataprovider.Document = network.networkmain.St5FLTrackingdoc;
        }

        

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try { 

            //network.St5Scanboxid = BoxID5.Text;

            //network.St5evt_FinishLabelRequest.Set();

        }


            catch (Exception ex)
            {
            }


        }

        

        private void MoveButton_Click(object sender, RoutedEventArgs e)
        {
            //network.networkmain.Client_sendBoxNumber_MOVESt5(this.MoveBoxID5.Text);
        }

        private void RequestFinishingLabel_Click2(object sender, RoutedEventArgs e)
        {

           // network.Station5ForSealer1Scanboxid = BoxID5.Text;//simulate actual situation send from PLC


            network.evnt_FindFinishingLabelForSealer1.Set();

        }




    }
}
