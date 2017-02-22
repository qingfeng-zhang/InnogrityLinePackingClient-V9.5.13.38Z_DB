using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NLog;

public class TCPClass
{

    System.Net.Sockets.Socket oServer;
    System.Net.Sockets.Socket oClient;
    byte[] oString = new byte[2048];

    bool bConnected;
    bool bWaitingForConnection;
    System.Timers.Timer oTimer;

    Logger log = LogManager.GetLogger("NetworkTrace");
        
    public delegate void TCPConnectChangedEventHandler(bool bConnected);
    private TCPConnectChangedEventHandler TCPConnectChangedEvent;

    public event TCPConnectChangedEventHandler TCPConnectChanged
    {
        add
        {
            TCPConnectChangedEvent = (TCPConnectChangedEventHandler)System.Delegate.Combine(TCPConnectChangedEvent, value);
        }
        remove
        {
            TCPConnectChangedEvent = (TCPConnectChangedEventHandler)System.Delegate.Remove(TCPConnectChangedEvent, value);
        }
    }

    public delegate void TCPDataArrivalEventHandler(string sTCPData);
    private TCPDataArrivalEventHandler TCPDataArrivalEvent;

    public event TCPDataArrivalEventHandler TCPDataArrival
    {
        add
        {
            TCPDataArrivalEvent = (TCPDataArrivalEventHandler)System.Delegate.Combine(TCPDataArrivalEvent, value);
        }
        remove
        {
            TCPDataArrivalEvent = (TCPDataArrivalEventHandler)System.Delegate.Remove(TCPDataArrivalEvent, value);
        }
    }

    public TCPClass(System.Net.IPAddress address )
    {
        System.Net.Sockets.Socket oSocket = default(System.Net.Sockets.Socket);
        oSocket = new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
        oSocket.Bind(new System.Net.IPEndPoint(((System.Net.IPAddress)(address)), 5001));
        oSocket.Listen(10);
        
        f_WaitForConnection(oSocket);
        bConnected = false;
        oTimer = new System.Timers.Timer(100);
        oTimer.Elapsed += this.oTimer_Elapsed;
        oTimer.AutoReset = true;
        //oTimer.SynchronizingObject = System.Threading.Thread.CurrentThread
        oTimer.Start();
        log.Info("IGT Server Started");
    }
    public TCPClass()
    {
        System.Net.IPHostEntry oEntry = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
        string sIP;
        sIP = (string)(oEntry.AddressList.GetValue(0).ToString());
        System.Net.Sockets.Socket oSocket = default(System.Net.Sockets.Socket);
        oSocket = new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
        oSocket.Bind(new System.Net.IPEndPoint(((System.Net.IPAddress)(oEntry.AddressList.GetValue(2))), 5001));
        oSocket.Listen(10);
        f_WaitForConnection(oSocket);
        bConnected = false;
        oTimer = new System.Timers.Timer(100);
        oTimer.Elapsed += this.oTimer_Elapsed;
        oTimer.AutoReset = true;
        //oTimer.SynchronizingObject = System.Threading.Thread.CurrentThread
        oTimer.Start();
        log.Info("IGT Server Started");
    }

    private void f_WaitForConnection(System.Net.Sockets.Socket oSocket)
    {
        oSocket.BeginAccept(new System.AsyncCallback(ConnectionRequested), oSocket);
        bWaitingForConnection = true;
    }

    private void ConnectionRequested(System.IAsyncResult oResult)
    {
        oServer = (System.Net.Sockets.Socket)oResult.AsyncState;
        oClient = oServer.EndAccept(oResult);
        Console.WriteLine("Received connection request from " + oClient.RemoteEndPoint.ToString());
        bWaitingForConnection = false;
        bConnected = true;
        if (TCPConnectChangedEvent != null)
            TCPConnectChangedEvent(bConnected);
        f_WaitForData(oClient);
    }

    private void f_WaitForData(System.Net.Sockets.Socket oSocket)
    {
        oSocket.BeginReceive(oString, 0, oString.Length, System.Net.Sockets.SocketFlags.None, new System.AsyncCallback(DataArrival), oSocket);
    }

    private void DataArrival(System.IAsyncResult oResult)
    {
        log.Info("Data Arrive");

        System.Net.Sockets.Socket oSocket = (System.Net.Sockets.Socket)oResult.AsyncState;
        try
        {
            int nBytes = System.Convert.ToInt32(oSocket.EndReceive(oResult));
            if (nBytes > 0)
            {
                string sData = (string)(System.Text.Encoding.ASCII.GetString(oString, 0, nBytes));
                if (TCPDataArrivalEvent != null)
                    TCPDataArrivalEvent(sData);
                f_WaitForData(oSocket);
            }
            else
            {
                oSocket.Shutdown(System.Net.Sockets.SocketShutdown.Both);
                oSocket.Close();
                log.Info("Socket Closed");

            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
            log.Error("Error:" + ex.Message);

        }
    }

    public void TCPSendData(string sData)
    {
        byte[] oBuffer = System.Text.Encoding.ASCII.GetBytes(sData);
        oClient.Send(oBuffer, oBuffer.Length, System.Net.Sockets.SocketFlags.None);
        log.Info("Data Sent");

    }

    private void oTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
        if (bConnected)
        {
            if (!oClient.Connected)
            {
                bConnected = false;
                if (TCPConnectChangedEvent != null)
                    TCPConnectChangedEvent(bConnected);
                Console.WriteLine("Connection lost");
                log.Info("Connection Lost");
                f_WaitForConnection(oServer);
            }
        }
    }

}