
using InnovacVacuumSealerPackage;
using NLog;
using System;
using System.Text;
using System.Threading;
using System.Xml;
using IGTwpf;
using System.Windows.Threading;

namespace InnogrityLinePackingClient
{
    partial class NetworkThread
    {
        DispatcherTimer Sealer1ErrResetTimer = new DispatcherTimer();
        DispatcherTimer Sealer2ErrResetTimer = new DispatcherTimer();
        DispatcherTimer Sealer3ErrResetTimer = new DispatcherTimer();
        private bool Sealer1ErrAtv, Sealer2ErrAtv, Sealer3ErrAtv;
        private string Sealer1ErrorCode, Sealer1EventMesg, Sealer2ErrorCode, Sealer2EventMesg, Sealer3ErrorCode, Sealer3EventMesg;
        public  short pNumber1;

        private void ExceptionMesgSend1(string ErrCode,string SealerID, string EventMesg, string ExMesg )
        {
            networkmain.Client_SendAlarmMessage(ErrCode,EventMesg,"SET");
            LogEr.Info(ErrCode + ";" + EventMesg + "; SET");
            MyEventQ.AddQ("533;Sealer1Error;SealerID;" + SealerID + ";SealerErrorCode;" + ErrCode + ";ErrorMessage;" + EventMesg);
            EvtLog.Info("533;SSealer1Error;SealerID;" + SealerID + ";SealerErrorCode;" + ErrCode + ";ErrorMessage;" + EventMesg);
        }
        private void Sealer1ErrResetTimer_Tick(object sender, EventArgs e)
        {
            if (Sealer1ErrorCode != "")
            {
                networkmain.Client_SendAlarmMessage(Sealer1ErrorCode, Sealer1EventMesg, "CLEAR");
                LogEr.Info(Sealer1ErrorCode + "," + Sealer1EventMesg + ", CLEAR");
                Sealer1ErrorCode = "";
                Sealer1EventMesg = "";
                //Sealer1ErrResetTimer.Tick -= Sealer1ErrResetTimer_Tick;
                //Sealer1ErrResetTimer.Stop();
                //Sealer1ErrAtv = false;
            }
            Sealer1ErrResetTimer.Tick -= Sealer1ErrResetTimer_Tick;
            Sealer1ErrResetTimer.Stop();
           // Sealer1ErrResetTimer.Start();
             Sealer1ErrAtv = false;
        }
        private void ExceptionMesgSend2(string ErrCode, string SealerID, string EventMesg, string ExMesg)
        {

            networkmain.Client_SendAlarmMessage(ErrCode, EventMesg, "SET");
            LogEr.Info(ErrCode + ";" + EventMesg + "; SET");
            MyEventQ.AddQ("534;Sealer2Error;SealerID;" + SealerID + ";SealerErrorCode;" + ErrCode + ";ErrorMessage;" + EventMesg);
            EvtLog.Info("534;Sealer2Error;SealerID;" + SealerID + ";SealerErrorCode;" + ErrCode + ";ErrorMessage;" + EventMesg);
        }
        private void Sealer2ErrResetTimer_Tick(object sender, EventArgs e)
        {
            if (Sealer2ErrorCode != "")
            {
                networkmain.Client_SendAlarmMessage(Sealer2ErrorCode, Sealer2EventMesg, "CLEAR");
                LogEr.Info(Sealer2ErrorCode+","+ Sealer2EventMesg+", CLEAR");
                Sealer2ErrorCode = "";
                Sealer2EventMesg = "";
            }
            Sealer2ErrResetTimer.Tick -= Sealer2ErrResetTimer_Tick;
            Sealer2ErrResetTimer.Stop();
           Sealer2ErrAtv = false;
        }
        private void ExceptionMesgSend3(string ErrCode, string SealerID, string EventMesg, string ExMesg)
        {

            networkmain.Client_SendAlarmMessage(ErrCode, EventMesg, "SET");
            LogEr.Info(ErrCode+ ";" + EventMesg + "; SET");
            MyEventQ.AddQ("535;Sealer3Error;SealerID;" + SealerID + ";SealerErrorCode;" + ErrCode + ";ErrorMessage;" + EventMesg);
            EvtLog.Info("535;Sealer3Error;SealerID;" + SealerID + ";SealerErrorCode;" + ErrCode + ";ErrorMessage;" + EventMesg);
        }
        private void Sealer3ErrResetTimer_Tick(object sender, EventArgs e)
        {
            if (Sealer3ErrorCode != "")
            {
                networkmain.Client_SendAlarmMessage(Sealer3ErrorCode, Sealer3EventMesg, "CLEAR");
                LogEr.Info(Sealer3ErrorCode + "," + Sealer3EventMesg + ", CLEAR");
                Sealer3ErrorCode = "";
                Sealer3EventMesg = "";
            }
            Sealer3ErrResetTimer.Tick -= Sealer3ErrResetTimer_Tick;
            Sealer3ErrResetTimer.Stop();
            Sealer3ErrAtv = false;
        }
        public void RunStation05Sealer1(object msgobj)
        {
            MainNetworkClass networkmain = (MainNetworkClass)msgobj;
            while (!bTerminate)
            {
                Thread.Sleep(100);
                try
                {

                    bool Sealerflag1 = false;

                    string abc;
                    if (PLCTelnet2 != null)
                        if (PLCTelnet2.connected) {

                       





                          #region  sealer program For Station5
                         

                            #region    //Sealer1

                            byte[] tmparray2 = new byte[10];
                            //DM5080
                            Array.Copy(PLCQueryRx6, 171, tmparray2, 0, 10);
                            //convert array to string                        
                            Station5ForSealer1Scanboxid = System.Text.Encoding.Default.GetString(tmparray2);
                            if (Station5ForSealer1Scanboxid != "\0\0\0\0\0\0\0\0\0\0")
                            {
                                ScanboxidSt5S1=Station5ForSealer1Scanboxid;
                                Sealer1Log.Info("Finishing Label for Station 5 Sealer1 " + Station5ForSealer1Scanboxid);
                                networkmain.stn5log = Station5ForSealer1Scanboxid + " slr.1 recieved";
                                #region Desiccant Timmer
                                Thread.Sleep(10);
                                if ((PLCQueryRx6[PLCQueryRx_DM5111 + 12] == 0X07) && (PLCWriteCommand6[PLCWriteCommand_DM5405]) == 0x00)//D5117
                                {
                                  try {

                                    #region HIC YES Or No Check

                                    //if(Station5ForSealer1HICYESNO=="NO")
                                    //{
                                    //(PLCWriteCommand6[PLCWriteCommand_DM5405]) = 0x07;
                                    //}
                                    

                                    //else
                                    //{

                                    Sealer1Log.Info("Finishing Label for Station 5 Sealer1 PLC Send HIC/Desiccant stop Signal " + Station5ForSealer1Scanboxid);
                                    networkmain.stn5log = Station5ForSealer1Scanboxid + " slr.1 HIC/Desct stop";
                                    if (Sealerflag1 == false)
                                    {

                                        try
                                        {
                                            Sealerflag1 = true;
                                            TimeSpan span = DateTime.Now.Subtract(DesiccantTimingMap[Station5ForSealer1Scanboxid]);
                                            double secs = span.TotalSeconds;
                                            Sealer1Log.Info("PC delete HIC/Desiccant Finishing Label for Station 5 Sealer1 " + Station5ForSealer1Scanboxid);
                                            networkmain.stn5log = Station5ForSealer1Scanboxid + " slr.1 PC delete HIC/Desiccant";
                                            // if (secs > 175) //NEED TO OPEN WHEN REAL TIME PRODUCTION
                                             if (secs > 300)
                                            {
                                                //Desiccant Expired. Reject the Part.
                                                //D5412=445
                                                Statusst5="Fail";
                                                MyEventQ.AddQ("9;Desiccant/HICTimeout;LotNumber;" + Station5ForSealer1Scanboxid);
                                                EvtLog.Info("9;Desiccant/HICTimeout;LotNumber;" + Station5ForSealer1Scanboxid);
                                                byte[] tmp = new byte[2];
                                                tmp = Encoding.ASCII.GetBytes("RJ");
                                                Array.Copy(tmp, 0, PLCWriteCommand6, 445, 2);

                                                try
                                                {
                                                    while ((!networkmain.UpdateRJLabelst5S1(Station5ForSealer1Scanboxid, "RJ", "500") && !bTerminate))
                                                    {
                                                        Thread.Sleep(100);
                                                    }
                                                    Sealer1Log.Info(" HIC/Desiccant Timeout Finishing Label (RJ) for Station 5 Sealer1  " + Station5ForSealer1Scanboxid);
                                                    networkmain.stn5log = Station5ForSealer1Scanboxid + " slr.1 HIC/Desiccant Timeout";
                                                    networkmain.OperatorLog = "Stn.5 Slr.1 HIC/Desiccant Timeout " + Station5ForSealer1Scanboxid;
                                                }
                                                catch (Exception ex)
                                                {
                                                    Sealer1Log.Error("station 5 Sealer1 HIC/Desiccant (RJ) Fail" + ex);
                                                    networkmain.stn5log = "slr.1 HIC/Desiccant fail";
                                                    networkmain.OperatorLog = "Stn.5 Slr.1HIC/Desiccant fail " + Station5ForSealer1Scanboxid;
                                                    byte[] tmp1 = new byte[2];
                                                    tmp1 = Encoding.ASCII.GetBytes("RJ");
                                                    Array.Copy(tmp1, 0, PLCWriteCommand6, 445, 2);

                                                    MyEventQ.AddQ("9;Desiccant/HICTimeout;LotNumber;" + Station5ForSealer1Scanboxid);
                                                    EvtLog.Info("9;Desiccant/HICTimeout;LotNumber;" + Station5ForSealer1Scanboxid);
                                                }
                                            }
                                            else
                                            {
                                                if (CheckeRJStatusForSealer1(Station5ForSealer1Scanboxid) == true)
                                                {
                                                    Sealer1Log.Info(" HIC/Desiccant Pass Finishing Label for Station 5 Sealer1  " + Station5ForSealer1Scanboxid);
                                                    networkmain.stn5log = Station5ForSealer1Scanboxid + " slr.1 HIC/Desiccant Pass";
                                                    Statusst5="Pass";


                                                }
                                                else
                                                {


                                            while ((!networkmain.UpdateRJLabelst5S1(Station5ForSealer1Scanboxid, "RJ", RJResultst5S1) && !bTerminate))
                                            {
                                                Thread.Sleep(100);
                                            }
                                                                                                     

                                                    byte[] tmp1 = new byte[2];
                                                    tmp1 = Encoding.ASCII.GetBytes("RJ");
                                                    Array.Copy(tmp1, 0, PLCWriteCommand6, 445, 2);
                                                    Statusst5="Fail";
                                                    Sealer1Log.Info(" Finishing Label for Station 5 Sealer1 (RJ) because of other station " + Station5ForSealer1Scanboxid +" RJ Code,"+RJResultst5S1);

                                                }
                                            }

                                            DesiccantTimingMap.Remove(Station5ForSealer1Scanboxid);
                                            (PLCWriteCommand6[PLCWriteCommand_DM5405]) = 0x07;
                                        }
                                        catch (Exception ex)
                                        {
                                            Sealer1Log.Error(ex.ToString());

                                            while ((!networkmain.UpdateRJLabelst5S1(Station5ForSealer1Scanboxid, "RJ", "505") && !bTerminate))
                                            {
                                                Thread.Sleep(100);
                                            }
                                            //D5412=445
                                            byte[] tmp = new byte[2];
                                            tmp = Encoding.ASCII.GetBytes("RJ");
                                            Array.Copy(tmp, 0, PLCWriteCommand6, 445, 2);
                                            Statusst5="Fail";
                                            Sealer1Log.Info(" HIC/Desiccant Fail Finishing Label (RJ) for Sealer1 because of station 3 but send signal to PLC " + Station5ForSealer1Scanboxid +" RJ Code,"+"505");
                                            networkmain.stn5log = Station5ForSealer1Scanboxid + " slr.1 HIC/Desiccant Fail Label because of station 3";
                                            networkmain.OperatorLog = "Stn.5 Slr.1 HIC/Desiccant Failed because of station 3" + Station5ForSealer1Scanboxid;
                                            (PLCWriteCommand6[PLCWriteCommand_DM5405]) = 0x07;
                                        }
                                    }
                                  //  } //change

                                    #endregion


                                     }

                                   catch (Exception ex)
                                        {
                                            Sealer1Log.Error(ex.ToString());
                                            Statusst5="Fail";

                                        }



                                }

                              

                                //  //TEST NEED TO DELET START
                                //if(PLCQueryRx6[PLCQueryRx_DM5111 + 12]==0X07)

                                //{ (PLCWriteCommand6[PLCWriteCommand_DM5405]) = 0x07;
                                //}
                                ////TEST NEED TO DELET END

                                #endregion
                                if (PLCWriteCommand6[PLCWriteCommand_DM5310] == 0x00)
                                {
                                    PLCWriteCommand6[PLCWriteCommand_DM5310] = 0x04; //busy Sealer1
                                    evnt_FindFinishingLabelForSealer1.Set();
                                    Sealer1Log.Info("Finishing Label for Station 5 Sealer1 receipt send already " + Station5ForSealer1Scanboxid);
                                    networkmain.stn5log = Station5ForSealer1Scanboxid + " slr.1 receipt send already";
                                    networkmain.OperatorLog = "Stn.5 slr.1 " + Station5ForSealer1Scanboxid + " receipt send already";
                                }
                                                           

                            }
                            else
                            {
                                PLCWriteCommand6[PLCWriteCommand_DM5310] = 0x00;
                                PLCWriteCommand6[PLCWriteCommand_DM5412] = 0x00;
                                 PLCWriteCommand6[PLCWriteCommand_DM5412b] = 0x00;
                                Station5ForSealer1Scanboxid = "\0\0\0\0\0\0\0\0\0\0";
                                Sealerflag1 = false;
                                pNumber1=0;
                                ScanboxidSt5S1="";
                                Statusst5="";
                            }

                            if (PLCQueryRx6[PLCQueryRx_DM5111 + 12] == 0X00)
                            {
                                (PLCWriteCommand6[PLCWriteCommand_DM5405]) = 0x00;
                            }

                            #endregion

                            
                            #endregion
                            #region Sealercomms
                            if (evnt_FindFinishingLabelForSealer1.WaitOne(0))
                            {
                                try
                                {
                                    if (VS1VacuumSealer != null)
                                    {
                                        //if (!VS1VacuumSealer.IsOpen)
                                        //{
                                        try
                                        {
                                            VS1VacuumSealer.Close();
                                        }
                                        catch (Exception) { }
                                        VS1VacuumSealer = null;
                                        //}
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
                                    }
                                    // VS1VacuumSealer.SelectAndConfirmProgram(int.Parse(SEALER_RECIPE), InnovacVacuumSealer.SealerBar.Right);
                                    if (networkmain.FindSealerReceipeForSealer1(Station5ForSealer1Scanboxid) == true)
                                    {

                                        short sealerReceipe = networkmain.SealerReceipt1;
                                        Sealer1Log.Info("Sealer1 send receipt " + sealerReceipe + "'" + Station5ForSealer1Scanboxid);
                                        networkmain.stn5log = Station5ForSealer1Scanboxid + " slr.1 send receipt " + sealerReceipe;
                                        VS1VacuumSealer.SelectAndConfirmProgram(sealerReceipe, InnovacVacuumSealer.SealerBar.Right);
                                        //bool type want
                                        PLCWriteCommand6[PLCWriteCommand_DM5310] = 0x08;
                                      //  evnt_FindFinishingLabelForSealer1.Reset();


                                    //// Wait until the sealing is completed/ Error returned/ Timeout happened.
                                    InnovacVacuumSealer.SealingCompleteMessage sealingCompleteMessage;
                                    try {
                                      VS1VacuumSealer.WaitSealingCompleted(out sealingCompleteMessage,120000);
                                      // Sealing Completed successfully
                                      Sealer1Log.Info(sealingCompleteMessage.ToString());
                                      PLCWriteCommand6[PLCWriteCommand_DM5310] = 0x09;


                                            #region

                                            try
                                      {
                                        //FL 1
                                        
                                        pNumber1=sealingCompleteMessage.ProgramNumber; //3


                                        if(pNumber1>0)

                                        {
                                       // InnovacVacuumSealer.SealerBar SBar=sealingCompleteMessage.SelectedSealerBar;
                                       // InnovacVacuumSealer.MessageCode Com   =sealingCompleteMessage.Command;
                                        
                                         short vacuumPV=sealingCompleteMessage.VacuumPV; //ActualVacuumReading 4
                                         short VacuumSP=sealingCompleteMessage.VacuumSP;//RecipeVacuumReading 5
                                         short SCurrentPV=sealingCompleteMessage.SealerCurrentPV;//ActualCurrentReading 6
                                         short SCurrentSP=sealingCompleteMessage.SealerCurrentSP;//RecipeCurrentReading 7
                                          float sealingTime=sealingCompleteMessage.SealingTime; //8
                                          string  Sfunction=sealingCompleteMessage.UsedFunction.ToString(); //9


                                          abc = Sealer1IDshow.Trim();
                                          Sealer1IDshow = abc;
                                          if (Sfunction == "VT")
                                          {

                                              Sfunction = "VT";

                                          }
                                          if (Sfunction == "VS")
                                          {

                                              Sfunction = "VS";

                                          }
                                          if (Sfunction == "VT,VS")
                                          {

                                              Sfunction = "VTVS";

                                          }



                                          networkmain.Client_SendEventMessageForSealer1("503", Station5ForSealer1Scanboxid, Sealer1IDshow, sealingCompleteMessage.ProgramNumber, sealingCompleteMessage.VacuumPV, sealingCompleteMessage.VacuumSP, sealingCompleteMessage.SealerCurrentPV, sealingCompleteMessage.SealerCurrentSP, sealingCompleteMessage.SealingTime, Sfunction);




                                          Sealer1Log.Info("Sealer1 send SealerInfo to Middleware " + Station5ForSealer1Scanboxid + "," + Sealer1IDshow + "," + sealingCompleteMessage.ProgramNumber + "," + sealingCompleteMessage.VacuumPV + "," + sealingCompleteMessage.VacuumSP + "," + sealingCompleteMessage.SealerCurrentPV + "," + sealingCompleteMessage.SealerCurrentSP + "," + sealingCompleteMessage.SealingTime + "," + Sfunction);

                                        }




                                        }

                                      catch
                                      {}

                                      #endregion




                                    } catch(TimeoutException ex) {
                                      // Timeout Happened
                                      Sealer1Log.Error(ex.ToString());
                                       abc = Sealer1IDshow.Trim();
                                      MyEventQ.AddQ("8;SealerCommunicationBreak;SealerID;" + abc);
                                      EvtLog.Info("8;SealerCommunicationBreak;SealerID;" + abc);

                                    } catch(InnovacVacuumSealer.SealingErrorException ex) {
                                            // Sealer return Error
                                            if (ex.ErrorCode == 2 || ex.ErrorCode == 3 || ex.ErrorCode == 7)
                                            {
                                                PLCWriteCommand6[PLCWriteCommand_DM5310] = 0xFF;
                                            }
                                            else
                                            {
                                                PLCWriteCommand6[PLCWriteCommand_DM5310] = 0x09;
                                            }
                                            if (!Sealer1ErrAtv)
                                            {
                                                Sealer1ErrorCode = "E" + (5600 + ex.ErrorCode).ToString();
                                                Sealer1EventMesg = ex.ErrorMessage;
                                                Sealer1ErrAtv = true;
                                                ExceptionMesgSend1( (5600 + ex.ErrorCode).ToString(), Sealer1IDshow.Trim(), ex.ErrorMessage, ex.ToString());
                                                Sealer1ErrResetTimer.Interval = TimeSpan.FromSeconds(10);
                                                Sealer1ErrResetTimer.Tick += Sealer1ErrResetTimer_Tick;
                                                Sealer1ErrResetTimer.Start(); //Start the bloody timer

                                            }
                                            else
                                            {
                                                //Reset the timer
                                                Sealer1ErrResetTimer.Stop();
                                                Sealer1ErrResetTimer.Start();
                                            }
                                           
                                    }




                                 }
                                    //finishing label not match
                                    // Set Status for PLC to Complete 0x08
                                    else
                                    {
                                        Sealer1Log.Info("Sealer1 Finishing Label don't have in System" + ", " + Station5ForSealer1Scanboxid);
                                        networkmain.stn5log = Station5ForSealer1Scanboxid + " slr.1 Finishing Label not in system";
                                        networkmain.OperatorLog = "Stn.5 dont have " + Station5ForSealer1Scanboxid + " in system";
                                        PLCWriteCommand6[PLCWriteCommand_DM5310] = 0x0F;



                                    }

                                }
                                catch (Exception ex)
                                {
                                    Sealer1Log.Error(ex.ToString());
                                    // Set status for PLC to Error 0x0F
                                    PLCWriteCommand6[PLCWriteCommand_DM5310] = 0x0F;
                                    Sealer1Log.Info("Sealer1 Finishing Label don't have in System" + "," + Station5ForSealer1Scanboxid);
                                    networkmain.stn5log = Station5ForSealer1Scanboxid + " slr.1 Finishing Label not in system";
                                    evnt_FindFinishingLabelForSealer1.Reset();
                                }
                                evnt_FindFinishingLabelForSealer1.Reset();
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
