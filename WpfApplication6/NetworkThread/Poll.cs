//#define DEBUG

using NLog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Xml;
using System.Net.NetworkInformation;
using IGTwpf;
using System.Threading.Tasks;
using System.Linq;
using System.ComponentModel;



namespace InnogrityLinePackingClient
{
    partial class NetworkThread
    {
        bool ST3NewFLRev = false;
        bool ST4NewFLRev = false;
        bool ST5NewFLRev = false;
        bool ST6NewFLRev = false;
        bool ST7NewFLRev = false;
        bool ST8NewFLRev = false;
        bool St1CriticalErrFlag = false;
        bool ST3JamFlag = false;
        bool ST4JamFlag = false;
        bool ST5JamFlag = false;
        bool ST6JamFlag = false;
        bool ST6_1JamFlag = false;
        bool ST7JamFlag = false;
        bool ST8JamFlag = false;
        public  Dictionary<string, DateTime> DesiccantTimingMap = new Dictionary<string, DateTime>();
        public EventQuece MyEventQ = new EventQuece();
        private int[] EventMemPLC1 = new int[420];
        private int[] EventMemPLC2 = new int[520];
        public bool isTray4, isTray7;
        public int parameterflag = 0;
        public int parameterflag2 = 0;
        public int Sealerparameterflag = 0;


        public string message = "";
        public string Err = "";


        public string message1 = "";
        public string Err1 = "";

        public string message2 = "";
        public string Err2 = "";

        public string messagest3 = "";
        public string Errst3 = "";


        public string messagest3_1 = "";
        public string Errst3_1 = "";

        public string messagest3_2 = "";
        public string Errst3_2 = "";

        public string messagest4 = "";
        public string Errst4 = "";


        public string messagest4_1 = "";
        public string Errst4_1 = "";

        public string messagest4_2 = "";
        public string Errst4_2 = "";

        public string messagest6 = "";
        public string Errst6 = "";


        public string messagest6_1 = "";
        public string Errst6_1 = "";

        public string messagest7 = "";
        public string Errst7 = "";
        public string messagest8 = "";
        public string Errst8 = "";
        public int st1Pur = 0;
        public int st2Pur = 0;
        public int st3Pur = 0;
        public int st4Pur = 0;
        public int st5Pur = 0;
        public int st6Pur = 0;
        public int st7Pur = 0;
        public int st8Pur = 0;

        public bool ATDeqOn;
        public bool FirstBlood = true;
        public bool FirstBlood2 = true;

        string FLMembkp4 = "";
        string FLMembkp6 = "";
        string FLMembkp8 = "";
        string FLMembkp8_1 = "";

        public void EnterStation_DoWorks(object sender, DoWorkEventArgs e)
        {
            int[] args = e.Argument as int[];
            int ST = args[0];
            int dm = args[1];
            string stFL = "";

            //Finish Lable  ---->
            byte[] tmparrayFl = new byte[10];
            Array.Copy(PLCQueryRxPara, dm, tmparrayFl, 0, 10);
            //byte tmp;
            //for (int i = 0; i < tmparrayFl.Length; i = i + 2)
            //{
            //    tmp = tmparrayFl[i];
            //    tmparrayFl[i] = tmparrayFl[i + 1];
            //    tmparrayFl[i + 1] = tmp;
            //}
            stFL = System.Text.Encoding.Default.GetString(tmparrayFl);
            EvtLog.Info("ST" + ST + " DM=" + dm.ToString() + " stFL=" + stFL);

            int OEEID = rq.ReqIDbyFL(stFL);

            if (OEEID > 0)
            {
                rq.UpdStNobyID(ST, OEEID);
                EvtLog.Info("ST" + ST + " Entered by  " + OEEID + " FinishingLabel = " + stFL);
            }
            else
            {
                EvtLog.Info("OEEID REQ FAIL for ST" + ST);
            }

        }

        public void EnterStation_DoWorks2(object sender, DoWorkEventArgs e)
        {
            int[] args = e.Argument as int[];
            int ST = args[0];
            int dm = args[1];
            string stFL = "";

            //Finish Lable  ---->
            byte[] tmparrayFl = new byte[10];
            Array.Copy(PLCQueryRxParaPlc2, dm, tmparrayFl, 0, 10);
            //byte tmp;
            //for (int i = 0; i < tmparrayFl.Length; i = i + 2)
            //{
            //    tmp = tmparrayFl[i];
            //    tmparrayFl[i] = tmparrayFl[i + 1];
            //    tmparrayFl[i + 1] = tmp;
            //}
            stFL = System.Text.Encoding.Default.GetString(tmparrayFl);
            EvtLog.Info("ST" + ST + " DM=" + dm.ToString() + " stFL=" + stFL);

            int OEEID = rq.ReqIDbyFL(stFL);

            if (OEEID > 0)
            {
                rq.UpdStNobyID(ST, OEEID);
                EvtLog.Info("ST" + ST + " Entered by  " + OEEID + " FinishingLabel = " + stFL);
            }
            else
            {
                EvtLog.Info("OEEID REQ FAIL for ST" + ST);
            }

        }
        public void ClearAllJam()
        {
            AllRJEvent.Info("ClearAllJam after startup ");
            ClearOneJamm(2);
            ClearOneJamm(3);
            ClearOneJamm(4);
            ClearOneJamm(5);
            ClearOneJamm(6);
            ClearOneJamm(7);
            ClearOneJamm(8);
        }
        public void ClearOneJamm(int stNo)
        {
            string[] FLbatch = rq.UpdJamstatus(stNo, stNo * 10 + stNo, 90 + stNo);

            if (FLbatch.Length > 0)
            {
                foreach (string FL in FLbatch)
                {
                    networkmain.Client_SendEventMessage("9" + stNo, "ST" + stNo + "JAMCLEAR", "BOX_ID", FL);
                    AllRJEvent.Info("9" + stNo+",ST" + stNo + " JAMCLEAR BOX_ID " + FL);
                }
            }
        }
        public void RunPoll(object msgobj)
        {
            //Station6ForOP1Scanboxid = "1234";
#if DEBUG
            Dictionary<string, int> dST8TrackingLabel = new Dictionary<string, int>();
#endif
            MainNetworkClass networkmain = (MainNetworkClass)msgobj;
            PLCTelnet2 = new TelnetClient();
            string EventFomat = PLC1EventMsg('N', 1) + PLC1EventMsg('N', 2) + PLC1EventMsg('N', 3) + PLC1EventMsg('N', 4) + "FirstBlood";
            string[] chkDataArray3 = EventFomat.Split(',');
            string EventFomat2 = PLC1EventMsg('N', 5) + PLC1EventMsg('N', 6) + PLC1EventMsg('N', 7) + PLC1EventMsg('N', 8) + "FirstBlood";
            string[] chkDataArray32 = EventFomat2.Split(',');
            int m = 0;
            #region Connection PLC1,PLC2,Micron Server
            // Log1.Info("Thread Start");
            while (!bTerminate)
            {
                try
                {
                    Thread.Sleep(100);
                    #region PLC 1
                    //PLC Read Write Cycle
                    int abc1 = 0, bcd1 = 0;
                    string kkkk = "";
                    if (PLCTelnet != null)
                        if (PLCTelnet.connected)
                        {
                            #region Station 1 Error code

                            try

                            {

                                #region Event_PLC1
                                try
                                {
                                    for (int h = 0; h < chkDataArray3.Length; h++)
                                    {
                                        //FORMAT ==> "EVENT ID;ATT1 VAL;ATT1 SIZE;ATT1 VAL+ID;ATT1 SIZE;.......SO ON"
                                        Int16 CheckPlC = (Int16)(BitConverter.ToInt16(PLCQueryRxPara, 809));
                                        if (CheckPlC != 999)
                                        {
                                            break;
                                        }
                                        string item = chkDataArray3[h];
                                        string EventEvMsg = "";
                                        string EventATTMsg = "";
                                        string EventATTVal = "";
                                        //string MATTVal = "";
                                        string[] tmpVal = item.Split(';');
                                        if (item == "FirstBlood")
                                        {
                                            FirstBlood = false;
                                            chkDataArray3 = chkDataArray3.Where(w => w != chkDataArray3[chkDataArray3.Length - 1]).ToArray();
                                            break;
                                        }
                                        kkkk = tmpVal[0];
                                        int tmp0 = ((Int16.Parse(tmpVal[0]) - 6100) * 2) + 11;  //EVENT ID Calculation
                                        abc1 = tmp0;
                                        Int16 EventID = (Int16)(BitConverter.ToInt16(PLCQueryRxPara, tmp0)); //EventID Fetch
                                        bcd1 = EventID;
                                        if (EventID == 0)
                                        {
                                            EventMemPLC1[tmp0 / 2] = 0;
                                        }
                                        if (EventID == 1011 && EventID != EventMemPLC1[tmp0 / 2] && !FirstBlood) //OEE BUFFER GET PART
                                        {
                                            rq.INS_Func(DateTime.Now);
                                        }
                                        else if (EventID == 2027 && EventID != EventMemPLC1[tmp0 / 2] && !FirstBlood) //OEE PIN 1 FAIL UPDATE REASON
                                        {
                                            // EventMemPLC1[tmp0 / 2] = EventID;
                                            int MyId = rq.ReqLastID(); //USE MAX ID UPDATE REASON
                                            string MyFL = System.Text.Encoding.Default.GetString(PLCQueryRxPara, 381, 10);
                                            rq.UpdRJReasonbyID(299, MyId);
                                            networkmain.Client_SendEventMessage("75", "ST2 BUFFER PIN 1 DETECTION FAIL", "BOX_ID", MyFL);
                                            AllRJEvent.Info("PLC1 Send Reject Event to MiddleWare " + MyFL + "," + "299" + "," + "ST2 BUFFER PIN 1 DETECTION FAIL");

                                        }
                                        else if (EventID == 2029 && EventID != EventMemPLC1[tmp0 / 2] && !FirstBlood) //OEE BARCODE NG UPDATE REASON
                                        {
                                            //EventMemPLC1[tmp0 / 2] = EventID;
                                            int MyId = rq.ReqLastID(); //USE MAX ID UPDATE REASON
                                            rq.UpdRJReasonbyID(298, MyId);
                                            networkmain.Client_SendEventMessage("74", "ST2 BUFFER FINISHING LABEL READ FAIL", "BOX_ID", "FFFFFFFFFF");
                                            AllRJEvent.Info("PLC1 Send Reject Event to MiddleWare " + "FFFFFFFFFF" + "," + "298" + "," + "ST2 BUFFER FINISHING LABEL READ FAIL");
                                        }
                                        else if (EventID == 2102 && EventID != EventMemPLC1[tmp0 / 2] && !FirstBlood) //OEE BARCODE NG UPDATE REASON
                                        {
                                            //EventMemPLC1[tmp0 / 2] = EventID;
                                            int MyId = rq.ReqLastID(); //USE MAX ID UPDATE REASON
                                            rq.UpdRJReasonbyID(296, MyId);
                                            networkmain.Client_SendEventMessage("80", "ST2 TRAY FINISHING LABEL READ FAIL", "BOX_ID", "FFFFFFFFFF");
                                            AllRJEvent.Info("PLC1 Send Reject Event to MiddleWare " + "FFFFFFFFFF" + "," + "296" + "," + "ST2 TRAY FINISHING LABEL READ FAIL");
                                        }
                                        else if (EventID == 2107 && EventID != EventMemPLC1[tmp0 / 2] && !FirstBlood) //OEE BARCODE NG UPDATE REASON
                                        {
                                            //EventMemPLC1[tmp0 / 2] = EventID;
                                            int MyId = rq.ReqLastID(); //USE MAX ID UPDATE REASON
                                            rq.UpdRJReasonbyID(295, MyId);
                                            networkmain.Client_SendEventMessage("81", "ST2 TRAY CHAMFER NG", "BOX_ID", "FFFFFFFFFF");
                                            AllRJEvent.Info("PLC1 Send Reject Event to MiddleWare " + "FFFFFFFFFF" + "," + "295" + "," + "ST2 TRAY CHAMFER NG");
                                        }
                                        else if (EventID == 2202 && EventID != EventMemPLC1[tmp0 / 2] && !FirstBlood) //OEE BARCODE NG UPDATE REASON
                                        {
                                            //EventMemPLC1[tmp0 / 2] = EventID;
                                            string[] FLbatch = rq.UpdJamstatus(2, 22, 92);
                                            if (FLbatch.Length > 0)
                                            {
                                                foreach (string FL in FLbatch)
                                                {
                                                    networkmain.Client_SendEventMessage("92", "ST2JAMCLEAR", "BOX_ID", FL);
                                                    AllRJEvent.Info("92, ST2JAMCLEAR BOX_ID " + FL);

                                                    //networkmain.Client_sendFG01_FG02_MOVE(FL, "FG01_FG02_MOVE,ST2JAMCLEAR");        
                                                }
                                            }
                                            else
                                            {
                                                AllRJEvent.Info("92, ST2JAMCLEAR, No need to do anything");
                                            }
                                        }
                                        else if (EventID == 2099 && EventID != EventMemPLC2[tmp0 / 2] && !FirstBlood2) //  Station5RobotBagPlaceOnQueuingConveyor       
                                        {
                                            //    --ConfigEventcodeSource.xml  
                                            //   
                                            // DM6290-6100=190    2*190+11=391
                                            BackgroundWorker bw = new BackgroundWorker();
                                            bw.DoWork += new DoWorkEventHandler(EnterStation_DoWorks);

                                            bw.RunWorkerAsync(new int[] { 4, 391 });
                                        }
                                        else if (EventID == 3035 && EventID != EventMemPLC2[tmp0 / 2] && !FirstBlood2) //  Station5RobotBagPlaceOnQueuingConveyor       
                                        {
                                            //    --ConfigEventcodeSource.xml  
                                            //   
                                            // DM6304-6100=204    2*204+11=419
                                            // DM6340-6100=240    2*240+11=491 actual correct
                                            BackgroundWorker bw = new BackgroundWorker();
                                            bw.DoWork += new DoWorkEventHandler(EnterStation_DoWorks);

                                            bw.RunWorkerAsync(new int[] { 4, 419 });
                                        }
                                        else if (EventID == 3029 && EventID != EventMemPLC1[tmp0 / 2] && !FirstBlood) //OEE BARCODE NG UPDATE REASON
                                        {
                                            //EventMemPLC1[tmp0 / 2] = EventID;
                                            string[] FLbatch = rq.UpdJamstatus(3, 33, 93);
                                            if (FLbatch.Length > 0)
                                            {
                                                foreach (string FL in FLbatch)
                                                {
                                                    networkmain.Client_SendEventMessage("93", "ST3AMCLEAR", "BOX_ID", FL);
                                                    AllRJEvent.Info("93, ST3JAMCLEAR BOX_ID " + FL);

                                                    //networkmain.Client_sendFG01_FG02_MOVE(FL, "FG01_FG02_MOVE,ST3JAMCLEAR");    
                                                }
                                            }
                                            else
                                            {
                                                AllRJEvent.Info("93, ST3JAMCLEAR, No need to do anything");
                                            }
                                        }
                                        else if (EventID == 4033 && EventID != EventMemPLC1[tmp0 / 2] && !FirstBlood) //OEE BARCODE NG UPDATE REASON
                                        {
                                            //EventMemPLC1[tmp0 / 2] = EventID;
                                            string[] FLbatch = rq.UpdJamstatus(4, 44, 94);
                                            if (FLbatch.Length > 0)
                                            {
                                                foreach (string FL in FLbatch)
                                                {
                                                    networkmain.Client_SendEventMessage("94", "ST4AMCLEAR", "BOX_ID", FL);
                                                    AllRJEvent.Info("94, ST4JAMCLEAR BOX_ID " + FL);
                                                   // networkmain.Client_sendFG01_FG02_MOVE(FL, "FG01_FG02_MOVE,ST4AMCLEAR");
                                                }
                                            }
                                            else
                                            {
                                                AllRJEvent.Info("94, ST4JAMCLEAR, No need to do anything");
                                            }
                                        }
                                        if (EventID != 0 && EventID != EventMemPLC1[tmp0 / 2]) //If EVENTID change and not equal zero
                                        {
                                            EventMemPLC1[tmp0 / 2] = EventID; //Update the changed
                                            String EventID3Digit = EventATTVal = PLC1EventMsg('V', EventID);
                                            EventEvMsg = EventID3Digit + ";" + PLC1EventMsg('E', EventID);

                                            string tmpData1 = PLC1EventMsg('S', EventID);

                                            int havcoor = tmpData1.IndexOf(";");
                                            if (havcoor < 0)
                                            {
                                                int AttSize = Int16.Parse(tmpData1);
                                                for (int k = 0; k < AttSize; k++)
                                                {
                                                    int i = (k * 2) + 1;
                                                    if (tmpVal[i] != "0")
                                                    {
                                                        int tmp2 = ((Int16.Parse(tmpVal[i]) - 6100) * 2) + 11; //ATT VAL (eg: AT = 1001 & EVENTID = 3008)
                                                                                                               //ATT ID = (13008)                                        

                                                        //ATT Value = (130081)
                                                        int tmp3 = Int16.Parse(tmpVal[i + 1]); //ATT Size
                                                        if (tmp3 == 10) //is Finishing Label
                                                        {
                                                            EventATTVal = System.Text.Encoding.Default.GetString(PLCQueryRxPara, tmp2, tmp3); //Fetch Finishing label direct from PLC
                                                            EventATTMsg = EventATTMsg + ";" + "LotNumber" + ";" + EventATTVal;   //FORMAT (ATT1 Mesg; ATT1 Val;ATT2 Mesg; ATT2 Val
                                                        }
                                                        else if (tmp3 == 12) //SealerID
                                                        {
                                                            EventATTVal = System.Text.Encoding.Default.GetString(PLCQueryRxPara, tmp2, tmp3); //Fetch Finishing label direct from PLC
                                                            EventATTMsg = EventATTMsg + ";" + "SealerID" + ";" + EventATTVal;   //FORMAT (ATT1 Mesg; ATT1 Val;ATT2 Mesg; ATT2 Val
                                                        }
                                                        else if (tmp3 == 8) //TrolleyBarcode
                                                        {

                                                            EventATTVal = System.Text.Encoding.Default.GetString(PLCQueryRxPara, tmp2, tmp3); //Fetch Finishing label direct from PLC
                                                            EventATTVal = EventATTVal.Trim();
                                                            EventATTMsg = EventATTMsg + ";" + "TrolleyBarcode" + ";" + EventATTVal;   //FORMAT (ATT1 Mesg; ATT1 Val;ATT2 Mesg; ATT2 Val
                                                        }

                                                        else if (tmp3 == 1)//is Other
                                                        {

                                                            int AttrVal = Convert.ToInt16(PLCQueryRxParaPlc2[tmp2]);
                                                            string AttStr = i.ToString() + EventID.ToString();
                                                            EventATTVal = PLC1EventMsg('A', Int16.Parse(AttStr)); //Fetch Attribute Val from Xml (e.g: 130081)
                                                            EventATTMsg = EventATTMsg + ";" + EventATTVal + ";" + AttrVal;
                                                        }
                                                        else if (tmp3 == 2)//is Other
                                                        {

                                                            Int32 AttrVal = (Int32)BitConverter.ToInt32(PLCQueryRxParaPlc2, tmp2);
                                                            string AttStr = ((i + 1) / 2).ToString() + EventID.ToString();
                                                            EventATTVal = PLC1EventMsg('A', Int32.Parse(AttStr)); //Fetch Attribute Val from Xml (e.g: 130081)

                                                            //string MulATTVal = PLC1EventMsg('M', Int16.Parse(AttStr));
                                                            //int tempmul = Int16.Parse(MulATTVal);
                                                            //int tempatt = Int16.Parse(EventATTVal);
                                                            //int tempevent = tempmul * tempatt;
                                                            // EventATTVal = tempevent.ToString();

                                                            EventATTMsg = EventATTMsg + ";" + EventATTVal + ";" + AttrVal;
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                string TrayRJ4044 = "";
                                                string[] tmpArray1 = tmpData1.Split(',');
                                                for (int i = 0; i < tmpArray1.Length; i++)
                                                {
                                                    string[] tempDataArray = tmpArray1[i].Split(';');
                                                    if (tempDataArray[0] != "0")
                                                    {
                                                        int dmValue = Int16.Parse(tempDataArray[0]);
                                                        int dmSize = Int16.Parse(tempDataArray[1]);
                                                        int tmp2 = (dmValue - 6100) * 2 + 11; //ATT VAL (eg: AT = 1001 & EVENTID = 3008)
                                                                                              //ATT ID = (13008)                                        

                                                        //ATT Value = (130081)

                                                        if (dmSize == 10) //is Finishing Label
                                                        {
                                                            EventATTVal = System.Text.Encoding.Default.GetString(PLCQueryRxPara, tmp2, dmSize); //Fetch Finishing label direct from PLC
                                                            EventATTMsg = EventATTMsg + ";" + "LotNumber" + ";" + EventATTVal;   //FORMAT (ATT1 Mesg; ATT1 Val;ATT2 Mesg; ATT2 Val
                                                            if (EventID == 4044)
                                                            {
                                                                TrayRJ4044 = EventATTVal;
                                                            }
                                                        }
                                                        else if (dmSize == 12) //is Finishing Label
                                                        {
                                                            EventATTVal = System.Text.Encoding.Default.GetString(PLCQueryRxPara, tmp2, dmSize); //Fetch Finishing label direct from PLC
                                                            EventATTMsg = EventATTMsg + ";" + "SealerID" + ";" + EventATTVal;   //FORMAT (ATT1 Mesg; ATT1 Val;ATT2 Mesg; ATT2 Val
                                                        }
                                                        else if (dmSize == 1)//is Other
                                                        {

                                                            int AttrVal = Convert.ToInt16(PLCQueryRxPara[tmp2]);
                                                            string AttStr = (i + 1).ToString() + EventID.ToString();
                                                            EventATTVal = PLC1EventMsg('A', Int16.Parse(AttStr)); //Fetch Attribute Val from Xml (e.g: 130081)
                                                            EventATTMsg = EventATTMsg + ";" + EventATTVal + ";" + AttrVal;
                                                        }
                                                        else if (dmSize == 2)//is Other
                                                        {
                                                            if (abc1 == 449)
                                                            {
                                                                int am = 1;
                                                                am = am + 1;
                                                            }
                                                            Int32 AttrVal = (Int32)BitConverter.ToInt32(PLCQueryRxPara, tmp2);
                                                            string AttStr = (i + 1).ToString() + EventID.ToString();
                                                            EventATTVal = PLC1EventMsg('A', Int32.Parse(AttStr)); //Fetch Attribute Val from Xml (e.g: 130081)

                                                            string MulATTVal = PLC1EventMsg('M', Int32.Parse(AttStr));
                                                            //Int32 tempevent = Int32.Parse(MulATTVal) ;
                                                            decimal tempevent = decimal.Parse(MulATTVal) * AttrVal;
                                                            string tmp4RJ = "";
                                                            if (EventID == 4044)
                                                            {
                                                                try
                                                                {

                                                                    XmlNode fltrackingroot = networkmain.FLTrackingdoc.DocumentElement;//this house the list of summarized xml details 
                                                                    XmlNode selectednode = fltrackingroot.SelectSingleNode("descendant::LABEL[ID='" + ScanboxidSt4 + "']");

                                                                    if (selectednode != null)
                                                                    {
                                                                        tmp4RJ = selectednode.SelectSingleNode("ErrorCode").InnerText;
                                                                    }


                                                                }
                                                                catch (Exception ex)
                                                                {
                                                                    Log1.Info(ex.ToString());
                                                                }
                                                            }
                                                            if (EventID == 4044 && tmp4RJ == "NA" && ScanboxidSt4 != "FFFFFFFFFF")
                                                            {
                                                                XmlDocument doc = new XmlDocument();
                                                                doc.Load(@"ConfigEvent.xml");
                                                                int RJCODE = 297;
                                                                XmlNode node = doc.SelectSingleNode(@"/EVENT/R" + RJCODE);
                                                                string RJName = node.InnerText;
                                                                XmlNode node1 = doc.SelectSingleNode(@"/EVENT/RK" + RJCODE);
                                                                string NewRjCod = node1.InnerText;
                                                                networkmain.Client_SendEventMessage(NewRjCod.ToString(), RJName, "BOX_ID", ScanboxidSt4);
                                                                AllRJEvent.Info(NewRjCod.ToString() + ";" + RJName + ";" + "BOX_ID" + ";" + ScanboxidSt4);
                                                                EventATTMsg = EventATTMsg + ";" + EventATTVal + ";" + "NA";
                                                                //if (AttrVal == 19279) //OK
                                                                //{
                                                                //    EventATTMsg = EventATTMsg + ";" + EventATTVal + ";" + "OK";
                                                                //}
                                                                //else if (AttrVal == 16718) //NA
                                                                //{
                                                                //    EventATTMsg = EventATTMsg + ";" + EventATTVal + ";" + "NA";
                                                                //}
                                                                //else if (AttrVal == 21061) //ER
                                                                //{
                                                                //    EventATTMsg = EventATTMsg + ";" + EventATTVal + ";" + "ER";
                                                                //}
                                                                //else if (AttrVal == 19026) //RJ
                                                                //{
                                                                //    EventATTMsg = EventATTMsg + ";" + EventATTVal + ";" + "RJ";
                                                                //}
                                                                //else
                                                                //{
                                                                //    EventATTMsg = EventATTMsg + ";" + EventATTVal + ";" + tempevent;
                                                                //}
                                                            }
                                                            else
                                                            {
                                                                EventATTMsg = EventATTMsg + ";" + EventATTVal + ";" + tempevent;
                                                            }





                                                        }
                                                    }
                                                }
                                            }
                                            if (EventATTMsg != "")
                                            {
                                                EventEvMsg = EventEvMsg + EventATTMsg; //Message for 1 event done, push it to stack
                                            }
                                            if (!FirstBlood)
                                            {
                                                MyEventQ.AddQ(EventEvMsg);//Push message to stack
                                                //EvtLog.Info("Push IN1:  " + EventEvMsg);
                                            }

                                        }

                                    }
                                }
                                catch (Exception ex)
                                {
                                    EvtLog.Info("ERROR1:" + abc1.ToString() + " " + bcd1.ToString() + " " + kkkk + " " + ex.ToString());
                                }
                                #endregion
                                #region EventAutoDeQ
                                if (!ATDeqOn && MyEventQ.Qcount())
                                {
                                    Task TaskA = Task.Factory.StartNew(() =>
                                    {
                                        AutoDeQ();
                                    });
                                }



                                #endregion


                                byte[] tmparrayER1_ = new byte[2];
                                Array.Copy(PLCQueryRx, 389, tmparrayER1_, 0, 2);  //189=389,190=391,191=393
                                                                                  //convert Byte array to int                 
                                Int32 erst1 = (Int32)(BitConverter.ToInt16(tmparrayER1_, 0));
                                ErrCode1 = erst1.ToString();

                                byte[] tmparrayER_1 = new byte[2];
                                Array.Copy(PLCQueryRx, 391, tmparrayER_1, 0, 2);  //190=391,191=393
                                                                                  //convert Byte array to int                 
                                Int32 erst1_1 = (Int32)(BitConverter.ToInt16(tmparrayER_1, 0));
                                ErrCode1_1 = erst1_1.ToString();

                                byte[] tmparrayER1_2 = new byte[2];
                                Array.Copy(PLCQueryRx, 393, tmparrayER1_2, 0, 2);  //191=393
                                                                                   //convert Byte array to int                 
                                Int32 erst1_2 = (Int32)(BitConverter.ToInt16(tmparrayER1_2, 0));
                                ErrCode1_2 = erst1_2.ToString();







                                if (erst1 > 0 || erst1_1 > 0 || erst1_2 > 0)
                                {


                                    // LogEr.Info("Station 1 Error Code " + ErrCode1 + ErrCode1_1 + ErrCode1_2);
                                    Errmessage1 = "Stn.1 Err " +
                                         (erst1 > 0 ? ErrCode1 + ": " + Stn1ErrToMsg(erst1) : "") +
                                         (erst1_1 > 0 && erst1 != erst1_1 ? ", " + ErrCode1_1 + ": " + Stn1ErrToMsg(erst1_1) : "") +
                                         (erst1_2 > 0 && erst1 != erst1_2 ? ", " + ErrCode1_2 + ": " + Stn1ErrToMsg(erst1_2) : "");

                                    // LogEr.Info(Errmessage1);
                                    St1CriticalErrFlag = ST1CriticalError((int)Station.StationNumber.Station01, erst1.ToString() + ";" + erst1_1.ToString() + ";" + erst1_2.ToString() + ";");

                                }
                                else
                                {
                                    Errmessage1 = String.Empty;
                                }
                                UpdateErrorMsg((int)Station.StationNumber.Station01, Errmessage1, St1CriticalErrFlag);

                                //Check if ST1 JAM, IF JAM LOG

                                if (erst1 > 0 && networkmain.control == 0)
                                {
                                    Err = erst1.ToString();
                                    networkmain.control = 1;
                                    message = Stn1ErrToMsg(erst1);
                                    networkmain.Client_SendAlarmMessage(erst1.ToString(), message, "SET");
                                    LogEr.Info("Error Set st1 " + erst1.ToString() + message);
                                }
                                if (erst1 == 0 && networkmain.control == 1)
                                {
                                    networkmain.Client_SendAlarmMessage(Err, message, "CLEAR");
                                    LogEr.Info("Error Clear st1 " + Err + message);
                                    networkmain.control = 0;
                                    Err = "";
                                    message = "";
                                }
                                if (erst1_1 > 0 && erst1 != erst1_1 && erst1_1 != erst1_2 && networkmain.control1 == 0)
                                {
                                    Err1 = erst1_1.ToString();
                                    networkmain.control1 = 1;
                                    message1 = Stn1ErrToMsg(erst1_1);
                                    networkmain.Client_SendAlarmMessage(erst1_1.ToString(), message1, "SET");
                                    LogEr.Info("Error Set st1 " + erst1_1.ToString() + message1);

                                }
                                if (erst1_1 == 0 && networkmain.control1 == 1)
                                {
                                    networkmain.Client_SendAlarmMessage(Err1, message1, "CLEAR");
                                    networkmain.control1 = 0;
                                    LogEr.Info("Error Clear st1 " + Err1 + message1);
                                    Err1 = "";
                                    message1 = "";

                                }

                                if (erst1_2 > 0 && erst1 != erst1_2 && erst1_1 != erst1_2 && networkmain.control2 == 0)
                                {
                                    Err2 = erst1_2.ToString();
                                    networkmain.control2 = 1;
                                    message2 = Stn1ErrToMsg(erst1_2);
                                    networkmain.Client_SendAlarmMessage(erst1_2.ToString(), message2, "SET");
                                    LogEr.Info("Error Set st1 " + erst1_2.ToString() + message2);

                                }

                                if (erst1_2 == 0 && networkmain.control2 == 1)
                                {
                                    networkmain.Client_SendAlarmMessage(Err2, message2, "CLEAR");
                                    networkmain.control2 = 0;
                                    LogEr.Info("Error Clear st1 " + Err2 + message2);
                                    Err2 = "";
                                    message2 = "";

                                }



                            }


                            catch (Exception ex)
                            {
                                Log1.Info(ex.ToString());
                            }

                            #endregion

                            #region  Station 3


                            try
                            {

                                bool flag = false;
                                byte[] bcarray2 = new byte[10];
                                Array.Copy(PLCQueryRx, 331, bcarray2, 0, 10);  //D160~164
                                ScanboxidSt3 = System.Text.Encoding.Default.GetString(bcarray2);
                                ScanboxidSt3 = ScanboxidSt3.Trim();
                                if ((ScanboxidSt3 != "\0\0\0\0\0\0\0\0\0\0") && (ScanboxidSt3 != "") && (ScanboxidSt3 != null))
                                {
                                    //evt_FinishLabelRequestSt4.Set();
                                    //if (!ST3NewFLRev)
                                    //{
                                    ScanboxidSt31 = ScanboxidSt3;
                                    networkmain.linePack.Info("Finishing Label for station 3 " + ScanboxidSt3);

                                    //     Station3Log.Info("Finishing Label for station 3 " + ScanboxidSt3);

                                    networkmain.stn3log = ScanboxidSt3;
                                    CheckStringUpdateFor3(Station3OFFSET, ScanboxidSt3);
                                    ST3NewFLRev = true;
                                    //}
                                    #region Finishing Label Timer

                                    if ((PLCQueryRx[PLCQueryRx_DM179] == 0x07) && (PLCWriteCommand[PLCWriteCommand_DM399] == 0x00)) //D179=369
                                    { // Desiccant signal.
                                        if (flag == false)
                                        {
                                            try
                                            {
                                                //      Station3Log.Info("Finishing Label for Station 3  PLC Send HIC/Desiccant start Signal " + ScanboxidSt3);

                                                networkmain.linePack.Info("Finishing Label for Station 3  PLC Send HIC/Desiccant start Signal " + ScanboxidSt3);
                                                DesiccantTimingMap.Add(ScanboxidSt3, DateTime.Now);
                                                flag = true;
                                                HIC_Desiccant = "Dispensed";
                                            }
                                            catch
                                            {
                                                // Station3Log.Info("Same Finishing label two time come in " + ScanboxidSt3);
                                                //networkmain.linePack.Info("Same Finishing label two time come in " + ScanboxidSt3);
                                                networkmain.linePack.Info("Same Finishing label two time come in " + ScanboxidSt3);
                                                networkmain.stn3log = ScanboxidSt3 + " Same label come in Error";
                                                PLCWriteCommand[PLCWriteCommand_DM399] = 0x07;
                                            }
                                        }
                                        // Station3Log.Info("Finishing Label for Station 3  PC Send PLC HIC/Desiccant Receive Signal " + ScanboxidSt3);
                                        networkmain.linePack.Info("Finishing Label for Station 3  PC Send PLC HIC/Desiccant Receive Signal " + ScanboxidSt3);

                                        networkmain.stn3log = ScanboxidSt3 + " Desiccant recieved from PLC st3";
                                        PLCWriteCommand[PLCWriteCommand_DM399] = 0x07;
                                    }
                                    #endregion

                                    if (PLCQueryRx[PLCQueryRx_DM179] == 0x00) //What is this for? #questionforpon
                                    {
                                        PLCWriteCommand[PLCWriteCommand_DM399] = 0x00;
                                    }

                                    #region RJ update For Station 3
                                    byte[] tmparrayER3 = new byte[2];
                                    Array.Copy(PLCQueryRx, 327, tmparrayER3, 0, 2); //DM158
                                                                                    //convert Byte array to int                 
                                    Int32 er3 = (Int32)(BitConverter.ToInt16(tmparrayER3, 0));
                                    if (er3 > 0)
                                    {
                                        byte[] tmpbyte = new byte[2];
                                        tmpbyte = Encoding.ASCII.GetBytes("RJ");
                                        Array.Copy(tmpbyte, 0, PLCWriteCommand, 351, 2); //365
                                        string rj = "RJ";
                                        // Station3Log.Info("Station 3 Reject " + ScanboxidSt3 + " ' " + er3);
                                        networkmain.linePack.Info("Station 3 Reject " + ScanboxidSt3 + " ' " + er3);

                                        networkmain.stn3log = ScanboxidSt3 + " Reject";
                                        networkmain.OperatorLog = "Stn.3 Rejected " + ScanboxidSt3;
                                        try
                                        {
                                            while ((!networkmain.UpdateRJLabel(ScanboxidSt3, rj, er3.ToString()) && !bTerminate))
                                            {
                                                Thread.Sleep(100);
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            //  Station3Log.Error("station 3 error " + ex);
                                            // networkmain.stn3log = "EX";
                                            byte[] tmpbyte1 = new byte[2];
                                            tmpbyte1 = Encoding.ASCII.GetBytes("RJ");
                                            Array.Copy(tmpbyte1, 0, PLCWriteCommand, 351, 2); //365
                                                                                              //update fail..
                                                                                              // reply to PLC update fail
                                                                                              // may need this other time
                                        }
                                    }
                                    #endregion

                                    if (PLCQueryRx[PLCQueryRx_DM157] == 0x0F)
                                    {
                                        PLCWriteCommand[PLCWriteCommand_DM380] = 0x0F;
                                    }
                                    if (PLCQueryRx[PLCQueryRx_DM157] == 0x09)
                                    {
                                        PLCWriteCommand[PLCWriteCommand_DM380] = 0x09;
                                    }
                                }
                                else
                                {
                                    //PLCWriteCommand[PLCQueryRx_DM157] = 0x00;
                                    ST3NewFLRev = false;
                                    flag = false;
                                    CheckstringClearFor3(Station3OFFSET, ScanboxidSt3); // clear Offset // need to update here 
                                    HIC_Desiccant = "";
                                    ScanboxidSt31 = "";
                                }

                                if ((PLCQueryRx[PLCQueryRx_DM157] == 0x00))
                                {
                                    PLCWriteCommand[PLCWriteCommand_DM380] = 0x00;
                                }

                                #region Station 3 Error code

                                byte[] tmparrayER_ = new byte[2];
                                Array.Copy(PLCQueryRx, 395, tmparrayER_, 0, 2);  //192=395,193=397,194=399
                                                                                 //convert Byte array to int                 
                                Int32 erst3 = (Int32)(BitConverter.ToInt16(tmparrayER_, 0));
                                ErrCode3 = erst3.ToString();

                                byte[] tmparrayER_3 = new byte[2];
                                Array.Copy(PLCQueryRx, 397, tmparrayER_3, 0, 2);  //193=397,194=399
                                                                                  //convert Byte array to int                 
                                Int32 erst3_1 = (Int32)(BitConverter.ToInt16(tmparrayER_3, 0));
                                ErrCode3_1 = erst3_1.ToString();

                                byte[] tmparrayER3_2 = new byte[2];
                                Array.Copy(PLCQueryRx, 399, tmparrayER3_2, 0, 2);  //194=399
                                                                                   //convert Byte array to int                 
                                Int32 erst3_2 = (Int32)(BitConverter.ToInt16(tmparrayER3_2, 0));
                                ErrCode3_2 = erst3_2.ToString();




                                #region NewErrorCode

                                if ((erst3 > 0 && erst3 != 18) || erst3_1 > 0 || erst3_2 > 0)
                                {

                                    //  LogEr.Info("Station 3 Error Code " + ErrCode3 + ErrCode3_1 + ErrCode3_2);


                                    Errmessage3 = "Stn.3 Err " +
                                        (((erst3 > 0) && (erst3 != 18)) ? ErrCode3 + ": " + Stn3ErrToMsg(erst3) : "") +
                                        (((erst3_1 > 0) && (erst3_1 != erst3)) ? ", " + ErrCode3_1 + ": " + Stn3ErrToMsg(erst3_1) : "") +
                                        (((erst3_2 > 0) && (erst3_2 != erst3)) ? ", " + ErrCode3_2 + ": " + Stn3ErrToMsg(erst3_2) : "");
                                    if (!ST3JamFlag)
                                    {
                                        bool St3JamTrig = ST2PauseFunction(3, erst3 + ";" + erst3_1 + ";" + erst3_2); //Check if is a JAM
                                        if (St3JamTrig)
                                        {
                                            ST3JamFlag = true;
                                            //string[] FLbatch = rq.UpdJamstatus(3, 333); //Update Jam FL
                                            //if (FLbatch != null)
                                            //{
                                            //    networkmain.Client_SendEventMsg("334", "Station3FLJAMRecovery", FLbatch);//Update Jam recovery FL to middleware
                                            //}
                                        }
                                    }


                                }
                                else
                                {
                                    Errmessage3 = String.Empty;
                                    ST3JamFlag = false;
                                }
                                #endregion
                                #region OldCode
                                //if ((erst3 > 0 && erst3 != 18) || erst3_1 > 0 || erst3_2 > 0)
                                //{

                                //    //  LogEr.Info("Station 3 Error Code " + ErrCode3 + ErrCode3_1 + ErrCode3_2);


                                //    Errmessage3 = "Stn.3 Err " +
                                //        (((erst3 > 0) && (erst3 != 18)) ? ErrCode3 + ": " + Stn3ErrToMsg(erst3) : "") +
                                //        (((erst3_1 > 0) && (erst3_1 != erst3)) ? ", " + ErrCode3_1 + ": " + Stn3ErrToMsg(erst3_1) : "") +
                                //        (((erst3_2 > 0) && (erst3_2 != erst3)) ? ", " + ErrCode3_2 + ": " + Stn3ErrToMsg(erst3_2) : "");

                                //    // LogEr.Info(Errmessage3);

                                //}
                                //else
                                //{
                                //    Errmessage3 = String.Empty;

                                //}
                                #endregion
                                UpdateErrorMsg((int)Station.StationNumber.Station03, Errmessage3, ST3JamFlag);



                                if ((erst3 > 0) && (erst3 != 18) && networkmain.controlst3 == 0)
                                {
                                    Errst3 = erst3.ToString();
                                    networkmain.controlst3 = 1;
                                    messagest3 = Stn3ErrToMsg(erst3);
                                    networkmain.Client_SendAlarmMessage(erst3.ToString(), messagest3, "SET");
                                    LogEr.Info("Error Set st3 " + erst3.ToString() + messagest3);

                                }
                                if (erst3 == 0 && networkmain.controlst3 == 1)
                                {
                                    networkmain.Client_SendAlarmMessage(Errst3, messagest3, "CLEAR");
                                    networkmain.controlst3 = 0;
                                    LogEr.Info("Error CLEAR st3 " + Errst3 + messagest3);
                                    Errst3 = "";
                                    messagest3 = "";
                                }




                                if ((erst3_1 > 0) && (erst3_1 != erst3) && networkmain.controlst3_1 == 0)
                                {
                                    Errst3_1 = erst3_1.ToString();
                                    networkmain.controlst3_1 = 1;
                                    messagest3_1 = Stn3ErrToMsg(erst3_1);
                                    networkmain.Client_SendAlarmMessage(erst3_1.ToString(), messagest3_1, "SET");
                                    LogEr.Info("Error Set st3 " + erst3_1.ToString() + messagest3_1);


                                }
                                if (erst3_1 == 0 && networkmain.controlst3_1 == 1)
                                {
                                    networkmain.Client_SendAlarmMessage(Errst3_1, messagest3_1, "CLEAR");
                                    networkmain.controlst3_1 = 0;
                                    LogEr.Info("Error CLEAR st3 " + Errst3_1 + messagest3_1);
                                    Errst3_1 = "";
                                    messagest3_1 = "";
                                }



                                if ((erst3_2 > 0) && (erst3_2 != erst3) && (erst3_2 != erst3_1) && networkmain.controlst3_2 == 0)
                                {
                                    Errst3_2 = erst3_2.ToString();
                                    networkmain.controlst3_2 = 1;
                                    messagest3_2 = Stn3ErrToMsg(erst3_2);
                                    networkmain.Client_SendAlarmMessage(erst3_2.ToString(), messagest3_2, "SET");
                                    LogEr.Info("Error Set st3 " + erst3_2.ToString() + messagest3_2);

                                }
                                if (erst3_2 == 0 && networkmain.controlst3_2 == 1)
                                {
                                    networkmain.Client_SendAlarmMessage(Errst3_2, messagest3_2, "CLEAR");
                                    LogEr.Info("Error CLEAR st3 " + Errst3_2 + messagest3_2);
                                    networkmain.controlst3_2 = 0;
                                    Errst3_2 = "";
                                    messagest3_2 = "";
                                }

                                #endregion


                            }

                            catch (Exception ex)
                            {
                                Log1.Info(ex.ToString());
                            }





                            #endregion


                            #region  PAUSE
                            try

                            {




                                if (PLCQueryRx[125] == 0x03) //D57
                                {
                                    PurgePau.Info("Middleware Pause,Station1 Stop Signal give back ");
                                    networkmain.PauseON = true;
                                    PLCWriteCommand[451] = 0x05;//D415


                                }



                                if (PLCQueryRx[125] == 0x05) //D151
                                {
                                    PurgePau.Info(" PAUSE,End Signal Get From PLC ");


                                    PLCWriteCommand[451] = 0x00;//D415


                                    networkmain.PauseON = false;

                                }


                            }


                            catch (Exception ex)
                            {
                                Log1.Info(ex.ToString());
                            }
                            #endregion


                            #region  PURGE

                            try
                            {


                                if (PLCQueryRx[313] == 0x08 && PLCQueryRx[315] == 0x08 && PLCQueryRx[125] == 0x08 && PLCQueryRx[127] == 0x08 && PLCQueryRx6[321] == 0x08 && PLCQueryRx6[323] == 0x08 && PLCQueryRx6[325] == 0x08 && PLCQueryRx6[327] == 0x08) //D151
                                {
                                    PurgePau.Info("PURGE Signal give from PLC");
                                    networkmain.PurgeON = true;


                                }


                                if (PLCQueryRx[127] == 0x07 && st2Pur == 0)
                                {
                                    st2Pur = 1;
                                    PurgePau.Info("Station 2 innitialized already,After finished all Purge Products");

                                }


                                if (PLCQueryRx[313] == 0x07 && st3Pur == 0)
                                {
                                    st3Pur = 1;
                                    PurgePau.Info("Station 3 innitialized already,After finished all Purge Products");


                                }



                                if (PLCQueryRx[315] == 0x07 && st4Pur == 0)
                                {
                                    st4Pur = 1;
                                    PurgePau.Info("Station 4 innitialized already,After finished all Purge Products");

                                }

                                if (PLCQueryRx6[321] == 0x07 && st5Pur == 0)
                                {
                                    st5Pur = 1;
                                    PurgePau.Info("Station 5 innitialized already,After finished all Purge Products");

                                }


                                if (PLCQueryRx6[323] == 0x07 && st6Pur == 0)
                                {
                                    st6Pur = 1;
                                    PurgePau.Info("Station 6 innitialized already,After finished all Purge Products");

                                }

                                if (PLCQueryRx6[325] == 0x07 && st7Pur == 0)
                                {
                                    st7Pur = 1;
                                    PurgePau.Info("Station 7 innitialized already,After finished all Purge Products");

                                }


                                if (PLCQueryRx6[327] == 0x07 && st8Pur == 0)
                                {
                                    st8Pur = 1;
                                    PurgePau.Info("Station 8 innitialized already,After finished all Purge Products");

                                }


                                if (PLCQueryRx[127] == 0x07 && PLCQueryRx[313] == 0x07 && PLCQueryRx[315] == 0x07 && PLCQueryRx6[321] == 0x07 && PLCQueryRx6[323] == 0x07 && PLCQueryRx6[325] == 0x07 && PLCQueryRx6[327] == 0x07)
                                {

                                    PurgePau.Info(" After PURGE,Station 2 to Station 8 Initialize finished");
                                    PLCWriteCommand[453] = 0x00;//D416
                                    PLCWriteCommand[455] = 0x00;//D417
                                    PLCWriteCommand[457] = 0x00;//D418
                                    PLCWriteCommand6[457] = 0x00;//D5418
                                    PLCWriteCommand6[339] = 0x00;//D5359
                                    PLCWriteCommand6[341] = 0x00;//D5360
                                    PLCWriteCommand6[343] = 0x00;//D5361 
                                    PLCWriteCommand[451] = 0x07;//D415 st1


                                    st2Pur = 0;
                                    st3Pur = 0;
                                    st4Pur = 0;
                                    st5Pur = 0;
                                    st6Pur = 0;
                                    st7Pur = 0;
                                    st8Pur = 0;


                                }

                                if (PLCQueryRx[125] == 0x07)
                                {
                                    PurgePau.Info("Station 1 initialize finished,Purge mode end");
                                    networkmain.PurgeON = false;
                                    PLCWriteCommand[451] = 0x00;//D415 st1
                                }

                            }

                            catch (Exception ex)
                            {
                                Log1.Info(ex.ToString());
                            }

                            #endregion



                            #region between Station 4 and Station 5

                            try
                            {


                                byte[] bcarray41 = new byte[10];
                                Array.Copy(PLCQueryRx, 231, bcarray41, 0, 10);  //D110~114
                                ScanboxidSt4_5 = System.Text.Encoding.Default.GetString(bcarray41);
                                if (ScanboxidSt4_5 != "\0\0\0\0\0\0\0\0\0\0")
                                {
                                    #region RJ update For Station 4
                                    byte[] tmparrayER4 = new byte[2];
                                    Array.Copy(PLCQueryRx, 321, tmparrayER4, 0, 2); //DM155
                                                                                    //convert Byte array to int                 
                                    Int32 er4 = (Int32)(BitConverter.ToInt16(tmparrayER4, 0));
                                    if (er4 > 0)
                                    {
                                        //byte[] tmpbyte = new byte[2];
                                        //tmpbyte = Encoding.ASCII.GetBytes("RJ");
                                        //Array.Copy(tmpbyte, 0, PLCWriteCommand, 285, 2); //D332=285
                                        string rj = "RJ";
                                        //  Station4Log.Info("Station 4 Reject" + ScanboxidSt4_5 + "'" + er4);

                                        networkmain.linePack.Info("Station 4 Reject " + ScanboxidSt4_5 + "'" + er4);
                                        networkmain.stn4log = ScanboxidSt4_5 + " rejected " + er4;
                                        try
                                        {
                                            while ((!networkmain.UpdateRJLabel(ScanboxidSt4_5, rj, er4.ToString()) && !bTerminate))
                                            {
                                                Thread.Sleep(100);
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            //Station4Log.Error("station 4 error" + ex);
                                            //byte[] tmpbyte1 = new byte[2];
                                            //tmpbyte1 = Encoding.ASCII.GetBytes("RJ");
                                            //Array.Copy(tmpbyte1, 0, PLCWriteCommand, 285, 2); //D332=285
                                            //update fail..
                                            // reply to PLC update fail
                                            // may need this other time
                                        }
                                    }


                                    #endregion
                                    //Station4Log.Info("Station 4 Reject" + ScanboxidSt4_5);
                                    networkmain.linePack.Info("Station 4 Reject " + ScanboxidSt4_5);
                                    PLCWriteCommand[PLCWriteCommand_DM304] = 0x0F;
                                }
                                else
                                {
                                    PLCWriteCommand[PLCWriteCommand_DM304] = 0x00;
                                }

                            }
                            catch (Exception ex)
                            {
                                Log1.Info(ex.ToString());
                            }




                            #endregion




                            #region Station 4 MBB Barcode Checking Buffer
                            try
                            {

                                byte[] bcarray11 = new byte[10];
                                Array.Copy(PLCQueryRx, 251, bcarray11, 0, 10);  //D 120 ~ D124
                                ScanboxidSt4_MBB = System.Text.Encoding.Default.GetString(bcarray11);
                                if (ScanboxidSt4_MBB != "\0\0\0\0\0\0\0\0\0\0")
                                {
                                    MBBFL = ScanboxidSt4_MBB;
                                    // Station4Log.Info("Finishing Label for station 4 MBB Check " + ScanboxidSt4_MBB);
                                    networkmain.linePack.Info("Finishing Label for station 4 MBB Check " + ScanboxidSt4_MBB);

                                    // networkmain.stn4log = "Finishing Label for station 4 MBB Check" + ScanboxidSt4_MBB;
                                    CheckStringUpdateFor4MBB(Station4MBBOFFSET, ScanboxidSt4_MBB);

                                }
                                {

                                    CheckstringClearForstation4MBB(Station4MBBOFFSET, ScanboxidSt4_MBB);
                                    ScanboxidSt4_MBB = "";
                                }

                            }
                            catch (Exception ex)
                            {
                                Log1.Info(ex.ToString());
                            }



                            #endregion





                            #region Station 4

                            try
                            {


                                //edit ......................
                                byte[] bcarray1 = new byte[10];
                                string OEEid4 = "0";
                                Array.Copy(PLCQueryRx, 191, bcarray1, 0, 10);  //D90~D94
                                ScanboxidSt4 = System.Text.Encoding.Default.GetString(bcarray1);
                                if (ScanboxidSt4 != "\0\0\0\0\0\0\0\0\0\0")
                                {
                                    //if (!ST4NewFLRev)
                                    //{
                                    ST4NewFLRev = true;

                                    //evt_FinishLabelRequestSt4.Set();
                                    ScanboxidSt41 = ScanboxidSt4;
                                    // Station4Log.Info("Finishing Label for station 4 " + ScanboxidSt4);

                                    networkmain.linePack.Info("Finishing Label for station 4 " + ScanboxidSt4);

                                    networkmain.stn4log = "Finishing Label for station 4 " + ScanboxidSt4;
                                    CheckStringUpdateFor4(Station4OFFSET, ScanboxidSt4);
                                    //}
                                    #region RJ update For Station 4
                                    byte[] tmparrayER4 = new byte[2];
                                    Array.Copy(PLCQueryRx, 329, tmparrayER4, 0, 2); //DM159
                                    Int32 er4 = (Int32)(BitConverter.ToInt16(tmparrayER4, 0)); //convert Byte array to int 
                                    if (er4 > 0)
                                    {
                                        byte[] tmpbyte = new byte[2];
                                        tmpbyte = Encoding.ASCII.GetBytes("RJ");
                                        Array.Copy(tmpbyte, 0, PLCWriteCommand, 285, 2); //D332=285
                                        string rj = "RJ";
                                        // Station4Log.Info("Station 4 Reject " + ScanboxidSt4 + " ' " + er4);

                                        networkmain.linePack.Info("Station 4 Reject " + ScanboxidSt4 + " ' " + er4);


                                        networkmain.stn4log = ScanboxidSt4 + " rejected " + er4;
                                        try
                                        {
                                            while ((!networkmain.UpdateRJLabel(ScanboxidSt4, rj, er4.ToString()) && !bTerminate))
                                            {
                                                Thread.Sleep(100);
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            // Station4Log.Error("station 4 error " + ex);


                                            byte[] tmpbyte1 = new byte[2];
                                            tmpbyte1 = Encoding.ASCII.GetBytes("RJ");
                                            Array.Copy(tmpbyte1, 0, PLCWriteCommand, 285, 2); //D332=285
                                                                                              //update fail..
                                                                                              // reply to PLC update fail
                                                                                              // may need this other time
                                        }
                                    }


                                    #endregion

                                    //if (PLCQueryRx[PLCQueryRx_DM140] == 0x0F)
                                    //{
                                    //    //reject info send
                                    //    // evnt_RejectFinishingLabelForStation4.Set();
                                    //    Station4Log.Info("St4 reject but continute St5 " + ScanboxidSt4);
                                    //    //networkmain.OperatorLog = "Stn.4 " + ScanboxidSt4 + " Reject but continute st5";
                                    //    //networkmain.stn4log = ScanboxidSt4 + " rejected";
                                    //    PLCWriteCommand[PLCWriteCommand_DM301] = 0x0F;

                                    //}

                                    if (PLCQueryRx[PLCQueryRx_DM140] == 0x07 && PLCWriteCommand[PLCWriteCommand_DM301] == 0x00)
                                    {
                                        //reject info send

                                        try
                                        {

                                            XmlNode fltrackingroot = networkmain.FLTrackingdoc.DocumentElement;//this house the list of summarized xml details 
                                            XmlNode selectednode = fltrackingroot.SelectSingleNode("descendant::LABEL[ID='" + ScanboxidSt4 + "']");

                                            if (selectednode != null)
                                            {
                                                resultst4 = selectednode.SelectSingleNode("PackageStatus").InnerText;
                                                ERCodest4 = selectednode.SelectSingleNode("ErrorCode").InnerText;
                                                OEEid4 = selectednode.SelectSingleNode("OEEid").InnerText;
                                            }


                                        }
                                        catch (Exception ex)
                                        {
                                            Log1.Info(ex.ToString());
                                        }


                                        try
                                        {
                                            string RJCODE = ERCodest4;

                                            if (RJCODE != "" && FLMembkp4 != ScanboxidSt4)
                                            {
                                                //int NewRjCod = 0;
                                                XmlDocument doc = new XmlDocument();
                                                doc.Load(@"ConfigEvent.xml");
                                                XmlNode node = doc.SelectSingleNode(@"/EVENT/R" + RJCODE);
                                                string RJName = node.InnerText;
                                                XmlNode node1 = doc.SelectSingleNode(@"/EVENT/RK" + RJCODE);
                                                string NewRjCod = node1.InnerText;
                                                networkmain.Client_SendEventMessage(NewRjCod.ToString(), RJName, "BOX_ID", ScanboxidSt4);
                                                // Station4Log.Info("Send Reject Event to MiddleWare " + ScanboxidSt4 + "," + RJCODE + "," + RJName);
                                                #region OEEStep3
                                                try
                                                {
                                                    rq.UpdST4RJ(OEEid4, 4, RJCODE);//UPDATE THE OEE DATABASE
                                                }
                                                catch (Exception ex)
                                                {
                                                    IGTOEELog.Info(ex.ToString());
                                                }

                                                #endregion
                                                networkmain.linePack.Info("PLC1 Send Reject Event to MiddleWare " + ScanboxidSt4 + "," + RJCODE + "," + RJName + "," + NewRjCod.ToString());
                                                AllRJEvent.Info("PLC1 Send Reject Event to MiddleWare " + ScanboxidSt4 + "," + RJCODE + "," + RJName + "," + NewRjCod.ToString());
                                                FLMembkp4 = ScanboxidSt4;
                                            }


                                        }
                                        catch (Exception ex)
                                        {

                                        }

                                        evnt_RejectFinishingLabelForStation4.Set();

                                        networkmain.OperatorLog = "Stn.4 " + ScanboxidSt4 + " Reject";
                                        networkmain.stn4log = ScanboxidSt4 + " Rejected";
                                        PLCWriteCommand[PLCWriteCommand_DM301] = 0x07;

                                    }


                                }
                                else
                                {
                                    if (PLCQueryRx[PLCQueryRx_DM140] != 0x99)
                                    {
                                        PLCWriteCommand[PLCWriteCommand_DM301] = 0x00;
                                    }
                                    CheckstringClearFor4(Station4OFFSET, ScanboxidSt4); // clear Offset // need to update here 
                                    CheckstringClearFor4Specktex(461, ScanboxidSt4);
                                    ScanboxidSt41 = "";

                                    VisionStatusst4 = "";
                                    PrintStatusst4 = "";
                                    VisionStatusst41 = "";
                                    PrintStatusst41 = "";
                                    ERCodest4 = "";
                                    resultst4 = "";
                                    ST4NewFLRev = false;



                                }
                                if ((PLCQueryRx[PLCQueryRx_DM140] == 0x00) && (PLCQueryRx[PLCQueryRx_DM140 + 2] == 0x00) && (PLCWriteCommand[PLCWriteCommand_DM301] != 0x00))
                                {
                                    PLCWriteCommand[PLCWriteCommand_DM301] = 0x00;
                                }
                                //if (PLCQueryRx[PLCQueryRx_DM157 + 18] == 0x0)//vision inspection trigger //state 2//DM166 for trigger
                                //{
                                //    PLCWriteCommand[PLCWriteCommand_DM380 + 4] = 0x00; //change here 08 to 00
                                //}
                                if ((PLCQueryRx[PLCQueryRx_DM140] == 0x99)
                                 &&
                                 (PLCWriteCommand[PLCWriteCommand_DM301] == 0x00)
                                 &&
                                 (!evt_Station04PrintReq.WaitOne(0)))// what is this for?? print index? Answer:sometime PLC not set print index,so check 
                                {
                                    PLCWriteCommand[PLCWriteCommand_DM301] = 0x4; //set busy signal
                                    ZebraTestPrint zbt = new ZebraTestPrint();
                                    bool Printok = zbt.ChecknLoadZPLForTestPrint(4);
                                    if (Printok)
                                    {
                                        MyEventQ.AddQ("82;PrinterTestPrint;PrinterNumber;4");//Push message to stack
                                        EvtLog.Info("82;PrinterTestPrint;PrinterNumber;4");
                                    }
                                    else
                                    {
                                        MyEventQ.AddQ("11;PrinterCommunicationBreak;Stationnumber;4");//Push message to stack
                                    }
                                    PLCWriteCommand[PLCWriteCommand_DM301] = 0x99; //set ok signal
                                    zbt = null;
                                }
                                if ((PLCQueryRx[PLCQueryRx_DM140] == 0x01)
                                    &&
                                    (PLCWriteCommand[PLCWriteCommand_DM301] == 0x00)
                                    &&
                                    (!evt_Station04PrintReq.WaitOne(0))
                                    &&
                                    (PLCQueryRx[PLCQueryRx_DM140 + 2] != 0x00))// what is this for?? print index? Answer:sometime PLC not set print index,so check 
                                {
                                    if (ScanboxidSt4.Trim().Length == 0)
                                    {
                                        log.Error("ST4 FL label is empty when requesting print");
                                    }
                                    else if (ScanboxidSt4 == "\0\0\0\0\0\0\0\0\0\0")
                                    {
                                        log.Error("ST4 FL label is \0\0\0\0\0\0\0\0\0\0 when requesting print");
                                    }
                                    else
                                    {
                                        PLCWriteCommand[PLCWriteCommand_DM301] = 0x4; //set busy signal
                                        ST04Rotatary_Str = ScanboxidSt4;

                                        // Station4Log.Info("Station 4 Printing Start");
                                        networkmain.linePack.Info("Station 4 Printing Start :" + ST04Rotatary_Str);
                                        evt_Station04PrintReq.Set();
                                        log.Info("ST4  Start request printing:" + ST04Rotatary_Str);
                                         
                                    }
                                }
                                if ((PLCQueryRx[PLCQueryRx_DM140] == 0x1)//vision inspection ready
                                     &&
                                    (PLCWriteCommand[PLCWriteCommand_DM301] == 0x05)
                                    &&
                                     (!evt_Station04PrintReq.WaitOne(0))//assumming data had been send to vision also
                                    )
                                {


                                    if ((PLCQueryRx[PLCQueryRx_DM140 + 2] == 0x01))
                                    {
                                        PrintStatusst4 = "Received";
                                    }

                                    if ((PLCQueryRx[PLCQueryRx_DM140 + 2] == 0x02))
                                    {
                                        PrintStatusst41 = "Received";
                                    }
                                    PLCWriteCommand[PLCWriteCommand_DM301] = 0x06;//print send complete 
                                                                                  //PrintStatus = "print send complete";
                                }
                                if (PLCQueryRx[PLCQueryRx_DM157 + 18] == 0x0)//vision inspection trigger //state 2//DM166 for trigger
                                {
                                    PLCWriteCommand[PLCWriteCommand_DM380 + 4] = 0x00;//busy  D382                 
                                }
                                if (((PLCQueryRx[PLCQueryRx_DM157 + 18] == 0x2)//vision inspection trigger //state 2//DM166 for trigger
                                     ||
                                    (PLCQueryRx[PLCQueryRx_DM157 + 18] == 0x3))
                                    &&
                                    (PLCWriteCommand[PLCWriteCommand_DM380 + 4] == 0x00)
                                    //vision ready signal
                                    )
                                {


                                    if (PLCQueryRx[PLCQueryRx_DM157 + 20] == 0x01)
                                    {
                                        VisionStatusst4 = "Received";
                                    }
                                    if (PLCQueryRx[PLCQueryRx_DM157 + 20] == 0x02)
                                    {
                                        VisionStatusst41 = "Received";
                                    }
                                    // PLCWriteCommand[PLCWriteCommand_DM380 + 4] = 0x08;//busy
                                    evt_Station04InspectionReq.Set();//request for inspection
                                                                     // PLCWriteCommand[PLCWriteCommand_DM380 + 4] = 0x9;//pass override

                                }

                                #region Printer Clear

                                if ((PLCQueryRx[PLCQueryRx_DM188] == 0x01)
                                    &&
                                    (PLCWriteCommand[PLCWriteCommand_DM401] == 0x00)
                                    &&
                                    (!evt_Station04PrintClearReq.WaitOne(0))
                                  )
                                {
                                    PLCWriteCommand[PLCWriteCommand_DM401] = 0x4; //set busy signal

                                    // Station4Log.Info("Station 4 Printing clear Start");
                                    networkmain.linePack.Info("Station 4 Printing clear Start");
                                    evt_Station04PrintClearReq.Set();
                                }

                                if ((PLCQueryRx[PLCQueryRx_DM188] == 0x1)//vision inspection ready
                                     &&
                                    (PLCWriteCommand[PLCWriteCommand_DM401] == 0x05)
                                    &&
                                     (!evt_Station04PrintClearReq.WaitOne(0))//assumming data had been send to vision also
                                    )
                                {
                                    PLCWriteCommand[PLCWriteCommand_DM401] = 0x06;//print send complete    
                                                                                  //PrintStatus = "print send complete";
                                }
                                if (PLCQueryRx[PLCQueryRx_DM188] == 0x00)
                                {
                                    PLCWriteCommand[PLCWriteCommand_DM401] = 0x00;
                                }
                                #endregion

                                #region Station 4 Error code

                                byte[] tmparrayER4_ = new byte[2];
                                Array.Copy(PLCQueryRx, 401, tmparrayER4_, 0, 2);  //195=401,196=403,197=405
                                Int32 erst4 = (Int32)(BitConverter.ToInt16(tmparrayER4_, 0)); //convert Byte array to int               
                                ErrCode4 = erst4.ToString();

                                byte[] tmparrayER_4 = new byte[2];
                                Array.Copy(PLCQueryRx, 403, tmparrayER_4, 0, 2);  //196=403,197=405
                                Int32 erst4_1 = (Int32)(BitConverter.ToInt16(tmparrayER_4, 0));  //convert Byte array to int        
                                ErrCode4_1 = erst4_1.ToString();

                                byte[] tmparrayER4_2 = new byte[2];
                                Array.Copy(PLCQueryRx, 405, tmparrayER4_2, 0, 2);  //197=405
                                Int32 erst4_2 = (Int32)(BitConverter.ToInt16(tmparrayER4_2, 0)); //convert Byte array to int     
                                ErrCode4_2 = erst4_2.ToString();








                                #region NewErrorCOde
                                if (erst4 > 0 || erst4_1 > 0 || erst4_2 > 0)
                                {

                                    //   LogEr.Info("Station 4 Error Code " + ErrCode4 + ErrCode4_1 + ErrCode4_2);
                                    Errmessage4 = "Stn.4 Err " +
                                        (erst4 > 0 ? ErrCode4 + ": " + Stn4ErrToMsg(erst4) : "") +
                                        (erst4_1 > 0 && erst4_1 != erst4 ?
                                        ", " + ErrCode4_1 + ": " + Stn4ErrToMsg(erst4_1) : "") +
                                        (erst4_2 > 0 && erst4_2 != erst4 ?
                                        ", " + ErrCode4_2 + ": " + Stn4ErrToMsg(erst4_2) : "");
                                    if (!ST4JamFlag)
                                    {
                                        bool St4JamTrig = ST2PauseFunction(4, erst4 + ";" + erst4_1 + ";" + erst4_2); //Check if is a JAM
                                        if (St4JamTrig)
                                        {
                                            ST4JamFlag = true;
                                            //string[] FLbatch = rq.UpdJamstatus(4, 444); //Update Jam FL
                                            //if (FLbatch != null)
                                            //{
                                            //    networkmain.Client_SendEventMsg("447", "Station4FLJAMRecovery", FLbatch);//Update Jam recovery FL to middleware
                                            //}
                                        }
                                    }




                                }
                                else
                                {
                                    ST4JamFlag = false;
                                    Errmessage4 = String.Empty;
                                }
                                #endregion
                                #region OldCode
                                //if (erst4 > 0 || erst4_1 > 0 || erst4_2 > 0)
                                //{

                                //    //   LogEr.Info("Station 4 Error Code " + ErrCode4 + ErrCode4_1 + ErrCode4_2);
                                //    Errmessage4 = "Stn.4 Err " +
                                //        (erst4 > 0 ? ErrCode4 + ": " + Stn4ErrToMsg(erst4) : "") +
                                //        (erst4_1 > 0 && erst4_1 != erst4 ?
                                //        ", " + ErrCode4_1 + ": " + Stn4ErrToMsg(erst4_1) : "") +
                                //        (erst4_2 > 0 && erst4_2 != erst4 ?
                                //        ", " + ErrCode4_2 + ": " + Stn4ErrToMsg(erst4_2) : "");

                                //    //  LogEr.Info(Errmessage4);



                                //}
                                //else
                                //{
                                //    Errmessage4 = String.Empty;
                                //}
                                #endregion
                                UpdateErrorMsg((int)Station.StationNumber.Station04, Errmessage4, ST4JamFlag);


                                if ((erst4 > 0) && networkmain.controlst4 == 0)
                                {
                                    Errst4 = erst4.ToString();
                                    networkmain.controlst4 = 1;
                                    messagest4 = Stn4ErrToMsg(erst4);
                                    networkmain.Client_SendAlarmMessage(erst4.ToString(), messagest4, "SET");
                                    LogEr.Info("Error Set st4 " + erst4.ToString() + messagest4);

                                }
                                if (erst4 == 0 && networkmain.controlst4 == 1)
                                {
                                    networkmain.Client_SendAlarmMessage(Errst4, messagest4, "CLEAR");
                                    LogEr.Info("Error CLEAR st4 " + Errst4 + messagest4);
                                    networkmain.controlst4 = 0;
                                    Errst4 = "";
                                    messagest4 = "";
                                }




                                if ((erst4_1 > 0) && (erst4_1 != erst4) && networkmain.controlst4_1 == 0)
                                {
                                    Errst4_1 = erst4_1.ToString();
                                    networkmain.controlst4_1 = 1;
                                    messagest4_1 = Stn4ErrToMsg(erst4_1);
                                    networkmain.Client_SendAlarmMessage(erst4_1.ToString(), messagest4_1, "SET");
                                    LogEr.Info("Error Set st4 " + erst4_1.ToString() + messagest4_1);

                                }
                                if (erst4_1 == 0 && networkmain.controlst4_1 == 1)
                                {
                                    networkmain.Client_SendAlarmMessage(Errst4_1, messagest4_1, "CLEAR");
                                    networkmain.controlst4_1 = 0;
                                    LogEr.Info("Error CLEAR st4 " + Errst4_1 + messagest4_1);
                                    Errst4_1 = "";
                                    messagest4_1 = "";
                                }



                                if ((erst4_2 > 0) && (erst4_2 != erst4) && (erst4_2 != erst4_1) && networkmain.controlst4_2 == 0)
                                {
                                    Errst4_2 = erst4_2.ToString();
                                    networkmain.controlst4_2 = 1;
                                    messagest4_2 = Stn4ErrToMsg(erst4_2);
                                    networkmain.Client_SendAlarmMessage(erst4_2.ToString(), messagest4_2, "SET");
                                    LogEr.Info("Error Set st4 " + erst4_2.ToString() + messagest4_2);

                                }
                                if (erst4_2 == 0 && networkmain.controlst4_2 == 1)
                                {
                                    networkmain.Client_SendAlarmMessage(Errst4_2, messagest4_2, "CLEAR");
                                    LogEr.Info("Error CLEAR st4 " + Errst4_2 + messagest4_2);
                                    networkmain.controlst4_2 = 0;
                                    Errst4_2 = "";
                                    messagest4_2 = "";
                                }


                                #endregion


                            }

                            catch (Exception ex)
                            {
                                Log1.Info(ex.ToString());
                            }


                            #endregion


                            #region Checke printer Connection at ST4


                            Ping PingPrinter4 = new Ping();

                            try
                            {

                                if ((PLCQueryRx[PLCQueryRx_DM135] == 0x01)
                                    &&
                                    (PLCWriteCommand[PLCWriteCommand_DM428] == 0x00)
                                   )
                                {


                                    PingReply PR4 = PingPrinter4.Send("192.168.3.225");
                                    if (PR4.Status == IPStatus.Success)
                                    {
                                        PLCWriteCommand[PLCWriteCommand_DM428] = 0x09;
                                    }
                                    else if (PR4.Status == IPStatus.DestinationHostUnreachable)
                                    {
                                        PLCWriteCommand[PLCWriteCommand_DM428] = 0xFF;
                                    }

                                }


                                if (PLCQueryRx[PLCQueryRx_DM135] == 0x00)
                                {
                                    PLCWriteCommand[PLCWriteCommand_DM428] = 0x00;

                                }



                            }
                            catch
                            {


                            }

                            #endregion


                            #region Station 4 and Station 5 Transfer and Complete Signal
                            byte[] tmparray3 = new byte[2];
                            //DM5105
                            Array.Copy(PLCQueryRx6, 221, tmparray3, 0, 2);
                            String a = System.Text.Encoding.Default.GetString(tmparray3); //convert array to string  
                            Array.Copy(tmparray3, 0, PLCWriteCommand, 225, 2); //D302
                            // 107 t0 5350 hand shaking
                            byte[] tmparray5 = new byte[2];
                            //DM107
                            Array.Copy(PLCQueryRx, 225, tmparray5, 0, 2);
                            String c = System.Text.Encoding.Default.GetString(tmparray5);
                            Array.Copy(tmparray5, 0, PLCWriteCommand6, 321, 2); //D5350

                            #endregion

                            #region   Station 4 and Station5 transfer

                            try
                            {


                                ////edit .............................
                                byte[] temp = new byte[10];
                                Array.Copy(PLCQueryRx, 295, temp, 0, 10); // DM 142 PLC1
                                Station5ForTransferScanboxidFromPLC1 = System.Text.Encoding.Default.GetString(temp);
                                if (Station5ForTransferScanboxidFromPLC1 != "\0\0\0\0\0\0\0\0\0\0")
                                {
                                    // transfer Data PLC1 DM142 to PLC2 DM5314
                                    ScanboxidSt4transpoter = Station5ForTransferScanboxidFromPLC1;
                                    string tmpstr = Station5ForTransferScanboxidFromPLC1;
                                    byte[] tmpbyte;
                                    tmpbyte = new byte[tmpstr.Length];
                                    tmpbyte = Encoding.ASCII.GetBytes(Station5ForTransferScanboxidFromPLC1);
                                    //  CheckstringClearForPLC1toPLC2(Station5OFFSET, Station5ForTransferScanboxidFromPLC1); //before update data in DM, clear Offset
                                    Array.Copy(tmpbyte, 0, PLCWriteCommand6, Station5OFFSET, tmpstr.Length); //DM5314 = 249 offset
                                                                                                             // Station5Log.Info("Station5 For Transfer  Scanboxid From PLC1 " + Station5ForTransferScanboxidFromPLC1);

                                    networkmain.linePack.Info("Station4  Transfer  Scanboxid to ST5(Transpoter) " + Station5ForTransferScanboxidFromPLC1);

                                }
                                else
                                {
                                    Station5ForTransferScanboxidFromPLC1 = "\0\0\0\0\0\0\0\0\0\0";
                                    CheckstringClearForPLC1toPLC2(Station5OFFSET, Station5ForTransferScanboxidFromPLC1); // update data in DM, clear Offset

                                    ScanboxidSt4transpoter = "";

                                }


                            }

                            catch (Exception ex)
                            {
                                Log1.Info(ex.ToString());
                            }
                            #endregion //commented out;

                            #region Station 3 and Station 5 Transfer and Complete Signal
                            byte[] tmparray3T = new byte[2];
                            //DM5104
                            Array.Copy(PLCQueryRx6, 219, tmparray3T, 0, 2);
                            String aT = System.Text.Encoding.Default.GetString(tmparray3T); //convert array to string  
                            Array.Copy(tmparray3T, 0, PLCWriteCommand, 481, 2); //D430
                            // 20 t0 5432 hand shaking
                            byte[] tmparray5T = new byte[2];
                            //DM20
                            Array.Copy(PLCQueryRx, 51, tmparray5T, 0, 2);
                            String cT = System.Text.Encoding.Default.GetString(tmparray5T);
                            Array.Copy(tmparray5T, 0, PLCWriteCommand6, 485, 2); //D5432

                            #endregion
                            //ST5&6
                            #region Total S5S6inOut and Constant
                            byte[] tmparrayC = new byte[2];
                            //DM5108 plc2
                            Array.Copy(PLCQueryRx6, 227, tmparrayC, 0, 2);
                            String ct = System.Text.Encoding.Default.GetString(tmparrayC); //convert array to string  
                            Array.Copy(tmparrayC, 0, PLCWriteCommand, 483, 2); //D431 plc1

                            byte[] tmparrayS5S6 = new byte[2];
                            //DM5109 PLC2
                            Array.Copy(PLCQueryRx6, 229, tmparrayS5S6, 0, 2);
                            String ctt = System.Text.Encoding.Default.GetString(tmparrayS5S6); //convert array to string  
                            Array.Copy(tmparrayS5S6, 0, PLCWriteCommand, 485, 2); //D432 plc1

                            byte[] S5startbit = new byte[2];
                            //DM5110 PLC2
                            Array.Copy(PLCQueryRx6, 231, S5startbit, 0, 2);
                            String cstart = System.Text.Encoding.Default.GetString(S5startbit); //convert array to string  
                            Array.Copy(S5startbit, 0, PLCWriteCommand, 487, 2); //D433 plc1
                            #endregion

                        }
                    #endregion
                    #region PLC 2

                    if (PLCTelnet2.connected)
                    {
                        int abc = 0, bcd = 0;
                        string jjjjj = "";
                        //string tmpstr100;
                        try
                        {

                            #region Event_PLC2
                            try
                            {
                                //string EventFomat = PLC1EventMsg('N', 1) + PLC1EventMsg('N', 2) + PLC1EventMsg('N', 3)+ PLC1EventMsg('N', 4) + ",";
                                //string[] chkDataArray3 = EventFomat.Split(',');
                                for (int h = 0; h < chkDataArray32.Length; h++)
                                {
                                    //FORMAT ==> "EVENT ID;ATT1 VAL;ATT1 SIZE;ATT1 VAL+ID;ATT1 SIZE;.......SO ON"
                                    Int16 CheckPlC = (Int16)(BitConverter.ToInt16(PLCQueryRxParaPlc2, 1009));
                                    if (CheckPlC != 999)
                                    {
                                        break;
                                    }
                                    string item = chkDataArray32[h];
                                    string EventEvMsg = "";
                                    string EventATTMsg = "";
                                    string EventATTVal = "";
                                    string[] tmpVal = item.Split(';');
                                    jjjjj = tmpVal[0];
                                    if (item == "FirstBlood")
                                    {
                                        FirstBlood2 = false;
                                        chkDataArray32 = chkDataArray32.Where(w => w != chkDataArray32[chkDataArray32.Length - 1]).ToArray();
                                        break;
                                    }
                                    int tmp0 = ((Int16.Parse(tmpVal[0]) - 6000) * 2) + 11;  //EVENT ID Calculation
                                    abc = tmp0;
                                    Int16 EventID = (Int16)(BitConverter.ToInt16(PLCQueryRxParaPlc2, tmp0)); //EventID Fetch
                                    bcd = EventID;
                                    if (EventID == 0)
                                    {
                                        EventMemPLC2[tmp0 / 2] = 0;
                                    }
                                    if (EventID == 5022 && EventID != EventMemPLC2[tmp0 / 2] && !FirstBlood2) //OEE BARCODE NG UPDATE REASON
                                    {
                                        //EventMemPLC1[tmp0 / 2] = EventID;
                                        string[] FLbatch = rq.UpdJamstatus(5, 55, 95);
                                        if (FLbatch.Length > 0)
                                        {
                                            foreach (string FL in FLbatch)
                                            {
                                                networkmain.Client_SendEventMessage("95", "ST5JAMCLEAR", "BOX_ID", FL);
                                                AllRJEvent.Info("95,ST5JAMCLEAR BOX_ID " + FL);
                                                // networkmain.Client_sendFG01_FG02_MOVE(FL, "FG01_FG02_MOVE,ST5AMCLEAR");
                                            }
                                        }
                                        else
                                        {
                                            AllRJEvent.Info("95, ST5JAMCLEAR, No need to do anything");
                                        }
                                    }
                                    else if (EventID == 5005 && EventID != EventMemPLC2[tmp0 / 2] && !FirstBlood2) //  Station5RobotBagPlaceOnQueuingConveyor       
                                    {
                                        //  <EV5005>Station5RobotBagPlaceOnQueuingConveyor</EV5005>  --ConfigEventcodeSource.xml  
                                        //   <AS5005>6175;10</AS5005>
                                        // DM6175-6000=75    2*75+11=361
                                        BackgroundWorker bw = new BackgroundWorker();
                                        bw.DoWork += new DoWorkEventHandler(EnterStation_DoWorks2);
                                        bw.RunWorkerAsync(new int[] { 6, 361 });

                                    }                                    
                                    else if (EventID == 6015 && EventID != EventMemPLC2[tmp0 / 2] && !FirstBlood2) //OEE BARCODE NG UPDATE REASON
                                    {
                                        //EventMemPLC1[tmp0 / 2] = EventID;
                                        string[] FLbatch = rq.UpdJamstatus(6, 66, 96);
                                        if (FLbatch.Length > 0)
                                        {
                                            foreach (string FL in FLbatch)
                                            {
                                                networkmain.Client_SendEventMessage("96", "ST6JAMCLEAR", "BOX_ID", FL);
                                                AllRJEvent.Info("96, ST6JAMCLEAR BOX_ID " + FL);
                                                // networkmain.Client_sendFG01_FG02_MOVE(FL, "FG01_FG02_MOVE,ST6AMCLEAR");
                                            }
                                        }
                                    }
                                    else if (EventID == 7020 && EventID != EventMemPLC2[tmp0 / 2] && !FirstBlood2) //without robot
                                    {
                                        string[] FLbatch = rq.UpdJamstatus(7, 77, 97);
                                        if (FLbatch.Length > 0)
                                        {
                                            foreach (string FL in FLbatch)
                                            {
                                                networkmain.Client_SendEventMessage("97", "ST7JAMCLEAR", "BOX_ID", FL);
                                                AllRJEvent.Info("97, ST7JAMCLEAR BOX_ID " + FL);
                                                // networkmain.Client_sendFG01_FG02_MOVE(FL, "FG01_FG02_MOVE,ST7AMCLEAR");
                                            }
                                        }
                                        else
                                        {
                                            AllRJEvent.Info("97, ST7JAMCLEAR, No need to do anything");
                                        }
                                    }
                                    else if (EventID == 7021 && EventID != EventMemPLC2[tmp0 / 2] && !FirstBlood2) //with robot
                                    {
                                        //EventMemPLC1[tmp0 / 2] = EventID;
                                        string[] FLbatch = rq.UpdJamstatus(7, 77, 97);
                                        if (FLbatch.Length > 0)
                                        {
                                            foreach (string FL in FLbatch)
                                            {
                                                networkmain.Client_SendEventMessage("97", "ST7JAMCLEAR", "BOX_ID", FL);
                                                AllRJEvent.Info("97, ST7JAMCLEAR BOX_ID " + FL);
                                                // networkmain.Client_sendFG01_FG02_MOVE(FL, "FG01_FG02_MOVE,ST7AMCLEAR");
                                            }
                                        }
                                        else
                                        {
                                            AllRJEvent.Info("97, ST7JAMCLEAR, No need to do anything");
                                        }
                                    }
                                    else if (EventID == 7010 && EventID != EventMemPLC2[tmp0 / 2] && !FirstBlood2) //OEE BARCODE NG UPDATE REASON
                                    {
                                        //  EV7010>BoxTransferredtoStation8</EV7010>  --ConfigEventcodeSource.xml  
                                        //   <AS5005>6175;10</AS5005>
                                        BackgroundWorker bw = new BackgroundWorker();
                                        bw.DoWork += new DoWorkEventHandler(delegate (object o, DoWorkEventArgs args)
                                        {
                                            //EventMemPLC1[tmp0 / 2] = EventID;
                                            int OEEID;
                                            string TLtoOEEID = "";
                                            //TLtoOEEID = System.Text.Encoding.Default.GetString(PLCQueryRxParaPlc2, 613, 8);
                                            //TLtoOEEID = TLtoOEEID.Replace("\0", string.Empty);
                                            //Tracking Lable 
                                            byte[] tmparrayTLOEEID = new byte[8];
                                            Array.Copy(PLCQueryRxParaPlc2, 613, tmparrayTLOEEID, 0, 8);

                                            byte tmp;
                                            for (int i = 0; i < tmparrayTLOEEID.Length; i = i + 2)
                                            {
                                                tmp = tmparrayTLOEEID[i];
                                                tmparrayTLOEEID[i] = tmparrayTLOEEID[i + 1];
                                                tmparrayTLOEEID[i + 1] = tmp;
                                            }

                                            TLtoOEEID = System.Text.Encoding.Default.GetString(tmparrayTLOEEID);

                                            bool GetFLok = GetFLByTL(TLtoOEEID, out OEEID);
                                            if (GetFLok && OEEID > 0)
                                            {
#if DEBUG
                                                if (!dST8TrackingLabel.ContainsKey(TLtoOEEID))
                                                {
                                                    dST8TrackingLabel.Add(TLtoOEEID, OEEID);
                                                }
                                                else
                                                {
                                                    EvtLog.Info("ST7 PLC Sending duiplicate tracking lable=" + TLtoOEEID);
                                                }
#endif
                                                rq.UpdStNobyID(8, OEEID);
                                                EvtLog.Info("ST8 Entered by OEEID = " + OEEID.ToString() + " TrackingLabe=" + TLtoOEEID);
                                            }
                                            else
                                            {
                                                EvtLog.Info("OEEID REQ FAIL for ST8" + " TrackingLabe=" + TLtoOEEID);
                                            }
                                        });
                                        bw.RunWorkerAsync();
                                    }
                                    else if (EventID == 8023 && EventID != EventMemPLC2[tmp0 / 2] && !FirstBlood2) //OEE BARCODE NG UPDATE REASON
                                    {
                                        //EventMemPLC1[tmp0 / 2] = EventID;
                                        string[] FLbatch = rq.UpdJamstatus(8, 88, 98);
                                        if (FLbatch.Length > 0)
                                        {
                                            foreach (string FL in FLbatch)
                                            {
                                                networkmain.Client_SendEventMessage("98", "ST8JAMCLEAR", "BOX_ID", FL);
                                                AllRJEvent.Info("98, ST8JAMCLEAR BOX_ID " + FL);
                                                // networkmain.Client_sendFG01_FG02_MOVE(FL, "FG01_FG02_MOVE,ST8AMCLEAR");
                                            }
                                        }
                                        else
                                        {
                                            AllRJEvent.Info("98, ST8JAMCLEAR, No need to do anything");
                                        }
                                    }
                                    if (EventID != 0 && EventID != EventMemPLC2[tmp0 / 2] && EventID != 5003 && EventID != 5029 && EventID != 5030) //If EVENTID change and not equal zero
                                    {
                                        EventMemPLC2[tmp0 / 2] = EventID; //Update the changed

                                        String EventID3Digit = EventATTVal = PLC1EventMsg('V', EventID);
                                        EventEvMsg = EventID3Digit + ";" + PLC1EventMsg('E', EventID);
                                        // EventEvMsg = EventID.ToString() + ";" + PLC1EventMsg('E', EventID) + ";";

                                        string tmpData1 = PLC1EventMsg('S', EventID);

                                        int havcoor = tmpData1.IndexOf(";");
                                        if (havcoor < 0)
                                        {
                                            int AttSize = Int16.Parse(tmpData1);
                                            for (int k = 0; k < AttSize; k++)
                                            {
                                                int i = (k * 2) + 1;
                                                if (tmpVal[i] != "0")
                                                {
                                                    int tmp2 = ((Int16.Parse(tmpVal[i]) - 6000) * 2) + 11; //ATT VAL (eg: AT = 1001 & EVENTID = 3008)
                                                                                                           //ATT ID = (13008)                                        

                                                    //ATT Value = (130081)
                                                    int tmp3 = Int16.Parse(tmpVal[i + 1]); //ATT Size
                                                    if (tmp3 == 10) //is Finishing Label
                                                    {
                                                        EventATTVal = System.Text.Encoding.Default.GetString(PLCQueryRxParaPlc2, tmp2, tmp3); //Fetch Finishing label direct from PLC
                                                        EventATTMsg = EventATTMsg + ";" + "LotNumber" + ";" + EventATTVal;   //FORMAT (ATT1 Mesg; ATT1 Val;ATT2 Mesg; ATT2 Val
                                                    }
                                                    else if (tmp3 == 12) //SealerID
                                                    {
                                                        EventATTVal = System.Text.Encoding.Default.GetString(PLCQueryRxParaPlc2, tmp2, tmp3); //Fetch Finishing label direct from PLC
                                                        EventATTMsg = EventATTMsg + ";" + "SealerID" + ";" + EventATTVal;   //FORMAT (ATT1 Mesg; ATT1 Val;ATT2 Mesg; ATT2 Val
                                                    }
                                                    else if (tmp3 == 8) //LicensePlateBarcode
                                                    {
                                                        EventATTVal = System.Text.Encoding.Default.GetString(PLCQueryRxParaPlc2, tmp2, tmp3); //Fetch Finishing label direct from PLC
                                                                                                                                              //EventATTVal = EventATTVal.Substring(0,7);
                                                        EventATTMsg = EventATTMsg + ";" + "LicensePlateBarcode" + ";" + EventATTVal;   //FORMAT (ATT1 Mesg; ATT1 Val;ATT2 Mesg; ATT2 Val
                                                    }
                                                    else if (tmp3 == 1)//is Other
                                                    {

                                                        int AttrVal = Convert.ToInt16(PLCQueryRxParaPlc2[tmp2]);
                                                        string AttStr = i.ToString() + EventID.ToString();
                                                        EventATTVal = PLC1EventMsg('A', Int16.Parse(AttStr)); //Fetch Attribute Val from Xml (e.g: 130081)
                                                        EventATTMsg = EventATTMsg + ";" + EventATTVal + ";" + AttrVal;
                                                    }
                                                    else if (tmp3 == 2)//is Other
                                                    {

                                                        Int32 AttrVal = (Int32)BitConverter.ToInt32(PLCQueryRxParaPlc2, tmp2);
                                                        string AttStr = ((i + 1) / 2).ToString() + EventID.ToString();
                                                        EventATTVal = PLC1EventMsg('A', Int32.Parse(AttStr)); //Fetch Attribute Val from Xml (e.g: 130081)
                                                        EventATTMsg = EventATTMsg + ";" + EventATTVal + ";" + AttrVal;
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            string[] tmpArray1 = tmpData1.Split(',');
                                            for (int i = 0; i < tmpArray1.Length; i++)
                                            {
                                                string[] tempDataArray = tmpArray1[i].Split(';');
                                                if (tempDataArray[0] != "0")
                                                {
                                                    int dmValue = Int16.Parse(tempDataArray[0]);
                                                    int dmSize = Int16.Parse(tempDataArray[1]);
                                                    int tmp2 = (dmValue - 6000) * 2 + 11; //ATT VAL (eg: AT = 1001 & EVENTID = 3008)
                                                                                          //ATT ID = (13008)                                        

                                                    //ATT Value = (130081)

                                                    if (dmSize == 10) //is Finishing Label
                                                    {
                                                        EventATTVal = System.Text.Encoding.Default.GetString(PLCQueryRxParaPlc2, tmp2, dmSize); //Fetch Finishing label direct from PLC
                                                        EventATTMsg = EventATTMsg + ";" + "LotNumber" + ";" + EventATTVal;   //FORMAT (ATT1 Mesg; ATT1 Val;ATT2 Mesg; ATT2 Val
                                                    }
                                                    else if (dmSize == 12) //is Finishing Label
                                                    {
                                                        EventATTVal = System.Text.Encoding.Default.GetString(PLCQueryRxParaPlc2, tmp2, dmSize); //Fetch Finishing label direct from PLC
                                                        EventATTMsg = EventATTMsg + ";" + "SealerID" + ";" + EventATTVal;   //FORMAT (ATT1 Mesg; ATT1 Val;ATT2 Mesg; ATT2 Val
                                                    }
                                                    else if (dmSize == 1)//is Other
                                                    {

                                                        int AttrVal = Convert.ToInt16(PLCQueryRxParaPlc2[tmp2]);
                                                        string AttStr = (i + 1).ToString() + EventID.ToString();
                                                        EventATTVal = PLC1EventMsg('A', Int16.Parse(AttStr)); //Fetch Attribute Val from Xml (e.g: 130081)
                                                        EventATTMsg = EventATTMsg + ";" + EventATTVal + ";" + AttrVal;
                                                    }
                                                    else if (dmSize == 2)//is Other
                                                    {

                                                        Int32 AttrVal = (Int32)BitConverter.ToInt32(PLCQueryRxParaPlc2, tmp2);
                                                        string AttStr = (i + 1).ToString() + EventID.ToString();
                                                        EventATTVal = PLC1EventMsg('A', Int32.Parse(AttStr)); //Fetch Attribute Val from Xml (e.g: 130081)
                                                        EventATTMsg = EventATTMsg + ";" + EventATTVal + ";" + AttrVal;
                                                    }
                                                }
                                            }
                                        }
                                        if (EventATTMsg != "")
                                        {
                                            EventEvMsg = EventEvMsg + EventATTMsg; //Message for 1 event done, push it to stack
                                        }

                                        if (!FirstBlood2)
                                        {
                                            MyEventQ.AddQ(EventEvMsg);//Push message to stack
                                                                      //EvtLog.Info("Push IN2:  " + EventEvMsg);
                                        }

                                    }

                                }
                            }
                            catch (Exception ex)
                            {
                                EvtLog.Info("ERROR2:" + abc.ToString() + " " + bcd.ToString() + " " + jjjjj + " " + ex.ToString());
                            }
                            #endregion


                            #region check Station 5 Sealer at Initialization time


                            try
                            {

                                if (PLCQueryRx6[PLCQueryRx_DM5159] == 0x01 && PLCWriteCommand6[PLCWriteCommand_DM5362] == 0x00)
                                {
                                    PLCWriteCommand6[PLCWriteCommand_DM5362] = 0x04;
                                    evnt_CheckingConnectionForSealer1.Set();
                                    // Station5Log.Info("Sealer1 Initialization");
                                    networkmain.linePack.Info("Sealer1 Initialization");
                                }
                                if (PLCQueryRx6[PLCQueryRx_DM5159] == 0x00)
                                {
                                    PLCWriteCommand6[PLCWriteCommand_DM5362] = 0x00;
                                }

                                if (PLCQueryRx6[PLCQueryRx_DM5172] == 0x01 && PLCWriteCommand6[PLCWriteCommand_DM5363] == 0x00)
                                {
                                    PLCWriteCommand6[PLCWriteCommand_DM5363] = 0x04;
                                    evnt_CheckingConnectionForSealer2.Set();
                                    // Station5Log.Info("Sealer2 Initialization");

                                    networkmain.linePack.Info("Sealer2 Initialization");
                                }
                                if (PLCQueryRx6[PLCQueryRx_DM5172] == 0x00)
                                {
                                    PLCWriteCommand6[PLCWriteCommand_DM5363] = 0x00;
                                }


                                if (PLCQueryRx6[PLCQueryRx_DM5173] == 0x01 && PLCWriteCommand6[PLCWriteCommand_DM5426] == 0x00)
                                {
                                    PLCWriteCommand6[PLCWriteCommand_DM5426] = 0x04;
                                    evnt_CheckingConnectionForSealer3.Set();
                                    // Station5Log.Info("Sealer3 Initialization");
                                    networkmain.linePack.Info("Sealer3 Initialization");
                                }
                                if (PLCQueryRx6[PLCQueryRx_DM5173] == 0x00)
                                {
                                    PLCWriteCommand6[PLCWriteCommand_DM5426] = 0x00;
                                }

                            }

                            catch (Exception ex)
                            {
                                Log1.Info(ex.ToString());
                            }





                            #endregion

                            #region St6 OK/RJ request

                            byte[] bcarrayst5check = new byte[10];
                            Array.Copy(PLCQueryRx6, 121, bcarrayst5check, 0, 10);  //D5055
                            St6CheckFL = System.Text.Encoding.Default.GetString(bcarrayst5check);
                            if (St6CheckFL != "\0\0\0\0\0\0\0\0\0\0")
                            {
                                ST6NewFLRev = true;
                                PLCWriteCommand6[483] = 0x04;
                                networkmain.linePack.Info("Station 6 Request OK/RJ at Conveyor Side" + St6CheckFL);

                                CheckStringUpdateForST6Check(St6CheckFL);
                            }

                            else
                            {
                                ST6NewFLRev = false;
                                St6CheckFL = "\0\0\0\0\0\0\0\0\0\0";
                                CheckstringClearFor6Check(481, St6CheckFL);
                                PLCWriteCommand6[483] = 0x00;
                                networkmain.linePack.Info("Station 6 Request OK/RJ at Conveyor Side Reset " + St6CheckFL);



                            }


                            #endregion



                            #region   Station 5 and Station 6 Boundary Area For Reject


                            try
                            {


                                //Tracking Label 
                                byte[] temp83 = new byte[10];
                                Array.Copy(PLCQueryRx6, 331, temp83, 0, 10); // DM 5160 ~ DM 5164 PLC1
                                Station6FinishLabelForReject = System.Text.Encoding.Default.GetString(temp83);
                                if (Station6FinishLabelForReject != "\0\0\0\0\0\0\0\0\0\0")
                                {




                                    #region RJ status update
                                    //need lock 
                                    //DM5143=297
                                    byte[] tmparrayER7 = new byte[2];
                                    Array.Copy(PLCQueryRx6, 297, tmparrayER7, 0, 2);
                                    //convert Byte array to int                 
                                    Int32 er7 = (Int32)(BitConverter.ToInt16(tmparrayER7, 0));
                                    if (er7 > 0)
                                    {
                                        // Station6RejectboundaryLog.Info("Station5 and 6 boundary Reject code" + er7);
                                        networkmain.linePack.Info("Reject code at Station 6 Reject Lane " + er7);

                                        string rj = "RJ";
                                        try
                                        {
                                            while ((!networkmain.UpdateRJLabel(Station6FinishLabelForReject, rj, er7.ToString()) && !bTerminate))
                                            {
                                                Thread.Sleep(100);
                                            }
                                        }
                                        catch
                                        {

                                        }

                                    }
                                    #endregion



                                    string OEEid6 = "0";
                                    try
                                    {

                                        XmlNode fltrackingroot = networkmain.FLTrackingdoc.DocumentElement;//this house the list of summarized xml details 
                                        XmlNode selectednode = fltrackingroot.SelectSingleNode("descendant::LABEL[ID='" + Station6FinishLabelForReject + "']");

                                        ERCodest6 = selectednode.SelectSingleNode("ErrorCode").InnerText;
                                        RJReasonForSealer = selectednode.SelectSingleNode("SealerResultReason").InnerText;
                                        OEEid6 = selectednode.SelectSingleNode("OEEid").InnerText;

                                    }
                                    catch (Exception ex)
                                    {
                                        Log1.Info(ex.ToString());
                                    }


                                    try
                                    {
                                        string RJCODE = ERCodest6;
                                        int rj = int.Parse(RJCODE);
                                        if (rj > 0 && rj != 994 && FLMembkp6 != Station6FinishLabelForReject)
                                        {
                                            XmlDocument doc = new XmlDocument();
                                            doc.Load(@"ConfigEvent.xml");
                                            XmlNode node = doc.SelectSingleNode(@"/EVENT/R" + RJCODE);
                                            string RJName = node.InnerText;


                                            XmlNode node6 = doc.SelectSingleNode(@"/EVENT/RK" + RJCODE);
                                            string NewRjCod6 = node6.InnerText;

                                            //networkmain.Client_SendEventMessagest6(RJCODE, RJName,"BOX_ID",Station6FinishLabelForReject);
                                            networkmain.Client_SendEventMessagest6(NewRjCod6, RJName, "BOX_ID", Station6FinishLabelForReject);
                                            //  Station6RejectboundaryLog.Info("Send Reject Event to MiddleWare " + Station6FinishLabelForReject+","+RJCODE+","+RJName); 
                                            #region OEEStep3

                                            try //OEE OPERATOR 1 MANUA REJECT
                                            {
                                                rq.UpdST6RJ(OEEid6, 6, RJCODE);
                                            }
                                            catch (Exception ex)
                                            {
                                                IGTOEELog.Info(ex.ToString());
                                            }

                                            #endregion
                                            AllRJEvent.Info("Send Reject Event to MiddleWare at ST6 Rejct Lane " + Station6FinishLabelForReject + "," + NewRjCod6 + "," + RJName);
                                            FLMembkp6 = Station6FinishLabelForReject;

                                        }




                                        if (rj > 0 && rj == 994 && FLMembkp6 != Station6FinishLabelForReject)
                                        {

                                            networkmain.Client_SendEventMessagest6(RJCODE, RJReasonForSealer, "BOX_ID", Station6FinishLabelForReject);
                                            //  Station6RejectboundaryLog.Info("Send Reject Event to MiddleWare " + Station6FinishLabelForReject+","+RJCODE+","+RJName); 
                                            try //OEE OPERATOR 1 MANUA REJECT
                                            {
                                                rq.UpdST6RJ(OEEid6, 6, RJCODE);
                                            }
                                            catch (Exception ex)
                                            {
                                                IGTOEELog.Info(ex.ToString());
                                            }

                                            AllRJEvent.Info("Send Reject Event to MiddleWare at ST6 Rejct Lane " + Station6FinishLabelForReject + "," + RJCODE + "," + RJReasonForSealer);
                                            FLMembkp6 = Station6FinishLabelForReject;

                                        }












                                    }
                                    catch (Exception ex)
                                    {

                                    }


                                    networkmain.Client_sendFG01_FG02_MOVE(Station6FinishLabelForReject, "ST6 Rejct Lane");
                                    // Station6RejectboundaryLog.Info("Station5 and 6 boundary Reject " + Station6FinishLabelForReject);
                                    networkmain.linePack.Info("Reject  at Station 6 Reject Lane " + Station6FinishLabelForReject);
                                    PLCWriteCommand6[PLCWriteCommand_DM5416] = 0x09;

                                }
                                else
                                {
                                    Station6TrackLabelForReject = "\0\0\0\0\0\0\0\0\0\0";
                                    PLCWriteCommand6[PLCWriteCommand_DM5416] = 0x00;
                                }
                            }
                            catch (Exception ex)
                            {
                                Log1.Info(ex.ToString());
                            }

                            #endregion

                            #region  Station6 Error Code

                            try
                            {
                                #region   For Operator 1
                                //D5141=293
                                byte[] tmparrayER = new byte[2];
                                Array.Copy(PLCQueryRx6, 293, tmparrayER, 0, 2);
                                Int32 er6 = (Int32)(BitConverter.ToInt16(tmparrayER, 0)); //convert Byte array to int





                                Operator1ErrCode = er6.ToString();
                                #region NewerrorCode
                                if (er6 > 0)
                                {
                                    // LogEr.Info("Station 6 QC1 Error Code"+Operator1ErrCode);
                                    Operator1Errmessage = "Stn.6 QC1 Err " + Operator1ErrCode + ": " + Stn6ErrToMsg(er6) + ", " + UserName1;

                                    if (!ST6JamFlag)
                                    {
                                        bool St6JamTrig = ST2PauseFunction(6, er6 + ";"); //Check if is a JAM
                                        if (St6JamTrig)
                                        {
                                            ST6JamFlag = true;
                                            //string[] FLbatch = rq.UpdJamstatus(6, 666); //Update Jam FL
                                            //if (FLbatch != null)
                                            //{
                                            //    networkmain.Client_SendEventMsg("653", "Station6FLJAMRecovery", FLbatch);//Update Jam recovery FL to middleware
                                            //}
                                        }
                                    }



                                }
                                else
                                {
                                    ST6JamFlag = false;
                                    Operator1Errmessage = String.Empty;
                                }
                                #endregion
                                #region OldCode
                                //if (er6 > 0)
                                //{
                                //    // LogEr.Info("Station 6 QC1 Error Code"+Operator1ErrCode);
                                //    Operator1Errmessage = "Stn.6 QC1 Err " + Operator1ErrCode + ": " + Stn6ErrToMsg(er6) + ", " + UserName1;


                                //    // LogEr.Info(Operator1Errmessage);


                                //}
                                //else
                                //{
                                //    Operator1Errmessage = String.Empty;
                                //}
                                #endregion
                                UpdateErrorMsg((int)Station.StationNumber.Station06Operator1, Operator1Errmessage, ST6JamFlag);


                                if (er6 > 0 && networkmain.controlst6 == 0)
                                {
                                    Errst6 = er6.ToString();
                                    networkmain.controlst6 = 1;
                                    messagest6 = Stn6ErrToMsg(er6);
                                    if (er6 != 6611 && er6 != 6616)
                                    {
                                        networkmain.Client_SendAlarmMessage(er6.ToString(), messagest6, "SET");
                                    }

                                    LogEr.Info("Error Set st6 QC1 " + er6.ToString() + messagest6);

                                }
                                if (er6 == 0 && networkmain.controlst6 == 1)
                                {
                                    if (er6 != 6611 && er6 != 6616)
                                    {
                                        networkmain.Client_SendAlarmMessage(Errst6, messagest6, "CLEAR");
                                    }

                                    LogEr.Info("Error Clear st6 QC1 " + Errst6 + messagest6);
                                    networkmain.controlst6 = 0;
                                    Errst6 = "";
                                    messagest6 = "";
                                }

                                #endregion
                                #region    For Operator2

                                //D5142=295     
                                byte[] tmparrayER_ = new byte[2];
                                Array.Copy(PLCQueryRx6, 295, tmparrayER_, 0, 2);
                                //convert Byte array to int                 
                                Int32 er6_1 = (Int32)(BitConverter.ToInt16(tmparrayER_, 0));



                                Operator2ErrCode = er6_1.ToString();


                                if (er6_1 > 0)
                                {
                                    // LogEr.Info("Station 6 QC2 Error Code"+Operator2ErrCode);
                                    Operator2Errmessage = "Stn.6 QC2 Err " + Operator2ErrCode + ": " + Stn6ErrToMsg(er6_1) + ", " + UserName2;

                                    if (!ST6_1JamFlag)
                                    {
                                        bool St6JamTrig = ST2PauseFunction(6, er6_1 + ";"); //Check if is a JAM
                                                                                            //if (St6JamTrig)
                                                                                            //{
                                        ST6_1JamFlag = true;
                                        //string[] FLbatch = rq.UpdJamstatus(6, 666); //Update Jam FL
                                        //if (FLbatch != null)
                                        //{
                                        //    networkmain.Client_SendEventMsg("653", "Station6FLJAMRecovery", FLbatch);//Update Jam recovery FL to middleware
                                        //}
                                    }
                                }
                                else
                                {
                                    ST6_1JamFlag = false;
                                    Operator2Errmessage = String.Empty;
                                }

                                UpdateErrorMsg((int)Station.StationNumber.Station06Operator2, Operator2Errmessage, ST6_1JamFlag);

                                if (er6_1 > 0 && networkmain.controlst6_1 == 0)
                                {
                                    Errst6_1 = er6_1.ToString();
                                    networkmain.controlst6_1 = 1;
                                    messagest6_1 = Stn6ErrToMsg(er6_1);
                                    networkmain.Client_SendAlarmMessage(er6_1.ToString(), messagest6_1, "SET");
                                    LogEr.Info("Error Set st6 QC2 " + er6_1.ToString() + messagest6_1);


                                }
                                if (er6_1 == 0 && networkmain.controlst6_1 == 1)
                                {
                                    networkmain.Client_SendAlarmMessage(Errst6_1, messagest6_1, "CLEAR");
                                    networkmain.controlst6_1 = 0;
                                    LogEr.Info("Error Clear st6 QC2 " + Errst6_1 + messagest6_1);
                                    Errst6_1 = "";
                                    messagest6_1 = "";
                                }

                                #endregion


                            }


                            catch (Exception ex)
                            {
                                Log1.Info(ex.ToString());
                            }
                            #endregion


                        }
                        catch (Exception ex)
                        {
                            Log1.Error(ex.ToString());
                        }
                    }
                    #endregion
                    //Start of Middleware Network Connectivity Check**********************************************
                    //End of Network Connectivity Check***************************************************
                    //******************Message processing for network************************************
                    if (networkmain.MiddlewareToIGTEvt_Param.WaitOne(100))
                    {
                        //process incoming messages
                        XmlNode list1 = networkmain.PARAM_ScreenAccess.SelectSingleNode("MESSAGE/BODY/PARAMETER_CHANGE");
                        XmlNode list2 = networkmain.PARAM_ScreenAccess.SelectSingleNode("MESSAGE/BODY/ACCESS_LEVEL");
                        XmlNode list3 = networkmain.PARAM_ScreenAccess.SelectSingleNode("MESSAGE/BODY/TERMINAL_ID");
                        XmlNode list4 = networkmain.PARAM_ScreenAccess.SelectSingleNode("MESSAGE/BODY/TOKEN");
                        //Set PLC to User Login/Logout for maintainance mode
                        if (list3.InnerText == "1" && PLCWriteCommand[459] == 0x00)//419
                        {

                            PLCWriteCommand[459] = 0x08;
                            string tmpstr;
                            byte[] tmpbyte;
                            tmpstr = list4.InnerText;
                            int token = int.Parse(tmpstr);
                            networkmain.Token1 = token;
                            tmpbyte = new byte[tmpstr.Length];
                            tmpbyte = BitConverter.GetBytes(token);
                            Array.Copy(tmpbyte, 0, PLCWriteCommand, 473, 2);//D426
                            parameter.Info("Parameter Change at PLC1 Login " + token);
                            list1.InnerText = "";
                            list2.InnerText = "";
                            list3.InnerText = "";
                            list4.InnerText = "";

                        }

                        if (list3.InnerText == "2" && PLCWriteCommand6[315] == 0x00)//D5347
                        {
                            PLCWriteCommand6[315] = 0x08;


                            string tmpstr;
                            byte[] tmpbyte;
                            tmpstr = list4.InnerText;
                            int token = int.Parse(tmpstr);
                            networkmain.Token2 = token;
                            tmpbyte = new byte[tmpstr.Length];
                            tmpbyte = BitConverter.GetBytes(token);
                            Array.Copy(tmpbyte, 0, PLCWriteCommand6, 469, 2);//D5424
                            parameter.Info("Parameter Change at PLC2 Login " + token);
                            list1.InnerText = "";
                            list2.InnerText = "";
                            list3.InnerText = "";
                            list4.InnerText = "";



                        }





                        if (list3.InnerText == "Server")//D5347
                        {

                            string tmpstr;
                            tmpstr = list4.InnerText;
                            int token = int.Parse(tmpstr);
                            networkmain.Token3 = token;
                            parameter.Info("Parameter Change at Server Login " + token);
                            list1.InnerText = "";
                            list2.InnerText = "";
                            list3.InnerText = "";
                            list4.InnerText = "";



                        }

                    }







                    if (networkmain.MiddlewareToIGTEvt_Sealer.WaitOne(100))
                    {
                        //process incoming messages
                        XmlNode list1 = networkmain.VacummResult.SelectSingleNode("MESSAGE/BODY/MESSAGE_TYPE");
                        XmlNode list2 = networkmain.VacummResult.SelectSingleNode("MESSAGE/BODY/BOX_NUMBER");
                        XmlNode list3 = networkmain.VacummResult.SelectSingleNode("MESSAGE/BODY/CHECK_RESULT");

                        if (list3.InnerText == "FAIL")
                        {

                            XmlNode list4 = networkmain.VacummResult.SelectSingleNode("MESSAGE/BODY/REASON");
                            networkmain.UpdateLabelForSealerResult(list2.InnerText, list3.InnerText, list4.InnerText);
                            networkmain.SealerResult.Info("SealerResult" + list2.InnerText + "," + list3.InnerText + "," + list4.InnerText);
                            list1.InnerText = "";
                            list2.InnerText = "";
                            list3.InnerText = "";
                            list4.InnerText = "";
                        }

                        if (list3.InnerText == "PASS")
                        {
                            networkmain.UpdateLabelForSealerResult(list2.InnerText, list3.InnerText, "NA");
                            networkmain.SealerResult.Info("SealerResult" + list2.InnerText + "," + list3.InnerText + "," + "NA");
                            list1.InnerText = "";
                            list2.InnerText = "";
                            list3.InnerText = "";

                        }






                    }








                    if (networkmain.MiddlewareToIGTEvt_Remote.WaitOne(100))
                    {
                        //process incoming messages
                        //XmlNodeList list = networkmain.REMOTE_Cmd.SelectNodes("MESSAGE/BODY/COMMAND");
                        //check for plurge commands.. if plurge set PLC to plurge


                        //process incoming messages
                        XmlNode list = networkmain.REMOTE_Cmd.SelectSingleNode("MESSAGE/BODY/COMMAND");
                        string CommandName = list.InnerText;
                        //check for plurge commands.. if plurge set PLC to plurge
                        if (CommandName == "PURGE" && PLCWriteCommand[457] == 0x00 && PLCWriteCommand[455] == 0x00 && PLCWriteCommand[451] == 0x00 && PLCWriteCommand[453] == 0x00 && PLCWriteCommand6[457] == 0x00 && PLCWriteCommand6[339] == 0x00 && PLCWriteCommand6[341] == 0x00 && PLCWriteCommand6[343] == 0x00)
                        {
                            PurgePau.Info("Middleware PURGE  to Machine");
                            // ClearDataForPURGE();


                            //COMENT TEMP


                            PLCWriteCommand[451] = 0x08;//D415
                            PLCWriteCommand[453] = 0x08;//D416
                            PLCWriteCommand[455] = 0x08;//D417
                            PLCWriteCommand[457] = 0x08;//D418
                            PLCWriteCommand6[457] = 0x08;//D5418
                            PLCWriteCommand6[339] = 0x08;//D5359
                            PLCWriteCommand6[341] = 0x08;//D5360
                            PLCWriteCommand6[343] = 0x08;//D5361     
                                                         //



                        }
                        if (CommandName == "PAUSE" && PLCWriteCommand[451] == 0x00)
                        {
                            PurgePau.Info("Middleware PAUSE to Machine");

                            //COMENT TEMP                         
                            PLCWriteCommand[451] = 0x03;//D415


                        }
                        networkmain.MiddlewareToIGTEvt_Remote.Reset();


                    }
                    if (evnt_RejectFinishingLabelForStation4.WaitOne(0))
                    {
                        //   networkmain.Client_sendReject_MOVE(ScanboxidSt4);



                        networkmain.Client_sendFG01_FG02_MOVE(ScanboxidSt4, " Station 4 Reject Finishing Label");
                        networkmain.stn4log = ScanboxidSt4 + " Rejected";
                        evnt_RejectFinishingLabelForStation4.Reset();
                    }
                    if (evnt_RejectFinishingLabelForStation8.WaitOne(0))
                    {
                        // networkmain.Client_sendReject_MOVE(Station8ForTransferScanboxid);
                        networkmain.Client_sendFG01_FG02_MOVE(st8FINISH, "Station 8 Reject Finishing Label");

                        networkmain.stn8log = st8FLabel + " Rejected";
                        evnt_RejectFinishingLabelForStation8.Reset();
                        evt_FG01_FG02Move_Rx.Set();
                    }
                    if (evnt_RejectFinishingLabelForStation6_OP1.WaitOne(0))
                    {
                        // networkmain.Client_sendReject_MOVE(Station6ForOP1Scanboxid);


                        try
                        {
                            string RJCODE = "603";
                            XmlDocument doc = new XmlDocument();
                            doc.Load(@"ConfigEvent.xml");
                            XmlNode node = doc.SelectSingleNode(@"/EVENT/R" + RJCODE);
                            string RJName = node.InnerText;

                            networkmain.Client_SendEventMessage("57", RJName, "BOX_ID", Station6ForOP1Scanboxid);
                            // log6.Info("QC1 Operator Reject Event Send to Middleware " +"603,"+RJName+","+Station6ForOP1Scanboxid);
                            networkmain.linePack.Info("QC1 Operator Reject Event Send to Middleware " + "603," + RJName + "," + Station6ForOP1Scanboxid);
                            MyEventQ.AddQ("21;Station6SealedMBBRejectedAtStation6;LotNumber;" + Station6ForOP1Scanboxid + ";QcstationNumber;1;OpearatorName"
                          + UserName1);
                        }
                        catch (Exception ex)
                        {

                            networkmain.linePack.Info(ex);

                        }

                        networkmain.Client_sendFG01_FG02_MOVE(Station6ForOP1Scanboxid, "Station 6 Operator 1 Reject Finishing Label");
                        // networkmain.stn6log = Station6ForOP1Scanboxid + " OP1 Rejected Message to Middleware";

                        evnt_RejectFinishingLabelForStation6_OP1.Reset();
                    }
                    if (evnt_RejectFinishingLabelForStation6_OP2.WaitOne(0))
                    {
                        //  networkmain.Client_sendReject_MOVE(Station6ForOP2Scanboxid);

                        try
                        {
                            string RJCODE = "604";
                            XmlDocument doc = new XmlDocument();
                            doc.Load(@"ConfigEvent.xml");
                            XmlNode node = doc.SelectSingleNode(@"/EVENT/R" + RJCODE);
                            string RJName = node.InnerText;

                            networkmain.Client_SendEventMessage("58", RJName, "BOX_ID", Station6ForOP2Scanboxid);
                            //log6_1.Info("QC2 Operator Reject Event Send to Middleware " +"604,"+RJName+","+Station6ForOP2Scanboxid);
                            networkmain.linePack.Info("QC2 Operator Reject Event Send to Middleware " + "604," + RJName + "," + Station6ForOP2Scanboxid);
                            MyEventQ.AddQ("21;Station6SealedMBBRejectedAtStation6;LotNumber;" + Station6ForOP1Scanboxid + ";QcstationNumber;1;OpearatorName"
                          + UserName1);

                        }
                        catch (Exception ex)
                        {

                            networkmain.linePack.Info(ex);

                        }


                        networkmain.Client_sendFG01_FG02_MOVE(Station6ForOP2Scanboxid, "Station 6 Operator 2 Reject Finishing Label");
                        //  networkmain.stn6log = Station6ForOP1Scanboxid + " OP2 Rejected Message to Middleware";
                        evnt_RejectFinishingLabelForStation6_OP2.Reset();
                    }
                    if (networkmain.ServerReplyEvt.WaitOne(0))
                    {
                        #region ServerReply
                        //A Finishing label message is recieved
                        //check if finishing label reside on any of the buffer zone
                        //if do not appear in any of the buffer zone.. send error to PLC?...
                        //if appear at buffer zone.. set PLC flag that SAP data recieved, printing ready
                        //check if finishing label is correct
                        //for demo, 
                        //print file to printer 1
                        //print file to printer 2
                        //root1 = FinishingLabelsInfo.DocumentElement;
                        //book = root1.SelectSingleNode("descendant::bk:MESSAGE[bk:BODY/bk:BOX_NUMBER='1212222.45']", nsmgr);
                        //push into list
                        // ReceiveFL = Scanboxid;
                        byte[] statusbyte = new byte[2];
                        statusbyte[0] = PLCWriteCommand[95];
                        statusbyte[1] = PLCWriteCommand[96];
                        string strStatus = System.Text.Encoding.Default.GetString(statusbyte);
                        //Address block D0210 to D0240 == PLCWriteCommand[41] to PLCWriteCommand[101]
                        if (ST02Rotatary_A_Str == "\0\0\0\0\0\0\0\0\0\0")
                        {
                            //set data available = none
                            PLCWriteCommand[95] = PLCWriteCommand[96] = 0;
                        }
                        else
                        {
                            try
                            {
                                while (!networkmain.UpdateRotaryABC_FinishingLabel(ST02Rotatary_A_Str, "RA"))//assume PLC data moved before server reply .............Need to change here ****
                                {
                                    Thread.Sleep(10);
                                }
                                if (((strStatus == "NA") || ((PLCWriteCommand[95] == 0) && (PLCWriteCommand[96] == 0))))
                                {
                                    byte[] plcbuffer;
                                    UpdatePLCFinishingLabelDMAddress(out plcbuffer, _ST02Rotatary_A_Str);
                                    Array.Copy(plcbuffer, 0, PLCWriteCommand, RA_PCtoPLCFinishingLabelOFFSET, 58);//PLCWRITE 41 is DM210
                                                                                                                  // St2Log.Info("Rotary A Give Infomation to PLC " + _ST02Rotatary_A_Str);
                                    networkmain.linePack.Info("Rotary A Give Infomation to PLC " + _ST02Rotatary_A_Str);


                                    ReceiveFL = _ST02Rotatary_A_Str;

                                }//data available
                                else
                                {
                                    //data not found..set data not available to PLC
                                    byte[] tmpbyte = new byte[2];
                                    tmpbyte = Encoding.ASCII.GetBytes("ER");
                                    //   St2Log.Info("Rotary A Give Infomation to PLC Fail ,System Don't have that Label " + _ST02Rotatary_A_Str);
                                    Array.Copy(tmpbyte, 0, PLCWriteCommand, 95, 2);
                                }//data not available
                            }
                            catch (Exception ex)
                            {
                                byte[] tmpbyte = new byte[2];
                                tmpbyte = Encoding.ASCII.GetBytes("ER");
                                Array.Copy(tmpbyte, 0, PLCWriteCommand, 95, 2);
                                //  St2Log.Info("Rotary A Give Infomation to PLC Fail ,System Don't have that Label " + _ST02Rotatary_A_Str);
                            }
                        }
                        statusbyte = new byte[2];
                        statusbyte[0] = PLCWriteCommand[155];//DM267
                        statusbyte[1] = PLCWriteCommand[156];//DM267
                        strStatus = System.Text.Encoding.Default.GetString(statusbyte);
                        //Address block D0241 to D0270 == PLCWriteCommand[41] to PLCWriteCommand[101]
                        if (ST02Rotatary_B_Str == "\0\0\0\0\0\0\0\0\0\0")
                        {
                            //set data available = none
                            PLCWriteCommand[155] = PLCWriteCommand[156] = 0;
                        }
                        else
                        {//search for data in data list
                            try
                            {
                                while (!networkmain.UpdateRotaryABC_FinishingLabel(ST02Rotatary_B_Str, "RB"))//assume PLC data moved before server reply
                                {
                                    Thread.Sleep(10);
                                }
                                if ((strStatus == "NA") || ((PLCWriteCommand[155] == 0) && (PLCWriteCommand[156] == 0)))
                                {
                                    byte[] plcbuffer;
                                    UpdatePLCFinishingLabelDMAddress(out plcbuffer, ST02Rotatary_B_Str);
                                    Array.Copy(plcbuffer, 0, PLCWriteCommand, RB_PCtoPLCFinishingLabelOFFSET, 58);
                                    //St2Log.Info("Rotary B Give Infomation to PLC" + _ST02Rotatary_B_Str);

                                    networkmain.linePack.Info("Rotary B Give Infomation to PLC" + _ST02Rotatary_B_Str);
                                    ReceiveFL1 = _ST02Rotatary_B_Str;
                                }//data available
                                else
                                {
                                    //set data not available to PLC
                                    byte[] tmpbyte = new byte[2];
                                    tmpbyte = Encoding.ASCII.GetBytes("ER");
                                    Array.Copy(tmpbyte, 0, PLCWriteCommand, 155, 2);
                                    //  St2Log.Info("Rotary B Give Infomation to PLC Fail ,System Don't have that Label " + _ST02Rotatary_B_Str);
                                }//data not available
                            }
                            catch
                            {
                                byte[] tmpbyte = new byte[2];
                                tmpbyte = Encoding.ASCII.GetBytes("ER");
                                Array.Copy(tmpbyte, 0, PLCWriteCommand, 155, 2);
                                //   St2Log.Info("Rotary B Give Infomation to PLC Fail ,System Don't have that Label " + _ST02Rotatary_B_Str);
                            }
                        }
                        statusbyte = new byte[2];
                        statusbyte[0] = PLCWriteCommand[215];//DM297
                        statusbyte[1] = PLCWriteCommand[216];//DM297
                        strStatus = System.Text.Encoding.Default.GetString(statusbyte);
                        //Address block D0271 to D0300 == PLCWriteCommand[215] to PLCWriteCommand[216]
                        if (ST02Rotatary_C_Str == "\0\0\0\0\0\0\0\0\0\0")
                        {
                            //set data available = none
                            PLCWriteCommand[215] = PLCWriteCommand[216] = 0;
                        }
                        else
                        {
                            //search for data in data list
                            try
                            {
                                while (!networkmain.UpdateRotaryABC_FinishingLabel(ST02Rotatary_C_Str, "RC"))//assume PLC data moved before server reply
                                {
                                    Thread.Sleep(10);
                                }
                                if ((strStatus == "NA") || ((PLCWriteCommand[215] == 0) && (PLCWriteCommand[216] == 0)))
                                {
                                    byte[] plcbuffer;
                                    UpdatePLCFinishingLabelDMAddress(out plcbuffer, ST02Rotatary_C_Str);
                                    Array.Copy(plcbuffer, 0, PLCWriteCommand, RC_PCtoPLCFinishingLabelOFFSET, 58);//PLCWriteCommand[163] == DM271
                                                                                                                  // St2Log.Info("Rotary C Give Infomation to PLC " + _ST02Rotatary_C_Str);
                                    networkmain.linePack.Info("Rotary C Give Infomation to PLC " + _ST02Rotatary_C_Str);
                                    ReceiveFL2 = _ST02Rotatary_C_Str;
                                }//data available
                                else
                                {//set data not available to PLC
                                    byte[] tmpbyte = new byte[2];
                                    tmpbyte = Encoding.ASCII.GetBytes("ER");
                                    Array.Copy(tmpbyte, 0, PLCWriteCommand, 215, 2);
                                    //  St2Log.Info("Rotary C Give Infomation to PLC Fail ,System Don't have that Label " + _ST02Rotatary_C_Str);
                                }//data not available
                            }
                            catch
                            {
                                byte[] tmpbyte = new byte[2];
                                tmpbyte = Encoding.ASCII.GetBytes("ER");
                                Array.Copy(tmpbyte, 0, PLCWriteCommand, 215, 2);
                                //  St2Log.Info("Rotary C Give Infomation to PLC Fail ,System Don't have that Label " + _ST02Rotatary_C_Str);
                            }
                        }
                        networkmain.ServerReplyEvt.Reset();
                        #endregion
                    }
                    //end of handling finishing label
                    //Operator Logout update
                    //Error senario form PLC
                    //FG01 to FG02 Move
                    //AQL Message
                    //Common Event Structure 
                    //1. use for timing on individual finishing label
                    //2. use for logout of technician
                    //process outgoing messages
                    //*******************End of Message processing for network******************************
                    #endregion


                }
                catch (Exception ex)
                {
                    Log1.Info(ex.ToString());
                }
                // Log1.Info("Thread Exit");
            }//try first
        }
    
    }
}
