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
    public partial class SendParameterchange : Page
    {
        public SendParameterchange(MainWindow mainWindow)
        {
            InitializeComponent();
            this.DataContext = mainWindow.DataContext;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ((MainNetworkClass)DataContext).SendParameterchange(this.UserName.Text,this.ParamName.Text,this.StationID.Text,
                                                                this.OldValue.Text,this.NewValue.Text);
        }
    }
}
