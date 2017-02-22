using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net.Sockets;
using System.Xml;
using NLog;
using System.ComponentModel;
using System.Threading;
using System.IO;
using System.Threading.Tasks;

using System.Windows.Threading;
using System.Windows;
using System.Collections.Concurrent;


namespace InnogrityLinePackingClient
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

    class TelnetClient : INotifyPropertyChanged
    {
        public ManualResetEvent WriteToHostCompleteEvt;
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public TelnetClient()
        {
            WriteToHostCompleteEvt = new ManualResetEvent(false);
            connected = false;
        }
                
        public TcpClient tcpClient;
        Logger log = LogManager.GetLogger("TelnetInfo");

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
                log.Error("TCP port close");
            }
            catch (Exception ex)
            {
                msg = "Unable to complete write.";
                tcpClient.Close();
                connected = false;
                log.Error("TCP port close");

            }
            finally
            {
            }

        }

        public bool ConnectToHost(System.Net.IPAddress address, int port)
        {
            #region TCPClientConnection
            tcpClient = new TcpClient();
            log.Info("Connecting.....");

            IAsyncResult ar = tcpClient.BeginConnect(address, port, null, null);
            
            System.Threading.WaitHandle wh = ar.AsyncWaitHandle;
            try
            {
                if (!ar.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(5), false))
                {
                    tcpClient.Close();
                    connected = false;
                    log.Error("TCP Connection to " + address + "port " + port.ToString() + " timeout");
                    throw new TimeoutException();
                }

                tcpClient.EndConnect(ar);
                connected = true;
                log.Error("TCP Connection to " + address + "port " + port.ToString() + " completed");
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
        }//connect to Telnet server
        public bool SendDataToHost(byte[] cmd)//depreciated
        {
            try
            {

                //meadd Start
                //  int msglength = cmd.Length;
                //  byte[] size = BitConverter.GetBytes(msglength);//modified to make sure the number of byte sent can be recieved

                //  byte[] outputBuffer = cmd;
                //  NetworkStream strm = tcpClient.GetStream();
                //  strm.WriteTimeout = 1000;
                //  //strm.Write(size, 0, 4);

                //  WriteToHostCompleteEvt.Reset();
                //  strm.BeginWrite(size, 0, 4,
                //                  WriteAsyncCallback,
                //                  new MyAsyncInfo(size, strm));
                //  if (!WriteToHostCompleteEvt.WaitOne(5000)) throw new TimeoutException();
                //  //WriteToHostCompleteEvt.Reset();
                //  strm.BeginWrite(outputBuffer, 0, msglength,
                //                                 WriteAsyncCallback,
                //                                 new MyAsyncInfo(outputBuffer, strm));
                ////if (!WriteToHostCompleteEvt.WaitOne(5000)) throw new TimeoutException();


                //meadd End
                                
                int msglength = cmd.Length;
                NetworkStream strm = tcpClient.GetStream();
                strm.WriteTimeout = 1000;
                WriteToHostCompleteEvt.Reset();
                strm.BeginWrite(cmd, 0, msglength,
                                               WriteAsyncCallback,
                                               new MyAsyncInfo(cmd, strm));
                if (!WriteToHostCompleteEvt.WaitOne(5000)) throw new TimeoutException();               
            }
            catch (Exception ex)
            {
                log.Error("Send Data To Host Error : " + ex.ToString());
                //close coms.. at write async function
                if (tcpClient != null)
                    tcpClient.Close();
                connected = false;
                return false;
            }
            finally
            {
                WriteToHostCompleteEvt.Reset();
            }
            return true;
        }//send data to middleware server
        public void Close()
        {
            connected = false;
            tcpClient.Close();
        }
        public bool SendDataToHost(string cmd)//depreciated
        {
            try
            {
                int msglength = cmd.Length;
                byte[] outputBuffer = Encoding.ASCII.GetBytes(cmd);
                NetworkStream strm = tcpClient.GetStream();
                strm.WriteTimeout = 1000;
                WriteToHostCompleteEvt.Reset();
                strm.BeginWrite(outputBuffer, 0, msglength,
                                               WriteAsyncCallback,
                                               new MyAsyncInfo(outputBuffer, strm));
                if (!WriteToHostCompleteEvt.WaitOne(5000)) throw new TimeoutException();
            }
            catch (Exception ex)
            {
                log.Error("Send Data To Host Error : " + ex.ToString());
                //close coms.. at write async function
                tcpClient.Close();
                connected = false;
                return false;
            }
            finally
            {
                WriteToHostCompleteEvt.Reset();
            }
            return true;
        }//send data to middleware server
        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }
        public string GetDataFromHost(ref byte[] data, int size)
        {
            try
            {
                #region RecieveDataTCP
                // Start message receive

                NetworkStream networkStream = tcpClient.GetStream();
                // Set a 10 millisecond timeout for reading.
                if (!networkStream.DataAvailable) return "No Data";
                networkStream.ReadTimeout = 10;
                networkStream.Read(data, 0, size);//read only first 4 byte of data
                return ByteArrayToString(data);
                #endregion
            }
            catch (Exception Ex)
            {
            }
            return "ERR";
        }//recieve data from middlewareserver

    }
}
