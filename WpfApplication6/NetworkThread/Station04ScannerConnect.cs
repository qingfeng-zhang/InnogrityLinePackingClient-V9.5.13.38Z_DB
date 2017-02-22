using System;
using System.IO.Ports;
using System.Xml;
using IGTwpf;
namespace InnogrityLinePackingClient {
  partial class NetworkThread {
    public void Station04ScannerConnect(MainNetworkClass networkmain) {
      if(OP3CognexScanner != null)//change this to comport
            {
        try {
          OP3CognexScanner.Close();
        } catch(Exception ex) { log.Error("Station 4 scanner unable to close exception " + ex.ToString()); }
        //  OP2CognexScanner = null;
      }
      XmlDocument doc = new XmlDocument();
      doc.Load(@"Config.xml");
      XmlNode Scannernode = doc.SelectSingleNode(@"/CONFIG/SCANNER3/PORT");
      String comport = Scannernode.InnerText;
      XmlNode tryingtime = doc.SelectSingleNode(@"/CONFIG/SCANNER3/TRYINGTIME");
      X = int.Parse(tryingtime.InnerText);
      try {
        if(OP3CognexScanner == null) {
          OP3CognexScanner = new SerialPort(comport);
          OP3CognexScanner.BaudRate = 9600;
          OP3CognexScanner.Parity = Parity.None;
          OP3CognexScanner.StopBits = StopBits.One;
          OP3CognexScanner.DataBits = 8;
          networkmain.stn4log = "Scanner3 Open:" + comport;
        }
        OP3CognexScanner.Open();
      } catch(Exception ex) {
        log.Error("Station 4 scanner exception " + ex.ToString());
        //OP2CognexScanner = null;
      }
    }
  }
}
