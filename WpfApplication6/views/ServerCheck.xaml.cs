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
using IGTwpf;
using System.Net.NetworkInformation;

namespace InnogrityLinePackingClient.views
{
    /// <summary>
    /// Interaction logic for ServerCheck.xaml
    /// </summary>
    public partial class ServerCheck : Page
    {
        public ServerCheck()
        {
            InitializeComponent();
        }

        NetworkThread network;
        public ServerCheck(MainWindow mainWindow)
        {
            InitializeComponent();
            this.DataContext = mainWindow.DataContext;
            this.network = (NetworkThread)this.DataContext;
        }

        private void Button_Click(object sender,RoutedEventArgs e) {


           Ping PingPrinter2 = new Ping();
            Ping PingPrinter4 = new Ping();
            Ping PingPrinter7 = new Ping();

            PingReply PR2 = PingPrinter2.Send("192.168.3.224");
            if (PR2.Status == IPStatus.Success) 
            {
                network.Printer2NetworkAddress = "Printer Station2 (192.168.3.224) Ping Success";
                Printer2.Background = Brushes.Lime;
            }
            else if (PR2.Status == IPStatus.DestinationHostUnreachable ) 
            {
                network.Printer2NetworkAddress = "Printer Station2 (192.168.3.224) Ping Fail";
                Printer2.Background = Brushes.Red;
            }
            PingReply PR4 = PingPrinter4.Send("192.168.3.225");
            if (PR4.Status == IPStatus.Success)
            {
                network.Printer4NetworkAddress = "Printer Station4 (192.168.3.225) Ping Success";
                Printer4.Background = Brushes.Lime;
            }
            else if (PR4.Status == IPStatus.DestinationHostUnreachable)
            {
                network.Printer4NetworkAddress = "Printer Station4 (192.168.3.225) Ping Fail";
                Printer4.Background = Brushes.Red;
            }
            PingReply PR7 = PingPrinter7.Send("192.168.3.226");
            if (PR7.Status == IPStatus.Success)
            {
                network.Printer7NetworkAddress = "Printer Station7 (192.168.3.226) Ping Success";
                Printer7.Background = Brushes.Lime;
            }
            else if (PR7.Status == IPStatus.DestinationHostUnreachable)
            {
                network.Printer7NetworkAddress = "Printer Station7 (192.168.3.226) Ping Fail";
                Printer7.Background = Brushes.Red;
            }


        }

        private void Testing_Click(object sender, RoutedEventArgs e)
        {
         
            ZebraTestPrint zbt = new ZebraTestPrint();
            bool Printok = zbt.ChecknLoadZPLForTestPrint(2);
            zbt = null;
        }
    }
}
