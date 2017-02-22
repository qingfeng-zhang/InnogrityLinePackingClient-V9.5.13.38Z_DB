using NLog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Xml;
using System.Net.NetworkInformation;
using IGTwpf;

namespace InnogrityLinePackingClient
{
    partial class NetworkThread
    {

      
      public void Station6_7_8(object msgobj)
        {

            string st8trackHotLotOld = "";

            MainNetworkClass networkmain = (MainNetworkClass)msgobj;
            PLCTelnet2 = new TelnetClient();

            PLCTelnet = new TelnetClient();
            #region Connection PLC1,PLC2,Micron Server
           
            while (!bTerminate)
            {
                try
                {
                  Thread.Sleep(100);
                    #region PLC 1
                    //PLC Read Write Cycle
                    if (PLCTelnet != null)
                        if (PLCTelnet.connected) {



                          #region


                          #endregion

                      

                        }
                  #endregion
                  #region PLC 2
                    if (PLCTelnet2.connected)
                    {
                       
                        
                         #region Station 7 ESD Buffer


                          try{


                           byte[] temp7ESD = new byte[8];
                            Array.Copy(PLCQueryRx6, 361, temp7ESD, 0, 8); // DM 5175 ~ DM 5179 PLC1
                            Station7ESDScanboxid = System.Text.Encoding.Default.GetString(temp7ESD); // change here
                            if (Station7ESDScanboxid != "\0\0\0\0\0\0\0\0")
                            {       
                                ESDFL=Station7ESDScanboxid;                               
                                byte[] str1 = barcodechangeposition(temp7ESD); //Twist barcode read data to make untwist eg.UGHL,GULH
                                string st = System.Text.Encoding.Default.GetString(str1);
                                st7ESDtrack = st.Trim();
                                Station7Log.Info("Station 7 ESD Tracking Label " + st7ESDtrack);                          
                              // UpdatePLCFinishingLabelDMAddressFor7ESD(Station7ESDScanboxid);
                                 CheckStringUpdateFor7ESD(Station7ESDScanboxid);

                            }
                          else
                            {
                                                       
                                                         

                                CheckstringClearFor7ESD(461, Station7ESDScanboxid);// D5420
                                Station7ESDScanboxid = "\0\0\0\0\0\0\0\0";
                              //  Station7Log.Info("CLEAR FL st7" + Station7ESDScanboxid);
                                st7ESDtrack = "";
                                ESDFL="";
                                PrintStatusst7="";
                              
                            
                            }
                            #region pre Printing


                            
                              
                             if ((PLCQueryRx6[PLCQueryRx_DM5111] == 0x01)
                                &&
                                (!evt_Station07ESDPrintReq.WaitOne(0))
                                &&
                                (PLCWriteCommand6[PLCWriteCommand_DM5320] == 0x00) && (PLCQueryRx6[PLCQueryRx_DM5111 + 2] == 0x01))
                            {
                                PLCWriteCommand6[PLCWriteCommand_DM5320] = 0x4; //set busy signal
                               
                               
                                 //ST07Rotatary_Str =  ST07Rotatary_StrESD;
                                 evt_Station07ESDPrintReq.Set();
                                Station7Log.Info("ST7 ESD request printing:" + ST07Rotatary_StrESD);
                            }
                           
                            if ((PLCQueryRx6[PLCQueryRx_DM5111] == 0x1)//vision inspection ready
                                 &&
                                (PLCWriteCommand6[PLCWriteCommand_DM5320] == 0x05)
                                &&
                                (!evt_Station07ESDPrintReq.WaitOne(0))//assumming data had been send to vision also
                                )
                            {
                                PLCWriteCommand6[PLCWriteCommand_DM5320] = 0x06;//print send complete 
                                PrintStatusst7="Received";
                               // PrintStatus = "print send complete";
                              //  networkmain.FirstLabelPrint=true;
                              //if(networkmain.NumPrintLabel==1)
                              //{
                              //networkmain.FirstLabelPrint=false;
                              //}
                            }

                             

                           #endregion


                          }

                                     catch (Exception ex)
                          {
                              Log1.Info(ex.ToString());
                          }



                            #endregion


                            #region Station 7

                          try{

                            byte[] temp7 = new byte[8];
                            Array.Copy(PLCQueryRx6, 251, temp7, 0, 8); // DM 5120 ~ DM 5129 PLC1
                            Station7ForTransferScanboxidFromPLC1 = System.Text.Encoding.Default.GetString(temp7); // change here
                            if (Station7ForTransferScanboxidFromPLC1 != "\0\0\0\0\0\0\0\0")
                            {
                                ST7NewFLRev = true;
                                // st7track = Station7ForTransferScanboxidFromPLC1;
                                CheckStringUpdateFor7(Station7OFFSET, Station7ForTransferScanboxidFromPLC1);
                                //UpdatePLCFinishingLabelDMAddressFor71(Station7ForTransferScanboxidFromPLC1);
                                byte[] str1 = barcodechangeposition(temp7); //Twist barcode read data to make untwist eg.UGHL,GULH
                                string st = System.Text.Encoding.Default.GetString(str1);
                                st7track = st.Trim();
                                Station7Log.Info("Station 7 Tracking Label " + st7track);
                                #region RJ status update
                                //need lock 
                                //DM5144=299
                                byte[] tmparrayER7 = new byte[2];
                                Array.Copy(PLCQueryRx6, 299, tmparrayER7, 0, 2);
                                //convert Byte array to int                 
                                 Int32    er7 = (Int32)(BitConverter.ToInt16(tmparrayER7, 0));
                                if (er7 > 0)
                                {
                                    //DM5325=271
                                    byte[] tmp = new byte[2];
                                    tmp = Encoding.ASCII.GetBytes("RJ");
                                    Array.Copy(tmp, 0, PLCWriteCommand6, 271, 2);//may need to be below the exception
                                    string rj = "RJ";
                                    try
                                    {
                                        while ((!networkmain.UpdateRJLabelForTrackingLabel(Station7ForTransferScanboxidFromPLC1, rj, er7.ToString()) && !bTerminate))
                                        {
                                            Thread.Sleep(100);
                                        }
                                    }
                                    catch
                                    {
                                        byte[] tmp1 = new byte[2];
                                        tmp1 = Encoding.ASCII.GetBytes("RJ");
                                        Array.Copy(tmp1, 0, PLCWriteCommand6, 271, 2);
                                        //update fail..
                                        // reply to PLC update fail
                                        // may need this other time
                                    }
                                    Station7Log.Info("Station 7 reject code " + er7 + ", " + ST07Rotatary_Str1 + ", " + st7track);
                                    networkmain.stn7log = ST07Rotatary_Str1 + " RJ code " + er7 + ", " + st7track;
                                }
                                #endregion
                            }
                            else
                            {
                                ST7NewFLRev = false;
                                CheckstringClearFor7(Station7OFFSET, Station7ForTransferScanboxidFromPLC1); // data in DM, clear Offset
                                Station7ForTransferScanboxidFromPLC1 = "\0\0\0\0\0\0\0\0";
                                st7track = "";
                                st7Flabel="";
                              // PrintStatusst7="";
                               PrintStatusst71="";
                               PrintStatusst72="";
                               PrintStatusst73="";
                               VisionStatusst7="";
                               VisionStatusst71="";
                               VisionStatusst72="";
                               VisionStatusst73="";
                            }




                            if ((PLCQueryRx6[PLCQueryRx_DM5111] == 0x00) && (PLCQueryRx6[PLCQueryRx_DM5111 + 2] == 0x00))
                            {
                                //   PLCWriteCommand6[PLCWriteCommand_DM5320] = 0x00;
                                PLCWriteCommand6[PLCWriteCommand_DM5320] = 0x00;
                                PLCWriteCommand6[PLCWriteCommand_DM5326] = 0x00;
                                PLCWriteCommand6[PLCWriteCommand_DM5327] = 0x00;
                                PLCWriteCommand6[PLCWriteCommand_DM5328] = 0x00;
                            }
                            if (PLCQueryRx6[PLCQueryRx_DM5111] == 0x0F)
                            {
                                PLCWriteCommand6[PLCWriteCommand_DM5320] = 0x0F;
                            }
                            //////////////////////////////////
                            /////TEST PRINT FUNTION//////////
                            /////////////////////////////////
                            if ((PLCQueryRx6[PLCQueryRx_DM5111] == 0x99)
                               &&
                               (!evt_Station07PrintReq.WaitOne(0))
                               &&
                               (PLCWriteCommand6[PLCWriteCommand_DM5320] == 0x00))
                            {
                                PLCWriteCommand6[PLCWriteCommand_DM5320] = 0x4; //set busy signal

                                ZebraTestPrint zbt = new ZebraTestPrint();
                                bool Printok = zbt.ChecknLoadZPLForTestPrint(7);
                                if (Printok)
                                {
                                    MyEventQ.AddQ("82;PrinterTestPrint;PrinterNumber;7");//Push message to stack
                                    EvtLog.Info("82;PrinterTestPrint;PrinterNumber;7");
                                }
                                else
                                {
                                    MyEventQ.AddQ("83;PrinterCommunicationBreak;Stationnumber;7");//Push message to stack
                                }
                                PLCWriteCommand6[PLCWriteCommand_DM5320] = 0x99; //set ok signal
                                zbt = null;
                            }
                            if ((PLCQueryRx6[PLCQueryRx_DM5111] == 0x01)
                                &&
                                (!evt_Station07PrintReq.WaitOne(0))
                                &&
                                (PLCWriteCommand6[PLCWriteCommand_DM5320] == 0x00) && ( (PLCQueryRx6[PLCQueryRx_DM5111 + 2] == 0x02)|| (PLCQueryRx6[PLCQueryRx_DM5111 + 2] == 0x03)|| (PLCQueryRx6[PLCQueryRx_DM5111 + 2] == 0x04)))
                            {
                                PLCWriteCommand6[PLCWriteCommand_DM5320] = 0x4; //set busy signal

                                if (ST07Rotatary_Str1.Trim().Length == 0)
                                {
                                    log.Error("ST7 FL label is empty when requesting print");
                                }
                                else if (ST07Rotatary_Str1 == "\0\0\0\0\0\0\0\0\0\0" )
                                {
                                    log.Error("ST7 FL label is \0\0\0\0\0\0\0\0\0\0 when requesting print");
                                }
                                else
                                {
                                    ST07Rotatary_Str = ST07Rotatary_Str1;
                                    evt_Station07PrintReq.Set();
                                    log.Info("ST7 request printing:" + ST07Rotatary_Str);
                                }
                            }
                           
                            if ((PLCQueryRx6[PLCQueryRx_DM5111] == 0x1)//vision inspection ready
                                 &&
                                (PLCWriteCommand6[PLCWriteCommand_DM5320] == 0x05)
                                &&
                                (!evt_Station07PrintReq.WaitOne(0))//assumming data had been send to vision also
                                )
                            {
                                  
                                //networkmain.FirstLabelPrint=false;
                               // PrintStatus = "print send complete";
                               if(PLCQueryRx6[PLCQueryRx_DM5111 + 2] == 0x02)
                              {
                               PrintStatusst71="Received";
                              
                              }
                               if(PLCQueryRx6[PLCQueryRx_DM5111 + 2] == 0x03)
                              {
                               PrintStatusst72="Received";
                              
                              }


                               if(PLCQueryRx6[PLCQueryRx_DM5111 + 2] == 0x04)
                              {
                               PrintStatusst73="Received";
                              
                              }
                         PLCWriteCommand6[PLCWriteCommand_DM5320] = 0x06;//print send complete 



                            }

                        

                          
                           if (PLCQueryRx6[PLCQueryRx_DM5111 + 8] == 0x0)
                            {
                                PLCWriteCommand6[PLCWriteCommand_DM5402] = 0x00;//vision ready signal DM5402.. get the number from Pon to
                            }
                            // close
                            if ((PLCQueryRx6[PLCQueryRx_DM5111 + 8] == 0x2)//vision inspection trigger //state 2 dm5115
                                &&
                                (PLCWriteCommand6[PLCWriteCommand_DM5402] == 0x00)//vision ready signal DM5402.. get the number from Pon to correct his
                                &&
                                (!evt_Station07InspectionReq.WaitOne(0))
                                )
                            {

                               
                              this.networkmain.GetPrinterFilesBox(ST07Rotatary_Str1);
                              
                               if(PLCQueryRx6[PLCQueryRx_DM5111 + 10] == 0x01)
                              {
                               VisionStatusst7="Received";
                              
                              }
                             
                               if(PLCQueryRx6[PLCQueryRx_DM5111 + 10] == 0x02)
                              {
                               VisionStatusst71="Received";
                              
                              }
                               if(PLCQueryRx6[PLCQueryRx_DM5111 + 10] == 0x03)
                              {
                               VisionStatusst72="Received";
                              
                              }


                               if(PLCQueryRx6[PLCQueryRx_DM5111 + 10] == 0x04)
                              {
                               VisionStatusst73="Received";
                              
                              }


                                    evt_Station07InspectionReq.Set();
                              
                            }
                            //close

                            #region Sation 7 error code

                            byte[] tmparrayERst7 = new byte[2];
                            Array.Copy(PLCQueryRx6, 405, tmparrayERst7, 0, 2); //5197
                            //convert Byte array to int                 
                            Int32 erst7 = (Int32)(BitConverter.ToInt16(tmparrayERst7, 0));                          
                            ErrCode7 = erst7.ToString();

                            #region NewErrorCode

                            try
                            {
                                if (erst7 > 0)
                                {
                                    //  LogEr.Info("Station 7 Error Code" +ErrCode7);
                                    Errmessage7 = "Stn.7 Err " + ErrCode7 + ": " + Stn7ErrToMsg(erst7);


                                    if (!ST7JamFlag)
                                    {
                                        bool St7JamTrig = ST2PauseFunction(7, erst7 + ";"); //Check if is a JAM
                                        if (St7JamTrig)
                                        {
                                            ST7JamFlag = true;
                                            //string[] FLbatch = rq.UpdJamstatus(7, 777); //Update Jam FL
                                            //if (FLbatch != null)
                                            //{
                                            //    networkmain.Client_SendEventMsg("727", "Station7FLJAMRecovery", FLbatch);//Update Jam recovery FL to middleware
                                            //}
                                        }
                                    }





                                }
                                else
                                {
                                    ST7JamFlag = false;
                                    Errmessage7 = String.Empty;
                                }
                                #endregion
                                #region OldCode
                                //try
                                //{
                                //if (erst7 > 0)
                                //{
                                //    //  LogEr.Info("Station 7 Error Code" +ErrCode7);
                                //    Errmessage7 = "Stn.7 Err " + ErrCode7 + ": " + Stn7ErrToMsg(erst7);


                                //    // LogEr.Info(Errmessage7);



                                //}
                                //else
                                //{
                                //    Errmessage7 = String.Empty;
                                //}
                                    #endregion
                                    UpdateErrorMsg((int)Station.StationNumber.Station07, Errmessage7, ST7JamFlag);

                                if ((erst7 > 0) && networkmain.controlst7 == 0)
                                {
                                    Errst7 = erst7.ToString();
                                    networkmain.controlst7 = 1;
                                    messagest7 = Stn7ErrToMsg(erst7);
                                    networkmain.Client_SendAlarmMessage(erst7.ToString(), messagest7, "SET");
                                    LogEr.Info("Error Set st7 " + erst7.ToString() + messagest7);

                                }
                                if (erst7 == 0 && networkmain.controlst7 == 1)
                                {
                                    networkmain.Client_SendAlarmMessage(Errst7, messagest7, "CLEAR");
                                    networkmain.controlst7 = 0;
                                    LogEr.Info("Error CLEAR st7 " + Errst7 + messagest7);
                                    Errst7 = "";
                                    messagest7 = "";
                                }
                            }
                            catch
                            { 
                            
                            }
                            #endregion

                            #region Printer Clear

                            if ((PLCQueryRx6[PLCQueryRx_DM5145] == 0x01)
                                &&
                                (PLCWriteCommand6[PLCWriteCommand_DM5346] == 0x00)
                                &&
                                (!evt_Station07PrintClearReq.WaitOne(0))
                              )
                            {
                                PLCWriteCommand6[PLCWriteCommand_DM5346] = 0x4; //set busy signal
                                // ST04Rotatary_Str = ScanboxidSt4;
                                //show in table FL
                                //fire printer thread
                                Station7Log.Info("Station 7 Printing clear Start");
                                //Printer02Thread = new Thread(new ParameterizedThreadStart(Printer02Th));
                                //Printer02Thread.Start(networkmain);
                                evt_Station07PrintClearReq.Set();
                            }
                            if ((PLCQueryRx6[PLCQueryRx_DM5145] == 0x1)//vision inspection ready
                                 &&
                                (PLCWriteCommand6[PLCWriteCommand_DM5346] == 0x05)
                                &&
                                 (!evt_Station07PrintClearReq.WaitOne(0))//assumming data had been send to vision also
                                )
                            {
                                PLCWriteCommand6[PLCWriteCommand_DM5346] = 0x06;//print send complete    
                                //PrintStatus = "print send complete";
                            }
                            if (PLCQueryRx6[PLCQueryRx_DM5145] == 0x00)
                            {
                                PLCWriteCommand6[PLCWriteCommand_DM5346] = 0x00;
                            }

                            #endregion

                          }

                                     catch (Exception ex)
                          {
                              Log1.Info(ex.ToString());
                          }

                            #endregion

                            #region  Station 7 Reject Buffer
                          try
                          {

                            byte[] temp7BRJ = new byte[8];
                            Array.Copy(PLCQueryRx6, 341, temp7BRJ, 0, 8); // DM 5165=341 ~ DM 5169 PLC1
                            Station7ScanboxidForReject = System.Text.Encoding.Default.GetString(temp7BRJ); // change here
                            if (Station7ScanboxidForReject != "\0\0\0\0\0\0\0\0")
                            {
                                #region RJ status update

                                UpdatePLCFinishingLabelDMAddressFor71ForBuffer(Station7ScanboxidForReject);
                                //need lock 
                                //
                                byte[] tmparrayER7 = new byte[2];
                                Array.Copy(PLCQueryRx6, 351, tmparrayER7, 0, 2);//D5170=351
                                //convert Byte array to int                 
                                 er71 = (Int32)(BitConverter.ToInt16(tmparrayER7, 0));

                                 byte[] str1 = barcodechangeposition(temp7BRJ); //Twist barcode read data to make untwist eg.UGHL,GULH
                                 string st = System.Text.Encoding.Default.GetString(str1);
                                 st7trackForBuffer = st.Trim();



                                if (er71 == 701)
                                {
                                   
                                  //vision fail
                                    string rj = "RJ";
                                    
                                    try
                                    {
                                        while ((!networkmain.UpdateRJLabelForTrackingLabel(Station7ScanboxidForReject, rj, er71.ToString()) && !bTerminate))
                                        {
                                            Thread.Sleep(100);
                                        }
                                    }
                                    catch
                                    {
                                      
                                    }

                                   

                                  try
                                    {
                                        while ((!networkmain.UpdatePassLabelForst7TrackingLabel(Station7ScanboxidForReject, er71.ToString()) && !bTerminate))
                                        {
                                            Thread.Sleep(100);
                                        }
                                    }
                                    catch
                                    {
                                      
                                    }




                                    Station7Log.Info("Station 7 buffer reject code " + er71 + ", " + st7FlabelForBuffer + ", " + st7trackForBuffer);
                                    networkmain.stn7log = st7FlabelForBuffer + " RJ code at buffer " + er71 + ", " + st7trackForBuffer;

                                  PLCWriteCommand6[PLCWriteCommand_DM5351] = 0x0F;
                                  er71 = 0;

                                }


                                   if (er71 == 702)
                                {
                                    //need to send middleware ESD Fail
                                    string rj = "RJ";
                                  
                                    try
                                    {
                                        while ((!networkmain.UpdateRJLabelForTrackingLabel(Station7ScanboxidForReject, rj, er71.ToString()) && !bTerminate))
                                        {
                                            Thread.Sleep(100);
                                        }
                                    }
                                    catch
                                    {
                                        
                                    }




                                     try
                                    {
                                        while ((!networkmain.UpdatePassLabelForst7TrackingLabel(Station7ScanboxidForReject, er71.ToString()) && !bTerminate))
                                        {
                                            Thread.Sleep(100);
                                        }
                                    }
                                    catch
                                    {
                                      
                                    }

                                   

                                    Station7Log.Info("Station 7 buffer ESD Fail code " + er71 + ", " + st7FlabelForBuffer + ", " + st7trackForBuffer);
                                    networkmain.stn7log = st7FlabelForBuffer + " ESD Fail code at buffer " + er71 + ", " + st7trackForBuffer;

                                   PLCWriteCommand6[PLCWriteCommand_DM5351] = 0x0F;
                                   er71 = 0;

                                }



                                          if (er71 == 703)
                                {
                                    //need to send middleware ER Fail
                                    string rj = "RJ";
                                  
                                    try
                                    {
                                        while ((!networkmain.UpdateRJLabelForTrackingLabel(Station7ScanboxidForReject, rj, er71.ToString()) && !bTerminate))
                                        {
                                            Thread.Sleep(100);
                                        }
                                    }
                                    catch
                                    {
                                       
                                    }

                                   

                                            try
                                    {
                                        while ((!networkmain.UpdatePassLabelForst7TrackingLabel(Station7ScanboxidForReject, er71.ToString()) && !bTerminate))
                                        {
                                            Thread.Sleep(100);
                                        }
                                    }
                                    catch
                                    {
                                      
                                    }



                                    Station7Log.Info("Station 7 buffer ER Fail code " + er71 + ", " + st7FlabelForBuffer + ", " + st7trackForBuffer);
                                    networkmain.stn7log = st7FlabelForBuffer + " ER Fail code at buffer " + er71 + ", " + st7trackForBuffer;

                                   PLCWriteCommand6[PLCWriteCommand_DM5351] = 0x0F;
                                    er71 = 0;

                                }



                                          if (er71 == 704)
                                          {
                                              //need to sensor Fail
                                              string rj = "RJ";

                                              try
                                              {
                                                  while ((!networkmain.UpdateRJLabelForTrackingLabel(Station7ScanboxidForReject, rj, er71.ToString()) && !bTerminate))
                                                  {
                                                      Thread.Sleep(100);
                                                  }
                                              }
                                              catch
                                              {
                                                 
                                              }




                                            try
                                    {
                                        while ((!networkmain.UpdatePassLabelForst7TrackingLabel(Station7ScanboxidForReject, er71.ToString()) && !bTerminate))
                                        {
                                            Thread.Sleep(100);
                                        }
                                    }
                                    catch
                                    {
                                      
                                    }

                                              Station7Log.Info("Station 7 buffer sensor Fail code " + er71 + ", " + st7FlabelForBuffer + ", " + st7trackForBuffer);
                                              networkmain.stn7log = st7FlabelForBuffer + "sensor Fail code at buffer " + er71 + ", " + st7trackForBuffer;

                                               PLCWriteCommand6[PLCWriteCommand_DM5351] = 0x0F;
                                               er71 = 0;

                                          }




                                          if (er71 == 705)
                                          {
                                              //need to manual Fail
                                              string rj = "RJ";

                                              try
                                              {
                                                  while ((!networkmain.UpdateRJLabelForTrackingLabel(Station7ScanboxidForReject, rj, er71.ToString()) && !bTerminate))
                                                  {
                                                      Thread.Sleep(100);
                                                  }
                                              }
                                              catch
                                              {
                                                  
                                              }




                                            try
                                    {
                                        while ((!networkmain.UpdatePassLabelForst7TrackingLabel(Station7ScanboxidForReject, er71.ToString()) && !bTerminate))
                                        {
                                            Thread.Sleep(100);
                                        }
                                    }
                                    catch
                                    {
                                      
                                    }
                                              Station7Log.Info("Station 7 buffer manual Fail code " + er71 + ", " + st7FlabelForBuffer + ", " + st7trackForBuffer);
                                              networkmain.stn7log = st7FlabelForBuffer + "manual Fail code at buffer " + er71 + ", " + st7trackForBuffer;

                                                PLCWriteCommand6[PLCWriteCommand_DM5351] = 0x0F;
                                                er71 = 0;

                                          }

                                if (er71 == 706)
                                {
                                    //need to manual Fail
                                    string rj = "RJ";

                                    try
                                    {
                                        while ((!networkmain.UpdateRJLabelForTrackingLabel(Station7ScanboxidForReject, rj, er71.ToString()) && !bTerminate))
                                        {
                                            Thread.Sleep(100);
                                        }
                                    }
                                    catch
                                    {

                                    }




                                    try
                                    {
                                        while ((!networkmain.UpdatePassLabelForst7TrackingLabel(Station7ScanboxidForReject, er71.ToString()) && !bTerminate))
                                        {
                                            Thread.Sleep(100);
                                        }
                                    }
                                    catch
                                    {

                                    }
                                    Station7Log.Info("ST7 VISION RESULT TIMEOUT REJECT " + er71 + ", " + st7FlabelForBuffer + ", " + st7trackForBuffer);
                                    networkmain.stn7log = st7FlabelForBuffer + "ST7 VISION RESULT TIMEOUT REJECT " + er71 + ", " + st7trackForBuffer;

                                    PLCWriteCommand6[PLCWriteCommand_DM5351] = 0x0F;
                                    er71 = 0;

                                }

                                if (er71 ==999)
                                          {
                                              //need to manual Fail
                                              string rj = "RJ";

                                              try
                                              {
                                                  while ((!networkmain.UpdateRJLabelForTrackingLabel(Station7ScanboxidForReject, rj, er71.ToString()) && !bTerminate))
                                                  {
                                                      Thread.Sleep(100);
                                                  }
                                              }
                                              catch
                                              {
                                                  
                                              }




                                               try
                                    {
                                        while ((!networkmain.UpdatePassLabelForst7TrackingLabel(Station7ScanboxidForReject, er71.ToString()) && !bTerminate))
                                        {
                                            Thread.Sleep(100);
                                        }
                                    }
                                    catch
                                    {
                                      
                                    }


                                              Station7Log.Info("Station 7 buffer PURGE code " + er71 + ", " + st7FlabelForBuffer + ", " + st7trackForBuffer);
                                              networkmain.stn7log = st7FlabelForBuffer + "PURGE code at buffer " + er71 + ", " + st7trackForBuffer;

                                               PLCWriteCommand6[PLCWriteCommand_DM5351] = 0x0F;
                                               er71 = 0;

                                          }


                              else if   (er71 == 799)
                                {
                                

                                try
                                    {
                                        while ((!networkmain.UpdatePassLabelForst7TrackingLabel(Station7ScanboxidForReject, er71.ToString()) && !bTerminate))
                                        {
                                            Thread.Sleep(100);
                                        }
                                    }
                                    catch
                                    {
                                      
                                    }
                                 Station7Log.Info("Station 7 buffer pass code " + er71 + ", " + st7FlabelForBuffer + ", " + st7trackForBuffer);
                                    networkmain.stn7log = st7FlabelForBuffer + " pass code at buffer " + er71 + ", " + st7trackForBuffer;

                                  PLCWriteCommand6[PLCWriteCommand_DM5351] = 0x09;
                                  er71 = 0;
                                }

                                #endregion


                            }
                            else
                            {

                                PLCWriteCommand6[PLCWriteCommand_DM5351] = 0x00;
                                Station7ScanboxidForReject = "\0\0\0\0\0\0\0\0";
                               

                            }
                          }

                                     catch (Exception ex)
                          {
                              Log1.Info(ex.ToString());
                          }

                            #endregion



                          #region Checke printer Connection at ST7


                          Ping PingPrinter7 = new Ping();

                          try
                          {

                              if ((PLCQueryRx6[PLCQueryRx_DM5130] == 0x01)
                                  &&
                                  (PLCWriteCommand6[PLCWriteCommand_DM5427] == 0x00)
                                 )
                              {


                                  PingReply PR7 = PingPrinter7.Send("192.168.3.226");
                                  if (PR7.Status == IPStatus.Success)
                                  {
                                      PLCWriteCommand6[PLCWriteCommand_DM5427] = 0x09;
                                  }
                                  else if (PR7.Status == IPStatus.DestinationHostUnreachable)
                                  {
                                      PLCWriteCommand6[PLCWriteCommand_DM5427] = 0xFF;
                                  }

                              }


                              if (PLCQueryRx6[PLCQueryRx_DM5130] == 0x00)
                              {
                                  PLCWriteCommand6[PLCWriteCommand_DM5427] = 0x00;

                              }



                          }
                          catch
                          {


                          }







                          #endregion

                            #region Station 8

                          try
                          {
                            string OEEid8="";
                            byte[] temp8 = new byte[8];
                            #region Hotlot

                            Array.Copy(PLCQueryRx6, 31, temp8, 0, 8); // PLC2  DM5010 ---DM 5014 ---- 31
                            Station8ForHotLotBoxid = System.Text.Encoding.Default.GetString(temp8);
                            if (Station8ForHotLotBoxid != "\0\0\0\0\0\0\0\0")
                            {
                                byte[] str1 = barcodechangeposition(temp8);//Twist barcode read data to make untwist eg.UGHL,GULH
                                string st = System.Text.Encoding.Default.GetString(str1);
                                st8trackHotLot = st.Trim();
                                if (!st8trackHotLot.Equals (st8trackHotLotOld) )
                                {
                                    st8trackHotLotOld = st8trackHotLot;
                                    Station8Log.Info("Station 8 HotLot Tracking Label " + st8trackHotLot);
                                    networkmain.stn8log= "Station 8 HotLot Tracking Label " + st8trackHotLot;
                                   st8FLabelHotlot = UpdatePLCFinishingLabelDMAddressFor81(Station8ForHotLotBoxid);

                                    byte[] tmpbyte;

                                    try
                                    {
                                        XmlNode fltrackingroot = networkmain.FLTrackingdoc.DocumentElement;//this house the list of summarized xml details 
                                        XmlNode selectednode = fltrackingroot.SelectSingleNode("descendant::LABEL[TrackingLabel='" + Station8ForHotLotBoxid + "']");
                                        if (selectednode != null)
                                        {
                                            result = selectednode.SelectSingleNode("PackageStatus").InnerText;
                                            ERCode = selectednode.SelectSingleNode("ErrorCode").InnerText;
                                            resultCode = selectednode.SelectSingleNode("St7Result").InnerText;
                                            OEEid8 = selectednode.SelectSingleNode("OEEid").InnerText;
                                            //rq.UpdStNobyID(8, int.Parse(OEEid8));

                                        }
                                        else
                                        {
                                          
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Station8Log.Error(ex.ToString());
                                      
                                    }

                                    if (resultCode == "701" || resultCode == "702" || resultCode == "703" || resultCode == "704" || resultCode == "705" || resultCode == "706" || networkmain.PurgeON == true || resultCode == "999")
                                    {
                                        tmpbyte = new byte[2];
                                        tmpbyte[0] = 0x02; ;
                                        //5335=291   
                                        Array.Copy(tmpbyte, 0, PLCWriteCommand6, 223, 1);  //DM5301  =223
                                        Station8Log.Info("Station 8 TTracking Label and Finishing Label(Hotlot) Reject by ST7 " + st8FLabelHotlot + "," + resultCode);
                                        networkmain.stn8log = st8track + " Tracking Label and Finishing Label(Hotlot) Reject by ST7 " + st8FLabelHotlot + "," + resultCode;
                                    }
                                    else if (resultCode == "799")
                                    {

                                        tmpbyte = new byte[2];
                                        tmpbyte[0] = 0x01; ;
                                        //5335=291   
                                        Array.Copy(tmpbyte, 0, PLCWriteCommand6, 223, 1);  //DM5301  =223

                                        Station8Log.Info("Station 8 Finishing label and Tracking label OK for Hotlot: " + st8FLabelHotlot);
                                        networkmain.stn8log = "Station 8 Hotlot FL : " + st8FLabelHotlot + " Tracking Label:" + st8trackHotLot;
                                    }
                                    else
                                    {
                                        tmpbyte = new byte[2];
                                        tmpbyte[0] = 0x02; ;
                                        //5335=291   
                                        Array.Copy(tmpbyte, 0, PLCWriteCommand6, 223, 1);  //DM5301  =223
                                        Station8Log.Info("Station 8 Finishing label and Tracking label Exception for Hotlot: " + st8FLabelHotlot+","+ resultCode);
                                        networkmain.stn8log = "Station 8 Hotlot Exception for FL : " + st8FLabelHotlot + " Tracking Label:" + st8trackHotLot + "," + resultCode;
                                    }
                                }
                                else
                                {
                                    if (st8trackHotLotOld != "\0\0\0\0\0\0\0\0")
                                    {
                                        //clear previous
                                        networkmain.Client_sendFG01_FG02_MOVE(st8FLabelHotlot, "Station 8 Hotlot Label");
                                        networkmain.Client_sendAQL_BOX(st8FLabelHotlot);//Send SMS to Micron
                                        st8trackHotLotOld = Station8ForHotLotBoxid;
                                    }

                                }

                            }
                          

                            if (PLCQueryRx6[PLCQueryRx_DM5016] == 0x01)
                            {
                                PLCWriteCommand6[21 + 100 * 2] = 0x0;    //clear DM5300 
                                                                       
                            }
                            #endregion Hotlot
                            Array.Copy(PLCQueryRx6, 391, temp8, 0, 8); // DM 5190 ~ DM 5194 PLC1
                            Station8ForTransferScanboxid = System.Text.Encoding.Default.GetString(temp8);
                            if (Station8ForTransferScanboxid != "\0\0\0\0\0\0\0\0")
                            {

                                
                                byte[] str1 = barcodechangeposition(temp8);//Twist barcode read data to make untwist eg.UGHL,GULH
                                string st = System.Text.Encoding.Default.GetString(str1);
                                st8track = st.Trim();
                                Station8Log.Info("Station 8 Tracking Label "+st8track);
                                networkmain.stn8log = "Station 8 Tracking Label "+st8track;
                                st8FLabel = UpdatePLCFinishingLabelDMAddressFor81(Station8ForTransferScanboxid);

                                try
                                {

                                    XmlNode fltrackingroot = networkmain.FLTrackingdoc.DocumentElement;//this house the list of summarized xml details 
                                    XmlNode selectednode = fltrackingroot.SelectSingleNode("descendant::LABEL[TrackingLabel='" + Station8ForTransferScanboxid + "']");

                                    if (selectednode!=null)
                                    { 
                                    result = selectednode.SelectSingleNode("PackageStatus").InnerText;
                                    ERCode = selectednode.SelectSingleNode("ErrorCode").InnerText;
                                    resultCode = selectednode.SelectSingleNode("St7Result").InnerText;
                                    OEEid8 = selectednode.SelectSingleNode("OEEid").InnerText;
                                    //rq.UpdStNobyID(8, int.Parse(OEEid8));
                                    }

                                }
                                catch (Exception ex)
                                {
                                    Log1.Info(ex.ToString());
                                }



                                   if (resultCode == "701" || resultCode == "702" || resultCode == "703" || resultCode == "704" || resultCode == "705" || resultCode == "706" || networkmain.PurgeON == true || resultCode == "999")
                                {                                   
                                   // st8trackForAutotimeoutReject = st7FlabelForBuffer;                                
                                   
                                 CheckStringUpdateFor8(Station8OFFSET, Station8ForTransferScanboxid);
                               
                                Station8Log.Info("Station 8 Tracking Label Reject by ST7 " + st8track + "," + st8FLabel + "," +resultCode);
                                networkmain.stn8log = st8track + " Tracking Label and Finishing Label Reject by ST7 " + st8FLabel + "," +resultCode;                     
                                
                                }
                                 else  if   (resultCode == "799")
                                {

                                CheckStringUpdateFor8(Station8OFFSET, Station8ForTransferScanboxid);
                                st8FLabel = UpdatePLCFinishingLabelDMAddressFor81(Station8ForTransferScanboxid);
                                Station8Log.Info("Station 8 Tracking Label " + st8track + "," + st8FLabel+"," + resultCode);
                                networkmain.stn8log = st8track + " Tracking Label and Finishing Label " + st8FLabel+"," + resultCode;                           

                                }
                                
                                else
                              {
                                //  Station7Log.Info("Station 7 buffer NO code GIVE in ST8" + er71 + ", " + st7FlabelForBuffer + ", " + st7trackForBuffer+resultCode);
                               //   networkmain.stn7log = st7FlabelForBuffer + "ST7 NO code GIVE at  in ST8 " + er71 + ", " + st7trackForBuffer + resultCode;
                                  Station8Log.Info("Station 8 Tracking Label NO code GIVE by ST7 " + st8track + "," + st8FLabel + "," +resultCode);
                                //  networkmain.stn8log = st8track + " Tracking Label and Finishing Label NO code GIVE by ST7 " + st8FLabel +"," + resultCode;                                 

                              }                             




                                #region RJ status update
                                //need lock 
                                //DM5114=239
                                byte[] tmparrayER8 = new byte[2];
                                Array.Copy(PLCQueryRx6, 239, tmparrayER8, 0, 2);
                                //convert Byte array to int                 
                                Int32 er8 = (Int32)(BitConverter.ToInt16(tmparrayER8, 0));
                                if (er8 > 0)
                                {
                                    XmlNode fltrackingroot1 = networkmain.FLTrackingdoc.DocumentElement;//this house the list of summarized xml details 
                                    XmlNode selectednode1 = fltrackingroot1.SelectSingleNode("descendant::LABEL[TrackingLabel='" + Station8ForTransferScanboxid + "']");
                                    selectednode1.SelectSingleNode("PackageStatus").InnerText = "RJ";
                                    selectednode1.SelectSingleNode("ErrorCode").InnerText = er8.ToString();
                                    OEEid8 = selectednode1.SelectSingleNode("OEEid").InnerText;
                                    //DM5342=305
                                    byte[] tmp = new byte[2];
                                    tmp = Encoding.ASCII.GetBytes("RJ");
                                    Array.Copy(tmp, 0, PLCWriteCommand6, 305, 2);
                                    Station8Log.Info("Station 8 RJ " + er8 + "'" + st8track + "," + st8FLabel);
                                    networkmain.stn8log = Station8ForTransferScanboxid + " Rejected " + er8;
                                    networkmain.OperatorLog = "Stn.8 rejected " + Station8ForTransferScanboxid + " with " + er8;
                                }
                                #endregion

                                if ((PLCQueryRx6[PLCQueryRx_DM5113] == 0x0F) &&
                                    (PLCWriteCommand6[PLCWriteCommand_DM5349] == 0x00) &&
                                    (!evnt_RejectFinishingLabelForStation8.WaitOne(0)))//part placed at reject bin
                                {                                    
                                   //resultCode

                                   try{
                           

                                    string RJCODE=resultCode;
                                     int rj=int.Parse(RJCODE);
                                     if (rj > 0 && FLMembkp8 != Station8ForTransferScanboxid)
                                     {
                                         //int NewRJcode = 0;
                                         XmlDocument doc = new XmlDocument();
                                         doc.Load(@"ConfigEvent.xml");
                                         XmlNode node = doc.SelectSingleNode(@"/EVENT/R" + RJCODE);
                                         string RJName = node.InnerText;
                                         XmlNode node2 = doc.SelectSingleNode(@"/EVENT/RK" + RJCODE);
                                         string NewRJcode = node2.InnerText;
                                            try
                                            {
                                                #region OEEStep7
                                                rq.UpdST8RJ(OEEid8, 8, RJCODE);
                                                #endregion
                                            }
                                            catch (Exception)
                                            {
                                                throw;
                                            }

                                            networkmain.Client_SendEventMessage(NewRJcode.ToString(), RJName, "BOX_ID", st8FLabel);
                                         Station8Log.Info("PLC2 Station 8 Send RJ Event to Middleware " + RJCODE + "," + RJName + "," + st8FLabel + "," + NewRJcode.ToString());
                                         AllRJEvent.Info("PLC2 Station 8 Send RJ Event to Middleware " + RJCODE + "," + RJName + "," + st8FLabel + "," + NewRJcode.ToString());
                                            FLMembkp8 = Station8ForTransferScanboxid;
                                        }



                                 }
                                 catch (Exception ex){                                    
                                 
                                                      }    


                                   //NEED TO SEND ACK MIDDLEWARE
                                    evnt_RejectFinishingLabelForStation8.Set();

                                    StationStarStopLog.Info("Running Stop,Result Fail " + st8track + "," + st8FLabel + resultCode);
                                    networkmain.stn8log = st8track + "," + st8FLabel + " Stopped ";
                                    networkmain.OperatorLog = "Stn.8 Stopped/Failed " + st8track + "," + st8FLabel + resultCode;
                                    resultCode = "";
                                  Statusst8="Fail";
                                    PLCWriteCommand6[PLCWriteCommand_DM5349] = 0x0F;
                                }
                                if ((PLCQueryRx6[PLCQueryRx_DM5113] == 0x09) &&
                                    (PLCWriteCommand6[PLCWriteCommand_DM5349] == 0x00) &&
                                    (!evt_FG01_FG02Move.WaitOne(0)) && resultCode == "799")//part placed at output trolley
                                {
                                    //  networkmain.sendFG01_FG02_MOVE(Station6ForOP1Scanboxid);
                                    //TMP REMOVE//networkmain.Client_sendFG01_FG02_MOVE(st8FINISH,"FG01_FG02_MOVE");
                                    try
                                    {
                                        #region OEEStep7
                                        rq.UpdST8RJ(OEEid8, 1, "0");
                                        #endregion
                                    }
                                    catch (Exception)
                                    {
                                        throw;
                                    }
                                   Statusst8="Pass";
                                    #region NEED TO OPEN WHEN REAL TIME
                                    evt_FG01_FG02Move.Set();//have problem here.. did not check if fg01 fg02 is sent successfully
                                    while (!evt_FG01_FG02Move_Rx.WaitOne(100) && !bTerminate) ;
                                    evt_FG01_FG02Move_Rx.Reset();
                                    #endregion


                                    StationStarStopLog.Info("Running Stop,Result Pass " + st8track + "," + st8FLabel + resultCode);
                                    networkmain.stn8log = st8track + "," + st8FLabel + " Stopped " + resultCode;
                                    networkmain.OperatorLog = "Stn.8 Stopped/Pass " + st8track + "," + st8FLabel;
                                    resultCode = "";
                                    PLCWriteCommand6[PLCWriteCommand_DM5349] = 0x09;
                                }
                                if ((PLCQueryRx6[PLCQueryRx_DM5113] == 0x06) &&
                                     (PLCWriteCommand6[PLCWriteCommand_DM5349] == 0x00))
                                //(!evnt_RejectFinishingLabelForStation8.WaitOne(0)))//part placed at reject bin
                                {
                                   Statusst8="AQL";
                                    try
                                    {
                                        #region OEEStep7
                                        rq.UpdST8RJ(OEEid8, 2, "0");
                                        #endregion
                                    }
                                    catch (Exception)
                                    {

                                        throw;
                                    }
                                    networkmain.Client_sendFG01_FG02_MOVE(st8FINISH, "Station 8 AQL Label");
                                    networkmain.Client_sendAQL_BOX(st8FLabel);//Send SMS to Micron
                                    
                                    StationStarStopLog.Info("Running Stop,Result AQL " + st8track + "," + st8FLabel + resultCode);
                                    networkmain.stn8log = st8track + "," + st8FLabel + " Stopped AQL " + resultCode;
                                    networkmain.OperatorLog = "Stn.8 Stopped/AQL " + st8track + "," + st8FLabel;
                                    resultCode = "";
                                    PLCWriteCommand6[PLCWriteCommand_DM5349] = 0x06;
                                }
                                if ((PLCQueryRx6[PLCQueryRx_DM5113] == 0x99) &&
                                   (PLCWriteCommand6[PLCWriteCommand_DM5349] == 0x00))
                                //(!evnt_RejectFinishingLabelForStation8.WaitOne(0)))//part placed at reject bin
                                {

                                    //NEED TO SEND ACK MIDDLEWARE R806

                                    try
                                    {
                                        string RJCODE = "807";
                                        XmlDocument doc = new XmlDocument();
                                        doc.Load(@"ConfigEvent.xml");
                                        XmlNode node = doc.SelectSingleNode(@"/EVENT/R" + RJCODE);
                                        string RJName = node.InnerText;
                                        try
                                        {
                                            #region OEEStep7
                                            rq.UpdST8RJ(OEEid8, 8, RJCODE);
                                            #endregion
                                        }
                                        catch (Exception)
                                        {
                                            throw;
                                        }
                                        networkmain.Client_SendEventMessage("77", RJName, "BOX_ID", st8FLabel);
                                        AllRJEvent.Info("Station 8 Send ST8 TOP ESD REJECT Event to Middleware " + RJCODE + "," + RJName + "," + st8FLabel);
                                        networkmain.stn8log = "Station 8 Send ST8 TOP ESD REJECT Event to Middleware " + RJCODE + "," + RJName + "," + st8FLabel;
                                    }
                                    catch (Exception ex)
                                    {

                                    }

                                    networkmain.Client_sendFG01_FG02_MOVE(st8FLabel, "Station 8 AutoTimeoutReject Label");
                                    Statusst8 = "AutoTimeoutReject";
                                    StationStarStopLog.Info("Running Stop,Result AutoTimeoutReject " + st8track + "," + st8FLabel + resultCode);
                                    networkmain.stn8log = st8track + "," + st8FLabel + "Running Stop,Result AutoTimeoutReject " + resultCode;
                                    //networkmain.OperatorLog = "Stn.8 Stopped/AQL " + st8track + "'" + st8FLabel;
                                    resultCode = "";
                                    PLCWriteCommand6[PLCWriteCommand_DM5349] = 0x99;
                                }

                            

                            if ((PLCQueryRx6[PLCQueryRx_DM5113] == 0x05) &&
                                     (PLCWriteCommand6[PLCWriteCommand_DM5349] == 0x00))
                                //(!evnt_RejectFinishingLabelForStation8.WaitOne(0)))//part placed at reject bin
                                {

                                 //NEED TO SEND ACK MIDDLEWARE R806

                                 try{
                           string RJCODE="806";
                          XmlDocument doc = new XmlDocument();
                          doc.Load(@"ConfigEvent.xml");
                          XmlNode node = doc.SelectSingleNode(@"/EVENT/R"+RJCODE);                         
                          string RJName=node.InnerText;
                                        try
                                        {
                                            #region OEEStep7
                                            rq.UpdST8RJ(OEEid8, 8, RJCODE);
                                            #endregion
                                        }
                                        catch (Exception)
                                        {
                                            throw;
                                        }
                                        networkmain.Client_SendEventMessage("64", RJName,"BOX_ID",st8FLabel);  
                                        Station8Log.Info("ST7 BARCODE READ FAIL " + RJCODE+","+RJName+","+st8FLabel);
                                        AllRJEvent.Info("ST7 BARCODE READ FAIL " + RJCODE+","+RJName+","+st8FLabel);
                                        networkmain.stn8log = "ST7 BARCODE READ FAIL " + RJCODE + "," + RJName + "," + st8FLabel;
                                    }
                                 catch (Exception ex){                                    
                                 
                                                      } 

                                    networkmain.Client_sendFG01_FG02_MOVE(st8FLabel, "Station 8 AutoTimeoutReject Label");
                                    Statusst8="AutoTimeoutReject";
                                    StationStarStopLog.Info("Running Stop,Result AutoTimeoutReject " + st8track + "," + st8FLabel + resultCode);
                                    networkmain.stn8log = st8track + "," + st8FLabel + "Running Stop,Result AutoTimeoutReject " + resultCode;
                                    //networkmain.OperatorLog = "Stn.8 Stopped/AQL " + st8track + "'" + st8FLabel;
                                    resultCode = "";
                                    PLCWriteCommand6[PLCWriteCommand_DM5349] = 0x05;
                                }
                                //FLMembkp8 = Station8ForTransferScanboxid;
                            }
                            else
                            {
                                CheckstringClearFor8(Station8OFFSET, Station8ForTransferScanboxid);
                                PLCWriteCommand6[PLCWriteCommand_DM5349] = 0x00;
                                Station8ForTransferScanboxid = "\0\0\0\0\0\0\0\0";
                               CheckstringClearFor8AQL(439,Station8ForTransferScanboxid);//D5409=439
                                 st8track = "";                             
                                 st8FLabel= "";
                                 Statusst8= "";
                            }



                              //if ((PLCQueryRx6[PLCQueryRx_DM5113] == 0x05) &&
                              //       (PLCWriteCommand6[PLCWriteCommand_DM5349] == 0x00))
                              //  //(!evnt_RejectFinishingLabelForStation8.WaitOne(0)))//part placed at reject bin
                              //  {
                              //      networkmain.Client_sendFG01_FG02_MOVE(st8trackForAutotimeoutReject, "Station 8 AutoTimeoutReject Label");
                                   
                              //      StationStarStopLog.Info("Running Stop,Result AutoTimeoutReject " + st8track + "'" + st8FLabel);
                              //      //networkmain.stn8log = st8track + "'" + st8FLabel + " Stopped AQL ";
                              //      //networkmain.OperatorLog = "Stn.8 Stopped/AQL " + st8track + "'" + st8FLabel;
                              //      PLCWriteCommand6[PLCWriteCommand_DM5349] = 0x05;
                              //  }
                          
                           if ((PLCQueryRx6[PLCQueryRx_DM5113] == 0x00))
                           {                           
                           PLCWriteCommand6[PLCWriteCommand_DM5349] = 0x00;                          
                           
                           }

                        

                            #region Station 8 Error Code

                            byte[] tmparrayERst8 = new byte[2];
                            Array.Copy(PLCQueryRx6, 407, tmparrayERst8, 0, 2); //5198
                            //convert Byte array to int                 
                            Int32 erst8 = (Int32)(BitConverter.ToInt16(tmparrayERst8, 0));                        

                            ErrCode8 = erst8.ToString();

                            #region NewErrorCode
                            if (erst8 > 0)
                            {
                                // LogEr.Info("Station 8 Error Code" + ErrCode8); 
                                Errmessage8 = "Stn.8 Err " + ErrCode8 + ": " + Stn8ErrToMsg(erst8);
                                if (!ST8JamFlag)
                                {
                                    bool St8JamTrig = ST2PauseFunction(8, erst8 + ";"); //Check if is a JAM
                                    if (St8JamTrig)
                                    {
                                        ST8JamFlag = true;
                                        //string[] FLbatch = rq.UpdJamstatus(8, 888); //Update Jam FL
                                        //if (FLbatch != null)
                                        //{
                                        //    networkmain.Client_SendEventMsg("828", "Station8FLJAMRecovery", FLbatch);//Update Jam recovery FL to middleware
                                        //}
                                    }
                                }


                            }
                            else
                            {
                                ST8JamFlag = false;
                                Errmessage8 = String.Empty;
                            }
                            #endregion
                            #region OldCode
                            //if (erst8 > 0)
                            //{
                            //    // LogEr.Info("Station 8 Error Code" + ErrCode8); 
                            //    Errmessage8 = "Stn.8 Err " + ErrCode8 + ": " + Stn8ErrToMsg(erst8);

                            //    //   LogEr.Info(Errmessage8);



                            //}
                            //else
                            //{

                            //    Errmessage8 = String.Empty;
                            //}
                            #endregion
                            UpdateErrorMsg((int)Station.StationNumber.Station08, Errmessage8, ST8JamFlag);

                              if((erst8 > 0)  && networkmain.controlst8==0)
                              {
                                Errst8=erst8.ToString();
                                networkmain.controlst8=1;
                               messagest8= Stn8ErrToMsg(erst8);
                               networkmain.Client_SendAlarmMessage(erst8.ToString(), messagest8,"SET");
                              LogEr.Info("Error Set st8 "+erst8.ToString()+messagest8);
                            
                              }
                                if(erst8 == 0 && networkmain.controlst8==1)
                              {
                              networkmain.Client_SendAlarmMessage(Errst8, messagest8,"CLEAR");
                              networkmain.controlst8=0;
                              LogEr.Info("Error Clear st8 "+Errst8+messagest8);
                               Errst8="";
                               messagest8="";
                              }

                            #endregion


                          }

                                       catch (Exception ex)
                            {
                                Log1.Info(ex.ToString());
                            }
                            #endregion

                         

                    }


                 #endregion




                }
                catch (Exception ex)
                {
                    Log1.Info(ex.ToString());
                }
               // Log1.Info(" ParameterChange Thread Exit");
            }//try first
        
                 #endregion





        }



  }
}
