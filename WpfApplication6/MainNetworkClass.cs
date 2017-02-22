using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.ComponentModel;
using System.Threading;
using System.Net.Sockets;
using NLog;
using InnovacVacuumSealerPackage;
using System.IO;
using System.Windows.Threading;
using System.Windows;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Collections.ObjectModel;


namespace Station
{
    public enum StationNumber
    {
        Station01 = 0,
        Station02 = 1,
        Station03 = 2,
        Station04 = 3,
        Station05 = 4,
        Station06Operator1 = 5,
        Station06Operator2 = 6,
        Station07 = 7,
        Station08 = 8
    }
}

namespace IGTwpf
{
    public class MyAsyncInfo
    {
        public Byte[] ByteArray { get; set; }
        public Stream MyStream { get; set; }

        public MyAsyncInfo(Byte[] array, Stream stream)
        {
            ByteArray = array;
            MyStream = stream;
        }
    }
    // Holds an object and uses a semephore to guarantee that
    // the object is only accessed one and only one time.
    public class LockEncapsulator<T>
    {
        private readonly T targetObject;
        private Semaphore semLock;
        public LockEncapsulator(T target)
        {
            targetObject = target;
            // 1 object, for only 1 operation.
            semLock = new Semaphore(1, 1);
        }
        // Called by the consumer to start.
        public T InstanceGet()
        {
            if (!semLock.WaitOne(100))
                return default(T);
            else
                return targetObject;
        }
        // Called by the consumer when done.
        public void InstanceReturn()
        {
            semLock.Release();
        }
    }

    partial class MainNetworkClass : INotifyPropertyChanged
    {
        #region Declaration for Variables for Critical Errors
        //Critical Error Strings
        public String _CriticalErrors0 = "";
        public String CriticalErrors0 { get { return _CriticalErrors0; } set { _CriticalErrors0 = value; OnPropertyChanged("CriticalErrors0"); } }

        public String _CriticalErrors1 = "";
        public String CriticalErrors1 { get { return _CriticalErrors1; } set { _CriticalErrors1 = value; OnPropertyChanged("CriticalErrors1"); } }

        private String _CriticalErrors2 = "";
        public String CriticalErrors2 { get { return _CriticalErrors2; } set { _CriticalErrors2 = value; OnPropertyChanged("CriticalErrors2"); } }

        public String _CriticalErrors3 = "";
        public String CriticalErrors3 { get { return _CriticalErrors3; } set { _CriticalErrors3 = value; OnPropertyChanged("CriticalErrors3"); } }

        public String _CriticalErrors4 = "";
        public String CriticalErrors4 { get { return _CriticalErrors4; } set { _CriticalErrors4 = value; OnPropertyChanged("CriticalErrors4"); } }

        public String _CriticalErrors5 = "";
        public String CriticalErrors5 { get { return _CriticalErrors5; } set { _CriticalErrors5 = value; OnPropertyChanged("CriticalErrors5"); } }

        public String _CriticalErrors6 = "";
        public String CriticalErrors6 { get { return _CriticalErrors6; } set { _CriticalErrors6 = value; OnPropertyChanged("CriticalErrors6"); } }

        public String _CriticalErrors7 = "";
        public String CriticalErrors7 { get { return _CriticalErrors7; } set { _CriticalErrors7 = value; OnPropertyChanged("CriticalErrors7"); } }


        public String _CriticalErrors8 = "";
        public String CriticalErrors8 { get { return _CriticalErrors8; } set { _CriticalErrors8 = value; OnPropertyChanged("CriticalErrors8"); } }

        //Critical Error Message Visibility
        public object CriticalError0_Visible { get { if (_CriticalErrors0 == "") { return Visibility.Hidden; } else { return Visibility.Visible; } } }
        public object CriticalError1_Visible { get { if (_CriticalErrors1 == "") { return Visibility.Hidden; } else { return Visibility.Visible; } } }
        public object CriticalError2_Visible { get { if (_CriticalErrors2 == "") { return Visibility.Hidden; } else { return Visibility.Visible; } } }
        public object CriticalError3_Visible { get { if (_CriticalErrors3 == "") { return Visibility.Hidden; } else { return Visibility.Visible; } } }
        public object CriticalError4_Visible { get { if (_CriticalErrors4 == "") { return Visibility.Hidden; } else { return Visibility.Visible; } } }
        public object CriticalError5_Visible { get { if (_CriticalErrors5 == "") { return Visibility.Hidden; } else { return Visibility.Visible; } } }
        public object CriticalError6_Visible { get { if (_CriticalErrors6 == "") { return Visibility.Hidden; } else { return Visibility.Visible; } } }
        public object CriticalError7_Visible { get { if (_CriticalErrors7 == "") { return Visibility.Hidden; } else { return Visibility.Visible; } } }
        public object CriticalError8_Visible { get { if (_CriticalErrors8 == "") { return Visibility.Hidden; } else { return Visibility.Visible; } } }

        #endregion

        bool m_Termination = false;
        LockEncapsulator<XmlDocument> le;
        //add
        private ConcurrentQueue<string> _IncomingSAPMessageQueue;
        public ConcurrentQueue<string> IncomingSAPMessageQueue
        {
            get { return _IncomingSAPMessageQueue; }
            set
            {
                _IncomingSAPMessageQueue = value;
            }
        }

        private ObservableCollection<string> _PLCErrorMessage = new ObservableCollection<string>(new string[Enum.GetNames(typeof(Station.StationNumber)).Length]);
        public ObservableCollection<string> PLCErrorMessage
        {
            get { return _PLCErrorMessage; }
            set
            {
                _PLCErrorMessage = value;
                OnPropertyChanged("PLCErrorMessage");
            }
        }

        public TcpClient tcpClient;
        Logger log = LogManager.GetLogger("ClientInfo");
        Logger Deletelog = LogManager.GetLogger("All Delete Finishing Lable Log");
     public   Logger linePack = LogManager.GetLogger("linePackAllInfo");
     public Logger SealerResult = LogManager.GetLogger("SealerResultFromMiddleware");


        //NumBarcode

        private bool _notconnected;
        public bool notconnected
        {
            get { return _notconnected; }
            set
            {
                _notconnected = value;
                OnPropertyChanged("notconnected");
            }
        }

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




      private bool _PurgeON;
        public bool PurgeON
        {
            get { return _PurgeON; }
            set
            {
                _PurgeON= value;
                OnPropertyChanged("PurgeON");
            }
        }


       private bool _PauseON;
        public bool PauseON
        {
            get { return _PauseON; }
            set
            {
                _PauseON= value;
                OnPropertyChanged("PauseON");
            }
        }






        private bool _FirstLabelPrint;
        public bool FirstLabelPrint
        {
            get { return _FirstLabelPrint; }
            set
            {
                _FirstLabelPrint= value;
                OnPropertyChanged("FirstLabelPrint");
            }
        }

        private System.Net.IPAddress _ipaddresstohost;
        public System.Net.IPAddress ipaddresstohost
        {
            get { return _ipaddresstohost; }
            set
            {
                _ipaddresstohost = value;
                OnPropertyChanged("ipaddresstohost");
            }
        }


      private int _NumPrintLabel;
        public int NumPrintLabel
        {
            get { return _NumPrintLabel; }
            set
            {
                _NumPrintLabel = value;
                OnPropertyChanged("NumPrintLabel");
            }
        }

       private int _Token1;
        public int Token1
        {
            get { return _Token1; }
            set
            {
                _Token1 = value;
                OnPropertyChanged("Token1");
            }
        }
       private int _Token2;
        public int Token2
        {
            get { return _Token2; }
            set
            {
                _Token2 = value;
                OnPropertyChanged("Token2");
            }
        }


        private int _Token3;
        public int Token3
        {
            get { return _Token3; }
            set
            {
                _Token3 = value;
                OnPropertyChanged("Token3");
            }
        }




        private int _NumBarcode;
        public int NumBarcode
        {
            get { return _NumBarcode; }
            set
            {
                _NumBarcode = value;
                OnPropertyChanged("NumBarcode");
            }
        }

        private bool _Isconnected;
        public bool Isconnected
        {
            get { return _Isconnected; }
            set
            {
                _Isconnected = value;
                OnPropertyChanged("Isconnected");
            }
        }

        private bool _IsNonStop;
        public bool IsNonStop
        {
            get { return _IsNonStop; }
            set
            {
                _IsNonStop = value;
                OnPropertyChanged("IsNonStop");
            }
        }
        private bool _NoPrintCheck;
        public bool NoPrintCheck
        {
            get { return _NoPrintCheck; }
            set
            {
                _NoPrintCheck = value;
                OnPropertyChanged("NoPrintCheck");
            }
        }

        public TCPClass oTcpClass;
        public event PropertyChangedEventHandler PropertyChanged;
        public ManualResetEvent ServerReplyEvt;
        public ManualResetEvent MiddlewareToIGTEvt_Login;
        public ManualResetEvent MiddlewareToIGTEvt_LoginCheckQC1;
        public ManualResetEvent MiddlewareToIGTEvt_LoginCheckQC2;
        public ManualResetEvent MiddlewareToIGTEvt_Remote;
        public ManualResetEvent MiddlewareToIGTEvt_Param;
        public ManualResetEvent WriteToHostCompleteEvt;
        public ManualResetEvent MiddlewareToIGTEvt_Sealer;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private string _xmlstring;
        public string XMLString
        {
            get
            {
                return _xmlstring;
            }
            set
            {
                _xmlstring = value;
                OnPropertyChanged("XMLString");
            }
        }
        
        private string _imgfilename;
        public string Imgfilename
        {
            get
            {
                return _imgfilename;
            }
            set
            {
                _imgfilename = value;
                OnPropertyChanged("Imgfilename");
            }
        }

        private short _sealerReceipt1;
        public short SealerReceipt1
        {
            get
            {
                return _sealerReceipt1;
            }
            set
            {
                _sealerReceipt1 = value;
                OnPropertyChanged("SealerReceipt1");
            }
        }

        private short _sealerReceipt2;
        public short SealerReceipt2
        {
            get
            {
                return _sealerReceipt2;
            }
            set
            {
                _sealerReceipt2 = value;
                OnPropertyChanged("SealerReceipt2");
            }
        }

        private short _sealerReceipt3;
        public short SealerReceipt3
        {
            get
            {
                return _sealerReceipt3;
            }
            set
            {
                _sealerReceipt3 = value;
                OnPropertyChanged("SealerReceipt3");
            }
        }

        private DateTime _StartDate;

        public DateTime StartDate
        {
            get { return _StartDate; }
            set
            {
                _StartDate = value;
                OnPropertyChanged("StartDate");
            }
        }
        private string _HMI1Data;
        public string HMI1Data
        {
            get
            {
                return _HMI1Data;
            }
            set
            {
                _HMI1Data = value;
                OnPropertyChanged("HMI1Data");
            }
        }



        private string _HMI2Data;
        public string HMI2Data
        {
            get
            {
                return _HMI2Data;
            }
            set
            {
                _HMI2Data = value;
                OnPropertyChanged("HMI2Data");
            }
        }




      private string _QC1Data;
        public string QC1Data
        {
            get
            {
                return _QC1Data;
            }
            set
            {
                _QC1Data = value;
                OnPropertyChanged("QC1Data");
            }
        }


       private string _QC2Data;
        public string QC2Data
        {
            get
            {
                return _QC2Data;
            }
            set
            {
                _QC2Data = value;
                OnPropertyChanged("QC2Data");
            }
        }
        private string _imgstring;
        public string Imgstring
        {
            get
            {
                return _imgstring;
            }
            set
            {
                _imgstring = value;
                OnPropertyChanged("Imgstring");
            }
        }

        private string _imgstring2;
        public string Imgstring2
        {
            get
            {
                return _imgstring2;
            }
            set
            {
                _imgstring2 = value;
                OnPropertyChanged("Imgstring2");
            }
        }

        private string _imgpath;
        public string Imgpath
        {
            get
            {
                return _imgpath;
            }
            set
            {
                _imgpath = value;
                OnPropertyChanged("Imgpath");
            }
        }

        private Uri _imgFullpath;
        public Uri ImgFullpath
        {
            get
            {
                return _imgFullpath;
            }
            set
            {
                _imgFullpath = value;
                OnPropertyChanged("ImgFullpath");
            }
        }

        private Uri _imgFullpath2;
        public Uri ImgFullpath2
        {
            get
            {
                return _imgFullpath2;
            }
            set
            {
                _imgFullpath2 = value;
                OnPropertyChanged("ImgFullpath2");
            }
        }


       

       private int _control;
        public int control
        {
            get
            {
                return _control;
            }
            set
            {
                _control = value;
                OnPropertyChanged("control");
            }
        }

        private int _control1;
        public int control1
        {
            get
            {
                return _control1;
            }
            set
            {
                _control1 = value;
                OnPropertyChanged("control1");
            }
        }

        private int _control2;
        public int control2
        {
            get
            {
                return _control2;
            }
            set
            {
                _control2 = value;
                OnPropertyChanged("control2");
            }
        }



       private int _controlst2;
        public int controlst2
        {
            get
            {
                return _controlst2;
            }
            set
            {
                _controlst2 = value;
                OnPropertyChanged("controlst2");
            }
        }


       private int _controlst2_1;
        public int controlst2_1
        {
            get
            {
                return _controlst2_1;
            }
            set
            {
                _controlst2_1 = value;
                OnPropertyChanged("controlst2_1");
            }
        }



      private int _controlst2_2;
        public int controlst2_2
        {
            get
            {
                return _controlst2_2;
            }
            set
            {
                _controlst2_2 = value;
                OnPropertyChanged("controlst2_2");
            }
        }


      private int _controlst2_3;
        public int controlst2_3
        {
            get
            {
                return _controlst2_3;
            }
            set
            {
                _controlst2_3 = value;
                OnPropertyChanged("controlst2_3");
            }
        }



      private int _controlst2_4;
        public int controlst2_4
        {
            get
            {
                return _controlst2_4;
            }
            set
            {
                _controlst2_4 = value;
                OnPropertyChanged("controlst2_4");
            }
        }


       private int _controlst2_5;
        public int controlst2_5
        {
            get
            {
                return _controlst2_5;
            }
            set
            {
                _controlst2_5 = value;
                OnPropertyChanged("controlst2_5");
            }
        }




       private int _controlst2_6;
        public int controlst2_6
        {
            get
            {
                return _controlst2_6;
            }
            set
            {
                _controlst2_6 = value;
                OnPropertyChanged("controlst2_6");
            }
        }



       private int _controlst2_7;
        public int controlst2_7
        {
            get
            {
                return _controlst2_7;
            }
            set
            {
                _controlst2_7 = value;
                OnPropertyChanged("controlst2_7");
            }
        }





      private int _controlst3;
        public int controlst3
        {
            get
            {
                return _controlst3;
            }
            set
            {
                _controlst3 = value;
                OnPropertyChanged("controlst3");
            }
        }



       private int _controlst3_1;
        public int controlst3_1
        {
            get
            {
                return _controlst3_1;
            }
            set
            {
                _controlst3_1 = value;
                OnPropertyChanged("controlst3_1");
            }
        }


       private int _controlst3_2;
        public int controlst3_2
        {
            get
            {
                return _controlst3_2;
            }
            set
            {
                _controlst3_2 = value;
                OnPropertyChanged("controlst3_2");
            }
        }


        private int _controlst4;
        public int controlst4
        {
            get
            {
                return _controlst4;
            }
            set
            {
                _controlst4 = value;
                OnPropertyChanged("controlst4");
            }
        }



       private int _controlst4_1;
        public int controlst4_1
        {
            get
            {
                return _controlst4_1;
            }
            set
            {
                _controlst4_1 = value;
                OnPropertyChanged("controlst4_1");
            }
        }


       private int _controlst4_2;
        public int controlst4_2
        {
            get
            {
                return _controlst4_2;
            }
            set
            {
                _controlst4_2 = value;
                OnPropertyChanged("controlst4_2");
            }
        }


       private int _controlst5;
        public int controlst5
        {
            get
            {
                return _controlst5;
            }
            set
            {
                _controlst5 = value;
                OnPropertyChanged("controlst5");
            }
        }



       private int _controlst5_1;
        public int controlst5_1
        {
            get
            {
                return _controlst5_1;
            }
            set
            {
                _controlst5_1 = value;
                OnPropertyChanged("controlst5_1");
            }
        }
            
      
  

       private int _controlst5_2;
        public int controlst5_2
        {
            get
            {
                return _controlst5_2;
            }
            set
            {
                _controlst5_2 = value;
                OnPropertyChanged("controlst5_2");
            }
        }
            
       private int _controlst6;
        public int controlst6
        {
            get
            {
                return _controlst6;
            }
            set
            {
                _controlst6 = value;
                OnPropertyChanged("controlst6");
            }
        }




       private int _controlst6_1;
        public int controlst6_1
        {
            get
            {
                return _controlst6_1;
            }
            set
            {
                _controlst6_1 = value;
                OnPropertyChanged("controlst6_1");
            }
        }





      private int _controlst7;
        public int controlst7
        {
            get
            {
                return _controlst7;
            }
            set
            {
                _controlst7 = value;
                OnPropertyChanged("controlst7");
            }
        }



      private int _controlst8;
        public int controlst8
        {
            get
            {
                return _controlst8;
            }
            set
            {
                _controlst8 = value;
                OnPropertyChanged("controlst8");
            }
        }


        //private string _message;
        //public string message
        //{
        //    get
        //    {
        //        return _message;
        //    }
        //    set
        //    {
        //        _message = value;
        //        OnPropertyChanged("message");
        //    }
        //}

      
      
        private string _operator1message;
        public string Operator1message
        {
            get
            {
                return _operator1message;
            }
            set
            {
                _operator1message = value;
                OnPropertyChanged("Operator1message");
            }
        }

        private string _operator2message;
        public string Operator2message
        {
            get
            {
                return _operator2message;
            }
            set
            {
                _operator2message = value;
                OnPropertyChanged("Operator2message");
            }
        }

        private string _operator1BoxNumber;
        public string operator1BoxNumber
        {
            get
            {
                return _operator1BoxNumber;
            }
            set
            {
                _operator1BoxNumber = value;
                OnPropertyChanged("operator1BoxNumber");
            }
        }

        private string _operator2BoxNumber;
        public string operator2BoxNumber
        {
            get
            {
                return _operator2BoxNumber;
            }
            set
            {
                _operator2BoxNumber = value;
                OnPropertyChanged("operator2BoxNumber");
            }
        }

        private XmlDocument _PARAM_ScreenAccess;
        public XmlDocument PARAM_ScreenAccess
        {
            get { return _PARAM_ScreenAccess; }
            set
            {
                _PARAM_ScreenAccess = value;
                OnPropertyChanged("PARAM_ScreenAccess");
            }
        }

        private XmlDocument _REMOTE_Cmd;
        public XmlDocument REMOTE_Cmd
        {
            get { return _REMOTE_Cmd; }
            set
            {
                _REMOTE_Cmd = value;
                OnPropertyChanged("REMOTE_Cmd");
            }
        }

       private XmlDocument _QC1LoginConnectionCheck;
        public XmlDocument QC1LoginConnectionCheck
        {
            get { return _QC1LoginConnectionCheck; }
            set
            {
                _QC1LoginConnectionCheck = value;
                OnPropertyChanged("QC1LoginConnectionCheck");
            }
        }

        private string _QC1LoginConnectionCheck1;
        public string QC1LoginConnectionCheck1
        {
            get { return _QC1LoginConnectionCheck1; }
            set
            {
                _QC1LoginConnectionCheck1 = value;
                OnPropertyChanged("QC1LoginConnectionCheck1");
            }
        }
       private bool _QC1LogOutSendReady;
        public bool QC1LogOutSendReady
        {
            get { return _QC1LogOutSendReady; }
            set
            {
                _QC1LogOutSendReady = value;
                OnPropertyChanged("QC1LogOutSendReady");
            }
        }

      private bool _QC2LogOutSendReady;
        public bool QC2LogOutSendReady
        {
            get { return _QC2LogOutSendReady; }
            set
            {
                _QC2LogOutSendReady = value;
                OnPropertyChanged("QC2LogOutSendReady");
            }
        }

        private string _QC2LoginConnectionCheck;
        public string QC2LoginConnectionCheck
        {
            get { return _QC2LoginConnectionCheck; }
            set
            {
                _QC2LoginConnectionCheck = value;
                OnPropertyChanged("QC2LoginConnectionCheck");
            }
        }
       //private XmlDocument _QC2LoginConnectionCheck;
       // public XmlDocument QC2LoginConnectionCheck
       // {
       //     get { return _QC2LoginConnectionCheck; }
       //     set
       //     {
       //         _QC2LoginConnectionCheck = value;
       //         OnPropertyChanged("QC2LoginConnectionCheck");
       //     }
       // }


       private XmlDocument _HMI1LoginConnectionCheck;
        public XmlDocument HMI1LoginConnectionCheck
        {
            get { return _HMI1LoginConnectionCheck; }
            set
            {
                _HMI1LoginConnectionCheck = value;
                OnPropertyChanged("HMI1LoginConnectionCheck");
            }
        }


        private XmlDocument _VacummResult;
        public XmlDocument VacummResult
        {
            get { return _VacummResult; }
            set
            {
                _VacummResult = value;
                OnPropertyChanged("VacummResult");
            }
        }










        private XmlDocument _BOXError;
        public XmlDocument BOXError
        {
            get { return _BOXError; }
            set
            {
                _BOXError = value;
                OnPropertyChanged("BOXError");
            }
        }

      private XmlDocument _HMI2LoginConnectionCheck;
        public XmlDocument HMI2LoginConnectionCheck
        {
            get { return _HMI2LoginConnectionCheck; }
            set
            {
                _HMI2LoginConnectionCheck = value;
                OnPropertyChanged("HMI2LoginConnectionCheck");
            }
        }



        private XmlDocument _QCLogin;
        public XmlDocument QCLogin
        {
            get { return _QCLogin; }
            set
            {
                _QCLogin = value;
                OnPropertyChanged("QCLogin");
            }
        }


        private XmlDocument _tnRdoc;//use for tracking TNR print label
        public XmlDocument tnRdoc
        {
            get { return _tnRdoc; }
            set
            {
                _tnRdoc = value;
                OnPropertyChanged("tnRdoc");
            }
        }


        private XmlDocument _tnRdocDemo;//use for tracking TNR print label
        public XmlDocument tnRdocDemo
        {
            get { return _tnRdocDemo; }
            set
            {
                _tnRdocDemo = value;
                OnPropertyChanged("tnRdocDemo");
            }
        }

        private XmlDocument _MBBdoc;//use for tracking MBB print label
        public XmlDocument MBBdoc
        {
            get { return _MBBdoc; }
            set
            {
                _MBBdoc = value;
                OnPropertyChanged("MBBdoc");
            }
        }

        private XmlDocument _MBBcleardoc;//use for tracking MBB print label
        public XmlDocument MBBcleardoc
        {
            get { return _MBBcleardoc; }
            set
            {
                _MBBcleardoc = value;
                OnPropertyChanged("MBBcleardoc");
            }
        }


       private XmlDocument _tnRcleardoc;//use for tracking tnR print label
        public XmlDocument tnRcleardoc
        {
            get { return _tnRcleardoc; }
            set
            {
                _tnRcleardoc = value;
                OnPropertyChanged("tnRcleardoc");
            }
        }



       private XmlDocument _Boxcleardoc;//use for tracking Box print label
        public XmlDocument Boxcleardoc
        {
            get { return _Boxcleardoc; }
            set
            {
                _Boxcleardoc = value;
                OnPropertyChanged("Boxcleardoc");
            }
        }

        
        private XmlDocument _Boxdoc;//use for tracking Box print label
        public XmlDocument Boxdoc
        {
            get { return _Boxdoc; }
            set
            {
                _Boxdoc = value;
                OnPropertyChanged("Boxdoc");
            }
        }

       private XmlDocument _BoxdocESD;//use for tracking Box print label
        public XmlDocument BoxdocESD
        {
            get { return _BoxdocESD; }
            set
            {
                _BoxdocESD = value;
                OnPropertyChanged("BoxdocESD");
            }
        }

        private XmlDocument _FLTrackingdoc;//use for tracking all finishing label informations
        public XmlDocument FLTrackingdoc
        {
            get
            {
                return _FLTrackingdoc;
            }
            set
            {
                _FLTrackingdoc = value;
            }
        }
        
        private XmlDocument _FLTrackingdocForA;//use for tracking all finishing label informations For rotary A
        public XmlDocument FLTrackingdocForA
        {
            get { return _FLTrackingdocForA; }
            set
            {
                _FLTrackingdocForA = value;
                OnPropertyChanged("FLTrackingdocForA");
            }
        }

        private XmlDocument _FLTrackingdocForB;//use for tracking all finishing label informations For rotary B
        public XmlDocument FLTrackingdocForB
        {
            get { return _FLTrackingdocForB; }
            set
            {
                _FLTrackingdocForB = value;
                OnPropertyChanged("FLTrackingdocForB");
            }
        }
        
        private XmlDocument _FLTrackingdocForC;//use for tracking all finishing label informations For rotary C
        public XmlDocument FLTrackingdocForC
        {
            get { return _FLTrackingdocForC; }
            set
            {
                _FLTrackingdocForC = value;
                OnPropertyChanged("FLTrackingdocForC");
            }
        }
        
        private XmlDocument _Station2Printerdoc;//use for tracking all ZPL files informations @ station 2
        public XmlDocument Station2Printerdoc
        {
            get { return _Station2Printerdoc; }
            set
            {
                _Station2Printerdoc = value;
                OnPropertyChanged("Station2Printerdoc");
            }
        }

        private XmlDocument _Station4Printerdoc;//use for tracking all ZPL files informations @ station 4
        public XmlDocument Station4Printerdoc
        {
            get { return _Station4Printerdoc; }
            set
            {
                _Station4Printerdoc = value;
                OnPropertyChanged("Station4Printerdoc");
            }
        }

        private XmlDocument _Station7Printerdoc;//use for tracking all ZPL files informations @ station 7
        public XmlDocument Station7Printerdoc
        {
            get { return _Station7Printerdoc; }
            set
            {
                _Station7Printerdoc = value;
                OnPropertyChanged("Station7rinterdoc");
            }
        }

        private XmlDocument _FinishingLabelsInfo;// I should put this in mainnetworkclass... 
        public XmlDocument FinishingLabelsInfo// I should put this in mainnetworkclass... 
        {
            get { return _FinishingLabelsInfo; }
            set
            {
                _FinishingLabelsInfo = value;
                OnPropertyChanged("FinishingLabelsInfo");
            }
        }
        
        private XmlDocument _BoxInformationDocument;
        public XmlDocument BoxInformationDocument
        {
            get { return _BoxInformationDocument; }
            set
            {
                _BoxInformationDocument = value;
                OnPropertyChanged("BoxInformationDocument");
            }
        }
                
        public MainNetworkClass()
        {
            //oTcpClass = new TCPClass();
            //oTcpClass.TCPConnectChanged += oTcpClass_TCPConnectChanged;
            //oTcpClass.TCPDataArrival += oTcpClass_TCPDataArrival;
            ServerReplyEvt = new ManualResetEvent(false);
            MiddlewareToIGTEvt_Login = new ManualResetEvent(false);
            MiddlewareToIGTEvt_LoginCheckQC1=new ManualResetEvent(false);
            MiddlewareToIGTEvt_LoginCheckQC2=new ManualResetEvent(false);
            MiddlewareToIGTEvt_Param = new ManualResetEvent(false);
            MiddlewareToIGTEvt_Remote = new ManualResetEvent(false);
            WriteToHostCompleteEvt = new ManualResetEvent(false);
            MiddlewareToIGTEvt_Sealer = new ManualResetEvent(false);


            QC1LogOutSendReady=false;
            QC2LogOutSendReady=false;
          
             control=0;
             control1=0;
             control2=0;           

          controlst2 =0;
          controlst2_1 =0;
          controlst2_2=0;
           controlst2_3=0;
          controlst2_4 =0;
          controlst2_5 =0;
          controlst2_6 =0;
          controlst2_7 =0;
          controlst3   =0;
          controlst3_1 =0;
          controlst3_2 =0;
          controlst4   =0;  
          controlst4_1 =0;
          controlst4_2 =0;
          controlst5   =0;  
          controlst5_1 =0;
          controlst6   =0;
          controlst6_1 =0;
          controlst7 =0;
          controlst8=0;
             FirstLabelPrint=false;
             notconnected = true;
            FinishingLabelsInfo = new XmlDocument(); //compile the list of finishing labels and informations in the system
            FinishingLabelsInfo.LoadXml(@"<BOXLIST></BOXLIST>");
            FLTrackingdoc = new XmlDocument();
            FLTrackingdoc.LoadXml(@"<LABELS></LABELS>");
            //changhinwai
            le = new LockEncapsulator<XmlDocument>(FLTrackingdoc);
            //add

            IncomingSAPMessageQueue = new ConcurrentQueue<string>();
            stopwatch = new Stopwatch();

            return;
        }

        public void InitNetwork(System.Net.IPAddress address)
        {
            oTcpClass = new TCPClass(address);
            oTcpClass.TCPConnectChanged += oTcpClass_TCPConnectChanged;
            oTcpClass.TCPDataArrival += oTcpClass_TCPDataArrival;
            IsNonStop = true;
            NoPrintCheck = true;
            NumBarcode = 0;
      
        }

        //General XMLDocument functions

        //add need to ask

        public void FindFinishingLabelDocument(string finishinglabelstr) //place this finishing information into xml document tr
        {
            try
            {
                XmlElement root1 = this.FinishingLabelsInfo.DocumentElement;    // this house the list of XML documents from middleware
                XmlNode node = root1.SelectSingleNode("descendant::MESSAGE[BODY/BOX_NUMBER='" + finishinglabelstr + "']");
                //  XmlNode Type = node.SelectSingleNode(@"BODY/DATA/BOX_TYPE");

                //find box number in fltrackingdoc
                XmlNode fltrackingroot = FLTrackingdoc.DocumentElement;          //this house the list of summarized xml details 
                //if finishing label do not match incoming message there is no error yet.. need to log this.
                XmlNode selectednode = fltrackingroot.SelectSingleNode("descendant::LABEL[ID='" + finishinglabelstr + "']");


                XmlNode HIC = node.SelectSingleNode(@"BODY/HIC_REQUIRED");
                selectednode.SelectSingleNode("HIC_REQUIRED").InnerText = HIC.InnerText;

                XmlNode DESICCANT = node.SelectSingleNode(@"BODY/DESICCANT_REQUIRED ");
                selectednode.SelectSingleNode("DESICCANT_REQUIRED ").InnerText = DESICCANT.InnerText;


                XmlNode AQL = node.SelectSingleNode(@"BODY/AQL");
                selectednode.SelectSingleNode("AQL").InnerText = AQL.InnerText;
                if (AQL.InnerText.Contains("YES"))
                {
                    stn2log = finishinglabelstr + " AQL ";
                    linePack.Info(finishinglabelstr + " AQL");
                }
                XmlNode Sealer = node.SelectSingleNode(@"BODY/SEALER_RECIPE");
                selectednode.SelectSingleNode("SEALER").InnerText = Sealer.InnerText;
                XmlNode MBBBag = node.SelectSingleNode(@"BODY/MBB_TYPE");
                selectednode.SelectSingleNode("MBBTYPE").InnerText = MBBBag.InnerText;
                XmlNode TapeWidth = node.SelectSingleNode(@"BODY/DATA/CARRIER_TAPE_WIDTH");//NUMBER_OF_Reel
                if (TapeWidth != null)
                    selectednode.SelectSingleNode("REELHEIGHT").InnerText = TapeWidth.InnerText;
                else
                    selectednode.SelectSingleNode("REELHEIGHT").InnerText = "0";

                XmlNode TrayNumber = node.SelectSingleNode(@"BODY/DATA/NUMBER_OF_TRAY");
                if (TrayNumber != null)

                    selectednode.SelectSingleNode("TRAYNO").InnerText = TrayNumber.InnerText;
                else
                    selectednode.SelectSingleNode("TRAYNO").InnerText = "0";

                XmlNode TRAY_THICKNESS = node.SelectSingleNode(@"BODY/DATA/TRAY_THICKNESS");
                selectednode.SelectSingleNode("TRAY_THICKNESS").InnerText = TRAY_THICKNESS.InnerText;

                XmlNode TOTAL_TRAY_HEIGHT = node.SelectSingleNode(@"BODY/DATA/TOTAL_TRAY_HEIGHT");
                selectednode.SelectSingleNode("TOTAL_TRAY_HEIGHT").InnerText = TOTAL_TRAY_HEIGHT.InnerText;

                XmlNode SPTK_MARKED = node.SelectSingleNode(@"BODY/DATA/SPTK_MARKED");
                if (SPTK_MARKED != null)

                    selectednode.SelectSingleNode("SPTK_MARKED").InnerText = SPTK_MARKED.InnerText;




                //# label @ station 2
                XmlElement fldocele = this.FinishingLabelsInfo.DocumentElement;
                XmlNode flnode = fldocele.SelectSingleNode("descendant::MESSAGE[BODY/BOX_NUMBER='" + finishinglabelstr + "']");
                XmlNodeList Station2labels = flnode.SelectNodes(@"BODY/LABEL_PLACEMENT/TAPE_REEL/FILE");
                selectednode.SelectSingleNode("STATION02PRINTNO").InnerText = Station2labels.Count.ToString();
                //# label @ station 4
                fldocele = this.FinishingLabelsInfo.DocumentElement;
                flnode = fldocele.SelectSingleNode("descendant::MESSAGE[BODY/BOX_NUMBER='" + finishinglabelstr + "']");
                XmlNodeList Station4labels = flnode.SelectNodes(@"BODY/LABEL_PLACEMENT/MBB/FILE");
                selectednode.SelectSingleNode("STATION04PRINTNO").InnerText = Station4labels.Count.ToString();
                //# label @ station 7
                fldocele = this.FinishingLabelsInfo.DocumentElement;
                flnode = fldocele.SelectSingleNode("descendant::MESSAGE[BODY/BOX_NUMBER='" + finishinglabelstr + "']");
                XmlNodeList Station7labels = flnode.SelectNodes(@"BODY/LABEL_PLACEMENT/BOX/FILE");
                selectednode.SelectSingleNode("STATION07PRINTNO").InnerText = Station7labels.Count.ToString();

                //ADD FOR STATION 6



                //fldocele = this.FinishingLabelsInfo.DocumentElement;
                //flnode = fldocele.SelectSingleNode("descendant::MESSAGE[BODY/BOX_NUMBER='" + finishinglabelstr + "']");
                //XmlNodeList Station6message = flnode.SelectNodes(@"BODY/QCMONITOR_INFO");
                //selectednode.SelectSingleNode("QCMONITOR_INFO").InnerText = Station6message.Count.ToString();



                XmlNode image6 = node.SelectSingleNode(@"BODY/QCMONITOR_INFO/IMAGE_FILENAME");
                selectednode.SelectSingleNode("IMAGE_FILENAME").InnerText = image6.InnerText;

                XmlNode message6 = node.SelectSingleNode(@"BODY/QCMONITOR_INFO/MESSAGE");
                selectednode.SelectSingleNode("MESSAGE").InnerText = message6.InnerText;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }




        //  added for Operator1 Station 6

        #region Station6



        private delegate void FindFinishingLabelForOperatordelegate(string text);
        public void FindFinishingLabelForOperator(string finishinglabelOpr)
        {

            try
            {

                XmlDocument doc = new XmlDocument();
                doc.Load(@"Config.xml");
                XmlNode node = doc.SelectSingleNode(@"/CONFIG/PLCCONTROLLER2/IMGFOLDER");
                string Imgfilename = node.InnerText;

                XmlNode fltrackingroot = FLTrackingdoc.DocumentElement;

                XmlNode selectednode = fltrackingroot.SelectSingleNode("descendant::LABEL[ID='" + finishinglabelOpr + "']");

                if (selectednode != null)
                {
                    operator1BoxNumber = finishinglabelOpr;
                    XmlNode IMAGE_FILENAME = selectednode.SelectSingleNode(@"IMAGE_FILENAME");
                    Imgstring = IMAGE_FILENAME.InnerText;
                    XmlNode oper1message = selectednode.SelectSingleNode(@"MESSAGE");
                    Operator1message = oper1message.InnerText;
                    string tmpstr = "";
                    if (selectednode.SelectSingleNode("AQL") != null)
                    {
                        tmpstr = selectednode.SelectSingleNode("AQL").InnerText;
                        if (tmpstr.Contains("YES"))
                        {
                            Operator1message += " (AQL)";
                        }
                    }
                    if (selectednode.SelectSingleNode("HotLot") != null)
                    {
                        tmpstr = selectednode.SelectSingleNode("HotLot").InnerText;
                        if (tmpstr.Contains("1"))
                        {
                            Operator1message += " (HotLot)";
                        }
                    }
                    //Imgpath = Imgfilename + Imgstring+".bmp";                        
                    Imgpath = Imgfilename + Imgstring;
                    //ImgFullpath = new System.Uri("Imgpath");
                    Uri myuri = null;
                    try
                    {
                        myuri = new Uri("file:///" + Imgpath, UriKind.Absolute);
                    }
                    catch (Exception ex)
                    {
                        myuri = null;
                    }
                    if (myuri == null)
                    {
                        myuri = new Uri("file:///C:/Station6_image_temp/defaultimage.bmp");
                    }
                    ImgFullpath = myuri;
                }
                else
                {

                    operator1BoxNumber = finishinglabelOpr;
                    //  XmlNode IMAGE_FILENAME = selectednode.SelectSingleNode(@"IMAGE_FILENAME");
                    Imgstring = "defaultimage.bmp";
                    // XmlNode oper1message = selectednode.SelectSingleNode(@"MESSAGE");
                    Operator1message = "Wrong Finishing Label";
                    //Imgpath = Imgfilename + Imgstring+".bmp";                        
                    Imgpath = Imgfilename + Imgstring;
                    //ImgFullpath = new System.Uri("Imgpath");
                    Uri myuri = new Uri("file:///" + Imgpath, UriKind.Absolute);
                    ImgFullpath = myuri;


                }



            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private delegate void FindFinishingLabelForOperator2delegate(string text);
        public void FindFinishingLabelForOperator2(string finishinglabelOpr)
        {

            try
            {

                XmlDocument doc = new XmlDocument();
                doc.Load(@"Config.xml");
                XmlNode node = doc.SelectSingleNode(@"/CONFIG/PLCCONTROLLER2/IMGFOLDER");
                string Imgfilename = node.InnerText;

                XmlNode fltrackingroot = FLTrackingdoc.DocumentElement;

                XmlNode selectednode = fltrackingroot.SelectSingleNode("descendant::LABEL[ID='" + finishinglabelOpr + "']");

                if (selectednode != null)
                {
                    operator2BoxNumber = finishinglabelOpr;
                    XmlNode IMAGE_FILENAME = selectednode.SelectSingleNode(@"IMAGE_FILENAME");
                    Imgstring2 = IMAGE_FILENAME.InnerText;
                    XmlNode oper2message = selectednode.SelectSingleNode(@"MESSAGE");
                    Operator2message = oper2message.InnerText;
                    string tmpstr = "";
                    if (selectednode.SelectSingleNode("AQL") != null)
                    {
                        tmpstr = selectednode.SelectSingleNode("AQL").InnerText;
                        if (tmpstr.Contains("YES"))
                        {
                            Operator2message += " (AQL)";
                        }
                    }
                    if (selectednode.SelectSingleNode("HotLot") != null)
                    {
                        tmpstr = selectednode.SelectSingleNode("HotLot").InnerText;
                        if (tmpstr.Contains("1"))
                        {
                            Operator2message += " (HotLot)";
                        }
                    }
                    Imgpath = Imgfilename + Imgstring2;
                    Uri myuri = null;
                    try
                    {
                        myuri = new Uri("file:///" + Imgpath, UriKind.Absolute);
                    }
                    catch (Exception ex)
                    {
                        myuri = null;
                    }
                    if (myuri == null)
                    {
                        myuri = new Uri("file:///C:/Station6_image_temp/defaultimage.bmp");
                    }
                    ImgFullpath2 = myuri;

                }
                else
                {


                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        #endregion


        #region Station 5 For Searler

        private delegate bool FindSealerReceipeForSealer1delegate(string text);
        public bool FindSealerReceipeForSealer1(string finishinglabelOpr)
        {

            try
            {
                XmlNode fltrackingroot = FLTrackingdoc.DocumentElement;

                XmlNode selectednode = fltrackingroot.SelectSingleNode("descendant::LABEL[ID='" + finishinglabelOpr + "']");

                if (selectednode != null)
                {

                    XmlNode NAME = selectednode.SelectSingleNode(@"SEALER");
                    SealerReceipt1 = short.Parse(NAME.InnerText);
                   selectednode.SelectSingleNode("SealerNumber").InnerText ="1";

                    return true;

                }
                else

                    return false;


            }
            catch
            {
                return false;

            }

        }

        private delegate bool FindSealerReceipeForSealer2delegate(string text);
        public bool FindSealerReceipeForSealer2(string finishinglabelOpr)
        {

            try
            {
                XmlNode fltrackingroot = FLTrackingdoc.DocumentElement;

                XmlNode selectednode = fltrackingroot.SelectSingleNode("descendant::LABEL[ID='" + finishinglabelOpr + "']");

                if (selectednode != null)
                {

                    XmlNode NAME = selectednode.SelectSingleNode(@"SEALER");
                    SealerReceipt2 = short.Parse(NAME.InnerText);
                    selectednode.SelectSingleNode("SealerNumber").InnerText ="2";
                    return true;

                }
                else

                    return false;


            }
            catch
            {
                return false;

            }

        }


        private delegate bool FindSealerReceipeForSealer3delegate(string text);
        public bool FindSealerReceipeForSealer3(string finishinglabelOpr)
        {

            try
            {
                XmlNode fltrackingroot = FLTrackingdoc.DocumentElement;

                XmlNode selectednode = fltrackingroot.SelectSingleNode("descendant::LABEL[ID='" + finishinglabelOpr + "']");

                if (selectednode != null)
                {

                    XmlNode NAME = selectednode.SelectSingleNode(@"SEALER");
                    SealerReceipt3 = short.Parse(NAME.InnerText);
                   selectednode.SelectSingleNode("SealerNumber").InnerText ="3";
                    return true;

                }
                else

                    return false;


            }
            catch
            {
                return false;

            }

        }


        #endregion

        public void GetPrinterFilesTnR(string boxid)
        {

          try
          {
            tnRdoc = new XmlDocument();
            XmlElement root1 = this.FinishingLabelsInfo.DocumentElement;
            XmlNode node = root1.SelectSingleNode("descendant::MESSAGE[BODY/BOX_NUMBER='" + boxid + "']");
            if (node == null)
            {
                log.Error("Can't find BOX ID:" + boxid);
            }
            XmlNodeList TnRFileList = node.SelectNodes(@"BODY/LABEL_PLACEMENT/TAPE_REEL/FILE");
            string filelist = "";
            foreach (XmlNode TNRfiledetails in TnRFileList)
            {
                filelist = filelist +
                           "<FILE>" +
                                "<FILE_NAME>" + TNRfiledetails.SelectSingleNode(@"FILE_NAME").InnerText + "</FILE_NAME>" +
                                "<SURFACE>" + "NA" + "</SURFACE>" +
                                "<COORDINATE_X>" + "0" + "</COORDINATE_X>" +
                                "<COORDINATE_Y>" + "0" + "</COORDINATE_Y>" +
                            "</FILE>";
            }
            string xmlstring = @"<BOXID>" +
                                   "<ID>" + boxid + "</ID>" +
                                   "<PRINTER>" + TnRFileList[0].SelectSingleNode(@"PRINTER_NUMBER").InnerText + "</PRINTER>" +
                                   filelist +
                                "</BOXID>";
            tnRdoc.LoadXml(xmlstring);
          }
           catch (Exception ex)
            {
                if (ex != null) log.Error("GetPrinterFilesTnR Exception:" + ex.Message);
                throw ex;
            }
        }


    




        public bool UpdateStationLabel(string finishinglabel, string StationLabel)
        {

            try
            {
                if (finishinglabel != null && StationLabel != null)
                {
                    log.Info(" Station update");
                    XmlNode root = FLTrackingdoc.DocumentElement;
                    //if finishing label do not match incoming message there is no error yet.. need to log this.
                    XmlNode selectednode = root.SelectSingleNode("descendant::LABEL[ID='" + finishinglabel + "']");
                    if (selectednode == null)
                        return false;
                    selectednode.SelectSingleNode("STATION").InnerText = StationLabel;
                    return true;
                }

                log.Info("Station update return false");

                return false;
            }
            catch
            {
                log.Info("Station  update exception");
                return false;

            }

        }

           
        public void GetPrinterFilesBox(string boxid)
        {
            try
            {
                Boxdoc = new XmlDocument();
                XmlElement root1 = FinishingLabelsInfo.DocumentElement;
                XmlNode node = root1.SelectSingleNode("descendant::MESSAGE[BODY/BOX_NUMBER='" + boxid + "']");
                XmlNodeList BoxFileList = node.SelectNodes(@"BODY/LABEL_PLACEMENT/BOX/FILE");
                string filelist = "";
                foreach (XmlNode BoxFiledetails in BoxFileList)
                {
                    filelist = filelist +
                               "<FILE>" +
                                    "<FILE_NAME>" + BoxFiledetails.SelectSingleNode(@"FILE_NAME").InnerText + "</FILE_NAME>" +
                                    "<SURFACE>" + BoxFiledetails.SelectSingleNode(@"SURFACE").InnerText + "</SURFACE>" +
                                    "<COORDINATE_X>" + BoxFiledetails.SelectSingleNode(@"COORDINATE_X").InnerText + "</COORDINATE_X>" +
                                    "<COORDINATE_Y>" + BoxFiledetails.SelectSingleNode(@"COORDINATE_Y").InnerText + "</COORDINATE_Y>" +
                                "</FILE>";
                }
                string xmlstring = @"<BOXID>" +
                                       "<ID>" + boxid + "</ID>" +
                                       "<PRINTER>" + BoxFileList[0].SelectSingleNode(@"PRINTER_NUMBER").InnerText + "</PRINTER>" +
                                       filelist +
                                    "</BOXID>";
                Boxdoc.LoadXml(xmlstring);


            }

             catch (Exception ex)
            {
                if (ex != null) log.Error("GetPrinterFilesBox:" + ex.Message);
                // throw ex;
            }



        }




       public void GetESDPrinterFilesBox(string boxid)
        {
         try{

            BoxdocESD = new XmlDocument();
            XmlElement root1 = FinishingLabelsInfo.DocumentElement;
            XmlNode node = root1.SelectSingleNode("descendant::MESSAGE[BODY/BOX_NUMBER='" + boxid + "']");
            XmlNodeList BoxFileList = node.SelectNodes(@"BODY/LABEL_PLACEMENT/BOX/FILE");
            string filelist = "";
            foreach (XmlNode BoxFiledetails in BoxFileList)
            {
                filelist = filelist +
                           "<FILE>" +
                                "<FILE_NAME>" + BoxFiledetails.SelectSingleNode(@"FILE_NAME").InnerText + "</FILE_NAME>" +
                                "<SURFACE>" + BoxFiledetails.SelectSingleNode(@"SURFACE").InnerText + "</SURFACE>" +
                                "<COORDINATE_X>" + BoxFiledetails.SelectSingleNode(@"COORDINATE_X").InnerText + "</COORDINATE_X>" +
                                "<COORDINATE_Y>" + BoxFiledetails.SelectSingleNode(@"COORDINATE_Y").InnerText + "</COORDINATE_Y>" +
                            "</FILE>";
            }
            string xmlstring = @"<BOXID>" +
                                   "<ID>" + boxid + "</ID>" +
                                   "<PRINTER>" + BoxFileList[0].SelectSingleNode(@"PRINTER_NUMBER").InnerText + "</PRINTER>" +
                                   filelist +
                                "</BOXID>";
           BoxdocESD.LoadXml(xmlstring);

         }

          catch (Exception ex)
            {
                throw ex;
            }
        }













        public void GetPrinterFilesMBB(string boxid) //Station 4 print
        {
          try{

            MBBdoc = new XmlDocument();
            XmlElement root1 = FinishingLabelsInfo.DocumentElement;
            XmlNode node = root1.SelectSingleNode("descendant::MESSAGE[BODY/BOX_NUMBER='" + boxid + "']");
            if (node == null)
            {
                log.Error("GetPrinterFilesMBB ,Can't find BOX ID:" + boxid);
            }
            XmlNodeList MBBBagFileList = node.SelectNodes(@"BODY/LABEL_PLACEMENT/MBB/FILE");
            string filelist = "";
            foreach (XmlNode MBBFiledetails in MBBBagFileList)
            {
                filelist = filelist +
                           "<FILE>" +
                                "<FILE_NAME>" + MBBFiledetails.SelectSingleNode(@"FILE_NAME").InnerText + "</FILE_NAME>" +
                                "<SURFACE>" + MBBFiledetails.SelectSingleNode(@"SURFACE").InnerText + "</SURFACE>" +
                                "<COORDINATE_X>" + MBBFiledetails.SelectSingleNode(@"COORDINATE_X").InnerText + "</COORDINATE_X>" +
                                "<COORDINATE_Y>" + MBBFiledetails.SelectSingleNode(@"COORDINATE_Y").InnerText + "</COORDINATE_Y>" +
                            "</FILE>";
            }
            string xmlstring = @"<BOXID>" +
                                   "<ID>" + boxid + "</ID>" +
                                   "<PRINTER>" + MBBBagFileList[0].SelectSingleNode(@"PRINTER_NUMBER").InnerText + "</PRINTER>" +
                                   filelist +
                                "</BOXID>";
            MBBdoc.LoadXml(xmlstring);
          }

           catch (Exception ex)
            {
                if (ex != null) log.Error("GetPrinterFilesMBB:",ex.Message);
                throw ex;
            }
        }




        public void GetStation4PrinterFilesForClearData(string boxid) //Station 4 print
        {         
          try{

            MBBcleardoc = new XmlDocument();
            XmlElement root1 = FinishingLabelsInfo.DocumentElement;
            XmlNode node = root1.SelectSingleNode("descendant::MESSAGE[BODY/BOX_NUMBER='" + boxid + "']");
            XmlNodeList MBBBagFileList = node.SelectNodes(@"BODY/LABEL_PLACEMENT/MBB/FILE");
            string filelist = "";
            string xmlstring = @"<BOXID>" +
                                   "<ID>" + boxid + "</ID>" +
                                   "<PRINTER>" + MBBBagFileList[0].SelectSingleNode(@"PRINTER_NUMBER").InnerText + "</PRINTER>" +
                                   filelist +
                                "</BOXID>";
            MBBcleardoc.LoadXml(xmlstring);
          }
           catch (Exception ex)
            {
                throw ex;
            }
        }




        public void GetStation7PrinterFilesForClearData(string boxid) //Station 7 print
        {
          try{
            Boxcleardoc = new XmlDocument();
            XmlElement root1 = FinishingLabelsInfo.DocumentElement;
            XmlNode node = root1.SelectSingleNode("descendant::MESSAGE[BODY/BOX_NUMBER='" + boxid + "']");
            XmlNodeList BoxFileList = node.SelectNodes(@"BODY/LABEL_PLACEMENT/BOX/FILE");
            string filelist = "";
            string xmlstring = @"<BOXID>" +
                                   "<ID>" + boxid + "</ID>" +
                                   "<PRINTER>" + BoxFileList[0].SelectSingleNode(@"PRINTER_NUMBER").InnerText + "</PRINTER>" +
                                   filelist +
                                "</BOXID>";
            Boxcleardoc.LoadXml(xmlstring);
          }
           catch (Exception ex)
            {
                throw ex;
            }
        }















      public void GetStation2PrinterFilesForClearData(string boxid) //Station 4 print
        {
        try{
            tnRcleardoc = new XmlDocument();
            XmlElement root1 = FinishingLabelsInfo.DocumentElement;
            XmlNode node = root1.SelectSingleNode("descendant::MESSAGE[BODY/BOX_NUMBER='" + boxid + "']");
            XmlNodeList TnRFileList = node.SelectNodes(@"BODY/LABEL_PLACEMENT/TAPE_REEL/FILE");
            string filelist = "";
            string xmlstring = @"<BOXID>" +
                                   "<ID>" + boxid + "</ID>" +
                                   "<PRINTER>" + TnRFileList[0].SelectSingleNode(@"PRINTER_NUMBER").InnerText + "</PRINTER>" +
                                   filelist +
                                "</BOXID>";
            tnRcleardoc.LoadXml(xmlstring);
        }
         catch (Exception ex)
            {
                throw ex;
            }
        }







        public void GetPrinterFilesBoxes(string boxid)
        {
          try{
            Boxdoc = new XmlDocument();
            XmlElement root1 = FinishingLabelsInfo.DocumentElement;
            XmlNode node = root1.SelectSingleNode("descendant::MESSAGE[BODY/BOX_NUMBER='" + boxid + "']");
            XmlNodeList BoxFileList = node.SelectNodes(@"BODY/LABEL_PLACEMENT/BOX/FILE");
            string filelist = "";
            foreach (XmlNode BoxFiledetails in BoxFileList)
            {
                filelist = filelist +
                           "<FILE>" +
                                "<FILE_NAME>" + BoxFiledetails.SelectSingleNode(@"FILE_NAME").InnerText + "</FILE_NAME>" +
                                "<SURFACE>" + BoxFiledetails.SelectSingleNode(@"SURFACE").InnerText + "</SURFACE>" +
                                "<COORDINATE_X>" + BoxFiledetails.SelectSingleNode(@"COORDINATE_X").InnerText + "</COORDINATE_X>" +
                                "<COORDINATE_Y>" + BoxFiledetails.SelectSingleNode(@"COORDINATE_Y").InnerText + "</COORDINATE_Y>" +
                            "</FILE>";
            }
            string xmlstring = @"<BOXID>" +
                                   "<ID>" + boxid + "</ID>" +
                                   "<PRINTER>" + BoxFileList[0].SelectSingleNode(@"PRINTER_NUMBER").InnerText + "</PRINTER>" +
                                   filelist +
                                "</BOXID>";
            Boxdoc.LoadXml(xmlstring);
          }
           catch (Exception ex)
            {
                throw ex;
            }
        }
        public bool UpdateRJLabel(string finishinglabel, string RJLabel, string Ercode)
        {
            if (le != null)
            {
                try
                {
                    XmlDocument fltrackingdoc = le.InstanceGet();
                    if (le == null)
                        return false;
                    if (finishinglabel != null && RJLabel != null)
                    {
                       linePack.Info("RJ label update " +finishinglabel+","+Ercode);
                        XmlNode root = fltrackingdoc.DocumentElement;
                        //if finishing label do not match incoming message there is no error yet.. need to log this.
                        XmlNode selectednode = root.SelectSingleNode("descendant::LABEL[ID='" + finishinglabel + "']");
                        if (selectednode == null)
                            throw new Exception("no node found");
                        selectednode.SelectSingleNode("PackageStatus").InnerText = RJLabel;
                        selectednode.SelectSingleNode("ErrorCode").InnerText = Ercode;

                        return true;
                    }
                    linePack.Info("RJ label update return false");
                    throw new Exception("finishing label not found");
                }
                catch (Exception ex)
                {
                   // log.Info("RJ label update exception");
                    throw ex;
                    // return false;
                }
                finally
                {
                    le.InstanceReturn();
                }
            }
            return false;
        }





      public bool UpdateRJLabelst5S1(string finishinglabel, string RJLabel, string Ercode)
        {
            if (le != null)
            {
                try
                {
                    XmlDocument fltrackingdoc = le.InstanceGet();
                    if (le == null)
                        return false;
                    if (finishinglabel != null && RJLabel != null)
                    {
                      linePack.Info("RJ label update " +finishinglabel+","+Ercode);
                        XmlNode root = fltrackingdoc.DocumentElement;
                        //if finishing label do not match incoming message there is no error yet.. need to log this.
                        XmlNode selectednode = root.SelectSingleNode("descendant::LABEL[ID='" + finishinglabel + "']");
                        if (selectednode == null)
                            throw new Exception("no node found");
                        selectednode.SelectSingleNode("PackageStatus").InnerText = RJLabel;
                        selectednode.SelectSingleNode("ErrorCode").InnerText = Ercode;

                        return true;
                    }
                    linePack.Info("RJ label update return false");
                    throw new Exception("finishing label not found");
                }
                catch (Exception ex)
                {
                   // log.Info("RJ label update exception");
                    throw ex;
                    // return false;
                }
                finally
                {
                    le.InstanceReturn();
                }
            }
            return false;
        }




      public bool UpdateRJLabelst5S2(string finishinglabel, string RJLabel, string Ercode)
        {
            if (le != null)
            {
                try
                {
                    XmlDocument fltrackingdoc = le.InstanceGet();
                    if (le == null)
                        return false;
                    if (finishinglabel != null && RJLabel != null)
                    {
                        linePack.Info("RJ label update " +finishinglabel+","+Ercode);
                        XmlNode root = fltrackingdoc.DocumentElement;
                        //if finishing label do not match incoming message there is no error yet.. need to log this.
                        XmlNode selectednode = root.SelectSingleNode("descendant::LABEL[ID='" + finishinglabel + "']");
                        if (selectednode == null)
                            throw new Exception("no node found");
                        selectednode.SelectSingleNode("PackageStatus").InnerText = RJLabel;
                        selectednode.SelectSingleNode("ErrorCode").InnerText = Ercode;

                        return true;
                    }
                  linePack.Info("RJ label update return false");
                    throw new Exception("finishing label not found");
                }
                catch (Exception ex)
                {
                   // log.Info("RJ label update exception");
                    throw ex;
                    // return false;
                }
                finally
                {
                    le.InstanceReturn();
                }
            }
            return false;
        }

      public bool UpdateRJLabelst5S3(string finishinglabel, string RJLabel, string Ercode)
        {
            if (le != null)
            {
                try
                {
                    XmlDocument fltrackingdoc = le.InstanceGet();
                    if (le == null)
                        return false;
                    if (finishinglabel != null && RJLabel != null)
                    {
                        linePack.Info("RJ label update " +finishinglabel+","+Ercode);
                        XmlNode root = fltrackingdoc.DocumentElement;
                        //if finishing label do not match incoming message there is no error yet.. need to log this.
                        XmlNode selectednode = root.SelectSingleNode("descendant::LABEL[ID='" + finishinglabel + "']");
                        if (selectednode == null)
                            throw new Exception("no node found");
                        selectednode.SelectSingleNode("PackageStatus").InnerText = RJLabel;
                        selectednode.SelectSingleNode("ErrorCode").InnerText = Ercode;

                        return true;
                    }
                    linePack.Info("RJ label update return false");
                    throw new Exception("finishing label not found");
                }
                catch (Exception ex)
                {
                   // log.Info("RJ label update exception");
                    throw ex;
                    // return false;
                }
                finally
                {
                    le.InstanceReturn();
                }
            }
            return false;
        }






        public bool UpdateS4VisionStat (string finishinglabel, string RJLabel, string Ercode,string visionResult)
        {
            if (le != null)
            {
                try
                {
                    XmlDocument fltrackingdoc = le.InstanceGet();
                    if (le == null)
                        return false;
                    if (finishinglabel != null && RJLabel != null)
                    {
                        
                        linePack.Info(finishinglabel + "station4 Vision update >>" +visionResult);
                        XmlNode root = fltrackingdoc.DocumentElement;
                        //if finishing label do not match incoming message there is no error yet.. need to log this.
                        XmlNode selectednode = root.SelectSingleNode("descendant::LABEL[ID='" + finishinglabel + "']");
                        if (selectednode == null)
                            throw new Exception("no node found");
                       selectednode.SelectSingleNode("PackageStatus").InnerText = RJLabel;                       
                        selectednode.SelectSingleNode("ErrorCode").InnerText = Ercode;
                       selectednode.SelectSingleNode("St4vision").InnerText =visionResult;

                        return true;
                    }
                    linePack.Info("St4 vision finishing label not found");
                    throw new Exception("finishing label not found");
                }
                catch (Exception ex)
                {
                   // log.Info("RJ label update exception");
                    throw ex;
                    // return false;
                }
                finally
                {
                    le.InstanceReturn();
                }
            }
            return false;
        }



      
        public bool UpdateS4VisionPass (string finishinglabel)
        {
            if (le != null)
            {
                try
                {
                    XmlDocument fltrackingdoc = le.InstanceGet();
                    if (le == null)
                        return false;
                    if (finishinglabel != null)
                    {
                        
                       
                        XmlNode root = fltrackingdoc.DocumentElement;
                        //if finishing label do not match incoming message there is no error yet.. need to log this.
                        XmlNode selectednode = root.SelectSingleNode("descendant::LABEL[ID='" + finishinglabel + "']");
                        if (selectednode == null)
                            throw new Exception("no node found");
                        selectednode.SelectSingleNode("St4vision").InnerText ="PASS";
                       linePack.Info(finishinglabel + " station4 Vision update pass >>"); 

                        return true;
                    }
                   linePack.Info("St4 vision finishing label not found "+ finishinglabel);
                    throw new Exception("finishing label not found");
                }
                catch (Exception ex)
                {
                    //log.Info("St4 vision finishing label update exception "+ finishinglabel);
                    throw ex;
                    // return false;
                }
                finally
                {
                    le.InstanceReturn();
                }
            }
            return false;
        }












        public bool UpdateRJLabelForTrackingLabel(string Trackinglabel, string RJLabel, string Ercode)
        {
            if (le != null)
            {
                try
                {
                    XmlDocument fltrackingdoc = le.InstanceGet();
                    if (le == null)
                        return false;
                    if (Trackinglabel != null && RJLabel != null)
                    {
                        linePack.Info("Tracking label update RJ "+Trackinglabel +","+Ercode);
                        XmlNode root = fltrackingdoc.DocumentElement;
                        //if finishing label do not match incoming message there is no error yet.. need to log this.
                        XmlNode selectednode = root.SelectSingleNode("descendant::LABEL[TrackingLabel='" + Trackinglabel + "']");
                        if (selectednode == null)
                            throw new Exception("no node found");
                        selectednode.SelectSingleNode("PackageStatus").InnerText = RJLabel;
                        selectednode.SelectSingleNode("ErrorCode").InnerText = Ercode;
                    
                        return true;
                    }
                    linePack.Info("Tracking label update RJ return false");
                    throw new Exception("no label found");
                }
                catch (Exception ex)
                {
                   // log.Info("Tracking label RJ update exception");
                    throw ex;
                }
                finally
                {
                    le.InstanceReturn();
                }
            }
            return false;
        }




       public bool UpdatePassLabelForst7TrackingLabel(string Trackinglabel,string passcode)
        {
            if (le != null)
            {
                try
                {
                    XmlDocument fltrackingdoc = le.InstanceGet();
                    if (le == null)
                        return false;
                    if (Trackinglabel != null )
                    {
                        linePack.Info("Tracking label update st7 pass code "+Trackinglabel +","+passcode);

                        XmlNode root = fltrackingdoc.DocumentElement;
                        //if finishing label do not match incoming message there is no error yet.. need to log this.
                        XmlNode selectednode = root.SelectSingleNode("descendant::LABEL[TrackingLabel='" + Trackinglabel + "']");
                        if (selectednode == null)
                            throw new Exception("no node found ");                        
                        selectednode.SelectSingleNode("St7Result").InnerText = passcode;
                    
                        return true;  
                    }
                    linePack.Info("Tracking label st7 update pass code return false "+Trackinglabel +","+passcode);
                    throw new Exception("no label found");
                }
                catch (Exception ex)
                {
                   // log.Info("Tracking label st7 update pass code exception "+Trackinglabel +","+passcode);
                    throw ex;
                }
                finally
                {
                    le.InstanceReturn();
                }
            }
            return false;
        }













      public bool CheckServerConnectionWithMiddlwareForQC1(string username)
      {
         // XmlDocument checkcon=QC1LoginConnectionCheck;
          string checkcon=QC1LoginConnectionCheck1;
          //XmlNode flnode = checkcon.SelectSingleNode(@"//QC1");
          string flnode = checkcon;
          if (username == null)
          {
              username = "";
          }

          if (flnode =="" && username=="" &&  QC1LogOutSendReady==false)
          {
              return false;

          }

         

          if (flnode == username && username!=null)
        {
         return true;
        
        }

          else if (flnode == "" && username != null)
          {
              return true;

           }
        else
        {
    
       return false;
         }
         
       


      }



       public bool CheckServerConnectionWithMiddlwareForQC2(string username)
      {
            string checkcon=QC2LoginConnectionCheck;
          //XmlNode flnode = checkcon.SelectSingleNode(@"//QC1");
          string flnode = checkcon;
          if (username == null)
          {
              username = "";
          }

          if (flnode == "" && username == "" && QC2LogOutSendReady==false)
          {
              return false;

          }
          if (flnode == username && username!=null)
        {
         return true;
        
        }
             else if (flnode == "" && username != null)
          {
              return true;

           }
          
        else
        {
    
       return false;
         }        
         
       
      }




           public void ProcessIncomingLabelRejectInformation()  //write Xaml to  text file
              {
                try
                {
                    XmlDocument boxinfo = BOXError;
                    /*Save incoming CML*/
                    string xmlfilename = "";
                    string foldername1 = "";
                    xmlfilename = string.Format("text-{0:yyyy-MM-dd_hh-mm-ss-tt}.xml", DateTime.Now);
                    foldername1 = string.Format("{0:yyyy-MM-dd-hh}", DateTime.Now);
                    //find save xml foldername
                    XmlDocument doc = new XmlDocument();
                    doc.Load(@"Config.xml");
                    XmlNode filefolder = doc.SelectSingleNode(@"/CONFIG/XMLARCHIVE");
                    if (!Directory.Exists(filefolder.InnerText))
                        Directory.CreateDirectory(filefolder.InnerText);

                    foldername1 = filefolder.InnerText + foldername1;
                    if (!Directory.Exists(foldername1))
                        Directory.CreateDirectory(foldername1);
                    foldername1 = foldername1 + "\\";
                    boxinfo.Save(foldername1 + xmlfilename + ".xml");// save xmlfiles on pc

                    XmlNode flnode = boxinfo.SelectSingleNode(@"//MESSAGE/BODY/BOX_NUMBER");  //check1


                    /*end of save incoming XML*/
                    XmlNodeList elemlist = boxinfo.GetElementsByTagName("MESSAGE");
                    XmlNode copiedNode = FinishingLabelsInfo.ImportNode(elemlist[0], true);
                    //what to do with this finishinglabelsinfo??
                    FinishingLabelsInfo.DocumentElement.AppendChild(copiedNode); //place node into finishinglabels

                    //create finishing label tracking system
                    //find box number in fltrackingdoc
                    XmlNode root = FLTrackingdoc.DocumentElement;
                    //if finishing label do not match incoming message there is no error yet.. need to log this.
                    XmlNode selectednode = root.SelectSingleNode("descendant::LABEL[ID='" + flnode.InnerText + "']");

                    if (selectednode == null)
                    {
                        throw new Exception(" Xaml node  error");
                        return;
                    }
                   
                        selectednode.SelectSingleNode("TYPE").InnerText = "NA";
                        selectednode.SelectSingleNode("PackageStatus").InnerText = "RJ";
                        selectednode.SelectSingleNode("ErrorCode").InnerText = "998";
                   
                }
                catch (Exception ex)
                {
                    //throw new Exception(ex.ToString());
                }
           
        }










        //process incoming finishing label information
        private delegate void ProcessIncomingLabelInformationdelegate();
        public void ProcessIncomingLabelInformation()  //write Xaml to  text file
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                try
                {
                    XmlDocument boxinfo = BoxInformationDocument;

                      XmlNode flnode = boxinfo.SelectSingleNode(@"//MESSAGE/BODY/BOX_NUMBER");  //check1
                      string BoxID = flnode.InnerText;
                    /*Save incoming CML*/
                    string xmlfilename = "";
                    string foldername1 = "";
                    xmlfilename = string.Format("text-{0:yyyy-MM-dd_hh-mm-ss-tt}.xml", DateTime.Now);
                    foldername1 = string.Format("{0:yyyy-MM-dd-hh}", DateTime.Now);
                    //find save xml foldername
                    XmlDocument doc = new XmlDocument();
                    doc.Load(@"Config.xml");
                    XmlNode filefolder = doc.SelectSingleNode(@"/CONFIG/XMLARCHIVE");
                    if (!Directory.Exists(filefolder.InnerText))
                        Directory.CreateDirectory(filefolder.InnerText);

                    foldername1 = filefolder.InnerText + foldername1;
                    if (!Directory.Exists(foldername1))
                        Directory.CreateDirectory(foldername1);
                    foldername1 = foldername1 + "\\";
                    boxinfo.Save(foldername1 + xmlfilename + ".xml");// save xmlfiles on pc

                   


                    /*end of save incoming XML*/
                    XmlNodeList elemlist = boxinfo.GetElementsByTagName("MESSAGE");
                    XmlNode copiedNode = FinishingLabelsInfo.ImportNode(elemlist[0], true);
                    //what to do with this finishinglabelsinfo??
                    FinishingLabelsInfo.DocumentElement.AppendChild(copiedNode); //place node into finishinglabels

                    //create finishing label tracking system
                    //find box number in fltrackingdoc
                    XmlNode root = FLTrackingdoc.DocumentElement;
                    //if finishing label do not match incoming message there is no error yet.. need to log this.
                    XmlNode selectednode = root.SelectSingleNode("descendant::LABEL[ID='" + flnode.InnerText + "']");

                    if (selectednode == null)
                    {
                        throw new Exception(" Xaml node  error");
                        return;
                    }
                    XmlNode boxtype = boxinfo.SelectSingleNode(@"//MESSAGE/BODY/DATA/BOX_TYPE");

                    log.Info("Box Data for FL:" + BoxID+",Type:"+ boxtype);

                    // Change Here For Box Type ,Check box type is TR or Dry Pack
                    if (boxtype.InnerText == "TR")
                    {
                        selectednode.SelectSingleNode("TYPE").InnerText = boxtype.InnerText;

                    }
                    else
                    {
                        XmlNode boxtype1 = boxinfo.SelectSingleNode(@"//MESSAGE/BODY/DATA/PACKING_MEDIA");

                        if (boxtype1.InnerText != null)
                        {

                            selectednode.SelectSingleNode("TYPE").InnerText = boxtype1.InnerText;

                        }


                    }


                    //Set finishing label document
                    FindFinishingLabelDocument(flnode.InnerText);
                }
                catch (Exception ex)
                {
                    //throw new Exception(ex.ToString());
                }
            }
            else
            {  // or BeginInvoke()
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Send, new ProcessIncomingLabelInformationdelegate(ProcessIncomingLabelInformation));
            }
        }



        //add

        public bool UpdateRotaryABC_FinishingLabel(string finishinglabel, string rotary) //update data
        {
            //LockEncapsulator<XmlDocument> le = new LockEncapsulator<XmlDocument>(FLTrackingdoc);
            if (le != null)
            {
                try
                {
                    XmlDocument fltrackingdoc = le.InstanceGet();//worry that there is a deathlock on this....
                    if (fltrackingdoc == null) return false;
                    XmlNode root = fltrackingdoc.DocumentElement;
                    //if finishing label do not match incoming message there is no error yet.. need to log this.
                    XmlNode selectednode = root.SelectSingleNode("descendant::LABEL[ID='" + finishinglabel + "']");
                    if (selectednode == null)
                    {
                        throw new Exception("node null error");
                        return false;
                    }
                    selectednode.SelectSingleNode("ST02Rotary").InnerText = rotary;

                    if (selectednode.SelectSingleNode("ST02Rotary").InnerText == "RA")
                    {

                        FLTrackingdocForA = fltrackingdoc;
                        return true;
                    }

                    if (selectednode.SelectSingleNode("ST02Rotary").InnerText == "RB")
                    {

                        FLTrackingdocForB = fltrackingdoc;
                        return true;
                    }
                    if (selectednode.SelectSingleNode("ST02Rotary").InnerText == "RC")
                    {

                        FLTrackingdocForC = fltrackingdoc;
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    le.InstanceReturn();
                }
            }

            return true;
        }


        public bool UpdateTrackingLabel(string finishinglabel, string TrackingLabel)//update data
        {
            if (le != null)
            {
                try
                {
                    XmlDocument fltrackingdoc = le.InstanceGet();
                    if (fltrackingdoc == null) return false;
                    if (finishinglabel != null && TrackingLabel != null)
                    {
                        linePack.Info("Operator  Tracking label update "+finishinglabel +","+TrackingLabel);
                        stn6log="Operator  Tracking label update " + finishinglabel + "," + TrackingLabel;
                        XmlNode root = fltrackingdoc.DocumentElement;
                        //if finishing label do not match incoming message there is no error yet.. need to log this.
                        XmlNode selectednode = root.SelectSingleNode("descendant::LABEL[ID='" + finishinglabel + "']");
                        if (selectednode == null)
                            throw new Exception("no node found ");
                        selectednode.SelectSingleNode("TrackingLabel").InnerText = TrackingLabel;
                        return true;
                    }
                   linePack.Info("Operator  Tracking label update return false "+finishinglabel +","+TrackingLabel);
                    throw new Exception("no Finishing label found ");
                }
                catch (Exception ex)
                {
                   // log.Info("Operator 1 Tracking label update exception "+finishinglabel +","+TrackingLabel);
                    throw ex;

                }
                finally
                {
                    le.InstanceReturn();

                }
            }
            return false;
        }








        public bool UpdateLabelForSealerResult(string finishinglabel, string SealerResultData,String Reason)//update data
        {
            if (le != null)
            {
                try
                {
                    XmlDocument fltrackingdoc = le.InstanceGet();
                    if (SealerResultData == null) return false;
                    if (finishinglabel != null && SealerResultData != null && Reason != null)
                    {
                        SealerResult.Info("Sealer label Result update " + finishinglabel + "," + SealerResultData + "," + Reason);
                        XmlNode root = fltrackingdoc.DocumentElement;
                        //if finishing label do not match incoming message there is no error yet.. need to log this.
                        XmlNode selectednode = root.SelectSingleNode("descendant::LABEL[ID='" + finishinglabel + "']");
                        if (selectednode == null)
                            throw new Exception("no node found ");
                        selectednode.SelectSingleNode("SealerResult").InnerText = SealerResultData;
                        selectednode.SelectSingleNode("SealerResultReason").InnerText = Reason;
                        return true;
                    }
                    SealerResult.Info("Sealer label Result update return false " + finishinglabel + "," + SealerResultData + "," + Reason);
                    throw new Exception("no Finishing label found ");
                }
                catch (Exception ex)
                {
                    // log.Info("Operator 1 Tracking label update exception "+finishinglabel +","+TrackingLabel);
                    throw ex;

                }
                finally
                {
                    le.InstanceReturn();

                }
            }
            return false;
        }



















      public bool CheckTrackingLabel(string TrackingLabel)//update data
        {
            if (le != null)
            {
                try
                {
                    XmlDocument fltrackingdoc = le.InstanceGet();
                    if (fltrackingdoc == null) return false;
                    if (TrackingLabel != null)
                    {
                       linePack.Info("Operator  Tracking label check "+TrackingLabel);
                        XmlNode root = fltrackingdoc.DocumentElement;
                        //if finishing label do not match incoming message there is no error yet.. need to log this.
                        XmlNode selectednode = root.SelectSingleNode("descendant::LABEL[TrackingLabel='" + TrackingLabel + "']");
                        if (selectednode == null)
                        {
                        return true;
                        }
                       
                        return false;
                    }
                    linePack.Info("Operator  Tracking label check return false "+TrackingLabel);
                    throw new Exception("PLC give no  Tracking  label found ");
                   
                }
                catch (Exception ex)
                {
                   // log.Info("Operator 1 Tracking label check exception "+TrackingLabel);
                    throw ex;

                }
                finally
                {
                    le.InstanceReturn();

                }
            }
            return false;
        }

        //modify

        private delegate bool updatefltrackinginfomationdelegate(string text, string id, string bHotlot);
        public bool updatefltrackinginfomation(string boxid, string OEEid, string HotL) //update data
        {
            //bool returnbool = false;
            if (Application.Current.Dispatcher.CheckAccess())
            {
                if (le != null)
                {
                    try
                    {
                        // do work on UI thread
                        XmlDocument fltrackingdoc = le.InstanceGet();//worry that there is a deathlock on this                     
                        if (fltrackingdoc == null) return false;
                        //create finishing label tracking system
                        string xmlflstring = @"<LABEL><ID>" + boxid + "</ID>";
                        xmlflstring = xmlflstring + "<TYPE>" + "NA" + "</TYPE>";
                        xmlflstring = xmlflstring + "<OEEid>"+ OEEid +"</OEEid>";
                        xmlflstring = xmlflstring + "<ST02Rotary>NA</ST02Rotary>";//track which rotary it have passed by
                        xmlflstring = xmlflstring + "<REELHEIGHT>NA</REELHEIGHT>";
                        xmlflstring = xmlflstring + "<TRAYNO>NA</TRAYNO>";
                        xmlflstring = xmlflstring + "<TRAY_THICKNESS>NA</TRAY_THICKNESS>";
                        xmlflstring = xmlflstring + "<TOTAL_TRAY_HEIGHT>NA</TOTAL_TRAY_HEIGHT>";
                        xmlflstring = xmlflstring + "<HIC_REQUIRED>NA</HIC_REQUIRED>";
                        xmlflstring = xmlflstring + "<DESICCANT_REQUIRED >NA</DESICCANT_REQUIRED >";
                        xmlflstring = xmlflstring + "<AQL>NA</AQL>";
                        xmlflstring = xmlflstring + "<MBBTYPE>NA</MBBTYPE>";
                        xmlflstring = xmlflstring + "<SEALER>NA</SEALER>";
                        xmlflstring = xmlflstring + "<STATION02PRINTNO>NA</STATION02PRINTNO>";
                        xmlflstring = xmlflstring + "<STATION04PRINTNO>NA</STATION04PRINTNO>";
                        xmlflstring = xmlflstring + "<STATION07PRINTNO>NA</STATION07PRINTNO>";
                        xmlflstring = xmlflstring + "<IMAGE_FILENAME>NA</IMAGE_FILENAME>";
                        xmlflstring = xmlflstring + "<MESSAGE>NA</MESSAGE>";
                        xmlflstring = xmlflstring + "<CUST_SPECIFIC_FLOW>NA</CUST_SPECIFIC_FLOW>";
                        xmlflstring = xmlflstring + "<SPTK_MARKED>NA</SPTK_MARKED>";
                        xmlflstring = xmlflstring + "<TrackingLabel>NA</TrackingLabel>";
                        xmlflstring = xmlflstring + "<PackageStatus>NA</PackageStatus>";
                        xmlflstring = xmlflstring + "<ErrorCode>NA</ErrorCode>";
                        xmlflstring = xmlflstring + "<St4vision>NA</St4vision>"; //Added By GYLEE
                        xmlflstring = xmlflstring + "<St7Result>NA</St7Result>"; //
                        xmlflstring = xmlflstring + "<SealerNumber>NA</SealerNumber>";
                        xmlflstring = xmlflstring + "<SealerResult>NA</SealerResult>";
                        xmlflstring = xmlflstring + "<SealerResultReason>NA</SealerResultReason>";
                        xmlflstring = xmlflstring + "<HotLot>" + HotL+ "</HotLot>";
                        xmlflstring = xmlflstring + @"</LABEL>";
                        //error code station 2 starts 2xx station 3 starts 3xx etc
                        //package status = "OK NA ER RJ"
                        XmlDocument fldoc = new XmlDocument();
                        fldoc.LoadXml(xmlflstring);
                        XmlNode copiedNode = fltrackingdoc.ImportNode(fldoc.GetElementsByTagName("LABEL")[0], true);
                        fltrackingdoc.DocumentElement.AppendChild(copiedNode); //place node into finishinglabels
                        // returnbool = true;
                        return true;
                    }
                    catch (Exception ex)
                    {
                        return false;
                    }
                    finally
                    {
                        le.InstanceReturn();
                    }
                }
                // else
                //   returnbool = false;
            }
            else
            {
                // or BeginInvoke()
                object obj = Application.Current.Dispatcher.Invoke(DispatcherPriority.Send, new updatefltrackinginfomationdelegate(updatefltrackinginfomation), boxid,OEEid, HotL);
                return (bool)obj;
            }
            return false;
        }
        //end of processing incoming finishing label information


        private delegate bool updatefltrackinginfomationdelegateb(string text, string id, Boolean bHotlot);
        public bool updatefltrackinginfomationb(string boxid, string OEEid, Boolean bHot) //update data
        {
            //bool returnbool = false;
            if (Application.Current.Dispatcher.CheckAccess())
            {
                if (le != null)
                {
                    try
                    {
                        // do work on UI thread
                        XmlDocument fltrackingdoc = le.InstanceGet();//worry that there is a deathlock on this                     
                        if (fltrackingdoc == null) return false;
                        //create finishing label tracking system
                        string xmlflstring = @"<LABEL><ID>" + boxid + "</ID>";
                        xmlflstring = xmlflstring + "<TYPE>" + "NA" + "</TYPE>";
                        xmlflstring = xmlflstring + "<OEEid>" + OEEid + "</OEEid>";
                        xmlflstring = xmlflstring + "<ST02Rotary>NA</ST02Rotary>";//track which rotary it have passed by
                        xmlflstring = xmlflstring + "<REELHEIGHT>NA</REELHEIGHT>";
                        xmlflstring = xmlflstring + "<TRAYNO>NA</TRAYNO>";
                        xmlflstring = xmlflstring + "<TRAY_THICKNESS>NA</TRAY_THICKNESS>";
                        xmlflstring = xmlflstring + "<TOTAL_TRAY_HEIGHT>NA</TOTAL_TRAY_HEIGHT>";
                        xmlflstring = xmlflstring + "<HIC_REQUIRED>NA</HIC_REQUIRED>";
                        xmlflstring = xmlflstring + "<DESICCANT_REQUIRED >NA</DESICCANT_REQUIRED >";
                        xmlflstring = xmlflstring + "<AQL>NA</AQL>";
                        xmlflstring = xmlflstring + "<MBBTYPE>NA</MBBTYPE>";
                        xmlflstring = xmlflstring + "<SEALER>NA</SEALER>";
                        xmlflstring = xmlflstring + "<STATION02PRINTNO>NA</STATION02PRINTNO>";
                        xmlflstring = xmlflstring + "<STATION04PRINTNO>NA</STATION04PRINTNO>";
                        xmlflstring = xmlflstring + "<STATION07PRINTNO>NA</STATION07PRINTNO>";
                        xmlflstring = xmlflstring + "<IMAGE_FILENAME>NA</IMAGE_FILENAME>";
                        xmlflstring = xmlflstring + "<MESSAGE>NA</MESSAGE>";
                        xmlflstring = xmlflstring + "<CUST_SPECIFIC_FLOW>NA</CUST_SPECIFIC_FLOW>";
                        xmlflstring = xmlflstring + "<SPTK_MARKED>NA</SPTK_MARKED>";
                        xmlflstring = xmlflstring + "<TrackingLabel>NA</TrackingLabel>";
                        xmlflstring = xmlflstring + "<PackageStatus>NA</PackageStatus>";
                        xmlflstring = xmlflstring + "<ErrorCode>NA</ErrorCode>";
                        xmlflstring = xmlflstring + "<St4vision>NA</St4vision>"; //Added By GYLEE
                        xmlflstring = xmlflstring + "<St7Result>NA</St7Result>"; //
                        xmlflstring = xmlflstring + "<SealerNumber>NA</SealerNumber>";
                        xmlflstring = xmlflstring + "<SealerResult>NA</SealerResult>";
                        xmlflstring = xmlflstring + "<SealerResultReason>NA</SealerResultReason>";
                        if ( bHot)  xmlflstring = xmlflstring + "<HotLot>1</HotLot>";
                                else xmlflstring = xmlflstring + "<HotLot>0</HotLot>";
                        xmlflstring = xmlflstring + @"</LABEL>";
                        //error code station 2 starts 2xx station 3 starts 3xx etc
                        //package status = "OK NA ER RJ"
                        XmlDocument fldoc = new XmlDocument();
                        fldoc.LoadXml(xmlflstring);
                        XmlNode copiedNode = fltrackingdoc.ImportNode(fldoc.GetElementsByTagName("LABEL")[0], true);
                        fltrackingdoc.DocumentElement.AppendChild(copiedNode); //place node into finishinglabels
                        // returnbool = true;
                        return true;
                    }
                    catch (Exception ex)
                    {
                        if (ex!=null) log.Error(" updatefltrackinginfomationb:"+ex.Message);
                        return false;
                    }
                    finally
                    {
                        le.InstanceReturn();
                    }
                }
                // else
                //   returnbool = false;
            }
            else
            {
                // or BeginInvoke()
                object obj = Application.Current.Dispatcher.Invoke(DispatcherPriority.Send, new updatefltrackinginfomationdelegateb(updatefltrackinginfomationb), boxid, OEEid, bHot);
                return (bool)obj;
            }
            return false;
        }
        //end of processing incoming finishing label information

        //Get Printer File Status
        public void GetPrintFileStatus(int printernumber, string LabelID)
        {
            switch (printernumber)
            {
                case 1://printer 1 (assuming T&R is printer 1)
                    GetPrinterFilesTnR(LabelID);
                    break;
                case 2://printer 2 (assuming MBB is printer 2)
                    GetPrinterFilesMBB(LabelID);
                    break;

                case 3://printer 3 (Assuming Station 7 is printer 3)
                    GetPrinterFilesBox(LabelID);

                    break;

                //case 4://printer 4 (assuming T&RDemo is printer )
                //    GetPrinterFilesTnRDemo(LabelID);
                //    break;

            }
        }
        //end of Get Printer File Status

        //End of General XMLDocument functions
        void oTcpClass_TCPDataArrival(string sTCPData)//depreciated
        {
            // throw new NotImplementedException();
            System.Xml.XmlDocument oXD = new System.Xml.XmlDocument();
            oXD.LoadXml(sTCPData);
            XmlNodeList elemList = oXD.GetElementsByTagName("MESSAGE_TYPE");
            XMLString = FormatXml(sTCPData);
            switch (elemList.Item(0).InnerXml)// list of message type recieved from middle ware
            {
                case "BOX_DATA":
                    //load up BoxInformationDocument
                    BoxInformationDocument = oXD;
                    ProcessIncomingLabelInformation();
                    ServerReplyEvt.Set();
                    break;

                case "QC_STATION_LOGIN":

                    //if (MiddlewareToIGTEvt_Login.WaitOne(50) == true) //if command not processed it will remain true
                    //          break;
                    //      //process incoming commands from middleware
                    QCLogin = oXD;
                    MiddlewareToIGTEvt_Login.Set();
                    break;

                case "QC_STATION_LOGOUT":
                    break;
                case "REMOTE_COMMAND":
                    break;
                case "PARAMETER_SCREEN_ACCESS":
                    break;

            }

        }
        void oTcpClass_TCPConnectChanged(bool bConnected)
        {
            //throw new NotImplementedException();Application.Current.Dispatcher.Invoke
        }

        //general functions
        public string GetXmlHeader(string EquipmentID)
        {
            string header = @"<HEADER><EQUIPMENT_ID>" + EquipmentID.Trim()
                           + @"</EQUIPMENT_ID><TIMESTAMP>"
                          + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
                           + "</TIMESTAMP></HEADER>";
            return header;
        }


        public string FormatXml(string xml)
        {
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            var stringBuilder = new StringBuilder();
            var xmlWriterSettings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true };
            doc.Save(XmlWriter.Create(stringBuilder, xmlWriterSettings));
            return stringBuilder.ToString();
        }
        //end of general functions
        //User below for IGT as client


        public bool CheckHost()
        {
            try
            {
                SendDataToHost("<TESTCONNECTION><QC1>"+QC1Data+"</QC1><QC2>"+QC2Data+"</QC2><HMI1>"+HMI1Data+"</HMI1><HMI2>"+HMI2Data+"</HMI2></TESTCONNECTION>");
            }
            catch (Exception ex)
            {
                throw ex;
            }

            connected = tcpClient.Connected;
            notconnected = !connected;
            return tcpClient.Connected;
        }



        public bool ConnectToHost(System.Net.IPAddress address)
        {
            #region TCPClientConnection
            tcpClient = new TcpClient();
            log.Info("Connecting.....");
            XmlDocument doc = new XmlDocument();
            doc.Load(@"Config.xml");
            XmlNode node = doc.SelectSingleNode(@"/CONFIG/MIDDLEWARE/ADD");
            address = System.Net.IPAddress.Parse(node.InnerText);
            node = doc.SelectSingleNode(@"/CONFIG/MIDDLEWARE/PORT");

            IAsyncResult ar = tcpClient.BeginConnect(address, int.Parse(node.InnerText), null, null);
            System.Threading.WaitHandle wh = ar.AsyncWaitHandle;
            try
            {
                if (!ar.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(5), false))
                {
                    tcpClient.Close();
                    connected = false;
                    notconnected = true;
                    log.Error("TCP Connection to " + address + "port 5001 timeout");
                    throw new TimeoutException();
                }

                tcpClient.EndConnect(ar);
                connected = true;
                notconnected = !connected;
                log.Error("TCP Connection to " + address + "port 5001 completed");
                ipaddresstohost = address;
            }
            catch (Exception ex)
            {
                return false;
            }
            finally
            {
                wh.Close();
            }
            return true;
            #endregion
        }//connect to middleware server
        private void WriteAsyncCallback(IAsyncResult ar)
        {
            MyAsyncInfo info = ar.AsyncState as MyAsyncInfo;
            string msg = "Done writing!!";

            try
            {
                if (info.MyStream != null)
                {
                    info.MyStream.EndWrite(ar);
                    WriteToHostCompleteEvt.Set();
                }
                else throw new Exception("timeout");

            }
            catch (IOException)
            {
                msg = "Unable to complete write.";
                tcpClient.Close();
                connected = false;
                notconnected = !connected;
                log.Error("TCP port close");
            }
            catch (Exception ex)
            {
                msg = "Unable to complete write.";
                tcpClient.Close();
                connected = false;
                notconnected = !connected;
                log.Error("TCP port close");

            }
            finally
            {
                // info.MyStream.Flush();
                //  info.MyStream.Close();//cannot close stream.. this will effectively close the tcpclient port.

            }

        }

        public bool SendDataToHost(string cmd)//depreciated
        {
            try
            {
                int msglength = cmd.Length;
                byte[] size = BitConverter.GetBytes(msglength);//modified to make sure the number of byte sent can be recieved

                byte[] outputBuffer = Encoding.ASCII.GetBytes(cmd);
                NetworkStream strm = tcpClient.GetStream();
                strm.WriteTimeout = 1000;
                //strm.Write(size, 0, 4);
                strm.BeginWrite(size, 0, 4,
                                WriteAsyncCallback,
                                new MyAsyncInfo(size, strm));
                if (!WriteToHostCompleteEvt.WaitOne(5000)) throw new TimeoutException();
                WriteToHostCompleteEvt.Reset();
                strm.BeginWrite(outputBuffer, 0, msglength,
                                               WriteAsyncCallback,
                                               new MyAsyncInfo(outputBuffer, strm));
                if (!WriteToHostCompleteEvt.WaitOne(5000)) throw new TimeoutException();

                //strm.Write(size, 0, 4);
                //strm.Write(outputBuffer, 0, msglength);
                //strm.Flush();
                //WriteToHostCompleteEvt.Reset();
            }
            catch (Exception ex)
            {
                log.Error("Send Data To Host Error : " + ex.ToString());
                //close coms.. at write async function
                tcpClient.Close();
                connected = false;
                notconnected = !connected;
                return false;
            }
            finally
            {
                WriteToHostCompleteEvt.Reset();
            }
            return true;
        }//send data to middleware server

        public Stopwatch stopwatch;

        public string GetDataFromHost()
        {
            try
            {
                #region RecieveDataTCP
                // Start message receive
                byte[] data = new byte[4];
                StringBuilder inputBuffer = new StringBuilder();

                NetworkStream networkStream = tcpClient.GetStream();
                // Set a 10 millisecond timeout for reading.
                if (!networkStream.DataAvailable) return "No Data";
                networkStream.ReadTimeout = 10;

                networkStream.Read(data, 0, 4);//read only first 4 byte of data

                NetworkStream strm = tcpClient.GetStream();
                //int recv = strm.Read(data, 0, 4);
                int datasize = BitConverter.ToInt32(data, 0);
                int recv = 4;
                int offset = 0;
                while (datasize > 0)
                {
                    data = new byte[datasize];
                    recv = strm.Read(data, 0, datasize);                    
                    inputBuffer.Append(Encoding.ASCII.GetString(data, 0, recv));
                    offset += recv;
                    datasize -= recv;
                }
                int length = 1;
                if (length > 0)// Some message has been received?
                {
                    //inputBuffer.Append(Encoding.ASCII.GetString(tempBuffer, 0, length));
                    XMLString = inputBuffer.ToString();

                    // throw new NotImplementedException();
                    System.Xml.XmlDocument oXD = new System.Xml.XmlDocument();
                    oXD.LoadXml(inputBuffer.ToString());//check if xml document is loaded successfully


                    //if xml documents is added successfully... place the string into concurrentqueue...
                    IncomingSAPMessageQueue.Enqueue(inputBuffer.ToString());
                    //thats all.. the rest of the messaage below will be handled by another thread...

                    //below handling should be handled by a message thread.... can i create this thread here? in this class??


                    XmlNodeList elemList;

                    try
                    {
                        elemList = oXD.GetElementsByTagName("TESTCONNECTION");
                        if (elemList.Count > 0)
                        {
                            stopwatch.Restart();
                        }
                        else
                        {
                            TimeSpan span = new TimeSpan(0, 0, 15); //7 old 
                            if (stopwatch.Elapsed > span)
                            {
                                log.Error("host no heartbeat connection close");
                                //close coms.. at write async function
                                tcpClient.Close();
                                connected = false;
                                notconnected = !connected;
                                stopwatch.Restart();
                            }
                        }
                    }
                    catch (Exception ex) { }

                    try
                    {

                        elemList = oXD.GetElementsByTagName("TESTCONNECTION");
                        foreach (XmlNode element in elemList.Item(0))
                        {
                            switch (element.Name)
                            {
                                case "QC1":
                                   // QC1LoginConnectionCheck = oXD;
                                    QC1LoginConnectionCheck1 = element.InnerText;
                                    MiddlewareToIGTEvt_LoginCheckQC1.Set();

                                    break;
                                case "QC2":
                                    QC2LoginConnectionCheck = element.InnerText;
                                MiddlewareToIGTEvt_LoginCheckQC2.Set();

                                   break;
                                case "HMI1":
                                    HMI1LoginConnectionCheck = oXD;

                                   break;

                                case "HMI2":
                                   HMI2LoginConnectionCheck = oXD;

                                   break;
                            }
                        }

                    }
                    catch (Exception ex) { }
                          



                    elemList = oXD.GetElementsByTagName("MESSAGE_TYPE");
                    XMLString = FormatXml(inputBuffer.ToString());
                    switch (elemList.Item(0).InnerXml)// list of message type recieved from middle ware
                    {
                        case "BOX_DATA":
                            //load up BoxInformationDocument
                            BoxInformationDocument = oXD;
                            ProcessIncomingLabelInformation();
                            ServerReplyEvt.Set();
                            break;

                        case "QC_STATION_LOGIN"://only send from middleware to igt server
                            //if set break;
                            //if (MiddlewareToIGTEvt_Login.WaitOne(50) == true) //if command not processed it will remain true
                            //    break;
                            //process incoming commands from middleware
                            QCLogin = oXD;
                            MiddlewareToIGTEvt_Login.Set();
                            break;
                        case "QC_STATION_LOGOUT":

                            break;
                        case "REMOTE_COMMAND":
                            //if set break;
                           // if (MiddlewareToIGTEvt_Remote.WaitOne(50) == true)//if command not processed it will remain true
                             //   break;
                            //process data incoming from middleware mainly for plurge
                            REMOTE_Cmd = oXD;
                            MiddlewareToIGTEvt_Remote.Set();
                            break;
                        case "PARAMETER_SCREEN_ACCESS":
                            //if set break;
                           // if (MiddlewareToIGTEvt_Remote.WaitOne(50) == true)//if command not processed it will remain true
                              //  break;
                            //process data from middleware, mainly for user login and logiff for setup screen on plc
                            //process incoming commands from middleware
                            PARAM_ScreenAccess = oXD;
                            MiddlewareToIGTEvt_Param.Set();
                            break;
                         case "BOX_QUERY_ERROR":
                            BOXError=oXD;
                           ProcessIncomingLabelRejectInformation();
                          // MiddlewareToIGTEvt_QueryError.Set();
                            break;



                         case "VACUUM_SEAL_CHECK_RESULT":
                               VacummResult = oXD;
                               MiddlewareToIGTEvt_Sealer.Set();
                           
                            break;



                    }
                   // StationStarStopLog.Info("Middleware send ");
                    return XMLString;
                }
                else
                {
                    try
                    {
                        tcpClient.Close();
                        connected = false;
                        notconnected = !connected;
                    }
                    catch (Exception ex)
                    {
                        log.Error("Client close : " + ex.ToString());
                    }
                    finally
                    {
                        tcpClient = null;
                    }
                    log.Error("No Data from incoming, tcpclosed");
                    throw new Exception("No Data from incoming, tcpclosed");
                }
                #endregion
            }
            catch (Exception Ex)
            {
                //tcpClient.Close();
                //connected = false;
                //notconnected = !connected;
                //log.Error(Ex.ToString() + "-->" + XMLString);
            }
            return "ERR";
        }//recieve data from middlewareserver

        //add
        public void ProcessIncomingSAPMesssages()
        {
            // do not need it.. only 1 thread is handling incoming message.. and out going message
        }



        public bool Client_SendScanBox(string BoxNumber)
        {
            try
            {
                log.Info("Send FL Request:" + BoxNumber);
                XmlNode root = FLTrackingdoc.DocumentElement;
                XmlNode selectednode = root.SelectSingleNode("descendant::LABEL[ID='" + BoxNumber + "']");
                if (selectednode == null)
                {


                    string sxml = @"<BODY><MESSAGE_TYPE>SCANNED_BOX_NUMBER</MESSAGE_TYPE><BOX_NUMBER>"
                                + BoxNumber
                                + "</BOX_NUMBER>"
                                + "</BODY>";
                    sxml = @"<MESSAGE>"
                            + GetXmlHeader("LINEPACK_AUTOMATION1")
                            + sxml
                         + @"</MESSAGE>";
                    return SendDataToHost(sxml);
                    
                }
            }

            catch { return false; 
            
            
            }

            return false; 



        }

        public bool Check_SendScanBoxFor5(string BoxNumber)
        {

            XmlNode root = FLTrackingdoc.DocumentElement;
            XmlNode selectednode = root.SelectSingleNode("descendant::LABEL[ID='" + BoxNumber + "']");
            if (selectednode == null)
            {
                return false;


            }

            return true;


        }


        public bool Check_SendScanBoxFor8(string BoxNumber)
        {

            XmlNode root = FLTrackingdoc.DocumentElement;
            XmlNode selectednode = root.SelectSingleNode("descendant::LABEL[TrackingLabel='" + BoxNumber + "']");
            if (selectednode == null)
            {
                return false;


            }

            string clearbox = selectednode.SelectSingleNode("ID").InnerText;

            //sendFG01_FG02_MOVE(clearbox);
            //Client_sendFG01_FG02_MOVE(clearbox);

            return true;


        }


        public bool Client_SendScanBox(string BoxNumber, string BoxNumber1)
        {
            string sxml = @"<BODY><MESSAGE_TYPE>SCANNED_BOX_NUMBER</MESSAGE_TYPE><BOX_NUMBER>"
                        + BoxNumber
                        + "</BOX_NUMBER><BOX_NUMBER1>"
                        + BoxNumber1
                        + "</BOX_NUMBER1></BODY>";
            sxml = @"<MESSAGE>"
                    + GetXmlHeader("LINEPACK_AUTOMATION1")
                    + sxml
                 + @"</MESSAGE>";
            return SendDataToHost(sxml);
        }



        public bool Client_SendQCStationLogin(string Station, string Operator)
        {
          try
          {
            string sxml =
                "<BODY>" +
                    "<MESSAGE_TYPE>QC_STATION_LOGIN_ACK</MESSAGE_TYPE>" +
                    "<STATION_ID>" + Station + "</STATION_ID>" +
                    "<USER_NAME>" + Operator + "</USER_NAME>" +
                "</BODY>";
            sxml = @"<MESSAGE>"
                    + GetXmlHeader("LINEPACK_AUTOMATION1")
                    + sxml
                 + @"</MESSAGE>";
         
            return SendDataToHost(sxml);
          }
           catch (Exception ex)
            {
                throw ex;
            }
        }





        public bool Client_SendQCStationLogout(string Station, string Operator)
        {

          try{
            string sxml =
                "<BODY>" +
                    "<MESSAGE_TYPE>QC_STATION_LOGOUT</MESSAGE_TYPE>" +
                    "<STATION_ID>" + Station + "</STATION_ID>" +
                    "<USER_NAME>" + Operator + "</USER_NAME>" +
                "</BODY>";
            sxml = @"<MESSAGE>"
                    + GetXmlHeader("LINEPACK_AUTOMATION1")
                    + sxml
                 + @"</MESSAGE>";
          

            return SendDataToHost(sxml);
          }

           catch (Exception ex)
            {
                throw ex;
            }
        }



        public bool Client_SendParameterchange(string User,
                                               string ParamterName,
                                               string StationName,
                                               string OldValue,
                                               string NewValue)
        {
            string sxml =
                        "<BODY>" +
                        "<MESSAGE_TYPE>PARAMETER_CHANGE</MESSAGE_TYPE>" +
                        "<STATION_NAME>" + StationName + "</STATION_NAME>" +
                        "<PARAMETER_NAME>" + ParamterName + "</PARAMETER_NAME>" +
                        "<USER_NAME>" + User + "</USER_NAME>" +
                        "<OLD_VALUE>" + OldValue + "</OLD_VALUE>" +
                        "<NEW_VALUE>" + NewValue + "</NEW_VALUE>" +
                        "</BODY>";
            sxml = @"<MESSAGE>"
                    + GetXmlHeader("LINEPACK_AUTOMATION1")
                    + sxml
                 + @"</MESSAGE>";
            return SendDataToHost(sxml);
        }




       public bool Client_SendParameterchange1(string ParamterName,                                              
                                               string OldValue,
                                               string NewValue)
        {
         try{
            string sxml =
                        "<BODY>" +
                        "<MESSAGE_TYPE>PARAMETER_CHANGE</MESSAGE_TYPE>" +
                       
                        "<PARAMETER_NAME>" + ParamterName + "</PARAMETER_NAME>" +                       
                        "<OLD_VALUE>" + OldValue + "</OLD_VALUE>" +
                        "<NEW_VALUE>" + NewValue + "</NEW_VALUE>" +
                        "</BODY>";
            sxml = @"<MESSAGE>"
                    + GetXmlHeader("LINEPACK_AUTOMATION1")
                    + sxml
                 + @"</MESSAGE>";
            return SendDataToHost(sxml);
          
         }
         catch (Exception ex)
                {
                    // throw ex;re
                    log.Error("Send ParameterChange" + ex + ParamterName+OldValue+NewValue);
                }
            return true;
        }














         public bool Client_SendParameterchangeLogout(string Paramterchange,
                                               string AccessLevel,
                                               string TerminalID,
                                               string Token)
        {

           try{
            string sxml =
                        "<BODY>" +
                        "<MESSAGE_TYPE>PARAMETER_SCREEN_ACCESS</MESSAGE_TYPE>" +
                        "<PARAMETER_CHANGE>" + Paramterchange + "</PARAMETER_CHANGE>" +
                        "<ACCESS_LEVEL>" + AccessLevel + "</ACCESS_LEVEL>" +
                        "<TERMINAL_ID>" + TerminalID + "</TERMINAL_ID>" +
                        "<TOKEN>" + Token + "</TOKEN>" +
                       
                        "</BODY>";
            sxml = @"<MESSAGE>"
                    + GetXmlHeader("LINEPACK_AUTOMATION1")
                    + sxml
                 + @"</MESSAGE>";
            return SendDataToHost(sxml);
           }

           catch (Exception ex)
                {
                    // throw ex;re
                    log.Error("Send ParameterChange" + ex +Paramterchange+AccessLevel+TerminalID+Token);
                }
            return true;
        }






        //Remove Node 

        private delegate bool Client_sendFG01_FG02_MOVEdelegate(string BoxNumber, string message);
        public bool Client_sendFG01_FG02_MOVE(string BoxNumber, string message)
        {

            if (Application.Current.Dispatcher.CheckAccess())
            {
                try
                {
                    
                    XmlElement myelement = this.FLTrackingdoc.DocumentElement;  //check
                    XmlNode mynode = myelement.SelectSingleNode("descendant::LABEL[ID='" + BoxNumber + "']");
                    myelement.RemoveChild(mynode);
                    //remove box number form trackinglist
                    XmlElement root1 = this.FinishingLabelsInfo.DocumentElement;
                    XmlNode node = root1.SelectSingleNode("descendant::MESSAGE[BODY/BOX_NUMBER='" + BoxNumber + "']");
                    root1.RemoveChild(node);
                   

                    Deletelog.Info("Delete Finishing Label " + BoxNumber +","+ message);
                   

                }
                catch (Exception ex)
                {
                    // throw ex;re
                  //  Deletelog.Error("remove child node " + ex +","+ message+"," + BoxNumber);
                }
            }
            else
            {
                // or BeginInvoke()
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Send, new Client_sendFG01_FG02_MOVEdelegate(Client_sendFG01_FG02_MOVE), BoxNumber, message);
                return true;
            }
            return true;
        }






       private delegate bool Client_sendFG01_FG02_MOVE1delegate(string BoxNumber, string message);
        public bool Client_sendFG01_FG02_MOVE1(string BoxNumber, string message)
        {

            if (Application.Current.Dispatcher.CheckAccess())
            {
                try
                {
                    string sxml = "<BODY>" +
                                    "<MESSAGE_TYPE>" + message + "</MESSAGE_TYPE>" +
                                    "<BOX_NUMBER>" + BoxNumber + "</BOX_NUMBER>" +
                                  "</BODY>";
                    sxml = @"<MESSAGE>"
                            + GetXmlHeader("LINEPACK_AUTOMATION1")
                            + sxml
                         + @"</MESSAGE>";
                   // XmlElement myelement = this.FLTrackingdoc.DocumentElement;  //check
                   // XmlNode mynode = myelement.SelectSingleNode("descendant::LABEL[ID='" + BoxNumber + "']");
                   // myelement.RemoveChild(mynode);
                   // //remove box number form trackinglist
                   // XmlElement root1 = this.FinishingLabelsInfo.DocumentElement;
                   // XmlNode node = root1.SelectSingleNode("descendant::MESSAGE[BODY/BOX_NUMBER='" + BoxNumber + "']");
                   // root1.RemoveChild(node);
                   // //node.RemoveChild(node);
                   // //remove all related files.
                   // //find all filesname

                   linePack.Info("FG01_FG02_MOVE Message send to Middleware " + BoxNumber + message);
                   //// stn8log = BoxNumber + " FG01_FG02_MOVE";

                   // //node.RemoveAll();

                    return SendDataToHost(sxml);

                }
                catch (Exception ex)
                {
                    // throw ex;re
                    log.Error("FG01_FG02_MOVE Message send to Middleware Error" + ex + message + BoxNumber);
                }
            }
            else
            {
                // or BeginInvoke()
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Send, new Client_sendFG01_FG02_MOVE1delegate(Client_sendFG01_FG02_MOVE1), BoxNumber, message);
                return true;
            }
            return true;
        }






        private delegate bool Client_sendFG01_FG02_MOVE6delegate(string BoxNumber);
        public bool Client_sendFG01_FG02_MOVE6(string BoxNumber)
        {

            if (Application.Current.Dispatcher.CheckAccess())
            {
                try
                {
                    string sxml = "<BODY>" +
                                    "<MESSAGE_TYPE>FG01_FG02_MOVE</MESSAGE_TYPE>" +
                                    "<BOX_NUMBER>" + BoxNumber + "</BOX_NUMBER>" +
                                  "</BODY>";
                    sxml = @"<MESSAGE>"
                            + GetXmlHeader("LINEPACK_AUTOMATION1")
                            + sxml
                         + @"</MESSAGE>";           
                                 
                  

                    return SendDataToHost(sxml);

                }
                catch (Exception ex)
                {
                   
                    log.Error("FG01_FG02_MOVE Message QC1 send to Middleware become Error" + ex + BoxNumber);
                }
            }
            else
            {
                // or BeginInvoke()
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Send, new Client_sendFG01_FG02_MOVE6delegate(Client_sendFG01_FG02_MOVE6), BoxNumber);
                return true;
            }
            return true;
        }

      


          private delegate bool Client_sendFG01_FG02_MOVE62delegate(string BoxNumber);
        public bool Client_sendFG01_FG02_MOVE62(string BoxNumber)
        {

            if (Application.Current.Dispatcher.CheckAccess())
            {
                try
                {
                    string sxml = "<BODY>" +
                                    "<MESSAGE_TYPE>FG01_FG02_MOVE</MESSAGE_TYPE>" +
                                    "<BOX_NUMBER>" + BoxNumber + "</BOX_NUMBER>" +
                                  "</BODY>";
                    sxml = @"<MESSAGE>"
                            + GetXmlHeader("LINEPACK_AUTOMATION1")
                            + sxml
                         + @"</MESSAGE>";           
                                 
                  

                    return SendDataToHost(sxml);

                }
                catch (Exception ex)
                {
                   
                    log.Error("FG01_FG02_MOVE Message QC2 send to Middleware become Error" + ex + BoxNumber);
                }
            }
            else
            {
                // or BeginInvoke()
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Send, new Client_sendFG01_FG02_MOVE62delegate(Client_sendFG01_FG02_MOVE62), BoxNumber);
                return true;
            }
            return true;
        }








        public bool Client_sendReject_MOVE(string BoxNumber)
        {


            try
            {
                string sxml = "<BODY>" +
                                "<MESSAGE_TYPE>FG01_FG02_Reject</MESSAGE_TYPE>" +
                                "<BOX_NUMBER>" + BoxNumber + "</BOX_NUMBER>" +
                              "</BODY>";
                sxml = @"<MESSAGE>"
                        + GetXmlHeader("LINEPACK_AUTOMATION1")
                        + sxml
                     + @"</MESSAGE>";
                XmlElement myelement = this.FLTrackingdoc.DocumentElement;  //check
                XmlNode mynode = myelement.SelectSingleNode("descendant::LABEL[ID='" + BoxNumber + "']");
                myelement.RemoveChild(mynode);
                //remove box number form trackinglist
                XmlElement root1 = this.FinishingLabelsInfo.DocumentElement;
                XmlNode node = root1.SelectSingleNode("descendant::MESSAGE[BODY/BOX_NUMBER='" + BoxNumber + "']");
                root1.RemoveChild(node);
                //node.RemoveChild(node);
                //remove all related files.
                //find all filesname

               linePack.Info("FG01_FG02_Reject from station" + BoxNumber);

                //node.RemoveAll();

                return SendDataToHost(sxml);

            }
            catch (Exception ex)
            {
                // throw ex;

                log.Error("remove child node" + ex);
            }
            return true;
        }

        public bool Client_sendAQL_BOX(string BoxNumber)
        {
            string sxml = "<BODY>" +
                            "<MESSAGE_TYPE>AQL_BOX</MESSAGE_TYPE>" +
                            "<BOX_NUMBER>" + BoxNumber + "</BOX_NUMBER>" +
                            "</BODY>";
            sxml = @"<MESSAGE>"
                    + GetXmlHeader("LINEPACK_AUTOMATION1")
                    + sxml
                 + @"</MESSAGE>";
            return SendDataToHost(sxml);
        }

        public bool Client_SendAlarmMessage(string AlarmID, string AlarmDescription, string AlarmStatus)
        {
            try
            {

                string sxml = "<BODY>" +
                                "<MESSAGE_TYPE>ALARM</MESSAGE_TYPE>" +
                                "<ALARM_ID>" + AlarmID + "</ALARM_ID >" +
                                "<ALARM_DESC>" + AlarmDescription + "</ALARM_DESC>" +
                                "<ALARM_STATUS>" + AlarmStatus + "</ALARM_STATUS>" +
                              "</BODY>";
                sxml = @"<MESSAGE>"
                        + GetXmlHeader("LINEPACK_AUTOMATION1")
                        + sxml
                     + @"</MESSAGE>";
                return SendDataToHost(sxml);
            }
            catch (Exception ex)
            {
                // throw ex;re
                log.Error("Sending Error" + ex);
            }
            return false;
        }
        //end of IGT as client function
        //use below function if IGT is as server
        public bool Client_SendEventMsg(string EventID, string EventMsg, string[] EventAtt) //Send Jam FL to Middleware
        {
            try
            {
                string ATTString = "";
                for (int i = 0; i < EventAtt.Length; i++)
                {
                    ATTString = ATTString + "<ATTR ID= LotNumber>" + EventAtt[i] + "</ATTR >";
                }
                string sxml = "<BODY>" +
                                "<MESSAGE_TYPE>EVENT</MESSAGE_TYPE>" +
                                "<EVENT_ID>" + EventID + "</EVENT_ID >" +
                                "<EVENT_DESC>" + EventMsg + "</EVENT_DESC>" +
                                "<ATTRIBUTES>" +
                                ATTString +
                                "</ATTRIBUTES>" +
                              "</BODY>";
                sxml = @"<MESSAGE>"
                        + GetXmlHeader("LINEPACK_AUTOMATION1")
                        + sxml
                     + @"</MESSAGE>";
                log.Error(sxml);
                return SendDataToHost(sxml);
            }
            catch (Exception ex)
            {
                // throw ex;re
                log.Error("Sending Error" + ex);
            }
            return false;
        }
        public bool Client_SendEventMsg(string EventAtt)
        {
            try
            {
                //Check if Attribute available
                
                string[] ATTArray = EventAtt.Split(';');
                string EventID = ATTArray[0];
                string EventMsg = ATTArray[1];
                string ATTString = "";
                for (int i = 2; i < ATTArray.Length ; i= i + 2)
                {
                    ATTString = ATTString + "<ATTR ID=" + '"' + ATTArray[i] + '"' + ">" + ATTArray[i + 1] + "</ATTR >";
                }
                string sxml = "<BODY>" +
                                "<MESSAGE_TYPE>EVENT</MESSAGE_TYPE>" +
                                "<EVENT_ID>" + EventID + "</EVENT_ID >" +
                                "<EVENT_DESC>" + EventMsg + "</EVENT_DESC>" +
                                "<ATTRIBUTES>" +
                                ATTString +
                                "</ATTRIBUTES>" +
                              "</BODY>";
                sxml = @"<MESSAGE>"
                        + GetXmlHeader("LINEPACK_AUTOMATION1")
                        + sxml
                     + @"</MESSAGE>";
                return SendDataToHost(sxml);
            }
            catch (Exception ex)
            {
                // throw ex;re
                log.Error("Sending Error" + ex);
            }
            return false;
        }
        public bool Client_SendAlarmMessage2(string AlarmID, string AlarmDescription, string AlarmStatus)
        {
            try
            {

                string sxml = "<BODY>" +
                                "<MESSAGE_TYPE>ALARM</MESSAGE_TYPE>" +
                                "<ALARM_ID>" + AlarmID + "</ALARM_ID >" +
                                "<ALARM_DESC>" + AlarmDescription + "</ALARM_DESC>" +
                                "<ALARM_STATUS>" + AlarmStatus + "</ALARM_STATUS>" +
                              "</BODY>";
                sxml = @"<MESSAGE>"
                        + GetXmlHeader("LINEPACK_AUTOMATION1")
                        + sxml
                     + @"</MESSAGE>";
                return SendDataToHost(sxml);
            }
            catch (Exception ex)
            {
                // throw ex;re
                log.Error("Sending Error" + ex);
            }
            return false;
        }




         public bool Client_SendAlarmMessage5(string AlarmID, string AlarmDescription, string AlarmStatus)
        {
            try
            {

                string sxml = "<BODY>" +
                                "<MESSAGE_TYPE>ALARM</MESSAGE_TYPE>" +
                                "<ALARM_ID>" + AlarmID + "</ALARM_ID >" +
                                "<ALARM_DESC>" + AlarmDescription + "</ALARM_DESC>" +
                                "<ALARM_STATUS>" + AlarmStatus + "</ALARM_STATUS>" +
                              "</BODY>";
                sxml = @"<MESSAGE>"
                        + GetXmlHeader("LINEPACK_AUTOMATION1")
                        + sxml
                     + @"</MESSAGE>";
                return SendDataToHost(sxml);
            }
            catch (Exception ex)
            {
                // throw ex;re
                log.Error("Sending Error" + ex);
            }
            return false;
        }

        public bool SendScanBox(string BoxNumber, string BoxNumber1)
        {
            string header = @"<HEADER><EQUIPMENT_ID>LINEPACK_AUTOMATION1</EQUIPMENT_ID><TIMESTAMP>" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff") + "</TIMESTAMP></HEADER>";
            string sxml = @"<BODY><MESSAGE_TYPE>SCANNED_BOX_NUMBER</MESSAGE_TYPE><BOX_NUMBER>" + BoxNumber + "</BOX_NUMBER><BOX_NUMBER1>" + BoxNumber1 + "</BOX_NUMBER1></BODY>";
            sxml = @"<MESSAGE>" + header + sxml + @"</MESSAGE>";
            oTcpClass.TCPSendData(sxml);
            //wait for reply and time out..
            //if (ServerReplyEvt.WaitOne(60000))//60sec time out
            //{
            //    ServerReplyEvt.Reset();
            //    return true;
            //}
            //ServerReplyEvt.Reset();
            //return false;
            return true;
        }
        public void SendQCStationLogout(string Station, string Operator)
        {
            string sxml =
                "<BODY>" +
                    "<MESSAGE_TYPE>QC_STATION_LOGOUT</MESSAGE_TYPE>" +
                    "<STATION_ID>" + Station + "</STATION_ID>" +
                    "<USER_NAME>" + Operator + "<USER_NAME>" +
                "</BODY>";
            oTcpClass.TCPSendData(sxml);
        }
        public void SendParameterchange(string User, string ParamterName, string StationName, string OldValue, string NewValue)
        {
            string sxml =
                        "<BODY>" +
                        "<MESSAGE_TYPE>PARAMETER_CHANGE</MESSAGE_TYPE>" +
                        "<STATION_NAME>" + StationName + "</STATION_NAME>" +
                        "<PARAMETER_NAME>" + ParamterName + "</PARAMETER_NAME>" +
                        "<USER_NAME>" + User + "</USER_NAME>" +
                        "<OLD_VALUE>" + OldValue + "</OLD_VALUE>" +
                        "<NEW_VALUE>" + NewValue + "</NEW_VALUE>" +
                        "</BODY>";
            oTcpClass.TCPSendData(sxml);
        }


   public bool Client_SendEventMessage(string EventID, string EventDescription, string AttributeName,string AttributeValue)
        {
            try
            {

                string sxml = "<BODY>" +
                                "<MESSAGE_TYPE>EVENT</MESSAGE_TYPE>" +
                                "<EVENT_ID>" + EventID + "</EVENT_ID >" +
                                "<EVENT_DESC>" + EventDescription + "</EVENT_DESC>" +
                                "<ATTRIBUTES>" 
                                +  "<ATTR  ID='"+ AttributeName + "' >" 
                                + AttributeValue +                                
                                "</ATTR>" +                                
                                "</ATTRIBUTES>" +
                              "</BODY>";
                sxml = @"<MESSAGE>"
                        + GetXmlHeader("LINEPACK_AUTOMATION1")
                        + sxml
                     + @"</MESSAGE>";
                return SendDataToHost(sxml);
            }
            catch (Exception ex)
            {
                // throw ex;re
                log.Error("Sending Event Error" + ex);
            }
            return false;
        }




       public bool Client_SendEventMessagest6(string EventID, string EventDescription, string AttributeName,string AttributeValue)
        {
            try
            {

                string sxml = "<BODY>" +
                                "<MESSAGE_TYPE>EVENT</MESSAGE_TYPE>" +
                                "<EVENT_ID>" + EventID + "</EVENT_ID >" +
                                "<EVENT_DESC>" + EventDescription + "</EVENT_DESC>" +
                                "<ATTRIBUTES>" 
                                +  "<ATTR  ID='"+ AttributeName + "' >" 
                                + AttributeValue +                                
                                "</ATTR>" +                                
                                "</ATTRIBUTES>" +
                              "</BODY>";
                sxml = @"<MESSAGE>"
                        + GetXmlHeader("LINEPACK_AUTOMATION1")
                        + sxml
                     + @"</MESSAGE>";
                return SendDataToHost(sxml);
            }
            catch (Exception ex)
            {
                // throw ex;re
                log.Error("Sending Event st 5 Sealer1 Error" + ex);
            }
            return false;
        }
        //st8
        public void sendFG01_FG02_MOVE(string BoxNumber)
        {
            string header = @"<HEADER><EQUIPMENT_ID>LINEPACK_AUTOMATION1</EQUIPMENT_ID><TIMESTAMP>" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff") + "</TIMESTAMP></HEADER>";
            string sxml = "<BODY>" +
                            "<MESSAGE_TYPE>FG01_FG02_MOVE</MESSAGE_TYPE>" +
                            "<BOX_NUMBER>" + BoxNumber + "</BOX_NUMBER >" +
                          "</BODY>";
            sxml = @"<MESSAGE>" + header + sxml + @"</MESSAGE>";
            oTcpClass.TCPSendData(sxml);
        }
        public void sendAQL_BOX(string BoxNumber)
        {
            string sxml = "<BODY>" +
                            "<MESSAGE_TYPE>AQL_BOX</MESSAGE_TYPE>" +
                            "<BOX_NUMBER>" + BoxNumber + "</BOX_NUMBER>" +
                            "</BODY>";
            oTcpClass.TCPSendData(sxml);
        }
        public void SendAlarmMessage(string AlarmID, string AlarmDescription, string AlarmStatus)
        {
            string sxml = "<BODY>" +
                            "<MESSAGE_TYPE>ALARM</MESSAGE_TYPE>" +
                            "<ALARM_ID>" + AlarmID + "</ALARM_ID >" +
                            "<ALARM_Desc>" + AlarmDescription + "</ALARM_Desc>" +
                            "<ALARM_STATUS>" + AlarmStatus + "</ALARM_STATUS>" +
                          "</BODY>";
            oTcpClass.TCPSendData(sxml);
        }




        public bool Client_SendEventMessageForSealer1(string EventID, string FninishingLabel, string SealerNumber, short VProgramNumber, short AVacuumReading, short RVacuumReading, short ACurrentReading, short RCurrentReading, float STime, string UseFunction)
        {
            try
            {

                string sxml = "<BODY>" +
                                "<MESSAGE_TYPE>EVENT</MESSAGE_TYPE>" +
                                "<EVENT_ID>" + "503" + "</EVENT_ID >" +
                                "<EVENT_DESC>Station5MBBSealingCompleted</EVENT_DESC>" +
                                "<ATTRIBUTES>" + 

                                "<ATTR  ID=\"LotNumber\" >" +
                                FninishingLabel +                                
                                "</ATTR>" +     
                           

                                 "<ATTR  ID=\"SealerID\" >" +SealerNumber+
                                                               
                                "</ATTR>" +


                                 "<ATTR  ID=\"VacuumProgramNumber\" >" +
                                  VProgramNumber +                                
                                "</ATTR>" +   

                                 "<ATTR  ID=\"ActualVacuumReading\" >" +
                                  "-" + AVacuumReading +                                
                                "</ATTR>" +   

                                "<ATTR  ID=\"RecipeVacuumReading\" >" +
                                  "-" + RVacuumReading +                                
                                "</ATTR>" +   

                                 "<ATTR  ID=\"ActualCurrentReading\" >" +
                                  ACurrentReading +                                
                                "</ATTR>" +   

                                "<ATTR  ID=\"RecipeCurrentReading\" >" +
                                  RCurrentReading +                                
                                "</ATTR>" +  
                                "<ATTR  ID=\"SealingTime\" >" +
                                  STime +                                
                                "</ATTR>" +   

                                "<ATTR  ID=\"FunctionUsed\" >" +
                                  UseFunction +                                
                                "</ATTR>" +  

                                "</ATTRIBUTES>" +
                              "</BODY>";
                sxml = @"<MESSAGE>"
                        + GetXmlHeader("LINEPACK_AUTOMATION1")
                        + sxml
                     + @"</MESSAGE>";
                return SendDataToHost(sxml);
            }
            catch (Exception ex)
            {
                // throw ex;re
                log.Error("Sending Event Error" + ex);
            }
            return false;
        }




        public bool Client_SendEventMessageForSealer2(string EventID, string FninishingLabel, string SealerNumber, short VProgramNumber, short AVacuumReading, short RVacuumReading, short ACurrentReading, short RCurrentReading, float STime, string UseFunction)
        {
            try
            {

                string sxml = "<BODY>" +
                                "<MESSAGE_TYPE>EVENT</MESSAGE_TYPE>" +
                                "<EVENT_ID>" + "503" + "</EVENT_ID >" +
                                "<EVENT_DESC>Station5MBBSealingCompleted</EVENT_DESC>" +
                                "<ATTRIBUTES>" + 

                                "<ATTR  ID=\"LotNumber\" >" +
                                FninishingLabel +                                
                                "</ATTR>" +     
                           

                                 "<ATTR  ID=\"SealerID\" >" +
                                 SealerNumber+
                                                               
                                "</ATTR>" +


                                 "<ATTR  ID=\"VacuumProgramNumber\" >" +
                                  VProgramNumber +                                
                                "</ATTR>" +   

                                 "<ATTR  ID=\"ActualVacuumReading\" >" +
                                 "-" + AVacuumReading +                                
                                "</ATTR>" +   

                                "<ATTR  ID=\"RecipeVacuumReading\" >" +
                                "-" + RVacuumReading +                                
                                "</ATTR>" +   

                                 "<ATTR  ID=\"ActualCurrentReading\" >" +
                                  ACurrentReading +                                
                                "</ATTR>" +   

                                "<ATTR  ID=\"RecipeCurrentReading\" >" +
                                  RCurrentReading +                                
                                "</ATTR>" +  
                                "<ATTR  ID=\"SealingTime\" >" +
                                  STime +                                
                                "</ATTR>" +   

                                "<ATTR  ID=\"FunctionUsed\" >" +
                                  UseFunction +                                
                                "</ATTR>" +  

                                "</ATTRIBUTES>" +
                              "</BODY>";
                sxml = @"<MESSAGE>"
                        + GetXmlHeader("LINEPACK_AUTOMATION1")
                        + sxml
                     + @"</MESSAGE>";
                return SendDataToHost(sxml);
            }
            catch (Exception ex)
            {
                // throw ex;re
                log.Error("Sending Event Error" + ex);
            }
            return false;
        }






        public bool Client_SendEventMessageForSealer3(string EventID, string FninishingLabel, string SealerNumber, short VProgramNumber, short AVacuumReading, short RVacuumReading, short ACurrentReading, short RCurrentReading, float STime, string UseFunction)
        {
            try
            {

                string sxml = "<BODY>" +
                                "<MESSAGE_TYPE>EVENT</MESSAGE_TYPE>" +
                                "<EVENT_ID>" + "503" + "</EVENT_ID >" +
                                "<EVENT_DESC>Station5MBBSealingCompleted</EVENT_DESC>" +
                                "<ATTRIBUTES>" + 

                                "<ATTR  ID=\"LotNumber\" >" +
                                FninishingLabel +                                
                                "</ATTR>" +     
                           

                                 "<ATTR  ID=\"SealerID\" >" +
                                    SealerNumber+                           
                                "</ATTR>" +


                                 "<ATTR  ID=\"VacuumProgramNumber\" >" +
                                  VProgramNumber +                                
                                "</ATTR>" +   

                                 "<ATTR  ID=\"ActualVacuumReading\" >" +
                                 "-" + AVacuumReading +                                
                                "</ATTR>" +   

                                "<ATTR  ID=\"RecipeVacuumReading\" >" +
                                "-" + RVacuumReading +                                
                                "</ATTR>" +   

                                 "<ATTR  ID=\"ActualCurrentReading\" >" +
                                  ACurrentReading +                                
                                "</ATTR>" +   

                                "<ATTR  ID=\"RecipeCurrentReading\" >" +
                                  RCurrentReading +                                
                                "</ATTR>" +  
                                "<ATTR  ID=\"SealingTime\" >" +
                                  STime +                                
                                "</ATTR>" +   

                                "<ATTR  ID=\"FunctionUsed\" >" +
                                  UseFunction +                                
                                "</ATTR>" +  

                                "</ATTRIBUTES>" +
                              "</BODY>";
                sxml = @"<MESSAGE>"
                        + GetXmlHeader("LINEPACK_AUTOMATION1")
                        + sxml
                     + @"</MESSAGE>";
                return SendDataToHost(sxml);
            }
            catch (Exception ex)
            {
                // throw ex;re
                log.Error("Sending Event Error" + ex);
            }
            return false;
        }








        //End of IGT Server Function
    }
}
