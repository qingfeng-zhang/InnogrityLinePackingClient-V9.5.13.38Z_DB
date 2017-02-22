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
using System.Net;
using InnogrityLinePackingClient;
namespace IGTwpf.views.Base
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class Header : UserControl
    {
        public MainWindow Mainwindow { get; set; }
        public Header(MainWindow main)
        {
            InitializeComponent();
            Mainwindow = main;
            this.DataContext = Mainwindow.DataContext;//NetworkThread as data context
        }
        
    }
}
