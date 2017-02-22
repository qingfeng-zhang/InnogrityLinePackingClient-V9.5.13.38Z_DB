
using System;
using System.Net;
using System.Threading;
using System.Xml;
using IGTwpf;
using System.ComponentModel;

namespace InnogrityLinePackingClient
{
    partial class NetworkThread
    {
        LPAUtils.DB.ReportQuery rq = new LPAUtils.DB.ReportQuery();
        public void RunMiddlewareScan(object msgobj)
        {
            MainNetworkClass networkmain = (MainNetworkClass)msgobj;
            int counter = 0;
            while (!bTerminate)
            {
                Thread.Sleep(10);
                try
                {
                    counter++;
                    connected = networkmain.connected;
                    //if not connected to middleware do a reconnection
                    if (networkmain.connected == false)
                    {
                        System.Net.IPHostEntry oEntry = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
                        System.Net.IPAddress address = oEntry.AddressList[0];
                        XmlDocument doc = new XmlDocument();
                        doc.Load(@"Config.xml");
                        XmlNode node = doc.SelectSingleNode(@"/CONFIG/MIDDLEWARE/ADD");
                        address = System.Net.IPAddress.Parse(node.InnerText);
                        node = doc.SelectSingleNode(@"/CONFIG/MIDDLEWARE/PORT");
                        NetworkAddress = "Trying to connect to " + address.ToString() + " Port : " + node.InnerText;
                        IPAddress ipAddress = address;//Dns.Resolve("localhost").AddressList[2];
                       
                        if (networkmain.ConnectToHost(ipAddress) == false)
                        {
                            NetworkAddress = "Connected to MICServer " + address.ToString() + " Port : " + node.InnerText + " Fail"+ "  V9.5.13.38Z_DB";
                            Thread.Sleep(100);// try to connect to server
                           UserName1 = "";
                           UserName2 = "";
                           StationStarStopLog.Info("Middleware Connection break "+ ipAddress);
                           networkmain.linePack.Info("Middleware Connection break "+ ipAddress);
                            MyEventQ.AddQ("1;MiddlewareCommunicationBreak" );//Push message to stack

                            // EvtLog.Info("1;MiddlewareCommunicationBreak");

                            continue;
                        }
                        NetworkAddress = "Connected to MICServer " + address.ToString() + " Port : " + node.InnerText + "  V9.5.13.38Z_DB";
                        MyEventQ.AddQ("4;InnogrityServerApplicationStarted");

                        BackgroundWorker bw = new BackgroundWorker();
                        bw.DoWork += new DoWorkEventHandler(delegate (object o, DoWorkEventArgs args)
                        {
                            ClearAllJam();
                        });
                        bw.RunWorkerAsync();

                        // EvtLog.Info("4;InnogrityServerApplicationStarted");
                    }
                    if (networkmain.connected)// if connected, do check 
                    {
                        if (counter > 100)
                        {
                            networkmain.CheckHost();//have to use proper xml to test check or else server may crash
                            counter = 0;
                        }
                        try
                        {
                            networkmain.GetDataFromHost();
                        }
                        catch (Exception ex) { }
                    }
                    #region Station2
                    //Station 2 check if there is any outgoing messages
                    //Finishing label
                    if (evt_FinishLabelRequest.WaitOne(0))
                    {
                        string Boxnumber = "myboxnumberbarcode";//sample box number/ finishing label request
                                                                //save finishing label...to label tracking, label tracking to be updated..later when server reply
                                                                //send finishing label request                   
                        if (networkmain.Client_SendScanBox(Scanboxid) == true)//previously wait for reply .. now reply is waited seperately
                                                                              //update label tracking data
                        {
                            #region OEEStep2
                            //OEE MODULE - GET ID and Bind ID number With FL 
                            int OEEid = 0;
                            try
                            {
                                OEEid = rq.ReqLastID();
                                rq.UpdFLbyID(Scanboxid, OEEid);
                            }
                            catch (Exception ex)
                            {
                                IGTOEELog.Info("OEEStep2" + ex.ToString());
                            }

                            #endregion
                            SendFL = Scanboxid;
                            Boolean bHotlot = false;
                            string sHotlot = "";
                            if (PLCQueryRx[11 + 109 * 2] == 0x08) //DM1019  --Hotlot
                            {
                                bHotlot = true;
                                sHotlot = "1";
                                networkmain.stn2log = "HotLot :" + Scanboxid;
                                networkmain.linePack.Info("HotLot :" + Scanboxid);
                            }
                            else
                            {
                                sHotlot = "0";
                            }
                            //while ((!networkmain.updatefltrackinginfomation(Scanboxid, OEEid.ToString(), sHotlot) && !bTerminate))
                            //{
                            //    Thread.Sleep(100);
                            //}
                            while ((!networkmain.updatefltrackinginfomationb(Scanboxid, OEEid.ToString(), bHotlot) && !bTerminate))
                            {
                                Thread.Sleep(100);
                            }
                            evt_FinishLabelRequest.Reset();//reset PLC request event
                            StationStarStopLog.Info("Running Start " + Scanboxid);
                            networkmain.linePack.Info("Already Sending Boxid to Middleware  " + Scanboxid);

                            PLCWriteCommand[21] = 0x08;// send request complete DM200
                        }
                        else
                        {
                            //will not write into tracking list. 
                            //There is a send fail!
                            evt_FinishLabelRequest.Reset();//reset PLC request event
                            //networkmain.linePack.Info("Reject FL at buffer because of same FL coming two times  " + Scanboxid);
                            networkmain.OperatorLog = "Same FL comging two times " + Scanboxid;
                            networkmain.Client_SendEventMessage("84", "ST2 BUFFER FL DUPLICATE REJECT", "BOX_ID", Scanboxid);
                            AllRJEvent.Info("84" + ";" + "ST2 BUFFER FL DUPPLICATE REJECT" + ";" + "BOX_ID" + ";" + Scanboxid);
                            rq.INS_Func(DateTime.Now);
                            int BufferDup = rq.ReqLastID();
                            rq.UpdST4RJ(BufferDup.ToString(),4,"294");
                            PLCWriteCommand[21] = 0x0F;// send request complete DM200
                        }
                    }
                    #endregion


                    #region MiddlewareToIGT   LoginCheckQC

                  if(networkmain.MiddlewareToIGTEvt_LoginCheckQC1.WaitOne(0))
                  {

                    try
                    {

                   if( networkmain.CheckServerConnectionWithMiddlwareForQC1(UserName1)==false)
                   {
                     StationStarStopLog.Info("Connection String not Match With Middleware,OP1 Logout " + UserName1);

                     networkmain.linePack.Info("Connection String not Match With Middleware,OP1 Logout " + UserName1);

                     networkmain.Client_SendQCStationLogout("1",  networkmain.QC1LoginConnectionCheck1);
                     UserName1="";
                      networkmain.QC1LogOutSendReady=true;
                    PLCWriteCommand6[PLCWriteCmdOP01LoginLogout] = 0x00;
                   }
                  
                   networkmain.MiddlewareToIGTEvt_LoginCheckQC1.Reset();
                    }
                    catch
                    {
                    
                    }




                  }



                   if(networkmain.MiddlewareToIGTEvt_LoginCheckQC2.WaitOne(0))
                  {
                     try{
                  if( networkmain.CheckServerConnectionWithMiddlwareForQC2(UserName2)==false)
                   {
                   StationStarStopLog.Info("Connection String not Match With Middleware,OP2 Logout " + UserName2);
                   networkmain.linePack.Info("Connection String not Match With Middleware,OP2 Logout " + UserName2);
                    networkmain.Client_SendQCStationLogout("2",networkmain.QC2LoginConnectionCheck);
                    networkmain.QC2LogOutSendReady=true;
                    UserName2="";
                    PLCWriteCommand6[PLCWriteCmdOP02LoginLogout] = 0x00;
                   }
                  
                   networkmain.MiddlewareToIGTEvt_LoginCheckQC2.Reset();
                     }
                     catch
                     {
                     
                     }
                  
                  
                  }

                    #endregion
                    //Station 06 Login/Logout server exchange
                    #region Station06OPLoginLogout
                    //check if there is any incoming messages for login

                    if (networkmain.MiddlewareToIGTEvt_Login.WaitOne(0))
                    {
                        //process incoming messages
                        //Display on Station #
                        //Get username and StationID
                        //XmlNodeList list = networkmain.QCLogin.SelectNodes("MESSAGE/BODY/STATION_ID");
                        XmlNode list = networkmain.QCLogin.SelectSingleNode("MESSAGE/BODY/STATION_ID");
                        string StationID = list.InnerText;
                        //string StationID = list.Item(0).ToString();
                        XmlNode list1 = networkmain.QCLogin.SelectSingleNode("MESSAGE/BODY/USER_NAME");
                        String UserName = list1.InnerText;
                        if (StationID == "1")
                        {
                            UserName1 = UserName;
                            StationID1 = StationID;
                           
                            //reset PLC request event
                            PLCWriteCommand6[PLCWriteCmdOP01LoginLogout] = 0x08;// send request complete DM5200
                            networkmain.MiddlewareToIGTEvt_Login.Reset();
                        }
                        else if (StationID == "2")
                        {
                            UserName2 = UserName;
                            StationID2 = StationID;
                            networkmain.QC2Data=UserName;
                            //reset PLC request event
                            PLCWriteCommand6[PLCWriteCmdOP02LoginLogout] = 0x08;// send request complete DM5201
                            networkmain.MiddlewareToIGTEvt_Login.Reset();
                        }
                    }
                    switch (Operator01LoginState)
                    {
                        case WaitForOperatorLogin:
                            if ((PLCQueryRx6[PLCQueryRx_DM5100] == 0x08) &&
                                (PLCWriteCommand6[PLCWriteCommand_DM5200] == 0x08))//request finishing label 
                            {
                                //log6.Info("Operator 1 Login request "+StationID1+","+UserName1);
                                networkmain.linePack.Info("Operator 1 Login request "+StationID1+","+UserName1);
                                networkmain.Client_SendQCStationLogin(StationID1, UserName1);
                                networkmain.QC1LogOutSendReady=false;
                                networkmain.QC1Data=UserName1;
                                Operator01LoginState = WaitForOperatorLogout;
                            }

                           
                             if ((PLCQueryRx6[PLCQueryRx_DM5100] == 0x0F) &&
                                (PLCWriteCommand6[PLCWriteCommand_DM5200] == 0x08))//timeout  
                            {
                                 networkmain.QC1Data=UserName1;
                               // log6.Info("Operator 1 login 3 mins timeout "+StationID1+","+UserName1);
                                networkmain.linePack.Info("Operator 1 login 3 mins timeout "+StationID1+","+UserName1);
                                networkmain.Client_SendQCStationLogout(StationID1, UserName1);
                                networkmain.QC1LogOutSendReady=false;
                                networkmain.QC1Data="";
                                UserName1="";   
                                POcount1 = 0;
                                PLCWriteCommand6[PLCWriteCommand_DM5200] = 0x00;
                                Operator01LoginState = WaitForOperatorLogin;
                            }
                            break;
                        case WaitForOperatorLogout:
                            if ((PLCQueryRx6[PLCQueryRx_DM5100] == 0xF) && (PLCWriteCommand6[PLCWriteCommand_DM5200] == 0x08))
                            {
                                // send middleware operator1 logout
                                PLCWriteCommand6[PLCWriteCommand_DM5200] = 0x00;
                                networkmain.Client_SendQCStationLogout(StationID1, UserName1);
                              
                                networkmain.QC1LogOutSendReady=true;
                                POcount1 = 0;
                                
                              // log6.Info("Operator 1 Logout request "+StationID1+","+UserName1);
                               networkmain.linePack.Info("Operator 1 Logout request "+StationID1+","+UserName1);

                               networkmain.QC1Data="";
                                UserName1="";                           
                                Operator01LoginState = WaitForOperatorLogin;
                            }
                            break;
                    }
                    switch (Operator02LoginState)
                    {
                        case WaitForOperatorLogin:
                            if ((PLCQueryRx6[PLCQueryRx_DM5100 + 2] == 0x08) &&
                                (PLCWriteCommand6[PLCWriteCommand_DM5200 + 2] == 0x08))//request finishing label 
                            {
                               // log6_1.Info("Operator 2 Login request "+StationID2+","+UserName2);
                                networkmain.linePack.Info("Operator 2 Login request "+StationID2+","+UserName2);
                                networkmain.Client_SendQCStationLogin(StationID2, UserName2);
                                networkmain.QC2LogOutSendReady=false;
                                networkmain.QC2Data=UserName2;
                                Operator02LoginState = WaitForOperatorLogout;
                            }

                         if ((PLCQueryRx6[PLCQueryRx_DM5100 +2] == 0x0F) &&
                                (PLCWriteCommand6[PLCWriteCommand_DM5200+2] == 0x08))//timeout  
                            {
                                 networkmain.QC2Data=UserName2;
                               // log6_1.Info("Operator 2 login 3 mins timeout "+StationID2+","+UserName2);
                                networkmain.linePack.Info("Operator 2 login 3 mins timeout "+StationID2+","+UserName2);

                                networkmain.Client_SendQCStationLogout(StationID2, UserName2);
                                networkmain.QC2LogOutSendReady=false;
                                networkmain.QC2Data="";
                                UserName2="";   
                                POcount2 = 0;
                                PLCWriteCommand6[PLCWriteCommand_DM5200+2] = 0x00;
                                Operator02LoginState = WaitForOperatorLogin;
                            }

                            break;
                        case WaitForOperatorLogout:
                            if ((PLCQueryRx6[PLCQueryRx_DM5100 + 2] == 0xF) && (PLCWriteCommand6[PLCWriteCommand_DM5200 + 2] == 0x08))
                            {
                                // send middleware operator1 logout
                                PLCWriteCommand6[PLCWriteCommand_DM5200 + 2] = 0x00;
                                networkmain.Client_SendQCStationLogout(StationID2, UserName2);
                                networkmain.QC2LogOutSendReady=true;
                                POcount2 = 0;
                               // log6_1.Info("Operator 2 Logout request "+StationID2+","+UserName2);

                               networkmain.linePack.Info("Operator 2 Logout request "+StationID2+","+UserName2);
                               UserName2="";
                                networkmain.QC2Data="";
                                
                               // UserName2="";
                                Operator02LoginState = WaitForOperatorLogin;
                            }
                            break;
                    }
                    #endregion
                    if (evt_FG01_FG02Move.WaitOne(0))
                    {
                        networkmain.Client_sendFG01_FG02_MOVE1(st8FINISH, "FG01_FG02_MOVE");
                        networkmain.Client_sendFG01_FG02_MOVE(st8FINISH, "FG01_FG02_MOVE");

                        evt_FG01_FG02Move.Reset();
                        evt_FG01_FG02Move_Rx.Set();
                    }
                }
                catch (Exception ex)
                {
                    log.Error(ex.ToString());


                }
            }//
        }
    }
}
