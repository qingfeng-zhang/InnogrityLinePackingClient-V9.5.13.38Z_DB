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
namespace InnogrityLinePackingClient.views
{
    /// <summary>
    /// Interaction logic for OmronVision.xaml
    /// </summary>
    public partial class OmronVision : Page
    {
        public OmronVision(MainWindow mainWindow)
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                if (e.Text == "." || e.Text == "-")
                {

                }
                else
                {
                    Convert.ToInt32(e.Text);
                }

            }
            catch
            {
                e.Handled = true;
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Save();
        }
    }
}
