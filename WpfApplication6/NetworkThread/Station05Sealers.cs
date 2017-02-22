
using InnovacVacuumSealerPackage;
using NLog;
using System;
using System.Text;
using System.Threading;
using System.Xml;
using IGTwpf;
namespace InnogrityLinePackingClient
{
    partial class NetworkThread
    {
        public string messagest5 = "";
        public string Errst5 = "";


        public string messagest5_1 = "";
        public string Errst5_1 = "";

        public string messagest5_2 = "";
        public string Errst5_2 = "";
        

        public void RunStation05Sealers(object msgobj)
        {
            MainNetworkClass networkmain = (MainNetworkClass)msgobj;
            while (!bTerminate)
            {
                Thread.Sleep(100);
                try
                {

                    bool Sealerflag1 = false;
                    bool Sealerflag2 = false;
                    bool Sealerflag3 = false;

                    if (PLCTelnet2 != null)
                        if (PLCTelnet2.connected) {

                          #region Sealer ID
                          //Sealer1

                            byte[] temps1 = new byte[12];
                            Array.Copy(PLCQueryRx6, 91, temps1, 0, 12); // DM 5040 PLC2
                            Sealer1ID = System.Text.Encoding.Default.GetString(temps1);

                          if (Sealer1ID != "\0\0\0\0\0\0\0\0\0\0\0\0"  && Sealer1ID != "            ")
                            {
                              Sealer1IDshow= Sealer1ID; 
                            }
                            else
                            {
                                Sealer1IDshow = "Sealer1IDisNULL";
                            }


                           byte[] temps2 = new byte[12];
                            Array.Copy(PLCQueryRx6, 141, temps2, 0, 12); // DM 5065 PLC2
                            Sealer2ID = System.Text.Encoding.Default.GetString(temps2);

                          if (Sealer2ID != "\0\0\0\0\0\0\0\0\0\0\0\0" && Sealer2ID != "            ")
                            {
                              Sealer2IDshow= Sealer2ID; 
                            }
                            else
                            {
                                Sealer2IDshow = "Sealer2IDisNULL";
                            }


                            byte[] temps3 = new byte[12];
                            Array.Copy(PLCQueryRx6, 153, temps3, 0, 12); // DM 5071 PLC2
                            Sealer3ID = System.Text.Encoding.Default.GetString(temps3);

                          if (Sealer3ID != "\0\0\0\0\0\0\0\0\0\0\0\0" && Sealer3ID != "            ")
                            {
                              Sealer3IDshow= Sealer3ID; 
                            }
                            else
                            {
                                Sealer3IDshow = "Sealer3IDisNULL";
                            }


                            #endregion





                            #region  sealer program For Station5
                            //edit .............................

                            byte[] temp2 = new byte[10];
                            Array.Copy(PLCQueryRx6, 201, temp2, 0, 10); // DM 5095 PLC2 =201 offset
                            Station5ForTransferScanboxidFromPLC2 = System.Text.Encoding.Default.GetString(temp2);

                            if (Station5ForTransferScanboxidFromPLC2 != "\0\0\0\0\0\0\0\0\0\0")
                            {
                                ST5NewFLRev = true;
                                CheckStringUpdateFor5(PLCFinishingLabelOFFSET, Station5ForTransferScanboxidFromPLC2);
                            }
                            else
                            {
                                ST5NewFLRev = false;
                                Station5ForTransferScanboxidFromPLC2 = "\0\0\0\0\0\0\0\0\0\0";
                                CheckStringClearFor5(PLCFinishingLabelOFFSET, Station5ForTransferScanboxidFromPLC2); // DM5271
                            }
                            // Compare PLC fl and thread fl
                            // Set Status to Busy

                            
                            #endregion



                            #region  Sealer checking in Station 5 Initialization time
                          
                            /*
                           if (evnt_CheckingConnectionForSealer1.WaitOne(50))
                            {
                                try
                                {
                                    if (VS1VacuumSealer != null)
                                    {
                                       
                                        try
                                        {
                                            VS1VacuumSealer.Close();
                                        }
                                        catch (Exception) { }
                                        VS1VacuumSealer = null;
                                      
                                    }
                                    if (VS1VacuumSealer == null)
                                    {
                                        XmlDocument doc = new XmlDocument();
                                        doc.Load(@"Config.xml");
                                        XmlNode Sealernode = doc.SelectSingleNode(@"/CONFIG/SEALER1/PORT");
                                        String comport = Sealernode.InnerText;
                                        //TODO: ConfigFIle
                                        VS1VacuumSealer = new InnovacVacuumSealer(comport);
                                        VS1VacuumSealer.Open();
                                       // short testreceipe = 240;
                                        //VS1VacuumSealer.SelectAndConfirmProgram(testreceipe, InnovacVacuumSealer.SealerBar.Right);
                                       Sealer1Log.Info("Sealer1 connected at initialization time");
                                       networkmain.stn5log ="slr.1 connected at initialization";
                                       networkmain.OperatorLog = "Sealer1 connected at initialization time";
                                       InnovacVacuumSealer.SystemStatus sysMessage;
                                       VS1VacuumSealer.GetSystemStatus(out sysMessage);
                                       if (sysMessage != InnovacVacuumSealer.SystemStatus.Undefined && sysMessage != InnovacVacuumSealer.SystemStatus.Error && Sealer1IDshow != "Sealer1IDisNULL")
                                       {
                                           PLCWriteCommand6[PLCWriteCommand_DM5362] = 0x06;
                                       }
                                       else
                                       {
                                           PLCWriteCommand6[PLCWriteCommand_DM5362] = 0xFF;
                                       }
                                    }                                  
                              

                                }
                                catch (Exception ex)
                                {
                                    Sealer1Log.Error(ex.ToString());
                                    
                                    PLCWriteCommand6[PLCWriteCommand_DM5362] = 0xFF;
                                    Sealer1Log.Info("Sealer1 Can't connected at initialization time");
                                    networkmain.stn5log = "slr.1 Can't connected at initialization";
                                    networkmain.OperatorLog = "Sealer1 Can't connected at initialization time";
                                    string  abc = Sealer1IDshow.Trim();
                                    MyEventQ.AddQ("8;SealerCommunicationBreak;SealerID;" + abc);
                                    EvtLog.Info("8;SealerCommunicationBreak;SealerID;" + abc);

                                    evnt_CheckingConnectionForSealer1.Reset();
                                }
                                evnt_CheckingConnectionForSealer1.Reset();
                            }

                           */


                           if (evnt_CheckingConnectionForSealer2.WaitOne(50))
                            {
                                try
                                {
                                    if (VS2VacuumSealer != null)
                                    {
                                       
                                        try
                                        {
                                            VS2VacuumSealer.Close();
                                        }
                                        catch (Exception) { }
                                        VS2VacuumSealer = null;
                                      
                                    }
                                    if (VS2VacuumSealer == null)
                                    {
                                        XmlDocument doc = new XmlDocument();
                                        doc.Load(@"Config.xml");
                                        XmlNode Sealernode = doc.SelectSingleNode(@"/CONFIG/SEALER2/PORT");
                                        String comport = Sealernode.InnerText;
                                        //TODO: ConfigFIle
                                        VS2VacuumSealer = new InnovacVacuumSealer(comport);
                                        VS2VacuumSealer.Open();
                                        //short testreceipe = 240;
                                       // VS2VacuumSealer.SelectAndConfirmProgram(testreceipe, InnovacVacuumSealer.SealerBar.Right);
                                        Sealer2Log.Info("Sealer2 connected at initialization time");
                                        networkmain.stn5log = "slr.2 connected at initialization";
                                        networkmain.OperatorLog = "Sealer2 connected at initialization time";
                                        InnovacVacuumSealer.SystemStatus sysMessage;
                                        VS2VacuumSealer.GetSystemStatus(out sysMessage);
                                        if (sysMessage != InnovacVacuumSealer.SystemStatus.Undefined && sysMessage != InnovacVacuumSealer.SystemStatus.Error && Sealer2IDshow != "Sealer2IDisNULL")
                                        {
                                            PLCWriteCommand6[PLCWriteCommand_DM5363] = 0x06;
                                        }
                                        else
                                        {
                                            PLCWriteCommand6[PLCWriteCommand_DM5363] = 0xFF;
                                        }
                                    }                                  
                              

                                }
                                catch (Exception ex)
                                {
                                    Sealer2Log.Error(ex.ToString());
                                    
                                    PLCWriteCommand6[PLCWriteCommand_DM5363] = 0xFF;
                                   Sealer2Log.Info("Sealer2 Can't connected at initialization time");
                                    networkmain.stn5log = "slr.2 Can't connected at initialization";
                                    networkmain.OperatorLog = "Sealer2 Can't connected at initialization time";
                        
                                        string abc = Sealer2IDshow.Trim();
                                    MyEventQ.AddQ("8;SealerCommunicationBreak;SealerID;" + abc);
                                    EvtLog.Info("8;SealerCommunicationBreak;SealerID;" + abc);
                                    evnt_CheckingConnectionForSealer2.Reset();
                                }
                                evnt_CheckingConnectionForSealer2.Reset();
                            }




                           if (evnt_CheckingConnectionForSealer3.WaitOne(50))
                            {
                                try
                                {
                                    if (VS3VacuumSealer != null)
                                    {
                                       
                                        try
                                        {
                                            VS3VacuumSealer.Close();
                                        }
                                        catch (Exception) { }
                                        VS3VacuumSealer = null;
                                      
                                    }
                                    if (VS3VacuumSealer == null)
                                    {
                                        XmlDocument doc = new XmlDocument();
                                        doc.Load(@"Config.xml");
                                        XmlNode Sealernode = doc.SelectSingleNode(@"/CONFIG/SEALER3/PORT");
                                        String comport = Sealernode.InnerText;
                                        //TODO: ConfigFIle
                                        VS3VacuumSealer = new InnovacVacuumSealer(comport);
                                        VS3VacuumSealer.Open();
                                        //short testreceipe = 240;
                                        //VS3VacuumSealer.SelectAndConfirmProgram(testreceipe, InnovacVacuumSealer.SealerBar.Right);
                                        Sealer3Log.Info("Sealer3 connected at initialization time");
                                        networkmain.stn5log = "slr.3 connected at initialization";
                                        networkmain.OperatorLog = "Sealer3 connected at initialization time";
                                        InnovacVacuumSealer.SystemStatus sysMessage;
                                        VS3VacuumSealer.GetSystemStatus(out sysMessage);
                                        if (sysMessage != InnovacVacuumSealer.SystemStatus.Undefined && sysMessage != InnovacVacuumSealer.SystemStatus.Error && Sealer3IDshow != "Sealer3IDisNULL")
                                        {
                                            PLCWriteCommand6[PLCWriteCommand_DM5426] = 0x06;
                                        }
                                        else
                                        {
                                            PLCWriteCommand6[PLCWriteCommand_DM5426] = 0xFF;
                                        }
                                    }                                  
                              

                                }
                                catch (Exception ex)
                                {
                                    Sealer3Log.Error(ex.ToString());

                                    PLCWriteCommand6[PLCWriteCommand_DM5426] = 0xFF;
                                    Sealer3Log.Info("Sealer3 Can't connected at initialization time");
                                    networkmain.stn5log = "slr.3 Can't connected at initialization";
                                    networkmain.OperatorLog = "Sealer3 Can't connected at initialization time";

                                    string abc = Sealer3IDshow.Trim();
                                    MyEventQ.AddQ("8;SealerCommunicationBreak;SealerID;" + abc);
                                    EvtLog.Info("8;SealerCommunicationBreak;SealerID;" + abc);

                                    evnt_CheckingConnectionForSealer3.Reset();
                                }
                                evnt_CheckingConnectionForSealer3.Reset();
                            }

                            #endregion
                          

                            #region Station 5 Error Code


                            byte[] tmparrayERst5 = new byte[2];
                            Array.Copy(PLCQueryRx6, 401, tmparrayERst5, 0, 2); //5195 
                            //convert Byte array to int                 
                            Int32 erst5 = (Int32)(BitConverter.ToInt16(tmparrayERst5, 0));

                            byte[] tmparrayERst5_1 = new byte[2];
                            Array.Copy(PLCQueryRx6, 403, tmparrayERst5_1, 0, 2); //5196 
                            //convert Byte array to int                 
                            Int32 erst5_1 = (Int32)(BitConverter.ToInt16(tmparrayERst5_1, 0));

                           byte[] tmparrayERst5_2 = new byte[2];
                            Array.Copy(PLCQueryRx6,359, tmparrayERst5_2, 0, 2); //5174
                            //convert Byte array to int                 
                            Int32 erst5_2 = (Int32)(BitConverter.ToInt16(tmparrayERst5_2, 0));
                          
                            ErrCode5_2 = erst5_2.ToString();
                            ErrCode5_1 = erst5_1.ToString();
                            ErrCode5 = erst5.ToString();



                            #region NewErrorCode
                            if (erst5 > 0 || erst5_1 > 0 || erst5_2 > 0)
                            {

                                // LogEr.Info("Station 5 Error Code"+ErrCode5+ErrCode5_1+ErrCode5_2);
                                Errmessage5 = "Stn.5 Err " +
                                    (erst5 > 0 ? ErrCode5 + ": " + Stn5ErrToMsg(erst5) : "") +
                                    (erst5_1 > 0 && erst5 != erst5_1 ? ", " + ErrCode5_1 + ": " + Stn5ErrToMsg(erst5_1) : "") +
                                    (erst5_2 > 0 && erst5 != erst5_2 ? ", " + ErrCode5_2 + ": " + Stn5ErrToMsg(erst5_2) : "");

                                if (!ST5JamFlag)
                                {
                                    bool St5JamTrig = ST2PauseFunction(5, erst5 + ";" + erst5_1 + ";" + erst5_2); //Check if is a JAM
                                    if (St5JamTrig)
                                    {
                                        ST5JamFlag = true;
                                        //string[] FLbatch = rq.UpdJamstatus(5, 555); //Update Jam FL
                                        //if (FLbatch != null)
                                        //{
                                        //    networkmain.Client_SendEventMsg("536", "Station5FLJAMRecovery", FLbatch);//Update Jam recovery FL to middleware
                                        //}
                                    }
                                }



                            }
                            else
                            {
                                ST5JamFlag = false;
                                Errmessage5 = String.Empty;
                            }
                            #endregion
                            #region OldCode
                            //if (erst5 > 0 || erst5_1 > 0 || erst5_2 > 0)
                            //{

                            //    // LogEr.Info("Station 5 Error Code"+ErrCode5+ErrCode5_1+ErrCode5_2);
                            //    Errmessage5 = "Stn.5 Err " +
                            //        (erst5 > 0 ? ErrCode5 + ": " + Stn5ErrToMsg(erst5) : "") +
                            //        (erst5_1 > 0 && erst5 != erst5_1 ? ", " + ErrCode5_1 + ": " + Stn5ErrToMsg(erst5_1) : "") +
                            //        (erst5_2 > 0 && erst5 != erst5_2 ? ", " + ErrCode5_2 + ": " + Stn5ErrToMsg(erst5_2) : "");


                            //    //     LogEr.Info(Errmessage5);




                            //}
                            //else
                            //{
                            //    Errmessage5 = String.Empty;
                            //}
                            #endregion
                            UpdateErrorMsg((int)Station.StationNumber.Station05, Errmessage5, ST5JamFlag);

                               if((erst5 > 0)  && networkmain.controlst5==0)
                              {
                                Errst5=erst5.ToString();
                                networkmain.controlst5=1;
                               messagest5= Stn5ErrToMsg(erst5);
                               networkmain.Client_SendAlarmMessage5(erst5.ToString(), messagest5,"SET");
                            
                              }
                                if(erst5 == 0 && networkmain.controlst5==1)
                              {
                              networkmain.Client_SendAlarmMessage5(Errst5, messagest5,"CLEAR");
                              networkmain.controlst5=0;
                               Errst5="";
                               messagest5="";
                              }




                              if((erst5_1 > 0) && (erst5_1 != erst5) && networkmain.controlst5_1==0)
                              {
                                Errst5_1=erst5_1.ToString();
                                networkmain.controlst5_1=1;
                               messagest5_1= Stn5ErrToMsg(erst5_1);
                               networkmain.Client_SendAlarmMessage5(erst5_1.ToString(), messagest5_1,"SET");
                            
                              }
                                if(erst5_1 == 0 && networkmain.controlst5_1==1)
                              {
                              networkmain.Client_SendAlarmMessage5(Errst5_1, messagest5_1,"CLEAR");
                              networkmain.controlst5_1=0;
                               Errst5_1="";
                               messagest5_1="";
                              }                              

                              if(erst5_2 > 0 && erst5 != erst5_2 && (erst5_1 != erst5_2) && networkmain.controlst5_2==0)
                              {
                                Errst5_2=erst5_2.ToString();
                                networkmain.controlst5_2=1;
                               messagest5_2= Stn5ErrToMsg(erst5_2);
                               networkmain.Client_SendAlarmMessage5(erst5_2.ToString(), messagest5_2,"SET");
                            
                              }
                                if(erst5_2 == 0 && networkmain.controlst5_2==1)
                              {
                              networkmain.Client_SendAlarmMessage5(Errst5_2, messagest5_2,"CLEAR");
                              networkmain.controlst5_2=0;
                               Errst5_2="";
                               messagest5_2="";
                              }

                            #endregion
                        }
                    //  break;
                }
                catch (Exception ex)
                {
                    log.Error(ex.ToString());
                }
            }
        }
    }
}
