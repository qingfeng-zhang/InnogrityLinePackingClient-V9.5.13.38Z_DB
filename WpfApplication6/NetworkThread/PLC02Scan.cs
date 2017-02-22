using System;
using System.Threading;
using System.Xml;
using IGTwpf;
namespace InnogrityLinePackingClient
{
    partial class NetworkThread
    {
        public void RunPLC02Scan(object msgobj)
        {
            MainNetworkClass networkmain = (MainNetworkClass)msgobj;
            PLCTelnet2 = new TelnetClient();
            byte[] tmpRx6 = new byte[411];
            byte[] tmpRxPlc2 = new byte[1011];
            bool plc2Break = false;

            while (!bTerminate)
            {
                Thread.Sleep(100);
                try
                {
                    if (PLCTelnet2.connected == false)
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.Load(@"Config.xml");
                        XmlNode node = doc.SelectSingleNode(@"/CONFIG/PLCCONTROLLER2/ADD");
                        System.Net.IPAddress address = System.Net.IPAddress.Parse(node.InnerText);
                        node = doc.SelectSingleNode(@"/CONFIG/PLCCONTROLLER2/PORT");
                        PLC2NetworkAddress = "Trying to connect to " + address.ToString() + " Port : " + node.InnerText;
                        if (PLCTelnet2.ConnectToHost(address, int.Parse(node.InnerText)) == false)
                        {
                            PLC2NetworkAddress = "Connected to PLCServer2 "
                                                + address.ToString()
                                                + " Port : "
                                                + node.InnerText + " Fail";//not connected
                            Thread.Sleep(100);// try to connect to server
                            if (!plc2Break)
                            {
                                MyEventQ.AddQ("5;PLCCommunicationBreak;PLC Number;2");//Push message to stack
                                EvtLog.Info("5;PLCCommunicationBreak;PLC Number;2");
                                plc2Break = true;
                            }
                            continue;
                        }
                        else
                            PLC2NetworkAddress = "Connected to PLCServer2 " + address.ToString() + " Port : " + node.InnerText;//connected
                            plc2Break = false;
                    }
                    if (PLCTelnet2.connected)
                    {
                        string tmpstr;
                        int timeoutcounter = 0;
                        PLCTelnet2.SendDataToHost(PLCQueryCmd6);//PLC02 Read Cycle
                        Thread.Sleep(waitdelay);
                        tmpstr = PLCTelnet2.GetDataFromHost(ref tmpRx6, PLCQueryRx6.Length);
                        //tmpstr100 = PLCTelnet.GetDataFromHost(ref PLCQueryRx, PLCQueryRx.Length);
                        if (tmpstr != "No Data")
                        {
                            if ((tmpstr.StartsWith("d00000ffff")) && (tmpstr.Length == 822))
                            {
                                Array.Copy(tmpRx6, PLCQueryRx6, PLCQueryRx6.Length);
                            }
                            else
                                PLCTelnet2.Close();
                        }
                      //  log.Info(tmpstr);
                        while ((tmpstr == "No Data") && PLCTelnet.connected)
                        {
                            Thread.Sleep(waitdelay);
                            tmpstr = PLCTelnet2.GetDataFromHost(ref tmpRx6, PLCQueryRx6.Length);
                            if (tmpstr != "No Data")
                            {
                                if ((tmpstr.StartsWith("d00000ffff")) && (tmpstr.Length == 822))
                                {
                                    Array.Copy(tmpRx6, PLCQueryRx6, PLCQueryRx6.Length);
                                }
                                else
                                {
                                    PLCTelnet2.Close();
                                    break;
                                }
                            }
                            //No Data loop again
                          //  log.Info(tmpstr);
                            timeoutcounter++;
                            if (timeoutcounter == 10)
                            {
                                //assume connection broken
                                PLCTelnet2.Close();
                                break;
                            }
                        }


                        #region READ  PLC2 Memory 6100 to 6499



                        timeoutcounter = 0;
                        if (PLCTelnet2.connected == false) continue;
                        PLCTelnet2.SendDataToHost(PLCQueryCmdParaPlc2);//query data from plc dm700 to dm999
                        //tmpstr100 = PLCTelnet.GetDataFromHost(ref PLCQueryRx7, PLCQueryRx7.Length);
                        tmpstr = PLCTelnet2.GetDataFromHost(ref tmpRxPlc2, tmpRxPlc2.Length);
                        //tmpstr100 = PLCTelnet.GetDataFromHost(ref PLCQueryRx, PLCQueryRx.Length);
                        if (tmpstr.StartsWith("d00000ffff") && (tmpstr.Length == 2022))
                        {
                            Array.Copy(tmpRxPlc2, PLCQueryRxParaPlc2, PLCQueryRxParaPlc2.Length);
                         }
                        else
                            tmpstr = "No Data";

                        while ((tmpstr == "No Data") && PLCTelnet2.connected)
                        {
                            Thread.Sleep(waitdelay);
                            tmpstr = PLCTelnet2.GetDataFromHost(ref tmpRxPlc2, tmpRxPlc2.Length);
                            //tmpstr = PLCTelnet.GetDataFromHost(ref PLCQueryRx, PLCQueryRx.Length);
                            if ((tmpstr.StartsWith("d00000ffff") && (tmpstr.Length == 2022)))
                            {
                                Array.Copy(tmpRxPlc2, PLCQueryRxParaPlc2, PLCQueryRxParaPlc2.Length);
                            }
                            else
                                tmpstr = "No Data";

                            timeoutcounter++;
                            if (timeoutcounter == 10)
                            {
                                //assume connection broken
                                PLCTelnet2.Close();
                                break;
                            }
                        }




                        #endregion
                        













                        timeoutcounter = 0;
                        PLCTelnet2.SendDataToHost(PLCWriteCommand6);//PLC02 Write cycle
                        Thread.Sleep(waitdelay);
                        tmpstr = PLCTelnet2.GetDataFromHost(ref PLCWriteCommandRX6, PLCWriteCommandRX6.Length);
                     //   log.Info("PLC 2 Write " + tmpstr);
                        while ((tmpstr == "No Data") && PLCTelnet.connected)
                        {
                            Thread.Sleep(waitdelay);
                            tmpstr = PLCTelnet2.GetDataFromHost(ref PLCWriteCommandRX6, PLCWriteCommandRX6.Length);
                        //    log.Info("PLC 2 Write " + tmpstr);
                            timeoutcounter++;
                            if (timeoutcounter == 10)
                            {
                                //assume connection broken
                                PLCTelnet2.Close();
                                break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error(ex.ToString());
                }
            }
        }
    }
}

