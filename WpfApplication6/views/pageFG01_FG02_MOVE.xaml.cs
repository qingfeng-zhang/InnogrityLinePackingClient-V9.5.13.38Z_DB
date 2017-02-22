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
namespace InnogrityLinePackingClient.views
{
    /// <summary>
    /// Interaction logic for pageFG01_FG02_MOVE.xaml
    /// </summary>
    public partial class pageFG01_FG02_MOVE : Page
    {
        NetworkThread network;
        public pageFG01_FG02_MOVE(MainWindow mainWindow)
        {
            InitializeComponent();
            this.DataContext = mainWindow.DataContext;
            this.network = (NetworkThread)this.DataContext;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            network.networkmain.Client_sendFG01_FG02_MOVE(this.FGtxt.Text, "FG01_FG02_MOVE");
        }
    }
}
