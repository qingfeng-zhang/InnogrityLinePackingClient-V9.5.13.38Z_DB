using NLog;
using System;
using System.IO.Ports;
using System.Text;
using System.Threading;
namespace InnovacVacuumSealerPackage {
  public class InnovacVacuumSealer:IDisposable {
    private short _SelectedProgramNumber = -1;
    private SealerBar _SelectedSealerBar = SealerBar.Undefined;
    private Logger Log;
    private byte[] receiveMessageBuffer = new byte[1024];
    // longest message possible now is 28 bytes.
    private int receiveMessageBufferTail = 0;
    public InnovacVacuumSealer(string comAddress) {
      COMAddress = comAddress;
      Log = LogManager.GetLogger(Name);
    }

    public delegate void AckNakReceived(Message message);
    public delegate void ChamberClosedReceived(ChamberClosedMessage message);
    public delegate void SealingCompleteReceived(SealingCompleteMessage message);
    public delegate void SealingErrorReceived(SealingErrorMessage message);
    public delegate void SelectProgramConfirmReceived(SelectProgramConfirmMessage message);
    public delegate void SystemStatusReceived(SystemStatusMessage message);
    public event AckNakReceived AckNakEvent = delegate { };
    public event ChamberClosedReceived ChamberClosedEvent = delegate { };
    public event SealingCompleteReceived SealingCompleteEvent = delegate { };
    public event SealingErrorReceived SealingErrorEvent = delegate { };
    public event SelectProgramConfirmReceived SelectProgramConfirmEvent = delegate { };
    public event SystemStatusReceived SystemStatusEvent = delegate { };
    public enum DeviceCommand:byte {
      Undefined,
      SelectProgram = (byte)'P',
      ConfirmProgram = (byte)'S',
      RequestStatus = (byte)'G',
      Ack = 0x06,
      Nak = 0x15,
    }
    public enum DeviceEvent:byte {
      Undefined,
      SelectProgramConfirm = (byte)'P',
      SystemStatus = (byte)'G',
      SealingComplete = (byte)'C',
      SealingError = (byte)'E',
      ChamberClosed = (byte)'H',
      Ack = 0x06,
      Nak = 0x15,
    }
    public enum SealerBar:byte {
      Undefined,
      Left = (byte)'L',
      Right = (byte)'R',
      Both = (byte)'B',
    }
    [FlagsAttribute]
    public enum SealingFunction {
      None = 0,
      VT = 1,
      VS = 2,
    }
    public enum SystemStatus:byte {
      Undefined,
      Standby = (byte)'1',
      Ready = (byte)'2',
      Running = (byte)'3',
      Error = (byte)'4',
    }
    public string COMAddress { get; private set; }
    public Exception LastException { get; private set; }
    public string Name { get { return "InnovacVacuumSealer_" + COMAddress; } }
    public short SelectedProgramNumber {
      get {
        return _SelectedProgramNumber;
      }
      set {
        _SelectedProgramNumber = value;
      }
    }
    public SealerBar SelectedSealerBar {
      get {
        return _SelectedSealerBar;
      }
      set { _SelectedSealerBar = value; }
    }
    protected SerialPort Port { get; private set; }
    public bool IsOpen {
      get { if(Port != null) { return Port.IsOpen; } return false; }
    }
    public void Close() {
     // Log.Info("Close:{0}",Name);
      try {
        Port.Close();
      } catch(Exception ex) {
        LastException = ex;
        // Assume Close always successful, as the com port will be disposed anyway.
        //throw;
      } finally {
        try {
          Port.Dispose();
        } catch(Exception) { }
        Port = null;
      }
    }
    public void Dispose() {
      try {
        if(Port != null) {
          Port.Dispose();
        }
      } catch(Exception ex) {
        LastException = ex;
        throw;
      }
    }
    public void GetSystemStatus(out SystemStatus status) {
    //  Log.Info("GetSystemStatus");
      Message message = new RequestStatusMessage();
      ManualResetEvent waitHandle = new ManualResetEvent(false);
      SystemStatusMessage response = null;
      SystemStatusReceived action = delegate(SystemStatusMessage msg) {
        response = msg;
        waitHandle.Set();
      };
      try {
        SystemStatusEvent += action;
        for(int i = 0;i < 5;i++) {
          SendMessage(message);
          if(waitHandle.WaitOne(5000)) // change here
                    {
            waitHandle.Reset();
            if(response != null) {
              status = response.Status;
            //  Log.Info("GetSystemStatus:{0}",status);
              return;
            }
          }
        }
       // Log.Warn("GetSystemStatus;Timeout");
        throw new TimeoutException();
      } catch(Exception ex) {
       // Log.Error("GetSystemStatus;Exception:{0}",ex.ToString());
        LastException = ex;
        throw;
      } finally {
        SystemStatusEvent -= action;
      }
    }
    public void Open() {
    //  Log.Info("Open:{0}",Name);
      try {
        if(Port == null) {
          Port = new SerialPort(COMAddress);
          Port.BaudRate = 9600;
          Port.Parity = Parity.None;
          Port.StopBits = StopBits.One;
          Port.DataBits = 8;
          Port.DataReceived += Port_DataReceived;
        }
        Port.Open();
       // Log.Info("Open:{0};Success",Name);
      } catch(Exception ex) {
      //  Log.Error("Open:{0};Exception:{1}",Name,ex.ToString());
        LastException = ex;
        throw;
      }
    }
    public void SelectAndConfirmProgram() {
      SelectAndConfirmProgram(_SelectedProgramNumber,_SelectedSealerBar);
    }
    public void SelectAndConfirmProgram(short programNumber,SealerBar selectedSealerBar) {
     // Log.Info("SelectProgram;ProgramNumber:{0:D};SelectedSealerBar:{1}",programNumber,selectedSealerBar.ToString());
      SelectedProgramNumber = programNumber;
      SelectedSealerBar = selectedSealerBar;
      Message message = new SelectProgramMessage() { ProgramNumber = programNumber,SelectedSealerBar = selectedSealerBar };
      ManualResetEvent waitHandle = new ManualResetEvent(false);
      SelectProgramConfirmMessage response = null;
      SelectProgramConfirmReceived action = delegate(SelectProgramConfirmMessage msg) {
        response = msg;
        waitHandle.Set();
      };
      try {
        SelectProgramConfirmEvent += action;
        for(int i = 0;i < 10;i++) {
          SendMessage(message);
          // Select program requires long timeout.
          if(waitHandle.WaitOne(10000)) {
            waitHandle.Reset();
            if(response != null && response.ProgramNumber.Equals(programNumber) && response.SelectedSealerBar.Equals(selectedSealerBar)) {
             // Log.Info("SelectProgram;ProgramNumber:{0:D};SelectedSealerBar:{1};Success",programNumber,selectedSealerBar.ToString());
              // Immediately Confirm Program.
              Thread.Sleep(100);
              ConfirmProgram(programNumber,selectedSealerBar);
              return;
            }
            Thread.Sleep(800);
          }
          else { //no reply
              Log.Info("Sealer No Reply program number!!");
          }
        }
       // Log.Warn("SelectProgram;ProgramNumber:{0:D};SelectedSealerBar:{1};Timeout",programNumber,selectedSealerBar.ToString());
        throw new TimeoutException();
      } catch(Exception ex) {
      //  Log.Error("SelectProgram;ProgramNumber:{0:D};SelectedSealerBar:{1};Exception:{2}",programNumber,selectedSealerBar.ToString(),ex.ToString());
        LastException = ex;
        throw;
      } finally {
        SelectProgramConfirmEvent -= action;
      }
    }
    public bool WaitChamberClosed(int timeout = Timeout.Infinite) {
    //  Log.Info("WaitChamberClosed");
      ManualResetEvent waitHandle = new ManualResetEvent(false);
      ChamberClosedReceived action = delegate(ChamberClosedMessage msg) {
        waitHandle.Set();
      };
      try {
        ChamberClosedEvent += action;
        if(waitHandle.WaitOne(timeout)) {
          waitHandle.Reset();
        //  Log.Info("WaitChamberClosed;Success");
          return true;
        }
      //  Log.Warn("WaitChamberClosed;Timeout");
        return false;
      } catch(Exception ex) {
      //  Log.Error("WaitChamberClosed;Exception:{0}",ex.ToString());
        LastException = ex;
        throw;
      } finally {
        ChamberClosedEvent -= action;
      }
    }
        public bool IsSeallerError()
        {
            SealingErrorMessage errorResponse = null;
            SealingErrorReceived errorAction = delegate (SealingErrorMessage msg) {
                errorResponse = msg;
            };
            try
            {
                if (errorResponse != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
         
        }
    public bool WaitSealingCompleted(out SealingCompleteMessage sealingResult,int timeout = Timeout.Infinite) {
   //   Log.Info("WaitSealingCompleted");
      ManualResetEvent[] waitHandle = new ManualResetEvent[] { new ManualResetEvent(false),new ManualResetEvent(false) };
      SealingCompleteMessage completeResponse = null;
      SealingErrorMessage errorResponse = null;
      SealingCompleteReceived completeAction = delegate(SealingCompleteMessage msg) {
        completeResponse = msg;
        waitHandle[0].Set();
      };
      SealingErrorReceived errorAction = delegate(SealingErrorMessage msg) {
        errorResponse = msg;
        waitHandle[1].Set();
      };
      try {
        SealingCompleteEvent += completeAction;
        SealingErrorEvent += errorAction;
        int result = EventWaitHandle.WaitAny(waitHandle,timeout);
        if(result == 0) {
          sealingResult = completeResponse;
          Log.Info("WaitSealingComplete;{0}",completeResponse);
          return true;
        } else if(result == 1) {
         Log.Warn("WaitSealingComplete;{0}",errorResponse);
          throw new SealingErrorException(errorResponse.ErrorCode,errorResponse.ProgramNumber,errorResponse.SelectedSealerBar);
        } else {
          sealingResult = null;
          Log.Warn("WaitSealingComplete;Timeout");
          return false;
        }
      } catch(Exception ex) {
        Log.Error("WaitSealingComplete;Exception:{0}",ex.ToString());
        LastException = ex;
        throw;
      } finally {
        SealingCompleteEvent -= completeAction;
        SealingErrorEvent -= errorAction;
      }
    }
    protected static byte[] Byte2DArrayToByteArray(byte[][] inputArray) {
      int length = 0;
      if(inputArray != null) {
        for(int i = 0;i < inputArray.Length;i++) {
          if(inputArray[i] != null) {
            length += inputArray[i].Length;
          }
        }
      } else {
        return new byte[] { };
      }
      if(length == 0) {
        return new byte[] { };
      }
      byte[] result = new byte[length];
      int position = 0;
      for(int i = 0;i < inputArray.Length;i++) {
        if(inputArray[i] != null) {
          inputArray[i].CopyTo(result,position);
          position += inputArray[i].Length;
        }
      }
      return result;
    }
    protected static SealingFunction SealingFunctionFromMessageByteArray(byte[] input) {
      switch(input[0]) {
        case (byte)'V':
          switch(input[1]) {
            case (byte)'S':
              return SealingFunction.VS;
            case (byte)'T':
              return SealingFunction.VT;
            default:
              return SealingFunction.None;
          }
        default:
          return SealingFunction.None;
      }
    }
    protected static byte[] SealingFunctionToMessageByteArray(InnovacVacuumSealer.SealingFunction input) {
      byte[][] result = new byte[2][];
      if(input == SealingFunction.VS) {
        result[0] = new byte[] { (byte)'V',(byte)'S' };
      }
      if(input == SealingFunction.VT) {
        result[1] = new byte[] { (byte)'V',(byte)'T' };
      }
      return Byte2DArrayToByteArray(result);
    }
    protected void ConfirmProgram(short programNumber,SealerBar selectedSealerBar) {
            Thread.Sleep(500);
            SendMessage(new InnovacVacuumSealer.DeviceCommandAckMessage() { EventCode = InnovacVacuumSealer.DeviceEvent.SelectProgramConfirm });
            Thread.Sleep(500);
            SelectedProgramNumber = programNumber;
            SelectedSealerBar = selectedSealerBar;
            Message message = new ConfirmProgramMessage() { ProgramNumber = programNumber, SelectedSealerBar = selectedSealerBar };
            ManualResetEvent waitHandle = new ManualResetEvent(false);
            Message response = null;
            AckNakReceived action = delegate (Message msg) {
                response = msg;
                waitHandle.Set();
            };
            try
            {
                AckNakEvent += action;
                for (int i = 0; i < 5; i++)
                {
                    Thread.Sleep(100);
                    SendMessage(message);
                    if (waitHandle.WaitOne(5000))
                    {
                        waitHandle.Reset();
                        if (response != null && response is DeviceEventAckMessage)
                        {
                            Log.Info("ConfirmProgram;ProgramNumber:{0:D};SelectedSealerBar:{1};Success", programNumber, selectedSealerBar.ToString());
                            return;
                        }
                    }
                    else
                    {
                        Log.Info("ConfirmProgram No Reply!!");
                    }
                }
                //    Log.Warn("ConfirmProgram;ProgramNumber:{0:D};SelectedSealerBar:{1};Timeout",programNumber,selectedSealerBar.ToString());
                throw new TimeoutException();
            }
            catch (Exception ex)
            {
                //    Log.Error("ConfirmProgram;ProgramNumber:{0:D};SelectedSealerBar:{1};Exception:{2}",programNumber,selectedSealerBar.ToString(),ex.ToString());
                LastException = ex;
                throw;
            }
            finally
            {
                AckNakEvent -= action;
            }
        }
    protected void Port_DataReceived(object sender,SerialDataReceivedEventArgs e) {
      try {
        while(true) {
          Message message = ReceiveMessage(0);
      //    Log.Info("MsgRcvd:{0}",message);
          switch((DeviceEvent)message.Command.Code) {
            case InnovacVacuumSealer.DeviceEvent.SelectProgramConfirm: {
                SendMessage(new InnovacVacuumSealer.DeviceCommandAckMessage() { EventCode = InnovacVacuumSealer.DeviceEvent.SelectProgramConfirm });
                SelectProgramConfirmEvent((SelectProgramConfirmMessage)message);
                break;
              }
            case InnovacVacuumSealer.DeviceEvent.ChamberClosed: {
                SendMessage(new InnovacVacuumSealer.DeviceCommandAckMessage() { EventCode = InnovacVacuumSealer.DeviceEvent.ChamberClosed });
                ChamberClosedEvent((ChamberClosedMessage)message);
                break;
              }
            case InnovacVacuumSealer.DeviceEvent.SealingComplete: {
                SendMessage(new InnovacVacuumSealer.DeviceCommandAckMessage() { EventCode = InnovacVacuumSealer.DeviceEvent.SealingComplete });
                SealingCompleteEvent((SealingCompleteMessage)message);
                break;
              }
            case InnovacVacuumSealer.DeviceEvent.SealingError: {
                SendMessage(new InnovacVacuumSealer.DeviceCommandAckMessage() { EventCode = InnovacVacuumSealer.DeviceEvent.SealingError });
                SealingErrorEvent((SealingErrorMessage)message);
                break;
              }
            case InnovacVacuumSealer.DeviceEvent.SystemStatus: {
                SystemStatusEvent((SystemStatusMessage)message);
                break;
              }
            case InnovacVacuumSealer.DeviceEvent.Ack:
            case InnovacVacuumSealer.DeviceEvent.Nak: {
                AckNakEvent(message);
                break;
              }
          }
        }
      } catch(TimeoutException ex) {
        // Ignore Timeout Exception.
      }
    }
    protected Message ReceiveMessage(int msTimeout = Timeout.Infinite) {
      try {
        lock(receiveMessageBuffer) {
          int startTime = Environment.TickCount;
          // Reading from port directly if buffer is empty. Else parse message from buffer.
          bool readBytesFromPort = receiveMessageBufferTail > 0 ? false : true;
          for(int timeLeft = msTimeout;
                (msTimeout == Timeout.Infinite) || (timeLeft >= 0);
                timeLeft = (msTimeout == Timeout.Infinite) ? Timeout.Infinite : (msTimeout - (Environment.TickCount - startTime))) {
            if(readBytesFromPort) {
              Port.ReadTimeout = timeLeft;
              int readLength = Port.Read(receiveMessageBuffer,receiveMessageBufferTail,receiveMessageBuffer.Length - receiveMessageBufferTail);
              receiveMessageBufferTail += readLength;
            }
            // Sample Cases: (S = STX, E = ETX, s = message content with value = STX, e = message content with value = ETX)
            // S....... (E not found) -> Continue reading.
            // ........ (S not found) -> Clear buffer, continue reading.
            // ...S.... -> Remove chars before S, continue reading.
            // S..ES..E -> Attempt first S..E part, successful (as valid message), remove S..E part from buffer. return message.
            // S..es..E -> Attempt first S..e part, unsuccessful, attempt S..es..E, successful, remove S..es..E part from buffer, return message.
            // s..Se..E -> Attempt the s..se part, unsuccessful, attempt s..Se..E, unsuccessful, attempt Se..E, successful, remove s..Se..E part from buffer, return message.
            // s.eseS.E -> Attempt the s.e part, unsuccessful, attempt s.ese part, unsuccessful, attempt s.eseS.E part, unsuccessful, attempt se part, unsuccessful, attempt seS.E part, unsuccessful, attempt S.E part, successful, remove s.eseS.E from buffer, return message.
            // Search for STX
            int firstSTX = -1;
            for(int i = 0;i < receiveMessageBufferTail;i++) {
              if(receiveMessageBuffer[i] == Message.StartByte) {
                if(firstSTX < 0) {
                  firstSTX = i;
                }
                // Search for ETX
                for(int j = i + 1;j < receiveMessageBufferTail;j++) {
                  if(receiveMessageBuffer[j] == Message.EndByte) {
                    // Attempt to decode message from i to j.
                    byte[] buffer = new byte[j - i + 1];
                    Array.Copy(receiveMessageBuffer,i,buffer,0,buffer.Length);
                    try {
                      Message result = Message.GetDeviceEventMessage(buffer);
                      // If decode successful, remove message from buffer.
                      if((receiveMessageBufferTail - j - 1) > 0) {
                        Array.Copy(receiveMessageBuffer,j + 1,receiveMessageBuffer,0,receiveMessageBufferTail - j - 1);
                        receiveMessageBufferTail = receiveMessageBufferTail - j - 1;
                      } else {
                        receiveMessageBufferTail = 0;
                      }
                      return result;
                    } catch(Exception) {
                      // Decode unsuccessful, try with the next ETX found.
                    }
                  }
                }
              }
            }
            // No more possible message part to attempt, need to wait next read bytes to be received.
            // Remove junks before the first STX found.
            if(firstSTX < 0) {
              // Remove all characters
              receiveMessageBufferTail = 0;
            } else if(firstSTX > 0) {
              // Remove junk characters
              Array.Copy(receiveMessageBuffer,firstSTX,receiveMessageBuffer,0,receiveMessageBufferTail - firstSTX);
              receiveMessageBufferTail = receiveMessageBufferTail - firstSTX;
            }
            readBytesFromPort = true;
          }
        }
      } catch(Exception ex) {
        LastException = ex;
        throw ex;
      }
      LastException = new TimeoutException();
      throw LastException;
    }
    protected void SendMessage(Message message,int msTimeout = Timeout.Infinite) {
      try {
        byte[] buffer = message.ToMessageByteArray(null);
        Port.Write(buffer,0,buffer.Length);
    //    Log.Info("MsgSent:{0}",message);
      } catch(Exception ex) {
        LastException = ex;
        throw ex;
      }
    }
    public class ChamberClosedMessage:SelectProgramMessage {
      public ChamberClosedMessage()
        : base(new MessageCode(DeviceEvent.ChamberClosed)) {
      }
      public ChamberClosedMessage(MessageCode code)
        : base(code) {
      }
      public ChamberClosedMessage(MessageCode code,byte[] message,out byte[] innerMessage)
        : base(code,message,out innerMessage) {
      }
    }
    public class ConfirmProgramMessage:SelectProgramMessage {
      public ConfirmProgramMessage()
        : base(new MessageCode(DeviceCommand.ConfirmProgram)) {
      }
      public ConfirmProgramMessage(MessageCode code)
        : base(code) {
      }
      public ConfirmProgramMessage(MessageCode code,byte[] message,out byte[] innerMessage)
        : base(code,message,out innerMessage) {
      }
    }
    public class DeviceCommandAckMessage:Message {
      public DeviceEvent EventCode;
      public DeviceCommandAckMessage()
        : base(new MessageCode(DeviceCommand.Ack)) {
      }
      public DeviceCommandAckMessage(MessageCode code)
        : base(code) {
      }
      public DeviceCommandAckMessage(MessageCode code,byte[] message,out byte[] innerMessage)
        : base(code) {
        EventCode = (DeviceEvent)message[0];
        if(message.Length > 1) {
          byte[] msg = new byte[message.Length - 1];
          Array.Copy(message,1,msg,0,msg.Length);
          innerMessage = msg;
        } else {
          innerMessage = new byte[] { };
        }
      }
      public override byte[] ToMessageByteArray(byte[] innerMessage) {
        return base.ToMessageByteArray(Byte2DArrayToByteArray(new byte[][] {
          new byte[]{(byte)EventCode},
          innerMessage
        }));
      }
      public override string ToString() {
        return string.Format("{0};EventCode={1}",base.ToString(),EventCode.ToString());
      }
    }
    public class DeviceCommandNakMessage:DeviceEventAckMessage {
      public DeviceCommandNakMessage()
        : base(new MessageCode(DeviceCommand.Nak)) {
      }
      public DeviceCommandNakMessage(MessageCode code)
        : base(code) {
      }
    }
    public class DeviceEventAckMessage:Message {
      public DeviceEventAckMessage()
        : base(new MessageCode(DeviceEvent.Ack)) {
      }
      public DeviceEventAckMessage(MessageCode code)
        : base(code) {
      }
    }
    public class DeviceEventNakMessage:Message {
      public DeviceEventNakMessage()
        : base(new MessageCode(DeviceEvent.Nak)) {
      }
      public DeviceEventNakMessage(MessageCode code)
        : base(code) {
      }
    }
    public abstract class Message {
      public const byte EndByte = 0x03;
      public const byte StartByte = 0x02;
      public MessageCode Command;
      private static byte[] MessageEnd = new byte[] { EndByte };
      private static byte[] MessageStart = new byte[] { StartByte };
      public Message(MessageCode code) {
        Command = code;
      }
      public static Message GetDeviceCommandMessage(byte[] message) {
        return GetMessageFromByteArray(message,false);
      }
      public static Message GetDeviceEventMessage(byte[] message) {
        return GetMessageFromByteArray(message,true);
      }
      public static Message GetMessageFromByteArray(byte[] message,bool isDeviceEvent = false) {
        if(message.Length < 3) {
          throw new ArgumentException("Message is too short.");
        }
        if(message[0] != StartByte) {
          throw new ArgumentException(string.Format("Message starts with invalid character 0x{0:X2}.",message[0]));
        }
        if(message[message.Length - 1] != EndByte) {
          throw new ArgumentException(string.Format("Message ends with invalid character 0x{0:X2}.",message[message.Length - 1]));
        }
        byte[] innerMessage;
        if(message.Length > 3) {
          innerMessage = new byte[message.Length - 3];
          Array.Copy(message,2,innerMessage,0,innerMessage.Length);
        } else {
          innerMessage = new byte[] { };
        }
        MessageCode code = new MessageCode(isDeviceEvent,message[1]);
        return code.GenerateNewMessage(innerMessage,out innerMessage);
      }
      public virtual byte[] ToMessageByteArray(byte[] innerMessage) {
        return Byte2DArrayToByteArray(new byte[][] {
          MessageStart,
          new byte[]{Command.ActualCode},
          innerMessage,
          MessageEnd
        });
      }
      public override string ToString() {
        return string.Format("{0};Command={1}",base.ToString(),Command.ToString());
      }
    }
    public class MessageCode {
      public MessageCode(bool isDeviceEvent,byte actualCode) {
        IsDeviceEvent = isDeviceEvent;
        ActualCode = actualCode;
      }
      public MessageCode(DeviceCommand command) {
        IsDeviceEvent = false;
        ActualCode = (byte)command;
      }
      public MessageCode(DeviceEvent command) {
        IsDeviceEvent = true;
        ActualCode = (byte)command;
      }
      public byte ActualCode { get; private set; }
      public object Code { get { return IsDeviceEvent ? (object)(DeviceEvent)ActualCode : (object)(DeviceCommand)ActualCode; } }
      public bool IsDeviceEvent { get; private set; }
      public Message GenerateNewMessage(byte[] message,out byte[] innerMessage) {
        innerMessage = null;
        if(IsDeviceEvent) {
          switch((InnovacVacuumSealer.DeviceEvent)Code) {
            case InnovacVacuumSealer.DeviceEvent.Ack:
              return new InnovacVacuumSealer.DeviceEventAckMessage(this);
            case InnovacVacuumSealer.DeviceEvent.ChamberClosed:
              return new InnovacVacuumSealer.ChamberClosedMessage(this,message,out innerMessage);
            case InnovacVacuumSealer.DeviceEvent.Nak:
              return new InnovacVacuumSealer.DeviceEventNakMessage(this);
            case InnovacVacuumSealer.DeviceEvent.SealingComplete:
              return new InnovacVacuumSealer.SealingCompleteMessage(this,message,out innerMessage);
            case InnovacVacuumSealer.DeviceEvent.SealingError:
              return new InnovacVacuumSealer.SealingErrorMessage(this,message,out innerMessage);
            case InnovacVacuumSealer.DeviceEvent.SelectProgramConfirm:
              return new InnovacVacuumSealer.SelectProgramConfirmMessage(this,message,out innerMessage);
            case InnovacVacuumSealer.DeviceEvent.SystemStatus:
              return new InnovacVacuumSealer.SystemStatusMessage(this,message,out innerMessage);
            default:
              throw new NotImplementedException();
          }
        } else {
          // Device command message
          switch((InnovacVacuumSealer.DeviceCommand)Code) {
            case InnovacVacuumSealer.DeviceCommand.Ack:
              return new InnovacVacuumSealer.DeviceCommandAckMessage(this);
            case InnovacVacuumSealer.DeviceCommand.ConfirmProgram:
              return new InnovacVacuumSealer.ConfirmProgramMessage(this,message,out innerMessage);
            case InnovacVacuumSealer.DeviceCommand.Nak:
              return new InnovacVacuumSealer.DeviceCommandNakMessage(this);
            case InnovacVacuumSealer.DeviceCommand.RequestStatus:
              return new InnovacVacuumSealer.RequestStatusMessage(this);
            case InnovacVacuumSealer.DeviceCommand.SelectProgram:
              return new InnovacVacuumSealer.SelectProgramMessage(this,message,out innerMessage);
            default:
              throw new NotImplementedException();
          }
        }
      }
      public override string ToString() {
        return string.Format("{0}:{1}",IsDeviceEvent ? "DeviceEvent" : "DeviceCommand",(char)ActualCode);
      }
    }
    public class RequestStatusMessage:Message {
      public RequestStatusMessage()
        : base(new MessageCode(DeviceCommand.RequestStatus)) {
      }
      public RequestStatusMessage(MessageCode code)
        : base(code) {
      }
    }
    public class SealingCompleteMessage:SelectProgramMessage {
      public short SealerCurrentPV;
      public short SealerCurrentSP;
      public float SealingTime;
      public SealingFunction UsedFunction;
      public short VacuumPV;
      public short VacuumSP;
      public SealingCompleteMessage()
        : base(new MessageCode(DeviceEvent.SealingComplete)) {
      }
      public SealingCompleteMessage(MessageCode code)
        : base(code) {
      }
      public SealingCompleteMessage(MessageCode code,byte[] message,out byte[] innerMessage)
        : base(code,message,out innerMessage) {
        VacuumPV = short.Parse(Encoding.ASCII.GetString(innerMessage,0,4));
        VacuumSP = short.Parse(Encoding.ASCII.GetString(innerMessage,4,4));
        SealerCurrentPV = short.Parse(Encoding.ASCII.GetString(innerMessage,8,4));
        SealerCurrentSP = short.Parse(Encoding.ASCII.GetString(innerMessage,12,4));
        SealingTime = (int.Parse(Encoding.ASCII.GetString(innerMessage,16,2)) / 10.0f);
        // Used function can be stackable
        for(int i = 18;i < innerMessage.Length;i += 2) {
          UsedFunction |= SealingFunctionFromMessageByteArray(new byte[] { innerMessage[i],innerMessage[i + 1] });
        }
        // Does not allow tailing messages
        innerMessage = new byte[] { };
      }
      public override byte[] ToMessageByteArray(byte[] innerMessage) {
        return base.ToMessageByteArray(Byte2DArrayToByteArray(new byte[][] {
          Encoding.ASCII.GetBytes(string.Format("{0:D4}",VacuumPV)),
          Encoding.ASCII.GetBytes(string.Format("{0:D4}",VacuumSP)),
          Encoding.ASCII.GetBytes(string.Format("{0:D4}",SealerCurrentPV)),
          Encoding.ASCII.GetBytes(string.Format("{0:D4}",SealerCurrentSP)),
          Encoding.ASCII.GetBytes(string.Format("{0:D2}",SealingTime*10)),
          SealingFunctionToMessageByteArray(UsedFunction),
          innerMessage
        }));
      }
      public override string ToString() {
        return string.Format("{0};VacuumPV={1:D};VacuumSP={2:D};SealerCurrentPV={3:D};SealerCurrentSP={4:D};SealingTime={5:F2}s;UsedFunction={6}",base.ToString(),VacuumPV,VacuumSP,SealerCurrentPV,SealerCurrentSP,SealingTime,UsedFunction.ToString());
      }
    }
    public class SealingErrorException:Exception {
      public short ErrorCode;
      public short ProgramNumber;
      public SealerBar SelectedSealerBar;
      public String ErrorMessage {
        get {
          switch(ErrorCode) {
            case 1:
              return "E01: Vacuum Leakage";
            case 2:
              return "E02: Seal Bar Short Circuit";
            case 3:
              return "E03: Seal Bar Open Circuit";
            case 4:
              return "E04: Oil Cycle Reach";
            case 5:
              return "E05: Vacuum Cannot Function";
            case 6:
              return "E06: Gas Flush Error";
            case 7:
              return "E07: Seal Contactor Abnormal";
            case 8:
              return "E08: Program/Recipe Empty";
            case 9:
              return "E09: Parameter Value Missing";
            case 10:
              return "E10: COM1 Hang. Pls Restart";
            case 11:
              return "E11: COM2 Hang. Pls Restart";
            case 12:
              return "E12: PC1 \"E\" CMD Repert Error";
            case 13:
              return "E13: PC1 \"P\" CMD Repert Error";
            case 14:
              return "E14: PC1 \"C\" CMD Repert Error";
            case 15:
              return "E15: PC1 Unknown Sealer Error";
            case 16:
              return "E16: PC2 \"E\" CMD Repert Error";
            case 17:
              return "E17: PC2 \"P\" CMD Repert Error";
            case 18:
              return "E18: PC2 \"C\" CMD Repert Error";
            case 19:
              return "E19: PC2 Unknown Sealer Error";
            default:
              return "E" + ErrorCode + ": Unknown Error";
          }
        }
      }
      public SealingErrorException(short errorCode,short programNumber,SealerBar selectedSealerBar) {
        ErrorCode = errorCode;
        ProgramNumber = programNumber;
        SelectedSealerBar = selectedSealerBar;
      }
    }
    public class SealingErrorMessage:SelectProgramMessage {
      public short ErrorCode;
      public SealingErrorMessage()
        : base(new MessageCode(DeviceEvent.SealingError)) {
      }
      public SealingErrorMessage(MessageCode code)
        : base(code) {
      }
      public SealingErrorMessage(MessageCode code,byte[] message,out byte[] innerMessage)
        : base(code,message,out innerMessage) {
        ErrorCode = short.Parse(Encoding.ASCII.GetString(innerMessage,0,2));
        if(innerMessage.Length > 3) {
          byte[] msg = new byte[innerMessage.Length - 3];
          Array.Copy(innerMessage,3,msg,0,msg.Length);
          innerMessage = msg;
        } else {
          innerMessage = new byte[] { };
        }
      }
      public override byte[] ToMessageByteArray(byte[] innerMessage) {
        return base.ToMessageByteArray(Byte2DArrayToByteArray(new byte[][] {
          Encoding.ASCII.GetBytes(string.Format("{0:D3}",ErrorCode)),
          innerMessage
        }));
      }
      public override string ToString() {
        return string.Format("{0};ErrorCode={1:D}",base.ToString(),ErrorCode);
      }
    }
    public class SelectProgramConfirmMessage:SelectProgramMessage {
      public SelectProgramConfirmMessage()
        : base(new MessageCode(DeviceEvent.SelectProgramConfirm)) {
      }
      public SelectProgramConfirmMessage(MessageCode code)
        : base(code) {
      }
      public SelectProgramConfirmMessage(MessageCode code,byte[] message,out byte[] innerMessage)
        : base(code,message,out innerMessage) {
      }
    }
    public class SelectProgramMessage:Message {
      public short ProgramNumber;
      public SealerBar SelectedSealerBar;
      public SelectProgramMessage()
        : base(new MessageCode(DeviceCommand.SelectProgram)) {
      }
      public SelectProgramMessage(MessageCode code)
        : base(code) {
      }
      public SelectProgramMessage(MessageCode code,byte[] message,out byte[] innerMessage) :
        base(code) {
        SelectedSealerBar = (SealerBar)message[0];
        ProgramNumber = short.Parse(Encoding.ASCII.GetString(message,1,4));
        if(message.Length > 5) {
          byte[] msg = new byte[message.Length - 5];
          Array.Copy(message,5,msg,0,msg.Length);
          innerMessage = msg;
        } else {
          innerMessage = new byte[] { };
        }
      }
      public override byte[] ToMessageByteArray(byte[] innerMessage) {
        return base.ToMessageByteArray(Byte2DArrayToByteArray(new byte[][] {
          new byte[]{(byte)SelectedSealerBar},
          //Encoding.ASCII.GetBytes(string.Format("{0:D3}",ProgramNumber)),
          Encoding.ASCII.GetBytes(string.Format("{0:D4}",ProgramNumber)),
          innerMessage
        }));
      }
      public override string ToString() {
        return string.Format("{0};SelectedSealerBar={1};ProgramNumber={2:D}",base.ToString(),SelectedSealerBar.ToString(),ProgramNumber);
      }
    }
    public class SystemStatusMessage:Message {
      public SystemStatus Status;
      public SystemStatusMessage()
        : base(new MessageCode(DeviceEvent.SystemStatus)) {
      }
      public SystemStatusMessage(MessageCode code)
        : base(code) {
      }
      public SystemStatusMessage(MessageCode code,byte[] message,out byte[] innerMessage)
        : base(code) {
        Status = (SystemStatus)message[0];
        if(message.Length > 1) {
          byte[] msg = new byte[message.Length - 1];
          Array.Copy(message,1,msg,0,msg.Length);
          innerMessage = msg;
        } else {
          innerMessage = new byte[] { };
        }
      }
      public override byte[] ToMessageByteArray(byte[] innerMessage) {
        return base.ToMessageByteArray(Byte2DArrayToByteArray(new byte[][] {
          new byte[]{(byte)Status},
          innerMessage
        }));
      }
      public override string ToString() {
        return string.Format("{0};Status={1}",base.ToString(),Status.ToString());
      }
    }
  }
}
