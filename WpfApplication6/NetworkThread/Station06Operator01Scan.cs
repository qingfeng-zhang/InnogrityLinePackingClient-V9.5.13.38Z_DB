using System;
using System.Text;
using System.Threading;
using System.Xml;
using IGTwpf;
namespace InnogrityLinePackingClient
{
    partial class NetworkThread
    {
        bool EarlierPickupFlag = false;
        public void RunStation06Operator01Scan(object msgobj)
        {
            MainNetworkClass networkmain = (MainNetworkClass)msgobj;
            Operator01State = 0;
            int retrycounter = 0;
            string barcode;
            bool ETI1Down = false;
            byte[] tmparray = new byte[10];
            while (!bTerminate)
            {
                Thread.Sleep(100);
                int tmp0 = ((6220 - 6000) * 2) + 11;  //EVENT ID Calculation
                Int16 QC1ETIMonitor = (Int16)(BitConverter.ToInt16(PLCQueryRxParaPlc2, tmp0)); //DM6220
                
                if (QC1ETIMonitor == 6056 && ETI1Down == false )
                {
                    MyEventQ.AddQ("656;QC1 ETI BUTTON TRIGGER;QcstationNumber;1;OperatorName;" + UserName1);
                    ETI1Down = true;
                }
                else if (QC1ETIMonitor == 0)
                {
                    ETI1Down = false;
                }
                try
                {
                   // JustLogIn(0); //0 is for operator 1, 2 is for operator 2.
                    if ((PLCQueryRx6[PLCQueryRx_DM5100 + 62] == 0x01))
                    {
                        SafetyStatus = AreaStatus.Block;
                    }
                    else if ((PLCQueryRx6[PLCQueryRx_DM5100 + 98] == 99))
                    {
                        SafetyStatus = AreaStatus.EarlyTake;
                        if (!EarlierPickupFlag)
                        {
                            networkmain.Client_SendEventMessage("654", "Station6QC1EarlyPickupDetected", "LotNumber", Station6ForOP1Scanboxid);
                            AllRJEvent.Info("654, Station6QC1EarlyPickupDetected,LotNumber," + Station6ForOP1Scanboxid);
                            EarlierPickupFlag = true;
                        }
                      
                    }
                    else if ((PLCQueryRx6[PLCQueryRx_DM5100 + 98] == 0x01))
                    {
                        SafetyStatus = AreaStatus.MemNoClear ;
                    }
                    else
                    {
                        SafetyStatus = AreaStatus.Null;
                    }
                    switch (Operator01State)
                    {
                        case WAITFORFINISHINGLABEL://wait for finishing label to be in ie. transition from "/0/0/0/0 .." to a 
                            CheckStringClearFor6_OP1(OperatorA_PCtoPLCFinishingLabelOFFSET, Station6ForOP1Scanboxid);
                           
                            //break;
                            if ((PLCQueryRx6[PLCQueryRx_DM5100] != 0x08) || (PLCWriteCommand6[PLCWriteCommand_DM5200] != 0x08))
                                break;
                            Array.Copy(PLCQueryRx6, 111, tmparray, 0, 10);
                            //convert array to string                        
                            Station6ForOP1Scanboxid = System.Text.Encoding.Default.GetString(tmparray);
                            Station6ForOP1Scanboxid.Trim();
                             RJButton="";
                            //relfect finishing label data status to station 6 OP 1
                            if (Station6ForOP1Scanboxid != "\0\0\0\0\0\0\0\0\0\0")
                            {
                              // log6.Info("Operator 1 getting label from station 5 : " + Station6ForOP1Scanboxid);
                              networkmain.linePack.Info("Operator 1 getting label from station 5 : " + Station6ForOP1Scanboxid);
                                //switch state
                                if (CheckStringUpdateFor6_OP1(OperatorA_PCtoPLCFinishingLabelOFFSET, Station6ForOP1Scanboxid))
                                    Operator01State = DISPLAYVALIDLABELINFO;
                                else
                                {
                                    byte[] tmpbyte = new byte[2];
                                    tmpbyte = Encoding.ASCII.GetBytes("ER");
                                    Array.Copy(tmpbyte, 0, PLCWriteCommand6, 95, 2);//copy ER to PLC DM5237
                                    Operator01State = INVALIDLABELHANDLE;
                                }
                            }
                            break;
                        case DISPLAYVALIDLABELINFO://display valid label informations.,... this state may not be necessary...
                            ScannerStatus = BarcodeStatus.NotScanned;//reset scan information
                            //may not be necessary
                            //evnt_FindFinishingLabelForOperator.Set();//display finishing label information on user screen
                            st6finish1 = Station6ForOP1Scanboxid;//display purposes only
                            networkmain.FindFinishingLabelForOperator(Station6ForOP1Scanboxid);
                            st1POcount++;
                            POcount1++;
                            //display finishing label on user screen
                            PLCWriteCommand6[PLCWriteCommand_DM5204] = 0x02;//state 0x02 ready to scan
                            retrycounter = 0;//reset retry counter
                            Operator01State = VALIDLABELRUNSCANNER;
                            break;
                        case VALIDLABELRUNSCANNER://finishing label is valid, turn on scanner
                            try
                            {
                                Array.Copy(PLCQueryRx6, 111, tmparray, 0, 10);
                                //convert array to string                        
                                Station6ForOP1Scanboxid = System.Text.Encoding.Default.GetString(tmparray);
                                Station6ForOP1Scanboxid.Trim();

                                if ((Station6ForOP1Scanboxid == "\0\0\0\0\0\0\0\0\0\0") && (PLCQueryRx6[PLCQueryRx_DM5100 + 4] == 0x0))
                                {

                                    Operator01State = ERHANDLER_STEP02;

                                }

                                Station06OOperator01ScannerConnect(networkmain);
                                Array.Copy(PLCQueryRx6, 111, tmparray, 0, 10);
                                //convert array to string                        
                                Station6ForOP1Scanboxid = System.Text.Encoding.Default.GetString(tmparray);
                                Station6ForOP1Scanboxid.Trim();
                                PLCWriteCommand6[PLCWriteCommand_DM5204] = 0x02;
                                if (retrycounter > X)//exceed retry
                                {
                                    Operator01State = SCANNUMBEREXCEEDHANDLE;
                                }
                                //log6.Info("Operator 1 Scanner FL read " + Station6ForOP1Scanboxid);
                                networkmain.linePack.Info("Operator 1 Scanner FL read " + Station6ForOP1Scanboxid);
                                networkmain.stn6log = Station6ForOP1Scanboxid + " OP1 Scanner read";
                                int cnt = 0;
                                barcode = null;
                                while ((cnt < 100) && (barcode == null))
                                {
                                    try
                                    {
                                        OP1CognexScanner.ReadTimeout = 100;
                                        barcode = OP1CognexScanner.ReadLine();
                                    }
                                    catch (Exception) { }
                                    cnt++;
                                }
                                if (cnt > 100)
                                    throw new TimeoutException();
                                //check if barcode equal to incoming finihsing label
                                //ADD NEW
                                if (evnt_RejForOperator1.WaitOne(0))
                                {
                                    // set ER to PLC
                                    // PLC return 0xF PC return 0xF ==> PLC go to 0x0, PC goto 0x0
                                    // wait for Station6ForOP1Scanboxid == "\0\0\0\0\0\0\0\0\0\0"
                                    // or reset by change state          
                                    //reject event recieved                                            
                                    Operator01State = ERHANDLER_STEP01;
                                    byte[] tmpbyte = new byte[2];
                                    tmpbyte = Encoding.ASCII.GetBytes("ER");
                                    Array.Copy(tmpbyte, 0, PLCWriteCommand6, 95, 2);//copy ER to PLC DM5237
                                   // log6.Info("Station 6 Operator 1 Technician Reject " + Station6ForOP1Scanboxid);

                                    networkmain.linePack.Info("Station 6 Operator 1 Technician Reject " + Station6ForOP1Scanboxid);
                                  //need to send ack to middleware
                                    networkmain.stn6log = Station6ForOP1Scanboxid + " OP1 Technician Reject";

                                 // networkmain.Client_sendFG01_FG02_MOVE(Station6ForOP1Scanboxid, "FG01_FG02_MOVE");


                                    evnt_RejForOperator1.Reset();
                                }







                                if (barcode != null)
                                    if (barcode.Contains(Station6ForOP1Scanboxid))//eliminate additional character issues
                                    {
                       
                                        //if equal,operator reject or shutting down program, exit and update PLC
                                        PLCWriteCommand6[PLCWriteCommand_DM5204] = 0x08;
                                        ScannerStatus = BarcodeStatus.Equal;
                                        // log6.Info("Operator 1 Scanner FL contain " + Station6ForOP1Scanboxid);
                                        Operator01State = 99;
                                    }
                                    else
                                    {
                                        //if not equal, number of retry is X number? if X number prompt display <--= dont exit.. 
                                        //go to read barcode again if not equal.
                                        if (PLCWriteCommand6[PLCWriteCommand_DM5204] != 0x08)
                                        {
                                            networkmain.linePack.Info("Operator 1 Scan Retry");
                                            PLCWriteCommand6[PLCWriteCommand_DM5204] = 0x0F;//why is this removed??!
                                        }
                                        retrycounter++;
                                        ScannerStatus = BarcodeStatus.NotEqual;
                                        barcode = null;
                                        Status = "not equal try again";
                                        //  break;
                                        continue;
                                    }
                            }
                            catch (TimeoutException)
                            {
                                if (PLCWriteCommand6[PLCWriteCommand_DM5204] != 0x08)
                                {
                                    networkmain.linePack.Info("Operator 1 Scan Timeout");
                                    PLCWriteCommand6[PLCWriteCommand_DM5204] = 0x0F;
                                }
                               
                                //not scan
                               // log6.Info("Operator 1 Scanner Timeout set 0 PLCWriteCommand_DM5204 ==0 ---9");
                                networkmain.stn6log = "OP 1 Timeout";
                                networkmain.OperatorLog = "Stn.6 OP 1 Timeout";
                                Status = "Timeout";
                                ScannerStatus = BarcodeStatus.Timeout;
                                barcode = null;
                                evnt_RejForOperator1.Reset(); //
                                evnt_ScannerRetryForOperator1.Reset();//
                                Operator01State = SCANNERTIMEOUTHANDLE;
                            }
                            catch (Exception ex)
                            {
                                if (PLCWriteCommand6[PLCWriteCommand_DM5204] != 0x08)
                                {
                                    networkmain.linePack.Info("Operator 1 Scanner exception " + ex.ToString());
                                    PLCWriteCommand6[PLCWriteCommand_DM5204] = 0x0F;
                                }
                               
                                //log6.Info("Operator 1 Scanner exception " + ex.ToString());
                               
                                networkmain.stn6log = "OP1 exception";
                                networkmain.OperatorLog = "Stn.6 OP1 Scanning Exception Error";
                                //not scan
                                barcode = null;
                                ScannerStatus = BarcodeStatus.ExceptionError;
                                Status = "Exception";
                                evnt_RejForOperator1.Reset();//
                                evnt_ScannerRetryForOperator1.Reset();//
                                Operator01State = SCANNERTIMEOUTHANDLE;
                            }
                            break;
                        case SCANNERTIMEOUTHANDLE://scan time out
                            // await technician reset or plurge from PLC by setting Er or reject on product
                            ScannerStatus = BarcodeStatus.Timeout;
                            
                            Array.Copy(PLCQueryRx6, 111, tmparray, 0, 10);
                            //convert array to string                        
                            Station6ForOP1Scanboxid = System.Text.Encoding.Default.GetString(tmparray);
                            Station6ForOP1Scanboxid.Trim();

                            if ((Station6ForOP1Scanboxid == "\0\0\0\0\0\0\0\0\0\0") && (PLCQueryRx6[PLCQueryRx_DM5100 + 4] == 0x0))
                            {

                                Operator01State = ERHANDLER_STEP02;

                            }
                            if (evnt_ScannerRetryForOperator1.WaitOne(0))
                            {
                                //change state to VALIDLABELRUNSCANNER, reset counter
                                Operator01State = VALIDLABELRUNSCANNER;
                                retrycounter = 0;
                                evnt_ScannerRetryForOperator1.Reset();
                            }
                            if (evnt_RejForOperator1.WaitOne(0))
                            {
                                // set ER to PLC
                                // PLC return 0xF PC return 0xF ==> PLC go to 0x0, PC goto 0x0
                                // wait for Station6ForOP1Scanboxid == "\0\0\0\0\0\0\0\0\0\0"
                                // or reset by change state          
                                //reject event recieved                                            
                                Operator01State = ERHANDLER_STEP01;
                                byte[] tmpbyte = new byte[2];
                                tmpbyte = Encoding.ASCII.GetBytes("ER");
                                Array.Copy(tmpbyte, 0, PLCWriteCommand6, 95, 2);//copy ER to PLC DM5237
                               // log6.Info("Station 6 Operator 1 Technician Reject " + Station6ForOP1Scanboxid);
                               
                                networkmain.linePack.Info("Station 6 Operator 1 Technician Reject " + Station6ForOP1Scanboxid);
                                networkmain.stn6log = Station6ForOP1Scanboxid + " OP1 Technician Reject";
                              //need to send ack to middleware
                                ScannerStatus = BarcodeStatus.Rejected;
                                evnt_RejForOperator1.Reset();
                            }
                            break;
                        case ERHANDLER_STEP01:

                            Array.Copy(PLCQueryRx6, 111, tmparray, 0, 10);
                            //convert array to string                        
                            Station6ForOP1Scanboxid = System.Text.Encoding.Default.GetString(tmparray);
                            Station6ForOP1Scanboxid.Trim();

                            if ((Station6ForOP1Scanboxid == "\0\0\0\0\0\0\0\0\0\0") && (PLCQueryRx6[PLCQueryRx_DM5100 + 4] == 0x0))
                            {

                                Operator01State = ERHANDLER_STEP02;

                            }
                            if (PLCQueryRx6[PLCQueryRx_DM5100 + 4] == 0xF)
                            {
                                //
                                //scxan not matching 0xf...plc wait for instruction...
                                //5237... na.. er etc etc... if rx er.... clear fl
                                //chk 5202 for 0
                                //send messsage to micron server (TBD)
                               try{
                         string RJCODE="603";
                        
                          XmlDocument doc = new XmlDocument();
                          doc.Load(@"ConfigEvent.xml");
                          XmlNode node = doc.SelectSingleNode(@"/EVENT/R"+RJCODE);                         
                          string RJName=node.InnerText;

                                    try //OEE OPERATOR 1 MANUA REJECT
                                    {
                                        XmlNode fltrackingroot = networkmain.FLTrackingdoc.DocumentElement;//this house the list of summarized xml details 
                                        XmlNode selectednode = fltrackingroot.SelectSingleNode("descendant::LABEL[ID='" + Station6ForOP1Scanboxid + "']");
                                        if (selectednode != null)
                                        {
                                            string OEEidOP1 = selectednode.SelectSingleNode("OEEid").InnerText;
                                            rq.UpdST6RJ(OEEidOP1, 6, RJCODE);
                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        IGTOEELog.Info(ex.ToString());
                                    }
                                    networkmain.Client_SendEventMessage("57", RJName,"BOX_ID",Station6ForOP1Scanboxid);
                              //  log6.Info("QC1 Operator RJ Event Send to Middleware " +"603,"+RJName+","+Station6ForOP1Scanboxid);

                                 networkmain.linePack.Info("QC1 Operator RJ Event Send to Middleware " +"603,"+RJName+","+Station6ForOP1Scanboxid);
                                    MyEventQ.AddQ("21;Station6SealedMBBRejectedAtStation6;LotNumber;" + Station6ForOP1Scanboxid + ";QcstationNumber;1;OpearatorName;"
                          + UserName1);
                                   
                                    AllRJEvent.Info("QC1 Operator RJ Event Send to Middleware " +"603,"+RJName+","+Station6ForOP1Scanboxid);
                                 }
                                 catch (Exception ex){

                                      networkmain.linePack.Info(ex);
                                 
                                 }

                                evnt_RejectFinishingLabelForStation6_OP1.Set();                           

                                st1Rejectcount++;
                                PLCWriteCommand6[PLCWriteCommand_DM5200 + 4] = 0xF;
                                Operator01State = ERHANDLER_STEP02;
                            }
                            break;
                        case ERHANDLER_STEP02:
                            Array.Copy(PLCQueryRx6, 111, tmparray, 0, 10);
                            //convert array to string                        
                            Station6ForOP1Scanboxid = System.Text.Encoding.Default.GetString(tmparray);
                            Station6ForOP1Scanboxid.Trim();
                           // log6.Info("Operator 1 getting label from station 5 : " + Station6ForOP1Scanboxid);
                            networkmain.linePack.Info("Operator 1 getting label from station 5 : " + Station6ForOP1Scanboxid);

                            //relfect finishing label data status to station 6 OP 1
                            if ((Station6ForOP1Scanboxid == "\0\0\0\0\0\0\0\0\0\0") && (PLCQueryRx6[PLCQueryRx_DM5100 + 4] == 0x0))
                            {
                               // 
                                //networkmain.stn6log = Station6ForOP1Scanboxid + " OP1 recieving";
                                //ScannerStatus = BarcodeStatus.Arriving;
                                PLCWriteCommand6[PLCWriteCommand_DM5204] = 0X0;
                                PLCWriteCommand6[PLCWriteCommand_DM5200 + 4] = 0x0;//DM202 reject status
                                PLCWriteCommand6[PLCWriteCommand_DM5204 + 4] = 0x0;//DM206 tracking label status
                                Station6ForOP1Scanboxid = "\0\0\0\0\0\0\0\0\0\0";
                                Station6ForOP1TrackingLabel = "\0\0\0\0\0\0\0\0";
                                networkmain.operator1BoxNumber = "";
                                SealerNumberQC1 ="";
                                st6finish1="";
                                station6track1="";
                                Status="";
                                CheckStringClearFor6_OP1(OperatorA_PCtoPLCFinishingLabelOFFSET, _Station6ForOP1Scanboxid);
                                Operator01State = WAITFORFINISHINGLABEL;
                            }
                            break;
                        case ERHANDLER_STEP03:
                            break;
                        case SCANNUMBEREXCEEDHANDLE://scan exceed 3 x
                            // await technician reset or plurge from PLC by setting Er or reject on product
                            ScannerStatus = BarcodeStatus.AttemptOut;
                            Array.Copy(PLCQueryRx6, 111, tmparray, 0, 10);
                            //convert array to string                        
                            Station6ForOP1Scanboxid = System.Text.Encoding.Default.GetString(tmparray);
                            Station6ForOP1Scanboxid.Trim();

                            if ((Station6ForOP1Scanboxid == "\0\0\0\0\0\0\0\0\0\0") && (PLCQueryRx6[PLCQueryRx_DM5100 + 4] == 0x0))
                            {

                                Operator01State = ERHANDLER_STEP02;

                            }

                            if (evnt_ScannerRetryForOperator1.WaitOne(0))
                            {
                                //change state to VALIDLABELRUNSCANNER, reset counter
                                Operator01State = VALIDLABELRUNSCANNER;
                                retrycounter = 0;
                                evnt_ScannerRetryForOperator1.Reset();
                            }
                            if (evnt_RejForOperator1.WaitOne(0))
                            {
                                // set ER to PLC
                                // PLC return 0xF PC return 0xF ==> PLC go to 0x0, PC goto 0x0
                                // wait for Station6ForOP1Scanboxid == "\0\0\0\0\0\0\0\0\0\0"
                                // or reset by change state          
                                //reject event recieved                                            
                                Operator01State = ERHANDLER_STEP01;
                                byte[] tmpbyte = new byte[2];
                                tmpbyte = Encoding.ASCII.GetBytes("ER");
                                Array.Copy(tmpbyte, 0, PLCWriteCommand6, 95, 2);//copy ER to PLC DM5237
                               //need to send ack to middleware
                                evnt_RejForOperator1.Reset();
                            }
                            break;
                        case SCANMATCHWAITTRACKINGLABEL://scan matched, waiting for tracking label or wait for reject
                            if (PLCQueryRx6[PLCQueryRx_DM5100 + 4] == 0xF)
                            {
                                Operator01State = ERHANDLER_STEP01;
                                break;
                            }
                         if (PLCQueryRx6[PLCQueryRx_DM5100 + 4] == 0x9)
                            {
                                Operator01State = SPECLABELPROCESS;
                                break;
                            }
                            Array.Copy(PLCQueryRx6, 111, tmparray, 0, 10);
                            //convert array to string                        
                            Station6ForOP1Scanboxid = System.Text.Encoding.Default.GetString(tmparray);
                            Station6ForOP1Scanboxid.Trim();

                            if ((Station6ForOP1Scanboxid == "\0\0\0\0\0\0\0\0\0\0") && (PLCQueryRx6[PLCQueryRx_DM5100 + 4] == 0x0))
                            {

                                Operator01State = ERHANDLER_STEP02;

                            }
                            byte[] tmparray1 = new byte[8];
                            //DM5020 TrackingLabel Update
                            Array.Copy(PLCQueryRx6, 51, tmparray1, 0, 8);
                            //convert array to string                        
                            Station6ForOP1TrackingLabel = System.Text.Encoding.Default.GetString(tmparray1);
                            #region NewCode28042016
                            if ((PLCQueryRx6[PLCQueryRx_DM5100 + 12] == 0x08 && Station6ForOP1TrackingLabel != "\0\0\0\0\0\0\0\0" && PLCWriteCommand6[PLCWriteCommand_DM5204 + 4] != 99)) //added by GY 28/4/2016
                            {
                                byte[] str1 = barcodechangeposition(tmparray1);
                                string st = System.Text.Encoding.Default.GetString(str1);
                                station6track1 = st.Trim();
                                if (networkmain.CheckTrackingLabel(Station6ForOP1TrackingLabel))
                                {
                                    MyEventQ.AddQ("645;Station6QC1TLValidationOK;LotNumber;" + Station6ForOP1Scanboxid + ";LicensePlateBarcode;" + station6track1 +
                                        ";QcstationNumber;1;OperatorName;" + UserName1);
                                    networkmain.stn6log = station6track1 + " OP1 TL Validate OK";
                                    PLCWriteCommand6[PLCWriteCommand_DM5204 + 4] = 99;
                                    ScannerStatus = BarcodeStatus.Null;
                                }
                                else
                                {
                                    MyEventQ.AddQ("646;Station6QC1TLValidationNG;LotNumber;" + Station6ForOP1Scanboxid + ";LicensePlateBarcode;" + station6track1 +
                                        ";QcstationNumber;1;OperatorName;" + UserName1);
                                    networkmain.stn6log = station6track1 + " OP1 TL Validate NG";
                                    PLCWriteCommand6[PLCWriteCommand_DM5204 + 4] = 0x0F;
                                    ScannerStatus = BarcodeStatus.Duplicated;
                                    continue;
                                }
                            }
                            else if (PLCQueryRx6[PLCQueryRx_DM5100 + 12] == 0x00)
                            {
                                PLCWriteCommand6[PLCWriteCommand_DM5204 + 4] = 0;
                            }

                            if ((PLCQueryRx6[PLCQueryRx_DM5100 + 12] == 99 && Station6ForOP1TrackingLabel != "\0\0\0\0\0\0\0\0")) //added by GY 28/4/2016
                            {
                                int cnt = 0;
                                if (networkmain.CheckTrackingLabel(Station6ForOP1TrackingLabel))
                                {
                                    while (!networkmain.UpdateTrackingLabel(Station6ForOP1Scanboxid,
                                                                    Station6ForOP1TrackingLabel) &&
                                                                    !bTerminate)//assume PLC data moved before server reply
                                    {
                                        if (cnt > 10) break;
                                        Thread.Sleep(10);
                                        cnt++;
                                        //need to handle invalid tracking label
                                    }
                                    if (cnt > 10) break;
                                    MyEventQ.AddQ("647;Station6QC1TLBindOK;LotNumber;" + Station6ForOP1Scanboxid + ";LicensePlateBarcode;" + station6track1 +
                                        ";QcstationNumber;1;OperatorName;" + UserName1);
                                    PLCWriteCommand6[PLCWriteCommand_DM5204 + 4] = 0x08;// association completed
                                                                                        //--> how to go back 0x0?? if finishing label is /0/0 i can assume all data cleared
                                    networkmain.stn6log = station6track1 + "+" + Station6ForOP1Scanboxid + " OP1 Bind OK";
                                    ScannerStatus = BarcodeStatus.Null;
                                    Operator01State = WAITTRACKINGLABELCLEAR;//create a state to wait for tracking label to clear
                                }
                                else
                                {
                                    MyEventQ.AddQ("648;Station6QC1TLBindNG;LotNumber;" + Station6ForOP1Scanboxid + ";LicensePlateBarcode;" + station6track1 +
                                ";QcstationNumber;1;OperatorName;" + UserName1);
                                    networkmain.stn6log = station6track1 + "+" + Station6ForOP1Scanboxid + " OP1 Bind NG";
                                    PLCWriteCommand6[PLCWriteCommand_DM5204 + 4] = 0x0F;
                                }
                            }
                            #endregion
                            #region ObseleteCode
                            //if ((Station6ForOP1TrackingLabel != "\0\0\0\0\0\0\0\0"))//check for valid tracking label
                            //{

                            //    byte[] str1 = barcodechangeposition(tmparray1);
                            //    string st = System.Text.Encoding.Default.GetString(str1);
                            //    station6track1 = st.Trim();
                            //    //  log6.Info("Operator 1 set tracking label " + station6track1);
                            //    networkmain.linePack.Info("Operator 1 set tracking label " + station6track1);

                            //    networkmain.stn6log = station6track1 + " OP1 Set Tracking Lbl";

                            //    if (networkmain.CheckTrackingLabel(Station6ForOP1TrackingLabel) == true)

                            //    {
                            //        int cnt = 0;
                            //        while (!networkmain.UpdateTrackingLabel(Station6ForOP1Scanboxid,
                            //                                            Station6ForOP1TrackingLabel) &&
                            //                                            !bTerminate)//assume PLC data moved before server reply
                            //        {
                            //            if (cnt > 10) break;
                            //            Thread.Sleep(10);
                            //            cnt++;
                            //            //need to handle invalid tracking label
                            //        }
                            //        if (cnt > 10) break;


                            //        //log6.Info("Operator 1 match " + "," + Station6ForOP1Scanboxid + "," + station6track1);
                            //        networkmain.linePack.Info("Operator 1 match " + "," + Station6ForOP1Scanboxid + "," + station6track1);
                            //        MyEventQ.AddQ("24;Station6SealedMBBAcceptedAtStation6;LotNumber;" + Station6ForOP1Scanboxid + ";LicensePlateBarcode;" + station6track1 +
                            //            ";QcstationNumber;1;OperatorName;" + UserName1);
                            //        PLCWriteCommand6[PLCWriteCommand_DM5204 + 4] = 0x08;// association completed
                            //                                                            //--> how to go back 0x0?? if finishing label is /0/0 i can assume all data cleared
                            //        ScannerStatus = BarcodeStatus.Null;
                            //        Operator01State = WAITTRACKINGLABELCLEAR;//create a state to wait for tracking label to clear


                            //    }
                            //    else

                            //    {
                            //        // log6.Info("Operator 1 Same tracking label  inside the System " + station6track1);
                            //        networkmain.linePack.Info("Operator 1 Same tracking label  inside the System " + station6track1);
                            //        PLCWriteCommand6[PLCWriteCommand_DM5204 + 4] = 0x0F;
                            //        ScannerStatus = BarcodeStatus.Duplicated;
                            //        continue;


                            //    }

                            //}
                            //else
                            //{
                            //    PLCWriteCommand6[PLCWriteCommand_DM5204 + 4] = 0x00;
                            //    // Station6ForOP1TrackingLabel = "\0\0\0\0\0\0\0\0\0\0";
                            //    //  log6.Info("Operator 1 waiting tracking label " + Station6ForOP1Scanboxid);
                            //    networkmain.linePack.Info("Operator 1 waiting tracking label From PLC2 " + Station6ForOP1Scanboxid);

                            //    // need to check for operator reject signal from PLC 
                            //}
                            #endregion
                            break; //update here
                        case WAITTRACKINGLABELCLEAR:
                            //wait for tracking label and finishing label to clear zero??
                            byte[] tmparray2 = new byte[8];
                            //DM5020 TrackingLabel Update
                            Array.Copy(PLCQueryRx6, 51, tmparray2, 0, 8);
                            //convert array to string                        
                            Station6ForOP1TrackingLabel = System.Text.Encoding.Default.GetString(tmparray2);
                            Array.Copy(PLCQueryRx6, 111, tmparray, 0, 10);
                            //convert array to string                        
                            Station6ForOP1Scanboxid = System.Text.Encoding.Default.GetString(tmparray);
                            Station6ForOP1Scanboxid.Trim();

                            //relfect finishing label data status to station 6 OP 1
                            if ((Station6ForOP1Scanboxid == "\0\0\0\0\0\0\0\0\0\0") &&
                                (Station6ForOP1TrackingLabel == "\0\0\0\0\0\0\0\0"))
                            {

                                //log6.Info("Operator 1 getting label from station 5 : " + Station6ForOP1Scanboxid);
                                networkmain.linePack.Info("Operator 1 getting label from station 5 : " + Station6ForOP1Scanboxid);
                                networkmain.stn6log = Station6ForOP1Scanboxid + " OP1 recieving frm stn.5";
                                //both id goes zero
                                PLCWriteCommand6[PLCWriteCommand_DM5204] = 0X0;
                                PLCWriteCommand6[PLCWriteCommand_DM5200 + 4] = 0x0;//DM202 reject status
                                PLCWriteCommand6[PLCWriteCommand_DM5204 + 4] = 0x0;//DM206 tracking label status
                                networkmain.operator1BoxNumber = "";
                                Station6ForOP1Scanboxid = "\0\0\0\0\0\0\0\0\0\0";
                                Station6ForOP1TrackingLabel = "\0\0\0\0\0\0\0\0";
                                SealerNumberQC1="";
                                st6finish1="";
                                station6track1="";
                                Status="";
                                CheckStringClearFor6_OP1(OperatorA_PCtoPLCFinishingLabelOFFSET, _Station6ForOP1Scanboxid);
                                EarlierPickupFlag = false;
                                Operator01State = WAITFORFINISHINGLABEL;
                            }
                            break;
                        case TRACKINGLABELAVAILABLE://tracking label available
                            break;
                        case REJECTRXAFTERSCANMATCHED://reject recieved from PLC @ state AFTERSCANMATCHED
                            break;
                        case INVALIDLABELHANDLE://recieved an invalid finishing label @ state checkforvalidfinishinglabel
                            //wait for label to be 00
                            Operator01State = ERHANDLER_STEP01;
                            break;
                        case 99://recieved an invalid finishing label @ state checkforvalidfinishinglabel
                            //wait for label to be 00
                            if (PLCQueryRx6[PLCQueryRx_DM5100 + 12] == 0x01)
                            {
                                networkmain.linePack.Info("Operator 1 Scanner FL contain " + Station6ForOP1Scanboxid);
                                // networkmain.stn6log = Station6ForOP1Scanboxid + " Scanner FL contain by OP1";
                                Status = "equal";

                                Operator01State = SCANMATCHWAITTRACKINGLABEL;
                            }
                            break;
                        case SPECLABELPROCESS:

                            //byte[] tmpbytesp = new byte[2];
                            //tmpbytesp = Encoding.ASCII.GetBytes("SP");
                            //Array.Copy(tmpbytesp, 0, PLCWriteCommand6, 95, 2);//copy SP to PLC DM5237
                            // log6.Info("Station 6 Operator 1 SPECKTEK " + Station6ForOP1Scanboxid);
                            try //OEE OPERATOR 1 MANUA REJECT
                            {
                                XmlNode fltrackingroot = networkmain.FLTrackingdoc.DocumentElement;//this house the list of summarized xml details 
                                XmlNode selectednode = fltrackingroot.SelectSingleNode("descendant::LABEL[ID='" + Station6ForOP1Scanboxid + "']");
                                if (selectednode != null)
                                {
                                    string OEEidOP1 = selectednode.SelectSingleNode("OEEid").InnerText;
                                    rq.UpdST6RJ(OEEidOP1, 1, "0");
                                }

                            }
                            catch (Exception ex)
                            {
                                IGTOEELog.Info(ex.ToString());
                            }
                            networkmain.linePack.Info("Station 6 Operator 2 SPECKTEK " + Station6ForOP1Scanboxid);
                            MyEventQ.AddQ("22;Station6SealedSpectekAcceptedAtStation6;LotNumber;" + Station6ForOP1Scanboxid + ";QcstationNumber;1;OpearatorName"
                         + UserName1);
                            PLCWriteCommand6[PLCWriteCommand_DM5200 + 4] = 0x09;
                           networkmain.stn6log = Station6ForOP1Scanboxid + " OP1 SPECK";
                         //networkmain.Client_sendFG01_FG02_MOVE1(Station6ForOP1Scanboxid, "FG01_FG02_MOVE");
                           networkmain.Client_sendFG01_FG02_MOVE(Station6ForOP1Scanboxid, "FG01_FG02_MOVE QC1 Spectex");
                            
                            networkmain.Client_sendFG01_FG02_MOVE6(Station6ForOP1Scanboxid); //need to open in real time
                        //NEED TO SEND ACK TO MIDDLEWARE 
                            Operator01State = ERHANDLER_STEP02;

                            break;
                    }
                }
                catch (Exception ex)
                {
                    log.Error(ex.ToString());
                }
            }//
        }
    }
}
