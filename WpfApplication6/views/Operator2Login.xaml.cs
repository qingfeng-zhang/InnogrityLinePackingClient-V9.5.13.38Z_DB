﻿using System;
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
    /// Interaction logic for Operator1Login.xaml
    /// </summary>
    public partial class Operator2Login : Page
    {
        public Operator2Login()
        {
            InitializeComponent();
        }




        NetworkThread network;
        private TechnicianPage technicianPage;

        public Operator2Login(MainWindow mainWindow)
        {
            InitializeComponent();
            this.DataContext = mainWindow.DataContext;
            this.network = (NetworkThread)this.DataContext;
        }

        public Operator2Login(TechnicianPage technicianPage)
        {
            // TODO: Complete member initialization
            this.technicianPage = technicianPage;
            InitializeComponent();
            this.DataContext = technicianPage.DataContext;
            this.network = (NetworkThread)this.DataContext;


        }

        private void LoginBTN2_Click(object sender, RoutedEventArgs e)
        {

            //if (!usernameTB2.Text.Equals("") && !PasswordBoxPB2.Password.Equals(""))
            //{
            //    if (usernameTB2.Text.Equals("operator2") && PasswordBoxPB2.Password.Equals("1"))
            //    {

            //        // LoginBTN.Visibility = Visibility.Collapsed;
            //        Mylogin2.Visibility = Visibility.Collapsed;


            //    }
            //    else
            //        MessageBox.Show("Wrong Password");
            //}
            //else
            //    MessageBox.Show("Wrong Info");
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if ((network.Operator02State == 3) || (network.Operator02State == 4))
            {
                network.evnt_ScannerRetryForOperator2.Set();// reset state .. this is a hack.. right fully this should be done on the state engine
                //close ui?
            }

            MessageBox.Show("QC2 Reset Scanner time Successfully");
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {


            if ((network.Operator01State == 2) ||(network.Operator02State == 3) || (network.Operator02State == 4))
            {
                //network UpdateRJLabel
                //network.networkmain.UpdateRJLabel(
               network.RJButton2="QC2 Reject Successfully";


                try{
                         string RJCODE="602";
                          XmlDocument doc = new XmlDocument();
                          doc.Load(@"ConfigEvent.xml");
                          XmlNode node = doc.SelectSingleNode(@"/EVENT/R"+RJCODE);                         
                          string RJName=node.InnerText;

                               network.networkmain.Client_SendEventMessage("56", RJName,"BOX_ID",network.Station6ForOP2Scanboxid);
                               //log6.Info("QC1 Operator Reject Event Send to Middleware " +"603,"+RJName+","+Station6ForOP1Scanboxid);

                                 }
                                 catch (Exception ex){

                                      //log6.Info(ex);
                                 
                                 }

                       network.networkmain.Client_sendFG01_FG02_MOVE(network.Station6ForOP2Scanboxid, "Station 6 QC2 Technician  Reject Finishing Label");
              
                network.evnt_RejForOperator2.Set();
                //close ui?
            }
            MessageBox.Show("QC2 Reject Successfully");
        }

       private void Button_Click_QC(object sender,RoutedEventArgs e) {

             network.ResetCountButton2="Successfully";
              network.st2POcount=0;
             network.ResetCountButton2="";
             MessageBox.Show("Successfully");

        }

        private void Button_Click_RJ(object sender,RoutedEventArgs e) {
          network.ResetCountButton2="Successfully";
          network.st2Rejectcount=0;
          network.ResetCountButton2="";
          MessageBox.Show("Successfully");

        }




    }








}

