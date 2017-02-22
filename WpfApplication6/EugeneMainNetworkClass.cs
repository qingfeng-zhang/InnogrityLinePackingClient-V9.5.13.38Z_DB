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
using System.IO;
using System.Windows.Threading;
using System.Windows;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace IGTwpf
{
    partial class MainNetworkClass : INotifyPropertyChanged
    {
        private int maxloglength = 8000;
        //Eugene gui functions
        private string _startdate = "Error log started since: " + DateTime.Now.ToString();
        public string startdate
        {
            get
            {
                return _startdate;
            }
        }

        private string _OperatorLog = DateTime.Now.ToShortTimeString() + " Connected to Innogrity Server.";
        private string _OperatorLogPrevous = "0";
        public string OperatorLog
        {
            get { return _OperatorLog; }
            set
            {
                if (CheckIfNullorRepeated(value, _OperatorLogPrevous))
                {
                    _OperatorLogPrevous = value;

                    _OperatorLog = IndentToNextline(value, _OperatorLog);

                    OnPropertyChanged("OperatorLog");
                }
            }
        }

        public string AllLiveLogs
        {
            set
            {
                stn2log = value;
                stn3log = value;
                stn4log = value;
                stn5log = value;
                stn6log = value;
                stn7log = value;
                stn8log = value;
            }
        }

        private string _stn2log = DateTime.Now.ToShortTimeString() + " Station 2 log initialized";
        private string _stn2logPrevious = "0";
        public string stn2log
        {
            get { return _stn2log; }
            set
            {
                if (CheckIfNullorRepeated(value, _stn2logPrevious))
                {
                    _stn2logPrevious = value;

                    _stn2log = IndentToNextline(value, _stn2log);

                    OnPropertyChanged("stn2log");
                }
            }
        }

        private string _stn3log = DateTime.Now.ToShortTimeString() + " Station 3 log initialized";
        private string _stn3logPrevious = "0";
        public string stn3log
        {
            get { return _stn3log; }
            set
            {
                if (CheckIfNullorRepeated(value, _stn3logPrevious))
                {
                    _stn3logPrevious = value;
                    _stn3log = IndentToNextline(value, _stn3log);
                    OnPropertyChanged("stn3log");
                }
            }
        }

        private string _stn4log = DateTime.Now.ToShortTimeString() + " Station 4 log initialized";
        private string _stn4logPrevious = "0";
        public string stn4log
        {
            get { return _stn4log; }
            set
            {
                if (CheckIfNullorRepeated(value, _stn4logPrevious))
                {
                    _stn4logPrevious = value;
                    _stn4log = IndentToNextline(value, _stn4log);
                    OnPropertyChanged("stn4log");
                }
            }
        }

        private string _stn5log = DateTime.Now.ToShortTimeString() + " Station 5 log initialized";
        private string _stn5logPrevious = "0";
        public string stn5log
        {
            get { return _stn5log; }
            set
            {
                if (CheckIfNullorRepeated(value, _stn5logPrevious))
                {
                    _stn5logPrevious = value;
                    _stn5log = IndentToNextline(value, _stn5log);
                    OnPropertyChanged("stn5log");
                }
            }
        }

        private string _stn6log = DateTime.Now.ToShortTimeString() + " Station 6 log initialized";
        private string _stn6logPrevious = "0";
        public string stn6log
        {
            get { return _stn6log; }
            set
            {
                if (CheckIfNullorRepeated(value, _stn6logPrevious))
                {
                    _stn6logPrevious = value;
                    _stn6log = IndentToNextline(value, _stn6log);
                    OnPropertyChanged("stn6log");
                }
            }
        }

        private string _stn7log = DateTime.Now.ToShortTimeString() + " Station 7 log initialized";
        private string _stn7logPrevious = "0";
        public string stn7log
        {
            get { return _stn7log; }
            set
            {
                if (CheckIfNullorRepeated(value, _stn7logPrevious))
                {
                    _stn7logPrevious = value;
                    _stn7log = IndentToNextline(value, _stn7log);
                    OnPropertyChanged("stn7log");
                }
            }
        }

        private string _stn8log = DateTime.Now.ToShortTimeString() + " Station 8 log initialized";
        private string _stn8logPrevious = "0";
        public string stn8log
        {
            get { return _stn8log; }
            set
            {
                if (CheckIfNullorRepeated(value, _stn8logPrevious))
                {
                    _stn8logPrevious = value;
                    _stn8log = IndentToNextline(value, _stn8log);
                    OnPropertyChanged("stn8log");
                }
            }
        }

        public string IndentToNextline(string NewValue, string OldValue)
        {
            OldValue = StringShortener(OldValue);
            return DateTime.Now.ToShortTimeString() + " " + NewValue + "\r\n" + OldValue;
        }

        public bool CheckIfNullorRepeated(string value, string previoussting)
        {
            if (value != "" && value != previoussting)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public string StringShortener(string thestring)
        {
            if (thestring.Length > maxloglength)
            {
                thestring = thestring.Substring(0, maxloglength);
            }
            return thestring;
        }

        //End of Eugene's gui functions
    }
}
