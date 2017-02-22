
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
       public  short pNumber3;

        public void RunStation05Sealer3(object msgobj)
        {
            MainNetworkClass networkmain = (MainNetworkClass)msgobj;
            while (!bTerminate)
            {
                Thread.Sleep(100);
                try
                {

                
                    bool Sealerflag3 = false;

                    if (PLCTelnet2 != null)
                        if (PLCTelnet2.connected) {






                          #region  sealer program For Station5
                         


                            #region //Sealer3
                            byte[] tmparray4 = new byte[10];
                            //DM5090
                            Array.Copy(PLCQueryRx6, 191, tmparray4, 0, 10);
                            //convert array to string                        
                            Station5ForSealer3Scanboxid = System.Text.Encoding.Default.GetString(tmparray4);
                            if (Station5ForSealer3Scanboxid != "\0\0\0\0\0\0\0\0\0\0")
                            {
                               ScanboxidSt5S3=Station5ForSealer3Scanboxid;
                                Sealer3Log.Info("Finishing Label for Station 5 Sealer3 " + Station5ForSealer3Scanboxid);
                                networkmain.stn5log = Station5ForSealer3Scanboxid + " slr.3 Label recieved";
                                #region Desiccant Timmer
                                Thread.Sleep(10);
                                if ((PLCQueryRx6[PLCQueryRx_DM5111 + 16] == 0X07) && (PLCWriteCommand6[PLCWriteCommand_DM5405 + 4]) == 0x00) //D5119
                                {
                                    Sealer3Log.Info("Finishing Label for Station 5 Sealer3 PLC Send HIC/Desiccant stop Signal " + Station5ForSealer3Scanboxid);
                                    networkmain.stn5log = Station5ForSealer3Scanboxid + " Slr.3 stop Signal";
                                    if (Sealerflag3 == false)
                                    {
                                        try
                                        {
                                            Sealerflag3 = true;
                                            TimeSpan span = DateTime.Now.Subtract(DesiccantTimingMap[Station5ForSealer3Scanboxid]);
                                            double secs = span.TotalSeconds;
                                            Sealer3Log.Info("PC delete HIC/Desiccant Finishing Label for Station 5 Sealer3 " + Station5ForSealer3Scanboxid);
                                            networkmain.stn5log = Station5ForSealer3Scanboxid + " Slr.3 PC delete HIC/Desiccant label";
                                           // if (secs > 175) //NEED TO OPEN WHEN REAL TIME PRODUCTION
                                             if (secs > 300)
                                            {
                                                //Desiccant Expired. Reject the Part.
                                                //D5414=449
                                                MyEventQ.AddQ("9;Desiccant/HICTimeout;LotNumber;" + Station5ForSealer3Scanboxid);
                                                EvtLog.Info("9;Desiccant/HICTimeout;LotNumber;" + Station5ForSealer3Scanboxid);
                                                Statusst52="Fail";
                                                byte[] tmp7 = new byte[2];
                                                tmp7 = Encoding.ASCII.GetBytes("RJ");
                                                Array.Copy(tmp7, 0, PLCWriteCommand6, 449, 2);
                                                try
                                                {
                                                    while ((!networkmain.UpdateRJLabelst5S3(Station5ForSealer3Scanboxid, "RJ", "502") && !bTerminate))
                                                    {
                                                        Thread.Sleep(100);
                                                    }
                                                    Sealer3Log.Info(" HIC/Desiccant Timeout Finishing Label (RJ) for Station 5 Sealer3  " + Station5ForSealer3Scanboxid);
                                                    networkmain.stn5log = Station5ForSealer3Scanboxid + " slr.3 HIC/Desiccant Timeout";
                                                    networkmain.OperatorLog = "Stn.5 Slr.3 HIC/Desiccant Timeout " + Station5ForSealer3Scanboxid;


                                                   

                                                }
                                                catch (Exception ex)
                                                {
                                                    Sealer3Log.Error("station 5 Sealer3 HIC/Desiccant Fail" + ex);
                                                    networkmain.stn5log = "HIC/Desiccant Fail Slr.3";
                                                    networkmain.OperatorLog = "Stn.5 Slr.3 HIC/Desiccant Fail";
                                                    byte[] tmp71 = new byte[2];
                                                    tmp71 = Encoding.ASCII.GetBytes("RJ");
                                                    Array.Copy(tmp71, 0, PLCWriteCommand6, 449, 2);
                                                    Statusst52="Fail";

                                                    MyEventQ.AddQ("9;Desiccant/HICTimeout;LotNumber;" + Station5ForSealer3Scanboxid);
                                                    EvtLog.Info("9;Desiccant/HICTimeout;LotNumber;" + Station5ForSealer3Scanboxid);
                                                }
                                            }
                                            else
                                            {
                                                if (CheckeRJStatusForSealer3(Station5ForSealer3Scanboxid) == true)
                                                {
                                                    Sealer3Log.Info("HIC/Desiccant Pass Finishing Label for Station 5 Sealer3 " + Station5ForSealer3Scanboxid);
                                                    networkmain.stn5log = Station5ForSealer3Scanboxid + " slr.3 HIC/Desiccant Pass";
                                                   Statusst52="Pass";
                                                }
                                                else
                                                {

                                                              
                                            while ((!networkmain.UpdateRJLabelst5S3(Station5ForSealer3Scanboxid, "RJ", RJResultst5S3) && !bTerminate))
                                            {
                                                Thread.Sleep(100);
                                            }




                                                    byte[] tmp7 = new byte[2];
                                                    tmp7 = Encoding.ASCII.GetBytes("RJ");
                                                    Array.Copy(tmp7, 0, PLCWriteCommand6, 449, 2);
                                                    Sealer3Log.Info(" Finishing Label for Station 5 Sealer3 (RJ) because of other station " + Station5ForSealer3Scanboxid+" RJ Code,"+RJResultst5S3);
                                                    Statusst52="Fail";





                                                }
                                            }
                                            DesiccantTimingMap.Remove(Station5ForSealer3Scanboxid);
                                            (PLCWriteCommand6[PLCWriteCommand_DM5405 + 4]) = 0x07;
                                        }
                                        catch (Exception ex)
                                        {
                                            Sealer3Log.Error(ex.ToString());
                                            while ((!networkmain.UpdateRJLabel(Station5ForSealer3Scanboxid, "RJ", "507") && !bTerminate))
                                            {
                                                Thread.Sleep(100);
                                            }
                                            //D5414=449
                                            byte[] tmpS = new byte[2];
                                            tmpS = Encoding.ASCII.GetBytes("RJ");
                                            Array.Copy(tmpS, 0, PLCWriteCommand6, 449, 2);
                                            Sealer3Log.Info(" HIC/Desiccant Fail (RJ)Finishing Label for Sealer3 but send signal to PLC " + Station5ForSealer3Scanboxid+" RJ Code,507");
                                            networkmain.stn5log = Station5ForSealer3Scanboxid + " slr.3 HIC/Desiccant fail but sent signal to PLC because of st3";
                                            networkmain.OperatorLog = "Stn.5 slr.3 " + Station5ForSealer3Scanboxid + "HIC/Desiccant fail because of st3";
                                            (PLCWriteCommand6[PLCWriteCommand_DM5405 + 4]) = 0x07;
                                            Statusst52="Fail";
                                        }
                                    }
                                }
                                #endregion
                                if (PLCWriteCommand6[PLCWriteCommand_DM5310 + 4] == 0x00)
                                {
                                    PLCWriteCommand6[PLCWriteCommand_DM5310 + 4] = 0x04; //busy Sealer3
                                    evnt_FindFinishingLabelForSealer3.Set();
                                    Sealer3Log.Info("Finishing Label for Station 5 Sealer3 receipt send already " + Station5ForSealer3Scanboxid);
                                    networkmain.stn5log = Station5ForSealer3Scanboxid + " slr.3 receipt sent already";
                                }
                            }
                            else
                            {
                                PLCWriteCommand6[PLCWriteCommand_DM5310 + 4] = 0x00;
                                PLCWriteCommand6[PLCWriteCommand_DM5414] = 0x00;
                                PLCWriteCommand6[PLCWriteCommand_DM5414b] = 0x00;
                                Station5ForSealer3Scanboxid = "\0\0\0\0\0\0\0\0\0\0";
                                Sealerflag3 = false;
                                pNumber3=0;
                                ScanboxidSt5S3="";
                               Statusst52="";
                            }
                            if (PLCQueryRx6[PLCQueryRx_DM5111 + 16] == 0X00) //D5119
                            {
                                (PLCWriteCommand6[PLCWriteCommand_DM5405 + 4]) = 0x00;
                            }
                            #endregion
                            
                            #endregion
                            #region Sealercomms
                          
                            if (evnt_FindFinishingLabelForSealer3.WaitOne(0))
                            {
                                try
                                {
                                    if (VS3VacuumSealer != null)
                                    {
                                        //if (!VS3VacuumSealer.IsOpen)
                                        //{
                                        try
                                        {
                                            VS3VacuumSealer.Close();
                                        }
                                        catch (Exception) { }
                                        VS3VacuumSealer = null;
                                        //}
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
                                    }
                                    //For test Only
                                    // VS3VacuumSealer.SelectAndConfirmProgram(240, InnovacVacuumSealer.SealerBar.Right);
                                    //PLCWriteCommand6[PLCWriteCommand_DM5310 + 4] = 0x08;
                                    if (networkmain.FindSealerReceipeForSealer3(Station5ForSealer3Scanboxid) == true)
                                    {
                                        short sealerReceipe = networkmain.SealerReceipt3;
                                        Sealer3Log.Info("Sealer3 send receipt" + sealerReceipe);
                                        VS3VacuumSealer.SelectAndConfirmProgram(sealerReceipe, InnovacVacuumSealer.SealerBar.Right);
                                        //bool type want
                                        PLCWriteCommand6[PLCWriteCommand_DM5310 + 4] = 0x08;
                                        // Set Status for PLC to Complete 0x08

                                      //// Wait until the sealing is completed/ Error returned/ Timeout happened.
                                    InnovacVacuumSealer.SealingCompleteMessage sealingCompleteMessage3;
                                    try {
                                      VS3VacuumSealer.WaitSealingCompleted(out sealingCompleteMessage3,120000);
                                      // Sealing Completed successfully
                                      Sealer3Log.Info(sealingCompleteMessage3.ToString());
                                      PLCWriteCommand6[PLCWriteCommand_DM5310 + 4] = 0x09;
                                       #region

                                            try
                                      {
                                        //FL 1
                                        
                                         pNumber3=sealingCompleteMessage3.ProgramNumber; //3


                                        if(pNumber3>0)

                                        {
                                       // InnovacVacuumSealer.SealerBar SBar=sealingCompleteMessage.SelectedSealerBar;
                                       // InnovacVacuumSealer.MessageCode Com   =sealingCompleteMessage.Command;
                                        
                                         short vacuumPV=sealingCompleteMessage3.VacuumPV; //ActualVacuumReading 4
                                         short VacuumSP=sealingCompleteMessage3.VacuumSP;//RecipeVacuumReading 5
                                         short SCurrentPV=sealingCompleteMessage3.SealerCurrentPV;//ActualCurrentReading 6
                                         short SCurrentSP=sealingCompleteMessage3.SealerCurrentSP;//RecipeCurrentReading 7
                                          float sealingTime=sealingCompleteMessage3.SealingTime; //8
                                          string  Sfunction=sealingCompleteMessage3.UsedFunction.ToString(); //9

                                          string abc = Sealer3IDshow.Trim();
                                          Sealer3IDshow = abc;

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


                                          networkmain.Client_SendEventMessageForSealer3("503", Station5ForSealer3Scanboxid, Sealer3IDshow, sealingCompleteMessage3.ProgramNumber, sealingCompleteMessage3.VacuumPV, sealingCompleteMessage3.VacuumSP, sealingCompleteMessage3.SealerCurrentPV, sealingCompleteMessage3.SealerCurrentSP, sealingCompleteMessage3.SealingTime, Sfunction);

                                          Sealer3Log.Info("Sealer3 send SealerInfo to Middleware " + Station5ForSealer3Scanboxid + "," + Sealer3IDshow + "," + sealingCompleteMessage3.ProgramNumber + "," + sealingCompleteMessage3.VacuumPV + "," + sealingCompleteMessage3.VacuumSP + "," + sealingCompleteMessage3.SealerCurrentPV + "," + sealingCompleteMessage3.SealerCurrentSP + "," + sealingCompleteMessage3.SealingTime + "," + Sfunction);

                                        }




                                        }

                                      catch
                                      {}

                                      #endregion












                                    } catch(TimeoutException ex) {
                                      // Timeout Happened
                                      Sealer3Log.Error(ex.ToString());
                                      string abc = Sealer3IDshow.Trim();
                                      MyEventQ.AddQ("8;SealerCommunicationBreak;SealerID;" + abc);
                                      EvtLog.Info("8;SealerCommunicationBreak;SealerID;" + abc);


                                    } catch(InnovacVacuumSealer.SealingErrorException ex) {
                                            // Sealer return Error
                                            if (ex.ErrorCode == 2 || ex.ErrorCode == 3 || ex.ErrorCode == 7)
                                            {
                                                PLCWriteCommand6[PLCWriteCommand_DM5310 + 4] = 0xFF;
                                            }
                                            else
                                            {
                                                PLCWriteCommand6[PLCWriteCommand_DM5310 + 4] = 0x09;
                                            }                              
                                            if (!Sealer3ErrAtv)
                                            {
                                                Sealer3ErrorCode = "E" + (5660 + ex.ErrorCode).ToString();
                                                Sealer3EventMesg = ex.ErrorMessage;
                                                Sealer3ErrAtv = true;
                                                ExceptionMesgSend3((5660 + ex.ErrorCode).ToString(), Sealer3IDshow.Trim(), ex.ErrorMessage, ex.ToString());
                                                Sealer3ErrResetTimer.Interval = TimeSpan.FromSeconds(10);
                                                Sealer3ErrResetTimer.Tick += Sealer3ErrResetTimer_Tick;
                                                Sealer3ErrResetTimer.Start(); //Start the bloody timer

                                            }
                                            else
                                            {
                                                //Reset the timer
                                                Sealer3ErrResetTimer.Stop();
                                                Sealer3ErrResetTimer.Start();
                                            }
                                        }






                                    }
                                    //finishing label not match
                                    else
                                    {
                                        Sealer3Log.Info("Sealer3 Finishing Label don't have in System" + "," + Station5ForSealer3Scanboxid);
                                        PLCWriteCommand6[PLCWriteCommand_DM5310 + 4] = 0x0F;

                                    }


                                }
                                catch (Exception ex)
                                {
                                    Sealer3Log.Error(ex.ToString());
                                    // Set status for PLC to Error 0x0F
                                    PLCWriteCommand6[PLCWriteCommand_DM5310 + 4] = 0x0F;
                                    Sealer3Log.Info("Sealer3 Finishing Label don't have in System" + "," + Station5ForSealer3Scanboxid);
                                }
                                evnt_FindFinishingLabelForSealer3.Reset();
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
