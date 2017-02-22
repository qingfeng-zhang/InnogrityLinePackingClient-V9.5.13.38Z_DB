using System;
using System.Threading;
using System.Xml;
using IGTwpf;
namespace InnogrityLinePackingClient
{
    partial class NetworkThread
    {
        public void RunPLC01Scan(object msgobj)
        {
            MainNetworkClass networkmain = (MainNetworkClass)msgobj;
            byte[] tmpRx = new byte[411];
            byte[] tmpRx1 = new byte[611];
            byte[] tmpRx2 = new byte[811];
            bool plc1Break = false;
            PLCTelnet = new TelnetClient();
            //check for TCP network connection for PLC
            while (!bTerminate)
            {
                Thread.Sleep(100);
                try
                {
                    if (PLCTelnet.connected == false)
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.Load(@"Config.xml");
                        XmlNode node = doc.SelectSingleNode(@"/CONFIG/PLCCONTROLLER1/ADD");
                        System.Net.IPAddress address = System.Net.IPAddress.Parse(node.InnerText);
                        node = doc.SelectSingleNode(@"/CONFIG/PLCCONTROLLER1/PORT");
                        PLCNetworkAddress = "Trying to connect to " + address.ToString() + " Port : " + node.InnerText;
                        if (PLCTelnet.ConnectToHost(address, int.Parse(node.InnerText)) == false)
                        {
                            PLCNetworkAddress = "Connected to PLCServer1 "
                                                + address.ToString()
                                                + " Port : "
                                                + node.InnerText
                                                + " Fail";//not connected
                            Thread.Sleep(100);// try to connect to server
                            if (!plc1Break)
                            {
                                MyEventQ.AddQ("5;PLCCommunicationBreak;PLC Number;1");//Push message to stack
                                EvtLog.Info("5;PLCCommunicationBreak;PLC Number;1");
                                plc1Break = true;
                            }
                            continue;
                        }
                        else
                            PLCNetworkAddress = "Connected to PLCServer1 " + address.ToString() + " Port : " + node.InnerText;//connected
                            plc1Break = false;
                    }
                    //PLC Read Write Cycle
                    if (PLCTelnet.connected)
                    {
                        //get read data from PLC
                        PLCTelnet.SendDataToHost(PLCQueryCmd);//query data from plc D000 to D199
                        string tmpstr100;
                        tmpstr100 = PLCTelnet.GetDataFromHost(ref tmpRx, tmpRx.Length);
                        if (tmpstr100.StartsWith("d00000ffff") && tmpstr100.Length == 822)
                        {
                            Array.Copy(tmpRx, PLCQueryRx, PLCQueryRx.Length);
                        }
                        else
                        {
                            tmpstr100 = "No Data";
                        }
                       
                        int timeoutcounter = 0;
                        while ((tmpstr100 == "No Data") && PLCTelnet.connected)
                        {
                            Thread.Sleep(waitdelay);
                            //tmpstr100 = PLCTelnet.GetDataFromHost(ref PLCQueryRx, PLCQueryRx.Length);
                            tmpstr100 = PLCTelnet.GetDataFromHost(ref tmpRx, tmpRx.Length);
                            //tmpstr100 = PLCTelnet.GetDataFromHost(ref PLCQueryRx, PLCQueryRx.Length);
                            if (tmpstr100.StartsWith("d00000ffff") && (tmpstr100.Length == 822))
                            {
                                Array.Copy(tmpRx, PLCQueryRx, PLCQueryRx.Length);
                            }
                            else
                            {
                                tmpstr100 = "No Data";
                            }
                            //log.Info(tmpstr100);
                            timeoutcounter++;
                            if (timeoutcounter == 10)
                            {
                                //assume connection broken
                                PLCTelnet.Close();
                                break;
                            }
                        }

                        //Update 14/12/2015 May Pon
                        #region READ  PLC1 Memory 6100 to 6499



                        timeoutcounter = 0;
                        if (PLCTelnet.connected == false) continue;
                        PLCTelnet.SendDataToHost(PLCQueryCmdPara);//query data from plc dm700 to dm999
                        //tmpstr100 = PLCTelnet.GetDataFromHost(ref PLCQueryRx7, PLCQueryRx7.Length);
                        tmpstr100 = PLCTelnet.GetDataFromHost(ref tmpRx2, tmpRx2.Length);
                        //tmpstr100 = PLCTelnet.GetDataFromHost(ref PLCQueryRx, PLCQueryRx.Length);
                        if (tmpstr100.StartsWith("d00000ffff") && (tmpstr100.Length == 1622))
                        {
                            Array.Copy(tmpRx2, PLCQueryRxPara, PLCQueryRxPara.Length);
                        }
                        else
                            tmpstr100 = "No Data";
                       
                        while ((tmpstr100 == "No Data") && PLCTelnet.connected)
                        {
                            Thread.Sleep(waitdelay);
                            tmpstr100 = PLCTelnet.GetDataFromHost(ref tmpRx2, tmpRx2.Length);
                            //tmpstr100 = PLCTelnet.GetDataFromHost(ref PLCQueryRx, PLCQueryRx.Length);
                            if ((tmpstr100.StartsWith("d00000ffff") && (tmpstr100.Length == 1622)))
                            {
                                Array.Copy(tmpRx2, PLCQueryRxPara, PLCQueryRxPara.Length);
                            }
                            else
                                tmpstr100 = "No Data";
                          
                            timeoutcounter++;
                            if (timeoutcounter == 10)
                            {
                                //assume connection broken
                                PLCTelnet.Close();
                                break;
                            }
                        }




                        #endregion
                        

                        //Update 25/7/2015
                        #region   READ  PLC1 Memory 700 to 999

                        //timeoutcounter = 0;
                        //if (PLCTelnet.connected == false) continue;
                        //PLCTelnet.SendDataToHost(PLCQueryCmd7);//query data from plc dm700 to dm999
                        ////tmpstr100 = PLCTelnet.GetDataFromHost(ref PLCQueryRx7, PLCQueryRx7.Length);
                        //tmpstr100 = PLCTelnet.GetDataFromHost(ref tmpRx1, tmpRx1.Length);
                        ////tmpstr100 = PLCTelnet.GetDataFromHost(ref PLCQueryRx, PLCQueryRx.Length);
                        //if (tmpstr100.StartsWith("d00000ffff") && (tmpstr100.Length == 1222))
                        //{
                        //    Array.Copy(tmpRx1, PLCQueryRx7, PLCQueryRx7.Length);
                        //}
                        //else
                        //    tmpstr100 = "No Data";
                        //log.Info(tmpstr100);
                        //while ((tmpstr100 == "No Data") && PLCTelnet.connected)
                        //{
                        //    Thread.Sleep(waitdelay);
                        //    tmpstr100 = PLCTelnet.GetDataFromHost(ref tmpRx1, tmpRx1.Length);
                        //    //tmpstr100 = PLCTelnet.GetDataFromHost(ref PLCQueryRx, PLCQueryRx.Length);
                        //    if ((tmpstr100.StartsWith("d00000ffff") && (tmpstr100.Length == 1222)))
                        //    {
                        //        Array.Copy(tmpRx1, PLCQueryRx7, PLCQueryRx7.Length);
                        //    }
                        //    else
                        //        tmpstr100 = "No Data";
                        //    log.Info(tmpstr100);
                        //    timeoutcounter++;
                        //    if (timeoutcounter == 10)
                        //    {
                        //        //assume connection broken
                        //        PLCTelnet.Close();
                        //        break;
                        //    }
                        //}
                        #endregion


                        timeoutcounter = 0;
                        if (PLCTelnet.connected == false) continue;
                        PLCTelnet.SendDataToHost(PLCWriteCommand);
                        tmpstr100 = PLCTelnet.GetDataFromHost(ref PLCWriteCommandRX, PLCWriteCommandRX.Length);//write to dm 200 to dm 599
                      //  log.Info("PLC 1 Write " + tmpstr100);
                        while ((tmpstr100 == "No Data") && PLCTelnet.connected)
                        {
                            Thread.Sleep(waitdelay);
                            tmpstr100 = PLCTelnet.GetDataFromHost(ref PLCWriteCommandRX, PLCWriteCommandRX.Length);
                          //  log.Info("PLC 1 Write " + tmpstr100);
                            timeoutcounter++;
                            if (timeoutcounter == 10)
                            {
                                //assume connection broken
                                PLCTelnet.Close();
                                break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Info("PLC01Polling Thread Error " + ex.ToString());
                }
            }//while ! terminated loop
        }
    }
}
