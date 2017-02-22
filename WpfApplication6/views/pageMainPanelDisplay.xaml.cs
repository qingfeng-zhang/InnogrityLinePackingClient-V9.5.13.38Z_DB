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

namespace InnogrityLinePackingClient.views.Base
{
    /// <summary>
    /// Interaction logic for pageMainPanelDisplay.xaml
    /// </summary>
    public partial class pageMainPanelDisplay : Page
    {
        NetworkThread network;
        int ctr = 1;
        public pageMainPanelDisplay(MainWindow mainWindow)
        {
             
            InitializeComponent();
            this.DataContext = mainWindow.DataContext;
            this.network = (NetworkThread)this.DataContext;


          
           




            TextBlock tb = new TextBlock();
            tb.MouseLeftButtonDown += new MouseButtonEventHandler(TextBlock_MouseLeftButtonDown);
            tb.Cursor = Cursors.Hand;


            TextBlock tb1 = new TextBlock();
            tb1.MouseLeftButtonDown += new MouseButtonEventHandler(TextBlock_MouseLeftButtonDown_1);
            tb1.Cursor = Cursors.Hand;



            TextBlock tb2 = new TextBlock();
            tb2.MouseLeftButtonDown += new MouseButtonEventHandler(TextBlock_MouseLeftButtonDown_2);
            tb2.Cursor = Cursors.Hand;

            TextBlock tb3 = new TextBlock();
            tb3.MouseLeftButtonDown += new MouseButtonEventHandler(TextBlock_MouseLeftButtonDown_3);
            tb3.Cursor = Cursors.Hand;


            TextBlock tb4 = new TextBlock();
            tb4.MouseLeftButtonDown += new MouseButtonEventHandler(TextBlock_MouseLeftButtonDown_4);
            tb4.Cursor = Cursors.Hand;


            TextBlock tb5 = new TextBlock();
            tb5.MouseLeftButtonDown += new MouseButtonEventHandler(TextBlock_MouseLeftButtonDown_5);
            tb5.Cursor = Cursors.Hand;



            TextBlock tb6 = new TextBlock();
            tb6.MouseLeftButtonDown += new MouseButtonEventHandler(TextBlock_MouseLeftButtonDown_6);
            tb6.Cursor = Cursors.Hand;



            TextBlock tb7 = new TextBlock();
            tb7.MouseLeftButtonDown += new MouseButtonEventHandler(TextBlock_MouseLeftButtonDown_7);
            tb7.Cursor = Cursors.Hand;





            TextBlock tb8 = new TextBlock();
            tb8.MouseLeftButtonDown += new MouseButtonEventHandler(TextBlock_MouseLeftButtonDown_8);
            tb8.Cursor = Cursors.Hand;









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
              
            }
        }

        private void DoubleAnimation_Completed(object sender, EventArgs e)
        {
            ShowRobot1Image();
            darobot.BeginAnimation(Image.OpacityProperty, darobot);
        }
        private void ShowRobot1Image()
        {
            string filename = @"/InnogrityLinePackingClient;component/resources/robo" + ctr + ".png";
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(filename, UriKind.RelativeOrAbsolute);
            image.EndInit();
            this.st5_robot.Source = image;
            ctr++;
            if (ctr > 4)
            {
                ctr = 1;
            }
        }









       

        public void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            mycanvas.Visibility = System.Windows.Visibility.Hidden;
            ////mycanvas_Copy.Visibility = System.Windows.Visibility.Visible;
            //img1.Visibility = System.Windows.Visibility.Visible;




            //this.NavigationService.Navigate(new Uri("/views/Station1.xaml", UriKind.Relative),this);

            string str = ((NetworkThread)DataContext).ToString();
            this.ViewSide = new Station1(this);
            


        

        }

        private void TextBlock_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {

            mycanvas.Visibility = System.Windows.Visibility.Hidden;

            string str = ((NetworkThread)DataContext).ToString();
            this.ViewSide = new Station2(this);
            //this.NavigationService.Navigate(new Uri("/views/Station2.xaml", UriKind.Relative));
        

            

          
        }

        private void TextBlock_MouseLeftButtonDown_2(object sender, MouseButtonEventArgs e)
        {

           // this.NavigationService.Navigate(new Uri("/views/Station3.xaml", UriKind.Relative));

            mycanvas.Visibility = System.Windows.Visibility.Hidden;

            string str = ((NetworkThread)DataContext).ToString();
            this.ViewSide = new Station3(this);



        }

        private void TextBlock_MouseLeftButtonDown_3(object sender, MouseButtonEventArgs e)
        {


          //  this.NavigationService.Navigate(new Uri("/views/Station4.xaml", UriKind.Relative));

            mycanvas.Visibility = System.Windows.Visibility.Hidden;

            string str = ((NetworkThread)DataContext).ToString();
            this.ViewSide = new Station4(this);





        }

        private void TextBlock_MouseLeftButtonDown_4(object sender, MouseButtonEventArgs e)
        {
           // this.NavigationService.Navigate(new Uri("/views/Station5.xaml", UriKind.Relative));


            mycanvas.Visibility = System.Windows.Visibility.Hidden;

            string str = ((NetworkThread)DataContext).ToString();
            this.ViewSide = new Station5(this);


        }

        private void TextBlock_MouseLeftButtonDown_5(object sender, MouseButtonEventArgs e)
        {


           // this.NavigationService.Navigate(new Uri("/views/Station6.xaml", UriKind.Relative));


            mycanvas.Visibility = System.Windows.Visibility.Hidden;

            string str = ((NetworkThread)DataContext).ToString();
            this.ViewSide = new Station6(this);
        }

        private void TextBlock_MouseLeftButtonDown_6(object sender, MouseButtonEventArgs e)
        {
          //  this.NavigationService.Navigate(new Uri("/views/Station6.xaml", UriKind.Relative));



            mycanvas.Visibility = System.Windows.Visibility.Hidden;

            string str = ((NetworkThread)DataContext).ToString();
            this.ViewSide = new Station6(this);
        }

        private void TextBlock_MouseLeftButtonDown_7(object sender, MouseButtonEventArgs e)
        {
           // this.NavigationService.Navigate(new Uri("/views/Station7.xaml", UriKind.Relative));


            mycanvas.Visibility = System.Windows.Visibility.Hidden;

            string str = ((NetworkThread)DataContext).ToString();
            this.ViewSide = new Station7(this);

        }

        private void TextBlock_MouseLeftButtonDown_8(object sender, MouseButtonEventArgs e)
        {
            //this.NavigationService.Navigate(new Uri("/views/Station8.xaml", UriKind.Relative));



            mycanvas.Visibility = System.Windows.Visibility.Hidden;

            string str = ((NetworkThread)DataContext).ToString();
            this.ViewSide = new Station8(this);
        }

        private void ViewSideFrame_Navigated(object sender, NavigationEventArgs e)
        {

        }






    }
}
