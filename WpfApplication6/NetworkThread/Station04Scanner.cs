using NLog;
using System;
using System.Text;
using System.Threading;
using IGTwpf;
namespace InnogrityLinePackingClient
{
    partial class NetworkThread
    {
        //public Logger Log3 = LogManager.GetLogger("Station04Scanner");
        public void Station04Scanner(object msgobj)
        {
            MainNetworkClass networkmain = (MainNetworkClass)msgobj;
            #region  Station 4 Hand Scan and Data send to PLC
            //Log3.Info("Thread Start");
            ScanboxidSt4barcode = "\0\0\0\0\0\0\0\0\0\0";
            while (!bTerminate)
            {
                Thread.Sleep(100);

              //  Log3.Info("Thread Loop Start");
              //  Log3.Info("Waiting PLC to send 'Read' Signal");
                if ((ScanboxidSt4barcode == "\0\0\0\0\0\0\0\0\0\0") && PLCQueryRx[PLCQueryRx_DM177] == 0x00)
                {
                    int cnt = 0;
                    string barcode = null;
                    try
                    {
                        Station04ScannerConnect(networkmain);
                        while ((cnt < 100) || (barcode == null) || barcode == "\r" || barcode == "")
                        {
                            try
                            {
                                OP3CognexScanner.ReadTimeout = 100;
                                barcode = OP3CognexScanner.ReadLine();
                                // if barcode avaliable break
                                Thread.Sleep(10);
                                //if(barcode == "\r")
                                //{
                                //    continue;

                                //}
                                if (barcode != null && barcode != "\r" && barcode != "")
                                {
                                    //PLCWriteCommand[PLCWriteCommand_DM399] = 0x02;
                                   // Log3.Info("Station04Scanner Read: " + barcode);
                                    ScanboxidSt4barcode = barcode.Trim();
                                    break;
                                }
                            }
                            catch (Exception)
                            {
                                throw;
                            }
                            cnt++;
                        }
                        if (cnt > 100)
                        {
                            throw new TimeoutException();
                        }
                    }
                    catch (TimeoutException ex)
                    {
                        //Log3.Error("Timeout EX");
                        barcode = null;
                        continue;
                    }
                    catch (Exception ex)
                    {
                       // Log3.Error("EX");
                        barcode = null;
                        continue;
                    }
                }
                else
                {
                    continue;
                }
                while (!bTerminate)
                {
                    Thread.Sleep(100);
                    //Log3.Info("Trying to write barcode into PLC.");
                    // Write barcode into PLC
                    if ((ScanboxidSt4barcode != "\0\0\0\0\0\0\0\0\0\0") && PLCQueryRx[PLCQueryRx_DM177] == 0x00 && ScanboxidSt4barcode != null && ScanboxidSt4barcode != "")
                    {
                        string tmpstr;
                        byte[] tmpbyte;
                        tmpstr = ScanboxidSt4barcode;
                        tmpbyte = new byte[tmpstr.Length];
                        tmpbyte = Encoding.ASCII.GetBytes(tmpstr);
                        Array.Copy(tmpbyte, 0, PLCWriteCommand, XOFFSETForst4handscanbarcode, tmpstr.Length);//335
                                                                                                             // PLCWriteCommand[PLCWriteCommand_DM399] = 0x08;
                       // Log3.Info("Write barcode into PLC successful");
                        break;
                    }
                    else
                    {
                        continue;
                        // break;

                    }

                }// second loop

                while (!bTerminate)
                {
                    Thread.Sleep(100);
                    //Log3.Info("Waiting PLC to send 'Clear barcode' signal.");
                    if (PLCQueryRx[PLCQueryRx_DM177] == 0x8)
                    { //177
                        ScanboxidSt4barcode = "\0\0\0\0\0\0\0\0\0\0";
                        CheckstringClearForstation4barcodeData(XOFFSETForst4handscanbarcode, ScanboxidSt4barcode);
                        break;
                    }
                    else
                    {
                        continue;
                        // break;

                    }
                }//third

            } //while Big loop end 
            #endregion
            //Log3.Info("Thread Exit");
        }
    }
}
