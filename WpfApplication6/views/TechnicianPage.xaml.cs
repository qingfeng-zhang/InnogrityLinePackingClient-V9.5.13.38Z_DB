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
using IGTwpf.views;

namespace InnogrityLinePackingClient.views
{
    /// <summary>
    /// Interaction logic for TechnicianPage.xaml
    /// </summary>
    public partial class TechnicianPage : Page
    {
        public TechnicianPage()
        {
            InitializeComponent();
        }

        public string opertor2;
        
        NetworkThread network;
        MainWindow mainwin;
        public TechnicianPage(MainWindow mainWindow)
        {
            InitializeComponent();
            mainwin = mainWindow;
            this.DataContext = mainWindow.DataContext;
            this.network = (NetworkThread)this.DataContext;


            
            
        }

        private void btnLogin(object sender, RoutedEventArgs e)
        {
            popup.IsOpen =true;
            opertor2 = "login1";
           

        }


       

        private void btnloginOp2(object sender, RoutedEventArgs e)
        {
            popup.IsOpen = true;
            opertor2 ="login2";

        }


        public Page ViewSide
        {
            get { return (Page)this.ViewSideFrame.Content; }
            set
            {
                this.ViewSideFrame.Navigate(value);
                this.ViewSideFrame.NavigationService.RemoveBackEntry();
                TranslateTransform scale = new TranslateTransform(-5, 0);
                this.ViewSideFrame.SetValue(RenderTransformProperty, scale);
             //   scale.BeginAnimation(TranslateTransform.XProperty, new DoubleAnimation(0, TimeSpan.FromMilliseconds(200)));
            }
        }


        private void ViewSideFrame_Navigated(object sender, NavigationEventArgs e)
        {

        }

        private void ButtonLogin_Click(object sender, RoutedEventArgs e)
        {
            int micronpass = network.networkmain.Token3;
            string micPass = micronpass.ToString();
            if ((passwordBox.Password == "Innogrity" || passwordBox.Password == micPass) && opertor2=="login2")
            {
                opertor2 = "";
                popup.IsOpen = false;
                passwordBox.Password = "";

                operator1canvas.Visibility = System.Windows.Visibility.Hidden;
                string str = ((NetworkThread)DataContext).ToString();
                mainwin.ViewSide = new Operator2Login(mainwin);   
                
            }

            else if ((passwordBox.Password == "Innogrity" || passwordBox.Password == micPass)&&  opertor2=="login1")
            {
                popup.IsOpen = false;
                passwordBox.Password = "";
                opertor2 = "";

                operator1canvas.Visibility = System.Windows.Visibility.Hidden;
                string str = ((NetworkThread)DataContext).ToString();
                //this.ViewSide = new Operator1Login(this);
                mainwin.ViewSide = new Operator1Login(mainwin);




            }

            
            else
            {
                opertor2 = "";
                popup.IsOpen = false;
                MessageBox.Show("Wrong Password,try againg!");
                passwordBox.Password = "";


            }
        }

        private void ButtonCancel_Click_1(object sender, RoutedEventArgs e)
        {
            popup.IsOpen = false;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
             mainwin.ViewSide = new SendAlarmMessageDlg(this.mainwin);
        }
    }
}
