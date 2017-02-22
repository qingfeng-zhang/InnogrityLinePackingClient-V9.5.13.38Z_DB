using NLog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Xml;
using IGTwpf;
using System.Collections.Concurrent;

namespace InnogrityLinePackingClient
{
    partial class NetworkThread
    {
       public Logger parameter = LogManager.GetLogger("ParameterChanging");

     //  public Logger Blocktest = LogManager.GetLogger("BlockTest");
       public Logger checkresultST5 = LogManager.GetLogger("CheckResultST5");

       public void AutoDeQ()
       {
           ATDeqOn = true;
           while (MyEventQ.Qcount())
           {
               string value;
               value = MyEventQ.Rmov();

               if (value != "")
               {
                   value = SReplace2(value, new string[] { "\0" }, "");
                   value = value.Trim();
                  // networkmain.Client_SendEventMsg(value);
                   EvtLog.Info("Send OUT:  " + value);
                    char[] delimiterChars = { ';', '\t' };

                    String[] details = value.Split(delimiterChars);
                    if (details[0].StartsWith("EV"))
                    {
                        //event

                        int stn = Int32.Parse(details[0].Substring(2, 1));
                        String code = details[0].Substring(2);
                        EvtLog.Debug("DB Insert to EventhEventHistoyistroy " + stn + " ," + code);
                        dbUtils.EventHistory_Insert(stn, code, "E");
                    }
                    else if (details[0].StartsWith("RJ"))
                    {
                        //reject 
                        int stn = 10;
                        String code = details[0].Substring(2);
                        EvtLog.Debug("DB Insert to EventHistoy " + stn + " ," + code);
                        dbUtils.EventHistory_Insert(stn, code, "R");
                    }
                }
               else
               {
                   EvtLog.Info("Send OUT: Null ");
               }
           }
           ATDeqOn = false ;
       }
       private static string SReplace2(string s, string[] separators, string newVal)
       {
           string[] temp;
           temp = s.Split(separators, StringSplitOptions.RemoveEmptyEntries);
           return string.Join(newVal, temp);
       }
       public void ClearDataForPURGE()
        {
            if (AllLabels.Capacity == 0) //if nothing in All Labels array
            {
             
            }
            else
            {      
               // int flushcount = 1; //to number each FL in log
                    foreach (var thing in AllLabels)
                    {
                        networkmain.UpdateRJLabel(thing.ToString(),"RJ","999");  
                        PurgePau.Info("PURGE Finishing Label by Middleware "+thing.ToString());
                      //flushcount++;
                        Thread.Sleep(10);
                    }
                                   

                  
                
            }
        }


      public void ParameterChange(object msgobj)
        {
             
          
            MainNetworkClass networkmain = (MainNetworkClass)msgobj;
            PLCTelnet2 = new TelnetClient();
            #region Connection PLC1,PLC2,Micron Server
            Log1.Info("ParameterChange Thread Start");
            while (!bTerminate)
            {
                try
                {
                  Thread.Sleep(100);
                    #region PLC 1
                    //PLC Read Write Cycle
                    if (PLCTelnet != null)
                        if (PLCTelnet.connected) {

                          #region Parameter Change
                           //bool bypass = true;



 










                            if ((PLCWriteCommand[459] == 0x08 && PLCQueryRx[263] == 0x08 )) //D419 and D126
                          {
                          

                             byte[] PName = new byte[2];
                            Array.Copy(PLCQueryRx, 265, PName, 0,2);  //D127                           
                            Int32 PNint = (Int32)(BitConverter.ToInt16(PName, 0));
                            ParaName = PNint.ToString();
                          

                             byte[] POld = new byte[4];
                            Array.Copy(PLCQueryRx, 267, POld, 0,4);  //D128~D129                          
                            Int32 POldint = (Int32)(BitConverter.ToInt32(POld, 0));
                            ParaOldValue = POldint.ToString();



                             byte[] PNew = new byte[4];
                            Array.Copy(PLCQueryRx, 271, PNew, 0,4);  //D130~D131                           
                            Int32 PNewint = (Int32)(BitConverter.ToInt32(PNew, 0));
                            ParaNewValue = PNewint.ToString();

                            if (PNint != -9999 && POldint != -9999 && PNewint != -9999 && parameterflag == 0)
                            {

                                try
                                {
                                    ParaName = ParameterDesPLC1(int.Parse(ParaName));


                                    networkmain.Client_SendParameterchange1(ParaName, ParaOldValue, ParaNewValue);
                                    parameter.Info("parameter change at PLC1 " + ParaName + "," + ParaOldValue + "," + ParaNewValue);

                                }
                                catch { 
                                
                                }

                             Thread.Sleep(10);
                             PLCWriteCommand[469] =0x07;//D424
                              parameterflag=1;
                            }

                            if (PNint == -9999 && parameterflag == 1)
                            {
                             PLCWriteCommand[469] =0x00;//D424
                               parameterflag=0;
                            }




                          }
                          if(PLCQueryRx[263]==0x0F && PLCWriteCommand[459] == 0x08)
                          {
                          PLCWriteCommand[459] =0x00;
                            string token =networkmain.Token1.ToString();
                            networkmain.Client_SendParameterchangeLogout("DENY","1","1",token);
                             parameter.Info("parameter change at PLC1 logout "+token);
                          }

                     




                    #endregion


                          #region


                          #endregion

                        #region 1DM Each Stop

                          try{

                           if ((PLCQueryRx[PLCQueryRx_DM198] == 0x01) &&
                                    (PLCQueryRx6[PLCQueryRx_DM5199] == 0x01))

                           {
                           
                           RoyalFlush();

                           
                                    PLCWriteCommand[PLCWriteCommand_DM303] = 0x07;
                             
                                    PLCWriteCommand6[PLCWriteCommand_DM5353] = 0x07;
                           
                           }
                          else
                           {
                                    PLCWriteCommand[PLCWriteCommand_DM303] = 0x00;
                             
                                    PLCWriteCommand6[PLCWriteCommand_DM5353] = 0x00;
                           
                           }

                          }

                                     catch (Exception ex)
                          {
                              Log1.Info(ex.ToString());
                          }

                          #endregion



                            #region test DM6110 to DM6499 Reading

                          byte[] TestPara = new byte[2];
                          Array.Copy(PLCQueryRxPara,11, TestPara, 0, 2);  //D6100                          
                          Int32 TestPara1 = (Int32)(BitConverter.ToInt16(TestPara, 0));
                          int TestParastring = TestPara1;
                          if (TestParastring > 0)
                          {
                              //Blocktest.Info("PLC1 Testing DM6100 ," + TestParastring);
                          }

                          byte[] TestPara2 = new byte[2];
                          Array.Copy(PLCQueryRxPara, 809, TestPara2, 0, 2);  //D6499                            
                          Int32 TestPara3 = (Int32)(BitConverter.ToInt16(TestPara2, 0));
                          int TestParastring1 = TestPara3;
                          if (TestParastring1 > 0)
                          {
                             // Blocktest.Info("PLC1 Testing DM6499 ," + TestParastring1);
                          }


                            #endregion


                        }
                  #endregion
                  #region PLC 2
                    if (PLCTelnet2.connected)
                    {
                       
                        


                           #region Parameter Change
                        //bool bypass = true;

                          if((PLCWriteCommand6[315] ==0x08 && PLCQueryRx6[353]==0x08)) //  ,D5347,D5171
                          {
                          

                             byte[] PName = new byte[2];
                            Array.Copy(PLCQueryRx6, 303, PName, 0,2);  //D5146                           
                            Int32 PNint = (Int32)(BitConverter.ToInt16(PName, 0));
                            ParaName1 = PNint.ToString();
                          

                             byte[] POld = new byte[4];
                            Array.Copy(PLCQueryRx6, 383, POld, 0,4);  //D5186~D5187                         
                            Int32 POldint = (Int32)(BitConverter.ToInt32(POld, 0));
                            ParaOldValue1 = POldint.ToString();



                             byte[] PNew = new byte[4];
                            Array.Copy(PLCQueryRx6,387, PNew, 0,4);  //D5188~D5189                           
                            Int32 PNewint = (Int32)(BitConverter.ToInt32(PNew, 0));
                            ParaNewValue1 = PNewint.ToString();

                            if (PNint != -9999 && POldint != -9999 && PNewint != -9999 && parameterflag2 == 0)
                            {

                                try
                                {
                                    ParaName1 = ParameterDesPLC2(int.Parse(ParaName1));



                                    networkmain.Client_SendParameterchange1(ParaName1, ParaOldValue1, ParaNewValue1);
                                    parameter.Info("parameter change at PLC2 " + ParaName1 + "," + ParaOldValue1 + "," + ParaNewValue1);
                                }
                                catch
                                { 
                                
                                }
                             Thread.Sleep(10);
                             PLCWriteCommand6[465] =0x07;//D5422
                               parameterflag2 =1;
                            }

                            if (PNint == -9999 && parameterflag2 == 1)
                            {
                             PLCWriteCommand6[465] =0x00;//D5422
                              parameterflag2 =0;
                            }



                            #region Sealer parameter

                            byte[] SealerPName = new byte[2];
                            Array.Copy(PLCQueryRx6, 279, SealerPName, 0,2);  //D5134                          
                            Int32 SealerPNint = (Int32)(BitConverter.ToInt16(SealerPName, 0));
                            SealerParaName = SealerPNint.ToString();
                          

                             byte[] SealerPOld = new byte[12];
                            Array.Copy(PLCQueryRx6, 281, SealerPOld, 0,12);  //D5135~D5140                        
                            Int32 SealerPOldint = (Int32)(BitConverter.ToInt16(SealerPOld, 0));
                            SealerParaOldValue = SealerPOldint.ToString();
                            string SealerOld = System.Text.Encoding.Default.GetString(SealerPOld);



                             byte[] SealerPNew = new byte[12];
                            Array.Copy(PLCQueryRx6,371, SealerPNew, 0,12);  //D5180~D5185                           
                            Int32 SealerPNewint = (Int32)(BitConverter.ToInt16(SealerPNew, 0));
                            SealerParaNewValue = SealerPNewint.ToString();
                            string SealerNew = System.Text.Encoding.Default.GetString(SealerPNew);

                            if( SealerPNint>0 && SealerPOldint>0 && SealerPNewint>0  && Sealerparameterflag == 0)
                            {

                                networkmain.Client_SendParameterchange1(SealerParaName, SealerOld, SealerNew);
                                parameter.Info("Sealer parameter change at PLC2 " + SealerParaName + "," + SealerOld + "," + SealerNew);
                             Thread.Sleep(10);
                             PLCWriteCommand6[479] =0x07;//D5429
                               Sealerparameterflag =1;
                            }

                            if( SealerPNint==0   &&  Sealerparameterflag == 1)
                            {
                             PLCWriteCommand6[479] =0x00;//D5429
                              Sealerparameterflag =0;
                            }









                            #endregion


                          }


                          if(PLCQueryRx6[353]==0x0F &&  PLCWriteCommand6[315] ==0x08)
                          {
                            PLCWriteCommand6[315] =0x00;
                            string token =networkmain.Token2.ToString();
                            networkmain.Client_SendParameterchangeLogout("DENY","1","2",token);
                            parameter.Info("parameter change at PLC2 logout "+token);
                          }

                         




                    #endregion



                      #region check st5 RJ result

                      
                            //byte[] bcarrayst5check = new byte[10];
                            //Array.Copy(PLCQueryRx6, 121, bcarrayst5check, 0, 10);  //D5055
                            //St5CheckFL = System.Text.Encoding.Default.GetString(bcarrayst5check);
                            //if (St5CheckFL !="\0\0\0\0\0\0\0\0\0\0")
                            //{
                            //    checkresultST5.Info("FL For ST5 give to ST6" + St5CheckFL);
                            //}

                            //else
                            //{
                            //    St5CheckFL = "\0\0\0\0\0\0\0\0\0\0";
                            //}
                             //byte[] bcarrayst5checkRJ = new byte[2];
                             //Array.Copy(PLCQueryRx6, 219, bcarrayst5checkRJ, 0, 2);  //D5104
                             //string   St5CheckFLRJ = System.Text.Encoding.Default.GetString(bcarrayst5checkRJ);

                             //checkresultST5.Info("FL RJ For ST5 give to ST6" + St5CheckFLRJ);




                             // byte[] bcarrayst5checkS1 = new byte[2];
                             // Array.Copy(PLCQueryRx6, 165, bcarrayst5checkS1, 0, 2);  //D5077
                             // string   St5CheckFLS1 = System.Text.Encoding.Default.GetString(bcarrayst5checkS1);

                             //checkresultST5.Info("FL RJ For ST5 Sealer 1 give from Server===>" + St5CheckFLS1);

                             //byte[] bcarrayst5checkS2 = new byte[2];
                             // Array.Copy(PLCQueryRx6, 167, bcarrayst5checkS2, 0, 2);  //D5078
                             // string   St5CheckFLS2 = System.Text.Encoding.Default.GetString(bcarrayst5checkS2);

                             //checkresultST5.Info("FL RJ For ST5 Sealer 2 give from Server===>" + St5CheckFLS2);



                             // byte[] bcarrayst5checkS3 = new byte[2];
                             // Array.Copy(PLCQueryRx6, 169, bcarrayst5checkS3, 0, 2);  //D5077
                             // string   St5CheckFLS3 = System.Text.Encoding.Default.GetString(bcarrayst5checkS3);

                             // checkresultST5.Info("FL RJ For ST5 Sealer 3 give from Server===>" + St5CheckFLS3);




                      #endregion



                          #region test DM6110 to DM6499 Reading

                          byte[] TestPara = new byte[2];
                          Array.Copy(PLCQueryRxParaPlc2, 11, TestPara, 0, 2);  //D6100                          
                          Int32 TestPara1 = (Int32)(BitConverter.ToInt16(TestPara, 0));
                          int TestParastring = TestPara1;
                          if (TestParastring > 0)
                          {
                             // Blocktest.Info("PLC2 Testing DM6100 ," + TestParastring);
                          }

                          byte[] TestPara2 = new byte[2];
                          Array.Copy(PLCQueryRxParaPlc2, 809, TestPara2, 0, 2);  //D6499                            
                          Int32 TestPara3 = (Int32)(BitConverter.ToInt16(TestPara2, 0));
                          int TestParastring1 = TestPara3;
                          if (TestParastring1 > 0)
                          {
                              //Blocktest.Info("PLC2 Testing DM6499 ," + TestParastring1);
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
    public class EventQuece
    {
        ConcurrentQueue<string> myQ = new ConcurrentQueue<string>();
        public void AddQ(string EventMsg)
        {
            myQ.Enqueue(EventMsg);
        }
        public string Rmov()
        {
            string RtnMesg = "";
            myQ.TryDequeue(out RtnMesg);
            return RtnMesg;
        }
        public bool Qcount()
        {
            bool Qcontain;
            int Qcount = myQ.Count;
            if (Qcount > 0)
            {
                Qcontain = true;
            }
            else
            {
                Qcontain = false;
            }
            return Qcontain;
        }
    }
}