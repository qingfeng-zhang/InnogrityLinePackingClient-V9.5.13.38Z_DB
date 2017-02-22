
using NLog;
using System;
using System.Text;
using System.Threading;
using System.Net.NetworkInformation;
using IGTwpf;
namespace InnogrityLinePackingClient
{
    partial class NetworkThread
    {
        bool ST2JamFlag = false;
        public string messagest2 = "";
        public string Errst2 = "";


        public string messagest2_1 = "";
        public string Errst2_1 = "";

        public string messagest2_2 = "";
        public string Errst2_2 = "";

        public string messagest2_3 = "";
        public string Errst2_3 = "";

        public string messagest2_4 = "";
        public string Errst2_4 = "";

        public string messagest2_5 = "";
        public string Errst2_5 = "";

        public string messagest2_6 = "";
        public string Errst2_6 = "";

        public string messagest2_7 = "";
        public string Errst2_7 = "";


        public void RunStation02Scan(object msgobj)
        {
            MainNetworkClass networkmain = (MainNetworkClass)msgobj;
            while (!bTerminate)
            {
                Thread.Sleep(100);
                try
                {
                    if ((PLCQueryRx == null) || (PLCWriteCommand == null))
                    {
                        Thread.Sleep(100);
                        continue;
                    }
                    #region PLC_RequestFinishingLabelFromMiddleWare
                    //Process PLC Data                        
                    //Check Station 2 Finishing Label Status
                    //      plc TRIGGER @ DM0100     server send is in ready state @ DM200
                    if ((PLCQueryRx[PLCQueryRx_DM100] == 0x08) && (PLCWriteCommand[PLCWriteCommand_DM200] == 0))//request finishing label 
                    {
                        //PLC request FL from IGT 
                        //set busy flag
                        PLCWriteCommand[PLCWriteCommand_DM200] = 0x04; //busy
                        //get barcode scan string from DM010
                        //test code tmp//
                        byte[] bcarray = new byte[10];
                        Array.Copy(PLCQueryRx, 31, bcarray, 0, 10);
                        Scanboxid = System.Text.Encoding.Default.GetString(bcarray);
                        if (Scanboxid != "\0\0\0\0\0\0\0\0\0\0")
                        {
                            evt_FinishLabelRequest.Set();
                        }
                        else
                        {
                            PLCWriteCommand[PLCWriteCommand_DM200] = 0x00;
                        }
                    }
                    //trigger is cleared    Send Complete       Send Error
                    if ((PLCQueryRx[PLCQueryRx_DM100] == 0x0) && ((PLCWriteCommand[PLCWriteCommand_DM200] == 0x08)
                                                               || (PLCWriteCommand[PLCWriteCommand_DM200] == 0xF)))
                        PLCWriteCommand[PLCWriteCommand_DM200] = 0;//Server Send Ready
                    #endregion
                    #region Update_RA_RB_RC_Label
                    //      PLC Move FL to A, B, C Zone.
                    byte[] tmparray = new byte[10];
                    //DM0050
                    Array.Copy(PLCQueryRx, 111, tmparray, 0, 10);
                    //convert array to string                        
                    ST02Rotatary_A_Str = System.Text.Encoding.Default.GetString(tmparray);

                    if (ST02Rotatary_A_Str != "\0\0\0\0\0\0\0\0\0\0")
                    {
                        ReceiveFL = ST02Rotatary_A_Str;
                       
                    }


                       if (ST02Rotatary_A_Str == "\0\0\0\0\0\0\0\0\0\0")
                        {
                           ReceiveFL  ="";
                           PrintStatus="";
                           VisionStatus=""; 
                        }

                   if (ST02Rotatary_B_Str == "\0\0\0\0\0\0\0\0\0\0")
                        {
                           ReceiveFL1="";
                           PrintStatus1="";
                           VisionStatus1=""; 
                        }

                   if (ST02Rotatary_B_Str != "\0\0\0\0\0\0\0\0\0\0")
                   {
                       ReceiveFL1 = ST02Rotatary_B_Str;
                       
                   }




                   if (ST02Rotatary_C_Str == "\0\0\0\0\0\0\0\0\0\0")
                        {
                           ReceiveFL2="";
                           PrintStatus2="";
                           VisionStatus2=""; 
                        }



                   if (ST02Rotatary_C_Str != "\0\0\0\0\0\0\0\0\0\0")
                   {
                       ReceiveFL2 = ST02Rotatary_C_Str;
                       
                   }


                    //DM0060
                    Array.Copy(PLCQueryRx, 131, tmparray, 0, 10);
                    //convert array to string
                    ST02Rotatary_B_Str = System.Text.Encoding.Default.GetString(tmparray);
                    //DM0070
                    Array.Copy(PLCQueryRx, 151, tmparray, 0, 10);
                    //convert array to string
                    ST02Rotatary_C_Str = System.Text.Encoding.Default.GetString(tmparray);
                    //table
                    #endregion
                    #region PLCStatusUpdateOnRARBRC
                    //update RA RB RC status (later.. on station 4, 5, 6, 7??)
                    CheckStringClear(RA_PCtoPLCFinishingLabelOFFSET, ST02Rotatary_A_Str);
                    CheckStringClear(RB_PCtoPLCFinishingLabelOFFSET, ST02Rotatary_B_Str);
                    CheckStringClear(RC_PCtoPLCFinishingLabelOFFSET, ST02Rotatary_C_Str);
                    CheckStringUpdate(RA_PCtoPLCFinishingLabelOFFSET, ST02Rotatary_A_Str);
                    CheckStringUpdate(RB_PCtoPLCFinishingLabelOFFSET, ST02Rotatary_B_Str);
                    CheckStringUpdate(RC_PCtoPLCFinishingLabelOFFSET, ST02Rotatary_C_Str);
                    //Int64 rst;
                    #endregion
                    #region UpdateRJ
                    //Rotary A
                    byte[] tmparrayERA = new byte[2];
                    Array.Copy(PLCQueryRx, 121, tmparrayERA, 0, 2); //DM55=121
                    //convert Byte array to int                 
                    Int32 erA = (Int32)(BitConverter.ToInt16(tmparrayERA, 0));
                    if (erA > 0)
                    {
                        byte[] tmpbyte = new byte[2];
                        tmpbyte = Encoding.ASCII.GetBytes("RJ");
                        Array.Copy(tmpbyte, 0, PLCWriteCommand, 95, 2); //D237
                        string rj = "RJ";
                        try
                        {
                            while ((!networkmain.UpdateRJLabel(ST02Rotatary_A_Str, rj, erA.ToString()) && !bTerminate))
                            {
                                Thread.Sleep(100);
                            }
                           // St2Log.Info("Update RJ for Rotary A " + ST02Rotatary_A_Str +","+erA);
                           networkmain.linePack.Info("Update RJ for Rotary A " + ST02Rotatary_A_Str +","+erA);
                        }
                        catch (Exception ex)
                        {
                           // St2Log.Error("station 2 RA error" + ex);
                           networkmain.linePack.Error("station 2 RA error" + ex);
                            byte[] tmpbyte1 = new byte[2];
                            tmpbyte1 = Encoding.ASCII.GetBytes("RJ");
                            Array.Copy(tmpbyte1, 0, PLCWriteCommand, 95, 2); //D237
                            //update fail..
                            // reply to PLC update fail
                            // may need this other time
                        }
                    }


                    //Rotary B

                    byte[] tmparrayERB = new byte[2];
                    Array.Copy(PLCQueryRx, 141, tmparrayERB, 0, 2); //DM65=141
                    //convert Byte array to int                 
                    Int32 erB = (Int32)(BitConverter.ToInt16(tmparrayERB, 0));
                    if (erB > 0)
                    {
                        byte[] tmpbyte = new byte[2];
                        tmpbyte = Encoding.ASCII.GetBytes("RJ");
                        Array.Copy(tmpbyte, 0, PLCWriteCommand, 157, 2); //D268
                        string rj = "RJ";
                        try
                        {
                            while ((!networkmain.UpdateRJLabel(ST02Rotatary_B_Str, rj, erB.ToString()) && !bTerminate))
                            {
                                Thread.Sleep(100);
                            }
                         //   St2Log.Info("Update RJ for Rotary B " + ST02Rotatary_B_Str+","+erB);
                           networkmain.linePack.Info("Update RJ for Rotary B " + ST02Rotatary_B_Str+","+erB);

                        }
                        catch (Exception ex)
                        {
                           // St2Log.Error("station 2 RB error" + ex);

                          networkmain.linePack.Error("station 2 RB error" + ex);
                            byte[] tmpbyte1 = new byte[2];
                            tmpbyte1 = Encoding.ASCII.GetBytes("RJ");
                            Array.Copy(tmpbyte1, 0, PLCWriteCommand, 157, 2); //D268
                            //update fail..
                            // reply to PLC update fail
                            // may need this other time
                        }
                    }


                    //RC
                    byte[] tmparrayERC = new byte[2];
                    Array.Copy(PLCQueryRx, 161, tmparrayERC, 0, 2); //DM75=161
                    //convert Byte array to int                 
                    Int32 erC = (Int32)(BitConverter.ToInt16(tmparrayERC, 0));
                    if (erC > 0)
                    {
                        byte[] tmpbyte = new byte[2];
                        tmpbyte = Encoding.ASCII.GetBytes("RJ");
                        Array.Copy(tmpbyte, 0, PLCWriteCommand, 217, 2); //D298
                        string rj = "RJ";
                        try
                        {
                            while ((!networkmain.UpdateRJLabel(ST02Rotatary_C_Str, rj, erC.ToString()) && !bTerminate))
                            {
                                Thread.Sleep(100);
                            }
                           // St2Log.Info("Update RJ for Rotary C " + ST02Rotatary_C_Str+","+erC);

                            networkmain.linePack.Info("Update RJ for Rotary C " + ST02Rotatary_C_Str+","+erC);

                        }
                        catch (Exception ex)
                        {
                           // St2Log.Error("station 2 RC error" + ex);

                            networkmain.linePack.Error("station 2 RC error" + ex);
                            byte[] tmpbyte1 = new byte[2];
                            tmpbyte1 = Encoding.ASCII.GetBytes("RJ");
                            Array.Copy(tmpbyte1, 0, PLCWriteCommand, 217, 2); //D298
                            //update fail..
                            // reply to PLC update fail
                            // may need this other time
                        }
                    }
                    #endregion
                    #region Station2_PLC_RequestPrint
                    //check finishing label data ready at respective station or rotary
                    //check DM101,102,103
                    if ((PLCQueryRx[PLCQueryRx_DM100 + 2] == 0x0) &&//send reset signal to the IGT server DM101,102,103 have to be set to 0;
                        (PLCQueryRx[PLCQueryRx_DM100 + 4] == 0x0) &&//assume at any one time, only one rotary is doing the request
                        (PLCQueryRx[PLCQueryRx_DM100 + 6] == 0x0))//optimization code needed for PLC to print and inspect at the same time
                    {
                        PLCWriteCommand[PLCWriteCommand_DM200 + 2] = 0x00;
                    }
                    ////////////////////////////////////////
                    // Station 2 Testpront Function ////////
                    ////////////////////////////////////////
                    if ((PLCQueryRx[PLCQueryRx_DM100 + 2] == 0x99) //request print for rotary A        /*DM101*///state 1
                      &&
                      (!evt_Station02PrintReq.WaitOne(0))
                      &&
                      (PLCWriteCommand[PLCWriteCommand_DM200 + 2] == 0x00)/*DM201*/
                      )
                    {
                        PLCWriteCommand[PLCWriteCommand_DM200 + 2] = 0x4; //set busy signal
                    
                        ZebraTestPrint zbt = new ZebraTestPrint();
                      bool Printok =  zbt.ChecknLoadZPLForTestPrint(2);
                        if (Printok)
                        {
                             MyEventQ.AddQ("82;PrinterTestPrint;PrinterNumber;2");//Push message to stack
                             EvtLog.Info("82;PrinterTestPrint;PrinterNumber;2");
                        }
                        else
                        {
                            MyEventQ.AddQ("11;PrinterCommunicationBreak;Stationnumber;2");//Push message to stack
                        }
                        PLCWriteCommand[PLCWriteCommand_DM200 + 2] = 0x99; //set ok signal
                        zbt = null;
                    }
                    ////////////////////////////////////////
                    //request Orientation Chk on RA/B/C ////
                    ////////////////////////////////////////
                    if ((PLCQueryRx[PLCQueryRx_DM100 + 2] == 0x3)        /*DM101*///state 1
                        &&
                        (PLCWriteCommand[PLCWriteCommand_DM200 + 2] == 0x00)/*DM201*/
                        )
                    {
                        PLCWriteCommand[PLCWriteCommand_DM200 + 2] = 0x4; //set busy signal
                        evt_Station02InspectionReq.Set();
                    }
                    if ((PLCQueryRx[PLCQueryRx_DM100 + 2] == 0x1) //request print for rotary A        /*DM101*///state 1
                        &&
                        (!evt_Station02PrintReq.WaitOne(0))
                        &&
                        (PLCWriteCommand[PLCWriteCommand_DM200 + 2] == 0x00)/*DM201*/
                        )
                    {
                        if (ST02Rotatary_A_Str.Trim().Length == 0)
                        {
                            log.Error("ST2 RotaryA FL label is empty when requesting print");
                        }
                        else if (ST02Rotatary_A_Str == "\0\0\0\0\0\0\0\0\0\0")
                        {
                            log.Error("ST2 RotaryA FL label is \0\0\0\0\0\0\0\0\0\0 when requesting print");
                        }
                        else
                        {
                            PLCWriteCommand[PLCWriteCommand_DM200 + 2] = 0x4; //set busy signal
                            ST02Rotatary_Str = ST02Rotatary_A_Str;

                            //show in table FL
                            //fire printer thread
                            //Printer01Thread = new Thread(new ParameterizedThreadStart(Printer01Th));
                            //Printer01Thread.Start(networkmain);
                            evt_Station02PrintReq.Set();
                            log.Info("ST2 RotaryA Start request printing:" + ST02Rotatary_Str);
                        }
                    }
                    if ((PLCQueryRx[PLCQueryRx_DM100 + 4] == 0x1) //request print for rotary B        /*DM102*///state 1
                        &&
                        (!evt_Station02PrintReq.WaitOne(0))
                        &&
                        (PLCWriteCommand[PLCWriteCommand_DM200 + 2] == 0x00)) /*DM201*/
                    {
                        if (ST02Rotatary_B_Str.Trim().Length == 0)
                        {
                            log.Error("ST2 RotaryB FL label is empty when requesting print");
                        }
                        else if (ST02Rotatary_B_Str == "\0\0\0\0\0\0\0\0\0\0")
                        {
                            log.Error("ST2 RotaryB FL label is \0\0\0\0\0\0\0\0\0\0 when requesting print");
                        }
                        else
                        {
                            PLCWriteCommand[PLCWriteCommand_DM200 + 2] = 0x4; //set busy signal
                            ST02Rotatary_Str = ST02Rotatary_B_Str;

                            //fire printer thread
                            //Printer01Thread = new Thread(new ParameterizedThreadStart(Printer01Th));
                            //Printer01Thread.Start(networkmain);
                            evt_Station02PrintReq.Set();
                            log.Info("ST2 RotaryB Start request printing:" + ST02Rotatary_Str);
                        }
                    }
                    if ((PLCQueryRx[PLCQueryRx_DM100 + 6] == 0x1) //request print for rotary C        /*DM103*///state 1
                        &&
                        (!evt_Station02PrintReq.WaitOne(0))
                        &&
                        (PLCWriteCommand[PLCWriteCommand_DM200 + 2] == 0x00)) /*DM201*/
                    {
                        if (ST02Rotatary_C_Str.Trim().Length == 0)
                        {
                            log.Error("ST2 RotaryC FL label is empty when requesting print");
                        }
                        else if (ST02Rotatary_C_Str == "\0\0\0\0\0\0\0\0\0\0")
                        {
                            log.Error("ST2 RotaryC FL label is \0\0\0\0\0\0\0\0\0\0 when requesting print");
                        }
                        else
                        {
                            PLCWriteCommand[PLCWriteCommand_DM200 + 2] = 0x4; //set busy signal
                            ST02Rotatary_Str = ST02Rotatary_C_Str;
                            PrintStatus2 = "Received";
                            //fire printer thread
                            //Printer01Thread = new Thread(new ParameterizedThreadStart(Printer01Th));
                            //Printer01Thread.Start(networkmain);
                            evt_Station02PrintReq.Set();
                            log.Info("ST2 RotaryC Start request printing:" + ST02Rotatary_Str);
                        }
                    }
                    #endregion
                    #region Station2_PLC_RequestLabelInspection
                    if ((PLCQueryRx[PLCQueryRx_DM157 + 26] == 0x0) &&//send reset signal to the IGT server DM101,102,103 have to be set to 0;
                        (PLCQueryRx[PLCQueryRx_DM157 + 30] == 0x0) &&//assume at any one time, only one rotary is doing the request
                        (PLCQueryRx[PLCQueryRx_DM157 + 34] == 0x0))//optimization code needed for PLC to print and inspect at the same time
                    {
                        PLCWriteCommand[PLCWriteCommand_DM380 + 2] = 0x00;
                    }
                    #region RotaryARequest
                    if ((PLCQueryRx[PLCQueryRx_DM100 + 2] == 0x1)//vision inspection ready/*DM101*///state 2
                         &&
                        (PLCWriteCommand[PLCWriteCommand_DM200 + 2] == 0x05)
                         &&
                        (!evt_Station02PrintReq.WaitOne(0))//assumming data had been send to vision also
                        )
                    {
                        PLCWriteCommand[PLCWriteCommand_DM200 + 2] = 0x06;//print send complete    
                        PrintStatus = "Received";
                    }
                    if ((PLCQueryRx[PLCQueryRx_DM157 + 26] == 0x2)//vision inspection trigger /*DM170*///state 2
                        &&
                        (PLCWriteCommand[PLCWriteCommand_DM380 + 2] == 0x00)//vision ready signal
                        )
                    {
                        //  PLCWriteCommand[PLCWriteCommand_DM380 + 2] = 0x08;
                        //St02Vision01Thread = new Thread(new ParameterizedThreadStart(ST02Vision01Th));
                        //St02Vision01Thread.Start(networkmain);
                        VisionRotaryCheck="1";
                        evt_Station02InspectionReq.Set();
                    }
                    #endregion
                    #region RotaryBRequest
                    if ((PLCQueryRx[PLCQueryRx_DM100 + 4] == 0x1)//vision inspection ready/*DM101*///state 2
                         &&
                        (PLCWriteCommand[PLCWriteCommand_DM200 + 2] == 0x05)
                        &&
                        (!evt_Station02PrintReq.WaitOne(0))//assumming data had been send to vision also
                        )
                    {
                        PLCWriteCommand[PLCWriteCommand_DM200 + 2] = 0x06;//print send complete   
                         PrintStatus1 = "Received";
                    }
                    if ((PLCQueryRx[PLCQueryRx_DM157 + 30] == 0x2)//vision inspection trigger /*DM172*///state 2
                        &&
                        (PLCWriteCommand[PLCWriteCommand_DM380 + 2] == 0x00)//vision ready signal
                        )
                    {
                        // PLCWriteCommand[PLCWriteCommand_DM380 + 2] = 0x08;
                        //St02Vision01Thread = new Thread(new ParameterizedThreadStart(ST02Vision01Th));
                        //St02Vision01Thread.Start(networkmain);
                        VisionRotaryCheck="2";
                        evt_Station02InspectionReq.Set();
                    }
                    #endregion
                    #region RotaryCRequest
                    if ((PLCQueryRx[PLCQueryRx_DM100 + 6] == 0x1)//vision inspection ready/*DM101*///state 2
                         &&
                        (PLCWriteCommand[PLCWriteCommand_DM200 + 2] == 0x05)
                         &&
                        (!evt_Station02PrintReq.WaitOne(0))//assumming data had been send to vision also
                        )
                    {
                        PLCWriteCommand[PLCWriteCommand_DM200 + 2] = 0x06;//print send complete 
                        PrintStatus2 = "Received";
                    }
                    if ((PLCQueryRx[PLCQueryRx_DM157 + 34] == 0x2)//Rotary A vision inspection trigger /*DM174*///state 2
                        &&
                        (PLCWriteCommand[PLCWriteCommand_DM380 + 2] == 0x00)//vision ready signal
                        )
                    {
                        //  PLCWriteCommand[PLCWriteCommand_DM380 + 2] = 0x08;
                        //St02Vision01Thread = new Thread(new ParameterizedThreadStart(ST02Vision01Th));
                        //St02Vision01Thread.Start(networkmain);
                         VisionRotaryCheck="3";
                        evt_Station02InspectionReq.Set();
                    }
                    #endregion
                    #endregion

                          #region Output Transpoter

                   try
                          {


                              //Tracking Label 
                              byte[] temp83 = new byte[10];
                              Array.Copy(PLCQueryRx, 241, temp83, 0, 10); // DM 115~119 PLC1
                              OutputTranspoterFL = System.Text.Encoding.Default.GetString(temp83);
                              if (OutputTranspoterFL != "\0\0\0\0\0\0\0\0\0\0")
                              {
                                                                  
                                 

                                  #region RJ status update
                                  //need lock 
                                  //DM154
                                  byte[] tmparrayER7 = new byte[2];
                                  Array.Copy(PLCQueryRx, 319, tmparrayER7, 0, 2);
                                  //convert Byte array to int                 
                                  Int32 er7 = (Int32)(BitConverter.ToInt16(tmparrayER7, 0));
                                  if (er7 > 0)
                                  {

                                      string rj = "RJ";
                                      try
                                      {
                                          while ((!networkmain.UpdateRJLabel(OutputTranspoterFL, rj, er7.ToString()) && !bTerminate))
                                          {
                                              Thread.Sleep(100);
                                          }
                                      }
                                      catch
                                      {

                                      }

                                  }
                                  #endregion

                                  PLCWriteCommand[309] = 0x0F;//D344
                              }
                              else
                              {
                                  OutputTranspoterFL = "\0\0\0\0\0\0\0\0\0\0";
                                   PLCWriteCommand[309]  = 0x00;
                              }
                          }
                          catch (Exception ex)
                          {
                              Log1.Info(ex.ToString());
                          }












                          #endregion


                    #region Printer Clear Data

                    #region RA

                    if ((PLCQueryRx[PLCQueryRx_DM150] == 0x01)
                                &&
                                (PLCWriteCommand[PLCWriteCommand_DM340] == 0x00)
                                &&
                                (!evt_Station02PrintClearReqRA.WaitOne(0))
                              )
                    {
                        PLCWriteCommand[PLCWriteCommand_DM340] = 0x4; //set busy signal

                       // St2Log.Info("Station 2 Printing clear  Start");
                        networkmain.linePack.Info("Station 2 Printing clear  Start");
                        ST02Rotatary_Str = ST02Rotatary_A_Str;
                        evt_Station02PrintClearReqRA.Set();
                    }

                    if ((PLCQueryRx[PLCQueryRx_DM150] == 0x1)//vision inspection ready
                         &&
                        (PLCWriteCommand[PLCWriteCommand_DM340] == 0x05)
                        &&
                         (!evt_Station02PrintClearReqRA.WaitOne(0))//assumming data had been send to vision also
                        )
                    {
                        PLCWriteCommand[PLCWriteCommand_DM340] = 0x06;//print send complete    
                        //PrintStatus = "print send complete";
                    }


                    if (PLCQueryRx[PLCQueryRx_DM150] == 0x00)
                    {

                        PLCWriteCommand[PLCWriteCommand_DM340] = 0x00;
                    }


                    #endregion

                  

                

                    #endregion

                  
                    #region Station 2 Error code

                    byte[] tmparrayER2_ = new byte[2];
                    Array.Copy(PLCQueryRx, 371, tmparrayER2_, 0, 2);  //180=371,181=373,182=375,183=377,184=379,185=381,186=383,187=385
                    //convert Byte array to int                 
                    Int32 erst2 = (Int32)(BitConverter.ToInt16(tmparrayER2_, 0));
                    ErrCode2 = erst2.ToString();

                    byte[] tmparrayER2_1 = new byte[2];
                    Array.Copy(PLCQueryRx, 373, tmparrayER2_1, 0, 2);  //181=373,182=375,183=377,184=379,185=381,186=383,187=385
                    //convert Byte array to int                 
                    Int32 erst2_1 = (Int32)(BitConverter.ToInt16(tmparrayER2_1, 0));
                    ErrCode2_1 = erst2_1.ToString();

                    byte[] tmparrayER2_2 = new byte[2];
                    Array.Copy(PLCQueryRx, 375, tmparrayER2_2, 0, 2);  //182=375,183=377,184=379,185=381,186=383,187=385
                    //convert Byte array to int                 
                    Int32 erst2_2 = (Int32)(BitConverter.ToInt16(tmparrayER2_2, 0));
                    ErrCode2_2 = erst2_2.ToString();

                    byte[] tmparrayER2_3 = new byte[2];
                    Array.Copy(PLCQueryRx, 377, tmparrayER2_3, 0, 2);  //183=377,184=379,185=381,186=383,187=385
                    //convert Byte array to int                 
                    Int32 erst2_3 = (Int32)(BitConverter.ToInt16(tmparrayER2_3, 0));
                    ErrCode2_3 = erst2_3.ToString();

                    byte[] tmparrayER2_4 = new byte[2];
                    Array.Copy(PLCQueryRx, 379, tmparrayER2_4, 0, 2);  //184=379,185=381,186=383,187=385
                    //convert Byte array to int                 
                    Int32 erst2_4 = (Int32)(BitConverter.ToInt16(tmparrayER2_4, 0));
                    ErrCode2_4 = erst2_4.ToString();

                    byte[] tmparrayER2_5 = new byte[2];
                    Array.Copy(PLCQueryRx, 381, tmparrayER2_5, 0, 2);  //185=381,186=383,187=385
                    //convert Byte array to int                 
                    Int32 erst2_5 = (Int32)(BitConverter.ToInt16(tmparrayER2_5, 0));
                    ErrCode2_5 = erst2_5.ToString();

                    byte[] tmparrayER2_6 = new byte[2];
                    Array.Copy(PLCQueryRx, 383, tmparrayER2_6, 0, 2);  //186=383,187=385
                    //convert Byte array to int                 
                    Int32 erst2_6 = (Int32)(BitConverter.ToInt16(tmparrayER2_6, 0));
                    ErrCode2_6 = erst2_6.ToString();

                    byte[] tmparrayER2_7 = new byte[2];
                    Array.Copy(PLCQueryRx, 385, tmparrayER2_7, 0, 2);  //187=385
                    //convert Byte array to int                 
                    Int32 erst2_7 = (Int32)(BitConverter.ToInt16(tmparrayER2_7, 0));
                    ErrCode2_7 = erst2_7.ToString();








                    #region NewErrorCode
                    if (erst2 > 0 || erst2_1 > 0 || erst2_2 > 0 || erst2_3 > 0 || erst2_4 > 0 || erst2_5 > 0 || erst2_6 > 0 || erst2_7 > 0)
                    {
                        //GYL: erst2~erst2_7 is represent DM180=DM2031,DM181=2033,182=DM2038,183=DM2043,184=2036,185=2037,186=DM2039,187=2046
                        //   LogEr.Info("Station 2 Error Code"+ErrCode2+ErrCode2_1+ErrCode2_2+ErrCode2_3+ErrCode2_4+ErrCode2_5+ErrCode2_6+ErrCode2_7);     

                        Errmessage2 = "Stn.2 Err " +
                                   (erst2 > 0 ? ErrCode2 + ": " + Stn2ErrToMsg(erst2) : "") +
                                   (erst2_1 > 0 && erst2_1 != erst2 ? ", " + ErrCode2_1 + ": " + Stn2ErrToMsg(erst2_1) : "") +
                                   (erst2_2 > 0 && erst2_2 != erst2 ? ", " + ErrCode2_2 + ": " + Stn2ErrToMsg(erst2_2) : "") +
                                   (erst2_3 > 0 && erst2_3 != erst2 ? ", " + ErrCode2_3 + ": " + Stn2ErrToMsg(erst2_3) : "") +
                                   (erst2_4 > 0 && erst2_4 != erst2 ? ", " + ErrCode2_4 + ": " + Stn2ErrToMsg(erst2_4) : "") +
                                   (erst2_5 > 0 && erst2_5 != erst2 ? ", " + ErrCode2_5 + ": " + Stn2ErrToMsg(erst2_5) : "") +
                                   (erst2_6 > 0 && erst2_6 != erst2 ? ", " + ErrCode2_6 + ": " + Stn2ErrToMsg(erst2_6) : "") +
                                   (erst2_7 > 0 && erst2_7 != erst2 ? ", " + ErrCode2_7 + ": " + Stn2ErrToMsg(erst2_7) : "");

                        //  LogEr.Info(Errmessage2);
                        //GYL:Add Pause STN2 Function,and ST2 JAM trigger
                        if (!ST2JamFlag)
                        {
                            bool St2JamTrig = ST2PauseFunction(2, erst2 + ";" + erst2_1 + ";" + erst2_2 + ";" + erst2_3 + ";" + erst2_4 + ";" + erst2_5 + ";" + erst2_6 + ";" + erst2_7);
                            if (St2JamTrig)
                            {
                                ST2JamFlag = true;

                                //string[] FLbatch = rq.UpdJamstatus(2, 222); //Update Jam FL

                                //if (FLbatch != null)
                                //{
                                //    networkmain.Client_SendEventMsg("265", "Station2FLJAMRecovery", FLbatch);//Update Jam recovery FL to middleware

                                //}
                            }
                        }
                    }
                    else
                    {
                        Errmessage2 = String.Empty;
                        ST2JamFlag = false;
                    }
                    #endregion
                    #region OldAlarm
                    //if (erst2 > 0 || erst2_1 > 0 || erst2_2 > 0 || erst2_3 > 0 || erst2_4 > 0 || erst2_5 > 0 || erst2_6 > 0 || erst2_7 > 0)
                    //{

                    //    //   LogEr.Info("Station 2 Error Code"+ErrCode2+ErrCode2_1+ErrCode2_2+ErrCode2_3+ErrCode2_4+ErrCode2_5+ErrCode2_6+ErrCode2_7);     

                    //    Errmessage2 = "Stn.2 Err " +
                    //               (erst2 > 0 ? ErrCode2 + ": " + Stn2ErrToMsg(erst2) : "") +
                    //               (erst2_1 > 0 && erst2_1 != erst2 ? ", " + ErrCode2_1 + ": " + Stn2ErrToMsg(erst2_1) : "") +
                    //               (erst2_2 > 0 && erst2_2 != erst2 ? ", " + ErrCode2_2 + ": " + Stn2ErrToMsg(erst2_2) : "") +
                    //               (erst2_3 > 0 && erst2_3 != erst2 ? ", " + ErrCode2_3 + ": " + Stn2ErrToMsg(erst2_3) : "") +
                    //               (erst2_4 > 0 && erst2_4 != erst2 ? ", " + ErrCode2_4 + ": " + Stn2ErrToMsg(erst2_4) : "") +
                    //               (erst2_5 > 0 && erst2_5 != erst2 ? ", " + ErrCode2_5 + ": " + Stn2ErrToMsg(erst2_5) : "") +
                    //               (erst2_6 > 0 && erst2_6 != erst2 ? ", " + ErrCode2_6 + ": " + Stn2ErrToMsg(erst2_6) : "") +
                    //               (erst2_7 > 0 && erst2_7 != erst2 ? ", " + ErrCode2_7 + ": " + Stn2ErrToMsg(erst2_7) : "");

                    //    //  LogEr.Info(Errmessage2);



                    //}
                    //else
                    //{
                    //    Errmessage2 = String.Empty;
                    //}
                    #endregion
                    UpdateErrorMsg((int)Station.StationNumber.Station02, Errmessage2,ST2JamFlag);



                   if(erst2 > 0 && networkmain.controlst2==0)
                      {
                                Errst2=erst2.ToString();
                                networkmain.controlst2=1;
                               messagest2= Stn2ErrToMsg(erst2);
                               networkmain.Client_SendAlarmMessage2(erst2.ToString(), messagest2,"SET");                     


                      }
                      
                      if(erst2 == 0 && networkmain.controlst2==1)
                      {
                        networkmain.Client_SendAlarmMessage2(Errst2, messagest2,"CLEAR");
                        messagest2="";
                        Errst2="";
                        networkmain.controlst2=0;
                      }


                       if(erst2_1 > 0 && erst2_1 != erst2 && networkmain.controlst2_1 ==0)
                      {
                                Errst2_1 =erst2_1.ToString();
                                networkmain.controlst2_1 =1;
                               messagest2_1 = Stn2ErrToMsg(erst2_1 );
                               networkmain.Client_SendAlarmMessage2(erst2_1.ToString(), messagest2_1 ,"SET");                     


                      }
                      
                      if(erst2_1  == 0 && networkmain.controlst2_1 ==1)
                      {
                        networkmain.Client_SendAlarmMessage2(Errst2_1 , messagest2_1 ,"CLEAR");
                        messagest2_1 ="";
                        Errst2_1 ="";
                        networkmain.controlst2_1 =0;
                      }



                       if(erst2_2 > 0 && erst2_2 != erst2 && erst2_2 != erst2_1 && networkmain.controlst2_2 ==0)
                      {
                                Errst2_2 =erst2_2.ToString();
                                networkmain.controlst2_2 =1;
                               messagest2_2 = Stn2ErrToMsg(erst2_2 );
                               networkmain.Client_SendAlarmMessage2(erst2_2.ToString(), messagest2_2 ,"SET");                     


                      }
                      
                      if(erst2_2  == 0 && networkmain.controlst2_2 ==1)
                      {
                        networkmain.Client_SendAlarmMessage2(Errst2_2, messagest2_2,"CLEAR");
                        messagest2_2 ="";
                        Errst2_2 ="";
                        networkmain.controlst2_2 =0;
                      }




                        if(erst2_3 > 0 && erst2_3 != erst2 && erst2_3 != erst2_2 && erst2_3 != erst2_1 && networkmain.controlst2_3 ==0)
                      {
                                Errst2_3 =erst2_3.ToString();
                                networkmain.controlst2_3 =1;
                               messagest2_3 = Stn2ErrToMsg(erst2_3 );
                               networkmain.Client_SendAlarmMessage2(erst2_3.ToString(), messagest2_3 ,"SET");                     


                      }
                      
                      if(erst2_3  == 0 && networkmain.controlst2_3 ==1)
                      {
                        networkmain.Client_SendAlarmMessage2(Errst2_3, messagest2_3,"CLEAR");
                        messagest2_3 ="";
                        Errst2_3 ="";
                        networkmain.controlst2_3 =0;
                      }



                      if(erst2_4 > 0 && erst2_4 != erst2  && erst2_4 != erst2_3 && erst2_4 != erst2_2 && erst2_4 != erst2_1 && networkmain.controlst2_4 ==0)
                      {
                                Errst2_4 =erst2_4.ToString();
                                networkmain.controlst2_4 =1;
                               messagest2_4 = Stn2ErrToMsg(erst2_4 );
                               networkmain.Client_SendAlarmMessage2(erst2_4.ToString(), messagest2_4 ,"SET");                     


                      }
                      
                      if(erst2_4  == 0 && networkmain.controlst2_4 ==1)
                      {
                        networkmain.Client_SendAlarmMessage2(Errst2_4, messagest2_4,"CLEAR");
                        messagest2_4 ="";
                        Errst2_4 ="";
                        networkmain.controlst2_4 =0;
                      }
                      
                      if(erst2_5 > 0 && erst2_5 != erst2 && erst2_5 != erst2_4  && erst2_5 != erst2_3 && erst2_5 != erst2_2 && erst2_5 != erst2_1 && networkmain.controlst2_5 ==0)
                      {
                                Errst2_5 =erst2_5.ToString();
                                networkmain.controlst2_5 =1;
                               messagest2_5 = Stn2ErrToMsg(erst2_5);
                               networkmain.Client_SendAlarmMessage2(erst2_5.ToString(), messagest2_5 ,"SET");                     


                      }
                      
                      if(erst2_5  == 0 && networkmain.controlst2_5 ==1)
                      {
                        networkmain.Client_SendAlarmMessage2(Errst2_5, messagest2_5,"CLEAR");
                        messagest2_5 ="";
                        Errst2_5 ="";
                        networkmain.controlst2_5=0;
                      }
                      

                        if(erst2_6 > 0 && erst2_6 != erst2 && erst2_6 != erst2_5 && erst2_6 != erst2_4  && erst2_6 != erst2_3 && erst2_6 != erst2_2 && erst2_6 != erst2_1 && networkmain.controlst2_6 ==0)
                      {
                                Errst2_6 =erst2_6.ToString();
                                networkmain.controlst2_6 =1;
                               messagest2_6 = Stn2ErrToMsg(erst2_6);
                               networkmain.Client_SendAlarmMessage2(erst2_6.ToString(), messagest2_6 ,"SET");                     


                      }
                      
                      if(erst2_6  == 0 && networkmain.controlst2_6 ==1)
                      {
                        networkmain.Client_SendAlarmMessage2(Errst2_6, messagest2_6,"CLEAR");
                        messagest2_6 ="";
                        Errst2_6 ="";
                        networkmain.controlst2_6=0;
                      }
                      

                       if(erst2_7 > 0 && erst2_7 != erst2 && erst2_7 != erst2_6 && erst2_7 != erst2_5 && erst2_7 != erst2_4  && erst2_7 != erst2_3 && erst2_7 != erst2_2 && erst2_7 != erst2_1 && networkmain.controlst2_7 ==0)
                      {
                                Errst2_7 =erst2_7.ToString();
                                networkmain.controlst2_7 =1;
                               messagest2_7 = Stn2ErrToMsg(erst2_7);
                               networkmain.Client_SendAlarmMessage2(erst2_7.ToString(), messagest2_7 ,"SET");                     


                      }
                      
                      if(erst2_7  == 0 && networkmain.controlst2_7 ==1)
                      {
                        networkmain.Client_SendAlarmMessage2(Errst2_7, messagest2_7,"CLEAR");
                        messagest2_7 ="";
                        Errst2_7 ="";
                        networkmain.controlst2_7=0;
                      }
                    if (PLCQueryRx[PLCQueryRx_DM100 + 16] == 0x4) //d108
                    {
                        PLCWriteCommand[PLCWriteCommand_DM200 + 60] = 0x0; //D230, Station2 Pause trigger
                    }
                    

                    #endregion


                    #region Checke printer Connection at ST2


                    Ping PingPrinter2 = new Ping();

                      try
                      {

                          if ((PLCQueryRx[PLCQueryRx_DM134] == 0x01)
                              &&
                              (PLCWriteCommand[PLCWriteCommand_DM427] == 0x00)
                             )
                          {


                              PingReply PR2 = PingPrinter2.Send("192.168.3.224");
                              if (PR2.Status == IPStatus.Success)
                              {
                                  PLCWriteCommand[PLCWriteCommand_DM427] = 0x09;
                              }
                              else if (PR2.Status == IPStatus.DestinationHostUnreachable)
                              {
                                  PLCWriteCommand[PLCWriteCommand_DM427] = 0xFF;
                              }

                          }


                          if (PLCQueryRx[PLCQueryRx_DM134] == 0x00)
                          {
                              PLCWriteCommand[PLCWriteCommand_DM427] = 0x00;

                          }



                      }
                      catch
                      {


                      }







                      #endregion


                }
                catch (Exception ex)
                {
                    log.Error(ex.ToString());
                }
            }
        }
    }
}
