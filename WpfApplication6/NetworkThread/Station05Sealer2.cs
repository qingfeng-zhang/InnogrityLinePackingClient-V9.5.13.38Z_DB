
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
       public  short pNumber2; 
        

        public void RunStation05Sealer2(object msgobj)
        {
            MainNetworkClass networkmain = (MainNetworkClass)msgobj;
            while (!bTerminate)
            {
                Thread.Sleep(100);
                try
                {

                    bool Sealerflag2 = false;
                   

                    if (PLCTelnet2 != null)
                        if (PLCTelnet2.connected) {





                          #region  sealer program For Station5
                         

                            #region  //Sealer2
                            byte[] tmparray3 = new byte[10];
                            //DM5085
                            Array.Copy(PLCQueryRx6, 181, tmparray3, 0, 10);
                            //convert array to string                        
                            Station5ForSealer2Scanboxid = System.Text.Encoding.Default.GetString(tmparray3);
                            if (Station5ForSealer2Scanboxid != "\0\0\0\0\0\0\0\0\0\0")
                            {
                               ScanboxidSt5S2=Station5ForSealer2Scanboxid;
                                Sealer2Log.Info("Finishing Label for Station 5 Sealer2 " + Station5ForSealer2Scanboxid);
                                networkmain.stn5log = Station5ForSealer2Scanboxid + " slr. 2 recieved";
                                #region Desiccant Timmer Sealer2
                                Thread.Sleep(10);
                                if ((PLCQueryRx6[PLCQueryRx_DM5111 + 14] == 0X07) && (PLCWriteCommand6[PLCWriteCommand_DM5405 + 2]) == 0x00) //D5117
                                {
                                    Sealer2Log.Info("Finishing Label for Station 5 Sealer2 PLC Send HIC/Desiccant stop Signal " + Station5ForSealer2Scanboxid);
                                    networkmain.stn5log = Station5ForSealer2Scanboxid + " slr.2 PLC Send HIC/Desiccant stop Signal";

                                    if (Sealerflag2 == false)
                                    {

                                        try
                                        {
                                            Sealerflag2 = true;
                                            TimeSpan span = DateTime.Now.Subtract(DesiccantTimingMap[Station5ForSealer2Scanboxid]);
                                            Sealer2Log.Info("PC delete HIC/Desiccant Finishing Label for Station 5 Sealer2 " + Station5ForSealer2Scanboxid);
                                            networkmain.stn5log = Station5ForSealer2Scanboxid + " slr.2 deleted HIC/Desiccant";
                                            double secs = span.TotalSeconds;
                                            // if (secs > 175) //NEED TO OPEN WHEN REAL TIME PRODUCTION
                                            if (secs > 300)
                                            {
                                                //Desiccant Expired. Reject the Part.
                                                Statusst51 = "Fail";
                                                MyEventQ.AddQ("9;Desiccant/HICTimeout;LotNumber;" + Station5ForSealer2Scanboxid);
                                                EvtLog.Info("9;Desiccant/HICTimeout;LotNumber;" + Station5ForSealer2Scanboxid);
                                                //D5413=447
                                                byte[] tmp = new byte[2];
                                                tmp = Encoding.ASCII.GetBytes("RJ");
                                                Array.Copy(tmp, 0, PLCWriteCommand6, 447, 2);


                                                try
                                                {
                                                    Sealer2Log.Info(" HIC/Desiccant Timeout Finishing Label(RJ) for Station 5 Sealer2 " + Station5ForSealer2Scanboxid);
                                                    networkmain.stn5log = Station5ForSealer2Scanboxid + " HIC/Desiccant Timeout slr.2";
                                                    networkmain.OperatorLog = "Stn.5 slr.2 " + Station5ForSealer2Scanboxid + "HIC/Desiccant Timeout";
                                                    while ((!networkmain.UpdateRJLabelst5S2(Station5ForSealer2Scanboxid, "RJ", "501") && !bTerminate))
                                                    {
                                                        Thread.Sleep(100);
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    Sealer2Log.Error("station 5 Sealer2 HIC/Desiccant Fail" + ex);
                                                    networkmain.stn5log = "Fail HIC/Des slr.2 EX";
                                                    networkmain.OperatorLog = "Stn.5 slr.2 HIC/Des Fail EX";
                                                    byte[] tmp1 = new byte[2];
                                                    tmp1 = Encoding.ASCII.GetBytes("RJ");
                                                    Array.Copy(tmp1, 0, PLCWriteCommand6, 447, 2);
                                                    Sealer2Log.Info("RJ" + Station5ForSealer2Scanboxid);
                                                    Statusst51 = "Fail";
                                                    MyEventQ.AddQ("9;Desiccant/HICTimeout;LotNumber;" + Station5ForSealer2Scanboxid);
                                                    EvtLog.Info("9;Desiccant/HICTimeout;LotNumber;" + Station5ForSealer2Scanboxid);

                                                }
                                            }
                                            else
                                            {
                                                if (CheckeRJStatusForSealer2(Station5ForSealer2Scanboxid) == true)
                                                {
                                                    Sealer2Log.Info(" HIC/Desiccant Pass Finishing Label for Station 5 Sealer2 " + Station5ForSealer2Scanboxid);
                                                    networkmain.stn5log = Station5ForSealer2Scanboxid + " slr.2 HIC/Desiccant Pass";
                                                    Statusst51 = "Pass";

                                                }
                                                else
                                                {


                                                    while ((!networkmain.UpdateRJLabelst5S2(Station5ForSealer2Scanboxid, "RJ", RJResultst5S2) && !bTerminate))
                                                    {
                                                        Thread.Sleep(100);
                                                    }




                                                    byte[] tmp = new byte[2];
                                                    tmp = Encoding.ASCII.GetBytes("RJ");
                                                    Array.Copy(tmp, 0, PLCWriteCommand6, 447, 2);
                                                    Sealer2Log.Info(" Finishing Label for Station 5 Sealer2 (RJ) because of other station " + Station5ForSealer2Scanboxid + " RJ Code," + RJResultst5S2);
                                                    Statusst51 = "Fail";

                                                }



                                            }


                                            DesiccantTimingMap.Remove(Station5ForSealer2Scanboxid);
                                            (PLCWriteCommand6[PLCWriteCommand_DM5405 + 2]) = 0x07;
                                        }
                                        catch (Exception ex)
                                        {
                                            Sealer2Log.Error(ex.ToString());
                                            while ((!networkmain.UpdateRJLabelst5S2(Station5ForSealer2Scanboxid, "RJ", "506") && !bTerminate))
                                            {
                                                Thread.Sleep(100);
                                            }
                                            Sealer2Log.Info(" HIC/Desiccant Fail Finishing Label for Sealer2 because of st3 (RJ) but send signal to PLC " + Station5ForSealer2Scanboxid + " RJ Code,506");
                                            networkmain.stn5log = Station5ForSealer2Scanboxid + " slr.2 fail but sent signal to PLC because of st3";
                                            networkmain.OperatorLog = "Stn.5 slr.2 " + Station5ForSealer2Scanboxid + " fail because of st3";
                                            //D5413=447
                                            byte[] tmp = new byte[2];
                                            tmp = Encoding.ASCII.GetBytes("RJ");
                                            Array.Copy(tmp, 0, PLCWriteCommand6, 447, 2);
                                            (PLCWriteCommand6[PLCWriteCommand_DM5405 + 2]) = 0x07;
                                            Statusst51 = "Fail";
                                        }

                                    }
                                }


                                //  //TEST NEED TO DELETE START
                                //if (PLCQueryRx6[PLCQueryRx_DM5111 + 14] == 0X07)
                                //{ (PLCWriteCommand6[PLCWriteCommand_DM5405 + 2]) = 0x07; }

                                ////TEST NEED TO DELETE START END

                                #endregion

                                if (PLCWriteCommand6[PLCWriteCommand_DM5310 + 2] == 0x00)
                                {
                                    PLCWriteCommand6[PLCWriteCommand_DM5310 + 2] = 0x04; //busy Sealer2
                                    evnt_FindFinishingLabelForSealer2.Set();
                                    Sealer2Log.Info("Finishing Label for Station 5 Sealer2 receipt send already " + Station5ForSealer2Scanboxid);
                                    networkmain.stn5log = Station5ForSealer2Scanboxid + " receipt sent";
                                }
                            }
                            else
                            {
                                PLCWriteCommand6[PLCWriteCommand_DM5310 + 2] = 0x00;
                                PLCWriteCommand6[PLCWriteCommand_DM5413] = 0x00;
                                PLCWriteCommand6[PLCWriteCommand_DM5413b] = 0x00;
                                Station5ForSealer2Scanboxid = "\0\0\0\0\0\0\0\0\0\0";
                                Sealerflag2 = false;
                                ScanboxidSt5S2="";
                                 pNumber2=0;
                                Statusst51="";
                            }

                            if (PLCQueryRx6[PLCQueryRx_DM5111 + 14] == 0X00) //D5118
                            {
                                (PLCWriteCommand6[PLCWriteCommand_DM5405 + 2]) = 0x00;
                            }

                            #endregion

                     
                            
                            #endregion
                            #region Sealercomms
                           
                            if (evnt_FindFinishingLabelForSealer2.WaitOne(0))
                            {
                                try
                                {
                                    if (VS2VacuumSealer != null)
                                    {
                                        //if (!VS2VacuumSealer.IsOpen)
                                        //{
                                        try
                                        {
                                            VS2VacuumSealer.Close();
                                        }
                                        catch (Exception) { }
                                        VS2VacuumSealer = null;
                                        //}
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
                                    }
                                    //for testing only
                                    //VS2VacuumSealer.SelectAndConfirmProgram(240, InnovacVacuumSealer.SealerBar.Right);
                                    //PLCWriteCommand6[PLCWriteCommand_DM5310 + 2] = 0x08;
                                    #region
                                    if (networkmain.FindSealerReceipeForSealer2(Station5ForSealer2Scanboxid) == true)
                                    {
                                        short sealerReceipe = networkmain.SealerReceipt2;
                                        Sealer2Log.Info("Sealer2 send receipt " + sealerReceipe + "'" + Station5ForSealer2Scanboxid);
                                        VS2VacuumSealer.SelectAndConfirmProgram(sealerReceipe, InnovacVacuumSealer.SealerBar.Right);
                                        //bool type want
                                        PLCWriteCommand6[PLCWriteCommand_DM5310 + 2] = 0x08;
                                        // Set Status for PLC to Complete 0x08



                                      //// Wait until the sealing is completed/ Error returned/ Timeout happened.
                                    InnovacVacuumSealer.SealingCompleteMessage sealingCompleteMessage2;
                                    try {
                                      VS2VacuumSealer.WaitSealingCompleted(out sealingCompleteMessage2,120000);
                                      // Sealing Completed successfully
                                      Sealer2Log.Info(sealingCompleteMessage2.ToString());
                                      PLCWriteCommand6[PLCWriteCommand_DM5310 + 2] = 0x09;
                                      #region

                                            try
                                      {
                                        //FL 1
                                        
                                        pNumber2=sealingCompleteMessage2.ProgramNumber; //3


                                        if(pNumber2>0)

                                        {
                                       // InnovacVacuumSealer.SealerBar SBar=sealingCompleteMessage.SelectedSealerBar;
                                       // InnovacVacuumSealer.MessageCode Com   =sealingCompleteMessage.Command;
                                        
                                         short vacuumPV=sealingCompleteMessage2.VacuumPV; //ActualVacuumReading 4
                                         short VacuumSP=sealingCompleteMessage2.VacuumSP;//RecipeVacuumReading 5
                                         short SCurrentPV=sealingCompleteMessage2.SealerCurrentPV;//ActualCurrentReading 6
                                         short SCurrentSP=sealingCompleteMessage2.SealerCurrentSP;//RecipeCurrentReading 7
                                          float sealingTime=sealingCompleteMessage2.SealingTime; //8
                                          string  Sfunction=sealingCompleteMessage2.UsedFunction.ToString(); //9
                                          string abc = Sealer2IDshow.Trim();
                                          Sealer2IDshow = abc;

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




                                          networkmain.Client_SendEventMessageForSealer2("503", Station5ForSealer2Scanboxid, Sealer2IDshow, sealingCompleteMessage2.ProgramNumber, sealingCompleteMessage2.VacuumPV, sealingCompleteMessage2.VacuumSP, sealingCompleteMessage2.SealerCurrentPV, sealingCompleteMessage2.SealerCurrentSP, sealingCompleteMessage2.SealingTime, Sfunction);

                                          Sealer2Log.Info("Sealer2 send SealerInfo to Middleware " + Station5ForSealer2Scanboxid + "," + Sealer2IDshow + "," + sealingCompleteMessage2.ProgramNumber + "," + sealingCompleteMessage2.VacuumPV + "," + sealingCompleteMessage2.VacuumSP + "," + sealingCompleteMessage2.SealerCurrentPV + "," + sealingCompleteMessage2.SealerCurrentSP + "," + sealingCompleteMessage2.SealingTime + "," + Sfunction);


                                        }




                                        }

                                      catch
                                      {}

                                      #endregion









                                    } catch(TimeoutException ex) {
                                      // Timeout Happened
                                      Sealer2Log.Error(ex.ToString());
                                      string abc = Sealer2IDshow.Trim();
                                      MyEventQ.AddQ("8;SealerCommunicationBreak;SealerID;" + abc);
                                      EvtLog.Info("8;SealerCommunicationBreak;SealerID;" + abc);


                                       
                                    } catch(InnovacVacuumSealer.SealingErrorException ex) {
                                            // Sealer return Error
                                            if (ex.ErrorCode == 2 || ex.ErrorCode == 3 || ex.ErrorCode == 7)
                                            {
                                                PLCWriteCommand6[PLCWriteCommand_DM5310 + 2] = 0xFF;
                                            }
                                            else
                                            {
                                                PLCWriteCommand6[PLCWriteCommand_DM5310 + 2] = 0x09;
                                            }                        
                                            if (!Sealer2ErrAtv)
                                            {
                                                Sealer2ErrorCode = "E" + (5630 + ex.ErrorCode).ToString();
                                                Sealer2EventMesg = ex.ErrorMessage;
                                                Sealer2ErrAtv = true;
                                                ExceptionMesgSend2( (5630 + ex.ErrorCode).ToString(), Sealer2IDshow.Trim(), ex.ErrorMessage, ex.ToString());
                                                Sealer2ErrResetTimer.Interval = TimeSpan.FromSeconds(10);
                                                Sealer2ErrResetTimer.Tick += Sealer2ErrResetTimer_Tick;
                                                Sealer2ErrResetTimer.Start(); //Start the bloody timer

                                            }
                                            else
                                            {
                                                //Reset the timer
                                                Sealer2ErrResetTimer.Stop();
                                                Sealer2ErrResetTimer.Start();
                                            }
                                        }







                                    }
                                    //finishing label not match
                                    else
                                    {
                                        PLCWriteCommand6[PLCWriteCommand_DM5310 + 2] = 0x0F;
                                        Sealer2Log.Info("Sealer2 Finishing Label don't have in System" + "," + Station5ForSealer2Scanboxid);

                                    }



                                    #endregion
                                  


                                }
                                catch (Exception ex)
                                {
                                    Sealer2Log.Error(ex.ToString());
                                    // Set status for PLC to Error 0x0F
                                    PLCWriteCommand6[PLCWriteCommand_DM5310 + 2] = 0x0F;
                                    Sealer2Log.Info("Sealer2 Finishing Label don't have in System" + "," + Station5ForSealer2Scanboxid);
                                    networkmain.stn5log = Station5ForSealer2Scanboxid + " slr.2 Finishing Label not in system";
                                }
                                evnt_FindFinishingLabelForSealer2.Reset();
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
