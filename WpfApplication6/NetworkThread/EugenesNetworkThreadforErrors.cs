using NLog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Xml;
using IGTwpf;
using System.Windows;
using System.IO;
namespace InnogrityLinePackingClient
{
    partial class NetworkThread
    {
        #region NewCode
        public void RoyalFlush()
        {
            int TotalLabel = networkmain.FLTrackingdoc.GetElementsByTagName("ID").Count;
            if (TotalLabel == 0) //if nothing in All Labels array
            {
                MessageBox.Show("Nothing to remove.", "The Royal Flush");
            }
            else
            {
                MessageBoxResult sureanot = System.Windows.MessageBox.Show(
                    "Are you sure you want to remove all Labels?", "The Royal Flush", System.Windows.MessageBoxButton.YesNo);
                if (sureanot == MessageBoxResult.Yes)
                {
                    int flushcount = 1; //to number each FL in log
                    for (int i = 0; i < TotalLabel; i++)
                    {
                        XmlNodeList MyLabelList = networkmain.FLTrackingdoc.GetElementsByTagName("ID");
                        string XmlFlname = MyLabelList[0].InnerXml;
                        try
                        {

                            networkmain.Client_sendFG01_FG02_MOVE(XmlFlname, "FG01_FG02_MOVE,Technician Do The Royal Flush");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                            throw;
                        }

                        // NEED TO SEND ACK MIDDLEWARE R997
                        try
                        {
                            string RJCODE = "997";
                            XmlDocument doc = new XmlDocument();
                            doc.Load(@"ConfigEvent.xml");
                            XmlNode node = doc.SelectSingleNode(@"/EVENT/R" + RJCODE);
                            string RJName = node.InnerText;

                            networkmain.Client_SendEventMessage("67", RJName, "BOX_ID", XmlFlname);

                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                        }


                        networkmain.OperatorLog = flushcount.ToString() + ". " + XmlFlname + " flushed!";
                        flushcount++;
                        Thread.Sleep(10);
                    }
                    AllLabels.Clear();
                    networkmain.OperatorLog = "Royal Flush Complete! (All labels removed from server) " + DateTime.Now.ToString() + "\r\n \r\n \r\n \r\n";
                    networkmain.AllLiveLogs = "Royally Flushed! " + DateTime.Now.ToString() + "\r\n \r\n \r\n \r\n \r\n \r\n \r\n";

                    MessageBox.Show("All labels have been removed, you may run those lots again.", "Flush Complete");

                    RemoveZPLfiles();
                }
            }
        }
        #endregion
        #region OldCode
        //public void RoyalFlush()
        //{
        //    if (AllLabels.Capacity == 0) //if nothing in All Labels array
        //    {
        //        MessageBox.Show("Nothing to remove.", "The Royal Flush");
        //    }
        //    else
        //    {
        //        MessageBoxResult sureanot = System.Windows.MessageBox.Show(
        //            "Are you sure you want to remove all Labels?", "The Royal Flush", System.Windows.MessageBoxButton.YesNo);
        //        if (sureanot == MessageBoxResult.Yes)
        //        {
        //            int flushcount = 1; //to number each FL in log
        //            foreach (var thing in AllLabels)
        //            {
        //                networkmain.Client_sendFG01_FG02_MOVE(thing.ToString(), "FG01_FG02_MOVE,Technician Do The Royal Flush");
        //                //NEED TO SEND ACK MIDDLEWARE R997
        //                try
        //                {
        //                    string RJCODE = "997";
        //                    XmlDocument doc = new XmlDocument();
        //                    doc.Load(@"ConfigEvent.xml");
        //                    XmlNode node = doc.SelectSingleNode(@"/EVENT/R" + RJCODE);
        //                    string RJName = node.InnerText;

        //                    networkmain.Client_SendEventMessage("67", RJName, "BOX_ID", thing.ToString());

        //                }
        //                catch (Exception ex)
        //                {

        //                }


        //                networkmain.OperatorLog = flushcount.ToString() + ". " + thing.ToString() + " flushed!";
        //                flushcount++;
        //                Thread.Sleep(10);
        //            }
        //            AllLabels.Clear();
        //            networkmain.OperatorLog = "Royal Flush Complete! (All labels removed from server) " + DateTime.Now.ToString() + "\r\n \r\n \r\n \r\n";
        //            networkmain.AllLiveLogs = "Royally Flushed! " + DateTime.Now.ToString() + "\r\n \r\n \r\n \r\n \r\n \r\n \r\n";

        //            MessageBox.Show("All labels have been removed, you may run those lots again.", "Flush Complete");

        //            RemoveZPLfiles();
        //        }
        //    }
        //}
        #endregion
        public void CopyStn6Images()
        {
            Directory.CreateDirectory(@"C:\Station6_image_temp");
            foreach (var file in Directory.GetFiles(@"C:\Station6_image"))
                File.Copy(file, Path.Combine(@"C:\Station6_image_temp", Path.GetFileName(file)), true);
        }

        public void RemoveZPLfiles()
        {
            System.IO.DirectoryInfo directory = new System.IO.DirectoryInfo(@"C:\saplabels");  //Delete zpl files
            foreach (System.IO.FileInfo file in directory.GetFiles()) file.Delete();
            foreach (System.IO.DirectoryInfo subDirectory in directory.GetDirectories()) subDirectory.Delete(true);
        }

        private void JustLogIn(int i) //this function logs u in to the operators no matter what, so we can test runs fast. Remove when handing over
        {
            if (PLCQueryRx6[PLCQueryRx_DM5100 + i] == 0x08 && PLCWriteCommand6[PLCWriteCommand_DM5200 + i] != 0x08)
            {
                if (i == 0)
                {
                    ScannerStatus = BarcodeStatus.LoggedIn;
                    UserName1 = "OP1";
                }
                else
                {
                    ScannerStatus2 = BarcodeStatus.LoggedIn;
                    UserName2 = "OP2";
                }
                PLCWriteCommand6[PLCWriteCommand_DM5200 + i] = 0x08;
            }
        }

        public void UpdateErrorMsg(int Indexi, string msg, bool isCritical)
        {
            if (this.networkmain.PLCErrorMessage[Indexi] != msg)
            {
                this.networkmain.PLCErrorMessage[Indexi] = msg;
                this.networkmain.OperatorLog = msg;

            }

            //Checking for critical errors in msg
            switch (Indexi)
            {
                case 0:
                    if (isCritical) { this.networkmain.CriticalErrors0 = msg; } else { this.networkmain.CriticalErrors0 = ""; }
                    break;
                case 1:
                    if (isCritical) { this.networkmain.CriticalErrors1 = msg; } else { this.networkmain.CriticalErrors1 = ""; }
                    break;
                case 2:
                    if (isCritical) { this.networkmain.CriticalErrors2 = msg; } else { this.networkmain.CriticalErrors2 = ""; }
                    break;
                case 3:
                    if (isCritical) { this.networkmain.CriticalErrors3 = msg; } else { this.networkmain.CriticalErrors3 = ""; }
                    break;
                case 4:
                    if (isCritical) { this.networkmain.CriticalErrors4 = msg; } else { this.networkmain.CriticalErrors4 = ""; }
                    break;
                case 5:
                    if (isCritical) { this.networkmain.CriticalErrors5 = msg; } else { this.networkmain.CriticalErrors5 = ""; }
                    break;
                case 6:
                    if (isCritical) { this.networkmain.CriticalErrors6 = msg; } else { this.networkmain.CriticalErrors6 = ""; }
                    break;
                case 7:
                    if (isCritical) { this.networkmain.CriticalErrors7 = msg; } else { this.networkmain.CriticalErrors7 = ""; }
                    break;
                case 8:
                    if (isCritical) { this.networkmain.CriticalErrors8 = msg; } else { this.networkmain.CriticalErrors8 = ""; }
                    break;
            }


        }
        #region Event Code Messeges
        private String PLC1EventMsg(char Header, int evcode)
        {
            string ReturnStr = "";
            try
            {
                System.Data.DataRow[] drw = dbUtils.GetAllDataSet().EventCode.Select("Header='" + Header + "' and eventCode=" + evcode);
                if (drw != null && drw.Length > 0)
                {
                    // log.Info("DB Get " + drw[0]["Descript"]+" For Header="+ Header+" Evcode="+ evcode);
                }
                XmlDocument doc = new XmlDocument();
                doc.Load(@"ConfigEventcodeSource.xml");


                if (Header == 'E')
                {
                    XmlNode EV = doc.SelectSingleNode(@"/EVENT/EV" + evcode);
                    ReturnStr = EV.InnerText;
                }
                else if (Header == 'A')
                {
                    XmlNode ATT = doc.SelectSingleNode(@"/EVENT/AT" + evcode);
                    ReturnStr = ATT.InnerText;
                }
                else if (Header == 'N')
                {
                    XmlNode ATT = doc.SelectSingleNode(@"/EVENT/EN" + evcode); //<EN3> For Station3
                    ReturnStr = ATT.InnerText;
                }
                else if (Header == 'S')
                {
                    XmlNode ATT = doc.SelectSingleNode(@"/EVENT/AS" + evcode); //<AS> For Station3
                    ReturnStr = ATT.InnerText;
                }
                  else if (Header == 'V')
                {
                    XmlNode VK = doc.SelectSingleNode(@"/EVENT/VK" + evcode); //<VK> For Station3
                   ReturnStr = VK.InnerText;
                }
                else if (Header == 'M')
               {
                    XmlNode MV = doc.SelectSingleNode(@"/EVENT/MV" + evcode); //<VK> For Station3
                    ReturnStr = MV.InnerText;
                }
            }
            catch (Exception)
            {

                throw;
            }
            return ReturnStr;
        }
        #endregion
        #region Error Code Messeges
        private bool ST1CriticalError(int StationNumber, string errcode)
        {
            string[] errcodeArray = errcode.Split(';');
            try
            {
                bool RetResult = false;
                XmlDocument doc = new XmlDocument();
                doc.Load(@"ConfigJAMErrorList.xml");
                foreach (string item in errcodeArray)
                {
                    XmlNode node = doc.SelectSingleNode(@"/ERROR/Station" + "1" + "/E" + item.ToString());
                    if (node == null)
                    {

                        RetResult = RetResult || false;
                    }
                    else
                    {
                        EvtLog.Info("ST1 CriticalErr:  " + item.ToString() + "/STNo:" + StationNumber);
                        //                       PLCWriteCommand[PLCWriteCommand_DM200 + 60] = 0x4; //D230, Station2 Pause trigger
                        RetResult = RetResult || true;
                    }
                };
                return RetResult;

            }
            catch (Exception)
            {

                throw;
            }


        }
        private bool ST2PauseFunction(int StationNumber,string errcode)
        {
          string[] errcodeArray =  errcode.Split(';');
            try
            {
                bool RetResult = false;
                XmlDocument doc = new XmlDocument();
                doc.Load(@"ConfigJAMErrorList.xml");
                foreach (string item in errcodeArray) 
                {
                    System.Data.DataRow[] drs = dbUtils.GetAllDataSet().ErrCode.Select("StNo=" + StationNumber + " and ErrCode=" + item);
                    if (drs != null && (drs.Length > 0))
                    {
                        dbUtils.ErrHistory_Insert(StationNumber, item); //qf
                    }
                    XmlNode node = doc.SelectSingleNode(@"/ERROR/Station" + StationNumber + "/E" + item.ToString());
                    if (node == null)
                    {

                        RetResult = RetResult || false;
                    }
                    else
                    {
                        EvtLog.Info("Pause:  " + item.ToString() + "/STNo:" + StationNumber);
                        PLCWriteCommand[PLCWriteCommand_DM200 + 60] = 0x4; //D230, Station2 Pause trigger
                        RetResult = RetResult || true;
                    }
                };
                return RetResult;
                
            }
            catch (Exception)
            {

                throw;
            }
 
     
        }
    
        private string StnErrToMsg(int StationNumber, int errcode)
        {

            //LPAUtils.DB.DataSetLPADB.ErrCodeRow[]   drs=dbUtils.GetAllDataSet().ErrCode.Select("ErrCode=" + errcode);
            System.Data.DataRow[] drs = dbUtils.GetAllDataSet().ErrCode.Select("StNo=" + StationNumber + " and ErrCode='" + errcode + "'");
            if (drs != null)
            {
                if (drs[0]["Errcode"] != null)
                {
                    return drs[0]["Descript"].ToString();
                }
                else
                {
                    return "error when search errocode :" + errcode;
                }

            }
            return "error when search errocode :" + errcode;
        }
        private string Stn1ErrToMsg(int errcode)
        {
            //XmlDocument doc = new XmlDocument();
            //doc.Load(@"ConfigErrorcodeSource.xml");
            //XmlNode node = doc.SelectSingleNode(@"/ERROR/Station1/E" + errcode);
            //return node.InnerText;

            return StnErrToMsg(1, errcode);
        }
        private string Stn2ErrToMsg(int errcode)
        {
            //XmlDocument doc = new XmlDocument();
            //doc.Load(@"ConfigErrorcodeSource.xml");
            //XmlNode node = doc.SelectSingleNode(@"/ERROR/Station2/E"+errcode);
            //return node.InnerText;

            return StnErrToMsg(2, errcode);
        }

        private string Stn3ErrToMsg(int errcode)
        {

            // XmlDocument doc = new XmlDocument();
            // doc.Load(@"ConfigErrorcodeSource.xml");
            // XmlNode node = doc.SelectSingleNode(@"/ERROR/Station3/E"+errcode);
            //return node.InnerText;

            return StnErrToMsg(3, errcode);

        }

        private string Stn4ErrToMsg(int errcode)
        {

            // XmlDocument doc = new XmlDocument();
            // doc.Load(@"ConfigErrorcodeSource.xml");
            // XmlNode node = doc.SelectSingleNode(@"/ERROR/Station4/E"+errcode);
            //return node.InnerText;

            return StnErrToMsg(4, errcode);

        }

        private string Stn5ErrToMsg(int errcode)
        {

            // XmlDocument doc = new XmlDocument();
            // doc.Load(@"ConfigErrorcodeSource.xml");
            // XmlNode node = doc.SelectSingleNode(@"/ERROR/Station5/E"+errcode);
            //return node.InnerText;

            return StnErrToMsg(5, errcode);
        }

        private string Stn6ErrToMsg(int errcode)
        {

            // XmlDocument doc = new XmlDocument();
            // doc.Load(@"ConfigErrorcodeSource.xml");
            // XmlNode node = doc.SelectSingleNode(@"/ERROR/Station6/E"+errcode);
            //return node.InnerText;

            return StnErrToMsg(6, errcode);
        }

        private string Stn7ErrToMsg(int errcode)
        {
            // XmlDocument doc = new XmlDocument();
            // doc.Load(@"ConfigErrorcodeSource.xml");
            // XmlNode node = doc.SelectSingleNode(@"/ERROR/Station7/E"+errcode);
            //return node.InnerText;

            return StnErrToMsg(7, errcode);

        }

        private string Stn8ErrToMsg(int errcode)
        {

            // XmlDocument doc = new XmlDocument();
            // doc.Load(@"ConfigErrorcodeSource.xml");
            // XmlNode node = doc.SelectSingleNode(@"/ERROR/Station8/E"+errcode);
            //return node.InnerText;

            return StnErrToMsg(8, errcode);
        }


        private string ParameterDesPLC1(int Parcode)
        {

            XmlDocument doc = new XmlDocument();
            doc.Load(@"ConfigParameter.xml");
            XmlNode node = doc.SelectSingleNode(@"/PARAMETER/P" + Parcode);
            return node.InnerText;



        }



        private string ParameterDesPLC2(int Parcode)
        {
            
                XmlDocument doc = new XmlDocument();
                doc.Load(@"ConfigParameter.xml");
                XmlNode node = doc.SelectSingleNode(@"/PARAMETER/P" + Parcode);
                return node.InnerText;

           

        }















        #endregion
    }
}