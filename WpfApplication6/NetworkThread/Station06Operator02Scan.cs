using System;
using System.Text;
using System.Threading;
using System.Xml;
using IGTwpf;
namespace InnogrityLinePackingClient
{
    partial class NetworkThread
    {
        bool EarlierPickupFlag2 = false;
        public void RunStation06Operator02Scan(object msgobj)
        {
            MainNetworkClass networkmain = (MainNetworkClass)msgobj;
            Operator02State = 0;
            int retrycounter = 0;
            bool ETI2Down = false;
            string barcode;
            byte[] tmparray = new byte[10];
            while (!bTerminate)
            {
                Thread.Sleep(100);
                int tmp0 = ((6221 - 6000) * 2) + 11;  //EVENT ID Calculation
                Int16 QC2ETIMonitor = (Int16)(BitConverter.ToInt16(PLCQueryRxParaPlc2, tmp0)); //DM6221

                if (QC2ETIMonitor == 6057 && ETI2Down == false)
                {
                    MyEventQ.AddQ("657;QC2 ETI BUTTON TRIGGER;QcstationNumber;2;OperatorName;" + UserName2);
                    ETI2Down = true;
                }
                else if (QC2ETIMonitor == 0)
                {
                    ETI2Down = false;
                }
                try
                {
                   // JustLogIn(2);                    
                    if ((PLCQueryRx6[PLCQueryRx_DM5100 + 64] == 0x01))
                    {
                        SafetyStatus2 = AreaStatus.Block;
                    }
                    else if ((PLCQueryRx6[PLCQueryRx_DM5100 + 66] == 99))
                    {
                        SafetyStatus2 = AreaStatus.EarlyTake;
                        if (!EarlierPickupFlag2)
                        {
                            networkmain.Client_SendEventMessage("655", "Station6QC2EarlyPickupDetected", "LotNumber", Station6ForOP2Scanboxid);
                            AllRJEvent.Info("655, Station6QC2EarlyPickupDetected,LotNumber," + Station6ForOP2Scanboxid);
                            EarlierPickupFlag2 = true;
                        }

                    }
                    else  if ((PLCQueryRx6[PLCQueryRx_DM5100 + 66] == 0x01))
                    {
                        SafetyStatus2 = AreaStatus.MemNoClear;
                    }
                    else
                    {
                        SafetyStatus2 = AreaStatus.Null;
                    }
                    switch (Operator02State)
                    {
                        case WAITFORFINISHINGLABEL://wait for finishing label to be in ie. transition from "/0/0/0/0 .." to a 

                           
                            CheckStringClearFor6_OP2(OperatorB_PCtoPLCFinishingLabelOFFSET, Station6ForOP2Scanboxid);
       
                            if ((PLCQueryRx6[PLCQueryRx_DM5100 + 2] != 0x08) || (PLCWriteCommand6[PLCWriteCommand_DM5200 + 2] != 0x08))
                                break;
                            Array.Copy(PLCQueryRx6, 131, tmparray, 0, 10);
                            //convert array to string                        
                            Station6ForOP2Scanboxid = System.Text.Encoding.Default.GetString(tmparray);
                            Station6ForOP2Scanboxid.Trim();
                            RJButton2="";

                            //relfect finishing label data status to station 6 OP 1
                            if (Station6ForOP2Scanboxid != "\0\0\0\0\0\0\0\0\0\0")
                            {
                               // log6_1.Info("Operator 2 getting label from station 5 : " + Station6ForOP2Scanboxid);

                               networkmain.linePack.Info("Operator 2 getting label from station 5 : " + Station6ForOP2Scanboxid);
                                networkmain.stn6log = Station6ForOP2Scanboxid + " recieving";

                                //switch state
                                if (CheckStringUpdateFor6_OP2(OperatorB_PCtoPLCFinishingLabelOFFSET, Station6ForOP2Scanboxid))
                                    Operator02State = DISPLAYVALIDLABELINFO;
                                else
                                {
                                    byte[] tmpbyte = new byte[2];
                                    tmpbyte = Encoding.ASCII.GetBytes("ER");
                                    Array.Copy(tmpbyte, 0, PLCWriteCommand6, 155, 2);//copy ER to PLC DM5267
                                    Operator02State = INVALIDLABELHANDLE;
                                }
                            }
                            break;
                        case DISPLAYVALIDLABELINFO://display valid label informations.,... this state may not be necessary...
                            ScannerStatus2 = BarcodeStatus.NotScanned;//reset scan information
                            //may not be necessary
                            //evnt_FindFinishingLabelForOperator.Set();//display finishing label information on user screen
                            st6finish2 = Station6ForOP2Scanboxid;//display purposes only
                            networkmain.FindFinishingLabelForOperator2(Station6ForOP2Scanboxid);
                            st2POcount++;
                            POcount2++;
                            //display finishing label on user screen
                            PLCWriteCommand6[PLCWriteCommand_DM5204 + 2] = 0x02;//state 0x02 ready to scan
                            retrycounter = 0;//reset retry counter
                            Operator02State = VALIDLABELRUNSCANNER;
                            break;
                        case VALIDLABELRUNSCANNER://finishing label is valid, turn on scanner
                            try
                            {
                                Array.Copy(PLCQueryRx6, 131, tmparray, 0, 10);
                                //convert array to string                        
                                Station6ForOP2Scanboxid = System.Text.Encoding.Default.GetString(tmparray);
                                Station6ForOP2Scanboxid.Trim();

                                if ((Station6ForOP2Scanboxid == "\0\0\0\0\0\0\0\0\0\0") && (PLCQueryRx6[PLCQueryRx_DM5100 + 6] == 0x0))
                                {
                                    Operator02State = ERHANDLER_STEP02;
                                }
                                Station06OOperator02ScannerConnect(networkmain);
                                Array.Copy(PLCQueryRx6, 131, tmparray, 0, 10);
                                //convert array to string                        
                                Station6ForOP2Scanboxid = System.Text.Encoding.Default.GetString(tmparray);
                                Station6ForOP2Scanboxid.Trim();
                                PLCWriteCommand6[PLCWriteCommand_DM5204 + 2] = 0x02;
                                if (retrycounter > X)//exceed retry
                                {
                                    Operator02State = SCANNUMBEREXCEEDHANDLE;
                                }
                               // log6_1.Info("Operator 2 Scanner FL read " + Station6ForOP2Scanboxid);
                                networkmain.linePack.Info("Operator 2 Scanner FL read " + Station6ForOP2Scanboxid);
                                networkmain.stn6log = Station6ForOP2Scanboxid + " OP2 Scanned";
                                int cnt = 0;
                                barcode = null;
                                while ((cnt < 100) && (barcode == null))
                                {
                                    try
                                    {
                                        OP2CognexScanner.ReadTimeout = 100;
                                        barcode = OP2CognexScanner.ReadLine();
                                    }
                                    catch (Exception) { }
                                    cnt++;
                                }
                                if (cnt > 100)
                                    throw new TimeoutException();
                                if (evnt_RejForOperator2.WaitOne(0))
                                {
                                    // set ER to PLC
                                    // PLC return 0xF PC return 0xF ==> PLC go to 0x0, PC goto 0x0
                                    // wait for Station6ForOP1Scanboxid == "\0\0\0\0\0\0\0\0\0\0"
                                    // or reset by change state          
                                    //reject event recieved                                            
                                    Operator02State = ERHANDLER_STEP01;
                                    byte[] tmpbyte = new byte[2];
                                    tmpbyte = Encoding.ASCII.GetBytes("ER");
                                    Array.Copy(tmpbyte, 0, PLCWriteCommand6, 155, 2);//copy ER to PLC DM5267
                                  //need to send ack to middleware
                                    evnt_RejForOperator2.Reset();
                                }
                                if (barcode != null)
                                    //check if barcode equal to incoming finihsing label
                                    if (barcode.Contains(Station6ForOP2Scanboxid))//eliminate additional character issues
                                    {
                                        //if equal,operator reject or shutting down program, exit and update PLC
                                        PLCWriteCommand6[PLCWriteCommand_DM5204 + 2] = 0x08;
                                        ScannerStatus2 = BarcodeStatus.Equal;
                                        Operator02State = 99;
                                    }
                                    else
                                    {
                                        //if not equal, number of retry is X number? if X number prompt display <--= dont exit.. 
                                        //go to read barcode again if not equal.
                                        if (PLCWriteCommand6[PLCWriteCommand_DM5204 + 2] != 0x08)
                                        {
                                            networkmain.linePack.Info("Operator 2 Scan Retry");
                                            PLCWriteCommand6[PLCWriteCommand_DM5204 + 2] = 0x0F;//why is this removed??!
                                        }
                                     
                                        retrycounter++;
                                        ScannerStatus2 = BarcodeStatus.NotEqual;
                                        barcode = null;
                                        Status1 = "not equal try again";
                                        // break;
                                        continue;
                                    }
                            }
                            catch (TimeoutException)
                            {
                                if (PLCWriteCommand6[PLCWriteCommand_DM5204 + 2] != 0x08)
                                {
                                    networkmain.linePack.Info("Operator 2 Scan Timeout");
                                    PLCWriteCommand6[PLCWriteCommand_DM5204 + 2] = 0x0F;
                                }
                                
                                //not scan
                               // log6_1.Info("Operator 2 Scanner Timeout set 0 PLCWriteCommand_DM5204 + 2 ==0 ---9");
                                networkmain.stn6log = "OP2 Scanner Timeout";
                                networkmain.OperatorLog = "Stn.6 OP2 Timeout";
                                Status1 = "Timeout";
                                ScannerStatus2 = BarcodeStatus.Timeout;
                                barcode = null;
                                evnt_RejForOperator2.Reset();
                                evnt_ScannerRetryForOperator2.Reset();
                                Operator02State = SCANNERTIMEOUTHANDLE;
                            }
                            catch (Exception)
                            {
                                if (PLCWriteCommand6[PLCWriteCommand_DM5204 + 2] != 0x08)
                                {
                                    networkmain.linePack.Info("Operator 2 Scanner exception set 0 PLCWriteCommand_DM5204 ==0 ---9");
                                    PLCWriteCommand6[PLCWriteCommand_DM5204 + 2] = 0x0F;
                                }
                                networkmain.stn6log = "OP2 exception";
                                //not scan
                                barcode = null;
                                ScannerStatus2 = BarcodeStatus.ExceptionError;
                                Status1 = "Exception";
                                evnt_RejForOperator2.Reset();
                                evnt_ScannerRetryForOperator2.Reset();
                                Operator02State = SCANNERTIMEOUTHANDLE;
                            }
                            break;
                        case SCANNERTIMEOUTHANDLE://scan time out
                            // await technician reset or plurge from PLC by setting Er or reject on product
                            Array.Copy(PLCQueryRx6, 131, tmparray, 0, 10);
                            //convert array to string                        
                            Station6ForOP2Scanboxid = System.Text.Encoding.Default.GetString(tmparray);
                            Station6ForOP2Scanboxid.Trim();

                            if ((Station6ForOP2Scanboxid == "\0\0\0\0\0\0\0\0\0\0") && (PLCQueryRx6[PLCQueryRx_DM5100 + 6] == 0x0))
                            {
                                Operator02State = ERHANDLER_STEP02;

                            }
                            if (evnt_ScannerRetryForOperator2.WaitOne(0))
                            {
                                //change state to VALIDLABELRUNSCANNER, reset counter
                                Operator02State = VALIDLABELRUNSCANNER;
                                retrycounter = 0;
                                evnt_ScannerRetryForOperator2.Reset();
                            }
                            if (evnt_RejForOperator2.WaitOne(0))
                            {
                                // set ER to PLC
                                // PLC return 0xF PC return 0xF ==> PLC go to 0x0, PC goto 0x0
                                // wait for Station6ForOP1Scanboxid == "\0\0\0\0\0\0\0\0\0\0"
                                // or reset by change state          
                                //reject event recieved                                            
                                Operator02State = ERHANDLER_STEP01;
                                byte[] tmpbyte = new byte[2];
                                tmpbyte = Encoding.ASCII.GetBytes("ER");
                                Array.Copy(tmpbyte, 0, PLCWriteCommand6, 155, 2);//copy ER to PLC DM5267
                              //need to send ack to middleware
                              // networkmain.Client_sendFG01_FG02_MOVE(Station6ForOP2Scanboxid, "FG01_FG02_MOVE");//NEED TO CHECK 23102015
                                evnt_RejForOperator2.Reset();
                            }
                            break;
                        case ERHANDLER_STEP01:

                            Array.Copy(PLCQueryRx6, 131, tmparray, 0, 10);
                            //convert array to string                        
                            Station6ForOP2Scanboxid = System.Text.Encoding.Default.GetString(tmparray);
                            Station6ForOP2Scanboxid.Trim();

                            if ((Station6ForOP2Scanboxid == "\0\0\0\0\0\0\0\0\0\0") && (PLCQueryRx6[PLCQueryRx_DM5100 + 6] == 0x0))
                            {
                                Operator02State = ERHANDLER_STEP02;

                            }
                            if (PLCQueryRx6[PLCQueryRx_DM5100 + 6] == 0xF)
                            {
                                //
                                //scxan not matching 0xf...plc wait for instruction...
                                //5237... na.. er etc etc... if rx er.... clear fl
                                //chk 5202 for 0
                                //send messsage to micron server (TBD) 604
                               try{
                         string RJCODE="604";
                          XmlDocument doc = new XmlDocument();
                          doc.Load(@"ConfigEvent.xml");
                          XmlNode node = doc.SelectSingleNode(@"/EVENT/R"+RJCODE);                         
                          string RJName=node.InnerText;
                                    try //OEE OPERATOR 1 MANUA REJECT
                                    {
                                        XmlNode fltrackingroot = networkmain.FLTrackingdoc.DocumentElement;//this house the list of summarized xml details 
                                        XmlNode selectednode = fltrackingroot.SelectSingleNode("descendant::LABEL[ID='" + Station6ForOP2Scanboxid + "']");
                                        if (selectednode != null)
                                        {
                                            string OEEidOP2 = selectednode.SelectSingleNode("OEEid").InnerText;
                                            rq.UpdST6RJ(OEEidOP2, 6, RJCODE);
                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        IGTOEELog.Info(ex.ToString());
                                    }
                                    networkmain.Client_SendEventMessage("58", RJName,"BOX_ID",Station6ForOP2Scanboxid);
                                //log6_1.Info("QC2 Operator RJ Event Send to Middleware " +"604,"+RJName+","+Station6ForOP2Scanboxid);
                  networkmain.linePack.Info("QC2 Operator RJ Event Send to Middleware " +"604,"+RJName+","+Station6ForOP2Scanboxid);
                                    MyEventQ.AddQ("21;Station6SealedMBBRejectedAtStation6;LotNumber;" + Station6ForOP1Scanboxid + ";QcstationNumber;1;OpearatorName;"
                          + UserName1);

                                    AllRJEvent.Info("QC2 Operator RJ Event Send to Middleware " +"604,"+RJName+","+Station6ForOP2Scanboxid);
                                 }
                                 catch (Exception ex){

                                      networkmain.linePack.Info(ex);
                                 
                                 }
                              
                                evnt_RejectFinishingLabelForStation6_OP2.Set();

                                st2Rejectcount++;
                                PLCWriteCommand6[PLCWriteCommand_DM5200 + 6] = 0xF;
                                Operator02State = ERHANDLER_STEP02;
                            }
                            break;
                        case ERHANDLER_STEP02:
                            Array.Copy(PLCQueryRx6, 131, tmparray, 0, 10);
                            //convert array to string                        
                            Station6ForOP2Scanboxid = System.Text.Encoding.Default.GetString(tmparray);
                            Station6ForOP2Scanboxid.Trim();
                            //log6_1.Info("Operator 2 getting label from station 5 : " + Station6ForOP2Scanboxid);


                           networkmain.linePack.Info("Operator 2 getting label from station 5 : " + Station6ForOP2Scanboxid);
                            networkmain.stn6log = Station6ForOP2Scanboxid + " OP2 recieving";
                            //relfect finishing label data status to station 6 OP 1
                            if ((Station6ForOP2Scanboxid == "\0\0\0\0\0\0\0\0\0\0") && (PLCQueryRx6[PLCQueryRx_DM5100 + 6] == 0x0))
                            {
                                //CheckStringClearFor6
                                PLCWriteCommand6[PLCWriteCommand_DM5204 + 2] = 0X0;
                                PLCWriteCommand6[PLCWriteCommand_DM5200 + 6] = 0x0;//DM202 reject status
                                PLCWriteCommand6[PLCWriteCommand_DM5204 + 6] = 0x0;//DM206 tracking label status
                                Station6ForOP2Scanboxid = "\0\0\0\0\0\0\0\0\0\0";
                                Station6ForOP2TrackingLabel = "\0\0\0\0\0\0\0\0";
                                networkmain.operator2BoxNumber = "";
                                SealerNumberQC2 ="";
                                st6finish2="";
                                station6track2="";
                                Status1="";
                                CheckStringClearFor6_OP2(OperatorB_PCtoPLCFinishingLabelOFFSET, _Station6ForOP2Scanboxid);
                                Operator02State = WAITFORFINISHINGLABEL;
                                EarlierPickupFlag2 = false;
                            }
                            break;
                        case ERHANDLER_STEP03:
                            break;
                        case SCANNUMBEREXCEEDHANDLE://scan exceed 3 x
                            // await technician reset or plurge from PLC by setting Er or reject on product
                            networkmain.stn6log = "OP2 Exceeded Scan Trys";
                            ScannerStatus2 = BarcodeStatus.AttemptOut;
                            Array.Copy(PLCQueryRx6, 131, tmparray, 0, 10);
                            //convert array to string                        
                            Station6ForOP2Scanboxid = System.Text.Encoding.Default.GetString(tmparray);
                            Station6ForOP2Scanboxid.Trim();

                            if ((Station6ForOP2Scanboxid == "\0\0\0\0\0\0\0\0\0\0") && (PLCQueryRx6[PLCQueryRx_DM5100 + 6] == 0x0))
                            {
                                Operator02State = ERHANDLER_STEP02;
                            }
                            if (evnt_ScannerRetryForOperator2.WaitOne(0))
                            {
                                //change state to VALIDLABELRUNSCANNER, reset counter
                                Operator02State = VALIDLABELRUNSCANNER;
                                retrycounter = 0;
                                evnt_ScannerRetryForOperator2.Reset();
                            }
                            if (evnt_RejForOperator2.WaitOne(0))
                            {
                                // set ER to PLC
                                // PLC return 0xF PC return 0xF ==> PLC go to 0x0, PC goto 0x0
                                // wait for Station6ForOP1Scanboxid == "\0\0\0\0\0\0\0\0\0\0"
                                // or reset by change state          
                                //reject event recieved                                            
                                Operator02State = ERHANDLER_STEP01;
                                byte[] tmpbyte = new byte[2];
                                tmpbyte = Encoding.ASCII.GetBytes("ER");
                                Array.Copy(tmpbyte, 0, PLCWriteCommand6, 155, 2);//copy ER to PLC DM5237
                              //need to send ack to middleware
                              // networkmain.Client_sendFG01_FG02_MOVE(Station6ForOP2Scanboxid, "FG01_FG02_MOVE"); //NEED TO CHECK 23102015
                                evnt_RejForOperator2.Reset();
                            }
                            break;
                        case SCANMATCHWAITTRACKINGLABEL://scan matched, waiting for tracking label or wait for reject
                            if (PLCQueryRx6[PLCQueryRx_DM5100 + 6] == 0xF)
                            {
                                Operator02State = ERHANDLER_STEP01;
                                break;
                            }


                        if (PLCQueryRx6[PLCQueryRx_DM5100 + 6] == 0x9) //change here
                            {
                                Operator02State =SPECLABELPROCESS ;
                                break;
                            }

                            Array.Copy(PLCQueryRx6, 131, tmparray, 0, 10);
                            //convert array to string                        
                            Station6ForOP2Scanboxid = System.Text.Encoding.Default.GetString(tmparray);
                            Station6ForOP2Scanboxid.Trim();

                            if ((Station6ForOP2Scanboxid == "\0\0\0\0\0\0\0\0\0\0") && (PLCQueryRx6[PLCQueryRx_DM5100 + 6] == 0x0))
                            {
                                Operator02State = ERHANDLER_STEP02;
                            }
                            byte[] tmparray1 = new byte[8];
                            //DM5020 TrackingLabel Update
                            Array.Copy(PLCQueryRx6, 71, tmparray1, 0, 8);
                            //convert array to string                        
                            Station6ForOP2TrackingLabel = System.Text.Encoding.Default.GetString(tmparray1);
                            #region New28042016
                            if ((PLCQueryRx6[PLCQueryRx_DM5100 + 14] == 0x08 && Station6ForOP2TrackingLabel != "\0\0\0\0\0\0\0\0") && PLCWriteCommand6[PLCWriteCommand_DM5204 + 6] != 99) //added by GY 28/4/2016
                            {
                                byte[] str1 = barcodechangeposition(tmparray1);
                                string st = System.Text.Encoding.Default.GetString(str1);
                                station6track2 = st.Trim();
                                if (networkmain.CheckTrackingLabel(Station6ForOP2TrackingLabel))
                                {
                                    MyEventQ.AddQ("649;Station6QC2TLValidationOK;LotNumber;" + Station6ForOP2Scanboxid + ";LicensePlateBarcode;" + station6track2 +
                                        ";QcstationNumber;2;OperatorName;" + UserName2);
                                    networkmain.stn6log = station6track2 + " OP2 TL Validate OK";
                                    PLCWriteCommand6[PLCWriteCommand_DM5204 + 6] = 99;
                                    ScannerStatus2 = BarcodeStatus.Null;
                                }
                                else
                                {
                                    MyEventQ.AddQ("650;Station6QC2TLValidationNG;LotNumber;" + Station6ForOP2Scanboxid + ";LicensePlateBarcode;" + station6track2 +
                                        ";QcstationNumber;2;OperatorName;" + UserName2);
                                    networkmain.stn6log = station6track2 + " OP2 TL Validate NG";
                                    PLCWriteCommand6[PLCWriteCommand_DM5204 + 6] = 0x0F;
                                    ScannerStatus2 = BarcodeStatus.Duplicated;
                                    continue;
                                }
                            }
                            else if (PLCQueryRx6[PLCQueryRx_DM5100 + 14] == 0x00 )
                            {
                                PLCWriteCommand6[PLCWriteCommand_DM5204 + 6] = 0;
                            }

                            if ((PLCQueryRx6[PLCQueryRx_DM5100 + 14] == 99 && Station6ForOP2TrackingLabel != "\0\0\0\0\0\0\0\0")) //added by GY 28/4/2016
                            {
                                if (networkmain.CheckTrackingLabel(Station6ForOP2TrackingLabel))
                                {
                                    int cnt = 0;
                                    while (!networkmain.UpdateTrackingLabel(Station6ForOP2Scanboxid,
                                        Station6ForOP2TrackingLabel) &&
                                        !bTerminate)//assume PLC data moved before server reply
                                    {
                                        if (cnt > 10) break;
                                        Thread.Sleep(10);
                                        cnt++;
                                        //need to handle invalid tracking label
                                    }
                                    if (cnt > 10) break;



                                    // log6_1.Info("Operator 2 match " + "," + Station6ForOP2Scanboxid + "," + station6track2);

                                    networkmain.linePack.Info("Operator 2 match " + "," + Station6ForOP2Scanboxid + "," + station6track2);
                                    MyEventQ.AddQ("651;Station6QC2TLBindOK;LotNumber;" + Station6ForOP2Scanboxid + ";LicensePlateBarcode;" + station6track2 + ";QcstationNumber;2;OperatorName;" + UserName2);
                                    PLCWriteCommand6[PLCWriteCommand_DM5204 + 6] = 0x08;// association completed
                                                                                        //--> how to go back 0x0?? if finishing label is /0/0 i can assume all data cleared
                                    networkmain.stn6log = station6track2 + "+" + Station6ForOP2Scanboxid + " OP2 TL Bind OK";
                                    ScannerStatus2 = BarcodeStatus.Null;
                                    Operator02State = WAITTRACKINGLABELCLEAR;//create a state to wait for tracking label to clear

                                }
                                else
                                {
                                    MyEventQ.AddQ("652;Station6QC2TLBindNG;LotNumber;" + Station6ForOP1Scanboxid + ";LicensePlateBarcode;" + station6track1 +
                                ";QcstationNumber;1;OperatorName;" + UserName1);
                                    networkmain.stn6log = station6track2 + "+" + Station6ForOP2Scanboxid + " OP2 TL Bind NG";
                                    PLCWriteCommand6[PLCWriteCommand_DM5204 + 4] = 0x0F;
                                }
                            }
                            #endregion
                            #region ObseleteCode
                            //if ((Station6ForOP2TrackingLabel != "\0\0\0\0\0\0\0\0"))//check for valid tracking label
                            //{
                            //    byte[] str1 = barcodechangeposition(tmparray1);
                            //    string st = System.Text.Encoding.Default.GetString(str1);
                            //    station6track2 = st.Trim();
                            //    // log6_1.Info("Operator 2 set tracking label " + station6track2);

                            //    networkmain.linePack.Info("Operator 2 set tracking label " + station6track2);
                            //    networkmain.stn6log = station6track2 + " OP2 Set Tracking Lbl";
                            //    if (networkmain.CheckTrackingLabel(Station6ForOP2TrackingLabel) == true)

                            //    {


                            //        int cnt = 0;
                            //        while (!networkmain.UpdateTrackingLabel(Station6ForOP2Scanboxid,
                            //            Station6ForOP2TrackingLabel) &&
                            //            !bTerminate)//assume PLC data moved before server reply
                            //        {
                            //            if (cnt > 10) break;
                            //            Thread.Sleep(10);
                            //            cnt++;
                            //            //need to handle invalid tracking label
                            //        }
                            //        if (cnt > 10) break;



                            //        // log6_1.Info("Operator 2 match " + "," + Station6ForOP2Scanboxid + "," + station6track2);

                            //        networkmain.linePack.Info("Operator 2 match " + "," + Station6ForOP2Scanboxid + "," + station6track2);
                            //        MyEventQ.AddQ("24;Station6SealedMBBAcceptedAtStation6;LotNumber;" + Station6ForOP2Scanboxid + ";LicensePlateBarcode;" + station6track2 + ";QcstationNumber;2;OperatorName;" + UserName2);
                            //        PLCWriteCommand6[PLCWriteCommand_DM5204 + 6] = 0x08;// association completed
                            //                                                            //--> how to go back 0x0?? if finishing label is /0/0 i can assume all data cleared
                            //        ScannerStatus2 = BarcodeStatus.Null;
                            //        Operator02State = WAITTRACKINGLABELCLEAR;//create a state to wait for tracking label to clear


                            //    }
                            //    else
                            //    {

                            //        // log6_1.Info("Operator 2 Same tracking label in the System " + station6track2);
                            //        networkmain.linePack.Info("Operator 2 Same tracking label in the System " + station6track2);
                            //        PLCWriteCommand6[PLCWriteCommand_DM5204 + 6] = 0x0F;
                            //        ScannerStatus2 = BarcodeStatus.Duplicated;
                            //        continue;
                            //    }





                            //}
                            //else
                            //{
                            //    PLCWriteCommand6[PLCWriteCommand_DM5204 + 6] = 0x00;
                            //    // Station6ForOP1TrackingLabel = "\0\0\0\0\0\0\0\0\0\0";
                            //    // log6_1.Info("Operator 2 waiting tracking label "+ Station6ForOP2Scanboxid);
                            //    networkmain.linePack.Info("Operator 2 waiting tracking label " + Station6ForOP2Scanboxid);
                            //    // need to check for operator reject signal from PLC 
                            //}
                            #endregion
                            break; //update here
                        case WAITTRACKINGLABELCLEAR:
                            //wait for tracking label and finishing label to clear zero??
                            byte[] tmparray2 = new byte[8];
                            //DM5020 TrackingLabel Update
                            Array.Copy(PLCQueryRx6, 71, tmparray2, 0, 8);
                            //convert array to string                        
                            Station6ForOP2TrackingLabel = System.Text.Encoding.Default.GetString(tmparray2);
                            Array.Copy(PLCQueryRx6, 131, tmparray, 0, 10);
                            //convert array to string                        
                            Station6ForOP2Scanboxid = System.Text.Encoding.Default.GetString(tmparray);
                            Station6ForOP2Scanboxid.Trim();
                          //  log6_1.Info("Operator 2 getting label from station 5 : " + Station6ForOP2Scanboxid);
                           // networkmain.stn6log = Station6ForOP2Scanboxid + " OP2 recieving";
                            //relfect finishing label data status to station 6 OP 1
                            if ((Station6ForOP2Scanboxid == "\0\0\0\0\0\0\0\0\0\0") &&
                                (Station6ForOP2TrackingLabel == "\0\0\0\0\0\0\0\0"))
                            {
                                //both id goes zero
                                 //log6_1.Info("Operator 2 getting label from station 5 : " + Station6ForOP2Scanboxid);

                               networkmain.linePack.Info("Operator 2 getting label from station 5 : " + Station6ForOP2Scanboxid);
                                 networkmain.stn6log = Station6ForOP2Scanboxid + " OP2 recieving";
                               // MyEventQ.AddQ("25;Station6SealedMBBArrivalAtQCStation;LotNumber;" + Station6ForOP2Scanboxid + ";QcstationNumber;2;");
                                PLCWriteCommand6[PLCWriteCommand_DM5204 + 2] = 0X0;
                                PLCWriteCommand6[PLCWriteCommand_DM5200 + 6] = 0x0;//DM202 reject status
                                PLCWriteCommand6[PLCWriteCommand_DM5204 + 6] = 0x0;//DM206 tracking label status
                                Station6ForOP2Scanboxid = "\0\0\0\0\0\0\0\0\0\0";
                                networkmain.operator2BoxNumber = "";
                                Station6ForOP2TrackingLabel = "\0\0\0\0\0\0\0\0";
                                SealerNumberQC2="";
                                st6finish2="";
                                station6track2="";
                                Status1="";
                                CheckStringClearFor6_OP2(OperatorB_PCtoPLCFinishingLabelOFFSET, _Station6ForOP2Scanboxid);
                                Operator02State = WAITFORFINISHINGLABEL;
                            }
                            break;
                        case TRACKINGLABELAVAILABLE://tracking label available
                            break;
                        case 99://reject recieved from PLC @ state AFTERSCANMATCHED
                            if (PLCQueryRx6[PLCQueryRx_DM5100 + 14] == 0x01)
                            {
                                //log6_1.Info("Operator 2 Scanner FL contain " + Station6ForOP2Scanboxid);
                                networkmain.linePack.Info("Operator 2 Scanner FL contain " + Station6ForOP2Scanboxid);
                                networkmain.stn6log = Station6ForOP2Scanboxid + " OP2 Boxed";
                                Status1 = "equal";

                                Operator02State = SCANMATCHWAITTRACKINGLABEL;
                            }
                            break;
                        case REJECTRXAFTERSCANMATCHED://reject recieved from PLC @ state AFTERSCANMATCHED
                            break;
                        case INVALIDLABELHANDLE://recieved an invalid finishing label @ state checkforvalidfinishinglabel
                            //wait for label to be 00
                            Operator02State = ERHANDLER_STEP01;
                            break;
                        case SPECLABELPROCESS:
                            //byte[] tmpbyteSP = new byte[2];
                            //tmpbyteSP = Encoding.ASCII.GetBytes("SP");
                            //Array.Copy(tmpbyteSP, 0, PLCWriteCommand6, 155, 2);//copy SP to PLC DM5267             
                            // log6_1.Info("Station 6 Operator 1 SPECKTEK " + Station6ForOP2Scanboxid);
                            //need to send SMS to micron
                            try //OEE OPERATOR 1 MANUA REJECT
                            {
                                XmlNode fltrackingroot = networkmain.FLTrackingdoc.DocumentElement;//this house the list of summarized xml details 
                                XmlNode selectednode = fltrackingroot.SelectSingleNode("descendant::LABEL[ID='" + Station6ForOP2Scanboxid + "']");
                                if (selectednode != null)
                                {
                                    string OEEidOP2 = selectednode.SelectSingleNode("OEEid").InnerText;
                                    rq.UpdST6RJ(OEEidOP2, 1, "0");
                                }

                            }
                            catch (Exception ex)
                            {
                                IGTOEELog.Info(ex.ToString());
                            }
                            networkmain.linePack.Info("Station 6 Operator 2 SPECTEK " + Station6ForOP2Scanboxid);
                            networkmain.stn6log = Station6ForOP2Scanboxid + " OP2 Speck";
                            // ScannerStatus2 = BarcodeStatus.Speck;
                            MyEventQ.AddQ("22;Station6SealedSpectekAcceptedAtStation6;LotNumber;" + Station6ForOP2Scanboxid + ";QcstationNumber;2;OpearatorName"
                          + UserName2);
                            // networkmain.Client_sendFG01_FG02_MOVE1(Station6ForOP2Scanboxid, "FG01_FG02_MOVE");
                            networkmain.Client_sendFG01_FG02_MOVE(Station6ForOP2Scanboxid, "FG01_FG02_MOVE QC2 Spectek");
                          networkmain.Client_sendFG01_FG02_MOVE62(Station6ForOP2Scanboxid); //need to open in real time
                        //NEED TO SEND ACK TO MIDDLEWARE 

                            PLCWriteCommand6[PLCWriteCommand_DM5200 + 6] = 0x9;
                            Operator02State = ERHANDLER_STEP02;

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
