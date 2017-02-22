using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using IGTwpf;
using System.Windows.Forms.ComponentModel;

using System.Xml;
using System.Collections;
using System.Web;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Threading;

namespace InnogrityLinePackingClient
{
    /// <summary>
    /// Interaction logic for operator1window.xaml
    /// </summary>
    public partial class operator1window : Window
    {
        NetworkThread network;
        //Thread workerThread;
        //bool bTerminate;
        //string filename;
        // Logger log = LogManager.GetLogger("Operator1WindowTrack");
        DispatcherTimer SecondTickTimer;
        String orgTitle = null;
        public operator1window(MainWindow mainWindow)
        {
            InitializeComponent();
            this.DataContext = mainWindow.DataContext;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SecondTickTimer = new DispatcherTimer();
            SecondTickTimer.Tick += new EventHandler(SecondTickTimer_tick);
            SecondTickTimer.Interval = new TimeSpan(0, 0, 1);
            SecondTickTimer.Start();
        }

        private void SecondTickTimer_tick(object sender, EventArgs e)
        {
            if (orgTitle == null)
            {
                orgTitle = this.Title;
            }
            this.Title = orgTitle + "       " + String.Format("{0:HH:mm:ss dd/MM/yy}", DateTime.Now);
        }
    }
}
