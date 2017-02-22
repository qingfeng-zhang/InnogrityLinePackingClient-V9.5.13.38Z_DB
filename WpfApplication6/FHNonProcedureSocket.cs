using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using NLog;
using InnogrityLinePackingClient;
using IGTwpf;
namespace FHNonProcedure
{
    public class FHNonProcedureSocket
    {
        Logger log = LogManager.GetLogger("OMronFH");
        NetworkThread networkthread;
        private Socket socket;
        public string HostAddress { get; set; }
        public int PortNumber { get; set; }
        public bool Connected
        {
            get
            {
                if (socket != null)
                {
                    return socket.Connected;
                }
                else
                {
                    return false;
                }
            }
        }
        public class FHNonProcedureSocketProcessingException : Exception
        {
            public FHNonProcedureSocketProcessingException()
                : base()
            {
            }
            public FHNonProcedureSocketProcessingException(string message)
                : base(message)
            {
            }
        }
        public void Close()
        {
            try
            {
                if (socket != null)
                {
                    socket.Close();
                }
            }
            finally
            {
                if (socket != null)
                {
                    socket.Dispose();
                    socket = null;
                }
            }
        }
        public void Connect()
        {
            if (socket != null)
            {
                try
                {
                    Close();
                }
                catch (Exception ex) { throw ex; }
            }
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.NoDelay = true;
            socket.LingerState = new LingerOption(false, 0);
            socket.ReceiveTimeout = 300; //GY OLD  = 300
            socket.SendTimeout = 300; //GY OLD  = 300
            socket.Connect(HostAddress, PortNumber);
        }

        private string[] SendCommand(string command, int NumberOfReply)
        {
            // Clear receive buffer.
            if (socket.Available > 0)
            {
                byte[] discardData = new byte[socket.Available];
                socket.Receive(discardData);
            }
            // Send command
            socket.Send(Encoding.ASCII.GetBytes(command));
            // Loop to wait complete message.
            byte[] receiveBuffer = new byte[2048];
            StringBuilder receiveStringBuffer = new StringBuilder();
           //Thread.Sleep(5000);
            int timeoutcounter = 0;
            while (true)
            {
                try
                {
                    socket.ReceiveTimeout = 2000; //Old value 2000
                    int receiveLength = socket.Receive(receiveBuffer);
                    receiveStringBuffer.Append(Encoding.ASCII.GetString(receiveBuffer, 0, receiveLength));
                }
                catch (SocketException sockEx)
                {
                   // log.Error("exception from automeasure rx error code : " + sockEx.ErrorCode.ToString());
                }
                catch (Exception ex)
                {
                    log.Error("exception from automeasure rx error code : " + ex.ToString());
                }
               int counter = receiveStringBuffer.ToString().Split('\r').Length - 1;
         
                // Try to comprehend message
               string[] receivedMessages = receiveStringBuffer.ToString().Split('\r');
             
                int messageStart = -1, messageEnd = -1;
                for (int i = 0; i < receivedMessages.Length; i++)
                {
                    if (receivedMessages[i].Equals("OK") || receivedMessages[i].Equals("ER"))
                    {
                        messageStart = messageEnd + 1;
                        messageEnd = i;
                    }
                }
                if (messageStart < 0 || messageEnd < 0)
                {
                    if (timeoutcounter > 100) //Old value 100
                    {
                        throw new TimeoutException();
                    }
                    if (counter < NumberOfReply)
                    {
                        Thread.Sleep(100);
                        timeoutcounter++;
                        continue;
                    }
                }
                if (receivedMessages[messageEnd].Equals("ER"))
                {
                    //networkthread.MyEventQ.AddQ("999;OmronVisionErrReceive");
                    log.Error("ER from Omron");
                    throw new FHNonProcedureSocketProcessingException();
          
                }
                if (timeoutcounter > 100)
                {
                    // networkthread.MyEventQ.AddQ("12;OmronVisionTimeout");//Push message to stack
                    log.Error("Omron timeout");
                    throw new TimeoutException();
                }

                if (counter < NumberOfReply && receivedMessages[0].Trim() != "-1.0000")
                {
                    Thread.Sleep(100);
                    timeoutcounter++;
                    continue;
                }

                if (receivedMessages[messageEnd].Equals("OK"))
                {
                    string[] resultMessages = new string[messageEnd - messageStart + 1];
                    if (resultMessages.Length == 1)
                    {
                        messageStart = 1;
                    }
                    Array.Copy(receivedMessages, messageStart, resultMessages, 0, resultMessages.Length);
                    return resultMessages;
                }
            }
        }






        private string[] SendCommand(string command)
        {
            // Clear receive buffer.
            if (socket.Available > 0)
            {
                byte[] discardData = new byte[socket.Available];
                socket.Receive(discardData);
            }
            // Send command
            socket.Send(Encoding.ASCII.GetBytes(command));
            // Loop to wait complete message.
            byte[] receiveBuffer = new byte[2048];
            StringBuilder receiveStringBuffer = new StringBuilder();
            while (true)
            {
                int receiveLength = socket.Receive(receiveBuffer);
                receiveStringBuffer.Append(Encoding.ASCII.GetString(receiveBuffer, 0, receiveLength));
                // Try to comprehend message
                string[] receivedMessages = receiveStringBuffer.ToString().Split('\r');
                int messageStart = -1, messageEnd = -1;
                for (int i = 0; i < receivedMessages.Length; i++)
                {
                    if (receivedMessages[i].Equals("OK") || receivedMessages[i].Equals("ER"))
                    {
                        messageStart = messageEnd + 1;
                        messageEnd = i;
                    }
                }
                if (messageStart < 0 || messageEnd < 0)
                {
                    continue;
                }
                if (receivedMessages[messageEnd].Equals("ER"))
                {
                    throw new FHNonProcedureSocketProcessingException();
                }
                else if (receivedMessages[messageEnd].Equals("OK"))
                {
                    string[] resultMessages = new string[messageEnd - messageStart + 1];
                    Array.Copy(receivedMessages, messageStart, resultMessages, 0, resultMessages.Length);
                    return resultMessages;
                }
            }
        }

      

        public void TeachImage2()
        {
            string[] resultMessages = SendCommand("TEACH2\r");
            if (resultMessages[resultMessages.Length - 1].Equals("OK"))
            {
                return;
            }
            else
            {
                throw new FHNonProcedureSocketProcessingException();
            }
        }

        public string Clear()
        {
            string[] resultMessages = SendCommand("CLRMEAS\r");
            if (resultMessages.Length == 1 && resultMessages[0].Equals("OK"))
            {
                return null;
            }

            else
            {
                throw new FHNonProcedureSocketProcessingException();
            }
        }
        public string AutoMeasure3()
        {
            string[] resultMessages;

            resultMessages = SendCommand("AUTOMEASURE3\r", 2);//OK,1,1,1,1

            if (resultMessages.Length == 0)
            {
                return null;
            }
            else if (resultMessages[0].Equals("         1.0000"))
            {
                string tmpstring = resultMessages[0].TrimStart();
              
                return (tmpstring);
            }
            else if (resultMessages[0].TrimStart().Equals("-1.0000"))
            {
                string tmpstring = resultMessages[0].TrimStart();

                return (tmpstring);
            }
            else
            {
                throw new FHNonProcedureSocketProcessingException();
            }
        }
        //Barcode retry
        public string AutoMeasure98()
        {
            //OK\r,1\r,1\r,1\r
            //OK\r 
            //1\r
            //1\r
            //1\r
            //  Thread.Sleep(100);
            string[] resultMessages;

            resultMessages = SendCommand("AutoMeasure98\r", 2);//OK,1

            if (resultMessages.Length == 1 && resultMessages[0].Equals("OK"))
            {
                return null;
            }
            else if (resultMessages.Length == 3 && resultMessages[resultMessages.Length - 1].Equals("OK"))
            {
                return (resultMessages[resultMessages.Length - 3].Trim() + "," +
                        resultMessages[resultMessages.Length - 2].Trim());
            }
            else if (resultMessages.Length == 4 && resultMessages[resultMessages.Length - 1].Equals("OK"))
            {
                return (resultMessages[resultMessages.Length - 4].Trim() + "," +
                        resultMessages[resultMessages.Length - 3].Trim() + "," +
                        resultMessages[resultMessages.Length - 2].Trim());
            }
            else if (resultMessages.Length == 5 && resultMessages[resultMessages.Length - 1].Equals("OK"))
            {
                return (resultMessages[resultMessages.Length - 5].Trim() + "," +
                          resultMessages[resultMessages.Length - 4].Trim() + "," +
                          resultMessages[resultMessages.Length - 3].Trim() + "," +
                          resultMessages[resultMessages.Length - 2].Trim());
            }
            else if (resultMessages[resultMessages.Length - 1].Equals("OK"))
            {
                string tmpstring = "";
                for (int i = 0; i < resultMessages.Length - 1; i++)
                {
                    tmpstring = tmpstring + resultMessages[i];
                }
                return (tmpstring);
            }
            else
            {
                throw new FHNonProcedureSocketProcessingException();
            }
        }
        public string AutoMeasure99()
        {
            //OK\r,1\r,1\r,1\r
            //OK\r 
            //1\r
            //1\r
            //1\r
            //  Thread.Sleep(100);
            string[] resultMessages;

            resultMessages = SendCommand("AutoMeasure99\r", 4);//OK,1,1



            //string[] resultMessages = SendCommand("AutoMeasure\r");//OK,1,1,1,1

            if (resultMessages.Length == 1 && resultMessages[0].Equals("OK"))
            {
                return null;
            }
            else if (resultMessages.Length == 3 && resultMessages[resultMessages.Length - 1].Equals("OK"))
            {
                return (resultMessages[resultMessages.Length - 3].Trim() + "," +
                        resultMessages[resultMessages.Length - 2].Trim());
            }
            else if (resultMessages.Length == 4 && resultMessages[resultMessages.Length - 1].Equals("OK"))
            {
                return (resultMessages[resultMessages.Length - 4].Trim() + "," +
                        resultMessages[resultMessages.Length - 3].Trim() + "," +
                        resultMessages[resultMessages.Length - 2].Trim());
            }
            else if (resultMessages.Length == 5 && resultMessages[resultMessages.Length - 1].Equals("OK"))
            {
                return (resultMessages[resultMessages.Length - 5].Trim() + "," +
                          resultMessages[resultMessages.Length - 4].Trim() + "," +
                          resultMessages[resultMessages.Length - 3].Trim() + "," +
                          resultMessages[resultMessages.Length - 2].Trim());
            }
            else if (resultMessages[resultMessages.Length - 1].Equals("OK"))
            {
                string tmpstring = "";
                for (int i = 0; i < resultMessages.Length - 1; i++)
                {
                    tmpstring = tmpstring + resultMessages[i];
                }
                return (tmpstring);
            }
            else
            {
                throw new FHNonProcedureSocketProcessingException();
            }
        }
        public string AutoMeasure(bool ocrEnable)
        {
            //OK\r,1\r,1\r,1\r
            //OK\r 
            //1\r
            //1\r
            //1\r
            //  Thread.Sleep(100);
            string[] resultMessages;
           
                resultMessages = SendCommand("AutoMeasure\r", 5);//OK,1,1,1,1
            
           

            //string[] resultMessages = SendCommand("AutoMeasure\r");//OK,1,1,1

            if (resultMessages.Length == 1 && resultMessages[0].Equals("OK"))
            {
                return null;
            }
            else if (resultMessages.Length == 4 && resultMessages[resultMessages.Length - 1].Equals("OK"))
            {
                return (resultMessages[resultMessages.Length - 4].Trim() + "," +
                        resultMessages[resultMessages.Length - 3].Trim() + "," +
                        resultMessages[resultMessages.Length - 2].Trim());
            }
            else if (resultMessages.Length == 5 && resultMessages[resultMessages.Length - 1].Equals("OK"))
            {
                return (resultMessages[resultMessages.Length - 5].Trim() + "," +
                          resultMessages[resultMessages.Length - 4].Trim() + "," +
                          resultMessages[resultMessages.Length - 3].Trim() + "," +
                          resultMessages[resultMessages.Length - 2].Trim());
            }
            else if (resultMessages[resultMessages.Length - 1].Equals("OK"))
            {
                string tmpstring = "";
                for (int i = 0; i < resultMessages.Length - 1; i++)
                {
                    tmpstring = tmpstring + resultMessages[i];
                }
                return (tmpstring);
            }
            else
            {
                throw new FHNonProcedureSocketProcessingException();
            }
        }
        public string BMPClear()
        {
            //OK\r,1\r,1\r,1\r
            //OK\r 
            //1\r
            //1\r
            //1\r
            string[] resultMessages = SendCommand("CLEARBMP\r");//OK,1,1,1

            //string[] resultMessages = SendCommand("AutoMeasure\r");//OK,1,1,1

            if (resultMessages[resultMessages.Length - 1].Equals("OK"))
            {
                return "-1";
            }
            else
            {
                throw new FHNonProcedureSocketProcessingException();
            }
        }
        public string AutoMeasure2(string lbname)
        {
            //OK\r,1\r,1\r,1\r
            //OK\r 
            //1\r
            //1\r
            //1\r
            //  Thread.Sleep(100);
            string[] resultMessages;

            resultMessages = SendCommand("AutoMeasure2 "+ lbname +"\r", 5);//OK,1,1,1,1



            //string[] resultMessages = SendCommand("AutoMeasure\r");//OK,1,1,1

            if (resultMessages.Length == 1 && resultMessages[0].Equals("OK"))
            {
                return null;
            }
            else if (resultMessages.Length == 4 && resultMessages[resultMessages.Length - 1].Equals("OK"))
            {
                return (resultMessages[resultMessages.Length - 4].Trim() + "," +
                        resultMessages[resultMessages.Length - 3].Trim() + "," +
                        resultMessages[resultMessages.Length - 2].Trim());
            }
            else if (resultMessages.Length == 5 && resultMessages[resultMessages.Length - 1].Equals("OK"))
            {
                return (resultMessages[resultMessages.Length - 5].Trim() + "," +
                        resultMessages[resultMessages.Length - 4].Trim() + "," +
                        resultMessages[resultMessages.Length - 3].Trim() + "," +
                        resultMessages[resultMessages.Length - 2].Trim());
            }
            else if (resultMessages[resultMessages.Length - 1].Equals("OK"))
            {
                string tmpstring = "";
                for (int i = 0; i < resultMessages.Length - 1; i++)
                {
                    tmpstring = tmpstring + resultMessages[i];
                }
                return (tmpstring);
            }
            else
            {
                throw new FHNonProcedureSocketProcessingException();
            }
        }



        public void TeachImage3(string filePath)
        {
            string[] resultMessages = SendCommand(string.Format("TEACH3 {0}\r", filePath));
            if (resultMessages[resultMessages.Length - 1].Equals("OK"))
            {
                return;
            }
            else
            {
                throw new FHNonProcedureSocketProcessingException();
            }
        }
        public string Prefix_Set(String Prefix)
        {
            string[] resultMessages = SendCommand("ILH " + Prefix + "\r");
            if (resultMessages.Length == 1 && resultMessages[0].Equals("OK"))
            {
                return null;
            }
            else if (resultMessages.Length > 1 && resultMessages[resultMessages.Length - 1].Equals("OK"))
            {
                return resultMessages[resultMessages.Length - 2];
            }
            else
            {
                throw new FHNonProcedureSocketProcessingException();
            }
        }

        public string Measure_Once()
        {
            string[] resultMessages = SendCommand("M\r");
            if (resultMessages.Length == 1 && resultMessages[0].Equals("OK"))
            {
                return null;
            }
            else if (resultMessages.Length > 1 && resultMessages[resultMessages.Length - 1].Equals("OK"))
            {
                return resultMessages[resultMessages.Length - 2];
            }
            else
            {
                throw new FHNonProcedureSocketProcessingException();
            }
        }
        public void RegImage_Register(int registeredImageNumber, int sourceToRegister, string imageFileName)
        {
            string[] resultMessages = SendCommand(string.Format("RID {0:D} {1:D} {2}\r", registeredImageNumber, sourceToRegister, imageFileName));
            if (resultMessages[resultMessages.Length - 1].Equals("OK"))
            {
                return;
            }
            else
            {
                throw new FHNonProcedureSocketProcessingException();
            }
        }
        public void RegImage_Load(int registeredImageNumber)
        {
            string[] resultMessages = SendCommand(string.Format("RID {0:D}\r", registeredImageNumber));
            if (resultMessages[resultMessages.Length - 1].Equals("OK"))
            {
                return;
            }
            else
            {
                throw new FHNonProcedureSocketProcessingException();
            }
        }
        public bool CalibrateImage(string filePath)
        {
            string[] resultMessages = SendCommand(string.Format("CALIB {0}\r", filePath));
            if (resultMessages[resultMessages.Length - 1].Equals("OK"))
            {
                return true;
            }
            else
            {
                return false;
                throw new FHNonProcedureSocketProcessingException();
            }
        }
        public void TeachImage(string filePath)
        {
            string[] resultMessages = SendCommand(string.Format("TEACH {0}\r", filePath));
            if (resultMessages[resultMessages.Length - 1].Equals("OK"))
            {
                return;
            }
            else
            {
                throw new FHNonProcedureSocketProcessingException();
            }
        }
        public void Scene_Switch(int sceneNumber)
        {
            string[] resultMessages = SendCommand(string.Format("S {0:D}\r", sceneNumber));
            if (resultMessages[resultMessages.Length - 1].Equals("OK"))
            {
                return;
            }
            else
            {
                throw new FHNonProcedureSocketProcessingException();
            }
        }
        public void SAVEBMP()
        {
            string[] resultMessages = SendCommand(string.Format("SAVEBMP\r"));
            if (resultMessages[resultMessages.Length - 1].Equals("OK"))
            {
                return;
            }
            else
            {
                throw new FHNonProcedureSocketProcessingException();
            }
        }
        public void MultiData_Change(string settingData)
        {
            string[] resultMessages = SendCommand(string.Format("MULTIDATA {0}\r", settingData));
            if (resultMessages[resultMessages.Length - 1].Equals("OK"))
            {
                return;
            }
            else
            {
                throw new FHNonProcedureSocketProcessingException();
            }
        }
        public void UnitData_Change(int processingUnitNumber, int externalReferenceTableNumber, string settingData)
        {
            string[] resultMessages = SendCommand(string.Format("UNITDATA {0:D} {1:D} {2}\r", processingUnitNumber, externalReferenceTableNumber, settingData));
            if (resultMessages[resultMessages.Length - 1].Equals("OK"))
            {
                return;
            }
            else
            {
                throw new FHNonProcedureSocketProcessingException();
            }
        }
        public string UnitData_Get(int processingUnitNumber, int externalReferenceTableNumber)
        {
            string[] resultMessages = SendCommand(string.Format("UNITDATA {0:D} {1:D}\r", processingUnitNumber, externalReferenceTableNumber));
            if (resultMessages.Length == 1 && resultMessages[0].Equals("OK"))
            {
                return null;
            }
            else if (resultMessages.Length > 1 && resultMessages[resultMessages.Length - 1].Equals("OK"))
            {
                return resultMessages[resultMessages.Length - 2];
            }
            else
            {
                throw new FHNonProcedureSocketProcessingException();
            }
        }
    }
}
