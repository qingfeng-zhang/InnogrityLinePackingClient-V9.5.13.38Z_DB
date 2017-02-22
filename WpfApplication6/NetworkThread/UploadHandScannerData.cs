using System;
using System.Text;
using System.Threading;
using System.Xml;
using IGTwpf;
using NLog;
namespace InnogrityLinePackingClient {
  partial class NetworkThread {
    public const int XOFFSETForst4handscanbarcode1 = 311; //345
    public const int XOFFSETForst4handscanbarcode2 = 387; //383
    public const int XOFFSETForst4handscanbarcode3 = 397; //388
    public const int XOFFSETForst4handscanbarcode4 = 407; //393
    public const int XOFFSETForst4handscanbarcode5 = 361; //370
    public const int XOFFSETForst4handscanbarcode6 = 371; //375
    //public Logger Log = LogManager.GetLogger("UploadhandScannerData");
    public void UploadhandScannerData(object msgobj) {
      //Log.Info("Thread Start");
      MainNetworkClass networkmain = (MainNetworkClass)msgobj;
      while(!bTerminate) {
       // Log.Info("Thread Loop");
        Thread.Sleep(100);
        try {
          string barcode1;
          XmlDocument doc = new XmlDocument();
          doc.Load(@"Config.xml");
          XmlNode node = doc.SelectSingleNode(@"/CONFIG/MSTBAGMATRIX/L-01");
          barcode1 = node.InnerText;
          string tmpstr;
          byte[] tmpbyte;
          tmpstr = barcode1;
          tmpbyte = new byte[tmpstr.Length];
          tmpbyte = Encoding.ASCII.GetBytes(tmpstr);
          Array.Copy(tmpbyte,0,PLCWriteCommand,XOFFSETForst4handscanbarcode1,tmpstr.Length);//D345=311
          node = doc.SelectSingleNode(@"/CONFIG/MSTBAGMATRIX/L-02");
          barcode1 = node.InnerText;
          tmpstr = barcode1;
          tmpbyte = new byte[tmpstr.Length];
          tmpbyte = Encoding.ASCII.GetBytes(tmpstr);
          Array.Copy(tmpbyte,0,PLCWriteCommand,XOFFSETForst4handscanbarcode2,tmpstr.Length);//D383=387
          node = doc.SelectSingleNode(@"/CONFIG/MSTBAGMATRIX/L-03");
          barcode1 = node.InnerText;
          tmpstr = barcode1;
          tmpbyte = new byte[tmpstr.Length];
          tmpbyte = Encoding.ASCII.GetBytes(tmpstr);
          Array.Copy(tmpbyte,0,PLCWriteCommand,XOFFSETForst4handscanbarcode3,tmpstr.Length);//D388=397
          node = doc.SelectSingleNode(@"/CONFIG/MSTBAGMATRIX/R-01");
          barcode1 = node.InnerText;
          tmpstr = barcode1;
          tmpbyte = new byte[tmpstr.Length];
          tmpbyte = Encoding.ASCII.GetBytes(tmpstr);
          Array.Copy(tmpbyte,0,PLCWriteCommand,XOFFSETForst4handscanbarcode4,tmpstr.Length);//D393=407
          node = doc.SelectSingleNode(@"/CONFIG/MSTBAGMATRIX/R-02");
          barcode1 = node.InnerText;
          tmpstr = barcode1;
          tmpbyte = new byte[tmpstr.Length];
          tmpbyte = Encoding.ASCII.GetBytes(tmpstr);
          Array.Copy(tmpbyte, 0, PLCWriteCommand, XOFFSETForst4handscanbarcode5, tmpstr.Length);//D370=361
          node = doc.SelectSingleNode(@"/CONFIG/MSTBAGMATRIX/R-03");
          barcode1 = node.InnerText;
          tmpstr = barcode1;
          tmpbyte = new byte[tmpstr.Length];
          tmpbyte = Encoding.ASCII.GetBytes(tmpstr);
          Array.Copy(tmpbyte,0,PLCWriteCommand,XOFFSETForst4handscanbarcode6,tmpstr.Length);//D375=371
        } catch { }
        break;
      }
      //Log.Info("Thread Exit");
    }
  }
}
