using System;
using System.IO.Ports;
using System.Xml;
using IGTwpf;
namespace InnogrityLinePackingClient {
  partial class NetworkThread {
    public void Station06OOperator01ScannerConnect(MainNetworkClass networkmain) {
      if(OP1CognexScanner != null) {
        try {
          OP1CognexScanner.Close();
        } catch(Exception ex) { log.Error("Operator1 scanner unable to close exception " + ex.ToString()); }
        //  OP1CognexScanner = null;
      }
      XmlDocument doc = new XmlDocument();
      doc.Load(@"Config.xml");
      XmlNode Scannernode = doc.SelectSingleNode(@"/CONFIG/SCANNER1/PORT");
      String comport = Scannernode.InnerText;
      XmlNode tryingtime = doc.SelectSingleNode(@"/CONFIG/SCANNER1/TRYINGTIME");
      X = int.Parse(tryingtime.InnerText);
      try {
        if(OP1CognexScanner == null) {
          OP1CognexScanner = new SerialPort(comport);
          OP1CognexScanner.BaudRate = 9600;
          OP1CognexScanner.Parity = Parity.None;
          OP1CognexScanner.StopBits = StopBits.One;
          OP1CognexScanner.DataBits = 8;
        }
        OP1CognexScanner.Open();
      } catch(Exception ex) {
        //try
        //{
        //    OP1CognexScanner.Close();
        //}
        //catch (Exception) { }
         log.Error("Operator1 scanner exception " + ex.ToString());
         networkmain.linePack.Error("Operator1 scanner exception " + ex.ToString());
        // OP1CognexScanner = null;
      }

    }
  }
}
