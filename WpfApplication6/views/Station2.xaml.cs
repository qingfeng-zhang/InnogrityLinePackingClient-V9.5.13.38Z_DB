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
using System.Threading;
using System.IO;
using NLog;
using System.Data;


namespace InnogrityLinePackingClient.views
{
    /// <summary>
    /// Interaction logic for Station2.xaml
    /// </summary>
    public partial class Station2 : Page
    {
        private Base.pageMainPanelDisplay pageMainPanelDisplay;

        public Station2()
        {
            InitializeComponent();
        }


        String DataForTblA;
        String DataForTblB;
        String DataForTblC;


        NetworkThread network;

        Thread workerThread;

        bool bTerminate;
        string filename;
        Logger log = LogManager.GetLogger("Station2FinishingLabelTrace");

        DataRow dr;
        DataTable dt1;
        DataTable dt2;
        DataTable dt3;

        public Station2(Base.pageMainPanelDisplay pageMainPanelDisplay)
        {
            // TODO: Complete member initialization
            this.pageMainPanelDisplay = pageMainPanelDisplay;
            InitializeComponent();
            this.DataContext = pageMainPanelDisplay.DataContext;
            this.network = (NetworkThread)this.DataContext;
            //St2FLdataprovider.Document = network.networkmain.St2FLTrackingdoc;

            //FLdataprovider1.Document = network.networkmain.FLTrackingdocForA;

            //FLdataprovider2.Document = network.networkmain.FLTrackingdocForB;

            //FLdataprovider3.Document = network.networkmain.FLTrackingdocForC;





            //dt1 = new DataTable("TableA");
            //DataColumn dc1 = new DataColumn("No", typeof(int));
            //DataColumn dc2 = new DataColumn("Finished Label For A", typeof(string));           
            //dt1.Columns.Add(dc1);//
            //dt1.Columns.Add(dc2);
            
            ////dataGrid1.ItemsSource = dt1.DefaultView;




            //dt2 = new DataTable("TableB");
            //DataColumn dc3 = new DataColumn("No", typeof(int));
            //DataColumn dc4 = new DataColumn("Finished Label For B", typeof(string));
            //dt2.Columns.Add(dc3);//
            //dt2.Columns.Add(dc4);

            ////dataGrid2.ItemsSource = dt2.DefaultView;




            //dt3 = new DataTable("TableC");
            //DataColumn dc5 = new DataColumn("No", typeof(int));
            //DataColumn dc6 = new DataColumn("Finished Label For C", typeof(string));
            //dt3.Columns.Add(dc5);//
            //dt3.Columns.Add(dc6);

            //dataGrid3.ItemsSource = dt3.DefaultView;






            DataForTblA = network.ST02Rotatary_A_Str;
            DataForTblB = network.ST02Rotatary_B_Str;
            DataForTblC = network.ST02Rotatary_C_Str;



            //if (DataForTblA != "")
            //{
                
            //    dr = dt1.NewRow();
            //    dr[0] = dr;
            //    dr[1] = DataForTblA;

            //    dt1.Rows.Add(dr);
            //    dataGrid1.ItemsSource = dt1.DefaultView;

            //    DataForTblA = "";

            //}



        }


        

        public Station2(MainWindow mainWindow)

        {
            InitializeComponent();
            this.DataContext = mainWindow.DataContext;
            this.network = (NetworkThread)this.DataContext;
            //FLdataprovider.Document = network.networkmain.FLTrackingdoc;
            //DataForTblA = network.ST02Rotatary_A_Str;
            //DataForTblB = network.ST02Rotatary_B_Str;
            //DataForTblC = network.ST02Rotatary_C_Str;



            if (DataForTblA != "")
            {




            }






        }


        private void Button_Click_1(object sender, RoutedEventArgs e)
        {



            try
            {

                //network.St2Scanboxid = BoxID2.Text;

                //network.St2evt_FinishLabelRequest.Set();

            }


            catch (Exception ex)
            {
            }



        }

        private void MoveButton_Click(object sender, RoutedEventArgs e)
        {
            //network.networkmain.Client_sendBoxNumber_MOVESt2(this.MoveBoxID2.Text);
        }




    }
}
