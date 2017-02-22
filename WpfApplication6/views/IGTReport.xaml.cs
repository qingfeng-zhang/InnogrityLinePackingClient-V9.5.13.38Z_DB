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
using System.Globalization;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Data;

namespace InnogrityLinePackingClient.views
{
    /// <summary>
    /// Interaction logic for OmronVision.xaml
    /// </summary
    public partial class IGTReport : Page , INotifyPropertyChanged
    {
        ReportQuery Rq = new ReportQuery();
        public IGTReport()
        {
                //Data = new ObservableCollection<MyData>();
                InitializeComponent();
                this.DataContext = this;
            txtNGReason.DataContext = this;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        
        private void Notify(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        private ObservableCollection<MyData> _data = null;
        public ObservableCollection<MyData> Data
        {
            get
            {
                return _data;
            }
            set
            {
                _data = value;
                Notify("Data");
            }
        }
    

        private DateTime _StartShift;
        public DateTime StartShift
        {
            get { return _StartShift; }
            set
            {
                _StartShift = value;
                Notify("StartShift");
            }
        }
        private string _GraphType;
        public string GraphType
        {
            get { return _GraphType; }
            set
            {
                _GraphType = value;
            }
        }
        private DateTime _EndShift;
        public DateTime EndShift
        {
            get { return _EndShift; }
            set
            {
                _EndShift = value;
                Notify("EndShift");
            }
        }
        private DateTime _StartDate;
        public DateTime StartDate
        {
            get { return _StartDate; }
            set
            {
                _StartDate = value;
                Notify("StartDate");
            }
        }

        private DateTime _DetailStDate;
        public DateTime DetailStDate
        {
            get { return _DetailStDate; }
            set
            {
                _DetailStDate = value;
                Notify("DetailStDate");
            }
        }
        private DateTime _EndDate;
        public DateTime EndDate
        {
            get { return _EndDate; }
            set
            {
                _EndDate = value;
                Notify("EndDate");
            }
        }
        private string _NGReason; //Added to Display NGReason
        public string NGReason
        {
            get { return _NGReason; }
            set
            {
                _NGReason = value;
                Notify("NGReason");
            }
        }
        private void button_Click(object sender, RoutedEventArgs e)
        {
            Data = new ObservableCollection<MyData>();
            
            bool IsOverNightShit;
            int hour = (EndShift.Hour - StartShift.Hour);
            if (hour < 0)
            {
                IsOverNightShit = true;
            }
            else
            {
                IsOverNightShit = false;
            }
            if (StartShift == EndShift)
            {
                MessageBox.Show("Please input difference time for start and End shift ");
                return;
            }
            if (StartDate == null||EndDate == null )
            {
                MessageBox.Show("Please input Start and End date ");
                return;
            }
            if ( StartShift == null || EndShift == null)
            {
                MessageBox.Show("Please input Start and End Shift ");
                return;
            }
            BarChart1.Legends = null;
            Data.Clear();
            button.IsEnabled = false;
            #region OLDCODE

            #region OldCode
            //List<string> List = Rq.RetQuerhyItem(StartDate, EndDate, StartShift.ToString("H:mm:ss tt"), EndShift.ToString("H:mm:ss tt"), IsOverNightShit);
            //int s = List.Count;
            //for (int i = 0; i < List.Count; i++)
            //{
            //    string[] tmpC = List[i].Split(',');
            //    for (int j = 1; j < 9; j++)
            //    {
            //        if (tmpC[j].Trim(' ') != "0")
            //        {
            //            WorkTypes tmpW;
            //            switch (j)
            //            {
            //                case 1:
            //                    tmpW = WorkTypes.TotalLot;
            //                    break;
            //                case 2:
            //                    tmpW = WorkTypes.JamClearLot;
            //                    break;
            //                case 3:
            //                    tmpW = WorkTypes.OKLot;
            //                    break;
            //                case 4:
            //                    tmpW = WorkTypes.AQLLot;
            //                    break;
            //                case 5:
            //                    tmpW = WorkTypes.RJ4Lot;
            //                    break;
            //                case 6:
            //                    tmpW = WorkTypes.RJ6Lot;
            //                    break;
            //                case 7:
            //                    tmpW = WorkTypes.RJ8Lot;
            //                    break;
            //                case 8:
            //                    tmpW = WorkTypes.TotalRJ;
            //                    break;
            //                default:
            //                    tmpW = WorkTypes.TotalRJ;
            //                    break;
            //            }
            //            DateTime dt = DateTime.Parse(tmpC[0]);
            //            Data.Add(new MyData() { Year = dt.ToString("ddMMMyyyy"), Value = int.Parse(tmpC[j]), WorkType = tmpW });
            //        }

            //   }

            //}
            #endregion

            #endregion
            #region NewTest
            DataTable dt = new DataTable();
            if (GraphType == "GOOD BOX+REJECT+AQL")
            {
                dt = Rq.RetQuerhyItem(StartDate, EndDate, StartShift.ToString("H:mm:ss tt"), EndShift.ToString("H:mm:ss tt"), IsOverNightShit);
            }
            else if (GraphType == "REJECT DETAIL")
            {
                dt = Rq.RetQuerhyItem2(StartDate, EndDate, StartShift.ToString("H:mm:ss tt"), EndShift.ToString("H:mm:ss tt"), IsOverNightShit);
            }
            else
            {
                dt = Rq.RetQuerhyItem3(StartDate, EndDate, StartShift.ToString("H:mm:ss tt"), EndShift.ToString("H:mm:ss tt"), IsOverNightShit);
            }
            foreach (DataRow row in dt.Rows)
            {
                for (int i = 1; i < dt.Columns.Count; i++)
                {
                    if (row[i]!=null && Convert.ToString(row[i]) !="0")
                    {
                        string Truncated = Convert.ToDouble(row[i]).ToString("F2");
                        Data.Add(new MyData() { Year = Convert.ToDateTime(row[0]).ToString("ddMMM"), Value = Convert.ToDouble(Truncated), WorkType = (WorkTypes)i-1 });
                    }
                }
            }
            #endregion

            BarChart1.ItemsSource = Data;
            if (BarChart1.Legends.Count > 0 )
            {
                LinearGradientBrush linGrBrush = new LinearGradientBrush();
                linGrBrush.StartPoint = new Point(0, 0.5);
                linGrBrush.EndPoint = new Point(1, 0.5);
                linGrBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0x5C, 0x8E, 0xFF), 0));
                linGrBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0xC2, 0xC2, 0xFC), 1));
                BarChart1.Legends[0].Color = linGrBrush;
                if (BarChart1.Legends.Count > 1)
                {
                    LinearGradientBrush linGrBrush2 = new LinearGradientBrush();
                    linGrBrush2.StartPoint = new Point(0, 0.5);
                    linGrBrush2.EndPoint = new Point(1, 0.5);
                    linGrBrush2.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0x63, 0xB7, 0x00), 0));
                    linGrBrush2.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0xBD, 0xEB, 0x94), 1));
                    BarChart1.Legends[1].Color = linGrBrush2;

                    LinearGradientBrush linGrBrush3 = new LinearGradientBrush();
                    linGrBrush3.StartPoint = new Point(0, 0.5);
                    linGrBrush3.EndPoint = new Point(1, 0.5);
                    linGrBrush3.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0xA9, 0xB7, 0x00), 0));
                    linGrBrush3.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0xEB, 0xEB, 0x94), 1));
                    BarChart1.Legends[2].Color = linGrBrush3;

                    LinearGradientBrush linGrBrush4 = new LinearGradientBrush();
                    linGrBrush4.StartPoint = new Point(0, 0.5);
                    linGrBrush4.EndPoint = new Point(1, 0.5);
                    linGrBrush4.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0xB7, 0x07, 0xB7), 0));
                    linGrBrush4.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0xD7, 0x94, 0xEB), 1));
                    BarChart1.Legends[3].Color = linGrBrush4;
                }
                BarChart1.ItemsSource = Data;
            }
          
    

            button.IsEnabled = true;

        }

        private void btnDetailRepGen_Click(object sender, RoutedEventArgs e)
        {
            btnDetailRepGen.IsEnabled = false;
            if (DetailStDate == null)
            {
                MessageBox.Show("Please input Start date ");
                btnDetailRepGen.IsEnabled = true;
                return;
            }
            if (StartShift == null || EndShift == null)
            {
                MessageBox.Show("Please input Start and End Shift ");
                btnDetailRepGen.IsEnabled = true;
                return;
            }
            if (StartShift == EndShift)
            {
                MessageBox.Show("Please input difference time for start and End shift ");
                btnDetailRepGen.IsEnabled = true;
                return;
            }

            bool IsOverNightShit;
            int hour = (EndShift.Hour - StartShift.Hour);
            if (hour < 0)
            {
                IsOverNightShit = true;
            }
            else
            {
                IsOverNightShit = false;
            }
            DataSet ds = Rq.CurrentYieldReq(DetailStDate, StartShift.ToString("H:mm:ss tt"), EndShift.ToString("H:mm:ss tt"), IsOverNightShit);
            dataGrid.ItemsSource = ds.Tables["ST4RJ"].DefaultView;
            dataGrid2.ItemsSource = ds.Tables["ST6RJ"].DefaultView;
            dataGrid3.ItemsSource = ds.Tables["ST8RJ"].DefaultView;
            dataGrid4.ItemsSource = ds.Tables["STJAM"].DefaultView;
            dataGrid5.ItemsSource = ds.Tables["STAQL"].DefaultView;
            dataGrid.Columns[1].Visibility = Visibility.Hidden;
            dataGrid2.Columns[1].Visibility = Visibility.Hidden;
            dataGrid3.Columns[1].Visibility = Visibility.Hidden;
            dataGrid4.Columns[1].Visibility = Visibility.Hidden;
            dataGrid5.Columns[1].Visibility = Visibility.Hidden;
            ds = null;
            DataTable dt = new DataTable();
            

            dt = Rq.RetQuerhyItem1(DetailStDate, DetailStDate.AddHours(24), StartShift.ToString("H:mm:ss tt"), EndShift.ToString("H:mm:ss tt"), IsOverNightShit);
            if (dt.Rows.Count > 0)
            {
                lbTotInputVal.Content = dt.Rows[0][8].ToString();
                lbTotRjVal.Content = dt.Rows[0][4].ToString();
                lbJamClearVal.Content = dt.Rows[0][1].ToString();
                lbAQLLotVal.Content = dt.Rows[0][3].ToString();
                lbOKLotVal.Content = dt.Rows[0][2].ToString();
                double Yield =( (double.Parse(dt.Rows[0][2].ToString()) + double.Parse(dt.Rows[0][3].ToString())) / (double.Parse(dt.Rows[0][8].ToString()) - double.Parse(dt.Rows[0][1].ToString())))*100;
                double Yield2 = ((double.Parse(dt.Rows[0][2].ToString()) + double.Parse(dt.Rows[0][3].ToString())) / (double.Parse(dt.Rows[0][8].ToString()) - double.Parse(dt.Rows[0][1].ToString()) - double.Parse(dt.Rows[0][9].ToString()))) * 100;
                lbYieldVal.Content = Yield.ToString("##.00") + " %";
                lbYieldVal2.Content = Yield2.ToString("##.00") + " %";
               // MessageBox.Show(dt.Rows[0][9].ToString());
            }

            dt = null;

            DataTable dt1 = new DataTable();


            dt1 = Rq.RetRJItem(DetailStDate, StartShift.ToString("H:mm:ss tt"), EndShift.ToString("H:mm:ss tt"), IsOverNightShit);
            if (dt1.Rows.Count > 0)
            {
                dt1.Columns.Add("RJMessage", typeof(string)).SetOrdinal(3);
                foreach (DataRow row in dt1.Rows)
                {
                    int RJcode =Convert.ToInt16(row["RJCode"].ToString());
                    string RJMesg = RJMessage(RJcode);
                    row["RJMessage"] = RJMesg;
                }
                dataGrid1.ItemsSource = dt1.DefaultView;
                dataGrid1.Columns[0].Visibility = Visibility.Hidden;
            }

            dt1 = null;
            btnDetailRepGen.IsEnabled = true;

        }
        private string RJMessage(int RJCode)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(@"ConfigEvent.xml");
                XmlNode node = doc.SelectSingleNode(@"/EVENT/R" + RJCode);
                string RJName = node.InnerText;
                return RJName;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
          

        }

        private void dataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            DataRowView drw = (DataRowView)dataGrid.SelectedItem;
            try
            {
                if (drw["Reason"].ToString() != null)
                {
                    NGReason = drw["Reason"].ToString() + "-" + RJMessage(Convert.ToInt16(drw["Reason"].ToString()));
                }
            }
            catch (Exception)
            {
            }
          
            drw = null;
        }

        private void dataGrid2_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            DataRowView drw = (DataRowView)dataGrid2.SelectedItem;
            try
            {
                if (drw["Reason"].ToString() != null)
                {
                    NGReason = drw["Reason"].ToString() + "-" + RJMessage(Convert.ToInt16(drw["Reason"].ToString()));
                }
            }
            catch (Exception)
            {
            }
            drw = null;
        }

        private void dataGrid3_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            DataRowView drw = (DataRowView)dataGrid3.SelectedItem;
            try
            {
                if (drw["Reason"].ToString() != null)
                {
                    NGReason = drw["Reason"].ToString() + "-" + RJMessage(Convert.ToInt16(drw["Reason"].ToString()));
                }
            
            }
            catch (Exception)
            {
            }
            drw = null;
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            var CurrentFocus = sender as RadioButton;
           GraphType = CurrentFocus.Content.ToString();
        }

        private void chkLegendHide_Click(object sender, RoutedEventArgs e)
        {
            if (chkLegendHide.IsChecked == true)
            {
                BarChart1.LegendsVisibility = Visibility.Visible;
            }
            else
            {
                BarChart1.LegendsVisibility = Visibility.Hidden;
            }
        }
    }

    public enum WorkTypes
    {
        JamClearLot,
        OKLot,
        TotalRJ,
        AQLLot,
        RJ4Lot,
        RJ6Lot,
        RJ8Lot,
        Yield
    }
    public class MyData
    {
        public MyData()
        {
        }

        public string Year { get; set; }
        
        public double Value { get; set; }

        public WorkTypes WorkType { get; set; }
    }

    #region Converter
    public class Bool2Visibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility)
                return (Visibility)value == Visibility.Visible ? true : false;
            else
                throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
                return (bool)value ? Visibility.Visible : Visibility.Collapsed;
            else
                throw new NotImplementedException();
        }
    }
    #endregion 
}
