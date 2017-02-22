using InnogrityLinePackingClient;
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
    public partial class SendAlarmMessageDlg : Page
    {
        public SendAlarmMessageDlg(MainWindow mainWindow)
        {
            InitializeComponent();
            this.DataContext = mainWindow.DataContext;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            NetworkThread networkthread = (NetworkThread)DataContext;
            networkthread.networkmain.SendAlarmMessage(AlarmID.Text, AlarmDes.Text, AlarmStatus.Text);
            MessageBox.Show("Send out Alarm", "Note");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            NetworkThread networkthread = (NetworkThread)DataContext;
        //   networkthread.networkmain.Client_SendEventMessage(EventID.Text, EventDes.Text, EventAtt.Text, EventAttValue.Text);

           networkthread.networkmain.Client_SendEventMessageForSealer1("503", "BYNDXLQ.21", "IV301P - 0017", 320, 380,385, 75, 62, (float)1.5," VS");

            MessageBox.Show("Send out Event", "Note");
        }

        private void SendParameter_Click(object sender, RoutedEventArgs e)
        {
            NetworkThread networkthread = (NetworkThread)DataContext;
            //networkthread.networkmain.SendParameterchange(this.UserName.Text, this.ParamName.Text, this.StationID.Text,
            //                                                    this.OldValue.Text, this.NewValue.Text);

            networkthread.networkmain.Client_SendParameterchange1(this.ParamName.Text,   this.OldValue.Text, this.NewValue.Text);

            MessageBox.Show("Send out Parameterchange", "Note");
        }
    }
}
