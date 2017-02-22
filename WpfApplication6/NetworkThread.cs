//using Cognex.DataMan.SDK.Utils;
//using CognexHandheldScannerPackage;
//using Cognex.DataMan.SDK;
//using Cognex.DataMan.SDK.Discovery;
using BCReader;
using InnovacVacuumSealerPackage;
using NLog;
using System;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Xml;
using IGTwpf;
using System.Collections;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Net.Sockets;
using LPAUtils.DB;

namespace InnogrityLinePackingClient
{
    partial class NetworkThread : INotifyPropertyChanged
    {
        public const int waitdelay = 50;
        #region Variable declare and Get,Set properties
        Thread Nkthread;
        Thread PLC01Thread;
        Thread PLC02Thread;
        Thread MiddleWareThread;
        Thread Station06Operator01Thread;
        Thread Station06Operator02Thread;
        Thread Station02PrinterThread;
        Thread Station04PrinterThread;
        Thread Station04PrinterclearThread;
        Thread Station02PrinterclearRAThread;
        Thread Station02PrinterclearRBThread;
        Thread Station02PrinterclearRCThread;
        Thread Station07PrinterESDThread;
        Thread Station07PrinterThread;
        Thread Station07PrinterclearThread;
        Thread Station02LabelInspectionThread;
        Thread Station04LabelInspectionThread;
        Thread Station07LabelInspectionThread;
        Thread Station02Thread;
        Thread Station05VacuumSealersThread;
        Thread Station05VacuumSealer1Thread;
        Thread Station05VacuumSealer2Thread;
        Thread Station05VacuumSealer3Thread;
        Thread UploadhandScannerDataThread;
        Thread Station04ScannerThread;
        //add
        //Thread PLCCommunicationThread;
        ////Thread Printer01Thread;
        // Thread Printer02Thread;
        //Thread Printer03Thread;
        //Thread Printer04Thread;
        //Thread St02Vision01Thread;
        //Thread St02Vision02Thread;
        //Thread St04Vision01Thread;
        //Thread St07Vision03Thread;
        //Thread Scanner01Thread;
        //Thread Scanner02Thread;
        Thread ParameterChangeThread;
        Thread Station6_7_8Thread;


        public Logger St2Log = LogManager.GetLogger("Station 2");
        public Logger EvtLog = LogManager.GetLogger("AllEventLog"); //Temporaly can split to by Station
        public Logger Sealer1Log = LogManager.GetLogger("Sealer1");
        public Logger Sealer2Log = LogManager.GetLogger("Sealer2");
        public Logger Sealer3Log = LogManager.GetLogger("Sealer3");
        public Logger Station5Log = LogManager.GetLogger("Station5");
        public  Int32 er71;
        public Logger PurgePau = LogManager.GetLogger("PURGE And PAUSE");
        public Logger Log1 = LogManager.GetLogger("Poll");
        public Logger LogEr = LogManager.GetLogger("All Station Error LOG");
        public Logger Station3Log = LogManager.GetLogger("Station3");
        public Logger StationStarStopLog = LogManager.GetLogger("StationStarStop");
        public Logger IGTOEELog = LogManager.GetLogger("IGTOEELog");
        public Logger Station4Log = LogManager.GetLogger("Station4");
        public Logger Station7Log = LogManager.GetLogger("Station7");
        public Logger Station8Log = LogManager.GetLogger("Station8");
        public Logger Station6RejectboundaryLog = LogManager.GetLogger("Station6RejectboundaryLog");

       public Logger AllRJEvent = LogManager.GetLogger("AllRJEvent");



        #region PLC1
        TelnetClient PLCTelnet;
        byte[] PLCQueryCmd = new byte[21];
        byte[] PLCQueryRx = new byte[211];
        byte[] PLCWriteCommand;
        byte[] PLCWriteCommandRX;
        byte[] PLCQueryCmd7 = new byte[21];
        byte[] PLCQueryRx7 = new byte[311];
        byte[] PLCWriteCommand7;
        byte[] PLCWriteCommandRX7;
        byte[] PLCQueryCmdPara = new byte[21];
        byte[] PLCQueryRxPara = new byte[811];

        byte[] PLCQueryCmdParaPlc2 = new byte[21];
        byte[] PLCQueryRxParaPlc2 = new byte[1011];

        public const int WaitForOperatorLogin = 0;
        public const int WaitForOperatorLogout = 1;
        public const int WAITFORFINISHINGLABEL = 0;//wait for finishing label to be in ie. transition from "/0/0/0/0 .." to a finishing label                          
        public const int DISPLAYVALIDLABELINFO = 1;//check for valid finishing label
        public const int VALIDLABELRUNSCANNER = 2;//finishing label is valid, turn on scanner
        public const int SCANNERTIMEOUTHANDLE = 3;//scan time out
        public const int SCANNUMBEREXCEEDHANDLE = 4;//scan exceed 3 x
        public const int SCANMATCHWAITTRACKINGLABEL = 5;//scan matched, waiting for tracking label or wait for reject
        public const int SPECLABELPROCESS = 13;
        public const int TRACKINGLABELAVAILABLE = 6;//tracking label available
        public const int REJECTRXAFTERSCANMATCHED = 7;//reject recieved from PLC @ state "5"
        public const int INVALIDLABELHANDLE = 8;//recieved an invalid finishing label @ sate "1"
        public const int WAITTRACKINGLABELCLEAR = 9;
        public const int ERHANDLER_STEP01 = 10;
        public const int ERHANDLER_STEP02 = 11;
        public const int ERHANDLER_STEP03 = 12;
        public const int RA_PCtoPLCFinishingLabelOFFSET = 41; //100 101 and 102 are OK,NA,ER
        public const int RB_PCtoPLCFinishingLabelOFFSET = 103;//DM241 
        public const int RC_PCtoPLCFinishingLabelOFFSET = 163;//DM271
        public const int PLCQueryRx_DM100 = 211;
        public const int PLCWriteCommand_DM200 = 21;
        public const int PLCWriteCommand_DM301 = 223;
        public const int PLCWriteCommand_DM303 = 227;
        public const int PLCWriteCommand_DM401 = 423;
        public const int PLCQueryRx_DM157 = 325;
        public const int PLCWriteCommand_DM380 = 381;
        public const int PLCQueryRx_DM177 = 365;
        public const int PLCWriteCommand_DM302 = 225; //PC DM302
        public const int PLCQueryRx_DM107 = 225;
        public const int Station3OFFSET = 321;//DM350
        public const int Station4MBBOFFSET = 427;//DM403
        public const int Station4OFFSET = 241;//DM310
        public const int PLCQueryRx_DM85 = 181; //DM85
        public const int PLCQueryRx_DM140 = 291; //DM140
        public const int PLCQueryRx_DM188 = 387; //DM188
        public const int PLCQueryRx_DM150 = 311;
        public const int SurfaceOFFSETst4 = 233;//DM306
        public const int XOFFSETForst4 = 235;//DM307
        public const int YOFFSETForst4 = 237;//DM308
        public const int XOFFSETForst4handscanbarcode = 291; //335
        public const int PLCWriteCommand_DM399 = 419;//399
        public const int PLCQueryRx_DM179 = 369;//D179
        public const int PLCWriteCommand_DM340 = 301;
        public bool St4Bypass, Isspekteck; //Temp -- Delete me by GY


        #endregion
        #region PLC2
        TelnetClient PLCTelnet2;
        byte[] PLCQueryCmd6 = new byte[21];
        byte[] PLCQueryRx6 = new byte[211];
        byte[] PLCWriteCommand6;
        byte[] PLCWriteCommandRX6;
        public const int OperatorA_PCtoPLCFinishingLabelOFFSET = 41;//DM5210
        public const int OperatorB_PCtoPLCFinishingLabelOFFSET = 101;//DM5240
        public const int PLCFinishingLabelOFFSET = 163;//DM5271
        public const int PLCQueryRx_DM5100 = 211;
        public const int PLCWriteCommand_DM5200 = 21;
        public const int PLCWriteCmdOP01LoginLogout = 21;
        public const int PLCWriteCmdOP02LoginLogout = 23;
        public const int PLCQueryRx_DM5111 = 233; //DM5111
        public const int OperatorA_TrackingLabelOFFSET = 51;//DM5020
        public const int OperatorB_TrackingLabelOFFSET = 71;//DM5030
        public const int Station5OFFSET = 249;//DM5314
        public const int PLCQueryRx_DM5113 = 237; //D5113
        public const int PLCWriteCommand_DM5349 = 319; //D5349
        public const int PLCQueryRx_DM5199 = 409;
        public const int PLCQueryRx_DM5016 = 43; //clear hotlot
        public const int PLCQueryRx_DM198=407;
        public const int Station6Barcode1OFFSET = 323;//DM5351
        public const int Station6Barcode2OFFSET = 343;//DM5361
        public const int Station6Barcode3OFFSET = 363;//DM5371
        public const int Station6Barcode4OFFSET = 383;//DM5381
        public const int Station7BarcodeOFFSET = 403;//DM5391
        public const int Station7OFFSET = 263;//DM5321
        public const int SurfaceOFFSETst7 = 273;//DM5326
        public const int XOFFSETForst7 = 275;//DM5327
        public const int YOFFSETForst7 = 277;//DM5328
        public const int Station8OFFSET = 291;//DM5335
        public const int PLCWriteCommand_DM5204 = 29; //PC
        public const int PLCWriteCommand_DM5330 = 281;
        public const int PLCWriteCommand_DM5310 = 241; //DM5310
        public const int PLCWriteCommand_DM5320 = 261; //PC DM5320
       // public const int PLCWriteCommand_DM5351 = 277; //PC DM5351
        //4and5

        public const int PLCQueryRx_DM5159 = 329;
        public const int PLCQueryRx_DM5172 = 355;
        public const int PLCQueryRx_DM5173 = 357;


        public const int PLCWriteCommand_DM5362 = 345;
        public const int PLCWriteCommand_DM5363 = 347;
        public const int PLCWriteCommand_DM5364 = 349;

        public const int PLCWriteCommand_DM5426 = 473;



        public const int PLCWriteCommand_DM5353 = 327;
        public const int PLCWriteCommand_DM5350 = 321; //PC DM5350
        public const int PLCWriteCommand_DM5402 = 425;// PC DM5402
        public const int PLCWriteCommand_DM5405 = 431;// PC DM5405
        public const int PLCWriteCommand_DM5406 = 433;// PC DM5406
        public const int PLCWriteCommand_DM5407 = 435;// PC DM5407
        public const int PLCWriteCommand_DM304 = 229;
        public const int PLCWriteCommand_DM5412 = 445;
        public const int PLCWriteCommand_DM5412b = 446;
        public const int PLCWriteCommand_DM5413 = 447;
        public const int PLCWriteCommand_DM5413b = 448;
        public const int PLCWriteCommand_DM5414 = 449;
        public const int PLCWriteCommand_DM5414b = 450;
        public const int PLCWriteCommand_DM5416 = 453;
        public const int PLCWriteCommand_DM5356 = 333;
        public const int PLCWriteCommand_DM5356b =334;






        public const int PLCQueryRx_DM5105 = 221;
        public const int PLCWriteCommand_DM5326 = 273; //PC DM5326
        public const int PLCWriteCommand_DM5327 = 275; //PC DM5327
        public const int PLCWriteCommand_DM5328 = 277; //PC DM5328

        public const int PLCWriteCommand_DM5351 = 323;
       
        public const int PLCQueryRx_DM5145 = 301;
        public const int PLCWriteCommand_DM5346 = 313;

        public const int PLCQueryRx_DM5130 = 271;
        public const int PLCWriteCommand_DM5427 = 475;

        public const int PLCQueryRx_DM134 = 279;
        public const int PLCWriteCommand_DM427 = 475;
        public const int PLCQueryRx_DM135 = 281;
        public const int PLCWriteCommand_DM428 = 477;

        #endregion
        #region
        private string _Station6OP1SpeckTeK;
        public string Station6OP1SpeckTeK
        {
            get { return _Station6OP1SpeckTeK; }
            set
            {
                _Station6OP1SpeckTeK = value;
                OnPropertyChanged("Station6OP1SpeckTeK");
            }
        }



       private string _RJResultst5S1;
        public string RJResultst5S1
        {
            get
            {
                return _RJResultst5S1;
            }
            set
            {
                _RJResultst5S1 = value;
                OnPropertyChanged("RJResultst5S1");
            }
        }

       private string _RJResultst5S2;
        public string RJResultst5S2
        {
            get
            {
                return _RJResultst5S2;
            }
            set
            {
                _RJResultst5S2 = value;
                OnPropertyChanged("RJResultst5S2");
            }
        }



      private string _RJResultst5S3;
        public string RJResultst5S3
        {
            get
            {
                return _RJResultst5S3;
            }
            set
            {
                _RJResultst5S3 = value;
                OnPropertyChanged("RJResultst5S3");
            }
        }


       private string _ResetCountButton;
        public string ResetCountButton
        {
            get
            {
                return _ResetCountButton;
            }
            set
            {
                _ResetCountButton = value;
                OnPropertyChanged("ResetCountButton");
            }
        }



         private string _ResetCountButton2;
        public string ResetCountButton2
        {
            get
            {
                return _ResetCountButton2;
            }
            set
            {
                _ResetCountButton2 = value;
                OnPropertyChanged("ResetCountButton2");
            }
        }






       private string _RJButton;
        public string RJButton
        {
            get
            {
                return _RJButton;
            }
            set
            {
                _RJButton = value;
                OnPropertyChanged("RJButton");
            }
        }


      private string _RJButton2;
        public string RJButton2
        {
            get
            {
                return _RJButton2;
            }
            set
            {
                _RJButton2 = value;
                OnPropertyChanged("RJButton2");
            }
        }
        private string _Station6OP1_TYPE;
        public string Station6OP1_TYPE
        {
            get { return _Station6OP1_TYPE; }
            set
            {
                _Station6OP1_TYPE = value;
                OnPropertyChanged("Station6OP1_TYPE");
            }
        }

        private string _Station6OP2_TYPE;
        public string Station6OP2_TYPE
        {
            get { return _Station6OP2_TYPE; }
            set
            {
                _Station6OP2_TYPE = value;
                OnPropertyChanged("Station6OP2_TYPE");
            }
        }






        private string _Station6OP2SpeckTeK;
        public string Station6OP2SpeckTeK
        {
            get { return _Station6OP2SpeckTeK; }
            set
            {
                _Station6OP2SpeckTeK = value;
                OnPropertyChanged("Station6OP2SpeckTeK");
            }

        }

        private int _Operator02State;
        public int Operator02State
        {
            get { return _Operator02State; }
            set
            {
                _Operator02State = value;
                OnPropertyChanged("Operator02State");
            }
        }
        private int _Operator01LoginState;
        public int Operator01LoginState
        {
            get { return _Operator01LoginState; }
            set
            {
                _Operator01LoginState = value;
                OnPropertyChanged("Operator01LoginState");
            }
        }
        private int _Operator02LoginState;
        public int Operator02LoginState
        {
            get { return _Operator02LoginState; }
            set
            {
                _Operator02LoginState = value;
                OnPropertyChanged("Operator02LoginState");
            }
        }
        private int _Operator01State;
        public int Operator01State
        {
            get { return _Operator01State; }
            set
            {
                _Operator01State = value;
                OnPropertyChanged("Operator01State");
            }
        }
        private string _Station1Barcode1;
        public string Station1Barcode1
        {
            get { return _Station1Barcode1; }
            set
            {
                _Station1Barcode1 = value;
                OnPropertyChanged("Station1Barcode1");
            }
        }
        private string _Station1Barcode2;
        public string Station1Barcode2
        {
            get { return _Station1Barcode2; }
            set
            {
                _Station1Barcode2 = value;
                OnPropertyChanged("Station1Barcode2");
            }
        }
        private string _Station1Barcode3;
        public string Station1Barcode3
        {
            get { return _Station1Barcode3; }
            set
            {
                _Station1Barcode3 = value;
                OnPropertyChanged("Station1Barcode3");
            }
        }
        private string _Station1Barcode4;
        public string Station1Barcode4
        {
            get { return _Station1Barcode4; }
            set
            {
                _Station1Barcode4 = value;
                OnPropertyChanged("Station1Barcode4");
            }
        }
        private string _Station4Barcode;
        public string Station4Barcode
        {
            get { return _Station4Barcode; }
            set
            {
                _Station4Barcode = value;
                OnPropertyChanged("Station4Barcode");
            }
        }
        private string _Station6Barcode1;
        public string Station6Barcode1
        {
            get { return _Station6Barcode1; }
            set
            {
                _Station6Barcode1 = value;
                OnPropertyChanged("Station6Barcode1");
            }
        }
        private string _Station6Barcode2;
        public string Station6Barcode2
        {
            get { return _Station6Barcode2; }
            set
            {
                _Station6Barcode2 = value;
                OnPropertyChanged("Station6Barcode2");
            }
        }
        private string _Station6Barcode3;
        public string Station6Barcode3
        {
            get { return _Station6Barcode3; }
            set
            {
                _Station6Barcode3 = value;
                OnPropertyChanged("Station6Barcode3");
            }
        }
        private string _Station6Barcode4;
        public string Station6Barcode4
        {
            get { return _Station6Barcode4; }
            set
            {
                _Station6Barcode4 = value;
                OnPropertyChanged("Station6Barcode4");
            }
        }
        private string _Station7Barcode;
        public string Station7Barcode
        {
            get { return _Station7Barcode; }
            set
            {
                _Station7Barcode = value;
                OnPropertyChanged("Station7Barcode");
            }
        }
        private byte _PLC1Station4;
        public byte PLC1Station4
        {
            get { return _PLC1Station4; }
            set
            {
                _PLC1Station4 = value;
                OnPropertyChanged("PLC1Station4");
            }
        }
        private byte _PLC2Station5;
        public byte PLC2Station5
        {
            get { return _PLC2Station5; }
            set
            {
                _PLC2Station5 = value;
                OnPropertyChanged("PLC2Station5");
            }
        }
        private int _POcount1;
        public int POcount1
        {
            get { return _POcount1; }
            set
            {
                _POcount1 = value;
                OnPropertyChanged("POcount1");
            }
        }
        private int _POcount2;
        public int POcount2
        {
            get { return _POcount2; }
            set
            {
                _POcount2 = value;
                OnPropertyChanged("POcount2");
            }
        }
        private int _st1POcount;
        public int st1POcount
        {
            get { return _st1POcount; }
            set
            {
                _st1POcount = value;
                OnPropertyChanged("st1POcount");
            }
        }
        private int _st2POcount;
        public int st2POcount
        {
            get { return _st2POcount; }
            set
            {
                _st2POcount = value;
                OnPropertyChanged("st2POcount");
            }
        }


        private string _SealerNumberQC1;
        public string SealerNumberQC1
        {
            get { return _SealerNumberQC1; }
            set
            {
                _SealerNumberQC1 = value;
                OnPropertyChanged("SealerNumberQC1");
            }
        }


      private string _SealerNumberQC2;
        public string SealerNumberQC2
        {
            get { return _SealerNumberQC2; }
            set
            {
                _SealerNumberQC2 = value;
                OnPropertyChanged("SealerNumberQC2");
            }
        }



        private int _st1Rejectcount;
        public int st1Rejectcount
        {
            get { return _st1Rejectcount; }
            set
            {
                _st1Rejectcount = value;
                OnPropertyChanged("st1Rejectcount");
            }
        }
        private int _st2Rejectcount;
        public int st2Rejectcount
        {
            get { return _st2Rejectcount; }
            set
            {
                _st2Rejectcount = value;
                OnPropertyChanged("st2Rejectcount");
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        // MainNetworkClass networkmain;
        private MainNetworkClass _networkmain;
        public MainNetworkClass networkmain
        {
            get { return _networkmain; }
            set
            {
                _networkmain = value;
                OnPropertyChanged("networkmain");
            }
        }
        static public volatile bool bTerminate;//non optimise so that other thread can change this values
        private bool _connected;
        public bool connected
        {
            get { return _connected; }
            set
            {
                _connected = value;
                OnPropertyChanged("connected");
            }
        }
        private ManualResetEvent _evt_Station02InspectionReq;
        public ManualResetEvent evt_Station02InspectionReq
        {
            get { return _evt_Station02InspectionReq; }
            set
            {
                _evt_Station02InspectionReq = value;
                OnPropertyChanged("evt_Station02InspectionReq");
            }
        }
        private ManualResetEvent _evt_Station04InspectionReq;
        public ManualResetEvent evt_Station04InspectionReq
        {
            get { return _evt_Station04InspectionReq; }
            set
            {
                _evt_Station04InspectionReq = value;
                OnPropertyChanged("evt_Station04InspectionReq");
            }
        }
        private ManualResetEvent _evt_Station07InspectionReq;
        public ManualResetEvent evt_Station07InspectionReq
        {
            get { return _evt_Station07InspectionReq; }
            set
            {
                _evt_Station07InspectionReq = value;
                OnPropertyChanged("evt_Station07InspectionReq");
            }
        }
        private ManualResetEvent _evt_Station02PrintReq;
        public ManualResetEvent evt_Station02PrintReq
        {
            get { return _evt_Station02PrintReq; }
            set
            {
                _evt_Station02PrintReq = value;
                OnPropertyChanged("evt_Station02PrintReq");
            }
        }
        private ManualResetEvent _evt_Station04PrintReq;
        public ManualResetEvent evt_Station04PrintReq
        {
            get { return _evt_Station04PrintReq; }
            set
            {
                _evt_Station04PrintReq = value;
                OnPropertyChanged("evt_Station04PrintReq");
            }
        }

        private ManualResetEvent _evt_Station04PrintClearReq;
        public ManualResetEvent evt_Station04PrintClearReq
        {
            get { return _evt_Station04PrintClearReq; }
            set
            {
                _evt_Station04PrintClearReq = value;
                OnPropertyChanged("evt_Station04PrintClearReq");
            }
        }
        private ManualResetEvent _evt_Station07PrintClearReq;
        public ManualResetEvent evt_Station07PrintClearReq
        {
            get { return _evt_Station07PrintClearReq; }
            set
            {
                _evt_Station07PrintClearReq = value;
                OnPropertyChanged("evt_Station07PrintClearReq");
            }
        }



        private ManualResetEvent _evt_Station02PrintClearReqRA;
        public ManualResetEvent evt_Station02PrintClearReqRA
        {
            get { return _evt_Station02PrintClearReqRA; }
            set
            {
                _evt_Station02PrintClearReqRA = value;
                OnPropertyChanged("evt_Station02PrintClearReqRA");
            }
        }

        private ManualResetEvent _evt_Station02PrintClearReqRB;
        public ManualResetEvent evt_Station02PrintClearReqRB
        {
            get { return _evt_Station02PrintClearReqRB; }
            set
            {
                _evt_Station02PrintClearReqRB = value;
                OnPropertyChanged("evt_Station02PrintClearReqRB");
            }
        }



        private ManualResetEvent _evt_Station02PrintClearReqRC;
        public ManualResetEvent evt_Station02PrintClearReqRC
        {
            get { return _evt_Station02PrintClearReqRC; }
            set
            {
                _evt_Station02PrintClearReqRC = value;
                OnPropertyChanged("evt_Station02PrintClearReqRC");
            }
        }



       private ManualResetEvent _evt_Station07ESDPrintReq;
        public ManualResetEvent evt_Station07ESDPrintReq
        {
            get { return _evt_Station07ESDPrintReq; }
            set
            {
                _evt_Station07ESDPrintReq = value;
                OnPropertyChanged("evt_Station07ESDPrintReq");
            }
        }




        private ManualResetEvent _evt_Station07PrintReq;
        public ManualResetEvent evt_Station07PrintReq
        {
            get { return _evt_Station07PrintReq; }
            set
            {
                _evt_Station07PrintReq = value;
                OnPropertyChanged("evt_Station07PrintReq");
            }
        }
        private ManualResetEvent _evt_FG01_FG02Move;
        public ManualResetEvent evt_FG01_FG02Move
        {
            get { return _evt_FG01_FG02Move; }
            set
            {
                _evt_FG01_FG02Move = value;
                OnPropertyChanged("evt_FG01_FG02Move");
            }
        }
        private ManualResetEvent _evt_FG01_FG02Move_Rx;
        public ManualResetEvent evt_FG01_FG02Move_Rx
        {
            get { return _evt_FG01_FG02Move_Rx; }
            set
            {
                _evt_FG01_FG02Move_Rx = value;
                OnPropertyChanged("evt_FG01_FG02Move_Rx");
            }
        }
        private ManualResetEvent _evt_FinishLabelRequestComplete;
        public ManualResetEvent evt_FinishLabelRequestComplete
        {
            get { return _evt_FinishLabelRequestComplete; }
            set
            {
                _evt_FinishLabelRequestComplete = value;
                OnPropertyChanged("evt_FinishLabelRequestComplete");
            }
        }
        private ManualResetEvent _evt_FinishLabelRequest;
        public ManualResetEvent evt_FinishLabelRequest
        {
            get { return _evt_FinishLabelRequest; }
            set
            {
                _evt_FinishLabelRequest = value;
                OnPropertyChanged("evt_FinishLabelRequest");
            }
        }
        private ManualResetEvent _evt_FinishLabelRequestSt4;
        public ManualResetEvent evt_FinishLabelRequestSt4
        {
            get { return _evt_FinishLabelRequestSt4; }
            set
            {
                _evt_FinishLabelRequestSt4 = value;
                OnPropertyChanged("evt_FinishLabelRequestSt4");
            }
        }

        private ManualResetEvent _evnt_CheckingConnectionForSealer1;
        public ManualResetEvent evnt_CheckingConnectionForSealer1
        {
            get { return _evnt_CheckingConnectionForSealer1; }
            set
            {
                _evnt_CheckingConnectionForSealer1 = value;
                OnPropertyChanged("evnt_CheckingConnectionForSealer1");
            }
        }

       private ManualResetEvent _evnt_CheckingConnectionForSealer2;
        public ManualResetEvent evnt_CheckingConnectionForSealer2
        {
            get { return _evnt_CheckingConnectionForSealer2; }
            set
            {
                _evnt_CheckingConnectionForSealer2 = value;
                OnPropertyChanged("evnt_CheckingConnectionForSealer2");
            }
        }
      private ManualResetEvent _evnt_CheckingConnectionForSealer3;
        public ManualResetEvent evnt_CheckingConnectionForSealer3
        {
            get { return _evnt_CheckingConnectionForSealer3; }
            set
            {
                _evnt_CheckingConnectionForSealer3 = value;
                OnPropertyChanged("evnt_CheckingConnectionForSealer3");
            }
        }


        private ManualResetEvent _evnt_FindFinishingLabelForSealer1;
        public ManualResetEvent evnt_FindFinishingLabelForSealer1
        {
            get { return _evnt_FindFinishingLabelForSealer1; }
            set
            {
                _evnt_FindFinishingLabelForSealer1 = value;
                OnPropertyChanged("evnt_FindFinishingLabelForSealer1");
            }
        }
        private ManualResetEvent _evnt_FindFinishingLabelForSealer2;
        public ManualResetEvent evnt_FindFinishingLabelForSealer2
        {
            get { return _evnt_FindFinishingLabelForSealer2; }
            set
            {
                _evnt_FindFinishingLabelForSealer2 = value;
                OnPropertyChanged("evnt_FindFinishingLabelForSealer2");
            }
        }
        private ManualResetEvent _evnt_FindFinishingLabelForSealer3;
        public ManualResetEvent evnt_FindFinishingLabelForSealer3
        {
            get { return _evnt_FindFinishingLabelForSealer3; }
            set
            {
                _evnt_FindFinishingLabelForSealer3 = value;
                OnPropertyChanged("evnt_FindFinishingLabelForSealer3");
            }
        }
        private ManualResetEvent _evnt_FindFinishingLabelForOperator;
        public ManualResetEvent evnt_FindFinishingLabelForOperator
        {
            get { return _evnt_FindFinishingLabelForOperator; }
            set
            {
                _evnt_FindFinishingLabelForOperator = value;
                OnPropertyChanged("evnt_FindFinishingLabelForOperator");
            }
        }
        private ManualResetEvent _evnt_FindFinishingLabelForOperator2;
        public ManualResetEvent evnt_FindFinishingLabelForOperator2
        {
            get { return _evnt_FindFinishingLabelForOperator2; }
            set
            {
                _evnt_FindFinishingLabelForOperator2 = value;
                OnPropertyChanged("evnt_FindFinishingLabelForOperator2");
            }
        }
        private ManualResetEvent _evnt_TrackingLabelForOperator;
        public ManualResetEvent evnt_TrackingLabelForOperator
        {
            get { return _evnt_TrackingLabelForOperator; }
            set
            {
                _evnt_TrackingLabelForOperator = value;
                OnPropertyChanged("evnt_TrackingLabelForOperator");
            }
        }
        private ManualResetEvent _evnt_TrackingLabelForOperator2;
        public ManualResetEvent evnt_TrackingLabelForOperator2
        {
            get { return _evnt_TrackingLabelForOperator2; }
            set
            {
                _evnt_TrackingLabelForOperator2 = value;
                OnPropertyChanged("evnt_TrackingLabelForOperator2");
            }
        }
        private ManualResetEvent _evnt_RejForOperator1;
        public ManualResetEvent evnt_RejForOperator1
        {
            get { return _evnt_RejForOperator1; }
            set
            {
                _evnt_RejForOperator1 = value;
                OnPropertyChanged("evnt_RejForOperator1");
            }
        }
        private ManualResetEvent _evnt_ScannerRetryForOperator1;
        public ManualResetEvent evnt_ScannerRetryForOperator1
        {
            get { return _evnt_ScannerRetryForOperator1; }
            set
            {
                _evnt_ScannerRetryForOperator1 = value;
                OnPropertyChanged("evnt_ScannerRetryForOperator1");
            }
        }
        private ManualResetEvent _evnt_ScannerRetryForOperator2;
        public ManualResetEvent evnt_ScannerRetryForOperator2
        {
            get { return _evnt_ScannerRetryForOperator2; }
            set
            {
                _evnt_ScannerRetryForOperator2 = value;
                OnPropertyChanged("evnt_ScannerRetryForOperator");
            }
        }


        private ManualResetEvent _evnt_RejectFinishingLabelForStation4;
        public ManualResetEvent evnt_RejectFinishingLabelForStation4
        {
            get { return _evnt_RejectFinishingLabelForStation4; }
            set
            {
                _evnt_RejectFinishingLabelForStation4 = value;
                OnPropertyChanged("evnt_RejectFinishingLabelForStation4");
            }
        }



        private ManualResetEvent _evnt_RejectFinishingLabelForStation6_OP1;
        public ManualResetEvent evnt_RejectFinishingLabelForStation6_OP1
        {
            get { return _evnt_RejectFinishingLabelForStation6_OP1; }
            set
            {
                _evnt_RejectFinishingLabelForStation6_OP1 = value;
                OnPropertyChanged("evnt_RejectFinishingLabelForStation6_OP1");
            }
        }


        private ManualResetEvent _evnt_RejectFinishingLabelForStation6_OP2;
        public ManualResetEvent evnt_RejectFinishingLabelForStation6_OP2
        {
            get { return _evnt_RejectFinishingLabelForStation6_OP2; }
            set
            {
                _evnt_RejectFinishingLabelForStation6_OP2 = value;
                OnPropertyChanged("evnt_RejectFinishingLabelForStation6_OP2");
            }
        }





        private ManualResetEvent _evnt_RejectFinishingLabelForStation8;
        public ManualResetEvent evnt_RejectFinishingLabelForStation8
        {
            get { return _evnt_RejectFinishingLabelForStation8; }
            set
            {
                _evnt_RejectFinishingLabelForStation8 = value;
                OnPropertyChanged("evnt_RejectFinishingLabelForStation8");
            }
        }











        private ManualResetEvent _evnt_RejForOperator2;
        public ManualResetEvent evnt_RejForOperator2
        {
            get { return _evnt_RejForOperator2; }
            set
            {
                _evnt_RejForOperator2 = value;
                OnPropertyChanged("evnt_RejForOperator2");
            }
        }
        private ManualResetEvent _evnt_ScannerForOperator;
        public ManualResetEvent evnt_ScannerForOperator
        {
            get { return _evnt_ScannerForOperator; }
            set
            {
                _evnt_ScannerForOperator = value;
                OnPropertyChanged("evnt_ScannerForOperator");
            }
        }
        private ManualResetEvent _evnt_ScannerForOperator2;
        public ManualResetEvent evnt_ScannerForOperator2
        {
            get { return _evnt_ScannerForOperator2; }
            set
            {
                _evnt_ScannerForOperator2 = value;
                OnPropertyChanged("evnt_ScannerForOperator2");
            }
        }
        private string _NetworkAddress;
        public string NetworkAddress
        {
            get { return _NetworkAddress; }
            set
            {
                _NetworkAddress = value;
                OnPropertyChanged("NetworkAddress");
            }
        }
        private string _PLCNetworkAddress;
        public string PLCNetworkAddress
        {
            get { return _PLCNetworkAddress; }
            set
            {
                _PLCNetworkAddress = value;
                OnPropertyChanged("PLCNetworkAddress");
            }
        }
        private string _PLC2NetworkAddress;
        public string PLC2NetworkAddress
        {
            get { return _PLC2NetworkAddress; }
            set
            {
                _PLC2NetworkAddress = value;
                OnPropertyChanged("PLC2NetworkAddress");
            }
        }


         private string _Printer2NetworkAddress;
        public string Printer2NetworkAddress
        {
            get { return _Printer2NetworkAddress; }
            set
            {
                _Printer2NetworkAddress = value;
                OnPropertyChanged("Printer2NetworkAddress");
            }
        }
        private string _Printer4NetworkAddress;
        public string Printer4NetworkAddress
        {
            get { return _Printer4NetworkAddress; }
            set
            {
                _Printer4NetworkAddress = value;
                OnPropertyChanged("Printer4NetworkAddress");
            }
        }
        private string _Printer7NetworkAddress;
        public string Printer7NetworkAddress
        {
            get { return _Printer7NetworkAddress; }
            set
            {
                _Printer7NetworkAddress = value;
                OnPropertyChanged("Printer7NetworkAddress");
            }
        }

        private string _VisionNetworkAddress;
        public string VisionNetworkAddress
        {
            get { return _VisionNetworkAddress; }
            set
            {
                _VisionNetworkAddress = value;
                OnPropertyChanged("VisionNetworkAddress");
            }
        }
        private string _VisionNetworkAddress4;
        public string VisionNetworkAddress4
        {
            get { return _VisionNetworkAddress4; }
            set
            {
                _VisionNetworkAddress4 = value;
                OnPropertyChanged("VisionNetworkAddress4");
            }
        }
        private string _VisionNetworkAddress7;
        public string VisionNetworkAddress7
        {
            get { return _VisionNetworkAddress7; }
            set
            {
                _VisionNetworkAddress7 = value;
                OnPropertyChanged("VisionNetworkAddress7");
            }
        }




        private string _ST02Rotatary_A_Str;
        public string ST02Rotatary_A_Str
        {
            get { return _ST02Rotatary_A_Str; }
            set
            {
                _ST02Rotatary_A_Str = value;
                OnPropertyChanged("ST02Rotatary_A_Str");
            }
        }
        private string _ST02Rotatary_B_Str;
        public string ST02Rotatary_B_Str
        {
            get { return _ST02Rotatary_B_Str; }
            set
            {
                _ST02Rotatary_B_Str = value;
                OnPropertyChanged("ST02Rotatary_B_Str");
            }
        }
        private string _ST02Rotatary_C_Str;
        public string ST02Rotatary_C_Str
        {
            get { return _ST02Rotatary_C_Str; }
            set
            {
                _ST02Rotatary_C_Str = value;
                OnPropertyChanged("ST02Rotatary_C_Str");
            }
        }
        private string _ST02Rotatary_Str;
        public string ST02Rotatary_Str
        {
            get { return _ST02Rotatary_Str; }
            set
            {
                _ST02Rotatary_Str = value;
                OnPropertyChanged("ST02Rotatary_Str");
            }
        }
        private string _ST07Rotatary_Str;
        public string ST07Rotatary_Str
        {
            get { return _ST07Rotatary_Str; }
            set
            {
                _ST07Rotatary_Str = value;
                OnPropertyChanged("ST07Rotatary_Str");
            }
        }
        private string _ST07Rotatary_Str1;
        public string ST07Rotatary_Str1
        {
            get { return _ST07Rotatary_Str1; }
            set
            {
                _ST07Rotatary_Str1 = value;
                OnPropertyChanged("ST07Rotatary_Str1");
            }
        }


      private string _ST07Rotatary_StrESD;
        public string ST07Rotatary_StrESD
        {
            get { return _ST07Rotatary_StrESD; }
            set
            {
                _ST07Rotatary_StrESD = value;
                OnPropertyChanged("ST07Rotatary_StrESD");
            }
        }



        private string _ST04Rotatary_Str;
        public string ST04Rotatary_Str
        {
            get { return _ST04Rotatary_Str; }
            set
            {
                _ST04Rotatary_Str = value;
                OnPropertyChanged("ST04Rotatary_Str");
            }
        }
        private string _RotationAngle;
        public string RotationAngle
        {
            get { return _RotationAngle; }
            set
            {
                _RotationAngle = value;
                OnPropertyChanged("RotationAngle");
            }
        }
        private string _PrintStatus;
        public string PrintStatus
        {
            get { return _PrintStatus; }
            set
            {
                _PrintStatus = value;
                OnPropertyChanged("PrintStatus");
            }
        }


       private string _PrintStatus1;
        public string PrintStatus1
        {
            get { return _PrintStatus1; }
            set
            {
                _PrintStatus1 = value;
                OnPropertyChanged("PrintStatus1");
            }
        }


       private string _PrintStatus2;
        public string PrintStatus2
        {
            get { return _PrintStatus2; }
            set
            {
                _PrintStatus2 = value;
                OnPropertyChanged("PrintStatus2");
            }
        }


       private string _PrintStatusst4;
        public string PrintStatusst4
        {
            get { return _PrintStatusst4; }
            set
            {
                _PrintStatusst4 = value;
                OnPropertyChanged("PrintStatusst4");
            }
        }


      private string _PrintStatusst41;
        public string PrintStatusst41
        {
            get { return _PrintStatusst41; }
            set
            {
                _PrintStatusst41 = value;
                OnPropertyChanged("PrintStatusst41");
            }
        }


      private string _PrintStatusst7;
        public string PrintStatusst7
        {
            get { return _PrintStatusst7; }
            set
            {
                _PrintStatusst7 = value;
                OnPropertyChanged("PrintStatusst7");
            }
        }

      private string _PrintStatusst71;
        public string PrintStatusst71
        {
            get { return _PrintStatusst71; }
            set
            {
                _PrintStatusst71 = value;
                OnPropertyChanged("PrintStatusst71");
            }
        }


       private string _PrintStatusst72;
        public string PrintStatusst72
        {
            get { return _PrintStatusst72; }
            set
            {
                _PrintStatusst72 = value;
                OnPropertyChanged("PrintStatusst72");
            }
        }

       private string _PrintStatusst73;
        public string PrintStatusst73
        {
            get { return _PrintStatusst73; }
            set
            {
                _PrintStatusst73 = value;
                OnPropertyChanged("PrintStatusst73");
            }
        }



      private string _VisionRotaryCheck;
        public string VisionRotaryCheck
        {
            get { return _VisionRotaryCheck; }
            set
            {
                _VisionRotaryCheck = value;
                OnPropertyChanged("VisionRotaryCheck");
            }
        }



        private string _VisionStatus;
        public string VisionStatus
        {
            get { return _VisionStatus; }
            set
            {
                _VisionStatus = value;
                OnPropertyChanged("VisionStatus");
            }
        }



       private string _VisionStatus1;
        public string VisionStatus1
        {
            get { return _VisionStatus1; }
            set
            {
                _VisionStatus1 = value;
                OnPropertyChanged("VisionStatus1");
            }
        }


       private string _VisionStatus2;
        public string VisionStatus2
        {
            get { return _VisionStatus2; }
            set
            {
                _VisionStatus2 = value;
                OnPropertyChanged("VisionStatus2");
            }
        }

       private string _VisionStatusst4;
        public string VisionStatusst4
        {
            get { return _VisionStatusst4; }
            set
            {
                _VisionStatusst4 = value;
                OnPropertyChanged("VisionStatusst4");
            }
        }



       private string _VisionStatusst41;
        public string VisionStatusst41
        {
            get { return _VisionStatusst41; }
            set
            {
                _VisionStatusst41 = value;
                OnPropertyChanged("VisionStatusst41");
            }
        }


       private string _VisionStatusst7;
        public string VisionStatusst7
        {
            get { return _VisionStatusst7; }
            set
            {
                _VisionStatusst7 = value;
                OnPropertyChanged("VisionStatusst7");
            }
        }

        private string _VisionStatusst71;
        public string VisionStatusst71
        {
            get { return _VisionStatusst71; }
            set
            {
                _VisionStatusst71 = value;
                OnPropertyChanged("VisionStatusst71");
            }
        }



        private string _VisionStatusst72;
        public string VisionStatusst72
        {
            get { return _VisionStatusst72; }
            set
            {
                _VisionStatusst72 = value;
                OnPropertyChanged("VisionStatusst72");
            }
        }


        private string _VisionStatusst73;
        public string VisionStatusst73
        {
            get { return _VisionStatusst73; }
            set
            {
                _VisionStatusst73 = value;
                OnPropertyChanged("VisionStatusst73");
            }
        }


        private string _SendFL;
        public string SendFL
        {
            get { return _SendFL; }
            set
            {
                _SendFL = value;
                OnPropertyChanged("SendFL");
            }
        }
        private string _ReceiveFL;
        public string ReceiveFL
        {
            get { return _ReceiveFL; }
            set
            {
                _ReceiveFL = value;
                OnPropertyChanged("ReceiveFL");
            }
        }

      private string _ReceiveFL1;
        public string ReceiveFL1
        {
            get { return _ReceiveFL1; }
            set
            {
                _ReceiveFL1 = value;
                OnPropertyChanged("ReceiveFL1");
            }
        }

      private string _HIC_Desiccant;
        public string HIC_Desiccant
        {
            get { return _HIC_Desiccant; }
            set
            {
                _HIC_Desiccant = value;
                OnPropertyChanged("HIC_Desiccant");
            }
        }


       private string _ReceiveFL2;
        public string ReceiveFL2
        {
            get { return _ReceiveFL2; }
            set
            {
                _ReceiveFL2 = value;
                OnPropertyChanged("ReceiveFL2");
            }
        }




       private string _MBBFL;
        public string MBBFL
        {
            get { return _MBBFL; }
            set
            {
                _MBBFL = value;
                OnPropertyChanged("MBBFL");
            }
        }





      private string _ESDFL;
        public string ESDFL
        {
            get { return _ESDFL; }
            set
            {
                _ESDFL = value;
                OnPropertyChanged("ESDFL");
            }
        }





        private string _Scanboxid;

        public ArrayList AllLabels = new ArrayList();



        public string Scanboxid
        {
            get { return _Scanboxid; }
            set
            {
              
                _Scanboxid = value;
             
                OnPropertyChanged("Scanboxid");
              //  AllLabels.Add(value);
            }
        }
        private string _ScanboxidSt3;
        public string ScanboxidSt3
        {
            get { return _ScanboxidSt3; }
            set
            {
                _ScanboxidSt3 = value;
                OnPropertyChanged("ScanboxidSt3");
            }
        }
        private string _ScanboxidSt31;
        public string ScanboxidSt31
        {
            get { return _ScanboxidSt31; }
            set
            {
                _ScanboxidSt31 = value;
                OnPropertyChanged("ScanboxidSt31");
            }
        }
        private string _ScanboxidSt4barcode;
        public string ScanboxidSt4barcode
        {
            get { return _ScanboxidSt4barcode; }
            set
            {
                _ScanboxidSt4barcode = value;
                OnPropertyChanged("ScanboxidSt4barcode");
            }
        }



      private string _ParaOldValue;
        public string ParaOldValue
        {
            get { return _ParaOldValue; }
            set
            {
                _ParaOldValue = value;
                OnPropertyChanged("ParaOldValue");
            }
        }


      private string _ParaNewValue;
        public string ParaNewValue
        {
            get { return _ParaNewValue; }
            set
            {
                _ParaNewValue = value;
                OnPropertyChanged("ParaNewValue");
            }
        }                                                                                                                            
          
          
       private string _ParaName;
        public string ParaName
        {
            get { return _ParaName; }
            set
            {
                _ParaName = value;
                OnPropertyChanged("ParaName");
            }
        }          
      
      

        private string _ParaOldValue1;
        public string ParaOldValue1
        {
            get { return _ParaOldValue1; }
            set
            {
                _ParaOldValue1 = value;
                OnPropertyChanged("ParaOldValue1");
            }
        }


      private string _ParaNewValue1;
        public string ParaNewValue1
        {
            get { return _ParaNewValue1; }
            set
            {
                _ParaNewValue1 = value;
                OnPropertyChanged("ParaNewValue1");
            }
        }                                                                                                                            
          
          
       private string _ParaName1;
        public string ParaName1
        {
            get { return _ParaName1; }
            set
            {
                _ParaName1 = value;
                OnPropertyChanged("ParaName1");
            }
        }          

      


         private string _SealerParaOldValue;
        public string SealerParaOldValue
        {
            get { return _SealerParaOldValue; }
            set
            {
                _SealerParaOldValue = value;
                OnPropertyChanged("SealerParaOldValue");
            }
        }


      private string _SealerParaNewValue;
        public string SealerParaNewValue
        {
            get { return _SealerParaNewValue; }
            set
            {
                _SealerParaNewValue = value;
                OnPropertyChanged("SealerParaNewValue");
            }
        }                                                                                                                            
          
          
       private string _SealerParaName;
        public string SealerParaName
        {
            get { return _SealerParaName; }
            set
            {
                _SealerParaName = value;
                OnPropertyChanged("SealerParaName");
            }
        }          
      






















        private string _ScanboxidSt4_5;
        public string ScanboxidSt4_5
        {
            get { return _ScanboxidSt4_5; }
            set
            {
                _ScanboxidSt4_5 = value;
                OnPropertyChanged("ScanboxidSt4_5");
            }
        }



        private string _ScanboxidSt4_MBB;
        public string ScanboxidSt4_MBB
        {
            get { return _ScanboxidSt4_MBB; }
            set
            {
                _ScanboxidSt4_MBB = value;
                OnPropertyChanged("ScanboxidSt4_MBB");
            }
        }


       private string _ScanboxidSt4RJ;
        public string ScanboxidSt4RJ
        {
            get { return _ScanboxidSt4RJ; }
            set
            {
                _ScanboxidSt4RJ = value;
                OnPropertyChanged("ScanboxidSt4RJ");
            }
        }




        private string _ScanboxidSt4;
        public string ScanboxidSt4
        {
            get { return _ScanboxidSt4; }
            set
            {
                _ScanboxidSt4 = value;
                OnPropertyChanged("ScanboxidSt4");
            }
        }
        private string _ScanboxidSt41;
        public string ScanboxidSt41
        {
            get { return _ScanboxidSt41; }
            set
            {
                _ScanboxidSt41 = value;
                OnPropertyChanged("ScanboxidSt41");
            }
        }

      private string _ScanboxidSt4transpoter;
        public string ScanboxidSt4transpoter
        {
            get { return _ScanboxidSt4transpoter; }
            set
            {
                _ScanboxidSt4transpoter = value;
                OnPropertyChanged("ScanboxidSt4transpoter");
            }
        }





        private string _ScanboxidSt5S1;
        public string ScanboxidSt5S1
        {
            get { return _ScanboxidSt5S1; }
            set
            {
                _ScanboxidSt5S1 = value;
                OnPropertyChanged("ScanboxidSt5S1");
            }
        }

        private string _ScanboxidSt5S2;
        public string ScanboxidSt5S2
        {
            get { return _ScanboxidSt5S2; }
            set
            {
                _ScanboxidSt5S2 = value;
                OnPropertyChanged("ScanboxidSt5S2");
            }
        }

        private string _ScanboxidSt5S3;
        public string ScanboxidSt5S3
        {
            get { return _ScanboxidSt5S3; }
            set
            {
                _ScanboxidSt5S3 = value;
                OnPropertyChanged("ScanboxidSt5S3");
            }
        }


       private string _Statusst5;
        public string Statusst5
        {
            get { return _Statusst5; }
            set
            {
                _Statusst5 = value;
                OnPropertyChanged("Statusst5");
            }
        }



       private string _Statusst51;
        public string Statusst51
        {
            get { return _Statusst51; }
            set
            {
                _Statusst51 = value;
                OnPropertyChanged("Statusst51");
            }
        }




       private string _Statusst52;
        public string Statusst52
        {
            get { return _Statusst52; }
            set
            {
                _Statusst52 = value;
                OnPropertyChanged("Statusst52");
            }
        }

        private string _Status;
        public string Status
        {
            get { return _Status; }
            set
            {
                _Status = value;
                OnPropertyChanged("Status");
            }
        }
        private string _Status1;
        public string Status1
        {
            get { return _Status1; }
            set
            {
                _Status1 = value;
                OnPropertyChanged("Status1");
            }
        }
        private AreaStatus _SafetyStatus;
        public AreaStatus SafetyStatus
        {
            get { return _SafetyStatus; }
            set
            {
                _SafetyStatus = value;
                OnPropertyChanged("SafetyStatus");
            }
        }
        private AreaStatus _SafetyStatus2;
        public AreaStatus SafetyStatus2
        {
            get { return _SafetyStatus2; }
            set
            {
                _SafetyStatus2 = value;
                OnPropertyChanged("SafetyStatus2");
            }
        }
        private BarcodeStatus _ScannerStatus;
        public BarcodeStatus ScannerStatus
        {
            get { return _ScannerStatus; }
            set
            {
                _ScannerStatus = value;
                OnPropertyChanged("ScannerStatus");
            }
        }
        private BarcodeStatus _ScannerStatus2;
        public BarcodeStatus ScannerStatus2
        {
            get { return _ScannerStatus2; }
            set
            {
                _ScannerStatus2 = value;
                OnPropertyChanged("ScannerStatus2");
            }
        }
        private string _UserName1;
        public string UserName1
        {
            get { return _UserName1; }
            set
            {
                _UserName1 = value;
                OnPropertyChanged("UserName1");
            }
        }
        private string _UserName2;
        public string UserName2
        {
            get { return _UserName2; }
            set
            {
                _UserName2 = value;
                OnPropertyChanged("UserName2");
            }
        }
        private string _StationID1;
        public string StationID1
        {
            get { return _StationID1; }
            set
            {
                _StationID1 = value;
                OnPropertyChanged("StationID1");
            }
        }
        private string _StationID2;
        public string StationID2
        {
            get { return _StationID2; }
            set
            {
                _StationID2 = value;
                OnPropertyChanged("StationID2");
            }
        }


        //  private string _Station4ForTransferScanboxidFromPLC1;
        //public string Station4ForTransferScanboxidFromPLC1
        //{
        //    get { return _Station4ForTransferScanboxidFromPLC1; }
        //    set
        //    {
        //        _Station4ForTransferScanboxidFromPLC1 = value;
        //        OnPropertyChanged("Station4ForTransferScanboxidFromPLC1");
        //    }
        //}
        private string _Station5ForTransferScanboxidFromPLC1;
        public string Station5ForTransferScanboxidFromPLC1
        {
            get { return _Station5ForTransferScanboxidFromPLC1; }
            set
            {
                _Station5ForTransferScanboxidFromPLC1 = value;
                OnPropertyChanged("Station5ForTransferScanboxidFromPLC1");
            }
        }
        private string _Station8ForTransferScanboxid;
        public string Station8ForTransferScanboxid
        {
            get { return _Station8ForTransferScanboxid; }
            set
            {
                _Station8ForTransferScanboxid = value;
                OnPropertyChanged("Station8ForTransferScanboxid");
            }
        }
        private string _Station8ForHotLotBoxid;
        public string Station8ForHotLotBoxid
        {
            get { return _Station8ForHotLotBoxid; }
            set
            {
                _Station8ForHotLotBoxid = value;
                OnPropertyChanged("Station8ForHotLotBoxid");
            }
        }
        private string _result;
        public string result
        {
            get { return _result; }
            set
            {
                _result = value;
                OnPropertyChanged("result");
            }
        }


        private string _ERCode;
        public string ERCode
        {
            get { return _ERCode; }
            set
            {
                _ERCode = value;
                OnPropertyChanged("ERCode");
            }
        }

        private string _resultCode;
        public string resultCode
        {
            get { return _resultCode; }
            set
            {
                _resultCode = value;
                OnPropertyChanged("resultCode");
            }
        }


     

      private string _resultst4;
        public string resultst4 
        {
            get { return _resultst4; }
            set
            {
                _resultst4 = value;
                OnPropertyChanged("resultst4");
            }
        }


        private string _ERCodest4;
        public string ERCodest4
        {
            get { return _ERCodest4; }
            set
            {
                _ERCodest4 = value;
                OnPropertyChanged("ERCodest4");
            }
        }




       private string _ERCodest6;
        public string ERCodest6
        {
            get { return _ERCodest6; }
            set
            {
                _ERCodest6 = value;
                OnPropertyChanged("ERCodest6");
            }
        }



        private string _RJReasonForSealer;
        public string RJReasonForSealer
        {
            get { return _RJReasonForSealer; }
            set
            {
                _RJReasonForSealer = value;
                OnPropertyChanged("RJReasonForSealer");
            }
        }







        private string _Station8TrackLabel1;
        public string Station8TrackLabel1
        {
            get { return _Station8TrackLabel1; }
            set
            {
                _Station8TrackLabel1 = value;
                OnPropertyChanged("Station8TrackLabel1");
            }
        }
        private string _Station8TrackLabel2;
        public string Station8TrackLabel2
        {
            get { return _Station8TrackLabel2; }
            set
            {
                _Station8TrackLabel2 = value;
                OnPropertyChanged("Station8TrackLabel2");
            }
        }
        private string _Station6TrackLabelForReject;
        public string Station6TrackLabelForReject
        {
            get { return _Station6TrackLabelForReject; }
            set
            {
                _Station6TrackLabelForReject = value;
                OnPropertyChanged("Station6TrackLabelForReject");
            }
        }
        private string _Station8TrackLabel4;
        public string Station8TrackLabel4
        {
            get { return _Station8TrackLabel4; }
            set
            {
                _Station8TrackLabel4 = value;
                OnPropertyChanged("Station8TrackLabel4");
            }
        }
        private string _Station8TrackLabel5;
        public string Station8TrackLabel5
        {
            get { return _Station8TrackLabel5; }
            set
            {
                _Station8TrackLabel5 = value;
                OnPropertyChanged("Station8TrackLabel5");
            }
        }
        private string _Station8TrackLabel6;
        public string Station8TrackLabel6
        {
            get { return _Station8TrackLabel6; }
            set
            {
                _Station8TrackLabel6 = value;
                OnPropertyChanged("Station8TrackLabel6");
            }
        }
        private string _Station8TrackLabel7;
        public string Station8TrackLabel7
        {
            get { return _Station8TrackLabel7; }
            set
            {
                _Station8TrackLabel7 = value;
                OnPropertyChanged("Station8TrackLabel7");
            }
        }
        private string _Station8TrackLabel8;
        public string Station8TrackLabel8
        {
            get { return _Station8TrackLabel8; }
            set
            {
                _Station8TrackLabel8 = value;
                OnPropertyChanged("Station8TrackLabel8");
            }
        }
        private string _Station8FinishLabel1;
        public string Station8FinishLabel1
        {
            get { return _Station8FinishLabel1; }
            set
            {
                _Station8FinishLabel1 = value;
                OnPropertyChanged("Station8FinishLabel1");
            }
        }
        private string _Station8FinishLabel2;
        public string Station8FinishLabel2
        {
            get { return _Station8FinishLabel2; }
            set
            {
                _Station8FinishLabel2 = value;
                OnPropertyChanged("Station8FinishLabel2");
            }
        }
        private string _Station8FinishLabel3;
        public string Station8FinishLabel3
        {
            get { return _Station8FinishLabel3; }
            set
            {
                _Station8FinishLabel3 = value;
                OnPropertyChanged("Station8FinishLabel3");
            }
        }


        private string _Station6FinishLabelForReject;
        public string Station6FinishLabelForReject
        {
            get { return _Station6FinishLabelForReject; }
            set
            {
                _Station6FinishLabelForReject = value;
                OnPropertyChanged("Station6FinishLabelForReject");
            }
        }

        private string _St6CheckFL;
        public string St6CheckFL
        {
            get
            {
                return _St6CheckFL;
            }
            set
            {
                _St6CheckFL = value;
                OnPropertyChanged("St6CheckFL");
            }
        }

       private string _St5CheckFL;
        public string St5CheckFL
        {
            get
            {
                return _St5CheckFL;
            }
            set
            {
                _St5CheckFL = value;
                OnPropertyChanged("St5CheckFL");
            }
        }


      private string _OutputTranspoterFL;
        public string OutputTranspoterFL
        {
            get { return _OutputTranspoterFL; }
            set
            {
                _OutputTranspoterFL = value;
                OnPropertyChanged("OutputTranspoterFL");
            }
        }






        private string _Station8FinishLabel4;
        public string Station8FinishLabel4
        {
            get { return _Station8FinishLabel4; }
            set
            {
                _Station8FinishLabel4 = value;
                OnPropertyChanged("Station8FinishLabel4");
            }
        }
        private string _Station8FinishLabel5;
        public string Station8FinishLabel5
        {
            get { return _Station8FinishLabel5; }
            set
            {
                _Station8FinishLabel5 = value;
                OnPropertyChanged("Station8FinishLabel5");
            }
        }
        private string _Station8FinishLabel6;
        public string Station8FinishLabel6
        {
            get { return _Station8FinishLabel6; }
            set
            {
                _Station8FinishLabel6 = value;
                OnPropertyChanged("Station8FinishLabel6");
            }
        }
        private string _Station8FinishLabel7;
        public string Station8FinishLabel7
        {
            get { return _Station8FinishLabel7; }
            set
            {
                _Station8FinishLabel7 = value;
                OnPropertyChanged("Station8FinishLabel7");
            }
        }
        private string _Station8FinishLabel8;
        public string Station8FinishLabel8
        {
            get { return _Station8FinishLabel8; }
            set
            {
                _Station8FinishLabel8 = value;
                OnPropertyChanged("Station8FinishLabel8");
            }
        }


      private string _Station7ESDScanboxid;
        public string Station7ESDScanboxid
        {
            get { return _Station7ESDScanboxid; }
            set
            {
                _Station7ESDScanboxid = value;
                OnPropertyChanged("Station7ESDScanboxid");
            }
        }







        private string _Station7ForTransferScanboxidFromPLC1;
        public string Station7ForTransferScanboxidFromPLC1
        {
            get { return _Station7ForTransferScanboxidFromPLC1; }
            set
            {
                _Station7ForTransferScanboxidFromPLC1 = value;
                OnPropertyChanged("Station7ForTransferScanboxidFromPLC1");
            }
        }


      private string _Station7ScanboxidForReject;
        public string Station7ScanboxidForReject
        {
            get { return _Station7ScanboxidForReject; }
            set
            {
                _Station7ScanboxidForReject = value;
                OnPropertyChanged("Station7ScanboxidForReject");
            }
        }

      private string _Sealer1ID;
        public string Sealer1ID
        {
            get { return _Sealer1ID; }
            set
            {
                _Sealer1ID = value;
                OnPropertyChanged("Sealer1ID");
            }
        }
       private string _Sealer1IDshow;
        public string Sealer1IDshow
        {
            get { return _Sealer1IDshow; }
            set
            {
                _Sealer1IDshow = value;
                OnPropertyChanged("Sealer1IDshow");
            }
        }

       private string _Sealer2IDshow;
        public string Sealer2IDshow
        {
            get { return _Sealer2IDshow; }
            set
            {
                _Sealer2IDshow = value;
                OnPropertyChanged("Sealer2IDshow");
            }
        }

       private string _Sealer2ID;
        public string Sealer2ID
        {
            get { return _Sealer2ID; }
            set
            {
                _Sealer2ID = value;
                OnPropertyChanged("Sealer2ID");
            }
        }




       private string _Sealer3IDshow;
        public string Sealer3IDshow
        {
            get { return _Sealer3IDshow; }
            set
            {
                _Sealer3IDshow = value;
                OnPropertyChanged("Sealer3IDshow");
            }
        }


      private string _Sealer3ID;
        public string Sealer3ID
        {
            get { return _Sealer3ID; }
            set
            {
                _Sealer3ID = value;
                OnPropertyChanged("Sealer3ID");
            }
        }


        private string _Station5ForTransferScanboxidFromPLC2;
        public string Station5ForTransferScanboxidFromPLC2
        {
            get { return _Station5ForTransferScanboxidFromPLC2; }
            set
            {
                _Station5ForTransferScanboxidFromPLC2 = value;
                OnPropertyChanged("Station5ForTransferScanboxidFromPLC2");
            }
        }



       private string _Station5ForSealer1HICYESNO;
        public string Station5ForSealer1HICYESNO
        {
            get { return _Station5ForSealer1HICYESNO; }
            set
            {
                _Station5ForSealer1HICYESNO = value;
                OnPropertyChanged("Station5ForSealer1HICYESNO");
            }
        }
       private string _Station5ForSealer2HICYESNO;
        public string Station5ForSealer2HICYESNO
        {
            get { return _Station5ForSealer2HICYESNO; }
            set
            {
                _Station5ForSealer2HICYESNO = value;
                OnPropertyChanged("Station5ForSealer2HICYESNO");
            }
        }

       private string _Station5ForSealer3HICYESNO;
        public string Station5ForSealer3HICYESNO
        {
            get { return _Station5ForSealer3HICYESNO; }
            set
            {
                _Station5ForSealer3HICYESNO = value;
                OnPropertyChanged("Station5ForSealer3HICYESNO");
            }
        }





        private string _Station5ForSealer1Scanboxid;
        public string Station5ForSealer1Scanboxid
        {
            get { return _Station5ForSealer1Scanboxid; }
            set
            {
                _Station5ForSealer1Scanboxid = value;
                OnPropertyChanged("Station5ForSealer1Scanboxid");
            }
        }
        private string _Station5ForSealer2Scanboxid;
        public string Station5ForSealer2Scanboxid
        {
            get { return _Station5ForSealer2Scanboxid; }
            set
            {
                _Station5ForSealer2Scanboxid = value;
                OnPropertyChanged("Station5ForSealer2Scanboxid");
            }
        }
        private string _Station5ForSealer3Scanboxid;
        public string Station5ForSealer3Scanboxid
        {
            get { return _Station5ForSealer3Scanboxid; }
            set
            {
                _Station5ForSealer3Scanboxid = value;
                OnPropertyChanged("Station5ForSealer3Scanboxid");
            }
        }
        private string _Operator1Errmessage;
        public string Operator1Errmessage
        {
            get { return _Operator1Errmessage; }
            set
            {
                _Operator1Errmessage = value;
                OnPropertyChanged("Operator1Errmessage");
            }
        }
        private string _Operator2Errmessage;
        public string Operator2Errmessage
        {
            get { return _Operator2Errmessage; }
            set
            {
                _Operator2Errmessage = value;
                OnPropertyChanged("Operator2Errmessage");
            }
        }
        private string _Operator1ErrCode;
        public string Operator1ErrCode
        {
            get { return _Operator1ErrCode; }
            set
            {
                _Operator1ErrCode = value;
                OnPropertyChanged("Operator1ErrCode");
            }
        }
        private string _Operator2ErrCode;
        public string Operator2ErrCode
        {
            get { return _Operator2ErrCode; }
            set
            {
                _Operator2ErrCode = value;
                OnPropertyChanged("Operator2ErrCode");
            }
        }


        private string _ErrCode7;
        public string ErrCode7
        {
            get { return _ErrCode7; }
            set
            {
                _ErrCode7 = value;
                OnPropertyChanged("ErrCode7");
            }
        }


        private string _Errmessage7;
        public string Errmessage7
        {
            get { return _Errmessage7; }
            set
            {
                _Errmessage7 = value;
                OnPropertyChanged("Errmessage7");
            }
        }


        private string _ErrCode3;
        public string ErrCode3
        {
            get { return _ErrCode3; }
            set
            {
                _ErrCode3 = value;
                OnPropertyChanged("ErrCode3");
            }
        }


        private string _Errmessage3;
        public string Errmessage3
        {
            get { return _Errmessage3; }
            set
            {
                _Errmessage3 = value;
                OnPropertyChanged("Errmessage3");
                OnPropertyChanged("Station1GotError");
            }
        }

        public bool Station3GotError
        {
            get
            {
                if (_Errmessage3 != String.Empty)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        private string _ErrCode3_1;
        public string ErrCode3_1
        {
            get { return _ErrCode3_1; }
            set
            {
                _ErrCode3_1 = value;
                OnPropertyChanged("ErrCode3_1");
            }
        }


        private string _Errmessage3_1;
        public string Errmessage3_1
        {
            get { return _Errmessage3_1; }
            set
            {
                _Errmessage3_1 = value;
                OnPropertyChanged("Errmessage3_1");
            }
        }




        private string _ErrCode3_2;
        public string ErrCode3_2
        {
            get { return _ErrCode3_2; }
            set
            {
                _ErrCode3_2 = value;
                OnPropertyChanged("ErrCode3_2");
            }
        }


        private string _Errmessage3_2;
        public string Errmessage3_2
        {
            get { return _Errmessage3_2; }
            set
            {
                _Errmessage3_2 = value;
                OnPropertyChanged("Errmessage3_2");
            }
        }





        private string _ErrCode1;
        public string ErrCode1
        {
            get { return _ErrCode1; }
            set
            {
                _ErrCode1 = value;
                OnPropertyChanged("ErrCode1");
            }
        }


        private string _Errmessage1;
        public string Errmessage1
        {
            get { return _Errmessage1; }
            set
            {
                _Errmessage1 = value;
                OnPropertyChanged("Errmessage1");
            }
        }

        private string _ErrCode1_1;
        public string ErrCode1_1
        {
            get { return _ErrCode1_1; }
            set
            {
                _ErrCode1_1 = value;
                OnPropertyChanged("ErrCode1_1");
            }
        }


        private string _Errmessage1_1;
        public string Errmessage1_1
        {
            get { return _Errmessage1_1; }
            set
            {
                _Errmessage1_1 = value;
                OnPropertyChanged("Errmessage1_1");
            }
        }




        private string _ErrCode1_2;
        public string ErrCode1_2
        {
            get { return _ErrCode1_2; }
            set
            {
                _ErrCode1_2 = value;
                OnPropertyChanged("ErrCode1_2");
            }
        }


        private string _Errmessage1_2;
        public string Errmessage1_2
        {
            get { return _Errmessage1_2; }
            set
            {
                _Errmessage1_2 = value;
                OnPropertyChanged("Errmessage1_2");
            }
        }



        private string _ErrCode4;
        public string ErrCode4
        {
            get { return _ErrCode4; }
            set
            {
                _ErrCode4 = value;
                OnPropertyChanged("ErrCode4");
            }
        }


        private string _Errmessage4;
        public string Errmessage4
        {
            get { return _Errmessage4; }
            set
            {
                _Errmessage4 = value;
                OnPropertyChanged("Errmessage4");
            }
        }

        private string _ErrCode4_1;
        public string ErrCode4_1
        {
            get { return _ErrCode4_1; }
            set
            {
                _ErrCode4_1 = value;
                OnPropertyChanged("ErrCode4_1");
            }
        }


        private string _Errmessage4_1;
        public string Errmessage4_1
        {
            get { return _Errmessage4_1; }
            set
            {
                _Errmessage4_1 = value;
                OnPropertyChanged("Errmessage4_1");
            }
        }




        private string _ErrCode4_2;
        public string ErrCode4_2
        {
            get { return _ErrCode4_2; }
            set
            {
                _ErrCode4_2 = value;
                OnPropertyChanged("ErrCode4_2");
            }
        }


        private string _Errmessage4_2;
        public string Errmessage4_2
        {
            get { return _Errmessage4_2; }
            set
            {
                _Errmessage4_2 = value;
                OnPropertyChanged("Errmessage4_2");
            }
        }



        private string _ErrCode2;
        public string ErrCode2
        {
            get { return _ErrCode2; }
            set
            {
                _ErrCode2 = value;
                OnPropertyChanged("ErrCode2");
            }
        }


        private string _Errmessage2;
        public string Errmessage2
        {
            get { return _Errmessage2; }
            set
            {
                _Errmessage2 = value;
                OnPropertyChanged("Errmessage2");
            }
        }

        private string _ErrCode2_1;
        public string ErrCode2_1
        {
            get { return _ErrCode2_1; }
            set
            {
                _ErrCode2_1 = value;
                OnPropertyChanged("ErrCode2_1");
            }
        }


        private string _Errmessage2_1;
        public string Errmessage2_1
        {
            get { return _Errmessage2_1; }
            set
            {
                _Errmessage2_1 = value;
                OnPropertyChanged("Errmessage2_1");
            }
        }




        private string _ErrCode2_2;
        public string ErrCode2_2
        {
            get { return _ErrCode2_2; }
            set
            {
                _ErrCode2_2 = value;
                OnPropertyChanged("ErrCode2_2");
            }
        }


        private string _Errmessage2_2;
        public string Errmessage2_2
        {
            get { return _Errmessage2_2; }
            set
            {
                _Errmessage2_2 = value;
                OnPropertyChanged("Errmessage2_2");
            }
        }




        private string _ErrCode2_3;
        public string ErrCode2_3
        {
            get { return _ErrCode2_3; }
            set
            {
                _ErrCode2_3 = value;
                OnPropertyChanged("ErrCode2_3");
            }
        }


        private string _Errmessage2_3;
        public string Errmessage2_3
        {
            get { return _Errmessage2_3; }
            set
            {
                _Errmessage2_3 = value;
                OnPropertyChanged("Errmessage2_3");
            }
        }

        private string _ErrCode2_4;
        public string ErrCode2_4
        {
            get { return _ErrCode2_4; }
            set
            {
                _ErrCode2_4 = value;
                OnPropertyChanged("ErrCode2_4");
            }
        }


        private string _Errmessage2_4;
        public string Errmessage2_4
        {
            get { return _Errmessage2_4; }
            set
            {
                _Errmessage2_4 = value;
                OnPropertyChanged("Errmessage2_4");
            }
        }




        private string _ErrCode2_5;
        public string ErrCode2_5
        {
            get { return _ErrCode2_5; }
            set
            {
                _ErrCode2_5 = value;
                OnPropertyChanged("ErrCode2_5");
            }
        }


        private string _Errmessage2_5;
        public string Errmessage2_5
        {
            get { return _Errmessage2_5; }
            set
            {
                _Errmessage2_5 = value;
                OnPropertyChanged("Errmessage2_5");
            }
        }





        private string _ErrCode2_6;
        public string ErrCode2_6
        {
            get { return _ErrCode2_6; }
            set
            {
                _ErrCode2_6 = value;
                OnPropertyChanged("ErrCode2_6");
            }
        }


        private string _Errmessage2_6;
        public string Errmessage2_6
        {
            get { return _Errmessage2_6; }
            set
            {
                _Errmessage2_6 = value;
                OnPropertyChanged("Errmessage2_6");
            }
        }




        private string _ErrCode2_7;
        public string ErrCode2_7
        {
            get { return _ErrCode2_7; }
            set
            {
                _ErrCode2_7 = value;
                OnPropertyChanged("ErrCode2_7");
            }
        }


        private string _Errmessage2_7;
        public string Errmessage2_7
        {
            get { return _Errmessage2_7; }
            set
            {
                _Errmessage2_7 = value;
                OnPropertyChanged("Errmessage2_7");
            }
        }




























        private string _ErrCode8;
        public string ErrCode8
        {
            get { return _ErrCode8; }
            set
            {
                _ErrCode8 = value;
                OnPropertyChanged("ErrCode8");
            }
        }


        private string _Errmessage8;
        public string Errmessage8
        {
            get { return _Errmessage8; }
            set
            {
                _Errmessage8 = value;
                OnPropertyChanged("Errmessage8");
            }
        }




        private string _ErrCode5;
        public string ErrCode5
        {
            get { return _ErrCode5; }
            set
            {
                _ErrCode5 = value;
                OnPropertyChanged("ErrCode5");
            }
        }











        private string _Errmessage5;
        public string Errmessage5
        {
            get { return _Errmessage5; }
            set
            {
                _Errmessage5 = value;
                OnPropertyChanged("Errmessage5");
            }
        }

        private string _ErrCode5_1;
        public string ErrCode5_1
        {
            get { return _ErrCode5_1; }
            set
            {
                _ErrCode5_1 = value;
                OnPropertyChanged("ErrCode5_1");
            }
        }






        private string _Errmessage5_1;
        public string Errmessage5_1
        {
            get { return _Errmessage5_1; }
            set
            {
                _Errmessage5_1 = value;
                OnPropertyChanged("Errmessage5_1");
            }
        }



       private string _ErrCode5_2;
        public string ErrCode5_2
        {
            get { return _ErrCode5_2; }
            set
            {
                _ErrCode5_2 = value;
                OnPropertyChanged("ErrCode5_2");
            }
        }
        private string _Errmessage5_2;
        public string Errmessage5_2
        {
            get { return _Errmessage5_2; }
            set
            {
                _Errmessage5_2 = value;
                OnPropertyChanged("Errmessage5_2");
            }
        }


        private string _Station6ForOP1Scanboxid;
        public string Station6ForOP1Scanboxid
        {
            get { return _Station6ForOP1Scanboxid; }
            set
            {
                _Station6ForOP1Scanboxid = value;
                OnPropertyChanged("Station6ForOP1Scanboxid");
            }
        }
        //private string _st6finish1;
        //public string st6finish1
        //{
        //    get { return _st6finish1; }
        //    set
        //    {
        //        _st6finish1 = value;
        //        OnPropertyChanged("st6finish1");
        //    }
        //}
        private string _st6finish1;
        public string st6finish1
        {
            get { return _st6finish1; }
            set
            {
                _st6finish1 = value;
                OnPropertyChanged("st6finish1");
            }
        }
        private string _st6finish2;
        public string st6finish2
        {
            get { return _st6finish2; }
            set
            {
                _st6finish2 = value;
                OnPropertyChanged("st6finish2");
            }
        }
        private string _station6track1;
        public string station6track1
        {
            get { return _station6track1; }
            set
            {
                _station6track1 = value;
                OnPropertyChanged("station6track1");
            }
        }
        private string _station6track2;
        public string station6track2
        {
            get { return _station6track2; }
            set
            {
                _station6track2 = value;
                OnPropertyChanged("station6track2");
            }
        }


      private string _st8trackForAutotimeoutReject;
        public string st8trackForAutotimeoutReject
        {
            get { return _st8trackForAutotimeoutReject; }
            set
            {
                _st8trackForAutotimeoutReject = value;
                OnPropertyChanged("st8trackForAutotimeoutReject");
            }
        }




        private string _st7trackForBuffer;
        public string st7trackForBuffer
        {
            get { return _st7trackForBuffer; }
            set
            {
                _st7trackForBuffer = value;
                OnPropertyChanged("st7trackForBuffer");
            }
        }

        private string _st7FlabelForBuffer;
        public string st7FlabelForBuffer
        {
            get { return _st7FlabelForBuffer; }
            set
            {
                _st7FlabelForBuffer = value;
                OnPropertyChanged("st7FlabelForBuffer");
            }
        }



       private string _st7ESDtrack;
        public string st7ESDtrack
        {
            get { return _st7ESDtrack; }
            set
            {
                _st7ESDtrack = value;
                OnPropertyChanged("st7ESDtrack");
            }
        }

        private string _st7track;
        public string st7track
        {
            get { return _st7track; }
            set
            {
                _st7track = value;
                OnPropertyChanged("st7track");
            }
        }
        private string _st7Flabel;
        public string st7Flabel
        {
            get { return _st7Flabel; }
            set
            {
                _st7Flabel = value;
                OnPropertyChanged("st7Flabel");
            }
        }
        private string _st8track;
        public string st8track
        {
            get { return _st8track; }
            set
            {
                _st8track = value;
                OnPropertyChanged("st8track");
            }
        }
        private string _st8trackHotLot;
        public string st8trackHotLot
        {
            get { return _st8trackHotLot; }
            set
            {
                _st8trackHotLot = value;
                OnPropertyChanged("st8trackHotlot");
            }
        }
        private string _st8FLabelHotlot;
        public string st8FLabelHotlot
        {
            get { return _st8FLabelHotlot; }
            set
            {
                _st8FLabelHotlot = value;
                OnPropertyChanged("st8trackHotlot");
            }
        }
        private string _Statusst8;
        public string Statusst8
        {
            get { return _Statusst8; }
            set
            {
                _Statusst8 = value;
                OnPropertyChanged("Statusst8");
            }
        }




        private string _st8track1;
        public string st8track1
        {
            get { return _st8track1; }
            set
            {
                _st8track1 = value;
                OnPropertyChanged("st8track1");
            }
        }
        private string _st8track2;
        public string st8track2
        {
            get { return _st8track2; }
            set
            {
                _st8track2 = value;
                OnPropertyChanged("st8track2");
            }
        }
        private string _st8track3;
        public string st8track3
        {
            get { return _st8track3; }
            set
            {
                _st8track3 = value;
                OnPropertyChanged("st8track3");
            }
        }


        private string _st6RejectTrack;
        public string st6RejectTrack
        {
            get { return _st6RejectTrack; }
            set
            {
                _st6RejectTrack = value;
                OnPropertyChanged("st6RejectTrack");
            }
        }

        private string _st8track4;
        public string st8track4
        {
            get { return _st8track4; }
            set
            {
                _st8track4 = value;
                OnPropertyChanged("st8track4");
            }
        }
        private string _st8track5;
        public string st8track5
        {
            get { return _st8track5; }
            set
            {
                _st8track5 = value;
                OnPropertyChanged("st8track5");
            }
        }
        private string _st8track6;
        public string st8track6
        {
            get { return _st8track6; }
            set
            {
                _st8track6 = value;
                OnPropertyChanged("st8track6");
            }
        }
        private string _st8track7;
        public string st8track7
        {
            get { return _st8track7; }
            set
            {
                _st8track7 = value;
                OnPropertyChanged("st8track7");
            }
        }
        private string _st8track8;
        public string st8track8
        {
            get { return _st8track8; }
            set
            {
                _st8track8 = value;
                OnPropertyChanged("st8track8");
            }
        }
        private string _st8FINISH;
        public string st8FINISH
        {
            get { return _st8FINISH; }
            set
            {
                _st8FINISH = value;
                OnPropertyChanged("st8FINISH");
            }
        }
        private string _st8FLabel;
        public string st8FLabel
        {
            get { return _st8FLabel; }
            set
            {
                _st8FLabel = value;
                OnPropertyChanged("st8FLabel");
            }
        }
        private string _Station6ForOP2Scanboxid;
        public string Station6ForOP2Scanboxid
        {
            get { return _Station6ForOP2Scanboxid; }
            set
            {
                _Station6ForOP2Scanboxid = value;
                OnPropertyChanged("Station6ForOP2Scanboxid");
            }
        }
        private string _Station6ForOP1TrackingLabel;
        public string Station6ForOP1TrackingLabel
        {
            get { return _Station6ForOP1TrackingLabel; }
            set
            {
                _Station6ForOP1TrackingLabel = value;
                OnPropertyChanged("Station6ForOP1TrackingLabel");
            }
        }
        private string _Station6ForOP2TrackingLabel;
        public string Station6ForOP2TrackingLabel
        {
            get { return _Station6ForOP2TrackingLabel; }
            set
            {
                _Station6ForOP2TrackingLabel = value;
                OnPropertyChanged("Station6ForOP2TrackingLabel");
            }
        }
        #endregion
        ManualResetEvent evt_QCStation01Logout;
        ManualResetEvent evt_QCStation02Logout;
        ManualResetEvent evt_ErrorStation01;
        ManualResetEvent evt_ErrorStation02;
        ManualResetEvent evt_ErrorStation03;
        ManualResetEvent evt_ErrorStation04;
        ManualResetEvent evt_ErrorStation05;
        ManualResetEvent evt_ErrorStation06_1;
        ManualResetEvent evt_ErrorStation06_2;
        ManualResetEvent evt_ErrorStation07;
        ManualResetEvent evt_ErrorStation08;
        ManualResetEvent evt_AQL_Msg;
        ManualResetEvent evt_EvtStation01;
        ManualResetEvent evt_EvtStation02;
        ManualResetEvent evt_EvtStation03;
        ManualResetEvent evt_EvtStation04;
        ManualResetEvent evt_EvtStation05;
        ManualResetEvent evt_EvtStation06_1;
        ManualResetEvent evt_EvtStation06_2;
        ManualResetEvent evt_EvtStation07;
        ManualResetEvent evt_EvtStation08;
        ManualResetEvent evt_PARAM_ScreenAccess;//technician logout
        #endregion
        //ADD
        Logger log = LogManager.GetLogger("NetworkThread");
        Logger OM1 = LogManager.GetLogger("Vision Check Station 2");
        Logger OM2 = LogManager.GetLogger("Vision Check Station 4");
        Logger OM3 = LogManager.GetLogger("Vision Check Station 7");
        Logger log6 = LogManager.GetLogger("Station6 Operator 1 ");
        Logger log6_1 = LogManager.GetLogger("Station6 Operator 2 ");
         DBUtils dbUtils = new DBUtils();
        //   XmlDocument FinishingLabelsInfo;// I should put this in mainnetworkclass... 
        public void shutdown()
        {
            bTerminate = true;
            Nkthread.Join();
            PLC01Thread.Join();
            PLC02Thread.Join();
            MiddleWareThread.Join();
            Station06Operator01Thread.Join();
            Station06Operator02Thread.Join();
            Station02Thread.Join();
            Station02PrinterThread.Join();
            Station04PrinterThread.Join();
            Station04ScannerThread.Join();
            Station04PrinterclearThread.Join();
            Station07PrinterclearThread.Join();
            Station02PrinterclearRAThread.Join();
           // Station02PrinterclearRBThread.Join();
           // Station02PrinterclearRCThread.Join();
            UploadhandScannerDataThread.Join();
            Station07PrinterESDThread.Join();
            Station07PrinterThread.Join();
            Station05VacuumSealersThread.Join();
            Station05VacuumSealer1Thread.Join();
            Station05VacuumSealer2Thread.Join();
            Station05VacuumSealer3Thread.Join();
            Station02LabelInspectionThread.Join();
            Station04LabelInspectionThread.Join();
            Station07LabelInspectionThread.Join();//added
            Station6_7_8Thread.Join();
            Environment.Exit(0);
        }
        public void NkThreadInit()
        {
           
            evt_FinishLabelRequestComplete = new ManualResetEvent(false);
            evt_FinishLabelRequest = new ManualResetEvent(false);
            evt_FinishLabelRequestSt4 = new ManualResetEvent(false);
            evnt_FindFinishingLabelForOperator = new ManualResetEvent(false);
            evnt_FindFinishingLabelForOperator2 = new ManualResetEvent(false);
            evnt_TrackingLabelForOperator = new ManualResetEvent(false);
            evnt_TrackingLabelForOperator2 = new ManualResetEvent(false);
            evnt_ScannerForOperator = new ManualResetEvent(false);
            evnt_ScannerForOperator2 = new ManualResetEvent(false);
            evnt_FindFinishingLabelForSealer1 = new ManualResetEvent(false);
            evnt_FindFinishingLabelForSealer2 = new ManualResetEvent(false);
            evnt_FindFinishingLabelForSealer3 = new ManualResetEvent(false);
            evnt_CheckingConnectionForSealer1= new ManualResetEvent(false);
            evnt_CheckingConnectionForSealer2= new ManualResetEvent(false);
            evnt_CheckingConnectionForSealer3= new ManualResetEvent(false);
            evnt_ScannerRetryForOperator1 = new ManualResetEvent(false);
            evnt_ScannerRetryForOperator2 = new ManualResetEvent(false);
            evnt_RejectFinishingLabelForStation4 = new ManualResetEvent(false);
            evnt_RejectFinishingLabelForStation6_OP1 = new ManualResetEvent(false);
            evnt_RejectFinishingLabelForStation6_OP2 = new ManualResetEvent(false);
            evnt_RejectFinishingLabelForStation8 = new ManualResetEvent(false);
            evnt_RejForOperator1 = new ManualResetEvent(false);
            evnt_RejForOperator2 = new ManualResetEvent(false);
            evt_Station02PrintReq = new ManualResetEvent(false);
            evt_Station04PrintReq = new ManualResetEvent(false);
            evt_Station07ESDPrintReq= new ManualResetEvent(false);
            evt_Station07PrintClearReq = new ManualResetEvent(false);
            evt_Station04PrintClearReq = new ManualResetEvent(false);
            evt_Station02PrintClearReqRA = new ManualResetEvent(false);
            evt_Station02PrintClearReqRB = new ManualResetEvent(false);
            evt_Station02PrintClearReqRC = new ManualResetEvent(false);
            evt_Station07PrintReq = new ManualResetEvent(false);
            evt_Station02InspectionReq = new ManualResetEvent(false);
            evt_Station04InspectionReq = new ManualResetEvent(false);
            evt_Station07InspectionReq = new ManualResetEvent(false);
            evt_QCStation01Logout = new ManualResetEvent(false);
            evt_QCStation02Logout = new ManualResetEvent(false);
            evt_ErrorStation01 = new ManualResetEvent(false);
            evt_ErrorStation02 = new ManualResetEvent(false);
            evt_ErrorStation03 = new ManualResetEvent(false);
            evt_ErrorStation04 = new ManualResetEvent(false);
            evt_ErrorStation05 = new ManualResetEvent(false);
            evt_ErrorStation06_1 = new ManualResetEvent(false);
            evt_ErrorStation06_2 = new ManualResetEvent(false);
            evt_ErrorStation07 = new ManualResetEvent(false);
            evt_ErrorStation08 = new ManualResetEvent(false);
            evt_FG01_FG02Move = new ManualResetEvent(false);
            evt_FG01_FG02Move_Rx = new ManualResetEvent(false);
            evt_AQL_Msg = new ManualResetEvent(false);
            evt_EvtStation01 = new ManualResetEvent(false);
            evt_EvtStation02 = new ManualResetEvent(false);
            evt_EvtStation03 = new ManualResetEvent(false);
            evt_EvtStation04 = new ManualResetEvent(false);
            evt_EvtStation05 = new ManualResetEvent(false);
            evt_EvtStation06_1 = new ManualResetEvent(false);
            evt_EvtStation06_2 = new ManualResetEvent(false);
            evt_EvtStation07 = new ManualResetEvent(false);
            evt_EvtStation08 = new ManualResetEvent(false);
            evt_PARAM_ScreenAccess = new ManualResetEvent(false);//technician logout
            networkmain = new MainNetworkClass();
            Nkthread = new Thread(new ParameterizedThreadStart(RunPoll));
            Nkthread.Start(networkmain);

            Station6_7_8Thread=new Thread(new ParameterizedThreadStart(Station6_7_8));
            Station6_7_8Thread.Start(networkmain);
            ParameterChangeThread= new Thread(new ParameterizedThreadStart(ParameterChange));
            ParameterChangeThread.Start(networkmain);
            PLC01Thread = new Thread(new ParameterizedThreadStart(RunPLC01Scan));
            PLC02Thread = new Thread(new ParameterizedThreadStart(RunPLC02Scan));
            MiddleWareThread = new Thread(new ParameterizedThreadStart(RunMiddlewareScan));
            Station06Operator01Thread = new Thread(new ParameterizedThreadStart(RunStation06Operator01Scan));
            Station06Operator02Thread = new Thread(new ParameterizedThreadStart(RunStation06Operator02Scan));
            Station02PrinterThread = new Thread(new ParameterizedThreadStart(Station02PrinterTh));
            Station04PrinterThread = new Thread(new ParameterizedThreadStart(Station04PrinterTh));
            Station04PrinterclearThread = new Thread(new ParameterizedThreadStart(Station04PrinterClearTh));
            Station07PrinterclearThread = new Thread(new ParameterizedThreadStart(Station07PrinterClearTh));

            Station02PrinterclearRAThread = new Thread(new ParameterizedThreadStart(Station02PrinterclearRATh));
           // Station02PrinterclearRBThread = new Thread(new ParameterizedThreadStart(Station02PrinterclearRBTh));
          //  Station02PrinterclearRCThread = new Thread(new ParameterizedThreadStart(Station02PrinterclearRCTh));

            Station07PrinterESDThread = new Thread(new ParameterizedThreadStart(Station07ESDPrinterTh));
            Station07PrinterThread = new Thread(new ParameterizedThreadStart(Station07PrinterTh));
            Station02LabelInspectionThread = new Thread(new ParameterizedThreadStart(ST02Vision01Th));
            Station04LabelInspectionThread = new Thread(new ParameterizedThreadStart(ST02Vision04Th));
            Station07LabelInspectionThread = new Thread(new ParameterizedThreadStart(ST02Vision07Th));
            Station02Thread = new Thread(new ParameterizedThreadStart(RunStation02Scan));
            Station05VacuumSealersThread = new Thread(new ParameterizedThreadStart(RunStation05Sealers));
            Station05VacuumSealersThread.Start(networkmain);


            Station05VacuumSealer1Thread = new Thread(new ParameterizedThreadStart(RunStation05Sealer1));
            Station05VacuumSealer1Thread.Start(networkmain);
          
            Station05VacuumSealer2Thread = new Thread(new ParameterizedThreadStart(RunStation05Sealer2));
            Station05VacuumSealer2Thread.Start(networkmain);

          
            Station05VacuumSealer3Thread = new Thread(new ParameterizedThreadStart(RunStation05Sealer3));
            Station05VacuumSealer3Thread.Start(networkmain);

            UploadhandScannerDataThread = new Thread(new ParameterizedThreadStart(UploadhandScannerData));
            UploadhandScannerDataThread.Start(networkmain);
            Station04ScannerThread = new Thread(new ParameterizedThreadStart(Station04Scanner));


            Station07PrinterESDThread.Start(networkmain);
            Station02PrinterclearRAThread.Start(networkmain);
           // Station02PrinterclearRBThread.Start(networkmain);
           // Station02PrinterclearRCThread.Start(networkmain);
            Station04ScannerThread.Start(networkmain);
            
            Station02Thread.Start(networkmain);
            Station04LabelInspectionThread.Start(networkmain);
            Station02LabelInspectionThread.Start(networkmain);
            Station07LabelInspectionThread.Start(networkmain);
            Station02PrinterThread.Start(networkmain);
            Station04PrinterThread.Start(networkmain);
            Station04PrinterclearThread.Start(networkmain);
            Station07PrinterclearThread.Start(networkmain);
            Station07PrinterThread.Start(networkmain);
            Station06Operator02Thread.Start(networkmain);
            Station06Operator01Thread.Start(networkmain);
            MiddleWareThread.Start(networkmain);
            PLC01Thread.Priority = ThreadPriority.AboveNormal;
            PLC02Thread.Priority = ThreadPriority.AboveNormal;
            PLC01Thread.Start(networkmain);
            PLC02Thread.Start(networkmain);
            //add PLC sending,PC Reading
            PLCQueryCmd = new byte[21];
            PLCQueryCmd[0] = 0x50;
            PLCQueryCmd[1] = 0x00;
            PLCQueryCmd[2] = 0x00;
            PLCQueryCmd[3] = 0xFF;
            PLCQueryCmd[4] = 0xFF;
            PLCQueryCmd[5] = 0x03;
            PLCQueryCmd[6] = 0x00;
            PLCQueryCmd[7] = 0x0C; //number of byte LSB
            PLCQueryCmd[8] = 0x00; //number of byte MSB
            PLCQueryCmd[9] = 0x10;
            PLCQueryCmd[10] = 0x00;
            PLCQueryCmd[11] = 0x01; //read command
            PLCQueryCmd[12] = 0x04; //read command
            PLCQueryCmd[13] = 0x00;
            PLCQueryCmd[14] = 0x00;
            PLCQueryCmd[15] = 0x00; //dm start address DM000
            PLCQueryCmd[16] = 0x00;
            PLCQueryCmd[17] = 0x00;
            PLCQueryCmd[18] = 0xA8; //dm address
            PLCQueryCmd[19] = 0xC8; //200 dm addresses from D000 to D0199
            PLCQueryCmd[20] = 0x00;
            //PLC Receiving ,PC Writting
            PLCQueryRx = new byte[411];//start reading from PLCQueryRX[11];//ie. DM100 //why 411???
            //PLC  Write Setup
            PLCWriteCommandRX = new byte[11]; //expect D000 00 FF FF03 00 0200 0000 ,,, improvement to check on 0200.. 02 is two byte.. anything more than that is an error
            //            PLCWriteCommand = new byte[221];//first 21 is fix by protocol, next 100 *2 is data write
            //            PLCWriteCommand = new byte[621];//first 21 is fix by protocol, next 300 *2 is data write
            PLCWriteCommand = new byte[821];//first 21 is fix by protocol, next 400 *2 is data write
            //500000
            PLCWriteCommand[0] = 0x50;
            PLCWriteCommand[1] = 0x00;
            PLCWriteCommand[2] = 0x00;
            //ffff
            PLCWriteCommand[3] = 0xFF;
            PLCWriteCommand[4] = 0xFF;
            //03001600
            PLCWriteCommand[5] = 0x03;
            PLCWriteCommand[6] = 0x00;
            PLCWriteCommand[7] = 0x2C;//LSB ..total 12 byte + 400 * 2 byte = 812 (32C)
            PLCWriteCommand[8] = 0x03;//number of byte MSB
            //            PLCWriteCommand[7] = 0x64;//LSB ..total 12 byte + 300 * 2 byte = 612 (264)
            //          PLCWriteCommand[8] = 0x02;//number of byte MSB
            //            PLCWriteCommand[7] = 0xD4;//LSB ..total 12 byte + 100 * 2 byte = 212 (D4)
            //            PLCWriteCommand[8] = 0x00;//number of byte MSB
            //1000
            PLCWriteCommand[9] = 0x10;
            PLCWriteCommand[10] = 0x00;
            //0114
            PLCWriteCommand[11] = 0x01;//batch write command
            PLCWriteCommand[12] = 0x14;
            //0000
            PLCWriteCommand[13] = 0x00;
            PLCWriteCommand[14] = 0x00;
            //640000
            PLCWriteCommand[15] = 0xC8;//DM200
            PLCWriteCommand[16] = 0x00;
            PLCWriteCommand[17] = 0x00;
            //a8
            PLCWriteCommand[18] = 0xA8;
            //0500 number of data 5
            PLCWriteCommand[19] = 0x90;//number of data 400 DM
            PLCWriteCommand[20] = 0x01;
            #region PLCReadWrite For PLC2
            PLCQueryCmd6 = new byte[21];
            PLCQueryCmd6[0] = 0x50;
            PLCQueryCmd6[1] = 0x00;
            PLCQueryCmd6[2] = 0x00;
            PLCQueryCmd6[3] = 0xFF;
            PLCQueryCmd6[4] = 0xFF;
            PLCQueryCmd6[5] = 0x03;
            PLCQueryCmd6[6] = 0x00;
            PLCQueryCmd6[7] = 0x0C; //number of byte LSB
            PLCQueryCmd6[8] = 0x00; //number of byte MSB
            PLCQueryCmd6[9] = 0x10;
            PLCQueryCmd6[10] = 0x00;
            PLCQueryCmd6[11] = 0x01; //read command
            PLCQueryCmd6[12] = 0x04; //read command
            PLCQueryCmd6[13] = 0x00;
            PLCQueryCmd6[14] = 0x00;
            PLCQueryCmd6[15] = 0x88; //dm start address DM5000 =1388 lsb
            PLCQueryCmd6[16] = 0x13;  //msb
            PLCQueryCmd6[17] = 0x00;
            PLCQueryCmd6[18] = 0xA8; //dm address
            PLCQueryCmd6[19] = 0xC8; //200 dm addresses from D5000 to D5199
            PLCQueryCmd6[20] = 0x00;
            PLCQueryRx6 = new byte[411];//start reading from PLCQueryRX[11];//ie. DM5000
            //PLC  Write Setup
            PLCWriteCommandRX6 = new byte[11];
            PLCWriteCommand6 = new byte[821];
            //500000
            PLCWriteCommand6[0] = 0x50;
            PLCWriteCommand6[1] = 0x00;
            PLCWriteCommand6[2] = 0x00;
            //ffff
            PLCWriteCommand6[3] = 0xFF;
            PLCWriteCommand6[4] = 0xFF;
            //03001600
            PLCWriteCommand6[5] = 0x03;
            PLCWriteCommand6[6] = 0x00;
            PLCWriteCommand6[7] = 0x2C;//LSB ..total 12 byte + 400 * 2 byte = 812 (32C)
            PLCWriteCommand6[8] = 0x03;//number of byte MSB
            //1000
            PLCWriteCommand6[9] = 0x10;
            PLCWriteCommand6[10] = 0x00;
            ////0114
            PLCWriteCommand6[11] = 0x01;//batch write command
            PLCWriteCommand6[12] = 0x14;
            //0000
            PLCWriteCommand6[13] = 0x00;
            PLCWriteCommand6[14] = 0x00;
            //640000
            PLCWriteCommand6[15] = 0x50;//DM5200 =1450
            PLCWriteCommand6[16] = 0x14;
            PLCWriteCommand6[17] = 0x00;
            //a8
            PLCWriteCommand6[18] = 0xA8;
            //0500 number of data 5
            PLCWriteCommand6[19] = 0x90;//number of data 400 DM
            PLCWriteCommand6[20] = 0x01;
            #endregion
            #region PLC1ReadWrite For Barcode
            //PLCQueryCmd7 = new byte[21];
            //PLCQueryCmd7[0] = 0x50;
            //PLCQueryCmd7[1] = 0x00;
            //PLCQueryCmd7[2] = 0x00;
            //PLCQueryCmd7[3] = 0xFF;
            //PLCQueryCmd7[4] = 0xFF;
            //PLCQueryCmd7[5] = 0x03;
            //PLCQueryCmd7[6] = 0x00;
            //PLCQueryCmd7[7] = 0x0C; //number of byte LSB
            //PLCQueryCmd7[8] = 0x00; //number of byte MSB
            //PLCQueryCmd7[9] = 0x10;
            //PLCQueryCmd7[10] = 0x00;
            //PLCQueryCmd7[11] = 0x01; //read command
            //PLCQueryCmd7[12] = 0x04; //read command
            //PLCQueryCmd7[13] = 0x00;
            //PLCQueryCmd7[14] = 0x00;
            //PLCQueryCmd7[15] = 0xBC; //dm start address DM700 =02BC lsb
            //PLCQueryCmd7[16] = 0x02;  //msb
            //PLCQueryCmd7[17] = 0x00;
            ////750=2EE
            ////700=2BC
            //400=
            ////300=012C
            ////200=00C8
            //PLCQueryCmd7[18] = 0xA8; //200= data size address
            //PLCQueryCmd7[19] = 0x2C; //300 dm addresses from D700 to D999
            //PLCQueryCmd7[20] = 0x01;
            //PLCQueryRx7 = new byte[611];//start reading from PLCQueryRX[11];//ie. DM700
            #endregion


            #region PLC1Read For Parameter

            PLCQueryCmdPara = new byte[21];
            PLCQueryCmdPara[0] = 0x50;
            PLCQueryCmdPara[1] = 0x00;
            PLCQueryCmdPara[2] = 0x00;
            PLCQueryCmdPara[3] = 0xFF;
            PLCQueryCmdPara[4] = 0xFF;
            PLCQueryCmdPara[5] = 0x03;
            PLCQueryCmdPara[6] = 0x00;
            PLCQueryCmdPara[7] = 0x0C; //number of byte LSB (12bytes)
            PLCQueryCmdPara[8] = 0x00; //number of byte MSB
            PLCQueryCmdPara[9] = 0x10;
            PLCQueryCmdPara[10] = 0x00;
            PLCQueryCmdPara[11] = 0x01; //read command
            PLCQueryCmdPara[12] = 0x04; //read command
            PLCQueryCmdPara[13] = 0x00;
            PLCQueryCmdPara[14] = 0x00;
            PLCQueryCmdPara[15] = 0xD4; //dm start address DM6100 =17D4 lsb
            PLCQueryCmdPara[16] = 0x17;  //msb
            PLCQueryCmdPara[17] = 0x00;
            //750=2EE
            //700=2BC
           //400 =0190
           //300=012C
           //200=00C8
            //100=64
            PLCQueryCmdPara[18] = 0xA8; //200= data size address
            PLCQueryCmdPara[19] = 0x90; //400 dm addresses from D6100 to D6499
            PLCQueryCmdPara[20] = 0x01;
            PLCQueryRxPara = new byte[811];//start reading from PLCQueryRX[11];//ie. DM6100
            #endregion



            #region PLC2Read For Parameter

            PLCQueryCmdParaPlc2 = new byte[21];
            PLCQueryCmdParaPlc2[0] = 0x50;
            PLCQueryCmdParaPlc2[1] = 0x00;
            PLCQueryCmdParaPlc2[2] = 0x00;
            PLCQueryCmdParaPlc2[3] = 0xFF;
            PLCQueryCmdParaPlc2[4] = 0xFF;
            PLCQueryCmdParaPlc2[5] = 0x03;
            PLCQueryCmdParaPlc2[6] = 0x00;
            PLCQueryCmdParaPlc2[7] = 0x0C; //number of byte LSB (12bytes)
            PLCQueryCmdParaPlc2[8] = 0x00; //number of byte MSB
            PLCQueryCmdParaPlc2[9] = 0x10;
            PLCQueryCmdParaPlc2[10] = 0x00;
            PLCQueryCmdParaPlc2[11] = 0x01; //read command
            PLCQueryCmdParaPlc2[12] = 0x04; //read command
            PLCQueryCmdParaPlc2[13] = 0x00;
            PLCQueryCmdParaPlc2[14] = 0x00;
            PLCQueryCmdParaPlc2[15] = 0x70; //dm start address DM6100 =17D4 lsb
            PLCQueryCmdParaPlc2[16] = 0x17;  //msb
            PLCQueryCmdParaPlc2[17] = 0x00;
            //750=2EE
            //700=2BC
            //400 =0190
            //300=012C
            //200=00C8
            //100=64
            PLCQueryCmdParaPlc2[18] = 0xA8; //200= data size address
            PLCQueryCmdParaPlc2[19] = 0xF4; //400 dm addresses from D6100 to D6499
            PLCQueryCmdParaPlc2[20] = 0x01;
            PLCQueryRxParaPlc2 = new byte[1011];//start reading from PLCQueryRX[11];//ie. DM6100
            #endregion
           




        }
        #region Function For Station 2

        #region RA

        public void Station02PrinterclearRATh(object msgobj)
        {
            MainNetworkClass networkmain = (MainNetworkClass)msgobj;
            while (!bTerminate)
            {
                if (evt_Station02PrintClearReqRA.WaitOne(100))
                {
                    try
                    {
                      //  this.networkmain.GetStation2PrinterFilesForClearData(ST02Rotatary_Str);
                        ClearPrintDataForSt2();
                        PLCWriteCommand[PLCWriteCommand_DM340] = 0x05; //print complete
                        //  PrintStatus = "print Data Send complete";
                        continue;
                    }
                    catch (Exception ex)
                    {
                        //print fail set Error code
                        log.Error("ST2  ClearData error"+ex);
                        PLCWriteCommand[PLCWriteCommand_DM340] = 0xFF;//printer error
                        //  PrintStatus = "print error";
                        continue;
                    }
                    finally
                    {
                        evt_Station02PrintClearReqRA.Reset();
                    }
                }
            }
        }

        #endregion
       


       

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        //add
        private LinePackInspection OmronSt02;
        public void ST02Vision01Th(object msgobj)
        {
            MainNetworkClass networkmain = (MainNetworkClass)msgobj;
            while (!bTerminate)
            {
                //wait for event to trigger this
                if (evt_Station02InspectionReq.WaitOne(100))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(@"Config.xml");
                    XmlNode VisionIPAddress = doc.SelectSingleNode(@"/CONFIG/LABELVISIONST02/ADD");
                    XmlNode VisionPortNo = doc.SelectSingleNode(@"/CONFIG/LABELVISIONST02/PORT");
                    if (OmronSt02 == null)
                    {
                        OmronSt02 = new LinePackInspection()
                        {
                            HostAddress = VisionIPAddress.InnerText,
                            PortNumber = int.Parse(VisionPortNo.InnerText)
                        };
                        OmronSt02.InitVision();
                        VisionNetworkAddress = "Connected to VisionServer " + VisionIPAddress.InnerText + " Port : " + VisionPortNo.InnerText;//connected
                    }
                    try
                    {
                        if (PLCQueryRx[PLCQueryRx_DM100 + 2] == 0x3){
                           
                            try
                            {
                                if (!OmronSt02.linepackomronsystem.Connected)
                                {
                                    OmronSt02.ConnectToVision();                       
                                }
                                XmlNodeList nodes = networkmain.tnRdoc.SelectNodes("//FILE/FILE_NAME");
                                int printindex = 0;
                                if (PLCQueryRx[PLCQueryRx_DM157 + 26] == 0x2) //170
                                {
                                    printindex = PLCQueryRx[PLCQueryRx_DM157 + 28];//171
                                }
                                if (PLCQueryRx[PLCQueryRx_DM157 + 30] == 0x2)
                                {
                                    printindex = PLCQueryRx[PLCQueryRx_DM157 + 32];
                                }
                                if (PLCQueryRx[PLCQueryRx_DM157 + 34] == 0x2)//DM174
                                {
                                    printindex = PLCQueryRx[PLCQueryRx_DM157 + 36];//dm175
                                }
                                string imagefilename = nodes[printindex - 1].InnerText;
                                imagefilename = imagefilename.Remove(imagefilename.Length - 3, 3) + "bmp";//remove extension and change to bmp
                                OmronSt02.linepackomronsystem.Scene_Switch(0);
                                Thread.Sleep(100);
                                OmronSt02.linepackomronsystem.Prefix_Set(Path.GetFileNameWithoutExtension(imagefilename) + "_");
                                Thread.Sleep(100);
                                result = OmronSt02.linepackomronsystem.AutoMeasure3();
                                OM1.Info("ST2 REEL ANGLE RUNOUT CHECK");
                                bool rst = (result == "1.0000");
                                if (rst)
                                {
                                    PLCWriteCommand[PLCWriteCommand_DM200 + 2] = 0x9;
                                    OM1.Info("ST2 Angle Check Pass");
                                    networkmain.stn2log = "ST2 Angle Check Pass";
                                }
                                else
                                {
                                    PLCWriteCommand[PLCWriteCommand_DM200 + 2] = 0xFF;
                                    OM1.Info("ST2 Angle Check Fail");
                                    networkmain.stn2log = "ST2 Angle Check Fail";
                                }
                                evt_Station02InspectionReq.Reset();
                            }
                            catch (Exception ex)
                            {
                                OM1.Error(ex.ToString());
                                PLCWriteCommand[PLCWriteCommand_DM200 + 2] = 0xFF; //vision inspection Setup Error
                                                                                   //  VisionStatus = "vision inspection Label Timeout ";
                                OmronSt02.DisconnectVision();
                                MyEventQ.AddQ("7;VisionControllerCommunicationBreak;Stationnumber;2");//Push message to stack
                                continue;
                            }

                        }
                        else
                        {
                        XmlNodeList nodes = networkmain.tnRdoc.SelectNodes("//FILE/FILE_NAME");
                        //get filefolder
                        string printer = networkmain.tnRdoc.SelectNodes("//BOXID/PRINTER")[0].InnerText;
                        XmlNode printerdetails = doc.DocumentElement.SelectSingleNode(@"./PRINTER[./PRINTERNAME='" + printer + "']");
                        string imagefolder = printerdetails.SelectSingleNode(@"IMGFOLDER").InnerText; //need to test
                        //where to get the index?? can assume index is always 1 for station 2 case
                        int printindex = 0;
                        if (PLCQueryRx[PLCQueryRx_DM157 + 26] == 0x2) //170
                        {
                            printindex = PLCQueryRx[PLCQueryRx_DM157 + 28];//171
                        }
                        if (PLCQueryRx[PLCQueryRx_DM157 + 30] == 0x2)
                        {
                            printindex = PLCQueryRx[PLCQueryRx_DM157 + 32];
                        }
                        if (PLCQueryRx[PLCQueryRx_DM157 + 34] == 0x2)//DM174
                        {
                            printindex = PLCQueryRx[PLCQueryRx_DM157 + 36];//dm175
                        }
                        if ((printindex - 1) < 0)
                        {
                            throw new Exception("Print index not set!");
                        }
                        string imagefilename = nodes[printindex - 1].InnerText;
                        imagefilename = imagefilename.Remove(imagefilename.Length - 3, 3) + "bmp";//remove extension and change to bmp
                        try
                        {
                            if (!OmronSt02.linepackomronsystem.Connected)
                            {
                                OmronSt02.ConnectToVision();
                            }
                            if (!OmronSt02.linepackomronsystem.Connected)
                            {
                                MyEventQ.AddQ("10;VisionPCCommunicationBreak;Station number;2");//Push message to stack
                            }
                                Thread.Sleep(50);
                            OmronSt02.LoadZPLFile(@"C:\SAPLABELS\" + nodes[printindex - 1].InnerText);


                            bool isIntel2 = false, Isspekteck2 = false;
                            //ADDED BY GYLEE 13/10/2013
                            int intelnum = OmronSt02.IntelLabelCheck(@"C:\SAPLABELS\" + nodes[printindex - 1].InnerText);
                            if (intelnum == 2)
                            {
                                isIntel2 = true;
                            }
                            else if (intelnum == 3)
                            {
                                Isspekteck2 = true;
                            }
                            bool rst = false;
                            OM1.Info("Station 2 result = "+ rst.ToString());
                            OmronSt02.linepackomronsystem.Scene_Switch(0);
                            OmronSt02.linepackomronsystem.Prefix_Set(Path.GetFileNameWithoutExtension(imagefilename) + "_");
                            OmronSt02.spektek2(Isspekteck2, isIntel2);
                            OmronSt02.linepackomronsystem.Scene_Switch(10);
                            string result = "none";
                            //string testfile = Path.GetFileNameWithoutExtension(textBox1.Text);
                            // OmronSt02.linepackomronsystem.UnitData_Change(1, 136, "S:\\");
                            OmronSt02.linepackomronsystem.UnitData_Change(1, 137, Path.GetFileNameWithoutExtension(imagefilename));
                            // OmronSt07Barcode.linepackomronsystem.UnitData_Change(1, 137, imagefilename);
                            OM1.Info("Station 2 Vision Measure Once  " + imagefilename);
                            OmronSt02.linepackomronsystem.Measure_Once();
                                Thread.Sleep(200);


                                OmronSt02.linepackomronsystem.SAVEBMP();
                                OM1.Info("Station 2 SAVE BMP to RAMDisk)");
                                Thread.Sleep(100);
                                OM1.Info("Station 2 Vision Scene_Switch(2)");
                            PLCWriteCommand[PLCWriteCommand_DM380 + 2] = 0x08;
                            OmronSt02.linepackomronsystem.Scene_Switch(2);
                            OM1.Info("Station 2 Vision BCClear");
                            //OmronSt02.MultiData_BCClear();
                            OmronSt02.BCClear();
                            OM1.Info("Station 2 Vision SendBCData");
                           // OmronSt02.MultiData_BCSend();
                            OmronSt02.SendBCData2();
                            OM1.Info("Station 2 Vision Scene_Switch(3)");
                            OmronSt02.linepackomronsystem.Scene_Switch(3);
                            OM1.Info("Station 2 Vision ocrlear");
                            //OmronSt02.MultiData_OcrClear();
                            OmronSt02.BCClear2();
                            OM1.Info("Station 2 Vision Send OCR Data");
                           // OmronSt02.MultiData_OCRLoc();
                            OmronSt02.LoadOCRlocation();
                            OM1.Info("Station 2 Vision AutoMeasure");
                            networkmain.stn2log = imagefilename + " Requested";
                           Thread.Sleep(50);
                            result = OmronSt02.linepackomronsystem.AutoMeasure(false);
                            Thread.Sleep(50);

                            OM1.Info("Station 2 Vision Result " + " " + imagefilename + " " + result);

                            networkmain.stn2log = imagefilename  + " " + result;

                                // bool rst = (result == "1.0000,1.0000,1.0000");
                                string result1 = "";
                            string[] SingleResult = result.Split(',');
                                if (SingleResult.Length > 2)
                                {
                                    if (SingleResult[2] != "1.0000")
                                    {
                                        result1 = OmronSt02.linepackomronsystem.AutoMeasure98();
                                        OM1.Info("Second Barcode Result " + imagefilename + " " + result1);
                                        networkmain.stn2log = "Second Barcode result " + result1;
                                        string BCResult = result1.Trim();
                                        if (BCResult == "1.0000")
                                        {
                                            SingleResult[2] = "1.0000";
                                            networkmain.stn2log = "Second BC Test " + imagefilename + " PASS";
                                            OM1.Info("Second BC Test " + imagefilename + " PASS");
                                        }
                                        else
                                        {
                                            networkmain.stn2log = "Second BC Test " + imagefilename + " FAIL";
                                            OM1.Info("Second BC Test " + imagefilename + " FAIL");
                                        }
                                    }
                                }
                                string dummy = OmronSt02.linepackomronsystem.BMPClear();
                                if (dummy == "-1")
                                {
                                    OM1.Info("Station 2 BMP Clear Execute");
                                }
                                if (SingleResult.Length > 2)
                            {
                                  MyEventQ.AddQ("18;Station2InspectionComplete;LotNumber;" + imagefilename + ";CalibrationScore;" + SingleResult[0] +
                                            ";PatternMatchingScore;" + SingleResult[1] + ";BarcodeScore;" + SingleResult[2] + ";OcrScore;" + SingleResult[3]);//Push message to stack
                            }
                            else if (SingleResult.Length == 1)
                            {
                                MyEventQ.AddQ("18;Station2InspectionComplete;LotNumber;" + imagefilename + ";CalibrationScore;-1.000");//Push message to stack
                                rst = false;
                            }
                            else
                            {
                                rst = false;
                            }
                                //for (int i = 0; i < SingleResult.Length - 1; i++)
                                //{
                                //    if (SingleResult[i] != "1.0000")
                                //    {
                                //        rst = false;
                                //        break;
                                //    }
                                //}
                                if (SingleResult[0] == "1.0000" && SingleResult[1] == "1.0000" && SingleResult[2] == "1.0000")
                                {
                                    rst = true;
                                }
                                if (rst == true)
                            {
                                OM1.Info("Station 2 Vision Pass " + imagefilename);
                                networkmain.stn2log = imagefilename + "  Pass";
                              if( VisionRotaryCheck=="1")
                              {
                                VisionStatus="Pass";
                              }
                              if( VisionRotaryCheck=="2")
                              {
                                VisionStatus1="Pass";
                              }


                              if( VisionRotaryCheck=="3")
                              {
                                VisionStatus2="Pass";
                              }
                                PLCWriteCommand[PLCWriteCommand_DM380 + 2] = 0x9;
                            }
                            else
                            {
                                OM1.Info("Station 2 Vision Fail " + imagefilename);

                                networkmain.stn2log = imagefilename + " Fail";
                                networkmain.OperatorLog = "Stn.2 " + imagefilename + " rejected by vision";
                              if( VisionRotaryCheck=="1")
                              {
                                VisionStatus="Fail";
                              }
                              if( VisionRotaryCheck=="2")
                              {
                                VisionStatus1="Fail";
                              }


                              if( VisionRotaryCheck=="3")
                              {
                                VisionStatus2="Fail";
                              }
                                PLCWriteCommand[PLCWriteCommand_DM380 + 2] = 0xFF;
                            }
                        }
                        //catch (FHNonProcedureSocket.FHNonProcedureSocketProcessingException ex)
                        //{
                        //    log.Error(ex.ToString());
                        //    PLCWriteCommand[PLCWriteCommand_DM380 + 2] = 0xFF; //vision inspection Setup Error
                        //    // VisionStatus = "vision inspection Label Setup Error";
                        //    continue;
                        //}
                        catch (TimeoutException ex)
                        {
                            OM1.Error(ex.ToString());
                            PLCWriteCommand[PLCWriteCommand_DM380 + 2] = 0xFF; //vision inspection Setup Error
                            //  VisionStatus = "vision inspection Label Timeout ";

                            OmronSt02.DisconnectVision();
                                MyEventQ.AddQ("7;VisionControllerCommunicationBreak;Stationnumber;2");//Push message to stack
                                continue;
                        }
                        catch (Exception ex)
                        {
                            OM1.Error(ex.ToString());
                            PLCWriteCommand[PLCWriteCommand_DM380 + 2] = 0xFF; //vision inspection Setup Error
                            OmronSt02.DisconnectVision();
                            MyEventQ.AddQ("7;VisionControllerCommunicationBreak;Stationnumber;2");//Push message to stack
                                continue;
                        }
                    }
                    }
                    catch (Exception ex)
                    {
                        OM1.Error(ex.ToString());
                        PLCWriteCommand[PLCWriteCommand_DM380 + 2] = 0xFF;  //vision inspection Error
                        // OmronSt02.DisconnectVision();
                        //  VisionStatus = "vision inspection Label Error";
                    }
                    finally
                    {
                        evt_Station02InspectionReq.Reset();
                    }
                    // PLCWriteCommand[PLCWriteCommand_DM380 + 2] = 0xFF;

                }
            }
        }
        public void Station02PrinterTh(object msgobj)
        {
            MainNetworkClass networkmain = (MainNetworkClass)msgobj;
            while (!bTerminate)
            {
                if (evt_Station02PrintReq.WaitOne(100))
                {
                    DateTime startTime = DateTime.Now;
                    //log.Info("ST2 Printer label: " + ST04Rotatary_Str + " started");  //error
                    log.Info("ST2 Printer label: " + ST02Rotatary_Str + " started");
                    try
                    {
                        RequestPrinterData(ST02Rotatary_Str,
                                            1,//printerid 1, 2 , 3 
                                            PLCQueryRx[PLCQueryRx_DM100 + 8]);//DM104 label index # send from PLC for label print 
                        //label index 1,2,3,4,....
                       // PrintStatus = "print Data Send to Printer";
                        CheckPrintFileExistAndPrint(networkmain.tnRdoc, PLCQueryRx[PLCQueryRx_DM100 + 8], ST02Rotatary_Str);
                        PLCWriteCommand[PLCWriteCommand_DM200 + 2] = 0x5; //print complete                      
                       
                        continue;
                    }
                    catch 
                    {
                        //print fail set Error code
                        log.Error("Stn.2 Printer Th Exception");
                        PLCWriteCommand[PLCWriteCommand_DM200 + 2] = 0xFF;//printer error
                       // PrintStatus = "print error";
                        continue;
                    }
                    finally
                    {
                        DateTime endTime = DateTime.Now;
                        log.Info("ST2 Printer label: " + ST04Rotatary_Str + " Time used " + endTime.Subtract(startTime).TotalSeconds);
                        evt_Station02PrintReq.Reset();
                    }
                }
            }
        }
      
        public bool CheckStringClear(int offset/*offset from PLCreaddata*/, //offset data 41 == DM210, 103 = DM241, 163 = DM271 
                                     string RotaryLabel)//check if string is empty and requires to clear happens in OK and NA status
        {
            if ((RotaryLabel == "\0\0\0\0\0\0\0\0\0\0") && //empty string found, clear data to 0
                ((PLCWriteCommand[offset + 54] != 0) && (PLCWriteCommand[offset + 55] != 0)))//(offset plus 54 and 55 is OK NA flag
            {
                for (int i = 0; i <= 57; i++)
                {
                    PLCWriteCommand[offset + i] = 0;
                }
            }
            return true;
        }
        public bool CheckStringUpdate(int offset/*offset from PLCreaddata*/,//offset data 41 == DM210, 103 = DM241, 163 = DM271  
                                      string RotaryLabel)//check if string is avaliable during empty status /0/0 and NA
        {
            if ((RotaryLabel != "\0\0\0\0\0\0\0\0\0\0") && //empty string found, clear data to 0
                (
                    ((PLCWriteCommand[offset + 54] == 0) && (PLCWriteCommand[offset + 55] == 0)) ||
                    ((PLCWriteCommand[offset + 54] == 69) && (PLCWriteCommand[offset + 55] == 82))
                ))//(offset plus 54 and 55 is OK NA flag .....ER
            {
                //Update PLC
                byte[] plcbuffer;
                UpdatePLCFinishingLabelDMAddress(out plcbuffer, RotaryLabel);//this inclue update 54 55 status flag
                Array.Copy(plcbuffer, 0, PLCWriteCommand, offset, 58);//PLCWRITE 41 is DM210
            }
            return true;
        }



       
        public bool UpdatePLCFinishingLabelDMAddress(out byte[] PLCBufferMemory, string strFinishingLabel)
        {
            PLCBufferMemory = new byte[60];
            try
            {
                string tmpstr;
                byte[] tmpbyte;
                string tmp;
                XmlNode fltrackingroot = networkmain.FLTrackingdoc.DocumentElement;//this house the list of summarized xml details 
                XmlNode selectednode = fltrackingroot.SelectSingleNode("descendant::LABEL[ID='" + strFinishingLabel + "']");
                string tmpstr1;
                byte[] tmpbyte1;
                tmpstr1 = selectednode.SelectSingleNode("PackageStatus").InnerText;
                tmpbyte1 = new byte[tmpstr1.Length];
                tmpbyte1 = Encoding.ASCII.GetBytes(tmpstr1);

                #region Check MBB Type Reject 

                 XmlDocument doc = new XmlDocument();
                   doc.Load(@"ConfigEvent.xml");
                 XmlNode MBBTrayID = doc.SelectSingleNode(@"/EVENT/REJECTTRAYMBB");
                 XmlNode MBBReelID = doc.SelectSingleNode(@"/EVENT/REJECTREELMBB");

                 string trayMbb=MBBTrayID.InnerText;
                 string  ReelMbb=MBBReelID.InnerText;
                
                 string tmpstr2 = selectednode.SelectSingleNode("MBBTYPE").InnerText;          
             

                #endregion

               tmpstr = selectednode.SelectSingleNode("TYPE").InnerText;//210-D212//240
                tmpbyte = new byte[tmpstr.Length];
                tmpbyte = Encoding.ASCII.GetBytes(tmpstr);
                if (tmpstr == "NA")
                {
                    Array.Copy(tmpbyte, 0, PLCBufferMemory, 54, tmpstr.Length);
                     networkmain.linePack.Info("Station 2  Finishing Label NA " + strFinishingLabel);
                }
                else if (tmpstr1 == "RJ")
                {
                    Array.Copy(tmpbyte1, 0, PLCBufferMemory, 54, tmpstr1.Length);
                    networkmain.linePack.Info("Station 2  Finishing Label RJ " + strFinishingLabel);
                }

                else   if(tmpstr2==trayMbb)
              {
                  selectednode.SelectSingleNode("PackageStatus").InnerText="RJ";
                  selectednode.SelectSingleNode("ErrorCode").InnerText="203";
                   tmpbyte = new byte[2];
                    tmpbyte = Encoding.ASCII.GetBytes("RJ");
                    Array.Copy(tmpbyte, 0, PLCBufferMemory, 54, 2);
                networkmain.linePack.Info("Station 2  Finishing Label Reject because of MBB Tray " + strFinishingLabel+","+ tmpstr2);
              }

             else  if(tmpstr2==ReelMbb)
              {
                  selectednode.SelectSingleNode("PackageStatus").InnerText="RJ";
                  selectednode.SelectSingleNode("ErrorCode").InnerText="204";
                  tmpbyte = new byte[2];
                    tmpbyte = Encoding.ASCII.GetBytes("RJ");
                    Array.Copy(tmpbyte, 0, PLCBufferMemory, 54, 2);
                 networkmain.linePack.Info("Station 2  Finishing Label  Reject because of MBB Reel " + strFinishingLabel+","+ tmpstr2);
              
              }                
                else
                {
                    tmpbyte = new byte[2];
                    tmpbyte = Encoding.ASCII.GetBytes("OK");
                    Array.Copy(tmpbyte, 0, PLCBufferMemory, 54, 2);
                    networkmain.linePack.Info("Station 2  Finishing Label OK " + strFinishingLabel);
                }

                tmpstr = selectednode.SelectSingleNode("TYPE").InnerText;//210-D212//240
                tmpbyte = new byte[tmpstr.Length];
                tmpbyte = Encoding.ASCII.GetBytes(tmpstr);
                //write to DM210(use 3 DM fro 6 bytes)
                Array.Copy(tmpbyte, 0, PLCBufferMemory, 0, tmpstr.Length);
               
                tmpstr = selectednode.SelectSingleNode("AQL").InnerText; //213-215
                tmpbyte = new byte[tmpstr.Length];
                tmpbyte = Encoding.ASCII.GetBytes(tmpstr);
                //write to DM213
                Array.Copy(tmpbyte, 0, PLCBufferMemory, 6, tmpstr.Length);
                tmpstr = selectednode.SelectSingleNode("SEALER").InnerText;//216-218
                tmpbyte = new byte[tmpstr.Length];
                tmpbyte = Encoding.ASCII.GetBytes(tmpstr);
                //write to DM210
                Array.Copy(tmpbyte, 0, PLCBufferMemory, 12, tmpstr.Length);
                tmpstr = selectednode.SelectSingleNode("MBBTYPE").InnerText;//219-221
                tmpbyte = new byte[tmpstr.Length];
                tmpbyte = Encoding.ASCII.GetBytes(tmpstr);
                //write to DM210
                Array.Copy(tmpbyte, 0, PLCBufferMemory, 18, tmpstr.Length);
                tmpstr = selectednode.SelectSingleNode("REELHEIGHT").InnerText;//222-224
                tmpbyte = new byte[tmpstr.Length];
                tmpbyte = Encoding.ASCII.GetBytes(tmpstr);
                //write to DM210
                Array.Copy(tmpbyte, 0, PLCBufferMemory, 24, tmpstr.Length);
                tmpstr = selectednode.SelectSingleNode("TRAYNO").InnerText;
                tmpbyte = new byte[tmpstr.Length];
                tmpbyte = Encoding.ASCII.GetBytes(tmpstr);
                //write to DM210
                Array.Copy(tmpbyte, 0, PLCBufferMemory, 30, tmpstr.Length);
                //# label @ station 2
                tmpstr = selectednode.SelectSingleNode("STATION02PRINTNO").InnerText;
                tmpbyte = new byte[tmpstr.Length];
                tmpbyte = Encoding.ASCII.GetBytes(tmpstr);
                //write to DM210
                Array.Copy(tmpbyte, 0, PLCBufferMemory, 36, tmpstr.Length);
                //# label @ station 4
                tmpstr = selectednode.SelectSingleNode("STATION04PRINTNO").InnerText;
                tmpbyte = new byte[tmpstr.Length];
                tmpbyte = Encoding.ASCII.GetBytes(tmpstr);
                //write to DM210
                Array.Copy(tmpbyte, 0, PLCBufferMemory, 42, tmpstr.Length);
                //# label @ station 7
                tmpstr = selectednode.SelectSingleNode("STATION07PRINTNO").InnerText;
                tmpbyte = new byte[tmpstr.Length];
                tmpbyte = Encoding.ASCII.GetBytes(tmpstr);
                //write to DM210
                Array.Copy(tmpbyte, 0, PLCBufferMemory, 48, tmpstr.Length);
            }
            catch 
            {
               // log.Error("UpdatePLCFinishingLabelDMAddress Exception");
                //St2Log.Error("Update Finishing Label On PLC Fail  for Station 2 Label = " + strFinishingLabel);
                byte[] tmpbyte = new byte[2];
                tmpbyte = Encoding.ASCII.GetBytes("ER");
                Array.Copy(tmpbyte, 0, PLCBufferMemory, 54, 2);
                return false;
            }
            return true;
        }

        private void CheckPrintFileExistAndPrint(XmlDocument printdoc, int printindex, string finishinglabel)
        {
            //find all files in btnStation2
            XmlNodeList nodes = printdoc.SelectNodes("//FILE/FILE_NAME");
            //get filefolder
            XmlDocument doc = new XmlDocument();
            doc.Load(@"Config.xml");
            XmlNode filefolder = doc.SelectSingleNode(@"/CONFIG/PRINTFILEDIR");
            XmlNode ArchiveFolder = doc.SelectSingleNode(@"/CONFIG/PRINTARCHIVE");
            string printer = printdoc.SelectNodes("//BOXID/PRINTER")[0].InnerText;
            XmlNode printerdetails = doc.DocumentElement.SelectSingleNode(@"./PRINTER[./PRINTERNAME='" + printer + "']");
            if ((printindex - 1) < 0)
            {
                throw new Exception("Print index not set!");
            }
            string filename = nodes[printindex - 1].InnerText;
            Ping PingPrinter2 = new Ping();

            PingReply PR2 = PingPrinter2.Send("192.168.3.224");

            if (PR2.Status == IPStatus.DestinationHostUnreachable)
            {
                MyEventQ.AddQ("11;PrinterCommunicationBreak;Stationnumber;2");//Push message to stack
            }
            try
            {
                int failCounter = 0;
                do
                {
                    bool exists = File.Exists(filefolder.InnerText + filename);
                    if (!exists)
                    {
                        failCounter++;
                        log.Error(filefolder.InnerText + filename + " Not Found! ,Printer2 Retry=" + failCounter);
                        Thread.Sleep(3000);
                        networkmain.stn2log = "Printer2 Retry " + failCounter;
                        if (failCounter <= 3) continue;
                        throw new Exception(filefolder.InnerText + filename + " Not Found!");
                    }
                    else
                    {
                        log.Info(filefolder.InnerText + filename + " Located!");
                        break;
                    }
                }
                while (failCounter < 3);
                string[] arg = new string[4];
                arg[0] = printerdetails.SelectSingleNode(@"IPADDRESS").InnerText;
                arg[1] = filefolder.InnerText;
                arg[2] = filename;
                arg[3] = printerdetails.SelectSingleNode(@"IMGFOLDER").InnerText;
                if (!Directory.Exists(ArchiveFolder.InnerText))
                    Directory.CreateDirectory(ArchiveFolder.InnerText);
                File.Copy(filefolder.InnerText + filename, ArchiveFolder.InnerText + filename + "t", true);
                ZebraPrinter zb = new ZebraPrinter(arg);
                //PrintStatus = "Printer Ready For Print";
                //do printer files backup
               networkmain.linePack.Info("station 2 printing finishing label " + finishinglabel + " file ==> " + filename);
                networkmain.linePack.Info("Station2 filefolder Name" + filefolder.InnerText);
                networkmain.linePack.Info("Station2 filename " + filename);
                MyEventQ.AddQ("15;Station2LabelPrinted;LabelFileName;" + filename);//Push message to stack
                EvtLog.Info("15;Station2LabelPrinted;LabelFileName;" + filename);
                if (!Directory.Exists(ArchiveFolder.InnerText))
                    Directory.CreateDirectory(ArchiveFolder.InnerText);
                File.Copy(filefolder.InnerText + filename, ArchiveFolder.InnerText + filename, true);
                //  File.Delete(filefolder.InnerText + filename);
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
                throw new Exception(ex.ToString());
            }
        }
        public void RequestPrinterData(string FinishingLabel,
                                      int PrinterID,//printerid 1, 2 , 3 
                                      int labelindex)//label index 1,2,3,4,....
        {
            try
            {
                //set required data to prepare for printing.
                networkmain.linePack.Info("Station 2 printer " + FinishingLabel);
                this.networkmain.GetPrinterFilesTnR(FinishingLabel);
                //check file available
                //update paste position
            }
            catch (Exception ex)
            {
                if (ex != null) log.Error("RequestPrinterData :" + ex.Message);
                throw ex;
            }
        }
        #endregion
        #region station 8 Function
        public bool CheckstringClearFor8(int offset, string label)  //need to add finishing label 
        {
            if (label == "\0\0\0\0\0\0\0\0")
            {
                for (int i = 0; i <= 15; i++)
                {
                    PLCWriteCommand6[offset + i] = 0;
                }
            }
            return true;
        }
        public bool CheckStringUpdateFor8(int offset, string Label)
        {
            if ((Label != "\0\0\0\0\0\0\0\0") && //empty string found, clear data to 0
                 (
                     ((PLCWriteCommand6[offset + 14] == 0) && (PLCWriteCommand6[offset + 15] == 0)) ||
                     ((PLCWriteCommand6[offset + 14] == 69) && (PLCWriteCommand6[offset + 15] == 82))
                 ))//(offset plus 8 and 8 is OK NA flag .....ER
            {   // D5342=305
                //5335=291
                //Update PLC
                byte[] plcbuffer;//update
                UpdatePLCFinishingLabelDMAddressFor8(out plcbuffer, Label);
                Array.Copy(plcbuffer, 0, PLCWriteCommand6, offset, 16);//PLCWRITE
            }
            return true;
        }
        public bool UpdatePLCFinishingLabelDMAddressFor8(out byte[] PLCBufferMemory, string strtrackingLabel)
        {
            PLCBufferMemory = new byte[16];
            try
            {
                //tracking label check
                string tmpstr;
                byte[] tmpbyte;
                XmlNode fltrackingroot = networkmain.FLTrackingdoc.DocumentElement;//this house the list of summarized xml details 
                XmlNode selectednode = fltrackingroot.SelectSingleNode("descendant::LABEL[TrackingLabel='" + strtrackingLabel + "']");
                st8FINISH = selectednode.SelectSingleNode("ID").InnerText;
                tmpstr = selectednode.SelectSingleNode("ID").InnerText;
                tmpbyte = new byte[tmpstr.Length];
                tmpbyte = Encoding.ASCII.GetBytes(tmpstr);
                Array.Copy(tmpbyte, 0, PLCBufferMemory, 0, tmpstr.Length);
                  string tmpstr1;
                byte[] tmpbyte1;
                tmpstr1 = selectednode.SelectSingleNode("PackageStatus").InnerText;
                tmpbyte1 = new byte[tmpstr1.Length];
                tmpbyte1 = Encoding.ASCII.GetBytes(tmpstr1);



                tmpstr = selectednode.SelectSingleNode("TYPE").InnerText;//D
                tmpbyte = new byte[tmpstr.Length];
                tmpbyte = Encoding.ASCII.GetBytes(tmpstr);
                if (tmpstr == "NA")
                {
                    Array.Copy(tmpbyte, 0, PLCBufferMemory,14, tmpstr.Length);
                   Station8Log.Info("Station 8 Finishing label and Tracking label NA " + st8track + "'" + st8FINISH);
                }
                else if (tmpstr1 == "RJ")
                {
                    Array.Copy(tmpbyte1, 0, PLCBufferMemory,14, tmpstr1.Length);
                      Station8Log.Info("Station 8 Finishing label and Tracking label RJ " + st8track + "'" + st8FINISH);
                }
                else
                {
                    tmpbyte = new byte[2];
                    tmpbyte = Encoding.ASCII.GetBytes("OK");
                    Array.Copy(tmpbyte, 0, PLCBufferMemory, 14, 2);
                   Station8Log.Info("Station 8 Finishing label and Tracking label OK " + st8track + "'" + st8FINISH);
                }



                //write to DM5321(use 3 DM fro 6 bytes)
                tmpbyte = new byte[tmpstr.Length];
                tmpbyte = Encoding.ASCII.GetBytes(tmpstr);
                Array.Copy(tmpbyte, 0, PLCBufferMemory, 10, tmpstr.Length);
                
                tmpstr = selectednode.SelectSingleNode("AQL").InnerText;
                tmpbyte = new byte[tmpstr.Length];
                tmpbyte = Encoding.ASCII.GetBytes(tmpstr);
                Array.Copy(tmpbyte, 0, PLCWriteCommand6, 439, tmpstr.Length); //D5409=439
                Station8Log.Info("Station 8 AQL: " + tmpstr +","+ st8track + "'" + st8FINISH);
                networkmain.stn8log = "Station 8 AQL: " + tmpstr + "," + st8track + "'" + st8FINISH;
            }
            catch 
            {
              //  log.Error("Update Finishing Label On PLC Fail for Station 8 Label = " + st8track);
                Station8Log.Error("Update Finishing Label On PLC Fail for Station 8 Label = " + st8track);
                networkmain.OperatorLog = "Stn.8 Updating Label on PLC failed";
                byte[] tmpbyte = new byte[2];
                tmpbyte = Encoding.ASCII.GetBytes("ER");
                Array.Copy(tmpbyte, 0, PLCBufferMemory, 14, 2);
                return false;
            }
            return true;
        }
        public string UpdatePLCFinishingLabelDMAddressFor81(string strtrackingLabel)
        {
            try
            {
                XmlNode fltrackingroot = networkmain.FLTrackingdoc.DocumentElement;//this house the list of summarized xml details 
                XmlNode selectednode = fltrackingroot.SelectSingleNode("descendant::LABEL[TrackingLabel='" + strtrackingLabel + "']");
                if (selectednode == null)
                    throw new Exception("no selectednode");
                string finishLb = selectednode.SelectSingleNode("ID").InnerText;
                Station8Log.Info("Station 8 Finishing label show in Buffer" + finishLb);
                return finishLb;
               

            }
            catch 
            {
               // log.Error("Update FL Exception " + st8track);
                Station8Log.Error("Update Finishing Label On PLC Fail for Station 8 Label = " + st8track);
                string finishLb1 = "";
                return finishLb1;
            }
        }
        #endregion
        #region Station 7





        public void Station07PrinterClearTh(object msgobj)
        {
            MainNetworkClass networkmain = (MainNetworkClass)msgobj;
            while (!bTerminate)
            {
                if (evt_Station07PrintClearReq.WaitOne(100))
                {
                    try
                    {
                        //this.networkmain.GetStation7PrinterFilesForClearData(ST07Rotatary_Str);
                        ClearPrintDataForSt7();
                        PLCWriteCommand6[PLCWriteCommand_DM5346] = 0x05; //print complete
                        //  PrintStatus = "print Data Send complete";
                        continue;
                    }
                    catch (Exception ex)
                    {
                        //print fail set Error code
                        log.Error(ex.ToString());
                        PLCWriteCommand6[PLCWriteCommand_DM5346] = 0xFF;//printer error
                        //  PrintStatus = "print error";
                        continue;
                    }
                    finally
                    {
                        evt_Station07PrintClearReq.Reset();
                    }
                }
            }
        }     

        // private FHNonProcedureSocket OmronSt07;
       private LinePackInspection OmronSt07Barcode;
        public void ST02Vision07Th(object msgobj)
        {
            MainNetworkClass networkmain = (MainNetworkClass)msgobj;
            while (!bTerminate)
            {
                Thread.Sleep(100);
                if (evt_Station07InspectionReq.WaitOne(100))
                {
                    try
                    {
                        //get filefolder
                        //need X,Y,surface codinate for PLC robot
                        bool rst = false;
                        OM3.Info("Station 7 Pre-Test result1 = " + rst.ToString());
                        XmlDocument doc = new XmlDocument();
                        doc.Load(@"Config.xml");
                        XmlNode VisionIPAddress = doc.SelectSingleNode(@"/CONFIG/LABELVISIONST07/ADD");
                        XmlNode VisionPortNo = doc.SelectSingleNode(@"/CONFIG/LABELVISIONST07/PORT");
                        // XmlNode VisionImgFile = doc.SelectSingleNode(@"/CONFIG/LABELVISIONST07/IMGFOLDER");  //need to create share folder for Image
                        VisionNetworkAddress7 = "Trying to connect to " + VisionIPAddress.InnerText + " Port : " + VisionPortNo.InnerText;
                        //get label number and image file name
                        XmlNodeList nodes = networkmain.Boxdoc.SelectNodes("//FILE/FILE_NAME");
                        //get filefolder
                        string printer = networkmain.Boxdoc.SelectNodes("//BOXID/PRINTER")[0].InnerText;
                        OM3.Info("Printer string" + printer);
                        XmlNode printerdetails = doc.DocumentElement.SelectSingleNode(@"./PRINTER[./PRINTERNAME='" + printer + "']");
                        string imagefolder = printerdetails.SelectSingleNode(@"IMGFOLDER").InnerText;
                        //where to get the index?? can assume index is always 1 for station 2 case
                        int printindex = PLCQueryRx6[PLCQueryRx_DM5111 + 10];//DM5116

                        OM3.Info("Printindex Data" + printindex);
                        if ((printindex - 1) < 0)
                        {
                            throw new Exception("Print index not set!");
                        }
                        string imagefilename = nodes[printindex - 1].InnerText;
                        // imagefilename = imagefolder + imagefilename.Remove(imagefilename.Length - 3, 3) + "bmp";//remove extension and change to bmp
                        // imagefilename = "'S:\'" + imagefilename.Remove(imagefilename.Length - 3, 3) + "bmp"; //need to test here
                        //  imagefilename = @"T:\master0.bmp";
                        //   imagefilename = @"S:\master" + ((int)(printindex - 1)).ToString() + ".bmp";
                        //  imagefilename = @"T:\master0.bmp";
                        try
                        {
                            imagefilename = imagefilename.Remove(imagefilename.Length - 3, 3) + "bmp";

                            if (OmronSt07Barcode == null)
                            {
                                OmronSt07Barcode = new LinePackInspection()
                                {
                                    HostAddress = VisionIPAddress.InnerText,//"192.168.3.122", 
                                    PortNumber = int.Parse(VisionPortNo.InnerText)//9877 

                                };
                                OmronSt07Barcode.InitVision();
                            }
                            if (!OmronSt07Barcode.linepackomronsystem.Connected)
                            {
                                OmronSt07Barcode.ConnectToVision();
                            }
                            if (!OmronSt07Barcode.linepackomronsystem.Connected)
                            {
                                MyEventQ.AddQ("10;VisionPCCommunicationBreak;Station number;7");//Push message to stack
                            }
                            Thread.Sleep(50);
                            OmronSt07Barcode.LoadZPLFile(@"C:\SAPLABELS\" + nodes[printindex - 1].InnerText);
                         
                            bool isIntel2 = false, Isspekteck2 = false;
                            //ADDED BY GYLEE 13/10/2013
                            int intelnum = OmronSt07Barcode.IntelLabelCheck(@"C:\SAPLABELS\" + nodes[printindex - 1].InnerText);
                            if (intelnum == 2)
                            {
                                isIntel2 = true;
                            }
                            else if (intelnum == 3)
                            {
                                Isspekteck2 = true;
                            }
                            OM3.Info("Station 7 Vision Start " + imagefilename);
                            OmronSt07Barcode.linepackomronsystem.Scene_Switch(0);
                            Thread.Sleep(50);
                            OmronSt07Barcode.linepackomronsystem.Prefix_Set(Path.GetFileNameWithoutExtension(imagefilename) + "_");
                            Thread.Sleep(50);
                            OmronSt07Barcode.spektek7(Isspekteck2, isIntel2,isTray7 );
                            OmronSt07Barcode.linepackomronsystem.Scene_Switch(10);
                            Thread.Sleep(50);

                            string result1 = "none";
                            string result = "none";
                            //string testfile = Path.GetFileNameWithoutExtension(textBox1.Text);
                            OmronSt07Barcode.linepackomronsystem.UnitData_Change(1, 137, Path.GetFileNameWithoutExtension(imagefilename));
                            // OmronSt07Barcode.linepackomronsystem.UnitData_Change(1, 137, imagefilename);
                            OM3.Info("Station 7 Vision Measure_Once  " + imagefilename);
                            OmronSt07Barcode.linepackomronsystem.Measure_Once();
                            Thread.Sleep(500);
                            OM3.Info("Station 7 SAVE BMP to RAMDisk)");
                           // OmronSt07Barcode.linepackomronsystem.SAVEBMP();
                            OM3.Info("Station 7 SAVE BMP to RAMDisk)");
                            Thread.Sleep(50);
                            OM3.Info("Station 7 Vision Scene_Switch(2)");
                            OmronSt07Barcode.linepackomronsystem.Scene_Switch(2);
                            OM3.Info("Station 7 Vision BCClear");
                            //OmronSt07Barcode.MultiData_BCClear();
                           // OmronSt07Barcode.BCClear();
                            OM3.Info("Station 7 Vision SendBCData");
                            PLCWriteCommand6[PLCWriteCommand_DM5402] = 0x08;
                           // OmronSt07Barcode.MultiData_BCSend();
                            OmronSt07Barcode.SendBCData2();
                            OM3.Info("Station 7 Vision Scene_Switch(3)");
                            Thread.Sleep(50);
                            OmronSt07Barcode.linepackomronsystem.Scene_Switch(3);
                            OM3.Info("Station 7 Vision Ocr Clear");
                            //OmronSt07Barcode.MultiData_OcrClear();
                            OmronSt07Barcode.BCClear2();
                            OM3.Info("Station 7 Vision Send OCR Data");
                            //OmronSt07Barcode.MultiData_OCRLoc();
                            OmronSt07Barcode.LoadOCRlocation7();
                            Thread.Sleep(50);
                            OM3.Info("Station 7 Vision AutoMeasure");
                            networkmain.stn7log = imagefilename + " Requested";
                            result = OmronSt07Barcode.linepackomronsystem.AutoMeasure(false);

                            Thread.Sleep(50);//CANNOT SEND CMD IMMEDIATELY AFTER OMRON VISION REPLY... OMRON PROBLEM
                            OM3.Info("Station 7 Vision Result" + " " + imagefilename + " " + result);
                            networkmain.stn7log = imagefilename + " " + result;
                            // bool rst = (result == "1.0000,1.0000,1.0000");
                           
                            string[] SingleResult = result.Split(',');
                            if (SingleResult.Length > 2)
                            {
                                if (SingleResult[2] != "1.0000")
                                {
                                    result1 = OmronSt07Barcode.linepackomronsystem.AutoMeasure98();
                                    OM3.Info("Second Barcode Result " + imagefilename + " " + result1);
                                    networkmain.stn7log = "Second Barcode result " + result1;
                                    string BCResult = result1.Trim();
                                    if (BCResult == "1.0000")
                                    {
                                        SingleResult[2] = "1.0000";
                                        networkmain.stn7log = "Second BC Test " + imagefilename + " PASS";
                                        OM3.Info("Second BC Test " + imagefilename + " PASS");
                                    }
                                    else
                                    {
                                        networkmain.stn7log = "Second BC Test " + imagefilename + " FAIL";
                                        OM3.Info("Second BC Test " + imagefilename + " FAIL");
                                    }
                                }
                            }
                            if (SingleResult.Length > 2)
                            {
                                if (SingleResult[1] != "1.0000")
                                {
                                    result1 = OmronSt07Barcode.linepackomronsystem.AutoMeasure99();
                                    OM3.Info("Second PM Result " + imagefilename + " " + result1);
                                    networkmain.stn7log = "Second PM result " + result1;
                                    string[] PMResult = result1.Split(',');
                                    if (PMResult[2] == "1.0000")
                                    {
                                        SingleResult[1] = "1.0000";
                                        networkmain.stn7log = "Second PM Test " + imagefilename + " Pass";
                                        OM3.Info("Second PM Test " + imagefilename + " Pass");
                                    }
                                    else
                                    {
                                        networkmain.stn7log = "Second PM Test " + imagefilename + " Fail";
                                        OM3.Info("Second PM Test " + imagefilename + " Fail");
                                    }
                                }
                            }
                            string dummy = OmronSt07Barcode.linepackomronsystem.BMPClear();
                            if (dummy == "-1")
                            {
                                OM3.Info("Station 7 BMP Clear Execute");
                            }
                            if (SingleResult.Length > 2)
                            {
                                  MyEventQ.AddQ("20;Station7InspectionComplete;LotNumber;" + imagefilename + ";CalibrationScore;" + SingleResult[0] +
                                          ";PatternMatchingScore;" + SingleResult[1] + ";BarcodeScore;" + SingleResult[2] + ";OcrScore;" + SingleResult[3]);//Push message to stack
                            }
                            else if (SingleResult.Length == 1)
                            {
                                MyEventQ.AddQ("20;Station7InspectionComplete;LotNumber;" + imagefilename + ";CalibrationScore;-1.000");//Push message to stack
                                rst = false;
                            }
                            else
                            {
                                rst = false;
                            }
                          
                            //for (int i = 0; i < SingleResult.Length - 1; i++)
                            //{
                            //    if (SingleResult[i] != "1.0000")
                            //    {
                            //        rst = false;
                            //        break;
                            //    }
                            //}
                            if (SingleResult[0] == "1.0000" && SingleResult[1] == "1.0000" && SingleResult[2] == "1.0000")
                            {
                                rst = true;
                            }
                            OM3.Info("Station 7 Test result1 = " + rst.ToString());
                            if (rst == true)
                            {
                                OM3.Info("Station 7 Vision Pass" + imagefilename);
                                networkmain.stn7log = imagefilename + " Pass";
                                PLCWriteCommand6[PLCWriteCommand_DM5402] = 0x09;
                            }
                            else
                            {
                                OM3.Info("Station 7 Vision Fail" + imagefilename);
                                networkmain.stn7log = imagefilename + " Fail";
                                PLCWriteCommand6[PLCWriteCommand_DM5402] = 0x0F;
                            }
                            SingleResult = null;
                        }
                        //catch (FHNonProcedureSocket.FHNonProcedureSocketProcessingException ex)
                        //{
                        //    log.Error(ex.ToString());
                        //    PLCWriteCommand6[PLCWriteCommand_DM5402] = 0xFF; //vision inspection Setup Error
                        //    // VisionStatus = "vision inspection Label Setup Error";
                        //    continue;
                        //}
                        catch (TimeoutException ex)
                        {
                            OM3.Error(ex.ToString());
                            PLCWriteCommand6[PLCWriteCommand_DM5402] = 0xFF; //vision inspection Setup Error
                            //  VisionStatus = "vision inspection Label Timeout ";
                            OmronSt07Barcode.DisconnectVision();
                            MyEventQ.AddQ("7;VisionControllerCommunicationBreak;Stationnumber;7");//Push message to stack
                            continue;
                        }
                        catch (Exception ex)
                        {
                            OM3.Error(ex.ToString());
                            PLCWriteCommand6[PLCWriteCommand_DM5402] = 0xFF; //vision inspection Setup Error
                            OmronSt07Barcode.DisconnectVision();
                            MyEventQ.AddQ("7;VisionControllerCommunicationBreak;Stationnumber;7");//Push message to stack
                            //  VisionStatus = "vision inspection Label Error";
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        OM3.Error(ex.ToString());//error due to reading of file
                        PLCWriteCommand6[PLCWriteCommand_DM5402] = 0xFF;  //vision inspection Error

                    }
                    finally
                    {
                        evt_Station07InspectionReq.Reset();
                    }
                    //  PLCWriteCommand6[PLCWriteCommand_DM5402] = 0xFF;
                }
            }
        }
        public void Station07PrinterTh(object msgobj)
        {
            MainNetworkClass networkmain = (MainNetworkClass)msgobj;
            while (!bTerminate)
            {
                if (evt_Station07PrintReq.WaitOne(100))
                {
                    DateTime startTime = DateTime.Now;
                    try
                    {
                        Station7Log.Info("ST7 Printer label: " + ST07Rotatary_Str + " started");
                        //  ST07Rotatary_Str1 = "BY";
                        RequestPrinterDataStation7(ST07Rotatary_Str,
                                            3,//printerid 1, 2 , 3 
                                            PLCQueryRx6[PLCQueryRx_DM5111 + 2]);//DM104 label index # send from PLC for label print 
                        CheckPrintFileExistAndPrintForSt7(networkmain.Boxdoc, PLCQueryRx6[PLCQueryRx_DM5111 + 2], ST07Rotatary_Str);
                        PLCWriteCommand6[PLCWriteCommand_DM5320] = 0x05; //print complete
                        //   PrintStatus = "print Data Send complete";
                        continue;
                    }
                    catch 
                    {
                        //print fail set Error code
                        Station7Log.Error("Station07PrinterTh exception:"+ ST07Rotatary_Str);
                        PLCWriteCommand6[PLCWriteCommand_DM5320] = 0xFF;//printer error

                        //   PrintStatus = "print error";
                        continue;
                    }
                    finally
                    {
                        evt_Station07PrintReq.Reset();
                        DateTime endTime = DateTime.Now;
                        Station7Log.Info("ST7 Printer label: " + ST07Rotatary_Str + " Time used " + endTime.Subtract(startTime).TotalSeconds);

                    }
                }
            }
        }


       public void Station07ESDPrinterTh(object msgobj)
        {
            MainNetworkClass networkmain = (MainNetworkClass)msgobj;
            while (!bTerminate)
            {
                if (evt_Station07ESDPrintReq.WaitOne(100))
                {
                    DateTime startTime = DateTime.Now;
                    try
                    {

                        Station7Log.Info("ST7 ESD printer " + ST07Rotatary_StrESD);
                        this.networkmain.GetESDPrinterFilesBox(ST07Rotatary_StrESD);
                        CheckPrintFileExistAndPrintForSt7(networkmain.BoxdocESD, PLCQueryRx6[PLCQueryRx_DM5111 + 2], ST07Rotatary_StrESD);
                        PLCWriteCommand6[PLCWriteCommand_DM5320] = 0x05; //print complete
                        //   PrintStatus = "print Data Send complete";
                        continue;
                    }
                    catch 
                    {
                        //print fail set Error code
                        Station7Log.Error("Station07PrinterTh exception");
                        PLCWriteCommand6[PLCWriteCommand_DM5320] = 0xFF;//printer error

                        //   PrintStatus = "print error";
                        continue;
                    }
                    finally
                    {
                        evt_Station07ESDPrintReq.Reset();
                        DateTime endTime = DateTime.Now;
                        Station7Log.Info("ST7 ESD Printer label: " + ST07Rotatary_Str + " Time used " + endTime.Subtract(startTime).TotalSeconds);

                    }
                }
            }
        }





        
        public void RequestPrinterDataStation7(string FinishingLabel,
                                 int PrinterID,//printerid 1, 2 , 3 
                                 int labelindex)//label index 1,2,3,4,....
        {
            try
            {
                Station7Log.Info("Station 7 printer: " + FinishingLabel);
                this.networkmain.GetPrinterFilesBox(FinishingLabel);
                //check file available
                //update paste position
            }
            catch (Exception ex)
            {
                if (ex != null) log.Error("RequestPrinterDataStation7:" + ex.Message);
                throw ex;
            }
        }
        public bool CheckstringClearFor7(int offset, string label)  //need to add finishing label 
        {
            if (label == "\0\0\0\0\0\0\0\0")
            {
                for (int i = 0; i <= 9; i++)
                {
                    PLCWriteCommand6[offset + i] = 0;
                }
            }
            return true;
        }



        public bool CheckstringClearFor7ESD(int offset, string label)  //need to add finishing label 
        {
            if (label == "\0\0\0\0\0\0\0\0")
            {
                for (int i = 0; i <= 1; i++)
                {
                    PLCWriteCommand6[offset + i] = 0;
                  //  Station7Log.Info("st7 clear offset" + offset + i);
                }
            }
            return true;
        }


       public bool CheckstringClearFor8AQL(int offset, string label)  //need to add finishing label 
        {
            if (label == "\0\0\0\0\0\0\0\0")
            {
                for (int i = 0; i <= 1; i++)
                {
                    PLCWriteCommand6[offset + i] = 0;
                   
                }
            }
            return true;
        }


      public bool CheckstringClearFor4Specktex(int offset, string label)  //need to add finishing label 
        {
            if (label == "\0\0\0\0\0\0\0\0\0\0")
            {
                for (int i = 0; i <= 7; i++)
                {
                    PLCWriteCommand[offset + i] = 0;
                   
                }
            }
            return true;
        }








        public byte[] barcodechangeposition(byte[] str)
        {
            byte tmpswitchmsb = 0;
            byte tmpswitchlsb = 0;
            for (int i = 0; i < 8; i++)
            {
                int mod = i % 2;
                if (mod == 0)
                    tmpswitchmsb = str[i];
                else
                {
                    tmpswitchlsb = str[i];
                    str[i] = tmpswitchmsb;
                    str[i - 1] = tmpswitchlsb;
                }
            }
            return str;
        }
        public bool CheckStringUpdateFor7(int offset, string Label)
        {
            if ((Label != "\0\0\0\0\0\0\0\0") && //empty string found, clear data to 0
                 (
                     ((PLCWriteCommand6[offset + 8] == 0) && (PLCWriteCommand6[offset + 9] == 0)) ||
                     ((PLCWriteCommand6[offset + 8] == 69) && (PLCWriteCommand6[offset + 9] == 82))//OK
                 ))//(offset plus 8 and 8 is OK NA flag .....ER
            {   // D5325=271
                //Update PLC
                byte[] plcbuffer;
                int tmpOeeid = 0;
                UpdatePLCFinishingLabelDMAddressFor7(out plcbuffer, Label, out tmpOeeid);
                rq.UpdStNobyID(7, tmpOeeid);
                Array.Copy(plcbuffer, 0, PLCWriteCommand6, offset, 10);//PLCWRITE
            }
            return true;
        }
        public bool GetFLByTL(string strtrackingLabel, out int OEEID)
        {
            try
            {
                XmlNode fltrackingroot = networkmain.FLTrackingdoc.DocumentElement;//this house the list of summarized xml details 
                XmlNode selectednode = fltrackingroot.SelectSingleNode("descendant::LABEL[TrackingLabel='" + strtrackingLabel + "']");
                OEEID = int.Parse(selectednode.SelectSingleNode("OEEid").InnerText);
                Station7Log.Info("Station 7 to Station 8 Tracking Label " + ST07Rotatary_Str1 + " OEEID =" + OEEID);
            }
            catch
            {
                log.Error("Get OEEID FAIL exception");
                OEEID = 0;
                return false;
            }
            return true;
        }
        public bool UpdatePLCFinishingLabelDMAddressFor7(out byte[] PLCBufferMemory, string strtrackingLabel, out int OEEID)
        {
            PLCBufferMemory = new byte[10];
            try
            {
                //tracking label check
                string tmpstr;
                byte[] tmpbyte;
                XmlNode fltrackingroot = networkmain.FLTrackingdoc.DocumentElement;//this house the list of summarized xml details 
                XmlNode selectednode = fltrackingroot.SelectSingleNode("descendant::LABEL[TrackingLabel='" + strtrackingLabel + "']");

                //  selectednode.SelectSingleNode("STATION").InnerText = "7";   
        
                 ST07Rotatary_Str1 = selectednode.SelectSingleNode("ID").InnerText;
                st7Flabel = selectednode.SelectSingleNode("ID").InnerText;
                Station7Log.Info("Station 7 Finishing Label " + ST07Rotatary_Str1);



               string tmpstr1;
                byte[] tmpbyte1;
                tmpstr1 = selectednode.SelectSingleNode("PackageStatus").InnerText;//D5325
                tmpbyte1 = new byte[tmpstr1.Length];
                tmpbyte1 = Encoding.ASCII.GetBytes(tmpstr1);

                OEEID = int.Parse(selectednode.SelectSingleNode("OEEid").InnerText);

                tmpstr = selectednode.SelectSingleNode("TYPE").InnerText;//D
                tmpbyte = new byte[tmpstr.Length];
                tmpbyte = Encoding.ASCII.GetBytes(tmpstr);
                if (tmpstr == "NA")
                {
                    Array.Copy(tmpbyte, 0, PLCBufferMemory,8, tmpstr.Length);
                   Station7Log.Info("Station 7 Tarcking Label NA " + st7track);
                }
                else if (tmpstr1 == "RJ")
                {
                    Array.Copy(tmpbyte1, 0, PLCBufferMemory,8, tmpstr1.Length);
                     Station7Log.Info("Station 7 Tarcking Label RJ " + st7track);
                }
                else
                {
                    tmpbyte = new byte[2];
                    tmpbyte = Encoding.ASCII.GetBytes("OK");
                    Array.Copy(tmpbyte, 0, PLCBufferMemory, 8, 2);
                    Station7Log.Info("Station 7 Tarcking Label OK " + st7track);
                }
                string tmpstr7 = selectednode.SelectSingleNode("TYPE").InnerText;//add by gylee
                if (tmpstr7 == "TRAY")
                {
                    isTray7 = true;
                }
                else
                {
                    isTray7 = false;
                }
                tmpbyte = new byte[tmpstr.Length];
                tmpbyte = Encoding.ASCII.GetBytes(tmpstr);
                //write to DM5321(use 3 DM fro 6 bytes)
                Array.Copy(tmpbyte, 0, PLCBufferMemory, 0, tmpstr.Length);
                
                tmpstr = selectednode.SelectSingleNode("STATION07PRINTNO").InnerText;
                int reel = int.Parse(tmpstr);
                tmpbyte = new byte[tmpstr.Length];
                tmpbyte = BitConverter.GetBytes(reel);
                //write to DM5324
                Array.Copy(tmpbyte, 0, PLCBufferMemory, 6, tmpstr.Length);
            }
            catch 
            {
               // log.Error("UpdatePLCFinishingLabelDMAddressFor7 exception");
                Station7Log.Error("Update Finishing Label On PLC Fail for Station 7 Label = " + st7track);
                byte[] tmpbyte = new byte[2];
                tmpbyte = Encoding.ASCII.GetBytes("ER");
                Array.Copy(tmpbyte, 0, PLCBufferMemory, 8, 2);
                OEEID = 0;
                return false;
            }
            return true;
        }
        //
        public bool UpdatePLCFinishingLabelDMAddressFor71(string strtrackingLabel)
        {
            try
            {
                //tracking label check
                //string tmpstr;
                //byte[] tmpbyte;
                XmlNode fltrackingroot = networkmain.FLTrackingdoc.DocumentElement;//this house the list of summarized xml details 
                XmlNode selectednode = fltrackingroot.SelectSingleNode("descendant::LABEL[TrackingLabel='" + strtrackingLabel + "']");
                ST07Rotatary_Str1 = selectednode.SelectSingleNode("ID").InnerText;
                st7Flabel = selectednode.SelectSingleNode("ID").InnerText;
                Station7Log.Info("Station 7 Finishing Label " + ST07Rotatary_Str1);
                ////track 
                //XmlNode selectednode1 = fltrackingroot.SelectSingleNode("descendant::LABEL[ID='" + ST07Rotatary_Str1 + "']");
                //st7track = selectednode1.SelectSingleNode("TrackingLabel").InnerText;
            }
            catch 
            {
               // log.Error("UpdatePLCFinishingLabelDMAddressFor71 exception");
                Station7Log.Error("Update Finishing Label On PLC Fail for Station 7 Label = " + strtrackingLabel);
                return false;
            }
            return true;
        }



     



       public bool CheckStringUpdateFor7ESD(string Label)
        {
            if ((Label != "\0\0\0\0\0\0\0\0") && //empty string found, clear data to 0
                 (
                     ((PLCWriteCommand6[461] == 0) && (PLCWriteCommand6[462] == 0)) ||
                     ((PLCWriteCommand6[461] == 69) && (PLCWriteCommand6[462] == 82))//OK
                 ))//(offset plus 8 and 8 is OK NA flag .....ER
            {   
              UpdatePLCFinishingLabelDMAddressFor7ESD(Label);


            }
            return true;
        }

      


      public bool UpdatePLCFinishingLabelDMAddressFor7ESD(string strtrackingLabel)
        {
            try
            {
                //tracking label check
                string tmpstr;
                byte[] tmpbyte;
                string sHotLot = "0";
                XmlNode fltrackingroot = networkmain.FLTrackingdoc.DocumentElement;//this house the list of summarized xml details 
                XmlNode selectednode = fltrackingroot.SelectSingleNode("descendant::LABEL[TrackingLabel='" + strtrackingLabel + "']");
                ST07Rotatary_StrESD = selectednode.SelectSingleNode("ID").InnerText;  
            
                Station7Log.Info("Station 7 Finishing Label ESD " + ST07Rotatary_StrESD);
                if (selectednode.SelectSingleNode("HotLot") != null)
                {
                    sHotLot = selectednode.SelectSingleNode("HotLot").InnerText;
 //                   sHotLot = "1"; //for testing
                }
                string tmpstr1;
                byte[] tmpbyte1;
                tmpstr1 = selectednode.SelectSingleNode("PackageStatus").InnerText;//D5420
                tmpbyte1 = new byte[tmpstr1.Length];
                tmpbyte1 = Encoding.ASCII.GetBytes(tmpstr1);



                tmpstr = selectednode.SelectSingleNode("TYPE").InnerText;//D
                tmpbyte = new byte[tmpstr.Length];
                tmpbyte = Encoding.ASCII.GetBytes(tmpstr);
                if (tmpstr == "NA")
                {
                   tmpbyte = new byte[2];
                    tmpbyte = Encoding.ASCII.GetBytes("NA");
                    Array.Copy(tmpbyte, 0, PLCWriteCommand6,461,2);//D5420
                    Station7Log.Info("Station 7 ESD Tarcking Label NA for st7 ESD" + st7ESDtrack);
                }
                else if (tmpstr1 == "RJ")
                {
		                tmpbyte = new byte[2];
                    tmpbyte = Encoding.ASCII.GetBytes("RJ");
                    Array.Copy(tmpbyte1, 0, PLCWriteCommand6, 461,2);
                     Station7Log.Info("Station 7 ESD Tarcking Label RJ " + st7ESDtrack);
                }
                else
                {
                    tmpbyte = new byte[2];
                    tmpbyte = Encoding.ASCII.GetBytes("OK");
                    Array.Copy(tmpbyte, 0, PLCWriteCommand6,461,2);//D5420
                    Station7Log.Info("Station 7 ESD Tarcking Label OK " + st7ESDtrack + " HotLot:" + sHotLot);
                    networkmain.stn7log = "Station 7 ESD Tarcking Label OK " + st7ESDtrack + " HotLot:" + sHotLot;
                    if (sHotLot.StartsWith("1"))
                    {
                        tmpbyte = new byte[2];
                        tmpbyte = Encoding.ASCII.GetBytes("1");    //241; //DM5310
                        Array.Copy(tmpbyte, 0, PLCWriteCommand6, 221, 1);//DM5300  HotLot
                    }
                    else
                    {
                        tmpbyte = new byte[2];
                        tmpbyte = Encoding.ASCII.GetBytes("0");
                        Array.Copy(tmpbyte, 0, PLCWriteCommand6, 221, 1);//D5300  HotLot
                    }
                }               
                          
             
               
            }
            catch 
            {
              //  log.Error("UpdatePLCFinishingLabelDMAddressFor7ESD exception");
                Station7Log.Error("Update Finishing Label On PLC Fail for Station 7 ESD Label = " + st7ESDtrack+","+  ST07Rotatary_StrESD);
                byte[] tmpbytest7;             

                tmpbytest7 = new byte[2];
                tmpbytest7 = Encoding.ASCII.GetBytes("ER");
                Array.Copy(tmpbytest7, 0, PLCWriteCommand6, 461, 2);//D5420
                Station7Log.Info("Station 7 ESD Tarcking Label ER " + st7ESDtrack);
                return false;
            }
            return true;
        }


       public bool UpdatePLCFinishingLabelDMAddressFor71ForBuffer(string strtrackingLabel)
        {
            try
            {
                //tracking label check
                //string tmpstr;
                //byte[] tmpbyte;
                XmlNode fltrackingroot = networkmain.FLTrackingdoc.DocumentElement;//this house the list of summarized xml details 
                XmlNode selectednode = fltrackingroot.SelectSingleNode("descendant::LABEL[TrackingLabel='" + strtrackingLabel + "']");
               // ST07Rotatary_Str1 = selectednode.SelectSingleNode("ID").InnerText;
                st7FlabelForBuffer = selectednode.SelectSingleNode("ID").InnerText;
                Station7Log.Info("Station 7 Buffer Finishing Label " +  st7FlabelForBuffer);
                ////track 
                //XmlNode selectednode1 = fltrackingroot.SelectSingleNode("descendant::LABEL[ID='" + ST07Rotatary_Str1 + "']");
                //st7track = selectednode1.SelectSingleNode("TrackingLabel").InnerText;
            }
            catch 
            {
              //  log.Error("UpdatePLCFinishingLabelDMAddressFor71ForBuffer exception");
                Station7Log.Error("Update Finishing Label On PLC Fail for Station 7 Label in Buffer = " + strtrackingLabel);
                return false;
            }
            return true;
        }


        public int sur;
        private void CheckPrintFileExistAndPrintForSt7(XmlDocument printdoc, int printindex, string finishinglabel)
        {
            //find all files in btnStation2
            XmlNodeList nodes = printdoc.SelectNodes("//FILE/FILE_NAME");
            XmlNodeList nodes1 = printdoc.SelectNodes("//FILE/SURFACE");
            XmlNodeList nodes2 = printdoc.SelectNodes("//FILE/COORDINATE_X");
            XmlNodeList nodes3 = printdoc.SelectNodes("//FILE/COORDINATE_Y");
            //get filefolder
            XmlDocument doc = new XmlDocument();
            doc.Load(@"Config.xml");
            XmlNode filefolder = doc.SelectSingleNode(@"/CONFIG/PRINTFILEDIR");
            XmlNode ArchiveFolder = doc.SelectSingleNode(@"/CONFIG/PRINTARCHIVE");
            string printer = printdoc.SelectNodes("//BOXID/PRINTER")[0].InnerText;
            XmlNode printerdetails = doc.DocumentElement.SelectSingleNode(@"./PRINTER[./PRINTERNAME='" + printer + "']");
            if ((printindex - 1) < 0)
            {
                throw new Exception("Print index not set!");
            }
            string surface = nodes1[printindex - 1].InnerText;
            switch (surface)
            {
                case "E":
                    sur = 1;
                    break;
                case "C":
                    sur = 2;
                    break;
                case "F":
                    sur = 3;
                    break;
                case "D":
                    sur = 4;
                    break;
                case "A":
                    sur = 5;
                    break;
                case "B":
                    sur = 6;
                    break;
            }
            string CoX = nodes2[printindex - 1].InnerText;
            string CoY = nodes3[printindex - 1].InnerText;
            //  int sur = int.Parse(surface);
            int cox = int.Parse(CoX);
            int coy = int.Parse(CoY);
            UpdateDMaddressForSt7(sur, cox, coy);
            string filename = nodes[printindex - 1].InnerText;
            try
            {
                bool exists = File.Exists(filefolder.InnerText + filename);
                if (!exists) throw new Exception(filefolder.InnerText + filename + "Not Found!");
                string[] arg = new string[4];
                arg[0] = printerdetails.SelectSingleNode(@"IPADDRESS").InnerText;
                arg[1] = filefolder.InnerText;
                arg[2] = filename;
                arg[3] = printerdetails.SelectSingleNode(@"IMGFOLDER").InnerText;
                if (!Directory.Exists(ArchiveFolder.InnerText))
                    Directory.CreateDirectory(ArchiveFolder.InnerText);
                File.Copy(filefolder.InnerText + filename, ArchiveFolder.InnerText + filename + "t", true);
                Ping PingPrinter2 = new Ping();

                PingReply PR2 = PingPrinter2.Send("192.168.3.226");

                if (PR2.Status == IPStatus.DestinationHostUnreachable)
                {
                    MyEventQ.AddQ("11;PrinterCommunicationBreak;Stationnumber;7");//Push message to stack
                }
                ZebraPrinter zb = new ZebraPrinter(arg);
                //  PrintStatus = "Printer Ready For Print";
                //do printer files backup
                Station7Log.Info("station 7 printing finishing label " + finishinglabel + " file ==> " + filename);
                Station7Log.Info("Station7 filefolder Name" + filefolder.InnerText);
                Station7Log.Info("Station7 filename " + filename);
                MyEventQ.AddQ("17;Station7LabelPrinted;LabelFileName;" + filename);//Push message to stack
                EvtLog.Info("17;Station7LabelPrinted;LabelFileName;" + filename);
                if (!Directory.Exists(ArchiveFolder.InnerText))
                    Directory.CreateDirectory(ArchiveFolder.InnerText);
                File.Copy(filefolder.InnerText + filename, ArchiveFolder.InnerText + filename, true);
                //  File.Delete(filefolder.InnerText + filename);
            }
            catch (Exception ex)
            {
                Station7Log.Error(ex.ToString());
                throw new Exception(ex.ToString());
            }
        }
        private void UpdateDMaddressForSt7(int surface, int X, int Y)
        {
            int suf = surface;
            int x = X;
            int y = Y;
            byte[] tmpbyte;
            tmpbyte = BitConverter.GetBytes(suf);
            // tmpbyte = Encoding.ASCII.GetBytes(surface);
            Array.Copy(tmpbyte, 0, PLCWriteCommand6, SurfaceOFFSETst7, 2); //DM5326
            byte[] tmpbyte1;
            tmpbyte1 = BitConverter.GetBytes(x);
            Array.Copy(tmpbyte1, 0, PLCWriteCommand6, XOFFSETForst7, 2); //DM5327
            byte[] tmpbyte2;
            tmpbyte2 = BitConverter.GetBytes(y);
            Array.Copy(tmpbyte2, 0, PLCWriteCommand6, YOFFSETForst7, 2); //DM5328
        }
        #endregion
        #region Station 3 Function




        public bool CheckstringClearFor3(int offset, string label)
        {
            if (label == "\0\0\0\0\0\0\0\0\0\0")
            {
                for (int i = 0; i <= 35; i++)
                {
                    PLCWriteCommand[offset + i] = 0;
                }
            }
            return true;
        }
        public bool CheckStringUpdateFor3(int offset, string RotaryLabel)
        {
            if ((RotaryLabel != "\0\0\0\0\0\0\0\0\0\0") && //empty string found, clear data to 0
                 (
                     ((PLCWriteCommand[offset + 30] == 0) && (PLCWriteCommand[offset + 31] == 0)) ||
                     ((PLCWriteCommand[offset + 30] == 69) && (PLCWriteCommand[offset + 31] == 82))
                 ))//(offset plus 30and 31 is OK NA flag .....ER 365
            {
                //Update PLC
                byte[] plcbuffer;
                int tmpOeeid = 0;
                UpdatePLCFinishingLabelDMAddressFor3(out plcbuffer, RotaryLabel,out tmpOeeid);
                rq.UpdStNobyID(3, tmpOeeid);
                Array.Copy(plcbuffer, 0, PLCWriteCommand, offset, 35);//PLCWRITE
            }
            return true;
        }
        public bool UpdatePLCFinishingLabelDMAddressFor3(out byte[] PLCBufferMemory, string strFinishingLabel, out int OEEID)
        {
            PLCBufferMemory = new byte[36];
            try
            {
                string tmpstr;
                byte[] tmpbyte;
                XmlNode fltrackingroot = networkmain.FLTrackingdoc.DocumentElement;//this house the list of summarized xml details 
                XmlNode selectednode = fltrackingroot.SelectSingleNode("descendant::LABEL[ID='" + strFinishingLabel + "']");
                //string st = "3";
               // networkmain.UpdateStationLabel(strFinishingLabel, st);
                 string tmpstr1;
                byte[] tmpbyte1;
                tmpstr1 = selectednode.SelectSingleNode("PackageStatus").InnerText;
                tmpbyte1 = new byte[tmpstr1.Length];
                tmpbyte1 = Encoding.ASCII.GetBytes(tmpstr1);

                OEEID = int.Parse(selectednode.SelectSingleNode("OEEid").InnerText);

                tmpstr = selectednode.SelectSingleNode("TYPE").InnerText;//
                tmpbyte = new byte[tmpstr.Length];
                tmpbyte = Encoding.ASCII.GetBytes(tmpstr);
                if (tmpstr == "NA")
                {
                    Array.Copy(tmpbyte, 0, PLCBufferMemory, 30, tmpstr.Length);
                     networkmain.linePack.Info("Station 3 Finishing Label NA " + strFinishingLabel);//D365
                }
                else if (tmpstr1 == "RJ")
                {
                    Array.Copy(tmpbyte1, 0, PLCBufferMemory, 30, tmpstr1.Length);
                    networkmain.linePack.Info("Station 3 Finishing Label RJ " + strFinishingLabel);
                }
                else
                {
                    tmpbyte = new byte[2];
                    tmpbyte = Encoding.ASCII.GetBytes("OK");
                    Array.Copy(tmpbyte, 0, PLCBufferMemory, 30, 2);
                  networkmain.linePack.Info("Station 3 Finishing Label OK " + strFinishingLabel);
                }

                tmpbyte = new byte[tmpstr.Length];
                tmpbyte = Encoding.ASCII.GetBytes(tmpstr);
                //write to DM(use 3 DM fro 6 bytes)
                Array.Copy(tmpbyte, 0, PLCBufferMemory, 0, tmpstr.Length);//D350
               
                tmpstr = selectednode.SelectSingleNode("HIC_REQUIRED").InnerText; //D353
                tmpbyte = new byte[tmpstr.Length];
                tmpbyte = Encoding.ASCII.GetBytes(tmpstr);
                //write to DM
                Array.Copy(tmpbyte, 0, PLCBufferMemory, 6, tmpstr.Length);
                tmpstr = selectednode.SelectSingleNode("DESICCANT_REQUIRED").InnerText;  //D356             
                tmpbyte = new byte[tmpstr.Length];
                tmpbyte = Encoding.ASCII.GetBytes(tmpstr);
                //write to DM
                Array.Copy(tmpbyte, 0, PLCBufferMemory, 12, tmpstr.Length);
                tmpstr = selectednode.SelectSingleNode("REELHEIGHT").InnerText;//D359
                tmpbyte = new byte[tmpstr.Length];
                tmpbyte = Encoding.ASCII.GetBytes(tmpstr);
                //write to DM
                Array.Copy(tmpbyte, 0, PLCBufferMemory, 18, tmpstr.Length);
               
                tmpstr = selectednode.SelectSingleNode("TRAYNO").InnerText;//D360
                int reel2 = int.Parse(tmpstr);
                tmpbyte = new byte[tmpstr.Length];
                tmpbyte = BitConverter.GetBytes(reel2);
                Array.Copy(tmpbyte, 0, PLCBufferMemory, 20, tmpstr.Length);

                tmpstr = selectednode.SelectSingleNode("TRAY_THICKNESS").InnerText; //D361/D362/D363/D364
                tmpbyte = new byte[tmpstr.Length];
                tmpbyte = Encoding.ASCII.GetBytes(tmpstr);
                //write to D
                Array.Copy(tmpbyte, 0, PLCBufferMemory, 22, tmpstr.Length);



               
            }
            catch 
            {
               // log.Error("UpdatePLCFinishingLabelDMAddressFor3 exception");
               // Station3Log.Error("Update Finishing Label On PLC Fail for Label Station 3 = " + strFinishingLabel);
                byte[] tmpbyte = new byte[2];
                tmpbyte = Encoding.ASCII.GetBytes("ER");
                Array.Copy(tmpbyte, 0, PLCBufferMemory, 30, 2);
                OEEID = 0;
                return false;
            }
            return true;
        }

        #endregion
        #region Station 4

        public void RequestPrinterDataStation4(string FinishingLabel,
                                    int PrinterID,//printerid 1, 2 , 3 
                                    int labelindex)//label index 1,2,3,4,....
        {
            try
            {
                //set required data to prepare for printing.   
                networkmain.linePack.Info("Station 4 printer " + FinishingLabel);
                this.networkmain.GetPrinterFilesMBB(FinishingLabel);
                //check file available
                //update paste position
            }
            catch (Exception ex)
            {
                if (ex != null) log.Error(ex.Message);
                throw ex;
            }
        }


        public bool CheckstringClearForstation4MBB(int offset, string label)
        {
            if (label == "\0\0\0\0\0\0\0\0\0\0")
            {
                for (int i = 0; i <= 21; i++)
                {
                    PLCWriteCommand[offset + i] = 0;
                }
            }
            return true;
        }




        public bool CheckstringClearFor4(int offset, string label)
        {
            if (label == "\0\0\0\0\0\0\0\0\0\0")
            {
                for (int i = 0; i <= 47; i++)
                {
                    PLCWriteCommand[offset + i] = 0;
                }
            }
            return true;
        }
        public bool CheckstringClearForstation4barcodeData(int offset, string label)
        {
            if (label == "\0\0\0\0\0\0\0\0\0\0")
            {
                for (int i = 0; i <= 9; i++)
                {
                    PLCWriteCommand[offset + i] = 0;
                }
            }
            return true;
        }



        public bool CheckStringUpdateFor4MBB(int offset, string RotaryLabel)
        {
            if ((RotaryLabel != "\0\0\0\0\0\0\0\0\0\0") && //empty string found, clear data to 0
                 (
                     ((PLCWriteCommand[offset + 20] == 0) && (PLCWriteCommand[offset + 21] == 0)) ||
                     ((PLCWriteCommand[offset + 20] == 69) && (PLCWriteCommand[offset + 21] == 82))
                 ))//(offset plus 44 and 45 is OK NA flag .....ER
            {
                //Update PLC
                byte[] plcbuffer;
                UpdatePLCFinishingLabelDMAddressFor4MBB(out plcbuffer, RotaryLabel);
                Array.Copy(plcbuffer, 0, PLCWriteCommand, offset, 22);//PLCWRITE
            }
            return true;
        }




        public bool UpdatePLCFinishingLabelDMAddressFor4MBB(out byte[] PLCBufferMemory, string strFinishingLabel)
        {
            PLCBufferMemory = new byte[22]; //403 T0 413
            try
            {
                //tracking label check
                string tmpstr;
                byte[] tmpbyte;
                XmlNode fltrackingroot = networkmain.FLTrackingdoc.DocumentElement;//this house the list of summarized xml details 

                XmlNode selectednode = fltrackingroot.SelectSingleNode("descendant::LABEL[ID='" + strFinishingLabel + "']");
                // selectednode.SelectSingleNode("STATION").InnerText = "4";         
                 string tmpstr1;
                byte[] tmpbyte1;
                tmpstr1 = selectednode.SelectSingleNode("PackageStatus").InnerText;
                tmpbyte1 = new byte[tmpstr1.Length];
                tmpbyte1 = Encoding.ASCII.GetBytes(tmpstr1);



                tmpstr = selectednode.SelectSingleNode("TYPE").InnerText;//D 412
                tmpbyte = new byte[tmpstr.Length];
                tmpbyte = Encoding.ASCII.GetBytes(tmpstr);
                if (tmpstr == "NA")
                {
                    Array.Copy(tmpbyte, 0, PLCBufferMemory, 20, tmpstr.Length);
                     networkmain.linePack.Info("Station 4 Finishing Label NA for MBB Check " + strFinishingLabel);
                }
                else if (tmpstr1 == "RJ")
                {
                    Array.Copy(tmpbyte1, 0, PLCBufferMemory, 20, tmpstr1.Length);
                    networkmain.linePack.Info("Station 4 Finishing Label RJ for MBB Check " + strFinishingLabel);
                }
                else
                {
                    tmpbyte = new byte[2];
                    tmpbyte = Encoding.ASCII.GetBytes("OK");
                    Array.Copy(tmpbyte, 0, PLCBufferMemory, 20, 2);
                    networkmain.linePack.Info("Station 4 Finishing Label OK For MBB Check" + strFinishingLabel);
                }

                tmpbyte = new byte[tmpstr.Length];
                tmpbyte = Encoding.ASCII.GetBytes(tmpstr);
                Array.Copy(tmpbyte, 0, PLCBufferMemory, 0, tmpstr.Length);
               
                tmpstr = selectednode.SelectSingleNode("MBBTYPE").InnerText;
                tmpbyte = new byte[tmpstr.Length];
                tmpbyte = Encoding.ASCII.GetBytes(tmpstr);
                //write to DM406~411
                Array.Copy(tmpbyte, 0, PLCBufferMemory, 6, tmpstr.Length);

            }
            catch
            {
               // log.Error("UpdatePLCFinishingLabelDMAddressFor4 MBB exception");
               // Station4Log.Error("Update Finishing Label On PLC Fail for ST4 MBB check Label = " + strFinishingLabel);
                byte[] tmpbyte = new byte[2];
                tmpbyte = Encoding.ASCII.GetBytes("ER"); //D412
                Array.Copy(tmpbyte, 0, PLCBufferMemory, 20, 2);
                return false;
            }
            return true;
        }














        public bool CheckStringUpdateFor4(int offset, string RotaryLabel)
        {
            if ((RotaryLabel != "\0\0\0\0\0\0\0\0\0\0") && //empty string found, clear data to 0
                 (
                     ((PLCWriteCommand[offset + 44] == 0) && (PLCWriteCommand[offset + 45] == 0)) ||
                     ((PLCWriteCommand[offset + 44] == 69) && (PLCWriteCommand[offset + 45] == 82))
                 ))//(offset plus 44 and 45 is OK NA flag .....ER
            {
                //Update PLC
                byte[] plcbuffer;
                int tmpOeeid = 0;
                UpdatePLCFinishingLabelDMAddressFor4(out plcbuffer, RotaryLabel, out tmpOeeid);
              //  rq.UpdStNobyID(4, tmpOeeid);
                Array.Copy(plcbuffer, 0, PLCWriteCommand, offset, 47);//PLCWRITE
            }
            return true;
        }
        public bool UpdatePLCFinishingLabelDMAddressFor4(out byte[] PLCBufferMemory, string strFinishingLabel, out int OEEID)
        {
            PLCBufferMemory = new byte[48]; //310 T0 333
            try
            {
                //tracking label check
                string tmpstr;
                byte[] tmpbyte;
                XmlNode fltrackingroot = networkmain.FLTrackingdoc.DocumentElement;//this house the list of summarized xml details 

                XmlNode selectednode = fltrackingroot.SelectSingleNode("descendant::LABEL[ID='" + strFinishingLabel + "']");
                // selectednode.SelectSingleNode("STATION").InnerText = "4";         
                 string tmpstr1;
                byte[] tmpbyte1;
                tmpstr1 = selectednode.SelectSingleNode("PackageStatus").InnerText;
                tmpbyte1 = new byte[tmpstr1.Length];
                tmpbyte1 = Encoding.ASCII.GetBytes(tmpstr1);


                OEEID = int.Parse(selectednode.SelectSingleNode("OEEid").InnerText);

                tmpstr = selectednode.SelectSingleNode("TYPE").InnerText;//D 332
                tmpbyte = new byte[tmpstr.Length];
                tmpbyte = Encoding.ASCII.GetBytes(tmpstr);
                if (tmpstr == "NA")
                {
                    Array.Copy(tmpbyte, 0, PLCBufferMemory, 44, tmpstr.Length);
                   networkmain.linePack.Info("Station 4 Finishing Label NA " + strFinishingLabel);
                }
                else if (tmpstr1 == "RJ")
                {
                    Array.Copy(tmpbyte1, 0, PLCBufferMemory, 44, tmpstr1.Length);
                     networkmain.linePack.Info("Station 4 Finishing Label RJ " + strFinishingLabel);
                }
                else
                {
                    tmpbyte = new byte[2];
                    tmpbyte = Encoding.ASCII.GetBytes("OK");
                    Array.Copy(tmpbyte, 0, PLCBufferMemory, 44, 2);
                    networkmain.linePack.Info("Station 4 Finishing Label OK " + strFinishingLabel);
                }


                tmpbyte = new byte[tmpstr.Length];
                tmpbyte = Encoding.ASCII.GetBytes(tmpstr);
                Array.Copy(tmpbyte, 0, PLCBufferMemory, 0, tmpstr.Length);


               string tmpstr9 = selectednode.SelectSingleNode("TYPE").InnerText;//add by gylee
                if (tmpstr9 == "TRAY")
                {
                    isTray4 = true;
                }
                else
                {
                    isTray4 = false;
                }

                
                tmpstr = selectednode.SelectSingleNode("MBBTYPE").InnerText;
                tmpbyte = new byte[tmpstr.Length];
                tmpbyte = Encoding.ASCII.GetBytes(tmpstr);
                //write to DM313~322
                Array.Copy(tmpbyte, 0, PLCBufferMemory, 6, tmpstr.Length);
                tmpstr = selectednode.SelectSingleNode("REELHEIGHT").InnerText;
                int reel1 = int.Parse(tmpstr);
                tmpbyte = new byte[tmpstr.Length];
                tmpbyte = BitConverter.GetBytes(reel1);
                //write to DM323~325
                Array.Copy(tmpbyte, 0, PLCBufferMemory, 26, tmpstr.Length);
                tmpstr = selectednode.SelectSingleNode("TRAYNO").InnerText;
                int reel2 = int.Parse(tmpstr);
                tmpbyte = new byte[tmpstr.Length];
                tmpbyte = BitConverter.GetBytes(reel2);
                //write to DM326~328
                Array.Copy(tmpbyte, 0, PLCBufferMemory, 32, tmpstr.Length);
                //# label @ station 4
                tmpstr = selectednode.SelectSingleNode("STATION04PRINTNO").InnerText;
                int reel = int.Parse(tmpstr);
                tmpbyte = new byte[tmpstr.Length];
                tmpbyte = BitConverter.GetBytes(reel);
                //write to DM329~331
                Array.Copy(tmpbyte, 0, PLCBufferMemory, 38, tmpstr.Length);

                tmpstr = selectednode.SelectSingleNode("SPTK_MARKED").InnerText;
                tmpbyte = new byte[tmpstr.Length];
                tmpbyte = Encoding.ASCII.GetBytes(tmpstr);
                Array.Copy(tmpbyte, 0, PLCWriteCommand, 461, tmpstr.Length);//D420

                tmpstr = selectednode.SelectSingleNode("HIC_REQUIRED").InnerText; //D422
                tmpbyte = new byte[tmpstr.Length];
                tmpbyte = Encoding.ASCII.GetBytes(tmpstr);
                //write to DM
                Array.Copy(tmpbyte, 0,  PLCWriteCommand, 465, tmpstr.Length);



            }
            catch
            {
              //  log.Error("UpdatePLCFinishingLabelDMAddressFor4 exception");
              //  Station4Log.Error("Update Finishing Label On PLC Fail for Label = " + strFinishingLabel);
                byte[] tmpbyte = new byte[2];
                tmpbyte = Encoding.ASCII.GetBytes("ER"); //D332
                Array.Copy(tmpbyte, 0, PLCBufferMemory, 44, 2);
                OEEID = 0;
                return false;
            }
            return true;
        }
        public void Station04PrinterTh(object msgobj)
        {
            MainNetworkClass networkmain = (MainNetworkClass)msgobj;
            while (!bTerminate)
            {
                if (evt_Station04PrintReq.WaitOne(100))
                {
                    DateTime startTime = DateTime.Now;
                    log.Info("ST4 Printer label: " + ST04Rotatary_Str + " started");
                    try
                    {
                        RequestPrinterDataStation4(ST04Rotatary_Str,
                                            2,//printerid 1, 2 , 3 
                                            PLCQueryRx[PLCQueryRx_DM140 + 2]);//DM141 label index # send from PLC for label print 

                        CheckPrintFileExistAndPrintForSt4(networkmain.MBBdoc, PLCQueryRx[PLCQueryRx_DM140 + 2], ST04Rotatary_Str);
                        PLCWriteCommand[PLCWriteCommand_DM301] = 0x05; //print complete
                        //  PrintStatus = "print Data Send complete";
                        continue;
                    }
                    catch (Exception ex)
                    {
                        //print fail set Error code
                        log.Error(ex.ToString());
                        PLCWriteCommand[PLCWriteCommand_DM301] = 0xFF;//printer error
                        //  PrintStatus = "print error";
                        continue;
                    }
                    finally
                    {
                        DateTime endTime = DateTime.Now;
                        log.Info("ST4 Printer label: " + ST04Rotatary_Str + " Time used " + endTime.Subtract(startTime).TotalSeconds);
                        evt_Station04PrintReq.Reset();
                    }
                }
            }
        }




        public void Station04PrinterClearTh(object msgobj)
        {
            MainNetworkClass networkmain = (MainNetworkClass)msgobj;
            while (!bTerminate)
            {
                if (evt_Station04PrintClearReq.WaitOne(100))
                {
                    try
                    {
                       // this.networkmain.GetStation4PrinterFilesForClearData(ST04Rotatary_Str);
                        ClearPrintDataForSt4();
                        PLCWriteCommand[PLCWriteCommand_DM401] = 0x05; //print complete
                        //  PrintStatus = "print Data Send complete";
                        continue;
                    }
                    catch (Exception ex)
                    {
                        //print fail set Error code
                       // Log.Error(ex.ToString());
                        PLCWriteCommand[PLCWriteCommand_DM401] = 0xFF;//printer error
                        //  PrintStatus = "print error";
                        continue;
                    }
                    finally
                    {
                        evt_Station04PrintClearReq.Reset();
                    }
                }
            }
        }
        
        public int sur1;
        private void CheckPrintFileExistAndPrintForSt4(XmlDocument printdoc, int printindex, string finishinglabel)
        {
            //find all files in btnStation2
            XmlNodeList nodes = printdoc.SelectNodes("//FILE/FILE_NAME");
            XmlNodeList nodes1 = printdoc.SelectNodes("//FILE/SURFACE");
            XmlNodeList nodes2 = printdoc.SelectNodes("//FILE/COORDINATE_X");
            XmlNodeList nodes3 = printdoc.SelectNodes("//FILE/COORDINATE_Y");
            //get filefolder
            XmlDocument doc = new XmlDocument();
            doc.Load(@"Config.xml");
            XmlNode filefolder = doc.SelectSingleNode(@"/CONFIG/PRINTFILEDIR");
            XmlNode ArchiveFolder = doc.SelectSingleNode(@"/CONFIG/PRINTARCHIVE");
            string printer = printdoc.SelectNodes("//BOXID/PRINTER")[0].InnerText;
            XmlNode printerdetails = doc.DocumentElement.SelectSingleNode(@"./PRINTER[./PRINTERNAME='" + printer + "']");
            if ((printindex - 1) < 0)
            {
                throw new Exception("Print index not set!");
            }
           networkmain.linePack.Info("Station4 Surface,X,Y");
            string surface = nodes1[printindex - 1].InnerText;
            switch (surface)
            {
                case "A":
                    sur1 = 1;
                    break;
                case "B":
                    sur1 = 2;
                    break;
            }
            string CoX = nodes2[printindex - 1].InnerText;
            string CoY = nodes3[printindex - 1].InnerText;
            //  int sur = int.Parse(surface);
            int cox = int.Parse(CoX);
            int coy = int.Parse(CoY);
            UpdateDMaddressForSt4(sur1, cox, coy);
            string filename = nodes[printindex - 1].InnerText;
            try
            {
                bool exists = File.Exists(filefolder.InnerText + filename);
                if (!exists) throw new Exception(filefolder.InnerText + filename + "Not Found!");
                string[] arg = new string[4];
                arg[0] = printerdetails.SelectSingleNode(@"IPADDRESS").InnerText;
                arg[1] = filefolder.InnerText;
                arg[2] = filename;
                arg[3] = printerdetails.SelectSingleNode(@"IMGFOLDER").InnerText;
                if (!Directory.Exists(ArchiveFolder.InnerText))
                    Directory.CreateDirectory(ArchiveFolder.InnerText);
                File.Copy(filefolder.InnerText + filename, ArchiveFolder.InnerText + filename + "t", true);
                Ping PingPrinter2 = new Ping();

                PingReply PR2 = PingPrinter2.Send("192.168.3.225");

                if (PR2.Status == IPStatus.DestinationHostUnreachable)
                {
                    MyEventQ.AddQ("11;PrinterCommunicationBreak;Stationnumber;4");//Push message to stack
                }
                ZebraPrinter zb = new ZebraPrinter(arg);
               networkmain.linePack.Info("station 4 printing finishing label " + finishinglabel + " file ==> " + filename);
               networkmain.linePack.Info("Station4 filefolder Name" + filefolder.InnerText);
                networkmain.linePack.Info("Station4 filename " + filename);
                MyEventQ.AddQ("16;Station4LabelPrinted;LabelFileName;" + filename);//Push message to stack
                EvtLog.Info("16;Station4LabelPrinted;LabelFileName;" + filename);
                //  PrintStatus = "Printer Ready For Print";
                //do printer files backup
                if (!Directory.Exists(ArchiveFolder.InnerText))
                    Directory.CreateDirectory(ArchiveFolder.InnerText);
                File.Copy(filefolder.InnerText + filename, ArchiveFolder.InnerText + filename, true);
                //  File.Delete(filefolder.InnerText + filename);
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
                throw new Exception(ex.ToString());
            }
        }


        private void ClearPrintDataForSt4()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(@"Config.xml");
            string printer ="ZEBRA_002";
            XmlNode printerdetails = doc.DocumentElement.SelectSingleNode(@"./PRINTER[./PRINTERNAME='" + printer + "']");
            
            try
            {
                //string[] arg = new string[4];
                //arg[0] =;       
                //arg[2] = 

                string[] arg = new string[4];
                arg[0] = printerdetails.SelectSingleNode(@"IPADDRESS").InnerText;
                //  arg[1] = filefolder.InnerText;
                arg[2] = "ClearDATA";
                //arg[3] = printerdetails.SelectSingleNode(@"IMGFOLDER").InnerText;       
                ZebraPrinter zb = new ZebraPrinter(arg);
            }
            catch (Exception ex)
            {
                log.Error("ClearPrintDataForSt4 exception");
                throw new Exception(ex.ToString());
            }
        }

        private void ClearPrintDataForSt7()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(@"Config.xml");
            string printer ="ZEBRA_003";
            XmlNode printerdetails = doc.DocumentElement.SelectSingleNode(@"./PRINTER[./PRINTERNAME='" + printer + "']");
            
            try
            {
                //string[] arg = new string[4];
                //arg[0] =;       
                //arg[2] = 

                string[] arg = new string[4];
                arg[0] = printerdetails.SelectSingleNode(@"IPADDRESS").InnerText;
                //  arg[1] = filefolder.InnerText;
                arg[2] = "ClearDATA";
                //arg[3] = printerdetails.SelectSingleNode(@"IMGFOLDER").InnerText;       
                ZebraPrinter zb = new ZebraPrinter(arg);
            }
            catch (Exception ex)
            {
                log.Error("ClearPrintDataForSt7 exception");
                throw new Exception(ex.ToString());
            }
        }
        private void ClearPrintDataForSt2()
        {
             XmlDocument doc = new XmlDocument();
            doc.Load(@"Config.xml");
            string printer ="ZEBRA_001";
            XmlNode printerdetails = doc.DocumentElement.SelectSingleNode(@"./PRINTER[./PRINTERNAME='" + printer + "']");
            
            try
            {

                //string[] arg = new string[4];
                //arg[0] =;       
                //arg[2] = 

                string[] arg = new string[4];
                arg[0] = printerdetails.SelectSingleNode(@"IPADDRESS").InnerText;
                //  arg[1] = filefolder.InnerText;
                arg[2] = "ClearDATA";
                //arg[3] = printerdetails.SelectSingleNode(@"IMGFOLDER").InnerText;       
                ZebraPrinter zb = new ZebraPrinter(arg);

            }
            catch (Exception ex)
            {
                log.Error("ClearPrintDataForSt2 exception");
                throw new Exception(ex.ToString());
            }
        }

        private void UpdateDMaddressForSt4(int surface, int X, int Y)
        {
            try
            {
                int suf = surface;
                int x = X;
                int y = Y;
                byte[] tmpbyte;
                tmpbyte = BitConverter.GetBytes(suf);
                // tmpbyte = Encoding.ASCII.GetBytes(surface);
                Array.Copy(tmpbyte, 0, PLCWriteCommand, SurfaceOFFSETst4, 2); //DM306
                byte[] tmpbyte1;
                tmpbyte1 = BitConverter.GetBytes(x);
                Array.Copy(tmpbyte1, 0, PLCWriteCommand, XOFFSETForst4, 2); //DM307
                byte[] tmpbyte2;
                tmpbyte2 = BitConverter.GetBytes(y);
                Array.Copy(tmpbyte2, 0, PLCWriteCommand, YOFFSETForst4, 2); //DM308
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        bool IsCaptureHang, IsCaptureHang2;
        private LinePackInspection OmronSt04;
         public void ST02Vision04Th(object msgobj)
        {
            
            MainNetworkClass networkmain = (MainNetworkClass)msgobj;
            string[] PrintlabelName = { "", "" };
            int tmpsol = 0;
            while (!bTerminate)
            {
                if (evt_Station04InspectionReq.WaitOne(100))
                {
                    try
                    {
                        //get filefolder
                        XmlDocument doc = new XmlDocument();
                        doc.Load(@"Config.xml");
                        XmlNode VisionIPAddress = doc.SelectSingleNode(@"/CONFIG/LABELVISIONST04/ADD");
                        XmlNode VisionPortNo = doc.SelectSingleNode(@"/CONFIG/LABELVISIONST04/PORT");
                       // XmlNode VisionImgFile = doc.SelectSingleNode(@"/CONFIG/LABELVISIONST04/IMGFOLDER");  //need to create share folder for Image
                        VisionNetworkAddress4 = "Trying to connect to " + VisionIPAddress.InnerText + " Port : " + VisionPortNo.InnerText;
                        //get label number and image file name
                        XmlNodeList nodes = networkmain.MBBdoc.SelectNodes("//FILE/FILE_NAME");
                        //get filefolder
                        string printer = networkmain.MBBdoc.SelectNodes("//BOXID/PRINTER")[0].InnerText;
                        XmlNode printerdetails = doc.DocumentElement.SelectSingleNode(@"./PRINTER[./PRINTERNAME='" + printer + "']");
                        string imagefolder = printerdetails.SelectSingleNode(@"IMGFOLDER").InnerText;
                        //get image index
                       
                        int printindex = printindex = PLCQueryRx[PLCQueryRx_DM157 + 20];//dm167;
                        if ((printindex - 1) < 0)
                        {
                            throw new Exception("Print index not set!");
                        }
                        string imagefilename = nodes[printindex - 1].InnerText;
                        imagefilename = imagefilename.Remove(imagefilename.Length - 3, 3) + "bmp";//remove extension and change to bmp
                        string imagefilename2 = imagefilename.Remove(imagefilename.Length - 4, 4);
                        if (OmronSt04 == null)
                        {
                            OmronSt04 = new LinePackInspection()
                            {
                                HostAddress = VisionIPAddress.InnerText,
                                PortNumber = int.Parse(VisionPortNo.InnerText)
                            };
                            OmronSt04.InitVision();
                            VisionNetworkAddress4 = "Connected to VisionServer " + VisionIPAddress.InnerText + " Port : " + VisionPortNo.InnerText;//connected
                            //  VisionStatus = "Vision Ready";
                        }
                        try
                        {
                            if (!OmronSt04.linepackomronsystem.Connected)
                            {
                                OmronSt04.ConnectToVision();
                            }
                            if (!OmronSt04.linepackomronsystem.Connected)
                            {
                                MyEventQ.AddQ("10;VisionPCCommunicationBreak;Station number;4");//Push message to stac
                            }
                                Thread.Sleep(50);
                            if (PLCQueryRx[PLCQueryRx_DM157 + 18] == 0x2)
                            {
                                
                                
                                //OmronSt04.LoadZPLFile(@"C:\SAPLABELS\" + nodes[printindex - 1].InnerText);
                                OM2.Info("Station 4 Vision Start" + imagefilename);
                                Thread.Sleep(50);
                                OmronSt04.linepackomronsystem.Prefix_Set(Path.GetFileNameWithoutExtension(imagefilename) + "_");
                                OmronSt04.linepackomronsystem.Scene_Switch(10);
                                OmronSt04.linepackomronsystem.UnitData_Change(1, 137, Path.GetFileNameWithoutExtension(imagefilename));
                                //if (nodes.Count == 1)
                                //{
                                //    Isspekteck = true;
                                //    tmpsol = 1;
                                //}
                                //else
                                //{
                                //    Isspekteck = false;
                                //    tmpsol = 2;
                                //}
                                //OmronSt04.spektek(Isspekteck);
                                OM2.Info("Station 4 Vision Measure_Once");
                                ScanboxidSt4RJ=ScanboxidSt4;
                                OmronSt04.linepackomronsystem.Measure_Once();
                                Thread.Sleep(200);

                                //********dISABLE AT msb
                                //OmronSt04.linepackomronsystem.SAVEBMP();
                               // OM2.Info("Station 4 SAVE BMP to RAMDisk)");
                                Thread.Sleep(100);
                                OM2.Info("Station 4 Vision Scene_Switch(2)");
                                PLCWriteCommand[PLCWriteCommand_DM380 + 4] = 0x08;
                                if (printindex == 1)
                                {
                                    PrintlabelName[0] = imagefilename2;
                                   tmpsol = 1;

                                }
                                else if (printindex == 2)
                                {
                                    PrintlabelName[1] = imagefilename2;
                                    tmpsol = 2;

                                }
                                else
                                {
                                    OM2.Info("Station 4 - Print index out of bound =" + printindex.ToString());
                                }
                                if (printindex == 1)
                                {
                                    IsCaptureHang = false;
                                }
                                else if (printindex == 2)
                                {
                                    IsCaptureHang2 = false;
                                }
                            }
                            else if(PLCQueryRx[PLCQueryRx_DM157 + 18] == 0x3 && !IsCaptureHang)
                            {
                                //ADDED BY GYLEE 13/10/2013
                                bool isIntel2 = false, Isspekteck2 = false;
                                bool[] rst = { false , false };
                                OM2.Info("Station 4 Pre-Test result1 = " + rst[0].ToString() + ", result2 = " + rst[1].ToString());
                                for (int i = 0; i < tmpsol ; i++) //Station 4 will constantly have 2 label
                                {
                                    OmronSt04.LoadZPLFile(@"C:\SAPLABELS\" + nodes[i].InnerText);
                                    //Thread.Sleep(50);



                                //ADDED BY GYLEE 13/10/2013
                                int intelnum = OmronSt04.IntelLabelCheck(@"C:\SAPLABELS\" + nodes[printindex - 1].InnerText);
                                if (intelnum == 2)
                                {
                                    isIntel2 = true;
                                }
                                else if (intelnum == 3)
                                {
                                    Isspekteck2 = true;
                                }
                                Thread.Sleep(50);
                                OmronSt04.linepackomronsystem.Scene_Switch(0);
                                OmronSt04.spektek4(Isspekteck2, isIntel2, isTray4);
                                ////added by gylee ended
                                string isThisTray = "";
                                string isSPEKTEK = "";
                                if (isTray4)
                                {
                                    isThisTray = "TRAY Recipe Used";
                                }
                                    else
	                            {
                                    isThisTray = "REEL Recipe Used";
	                            }

                                    if (Isspekteck2)
                                    {
                                        isSPEKTEK = ", use SPEKTEK rountine";
                                    }
                                    else
                                    {
                                        isSPEKTEK = ", use Normal rountine";
                                    }
                                OM2.Info("ST4 Omron Filter " + " " + PrintlabelName[i] + " " + isThisTray + isSPEKTEK);
                                networkmain.stn4log = "ST4 Omron Filter " + PrintlabelName[i] + " " + isThisTray + isSPEKTEK;


                                    string result = "none";
                                    PLCWriteCommand[PLCWriteCommand_DM380 + 4] = 0x08;
                                    OM2.Info("Station 4 Vision Scene_Switch(1)");
                                    OmronSt04.linepackomronsystem.Scene_Switch(1);

                                    if (isIntel2)
                                    {
                                        OmronSt04.linepackomronsystem.UnitData_Change(29,144,"1");
                                    }
                                    else
                                    {
                                        OmronSt04.linepackomronsystem.UnitData_Change(29, 144, "0");
                                    }
                                    OM2.Info("Station 4 Vision Scene_Switch(2)");
                                    OmronSt04.linepackomronsystem.Scene_Switch(2);
                                    OM2.Info("Station 4 Vision BCClear");
                                    //OmronSt04.MultiData_BCClear();
                                    OmronSt04.BCClear();
                                    OM2.Info("Station 4 Vision SendBCData");
                                    //OmronSt04.MultiData_BCSend();
                                    OmronSt04.SendBCData2();
                                    OM2.Info("Station 4 Vision Scene_Switch(3)");
                                    Thread.Sleep(50);
                                    OmronSt04.linepackomronsystem.Scene_Switch(3);
                                    OM2.Info("Station 4 Vision Ocr Clear2");
                                    //OmronSt04.MultiData_OcrClear();
                                    OmronSt04.BCClear2();
                                    OM2.Info("Station 4 Vision SendOcrData");
                                    //OmronSt04.MultiData_OCRLoc();
                                    OmronSt04.LoadOCRlocation();
                                    OM2.Info("Station 4 Vision AutoMeasure2, FL = " + PrintlabelName[i]); //Double snap sequence
                                    result = OmronSt04.linepackomronsystem.AutoMeasure2(PrintlabelName[i]);
                                    networkmain.stn4log = PrintlabelName[i] + " Requested";
                                    Thread.Sleep(50);
                                    OM2.Info("Station 4 Vision Result " + " " + PrintlabelName[i] + " " + result);
                                    networkmain.stn4log = PrintlabelName[i] + " " + result;
                                    //bool rst = (result == "1.0000,1.0000,1.0000");
                                    string result1 = "";
                                    string[] SingleResult = result.Split(',');
                                    if (!St4Bypass )
                                    {
                                        if (SingleResult.Length > 2) //BARCODE RETRY
                                        {
                                            if (SingleResult[2] != "1.0000")
                                            {
                                                result1 = OmronSt04.linepackomronsystem.AutoMeasure98();
                                                OM2.Info("Second Barcode Result " + imagefilename + " " + result1);
                                                networkmain.stn4log = "Second Barcode result " + result1;
                                                string BCResult = result1.Trim();
                                                if (BCResult == "1.0000")
                                                {
                                                    SingleResult[2] = "1.0000";
                                                    networkmain.stn4log = "Second BC Test " + imagefilename + " PASS";
                                                    OM2.Info("Second BC Test " + imagefilename + " PASS");
                                                }
                                                else
                                                {
                                                    networkmain.stn4log = "Second BC Test " + imagefilename + " FAIL";
                                                    OM2.Info("Second BC Test " + imagefilename + " FAIL");
                                                }
                                            }
                                        }

                                        if (SingleResult.Length == 1) // CA RETRY
                                        {                                          
                                                result1 = OmronSt04.linepackomronsystem.AutoMeasure2(PrintlabelName[i]);
                                                OM2.Info("Second CA Result " + imagefilename + " " + result1);
                                                networkmain.stn4log = "Second CA result " + result1;
                                                string BCResult = result1.Trim();
                                                SingleResult = BCResult.Split(',');

                                        }
                                        if (SingleResult.Length > 2)
                                        {
                                            MyEventQ.AddQ("19;Station4InspectionComplete;LotNumber;" + PrintlabelName[i] + ";CalibrationScore;" + SingleResult[0] +
                                            ";PatternMatchingScore;" + SingleResult[1] + ";BarcodeScore;" + SingleResult[2] + ";OcrScore;" + SingleResult[3]);//Push message to stack
                                        EvtLog.Info("19;Station4InspectionComplete;LotNumber;" + PrintlabelName[i] + ";CalibrationScore;" + SingleResult[0] +
                                            ";PatternMatchingScore;" + SingleResult[1] + ";BarcodeScore;" + SingleResult[2] + ";OcrScore;" + SingleResult[3]);
                                        }
                                        else if (SingleResult.Length == 1)
                                        {
                                            MyEventQ.AddQ("19;Station4InspectionComplete;LotNumber;" + PrintlabelName[i] + ";CalibrationScore;-1.000");//Push message to stack
                                            rst[i] = false;
                                        }
                                        else
                                        {
                                            rst[i] = false;
                                        }
                                        if (SingleResult[0] == "1.0000" && SingleResult[1] == "1.0000" && SingleResult[2] == "1.0000")
                                        {
                                            rst[i] = true;
                                        }

                                        if (Isspekteck2)
                                        {
                                            break;
                                        }
                                        //for (int j = 0; j < SingleResult.Length - 1; j++) //include OCR rsult for record,but exclude for judment
                                        //{
                                        //    if (SingleResult[j] != "1.0000")
                                        //    {
                                        //        rst[i] = false;
                                        //        break;
                                        //    }
                                        //}
                                    }
                                }
                                string dummy = OmronSt04.linepackomronsystem.BMPClear();
                                if (dummy == "-1")
                                {
                                    OM2.Info("Station 4 BMP Clear Execute");
                                }
                                if (Isspekteck2)
                                {
                                    OM2.Info("Station 4 test result1 = " + rst[0].ToString());
                                }
                                else
                                {
                                    OM2.Info("Station 4 testc result1 = " + rst[0].ToString() + ", result2 = " + rst[1].ToString());
                                }
                             
                                if ((Isspekteck2 && rst[0] == true) ||(rst[0] == true && rst[1] == true))
                                {                           
                                                                    
                                    try
                                    {
                                        while ((!networkmain.UpdateS4VisionPass(ScanboxidSt4RJ) && !bTerminate))
                                        {
                                            Thread.Sleep(100);
                                        }
                                    }
                                    catch
                                    {
                                      
                                    }
                                    if (Isspekteck2)
                                    {
                                        OM2.Info("Station 4 Vision SPEKTEK Pass " + PrintlabelName[0] );
                                        networkmain.stn4log = PrintlabelName[0] + "SPEKTEK PASS";
                                    }
                                    else
                                    {
                                        OM2.Info("Station 4 Vision Pass" + PrintlabelName[0] + " & " + PrintlabelName[1]);
                                        networkmain.stn4log = PrintlabelName[0] + " & " + PrintlabelName[1] + " PASS";
                                    }
                                    PLCWriteCommand[471] = 0x0;
                                    PLCWriteCommand[PLCWriteCommand_DM380 + 4] = 0x9;
                                }
                                else
                                {
                                    if (Isspekteck2 && rst[0] == false)
                                    {
                                        OM2.Info("Station 4 Vision SPEKTEK Fail" + PrintlabelName[0]);
                                        networkmain.stn4log = PrintlabelName[0] + " SPEKTEK Fail";
                                        networkmain.OperatorLog = "Stn.4 " + PrintlabelName[0] + " SPEKTEK rejected by vision.";
                                    }
                                    else if (rst[0] == false && rst[1] == true)
                                    {
                                        OM2.Info("Station 4 Vision Fail" + PrintlabelName[0]);
                                        networkmain.stn4log = PrintlabelName[0] + " Fail";
                                        networkmain.OperatorLog = "Stn.4 " + PrintlabelName[0] + " rejected by vision.";
                                    }
                                    else if (rst[1] == false && rst[0] == true)
                                    {
                                        OM2.Info("Station 4 Vision Fail" + PrintlabelName[1]);
                                        networkmain.stn4log = PrintlabelName[1] + " Fail";
                                        networkmain.OperatorLog = "Stn.4 " + PrintlabelName[1] + " rejected by vision.";
                                    }
                                    else
                                    {
                                        OM2.Info("Station 4 Vision Fail" + PrintlabelName[0] + " & " + PrintlabelName[1]);
                                        networkmain.stn4log = PrintlabelName[0] + " & " + PrintlabelName[1] + " Fail";
                                        networkmain.OperatorLog = "Stn.4 " + PrintlabelName[0] + " & " + PrintlabelName[1] + " rejected by vision.";
                                    }
                                    //SERVER take responsibility to update vision result before part going in ST6

                                    Station4Log.Info("Station 4 vision Reject " + ScanboxidSt4RJ);
                                    networkmain.stn4log = ScanboxidSt4RJ + " rejected by vision ST4 ";                        
                                    
                                   try
                                    {
                                        while ((!networkmain.UpdateS4VisionStat(ScanboxidSt4RJ, "RJ", "400","Fail") && !bTerminate))
                                        {
                                            Thread.Sleep(100);
                                        }
                                    }
                                    catch
                                    {
                                      
                                    }
                                   PLCWriteCommand[471] = 0x1;
                                    PLCWriteCommand[PLCWriteCommand_DM380 + 4] = 0x09;
                                }
                                OM2.Info("Station 4 Clear ImageFL " + PrintlabelName[0]);
                                OM2.Info("Station 4 Clear ImageFL " + PrintlabelName[1]);
                                PrintlabelName[0] = "";
                                PrintlabelName[1] = "";
                            }
                            else if (PLCQueryRx[PLCQueryRx_DM157 + 18] == 0x3 && (IsCaptureHang || IsCaptureHang2 ))
                            {
                                try
                                {
                                    while ((!networkmain.UpdateS4VisionStat(ScanboxidSt4RJ, "RJ", "400", "Fail") && !bTerminate))
                                    {
                                        Thread.Sleep(100);
                                    }
                                }
                                catch
                                {

                                }
                                PLCWriteCommand[471] = 0x1;
                                PLCWriteCommand[PLCWriteCommand_DM380 + 4] = 0x09;
                                OM2.Info("Station 4 Automeasure Abotr because of Capture fail");
                            }
                        }

                        catch (Exception ex)
                        {
                            IsCaptureHang = true;
                            IsCaptureHang2 = true;
                            OM2.Error(ex.ToString());
                            PLCWriteCommand[PLCWriteCommand_DM380 + 4] = 0xFF; //vision inspection Setup Error
                            OmronSt04.DisconnectVision();
                            MyEventQ.AddQ("7;VisionControllerCommunicationBreak;Stationnumber;4");//Push message to stack
                            //  VisionStatus = "vision inspection Label Error";
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        IsCaptureHang = true;
                        IsCaptureHang2 = true;
                        OM2.Error(ex.ToString());
                        PLCWriteCommand[PLCWriteCommand_DM380 + 4] = 0xFF;  //vision inspection Error
                        // OmronSt04.DisconnectVision();
                        //  VisionStatus = "vision inspection Label Error";
                    }
                    finally
                    {               
                        evt_Station04InspectionReq.Reset();
                    }

                }
            }
        }
        #endregion
        #region Station 6



         public bool CheckstringClearFor6Check(int offset, string label)  //need to add finishing label 
        {
            if (label == "\0\0\0\0\0\0\0\0\0\0")
            {
                for (int i = 0; i <= 1; i++)
                {
                    PLCWriteCommand6[offset + i] = 0;
                  //  Station7Log.Info("st7 clear offset" + offset + i);
                }
            }
            return true;
        }



       public bool CheckStringUpdateForST6Check(string Label)
        {
            if ((Label != "\0\0\0\0\0\0\0\0\0\0") && //empty string found, clear data to 0
                 (
                     ((PLCWriteCommand6[481] == 0) && (PLCWriteCommand6[482] == 0)) ||
                     ((PLCWriteCommand6[481] == 69) && (PLCWriteCommand6[482] == 82))//OK
                 ))//(offset plus 8 and 8 is OK NA flag .....ER
            {
                int tmpOeeid = 0;         
                UpdatePLCFinishingLabelDMAddressForSt6Check(Label, out tmpOeeid);
               // rq.UpdStNobyID(6, tmpOeeid);

            }
            return true;
        }


       public bool UpdatePLCFinishingLabelDMAddressForSt6Check(string strFinishingLabel, out int OEEID)
        {
            try
            {
                //tracking label check
                string tmpstr;
                byte[] tmpbyte;
               XmlNode fltrackingroot = networkmain.FLTrackingdoc.DocumentElement;//this house the list of summarized xml details 
                XmlNode selectednode = fltrackingroot.SelectSingleNode("descendant::LABEL[ID='" + strFinishingLabel + "']");
                if (selectednode == null)
                    throw new Exception("no selectednode");
              
            
               networkmain.linePack.Info("Station 6 Check Finishing Label at Conveyor side " + strFinishingLabel);


                string tmpstr1;
                string tmpstr2;
                string tmpstr3;
                byte[] tmpbyte1;
                byte[] tmpbyte2;
                byte[] tmpbyte3;
                tmpstr1 = selectednode.SelectSingleNode("PackageStatus").InnerText;//D5430
                tmpbyte1 = new byte[tmpstr1.Length];
                tmpbyte1 = Encoding.ASCII.GetBytes(tmpstr1);

                OEEID = int.Parse(selectednode.SelectSingleNode("OEEid").InnerText);
                tmpstr2 = selectednode.SelectSingleNode("SealerResult").InnerText;//D5430
                tmpbyte2 = new byte[tmpstr2.Length];
                tmpbyte2 = Encoding.ASCII.GetBytes(tmpstr2);

                tmpstr3 = selectednode.SelectSingleNode("SealerResultReason").InnerText;//D5430
                tmpbyte3 = new byte[tmpstr3.Length];
                tmpbyte3 = Encoding.ASCII.GetBytes(tmpstr3);



                tmpstr = selectednode.SelectSingleNode("TYPE").InnerText;//D
                tmpbyte = new byte[tmpstr.Length];
                tmpbyte = Encoding.ASCII.GetBytes(tmpstr);
                if (tmpstr == "NA")
                {
                   tmpbyte = new byte[2];
                    tmpbyte = Encoding.ASCII.GetBytes("NA");
                    Array.Copy(tmpbyte, 0, PLCWriteCommand6,481,2);//D5430
                   networkmain.linePack.Info("Station 6 Check Finishing Label NA at Conveyor side " + strFinishingLabel);
                }
                else if (tmpstr1 == "RJ")
                {
		             tmpbyte = new byte[2];
                    tmpbyte = Encoding.ASCII.GetBytes("RJ");
                    Array.Copy(tmpbyte, 0, PLCWriteCommand6, 481,2);
                    networkmain.linePack.Info("Station 6 Check Finishing Label RJ at Conveyor side "  + strFinishingLabel);
                }
                //else if (tmpstr2 == "NA")
                //{

                //    UpdatePLCFinishingLabelFor6Reject(strFinishingLabel, "995", "Middleware Sealer result Not come out on time");

                //    tmpbyte = new byte[2];
                //    tmpbyte = Encoding.ASCII.GetBytes("RJ");
                //    Array.Copy(tmpbyte, 0, PLCWriteCommand6, 481, 2);
                //    networkmain.linePack.Info("Station 6 Check Finishing Label,No Result From Middleware(Result Timeout) at Conveyor side " + strFinishingLabel);
                //}
                //else if (tmpstr2 == "FAIL")
                //{
                //    UpdatePLCFinishingLabelFor6Reject(strFinishingLabel, "994", tmpstr3);
                //    tmpbyte = new byte[2];
                //    tmpbyte = Encoding.ASCII.GetBytes("RJ");
                //    Array.Copy(tmpbyte, 0, PLCWriteCommand6, 481, 2);
                //    networkmain.linePack.Info("Station 6 Check Finishing Label,Reject From Middleware at Conveyor side " + strFinishingLabel + "," + tmpstr3);
                //}
                else
                {
                    tmpbyte = new byte[2];
                    tmpbyte = Encoding.ASCII.GetBytes("OK");
                    Array.Copy(tmpbyte, 0, PLCWriteCommand6,481,2);//D5420
                    networkmain.linePack.Info("Station 6 Check Finishing Label OK at Conveyor side "  + strFinishingLabel);
                }               
                          
             
               
            }
            catch 
            {
              //  log.Error("UpdatePLCFinishingLabelDMAddressFor7ESD exception");
                networkmain.linePack.Info("Update Finishing Label On PLC Fail for Station 6 Label at conveyor side = " + strFinishingLabel);
                byte[] tmpbytest7;             

                tmpbytest7 = new byte[2];
                tmpbytest7 = Encoding.ASCII.GetBytes("ER");
                Array.Copy(tmpbytest7, 0, PLCWriteCommand6, 481, 2);//D5420
                networkmain.linePack.Info("Station 6 Check Finishing Label ER at Conveyor side "  + strFinishingLabel);
                OEEID = 0;
                return false;
            }
            return true;
        }



















        public void UpdatePLCFinishingLabelFor6Reject(string strFinishingLabel,string RejectCode,string RejectReason)
        {
            try
            {
                XmlNode fltrackingroot = networkmain.FLTrackingdoc.DocumentElement;//this house the list of summarized xml details 
                XmlNode selectednode = fltrackingroot.SelectSingleNode("descendant::LABEL[ID='" + strFinishingLabel + "']");
                if (selectednode == null)
                    throw new Exception("no selectednode");              


                    selectednode.SelectSingleNode("PackageStatus").InnerText = "RJ";
                    selectednode.SelectSingleNode("ErrorCode").InnerText = RejectCode;
                    selectednode.SelectSingleNode("SealerResultReason").InnerText = RejectReason;

            }
            catch 
            {
               
            }
        }















        public void rescan2()
        {
            PLCWriteCommand6[PLCWriteCommand_DM5204 + 2] = 0x00;
        }
        public void rescan()
        {
            PLCWriteCommand6[PLCWriteCommand_DM5204] = 0x00;
        }
        public bool CheckstringClearForTrackingData(int offset, string label)   //for barcode transfer from PLC1 to PLC2
        {
            if (label == "\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0")
            {
                for (int i = 0; i <= 19; i++)
                {
                    PLCWriteCommand6[offset + i] = 0;
                }
            }
            return true;
        }
        public bool CheckStringClearFor6(int offset/*offset from PLCreaddata*/, //offset data 41 == DM5210, 103 = DM5241, 163 = DM5271 
                                    string RotaryLabel)//check if string is empty and requires to clear happens in OK and NA status
        {
            if (RotaryLabel == "\0\0\0\0\0\0\0\0\0\0")  //empty string found, clear data to 0
            {
                for (int i = 0; i <= 57; i++)
                {
                    PLCWriteCommand6[offset + i] = 0;
                }
            }
            return true;
        }
        public bool CheckStringClearFor6_OP2(int offset/*offset from PLCreaddata*/, //offset data 41 == DM5210, 103 = DM5241, 163 = DM5271 
                                    string RotaryLabel)//check if string is empty and requires to clear happens in OK and NA status
        {
            if (RotaryLabel == "\0\0\0\0\0\0\0\0\0\0")  //empty string found, clear data to 0
            {
                for (int i = 0; i <= 57; i++)
                {
                    PLCWriteCommand6[offset + i] = 0;
                }
            }
            return true;
        }
        public bool CheckStringClearFor6_OP1(int offset/*offset from PLCreaddata*/, //offset data 41 == DM5210, 103 = DM5241, 163 = DM5271 
                                    string RotaryLabel)//check if string is empty and requires to clear happens in OK and NA status
        {
            if (RotaryLabel == "\0\0\0\0\0\0\0\0\0\0")  //empty string found, clear data to 0
            {
                for (int i = 0; i <= 57; i++)
                {
                    PLCWriteCommand6[offset + i] = 0;
                }
            }
            return true;
        }
        public bool CheckStringClearFor5(int offset/*offset from PLCreaddata*/, //offset data 41 == DM5210, 103 = DM5241, 163 = DM5271 
                                   string RotaryLabel)//check if string is empty and requires to clear happens in OK and NA status
        {
            if (RotaryLabel == "\0\0\0\0\0\0\0\0\0\0")  //empty string found, clear data to 0
            {
                for (int i = 0; i <= 57; i++)
                {
                    PLCWriteCommand6[offset + i] = 0;
                }
            }
            return true;
        }

        public bool CheckStringUpdateFor6_OP1(int offset/*offset from PLCreaddata*/,//offset data 41 == DM210, 103 = DM241, 163 = DM271  
                                     string RotaryLabel)//check if string is avaliable during empty status /0/0 and NA
        {
            bool status = false;
            if ((RotaryLabel != "\0\0\0\0\0\0\0\0\0\0") && //empty string found, clear data to 0
                (
                    ((PLCWriteCommand6[offset + 54] == 0) && (PLCWriteCommand6[offset + 55] == 0)) ||//either 00 or ER
                    ((PLCWriteCommand6[offset + 54] == 69) && (PLCWriteCommand6[offset + 55] == 82))
                ))//(offset plus 54 and 55 is OK NA flag .....ER
            {
                //Update PLC
                byte[] plcbuffer;
                status = UpdatePLCFinishingLabelDMAddressFor6_OP1(out plcbuffer, RotaryLabel);//this inclue update 54 55 status flag.. query for the string and do update
                Array.Copy(plcbuffer, 0, PLCWriteCommand6, offset, 58);//PLCWRITE 41 is DM210
            }
            return status;
        }
        public bool CheckStringUpdateFor6_OP2(int offset/*offset from PLCreaddata*/,//offset data 41 == DM210, 103 = DM241, 163 = DM271  
                                     string RotaryLabel)//check if string is avaliable during empty status /0/0 and NA
        {
            bool status = false;
            if ((RotaryLabel != "\0\0\0\0\0\0\0\0\0\0") && //empty string found, clear data to 0
                (
                    ((PLCWriteCommand6[offset + 54] == 0) && (PLCWriteCommand6[offset + 55] == 0)) ||//either 00 or ER
                    ((PLCWriteCommand6[offset + 54] == 69) && (PLCWriteCommand6[offset + 55] == 82))
                ))//(offset plus 54 and 55 is OK NA flag .....ER
            {
                //Update PLC
                byte[] plcbuffer;
                status = UpdatePLCFinishingLabelDMAddressFor6_OP2(out plcbuffer, RotaryLabel);//this inclue update 54 55 status flag.. query for the string and do update
                Array.Copy(plcbuffer, 0, PLCWriteCommand6, offset, 58);//PLCWRITE 41 is DM210
            }
            return status;
        }
        //this function update ER, RJ,OK code for station 6
        public bool UpdateRJStatusDMAddressFor6(out byte[] PLCBufferMemory, string status)
        {
            PLCBufferMemory = new byte[60];
            string settoER = "ER";
            byte[] tmpbyte = Encoding.ASCII.GetBytes(settoER);
            if (settoER == "RJ")
            {
                Array.Copy(tmpbyte, 0, PLCBufferMemory, 54, settoER.Length);
            }
            return true;
        }
        public bool UpdatePLCFinishingLabelDMAddressFor6_OP1(out byte[] PLCBufferMemory, string strFinishingLabel)
        {
            PLCBufferMemory = new byte[60];
            try
            {
                string tmpstr;
                byte[] tmpbyte;
                XmlNode fltrackingroot = networkmain.FLTrackingdoc.DocumentElement;//this house the list of summarized xml details 
                XmlNode selectednode = fltrackingroot.SelectSingleNode("descendant::LABEL[ID='" + strFinishingLabel + "']");
                // selectednode.SelectSingleNode("STATION").InnerText = "6"; 

                SealerNumberQC1 = selectednode.SelectSingleNode("SealerNumber").InnerText;
                string tmpstr1;
                byte[] tmpbyte1;
                tmpstr1 = selectednode.SelectSingleNode("PackageStatus").InnerText;
                tmpbyte1 = new byte[tmpstr1.Length];
                tmpbyte1 = Encoding.ASCII.GetBytes(tmpstr1);



                tmpstr = selectednode.SelectSingleNode("TYPE").InnerText;
                tmpbyte = new byte[tmpstr.Length];
                tmpbyte = Encoding.ASCII.GetBytes(tmpstr);
                if (tmpstr == "NA")
                {
                    Array.Copy(tmpbyte, 0, PLCBufferMemory, 54, tmpstr.Length);
                   networkmain.linePack.Info("Station 6 Finishing  Label NA " + strFinishingLabel);
                    networkmain.stn6log = strFinishingLabel + " is NA";
                }
                else if (tmpstr1 == "RJ")
                {
                    Array.Copy(tmpbyte1, 0, PLCBufferMemory, 54, tmpstr1.Length);
                 networkmain.linePack.Info("Station 6 Finishing  Label RJ " + strFinishingLabel);
                    networkmain.stn6log = strFinishingLabel + " is RJ";
                }
                else
                {
                    tmpbyte = new byte[2];
                    tmpbyte = Encoding.ASCII.GetBytes("OK");
                    Array.Copy(tmpbyte, 0, PLCBufferMemory, 54, 2);
                   networkmain.linePack.Info("Station 6 Finishing  Label OK " + strFinishingLabel);
                    networkmain.stn6log = strFinishingLabel + "Label OK";
                    MyEventQ.AddQ("25;Station6SealedMBBArrivalAtQCStation;LotNumber;" + strFinishingLabel + ";QcstationNumber;1");
                }
                tmpbyte = new byte[tmpstr.Length];
                tmpbyte = Encoding.ASCII.GetBytes(tmpstr);
                //write to DM(use 3 DM fro 6 bytes)
                Array.Copy(tmpbyte, 0, PLCBufferMemory, 0, tmpstr.Length);
              //  log.Info("OP 1 update FL " + strFinishingLabel + "," + tmpstr);
                // need to check for speck-tex label

                tmpstr = selectednode.SelectSingleNode("SPTK_MARKED").InnerText; //change here
                tmpbyte = new byte[tmpstr.Length];
                tmpbyte = Encoding.ASCII.GetBytes(tmpstr);
                Array.Copy(tmpbyte, 0, PLCBufferMemory, 6, tmpstr.Length);
                tmpstr = selectednode.SelectSingleNode("REELHEIGHT").InnerText;
                int reel = int.Parse(tmpstr);
                tmpbyte = new byte[tmpstr.Length];
                tmpbyte = BitConverter.GetBytes(reel);
                //write to DM
                Array.Copy(tmpbyte, 0, PLCBufferMemory, 12, tmpstr.Length);
                tmpstr = selectednode.SelectSingleNode("TRAYNO").InnerText;
                int tray = int.Parse(tmpstr);
                tmpbyte = new byte[tmpstr.Length];
                tmpbyte = BitConverter.GetBytes(tray);
                //write to DM
                Array.Copy(tmpbyte, 0, PLCBufferMemory, 18, tmpstr.Length);
            }
            catch 
            {
                //log.Error("UpdatePLCFinishingLabelDMAddressFor6_OP1 exception");
               // log6.Error("Update Finishing Label On PLC Fail for Label = " + strFinishingLabel);
                networkmain.stn6log = strFinishingLabel + " updated PLC as fail";
                byte[] tmpbyte = new byte[2];
                tmpbyte = Encoding.ASCII.GetBytes("ER");
                Array.Copy(tmpbyte, 0, PLCBufferMemory, 54, 2);
                //log6.Info("Station 6 Finishing  Label ER " + strFinishingLabel);
                return false;
            }
            return true;
        }
        public bool UpdatePLCFinishingLabelDMAddressFor6_OP2(out byte[] PLCBufferMemory, string strFinishingLabel)
        {
            PLCBufferMemory = new byte[60];
            try
            {

                
                string tmpstr;
                byte[] tmpbyte;
                XmlNode fltrackingroot = networkmain.FLTrackingdoc.DocumentElement;//this house the list of summarized xml details 
                XmlNode selectednode = fltrackingroot.SelectSingleNode("descendant::LABEL[ID='" + strFinishingLabel + "']");
                // selectednode.SelectSingleNode("STATION").InnerText = "6"; 


                SealerNumberQC2 = selectednode.SelectSingleNode("SealerNumber").InnerText;
                string tmpstr1;
                byte[] tmpbyte1;
                tmpstr1 = selectednode.SelectSingleNode("PackageStatus").InnerText;
                tmpbyte1 = new byte[tmpstr1.Length];
                tmpbyte1 = Encoding.ASCII.GetBytes(tmpstr1);




                tmpstr = selectednode.SelectSingleNode("TYPE").InnerText;
                tmpbyte = new byte[tmpstr.Length];
                tmpbyte = Encoding.ASCII.GetBytes(tmpstr);
                if (tmpstr == "NA")
                {
                    Array.Copy(tmpbyte, 0, PLCBufferMemory, 54, tmpstr.Length);
                    networkmain.linePack.Info("Station 6 Operator 2 Finishing  Label NA " + strFinishingLabel);
                    networkmain.stn6log = strFinishingLabel + " OP2 Label NA";
                    // networkmain.stn6log = strFinishingLabel + " is NA at OP2";

                }
                else if (tmpstr1 == "RJ")
                {
                    Array.Copy(tmpbyte1, 0, PLCBufferMemory, 54, tmpstr1.Length);
                    networkmain.linePack.Info("Station 6 Operator 2 Finishing  Label RJ " + strFinishingLabel);
                    networkmain.stn6log = strFinishingLabel + " OP2 is RJ";
                }
                else
                {
                    tmpbyte = new byte[2];
                    tmpbyte = Encoding.ASCII.GetBytes("OK");
                    Array.Copy(tmpbyte, 0, PLCBufferMemory, 54, 2);
                   networkmain.linePack.Info("Station 6 Operator 2 Finishing  Label OK " + strFinishingLabel);
                    networkmain.stn6log = strFinishingLabel + " OP2 Label OK";
                    MyEventQ.AddQ("25;Station6SealedMBBArrivalAtQCStation;LotNumber;" + strFinishingLabel + ";QcstationNumber;2");
                }
                tmpbyte = new byte[tmpstr.Length];
                tmpbyte = Encoding.ASCII.GetBytes(tmpstr);
                //write to DM(use 3 DM fro 6 bytes)
                Array.Copy(tmpbyte, 0, PLCBufferMemory, 0, tmpstr.Length);
               // log.Info("OP 1 update FL " + strFinishingLabel + ", " + tmpstr);

                // need to check for speck-tex label
                tmpstr = selectednode.SelectSingleNode("SPTK_MARKED").InnerText;
                tmpbyte = new byte[tmpstr.Length];
                tmpbyte = Encoding.ASCII.GetBytes(tmpstr);
                Array.Copy(tmpbyte, 0, PLCBufferMemory, 6, tmpstr.Length);
                tmpstr = selectednode.SelectSingleNode("REELHEIGHT").InnerText;
                int reel = int.Parse(tmpstr);
                tmpbyte = new byte[tmpstr.Length];
                tmpbyte = BitConverter.GetBytes(reel);
                //write to DM
                Array.Copy(tmpbyte, 0, PLCBufferMemory, 12, tmpstr.Length);
                tmpstr = selectednode.SelectSingleNode("TRAYNO").InnerText;
                int tray = int.Parse(tmpstr);
                tmpbyte = new byte[tmpstr.Length];
                tmpbyte = BitConverter.GetBytes(tray);
                //write to DM
                Array.Copy(tmpbyte, 0, PLCBufferMemory, 18, tmpstr.Length);
            }
            catch
            {
               // log.Error("UpdatePLCFinishingLabelDMAddressFor6_OP2 exception");
              //  log6_1.Error("Update Finishing Label On PLC Fail for Label = " + strFinishingLabel);
                networkmain.stn6log = strFinishingLabel + " Updated PLC as Fail";
                byte[] tmpbyte = new byte[2];
                tmpbyte = Encoding.ASCII.GetBytes("ER");
                Array.Copy(tmpbyte, 0, PLCBufferMemory, 54, 2);
                log6_1.Info("Station 6 Operator 2 Finishing  Label ER " + strFinishingLabel);
                networkmain.stn6log = strFinishingLabel + " ER";
                return false;
            }
            return true;
        }

        public bool CheckSPTKLabelStation6_OP1(string strFinishingLabel)
        {
            try
            {
                //string tmpstr; //what is this for #questionforpon
                XmlNode fltrackingroot = networkmain.FLTrackingdoc.DocumentElement;//this house the list of summarized xml details 
                XmlNode selectednode = fltrackingroot.SelectSingleNode("descendant::LABEL[ID='" + strFinishingLabel + "']");
                if (selectednode != null)
                {
                    Station6OP1_TYPE = selectednode.SelectSingleNode("TYPE").InnerText;
                    string b = selectednode.SelectSingleNode("SPTK_MARKED").InnerText;
                    if (b == null)
                    {
                        Station6OP1SpeckTeK = "NO";
                    }
                    else
                        Station6OP1SpeckTeK = b;
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public bool CheckSPTKLabelStation6_OP2(string strFinishingLabel)
        {
            try
            {
                //string tmpstr;
                XmlNode fltrackingroot = networkmain.FLTrackingdoc.DocumentElement;//this house the list of summarized xml details 
                XmlNode selectednode = fltrackingroot.SelectSingleNode("descendant::LABEL[ID='" + strFinishingLabel + "']"); if (selectednode != null)
                {
                    Station6OP2_TYPE = selectednode.SelectSingleNode("TYPE").InnerText;
                    string a = selectednode.SelectSingleNode("SPTK_MARKED").InnerText;
                    if (a == null)
                    {
                        Station6OP2SpeckTeK = "NO";
                    }
                    else
                        Station6OP2SpeckTeK = a;

                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }

        }

        #endregion
        #region Station 5
        public bool CheckstringClearForPLC1toPLC2(int offset, string label)
        {
            if (label == "\0\0\0\0\0\0\0\0\0\0")
            {
                for (int i = 0; i <= 9; i++)
                {
                    PLCWriteCommand6[offset + i] = 0;
                }
            }
            return true;
        }
        public bool CheckStringUpdateFor5(int offset/*offset from PLCreaddata*/,
                                   string RotaryLabel)//check if string is avaliable during empty status /0/0 and NA
        {
            if ((RotaryLabel != "\0\0\0\0\0\0\0\0\0\0") && //empty string found, clear data to 0
                (
                    ((PLCWriteCommand6[offset + 54] == 0) && (PLCWriteCommand6[offset + 55] == 0)) ||
                    ((PLCWriteCommand6[offset + 54] == 69) && (PLCWriteCommand6[offset + 55] == 82))
                ))//(offset plus 54 and 55 is OK NA flag .....ER
            {
                //Update PLC
                byte[] plcbuffer;
                int tmpOeeid = 0;
                UpdatePLCFinishingLabelDMAddressFor5(out plcbuffer, RotaryLabel, out tmpOeeid);//this inclue update 54 55 status flag
                rq.UpdStNobyID(5, tmpOeeid);

                Array.Copy(plcbuffer, 0, PLCWriteCommand6, offset, 58);//PLCWRITE 41 is DM210
            }
            return true;
        }
        public bool CheckeRJStatusForSealer1(string strFinishingLabel)
        {   string tmpstr1;
            string tmpstr;
            string errorcode;
            byte[] tmpbyte;
            XmlNode fltrackingroot = networkmain.FLTrackingdoc.DocumentElement;//this house the list of summarized xml details 
            XmlNode selectednode = fltrackingroot.SelectSingleNode("descendant::LABEL[ID='" + strFinishingLabel + "']");

            tmpstr1 = selectednode.SelectSingleNode("St4vision").InnerText;
            errorcode=selectednode.SelectSingleNode("ErrorCode").InnerText;
         
            tmpstr = selectednode.SelectSingleNode("PackageStatus").InnerText;
            tmpbyte = new byte[tmpstr.Length];
            tmpbyte = Encoding.ASCII.GetBytes(tmpstr);
            if (tmpstr == "RJ" || tmpstr1=="NA" || tmpstr1=="Fail")
            {
              
              if (tmpstr == "RJ")
              {
                Sealer1Log.Info("Station 5 sealer 1 Finishing Label RJ from other Station " + strFinishingLabel+","+errorcode);
                RJResultst5S1=errorcode;
              }

              
            else  if (tmpstr1=="NA")
              {
                Sealer1Log.Info("Station 5 sealer 1 Finishing Label RJ from  Station4 vision result timeout" + strFinishingLabel+","+tmpstr1);
                RJResultst5S1="508";
                
              }


             else  if (tmpstr1=="Fail")
              {
                Sealer1Log.Info("Station 5 sealer 1 Finishing Label RJ from  Station4 vision result Fail" + strFinishingLabel+","+tmpstr1);
                 RJResultst5S1="511";

              }

              return false;
            }
            return true;
        }

        public bool CheckeRJStatusForSealer2(string strFinishingLabel)
        {
            string tmpstr;
            byte[] tmpbyte;
            string errorcode;
            XmlNode fltrackingroot = networkmain.FLTrackingdoc.DocumentElement;//this house the list of summarized xml details 
            XmlNode selectednode = fltrackingroot.SelectSingleNode("descendant::LABEL[ID='" + strFinishingLabel + "']");
            string tmpstr1;
            tmpstr1 = selectednode.SelectSingleNode("St4vision").InnerText;
            errorcode=selectednode.SelectSingleNode("ErrorCode").InnerText;
            tmpstr = selectednode.SelectSingleNode("PackageStatus").InnerText;
            tmpbyte = new byte[tmpstr.Length];
            tmpbyte = Encoding.ASCII.GetBytes(tmpstr);
           if (tmpstr == "RJ" || tmpstr1=="NA" || tmpstr1=="Fail")
            {
                
              if (tmpstr == "RJ")
              {
                Sealer2Log.Info("Station 5 sealer 2 Finishing Label RJ from other Station " + strFinishingLabel+","+errorcode);
                RJResultst5S2=errorcode;
              }

              
            else  if (tmpstr1=="NA")
              {
                Sealer2Log.Info("Station 5 sealer 2 Finishing Label RJ from  Station4 vision result timeout" + strFinishingLabel+","+tmpstr1);
                RJResultst5S2="509";
                
              }


             else  if (tmpstr1=="Fail")
              {
                Sealer2Log.Info("Station 5 sealer 2 Finishing Label RJ from  Station4 vision result Fail" + strFinishingLabel+","+tmpstr1);
                 RJResultst5S2="512";

              }

                return false;
            }
            return true;
        }

        public bool CheckeRJStatusForSealer3(string strFinishingLabel)
        {
            string tmpstr;
            byte[] tmpbyte;
            string errorcode;
            XmlNode fltrackingroot = networkmain.FLTrackingdoc.DocumentElement;//this house the list of summarized xml details 
            XmlNode selectednode = fltrackingroot.SelectSingleNode("descendant::LABEL[ID='" + strFinishingLabel + "']");
            string tmpstr1;
            tmpstr1 = selectednode.SelectSingleNode("St4vision").InnerText;

            errorcode=selectednode.SelectSingleNode("ErrorCode").InnerText;
            tmpstr = selectednode.SelectSingleNode("PackageStatus").InnerText;
            tmpbyte = new byte[tmpstr.Length];
            tmpbyte = Encoding.ASCII.GetBytes(tmpstr);
            if (tmpstr == "RJ" || tmpstr1=="NA" || tmpstr1=="Fail")
            {
               
                 if (tmpstr == "RJ")
              {
                Sealer3Log.Info("Station 5 sealer 3 Finishing Label RJ from other Station " + strFinishingLabel+","+errorcode);
                RJResultst5S3=errorcode;
              }

              
            else  if (tmpstr1=="NA")
              {
                Sealer3Log.Info("Station 5 sealer 3 Finishing Label RJ from  Station4 vision result timeout" + strFinishingLabel+","+tmpstr1);
                RJResultst5S3="510";
                
              }


             else  if (tmpstr1=="Fail")
              {
                Sealer3Log.Info("Station 5 sealer 3 Finishing Label RJ from  Station4 vision result Fail" + strFinishingLabel+","+tmpstr1);
                 RJResultst5S3="514";

              }
                return false;
            }
            return true;
        }

        public bool UpdatePLCFinishingLabelDMAddressFor5(out byte[] PLCBufferMemory, string strFinishingLabel, out int OEEID)
        {
            PLCBufferMemory = new byte[60];
            try
            {

                string tmpstr;
                byte[] tmpbyte;
                XmlNode fltrackingroot = networkmain.FLTrackingdoc.DocumentElement;//this house the list of summarized xml details 
                XmlNode selectednode = fltrackingroot.SelectSingleNode("descendant::LABEL[ID='" + strFinishingLabel + "']");
                // selectednode.SelectSingleNode("STATION").InnerText = "5";

                 string tmpstr1;
                byte[] tmpbyte1;
                tmpstr1 = selectednode.SelectSingleNode("PackageStatus").InnerText;//D5298
                tmpbyte1 = new byte[tmpstr1.Length];
                tmpbyte1 = Encoding.ASCII.GetBytes(tmpstr1);

                OEEID = int.Parse(selectednode.SelectSingleNode("OEEid").InnerText);

                tmpstr = selectednode.SelectSingleNode("TYPE").InnerText;//D
                tmpbyte = new byte[tmpstr.Length];
                tmpbyte = Encoding.ASCII.GetBytes(tmpstr);
                if (tmpstr == "NA")
                {
                    Array.Copy(tmpbyte, 0, PLCBufferMemory, 54, tmpstr.Length);
                 // Station5Log.Info("Station 5 Finishing Label NA " + strFinishingLabel);

                    networkmain.linePack.Info("Station 5 Finishing Label NA " + strFinishingLabel);
                    networkmain.stn5log = strFinishingLabel + " is NA";
                }
                else if (tmpstr1 == "RJ")
                {
                    Array.Copy(tmpbyte1, 0, PLCBufferMemory, 54, tmpstr1.Length);
                    // Station5Log.Info("Station 5 Finishing Label RJ " + strFinishingLabel);
                  networkmain.linePack.Info("Station 5 Finishing Label RJ " + strFinishingLabel);
                }
                else
                {
                    tmpbyte = new byte[2];
                    tmpbyte = Encoding.ASCII.GetBytes("OK");
                    Array.Copy(tmpbyte, 0, PLCBufferMemory, 54, 2);
                   // Station5Log.Info("Station 5 Finishing Label OK " + strFinishingLabel);
                    networkmain.linePack.Info("Station 5 Finishing Label OK " + strFinishingLabel);
                    networkmain.stn5log = strFinishingLabel + " is OK";
                }


                tmpbyte = new byte[tmpstr.Length];
                tmpbyte = Encoding.ASCII.GetBytes(tmpstr);
                //write to DM5271(use 3 DM fro 6 bytes)
                Array.Copy(tmpbyte, 0, PLCBufferMemory, 0, tmpstr.Length);
                
                tmpstr = selectednode.SelectSingleNode("REELHEIGHT").InnerText;
                int reel = int.Parse(tmpstr);
                tmpbyte = new byte[tmpstr.Length];
                tmpbyte = BitConverter.GetBytes(reel);
                //write to DM5274
                Array.Copy(tmpbyte, 0, PLCBufferMemory, 6, tmpstr.Length);
                tmpstr = selectednode.SelectSingleNode("TRAYNO").InnerText;
                int tray = int.Parse(tmpstr);
                tmpbyte = new byte[tmpstr.Length];
                tmpbyte = BitConverter.GetBytes(tray);
                //write to DM5277
                Array.Copy(tmpbyte, 0, PLCBufferMemory, 12, tmpstr.Length);

                tmpstr = selectednode.SelectSingleNode("HIC_REQUIRED").InnerText;
                // int tray = int.Parse(tmpstr);
                tmpbyte = new byte[tmpstr.Length];
                tmpbyte = Encoding.ASCII.GetBytes(tmpstr);
                //write to DM5280
                Array.Copy(tmpbyte, 0, PLCBufferMemory, 18, tmpstr.Length);


                tmpstr = selectednode.SelectSingleNode("TRAY_THICKNESS").InnerText;
                //  int trayThickness = int.Parse(tmpstr);        
                //  tmpbyte = BitConverter.GetBytes(tmpstr);
                tmpbyte = new byte[tmpstr.Length];
                tmpbyte = Encoding.ASCII.GetBytes(tmpstr);
                //write to DM5283
                Array.Copy(tmpbyte, 0, PLCBufferMemory, 24, tmpstr.Length);
            }
            catch 
            {
               //log.Error("UpdatePLCFinishingLabelDMAddressFor5 exception");
               // Station5Log.Error("Update Finishing Label On PLC Fail for Station 5 Label = " + strFinishingLabel);


                byte[] tmpbyte = new byte[2];
                tmpbyte = Encoding.ASCII.GetBytes("ER");
                Array.Copy(tmpbyte, 0, PLCBufferMemory, 54, 2); //D5298
                //Station5Log.Info("Station 5 Finishing Label ER " + strFinishingLabel);
                 networkmain.linePack.Info("Station 5 Finishing Label ER " + strFinishingLabel);
                OEEID = 0;
                return false;
            }
            return true;
        }
        public SerialPort OP1CognexScanner;
        public SerialPort OP3CognexScanner;
        //public CognexHandheldScanner OP1CognexScanner;
        //        public CognexHandheldScanner OP2CognexScanner;
        public SerialPort OP2CognexScanner;
        public InnovacVacuumSealer VS1VacuumSealer;
        public InnovacVacuumSealer VS2VacuumSealer;
        public InnovacVacuumSealer VS3VacuumSealer;
        public int X;
        public int Y;
        #endregion
    }
}
