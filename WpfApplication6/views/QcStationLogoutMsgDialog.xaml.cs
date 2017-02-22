using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace IGTwpf.views
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class QcStationLogoutMsgDialog : Page
    {
        public QcStationLogoutMsgDialog(MainWindow mainWindow)
        {
            this.DataContext = mainWindow.DataContext;
            InitializeComponent();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ((MainNetworkClass)DataContext).SendQCStationLogout(this.StationID.Text, this.UserName.Text);
        }
    }
}
