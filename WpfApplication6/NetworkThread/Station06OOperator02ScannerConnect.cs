using System;
using System.IO.Ports;
using System.Xml;
using IGTwpf;
namespace InnogrityLinePackingClient {
  partial class NetworkThread {
    public void Station06OOperator02ScannerConnect(MainNetworkClass networkmain) {
      if(OP2CognexScanner != null)//change this to comport
            {
        try {
          OP2CognexScanner.Close();
        } catch(Exception ex) { log.Error("Operator2 scanner unable to close exception " + ex.ToString()); }
        //  OP2CognexScanner = null;
      }
      XmlDocument doc = new XmlDocument();
      doc.Load(@"Config.xml");
      XmlNode Scannernode = doc.SelectSingleNode(@"/CONFIG/SCANNER2/PORT");
      String comport = Scannernode.InnerText;
      XmlNode tryingtime = doc.SelectSingleNode(@"/CONFIG/SCANNER2/TRYINGTIME");
      X = int.Parse(tryingtime.InnerText);
      try {
        if(OP2CognexScanner == null) {
          OP2CognexScanner = new SerialPort(comport);
          OP2CognexScanner.BaudRate = 9600;
          OP2CognexScanner.Parity = Parity.None;
          OP2CognexScanner.StopBits = StopBits.One;
          OP2CognexScanner.DataBits = 8;
        }
        OP2CognexScanner.Open();
      } catch(Exception ex) {
        log.Error("Operator2 scanner exception " + ex.ToString());
        networkmain.linePack.Error("Operator2 scanner exception " + ex.ToString());
        //OP2CognexScanner = null;
      }
    }
  }
}
