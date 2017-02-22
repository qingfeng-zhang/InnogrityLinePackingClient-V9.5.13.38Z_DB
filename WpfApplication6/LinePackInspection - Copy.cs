using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FHNonProcedure;
using System.Threading;
using InnogrityLinePackingClient.Properties;
namespace BCReader
{
    class LinePackInspection
    {
        #region   Code39 table
        string[,] CODE39_ESCAPE_TABLE = new string[92, 2] {
      {"%U","\0x00"},
      {"$A","\0x01"},
      {"$B","\0x02"},
      {"$C","\0x03"},
      {"$D","\0x04"},
      {"$E","\0x05"},
      {"$F","\0x06"},
      {"$G","\0x07"},
      {"$H","\0x08"},
      {"$I","\0x09"},
      {"$J","\0x0A"},
      {"$K","\0x0B"},
      {"$L","\0x0C"},
      {"$M","\0x0D"},
      {"$N","\0x0E"},
      {"$O","\0x0F"},
      {"$P","\0x10"},
      {"$Q","\0x11"},
      {"$R","\0x12"},
      {"$S","\0x13"},
      {"$T","\0x14"},
      {"$U","\0x15"},
      {"$V","\0x16"},
      {"$W","\0x17"},
      {"$X","\0x18"},
      {"$Y","\0x19"},
      {"$Z","\0x1A"},
      {"%A","\0x1B"},
      {"%B","\0x1C"},
      {"%C","\0x1D"},
      {"%D","\0x1E"},
      {"%E","\0x1F"},
      // Space
      {"/A","!"},
      {"/B","\""},
      {"/C","#"},
      {"/F","&"},
      {"/G","'"},
      {"/H","("},
      {"/I",")"},
      {"/J","*"},
      {"/L",","},
      {"/Z",":"},
      {"%F",";"},
      {"%G","<"},
      {"%H","="},
      {"%I",">"},
      {"%J","?"},
      {"%V","@"},
      {"%K","["},
      {"%L","\\"},
      {"%M","]"},
      {"%N","^"},
      {"%O","_"},
      {"%W","`"},
      {"+A","a"},
      {"+B","b"},
      {"+C","c"},
      {"+D","d"},
      {"+E","e"},
      {"+F","f"},
      {"+G","g"},
      {"+H","h"},
      {"+I","i"},
      {"+J","j"},
      {"+K","k"},
      {"+L","l"},
      {"+M","m"},
      {"+N","n"},
      {"+O","o"},
      {"+P","p"},
      {"+Q","q"},
      {"+R","r"},
      {"+S","s"},
      {"+T","t"},
      {"+U","u"},
      {"+V","v"},
      {"+W","w"},
      {"+X","x"},
      {"+Y","y"},
      {"+Z","z"},
      {"%P","{"},
      {"%Q","|"},
      {"%R","}"},
      {"%S","~"},
      {"%T","\0x7F"},
      {"%X","\0x7F"},
      {"%Y","\0x7F"},
      {"%Z","\0x7F"},
      {"/D","$"},
      {"/E","%"},
      {"/O","/"},
      {"/K","+"}
    };

        #endregion
        #region   Code93 table
        string[,] CODE93_ESCAPE_TABLE = new string[85, 2] {
      {"'U","\0x00"},
      {"&A","\0x01"},
      {"&B","\0x02"},
      {"&C","\0x03"},
      {"&D","\0x04"},
      {"&E","\0x05"},
      {"&F","\0x06"},
      {"&G","\0x07"},
      {"&H","\0x08"},
      {"&I","\0x09"},
      {"&J","\0x0A"},
      {"&K","\0x0B"},
      {"&L","\0x0C"},
      {"&M","\0x0D"},
      {"&N","\0x0E"},
      {"&O","\0x0F"},
      {"&P","\0x10"},
      {"&Q","\0x11"},
      {"&R","\0x12"},
      {"&S","\0x13"},
      {"&T","\0x14"},
      {"&U","\0x15"},
      {"&V","\0x16"},
      {"&W","\0x17"},
      {"&X","\0x18"},
      {"&Y","\0x19"},
      {"&Z","\0x1A"},
      {"'A","\0x1B"},
      {"'B","\0x1C"},
      {"'C","\0x1D"},
      {"'D","\0x1E"},
      {"'E","\0x1F"},
      // Space
      {"(A","!"},
      {"(B","\""},
      {"(C","#"},
      {"(F","&"},
      {"(G","'"},
      {"(H","("},
      {"(I",")"},
      {"(J","*"},
      {"(L",","},
      {"(Z",":"},
      {"'F",";"},
      {"'G","<"},
      {"'H","="},
      {"'I",">"},
      {"'J","?"},
      {"'V","@"},
      {"'K","["},
      {"'L","\\"},
      {"'M","]"},
      {"'N","^"},
      {"'O","_"},
      {"'W","`"},
      {")A","a"},
      {")B","b"},
      {")C","c"},
      {")D","d"},
      {")E","e"},
      {")F","f"},
      {")G","g"},
      {")H","h"},
      {")I","i"},
      {")J","j"},
      {")K","k"},
      {")L","l"},
      {")M","m"},
      {")N","n"},
      {")O","o"},
      {")P","p"},
      {")Q","q"},
      {")R","r"},
      {")S","s"},
      {")T","t"},
      {")U","u"},
      {")V","v"},
      {")W","w"},
      {")X","x"},
      {")Y","y"},
      {")Z","z"},
      {"'P","{"},
      {"'Q","|"},
      {"'R","}"},
      {"'S","~"},
      {"'T","\0x7F"}
    };

        #endregion

        string zplStrings;
        public List<string[]> barcodelist;
        public List<string[]> ocrlist;
        public List<string[]> ocrbarcodelist;
        public List<string[]> GBlist;
        public FHNonProcedureSocket linepackomronsystem;//set as public so that we can use this for testing..
        public string HostAddress { get; set; }
        public int PortNumber { get; set; }


        public void InitVision()//step 1 to connect to vision
        {
            //Send all OCR and BC data
            linepackomronsystem = new FHNonProcedureSocket()
            {
                HostAddress = HostAddress,
                PortNumber = PortNumber
            };
           
        }
        public void ConnectToVision()
        {
            linepackomronsystem.Connect();
        }
        public void DisconnectVision()
        {
            linepackomronsystem.Close();
        }
        public void LoadZPLFile(string zplfilepath)
        {

            try
            {
                zplStrings = System.IO.File.ReadAllText(zplfilepath);
                zplStrings = zplStrings.Replace("\r", string.Empty);
                zplStrings = zplStrings.Replace("\n", string.Empty);
                //zplStrings = SReplace(zplStrings, new string[] { @"\&", "_1E", "_1D", "_04" }, " ");
                int labeltype = IntelLabelCheck(zplfilepath);
                ocrbarcodelist = new List<string[]>();
                barcodelist = new List<string[]>();
                ocrlist = new List<string[]>();
                GBlist = new List<string[]>();
                int barcodeindexer = 0;
                int indexofposition = 0;
                switch (labeltype)
                {
                    case 1: //BOX TYPE
                        int indexB = zplStrings.IndexOf("^FS");//Search refine by GY
                        int LastMemB = 0;
                        while (indexB > 0)// Find all the ^FO
                        {
                            indexofposition = zplStrings.LastIndexOf("^FO", indexB);
                            if (indexofposition > 0 && LastMemB != indexofposition)
                            {
                                LastMemB = indexofposition;
                                string barcode_info = zplStrings.Substring(indexofposition, indexB - indexofposition);
                                //indexofposition++;
                                #region barcode search region
                                //scan for barcodes
                                barcodeindexer = barcode_info.IndexOf("^BC");
                                if (barcodeindexer > 0)
                                {
                                    //extract x y and label information
                                    string coordinates = barcode_info.Substring(3, barcodeindexer - 3);
                                    //split the coordinates
                                    string[] axis = coordinates.Split(',');
                                    for (int i = 0; i < axis.Length; i++)
                                    {
                                        int xindex = axis[i].IndexOf(";");
                                        if (xindex > 0)
                                        {
                                            axis[i] = axis[i].Substring(0, xindex);
                                        }
                                        xindex = axis[i].IndexOf("^");
                                        if (xindex > 0)
                                        {
                                            axis[i] = axis[i].Substring(0, xindex);
                                        }
                                    }
                                    int x = (int)(int.Parse(axis[0]) * 1.983);//assume ratio is 2x
                                    int y = (int)(int.Parse(axis[1]) * 1.983);
                                    axis[0] = x.ToString();
                                    axis[1] = y.ToString();
                                    int stringindex = barcode_info.IndexOf("^FD");
                                    string verificationstring = barcode_info.Substring(stringindex + 3, barcode_info.Length - 3 - stringindex);
                                    verificationstring = CODE128_INVOCATION_Conversion(verificationstring); //Added by GY 14/9/2016
                                    string orientationstring = barcode_info.Substring(barcodeindexer + 3, 1);
                                    //check orientation
                                    string mm = "0";
                                    if (orientationstring == "N")
                                    {
                                        mm = "0";
                                    }
                                    else
                                    {
                                        mm = "1";
                                    }
                                    string[] array = { axis[0], axis[1], orientationstring, verificationstring, "CODE128" };
                                    barcodelist.Add(array);
                                }
                                #endregion
                                indexB = zplStrings.IndexOf("^FS", indexB + 3);
                            }
                            else
                            {
                                indexB = zplStrings.IndexOf("^FS", indexB + 4);
                            }
                        }
                        #region OCR for intel Boxtype
                        //  string[] templist1 = new string[] { "^FO37,103", "^FO112,288", "^FO392,286", "^FO36,379" };
                        string[] templist1 = new string[] { "^FO37,110", "^FO36,379", "^FO392,286", "^FO112,288" };
                        for (int i = 0; i < templist1.Length; i++)
                        {

                            int tmpindex = zplStrings.IndexOf(templist1[i]);
                            int bcindex = zplStrings.IndexOf("^FS", tmpindex);
                            string tmpocr = zplStrings.Substring(tmpindex + 3, bcindex - tmpindex - 3);
                            int Aindex = tmpocr.IndexOf("^A");
                            string tmpaxis = tmpocr.Substring(0, Aindex);
                            string[] axis = tmpaxis.Split(',');
                            for (int j = 0; j < axis.Length; j++)
                            {
                                int xindex = axis[j].IndexOf(";");
                                if (xindex > 0)
                                {
                                    axis[j] = axis[j].Substring(0, xindex);
                                }
                                xindex = axis[j].IndexOf("^");
                                if (xindex > 0)
                                {
                                    axis[j] = axis[j].Substring(0, xindex);
                                }
                            }
                            int x = (int)(int.Parse(axis[0]) * 2.017);//assume ratio is 2x
                            int y = (int)(int.Parse(axis[1]) * 1.986);
                            axis[0] = x.ToString();
                            axis[1] = y.ToString();
                            int bcstrindex = tmpocr.IndexOf("^FD");
                            string verificationstring = tmpocr.Substring(bcstrindex + 3);
                            int UpperCaseString = StringLowerCaseDetect(verificationstring);
                            string[] array = { axis[0], axis[1], "1", "1", verificationstring, "0", "N", UpperCaseString.ToString() };
                            ocrbarcodelist.Add(array);
                        }
                        #endregion
                        break;
                    case 2: //INTEL TYPE
                        string[] templist2;
                        templist2 = zplStrings.Split(new string[] { "^FS" }, StringSplitOptions.RemoveEmptyEntries);
                        #region bcode for intel boxtype
                        int[] bcstep2 = { 7, 9, 13, 18, 22, 26, 30, 34, 38, 41, 52 };
                        for (int i = 0; i < 11; i++)
                        {

                            int tmpindex = templist2[bcstep2[i]].IndexOf("^FO");
                            int bcindex = templist2[bcstep2[i]].IndexOf("^BC");
                            string tmpaxis = templist2[bcstep2[i]].Substring(tmpindex + 3, bcindex - tmpindex - 3);
                            //tmpstring.Replace("/r/n", "");
                            //tmpstring.Replace(" ", "");
                            //split the coordinates
                            string[] axis = tmpaxis.Split(',');
                            int x = (int)(int.Parse(axis[0]) * 1.983);//assume ratio is 2x
                            int y = (int)(int.Parse(axis[1]) * 1.983);
                            axis[0] = x.ToString();
                            axis[1] = y.ToString();
                            int bcstrindex = templist2[bcstep2[i]].IndexOf("^FD");
                            string verificationstring = templist2[bcstep2[i]].Substring(bcstrindex + 3);
                            verificationstring = CODE128_INVOCATION_Conversion(verificationstring); //Added by GY 14/9/2016
                            string orientationstring = templist2[bcstep2[i]].Substring(bcindex + 3, 1);
                            string mm = "0";
                            if (orientationstring == "N")
                            {
                                mm = "0";
                            }
                            else
                            {
                                mm = "1";
                            }
                            string[] array = { axis[0], axis[1], orientationstring, verificationstring, "CODE128" };
                            //templist = Regex.Split(zplStrings, "\r\n");
                            barcodelist.Add(array);
                        }
                        #endregion
                        #region OCR for intel Boxtype
                        int[] ocrstep2 = { 6, 8, 12, 17, 21, 25, 29, 33, 37, 40, 51 };
                        //                 0  1  2   3   4   5  6   7   8   9  10   11  12  13  14  15  16  17  18  19
                        for (int i = 0; i < 11; i++)
                        {

                            int tmpindex = templist2[ocrstep2[i]].IndexOf("^FO");
                            int bcindex = templist2[ocrstep2[i]].IndexOf("^A");
                            string tmpaxis = templist2[ocrstep2[i]].Substring(tmpindex + 3, bcindex - tmpindex - 3);
                            //tmpstring.Replace("/r/n", "");
                            //tmpstring.Replace(" ", "");
                            //split the coordinates
                            string[] axis = tmpaxis.Split(',');
                            int x, y;
                            if (i == 9)
                            {
                                x = (int)(int.Parse(axis[0]) * 2.017) + 80;//assume ratio is 2x
                                y = (int)(int.Parse(axis[1]) * 1.986);
                            }
                            else
                            {
                                x = (int)(int.Parse(axis[0]) * 2.017);//assume ratio is 2x
                                y = (int)(int.Parse(axis[1]) * 1.986);
                            }
                            axis[0] = x.ToString();
                            axis[1] = y.ToString();
                            int bcstrindex = templist2[ocrstep2[i]].IndexOf("^FD");
                            int cistrindex = templist2[ocrstep2[i]].IndexOf("^CI");
                            string verificationstring = templist2[ocrstep2[i]].Substring(bcstrindex + 3);
                            string orientationstring = templist2[ocrstep2[i]].Substring(bcindex + 3, 1);
                            string tmpfontsize = templist2[ocrstep2[i]].Substring(bcindex + 5, cistrindex - bcindex - 5);
                            string[] fontsize = tmpfontsize.Split(',');
                            int UpperCaseString = StringLowerCaseDetect(verificationstring);
                            string orientationstr = templist2[ocrstep2[i]].Substring(bcindex + 3, 1);
                            string mm = "";
                            if (orientationstr == "N")
                            {
                                mm = "0";

                            }
                            else
                            {
                                mm = "1";
                            }

                            string[] array = { axis[0], axis[1], fontsize[0], fontsize[1], verificationstring, templist2[ocrstep2[i]].Substring(bcindex + 1, 2), orientationstring, UpperCaseString.ToString() };
                            //templist = Regex.Split(zplStrings, "\r\n");

                            ocrbarcodelist.Add(array);

                        }
                        #endregion
                        break;
                    //case 3: //SPEKTEK
                    //    int indexofFX;
                    //    int index2 = zplStrings.IndexOf("^BY");//search for first occurance of ^FO
                    //    while (index2 > 0)
                    //    {

                    //        indexofFX = zplStrings.IndexOf("^FS", index2 + 3);
                    //        string barcode_info2 = zplStrings.Substring(index2, indexofFX - index2);
                    //        barcodeindexer = barcode_info2.IndexOf("^FO");
                    //        if (barcodeindexer > 0)
                    //        {
                    //            //extract x y and label information
                    //            string coordinates = barcode_info2.Substring(3, barcodeindexer - 3);
                    //            //split the coordinates
                    //            string[] axis = coordinates.Split(',');
                    //            int x = (int)(int.Parse(axis[0]) * 1.983);//assume ratio is 2x
                    //            int  y = (int)(double.Parse(axis[1]) * 1.983);
                    //            axis[0] = x.ToString();
                    //            axis[1] = y.ToString();
                    //            int stringindex = barcode_info2.IndexOf("^FD");
                    //            string verificationstring = barcode_info2.Substring(stringindex + 3, barcode_info2.Length - 3 - stringindex);
                    //            if (verificationstring.IndexOf(">:") == 0)
                    //            {
                    //                verificationstring = verificationstring.Substring(2, verificationstring.Length - 2);//check with micron on this character string
                    //            }
                    //            string orientationstring = barcode_info2.Substring(barcodeindexer + 3, 1);
                    //            //check orientation

                    //            string[] array = { axis[0], axis[1], orientationstring, verificationstring, "CODE128" };
                    //            barcodelist.Add(array);
                    //            //OCR Search Region


                    //            index2 = zplStrings.IndexOf("^BY", index2 + 3);
                    //        }

                    //    }
                    //    #region OCR String SearchStart
                    //    int index5 = zplStrings.IndexOf("^FO");//search for first occurance of ^FO
                    //    while (index5 > 0)
                    //    {

                    //        indexofFX = zplStrings.IndexOf("^FS", index5 + 3);
                    //        string OCR_info = zplStrings.Substring(index5, indexofFX - index5);
                    //        int FontIndicator = OCR_info.IndexOf("^A");
                    //        if (FontIndicator > 0)//its a font, extract font information
                    //        {
                    //            string fonttype = OCR_info.Substring(FontIndicator + 2, 1);
                    //            string fontorientation = OCR_info.Substring(FontIndicator + 3, 1);
                    //            string coordinates5 = OCR_info.Substring(3, FontIndicator - 3);
                    //            //split the coordinates
                    //            int yindex = coordinates5.IndexOf("^");
                    //            if (yindex > 0)
                    //            {
                    //                coordinates5 = coordinates5.Substring(0, yindex);
                    //            }
                    //            string[] axis5 = coordinates5.Split(',');
                    //            for (int i = 0; i < axis5.Length; i++)
                    //            {
                    //                int xindex = axis5[i].IndexOf(";");

                    //                if (xindex > 0)
                    //                {
                    //                    axis5[i] = axis5[i].Substring(0, xindex);
                    //                }
                    //            }
                    //            int x5 = (int)(int.Parse(axis5[0]) * 2.017);
                    //            int y5 = (int)(int.Parse(axis5[1]) * 1.986);
                    //            axis5[0] = x5.ToString();
                    //            axis5[1] = y5.ToString();
                    //            int scaleindex = OCR_info.IndexOf("^", FontIndicator + 1);
                    //            string scale = OCR_info.Substring(FontIndicator + 5, scaleindex - 5 - FontIndicator);
                    //            string[] xyscale = scale.Split(',');
                    //            int stringindex5 = OCR_info.IndexOf("^FD");

                    //            string verificationstring5 = OCR_info.Substring(stringindex5 + 3, OCR_info.Length - 3 - stringindex5);


                    //            int UpperCaseString = StringLowerCaseDetect(verificationstring5);
                    //            string[] array5 = { axis5[0], axis5[1], xyscale[0], xyscale[1], verificationstring5, fonttype, fontorientation, UpperCaseString.ToString() };
                    //            //string ocrfont = xyscale[0] + "_"+xyscale[1] + "_"+fonttype;//incase font type is required can use this naming convention
                    //            ocrlist.Add(array5);
                    //          }
                    //        index5 = zplStrings.IndexOf("^FO", index5 + 3);
                    //    }
                    //    BarcoderelatedOCRSearch();
                    //    GBidentifier();
                    //    #endregion
                    //    break;
                    default:

                        // int indexFO = zplStrings.IndexOf("^FO");//search for first occurance of ^FO
                        int index = zplStrings.IndexOf("^FS");//Search refine by GY
                        int LastMem = 0;
                        while (index > 0)// Find all the ^FO
                        {
                            indexofposition = zplStrings.LastIndexOf("^FO", index);
                            if (indexofposition > 0 && LastMem != indexofposition)
                            {
                                LastMem = indexofposition;
                                string barcode_info = zplStrings.Substring(indexofposition, index - indexofposition);
                                //indexofposition++;
                                #region barcode search region
                                //scan for barcodes
                                barcodeindexer = barcode_info.IndexOf("^BC");
                                if (barcodeindexer > 0)
                                {
                                    //extract x y and label information
                                    string coordinates = barcode_info.Substring(3, barcodeindexer - 3);
                                    //split the coordinates
                                    string[] axis = coordinates.Split(',');
                                    for (int i = 0; i < axis.Length; i++)
                                    {
                                        int xindex = axis[i].IndexOf(";");
                                        if (xindex > 0)
                                        {
                                            axis[i] = axis[i].Substring(0, xindex);
                                        }
                                        xindex = axis[i].IndexOf("^");
                                        if (xindex > 0)
                                        {
                                            axis[i] = axis[i].Substring(0, xindex);
                                        }
                                    }
                                    int x = (int)(int.Parse(axis[0]) * 1.983);//assume ratio is 2x
                                    int y = (int)(int.Parse(axis[1]) * 1.983);
                                    axis[0] = x.ToString();
                                    axis[1] = y.ToString();
                                    int stringindex = barcode_info.IndexOf("^FD");
                                    string verificationstring = barcode_info.Substring(stringindex + 3, barcode_info.Length - 3 - stringindex);
                                    verificationstring = CODE128_INVOCATION_Conversion(verificationstring); //Added by GY 14/9/2016
                                    string orientationstring = barcode_info.Substring(barcodeindexer + 3, 1);
                                    //check orientation
                                    string mm = "0";
                                    if (orientationstring == "N")
                                    {
                                        mm = "0";
                                    }
                                    else
                                    {
                                        mm = "1";
                                    }
                                    string[] array = { axis[0], axis[1], orientationstring, verificationstring, "CODE128" };
                                    barcodelist.Add(array);
                                }
                                barcodeindexer = barcode_info.IndexOf("^B3");
                                if (barcodeindexer > 0)
                                {
                                    //extract x y and label information
                                    string coordinates = barcode_info.Substring(3, barcodeindexer - 3);
                                    //split the coordinates
                                    string[] axis = coordinates.Split(',');
                                    for (int i = 0; i < axis.Length; i++)
                                    {
                                        int xindex = axis[i].IndexOf(";");
                                        if (xindex > 0)
                                        {
                                            axis[i] = axis[i].Substring(0, xindex);
                                        }
                                        xindex = axis[i].IndexOf("^");
                                        if (xindex > 0)
                                        {
                                            axis[i] = axis[i].Substring(0, xindex);
                                        }
                                    }
                                    int locationychk = int.Parse(axis[1]);
                                    if (locationychk < 450) // assume all Y is less then 450
                                    {
                                        int x = (int)(int.Parse(axis[0]) * 1.983);//assume ratio is 2x
                                        int y = (int)(int.Parse(axis[1]) * 1.983);
                                        axis[0] = x.ToString();
                                        axis[1] = y.ToString();
                                        int stringindex = barcode_info.IndexOf("^FD");
                                        string verificationstring = barcode_info.Substring(stringindex + 3, barcode_info.Length - 3 - stringindex);
                                        string orientationstring = barcode_info.Substring(barcodeindexer + 3, 1);
                                        //check orientation
                                        // List<string> escapedArray = new List<string>();
                                        //foreach(string barcode in array) {
                                        //  string resultBarcode = barcode;
                                        //string resultBarcode ="";
                                        //resultBarcode = 
                                        //  verificationstring.Replace(@"/Z",":");
                                        int k = CODE39_ESCAPE_TABLE.GetLength(0);
                                        for (int i = 0; i < CODE39_ESCAPE_TABLE.GetLength(0); i++)
                                        {
                                            verificationstring = verificationstring.Replace(CODE39_ESCAPE_TABLE[i, 0], CODE39_ESCAPE_TABLE[i, 1]);
                                        }
                                        //   escapedArray.Add(resultBarcode);
                                        // don't change  >:
                                        if (verificationstring.IndexOf(">:") == 0)
                                        {
                                            verificationstring = verificationstring.Substring(2, verificationstring.Length - 2);//check with micron on this character string
                                        }
                                        /*
                                        k = CODE128_ESCAPE_TABLE.GetLength(0);
                                        for (int i = 0; i < CODE39_ESCAPE_TABLE.GetLength(0); i++)
                                        {
                                            verificationstring = verificationstring.Replace(CODE128_ESCAPE_TABLE[i, 0], CODE128_ESCAPE_TABLE[i, 1]);
                                        }
                                        */
                                        string[] array = { axis[0], axis[1], orientationstring, verificationstring, "CODE39" };

                                        // barcodelist.Add(escapedArray.ToArray());
                                        barcodelist.Add(array);
                                    }
                                }

                                barcodeindexer = barcode_info.IndexOf("^BA");
                                if (barcodeindexer > 0)
                                {
                                    //extract x y and label information
                                    string coordinates = barcode_info.Substring(3, barcodeindexer - 3);
                                    //split the coordinates
                                    string[] axis = coordinates.Split(',');
                                    for (int i = 0; i < axis.Length; i++)
                                    {
                                        int xindex = axis[i].IndexOf(";");
                                        if (xindex > 0)
                                        {
                                            axis[i] = axis[i].Substring(0, xindex);
                                        }
                                        xindex = axis[i].IndexOf("^");
                                        if (xindex > 0)
                                        {
                                            axis[i] = axis[i].Substring(0, xindex);
                                        }
                                    }
                                    int x = (int)(int.Parse(axis[0]) * 1.983);//assume ratio is 2x
                                    int y = (int)(int.Parse(axis[1]) * 1.983);
                                    axis[0] = x.ToString();
                                    axis[1] = y.ToString();
                                    int stringindex = barcode_info.IndexOf("^FD");
                                    string verificationstring = barcode_info.Substring(stringindex + 3, barcode_info.Length - 3 - stringindex);
                                    string orientationstring = barcode_info.Substring(barcodeindexer + 3, 1);
                                    //check orientation
                                    for (int i = 0; i < CODE93_ESCAPE_TABLE.GetLength(0); i++)
                                    {
                                        verificationstring = verificationstring.Replace(CODE93_ESCAPE_TABLE[i, 0], CODE93_ESCAPE_TABLE[i, 1]);
                                    }
                                    string[] array = { axis[0], axis[1], orientationstring, verificationstring, "CODE93" };
                                    barcodelist.Add(array);
                                }
                                barcodeindexer = barcode_info.IndexOf("^BL");
                                if (barcodeindexer > 0)
                                {
                                    //extract x y and label information
                                    string coordinates = barcode_info.Substring(3, barcodeindexer - 3);
                                    //split the coordinates
                                    string[] axis = coordinates.Split(',');
                                    for (int i = 0; i < axis.Length; i++)
                                    {
                                        int xindex = axis[i].IndexOf(";");
                                        if (xindex > 0)
                                        {
                                            axis[i] = axis[i].Substring(0, xindex);
                                        }
                                        xindex = axis[i].IndexOf("^");
                                        if (xindex > 0)
                                        {
                                            axis[i] = axis[i].Substring(0, xindex);
                                        }
                                    }
                                    int x = (int)(int.Parse(axis[0]) * 1.983);//assume ratio is 2x
                                    int y = (int)(int.Parse(axis[1]) * 1.983);
                                    axis[0] = x.ToString();
                                    axis[1] = y.ToString();
                                    int stringindex = barcode_info.IndexOf("^FD");
                                    string verificationstring = barcode_info.Substring(stringindex + 3, barcode_info.Length - 3 - stringindex);
                                    string orientationstring = barcode_info.Substring(barcodeindexer + 3, 1);
                                    //check orientation
                                    string[] array = { axis[0], axis[1], orientationstring, verificationstring, "LOGMARS" };
                                    barcodelist.Add(array);
                                }

                                barcodeindexer = barcode_info.IndexOf("^BX");
                                if (barcodeindexer > 0)
                                {
                                    //extract x y and label information
                                    string coordinates = barcode_info.Substring(3, barcodeindexer - 3);
                                    //split the coordinates
                                    string[] axis = coordinates.Split(',');
                                    for (int i = 0; i < axis.Length; i++)
                                    {
                                        int xindex = axis[i].IndexOf(";");
                                        if (xindex > 0)
                                        {
                                            axis[i] = axis[i].Substring(0, xindex);
                                        }
                                        xindex = axis[i].IndexOf("^");
                                        if (xindex > 0)
                                        {
                                            axis[i] = axis[i].Substring(0, xindex);
                                        }
                                    }
                                    int x = (int)(int.Parse(axis[0]) * 1.983);//assume ratio is 2x
                                    int y = (int)(int.Parse(axis[1]) * 1.983);
                                    axis[0] = x.ToString();
                                    axis[1] = y.ToString();
                                    int stringindex = barcode_info.IndexOf("^FD");
                                    string verificationstring = barcode_info.Substring(stringindex + 3, barcode_info.Length - 3 - stringindex);
                                    char[] xx = verificationstring.ToCharArray();

                                    string orientationstring = barcode_info.Substring(barcodeindexer + 3, 1);
                                    //check orientation
                                    string[] array = { axis[0], axis[1], orientationstring, verificationstring, "DATAMATRIX" };
                                    barcodelist.Add(array);
                                }

                                barcodeindexer = barcode_info.IndexOf("^B7");
                                if (barcodeindexer > 0)
                                {
                                    //extract x y and label information
                                    string coordinates = barcode_info.Substring(3, barcodeindexer - 3);
                                    //split the coordinates
                                    string[] axis = coordinates.Split(',');
                                    for (int i = 0; i < axis.Length; i++)
                                    {
                                        int xindex = axis[i].IndexOf(";");
                                        if (xindex > 0)
                                        {
                                            axis[i] = axis[i].Substring(0, xindex);
                                        }
                                        xindex = axis[i].IndexOf("^");
                                        if (xindex > 0)
                                        {
                                            axis[i] = axis[i].Substring(0, xindex);
                                        }
                                    }
                                    int x = (int)(int.Parse(axis[0]) * 1.983);//assume ratio is 2x
                                    int y = (int)(int.Parse(axis[1]) * 1.983);
                                    axis[0] = x.ToString();
                                    axis[1] = y.ToString();
                                    int stringindex = barcode_info.IndexOf("^FD");
                                    string verificationstring = barcode_info.Substring(stringindex + 3, barcode_info.Length - 3 - stringindex);

                                    string orientationstring = barcode_info.Substring(barcodeindexer + 3, 1);
                                    //check orientation
                                    string[] array = { axis[0], axis[1], orientationstring, verificationstring, "PDF417" };
                                    barcodelist.Add(array);
                                }


                                barcodeindexer = barcode_info.IndexOf("^BQ");
                                if (barcodeindexer > 0)
                                {
                                    //extract x y and label information
                                    string coordinates = barcode_info.Substring(3, barcodeindexer - 3);
                                    //split the coordinates
                                    string[] axis = coordinates.Split(',');
                                    for (int i = 0; i < axis.Length; i++)
                                    {
                                        int xindex = axis[i].IndexOf(";");
                                        if (xindex > 0)
                                        {
                                            axis[i] = axis[i].Substring(0, xindex);
                                        }
                                        xindex = axis[i].IndexOf("^");
                                        if (xindex > 0)
                                        {
                                            axis[i] = axis[i].Substring(0, xindex);
                                        }
                                    }
                                    int x = (int)(int.Parse(axis[0]) * 1.983);//assume ratio is 2x
                                    int y = (int)(int.Parse(axis[1]) * 1.983);
                                    axis[0] = x.ToString();
                                    axis[1] = y.ToString();
                                    int stringindex = barcode_info.IndexOf("^FD");
                                    string verificationstring = barcode_info.Substring(stringindex + 3, barcode_info.Length - 3 - stringindex);
                                    string orientationstring = barcode_info.Substring(barcodeindexer + 3, 1);
                                    //check orientation
                                    string[] array = { axis[0], axis[1], orientationstring, verificationstring, "QRCODE" };
                                    barcodelist.Add(array);
                                }

                                #endregion
                                //end of scan for barcodes

                                //OCR Search Region
                                #region OCR String SearchStart
                                string OCR_info = barcode_info;
                                int FontIndicator = OCR_info.IndexOf("^A");
                                char[] temp = OCR_info.ToCharArray();
                                if (FontIndicator > 0)//its a font, extract font information
                                {
                                    string fonttype = OCR_info.Substring(FontIndicator + 2, 1);
                                    string fontorientation = OCR_info.Substring(FontIndicator + 3, 1);
                                    string coordinates = OCR_info.Substring(3, FontIndicator - 3);
                                    //split the coordinates
                                    string[] axis = coordinates.Split(',');
                                    for (int i = 0; i < axis.Length; i++)
                                    {
                                        int xindex = axis[i].IndexOf(";");
                                        if (xindex > 0)
                                        {
                                            axis[i] = axis[i].Substring(0, xindex);
                                        }
                                        xindex = axis[i].IndexOf("^");
                                        if (xindex > 0)
                                        {
                                            axis[i] = axis[i].Substring(0, xindex);
                                        }
                                    }

                                    int x = (int)(int.Parse(axis[0]) * 2.017);
                                    int y = (int)(int.Parse(axis[1]) * 1.986);
                                    axis[0] = x.ToString();
                                    axis[1] = y.ToString();
                                    int scaleindex = OCR_info.IndexOf("^", FontIndicator + 1);
                                    string scale = OCR_info.Substring(FontIndicator + 5, scaleindex - 5 - FontIndicator);
                                    string[] xyscale = scale.Split(',');
                                    int stringindex = barcode_info.IndexOf("^FD");

                                    string verificationstring = OCR_info.Substring(stringindex + 3, OCR_info.Length - 3 - stringindex);

                                    int UpperCaseString = StringLowerCaseDetect(verificationstring);
                                    string mm = "0";
                                    if (fontorientation == "N")
                                    {
                                        mm = "0";
                                    }
                                    else
                                    {
                                        mm = "1";
                                    }
                                    string[] array = { axis[0], axis[1], xyscale[0], xyscale[1], verificationstring, fonttype, fontorientation, UpperCaseString.ToString() };
                                    //string ocrfont = xyscale[0] + "_"+xyscale[1] + "_"+fonttype;//incase font type is required can use this naming convention
                                    ocrlist.Add(array);

                                }
                                #endregion
                                index = zplStrings.IndexOf("^FS", index + 3);
                            }
                            else
                            {
                                index = zplStrings.IndexOf("^FS", index + 4);
                            }




                        }
                        BarcoderelatedOCRSearch();
                        GBidentifier();
                        break;
                }
            }
            catch (Exception ex)
            {
                //   MessageBox.Show(ex.ToString());
            }



        }
        /// <summary>
        /// This Function will replace > + 1 char with "" (empty string)
        /// e.g: >E become "" (This is to cater Code128 invocation table convertion)
        /// Except for "><", ">0", ">=", which need special conversion
        /// Will Auto remove '>' if found of the far right end.
        /// </summary>
        /// <param name="InputText"></param>
        /// <returns>Converted string</returns>
        public string CODE128_INVOCATION_Conversion(string InputText)
        {
            #region Code128_SubB
            Dictionary<string, string> CODE128_ESCAPE_TABLE = new Dictionary<string, string>()
            {
              {"><","?"},
              {">0",">"},
              {">=","~"}
            };
            #endregion
            InputText = InputText.TrimEnd('>');
            string Pattern = ">[^\x3C\x30\x3D]";
            StringBuilder convertedData = new StringBuilder();
            convertedData.Append(System.Text.RegularExpressions.Regex.Replace(InputText, Pattern, String.Empty, System.Text.RegularExpressions.RegexOptions.Compiled));
            foreach (KeyValuePair<String, String> r in CODE128_ESCAPE_TABLE)
            {
                convertedData = convertedData.Replace(r.Key, r.Value);
            }
            return convertedData.ToString();
        }
        public static string SReplace(string s, string[] separators, string newVal)
        {
            string[] temp;
            temp = s.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            return string.Join(newVal, temp);
        }
        public void InspectionPatternMatch(string imagepath)
        {
            linepackomronsystem.Scene_Switch(0);
            linepackomronsystem.CalibrateImage(imagepath);
            //check data
            string tmp = linepackomronsystem.UnitData_Get(67, 1000);
            tmp.Trim();
            double restdouble = double.Parse(tmp);
            int result = (int)restdouble;
            if (result < 0)
            {
                throw new Exception("Bitmap Calibration Failed.");
            }

            linepackomronsystem.TeachImage(imagepath);
            tmp = linepackomronsystem.UnitData_Get(54, 1000);
            tmp.Trim();
            restdouble = double.Parse(tmp);
            result = (int)restdouble;
            if (result < 0)
            {
                throw new Exception("bitmap teach Failed.");

            }
            //linepackomronsystem.Measure_Once();            
        }
        public void MultiData_BCSend()
        {
            StringBuilder sb = new StringBuilder();

            try
            {
                //setting unit data for barcode
                int bc_commentindex = 144;
                int bc_coordinatesindex = 136;
                int bc_x_normal_unit_index = 10;
                int bc_y_normal_unit_index = 13;
                int bc_verificationindex = 45;//45,50,55

                int bc_x_vertical_unit_index = 12;
                int bc_y_vertical_unit_index = 15;
                int bc_vertical_x_index = 136;
                int bc_vertical_cmt_index = 144;

                //sending of barcodes
                #region barcodes
                List<string[]> bc = barcodelist;
                for (int i = 0; i < bc.Count; i++)
                {
                    //make sure it is a normal orientation
                    sb.Clear();
                    if (bc[i][2] == "N")
                    {
                        switch (bc_x_normal_unit_index)//for normal orientation it can only be either 10 or 11
                        {
                            case 10:
                                bc_verificationindex = 45;
                                break;
                            case 11:
                                bc_verificationindex = 50;
                                break;
                        }
                        // Send barcode 
                        sb.Append(bc_x_normal_unit_index.ToString() + " " + bc_coordinatesindex.ToString() + " " + bc[i][4] + " "); //code type
                        sb.Append(bc_y_normal_unit_index.ToString() + " " + bc_commentindex.ToString() + " " + bc[i][1] + " "); //Y coordination
                        sb.Append(bc_x_normal_unit_index.ToString() + " " + bc_commentindex.ToString() + " " + bc[i][0] + " "); //X coordination
                        sb.Append(bc_verificationindex.ToString() + " " + bc_coordinatesindex.ToString() + " " + bc[i][3] + " "); //Verification String
                        linepackomronsystem.MultiData_Change(sb.ToString());
                        //****************

                        if (bc_coordinatesindex < 143)//make use of bc_coordinatesindex
                        {
                            bc_coordinatesindex++;
                            bc_commentindex++;
                        }
                        else
                        {
                            //reset index
                            bc_commentindex = 144;
                            bc_coordinatesindex = 136;
                            bc_y_normal_unit_index++;
                            bc_x_normal_unit_index++;
                        }
                    }
                    else
                    {
                        if (bc_vertical_x_index <= 143)//make use of bc_coordinatesindex
                        {

                            //*********************************
                            //execute datasend
                            sb.Append(bc_x_vertical_unit_index.ToString() + " " + bc_vertical_cmt_index.ToString() + " " + bc[i][0] + " "); //x coordinates
                            sb.Append(bc_y_vertical_unit_index.ToString() + " " + bc_vertical_cmt_index.ToString() + " " + bc[i][1] + " "); //y coordinates
                            sb.Append(bc_x_vertical_unit_index.ToString() + " " + bc_vertical_x_index.ToString() + " " + bc[i][4] + " "); //code type
                            sb.Append("55 " + bc_vertical_x_index.ToString() + " " + bc[i][3] + " "); //Verification String
                            //*********************************
                            //index next number
                            linepackomronsystem.MultiData_Change(sb.ToString());
                            bc_vertical_x_index++;
                            bc_vertical_cmt_index++;
                        }
                    }
                }

                #endregion
            }
            catch (Exception ex)
            {
                //  MessageBox.Show(ex.ToString());
            }
        }
        public void SendBCData2()
        {

            try
            {
                //setting unit data for barcode
                int bc_commentindex = 144;
                int bc_coordinatesindex = 136;
                int bc_x_normal_unit_index = 10;
                int bc_y_normal_unit_index = 13;
                int bc_verificationindex = 45;//45,50,55

                int bc_x_vertical_unit_index = 12;
                int bc_y_vertical_unit_index = 15;
                //int bc_verticalcomment_index = 54;
                int bc_vertical_x_index = 136;
                int bc_vertical_cmt_index = 144, BcodeDummy = 300;

                //sending of barcodes
                #region barcodes
                foreach (string[] bc in barcodelist)
                {
                    //make sure it is a normal orientation
                    if (bc[2] == "N")
                    {
                        switch (bc_x_normal_unit_index)//for normal orientation it can only be either 10 or 11
                        {
                            case 10:
                                bc_verificationindex = 72;
                                break;
                            case 11:
                                bc_verificationindex = 72;
                                break;
                        }
                        //****************

                        ////send bc data here
                        //linepackomronsystem.UnitData_Change(bc_x_normal_unit_index, bc_coordinatesindex, bc[0]);//x coordinates
                        //linepackomronsystem.UnitData_Change(bc_y_normal_unit_index, bc_coordinatesindex, bc[1]);//y coordinates
                        //linepackomronsystem.UnitData_Change(bc_x_normal_unit_index, bc_commentindex, bc[4]);//code type
                        //linepackomronsystem.UnitData_Change(bc_verificationindex, bc_coordinatesindex, bc[3]);//verification string
                        ////****************

                        ////send bc data here
                        //linepackomronsystem.UnitData_Change(bc_x_normal_unit_index, bc_coordinatesindex, bc[4]);//code type
                        //linepackomronsystem.UnitData_Change(bc_y_normal_unit_index, bc_commentindex, bc[1]);//y coordinates
                        //linepackomronsystem.UnitData_Change(bc_x_normal_unit_index, bc_commentindex, bc[0]);//x coordinates
                        ////bc[3] = bc[3].Replace(' ', '?');
                        //bc[3] = bc[3].Replace(" ", "");
                        //linepackomronsystem.UnitData_Change(bc_verificationindex, bc_coordinatesindex, bc[3]);//verification string
                        ////****************
                        if (bc[3].IndexOf("_1E_04") > 0)
                        {
                            bc[3] = bc[3].Replace(@"_1E", "?");
                            bc[3] = bc[3].Replace(@"_1D", "?");
                            bc[3] = bc[3].Replace(@"_04", "?");
                            bc[3] = bc[3].Replace(@"_20", "?");
                            // bc[3] = SReplace(bc[3], new string[] { @"_1E", "_1D", "_04" }, "?"); //gy_added
                        }
                        //bc[3] = SReplace(bc[3], new string[] { @"\&"}, "??"); //gy_added
                        bc[3] = bc[3].Replace(@"\&", "??");
                        //  bc[3] = SReplace(bc[3], new string[] { @"/1" }, ""); //gy_added
                        //send bc data here
                        linepackomronsystem.UnitData_Change(bc_x_normal_unit_index, bc_coordinatesindex, '"' + bc[4] + '"');//code type
                        linepackomronsystem.UnitData_Change(bc_y_normal_unit_index, bc_commentindex, bc[1]);//y coordinates
                        linepackomronsystem.UnitData_Change(bc_x_normal_unit_index, bc_commentindex, bc[0]);//x coordinates
                        //bc[3] = bc[3].Replace(' ', '?');
                        //bc[3] = bc[3].Replace(" ", "");
                        linepackomronsystem.UnitData_Change(bc_verificationindex, BcodeDummy, '"' + bc[3] + '"');//verification string
                        //****************






                        if (bc_coordinatesindex < 143)//make use of bc_coordinatesindex
                        {
                            bc_coordinatesindex++;
                            bc_commentindex++;
                            BcodeDummy++;
                        }
                        else
                        {
                            //reset index
                            bc_commentindex = 144;
                            bc_coordinatesindex = 136;
                            bc_y_normal_unit_index++;
                            bc_x_normal_unit_index++;
                            BcodeDummy = 310;
                        }
                    }
                    else
                    {
                        if (bc_vertical_x_index <= 143)//make use of bc_coordinatesindex
                        {
                            //int bc_x_vertical_unit_index = 12;
                            //int bc_y_vertical_unit_index = 13;
                            //int bc_verticalcomment_index = 54;
                            //*********************************
                            //execute datasend
                            linepackomronsystem.UnitData_Change(bc_x_vertical_unit_index, bc_vertical_cmt_index, bc[0]);//x coordinates
                            linepackomronsystem.UnitData_Change(bc_y_vertical_unit_index, bc_vertical_cmt_index, bc[1]);//y coordinates
                            linepackomronsystem.UnitData_Change(bc_x_vertical_unit_index, bc_vertical_x_index, bc[4]);//code type
                            //verification string .. 54 is a special unit number for ocr verificaiton string
                            linepackomronsystem.UnitData_Change(56, bc_vertical_x_index, bc[3]);
                            //*********************************
                            //index next number
                            bc_vertical_x_index++;
                            bc_vertical_cmt_index++;
                        }
                    }
                }
                #endregion


                //   MessageBox.Show("Send Barcode and OCR Data Complete");
            }
            catch (Exception ex)
            {
                //  MessageBox.Show(ex.ToString());
            }
        }
        public void SendBCData()
        {

            try
            {
                //setting unit data for barcode
                int bc_commentindex = 144;
                int bc_coordinatesindex = 136;
                int bc_x_normal_unit_index = 10;
                int bc_y_normal_unit_index = 13;
                int bc_verificationindex = 45;//45,50,55

                int bc_x_vertical_unit_index = 12;
                int bc_y_vertical_unit_index = 15;
                //int bc_verticalcomment_index = 54;
                int bc_vertical_x_index = 136;
                int bc_vertical_cmt_index = 144;

                //sending of barcodes
                #region barcodes
                foreach (string[] bc in barcodelist)
                {
                    //make sure it is a normal orientation
                    if (bc[2] == "N")
                    {
                        switch (bc_x_normal_unit_index)//for normal orientation it can only be either 10 or 11
                        {
                            case 10:
                                bc_verificationindex = 45;
                                break;
                            case 11:
                                bc_verificationindex = 50;
                                break;
                        }
                        //****************

                        ////send bc data here
                        //linepackomronsystem.UnitData_Change(bc_x_normal_unit_index, bc_coordinatesindex, bc[0]);//x coordinates
                        //linepackomronsystem.UnitData_Change(bc_y_normal_unit_index, bc_coordinatesindex, bc[1]);//y coordinates
                        //linepackomronsystem.UnitData_Change(bc_x_normal_unit_index, bc_commentindex, bc[4]);//code type
                        //linepackomronsystem.UnitData_Change(bc_verificationindex, bc_coordinatesindex, bc[3]);//verification string
                        ////****************

                        ////send bc data here
                        //linepackomronsystem.UnitData_Change(bc_x_normal_unit_index, bc_coordinatesindex, bc[4]);//code type
                        //linepackomronsystem.UnitData_Change(bc_y_normal_unit_index, bc_commentindex, bc[1]);//y coordinates
                        //linepackomronsystem.UnitData_Change(bc_x_normal_unit_index, bc_commentindex, bc[0]);//x coordinates
                        ////bc[3] = bc[3].Replace(' ', '?');
                        //bc[3] = bc[3].Replace(" ", "");
                        //linepackomronsystem.UnitData_Change(bc_verificationindex, bc_coordinatesindex, bc[3]);//verification string
                        ////****************


                        //send bc data here
                        linepackomronsystem.UnitData_Change(bc_x_normal_unit_index, bc_coordinatesindex, '"' + bc[4] + '"');//code type
                        linepackomronsystem.UnitData_Change(bc_y_normal_unit_index, bc_commentindex, bc[1]);//y coordinates
                        linepackomronsystem.UnitData_Change(bc_x_normal_unit_index, bc_commentindex, bc[0]);//x coordinates
                        //bc[3] = bc[3].Replace(' ', '?');
                        //bc[3] = bc[3].Replace(" ", "");
                        linepackomronsystem.UnitData_Change(bc_verificationindex, bc_coordinatesindex, '"' + bc[3] + '"');//verification string
                        //****************






                        if (bc_coordinatesindex < 143)//make use of bc_coordinatesindex
                        {
                            bc_coordinatesindex++;
                            bc_commentindex++;
                        }
                        else
                        {
                            //reset index
                            bc_commentindex = 144;
                            bc_coordinatesindex = 136;
                            bc_y_normal_unit_index++;
                            bc_x_normal_unit_index++;
                        }
                    }
                    else
                    {
                        if (bc_vertical_x_index <= 143)//make use of bc_coordinatesindex
                        {
                            //int bc_x_vertical_unit_index = 12;
                            //int bc_y_vertical_unit_index = 13;
                            //int bc_verticalcomment_index = 54;
                            //*********************************
                            //execute datasend
                            linepackomronsystem.UnitData_Change(bc_x_vertical_unit_index, bc_vertical_cmt_index, bc[0]);//x coordinates
                            linepackomronsystem.UnitData_Change(bc_y_vertical_unit_index, bc_vertical_cmt_index, bc[1]);//y coordinates
                            linepackomronsystem.UnitData_Change(bc_x_vertical_unit_index, bc_vertical_x_index, bc[4]);//code type
                            //verification string .. 54 is a special unit number for ocr verificaiton string
                            linepackomronsystem.UnitData_Change(55, bc_vertical_x_index, bc[3]);
                            //*********************************
                            //index next number
                            bc_vertical_x_index++;
                            bc_vertical_cmt_index++;
                        }
                    }
                }
                #endregion


                //   MessageBox.Show("Send Barcode and OCR Data Complete");
            }
            catch (Exception ex)
            {
                //  MessageBox.Show(ex.ToString());
            }
        }
        public int IntelLabelCheck(string zplfilepath)
        {
            int box1pos = zplfilepath.IndexOf("_BOX1");
            int box2pos = zplStrings.IndexOf("^FX   INTEL BOX");
            int box3pos = zplfilepath.IndexOf("SPECTEK");
            //int intelpos = -1;
            int count = 0, n = 0; //Count occurance of ^GB in the text
            string substring = "^GB";
            if (substring != "")
            {
                while ((n = zplStrings.IndexOf(substring, n, StringComparison.InvariantCulture)) != -1)
                {
                    n += substring.Length;
                    ++count;
                }
            }
            if (box1pos >= 0 || count == 12 || box2pos >= 0) // if is box label, then check if is INTEL label
            {
                //intelpos = zplStrings.IndexOf("^FX   INTEL BOX");
                if (box2pos >= 0)
                {
                    return 2;
                }
                else
                {
                    return 1;
                }

            }
            else if (box3pos >= 0)
            {
                return 3;
            }
            return -1;
        }

        private int StringLowerCaseDetect(string input)
        {
            for (int i = 0; i < input.Length; i++)
            {
                if (char.IsLetter(input[i]) && !char.IsUpper(input[i]))
                {
                    return 1;
                }
            }
            return 0;
        }

        public void GBidentifier()
        {
            int index = zplStrings.IndexOf("^FO");
            int indexofposition = 0;
            while (index > 0)// Find all the ^FO
            {
                indexofposition = zplStrings.IndexOf("^FS", indexofposition);
                while (indexofposition < index)
                {
                    indexofposition++;
                    indexofposition = zplStrings.IndexOf("^FS", indexofposition);
                }
                string gb_info = zplStrings.Substring(index, indexofposition - index);
                indexofposition++;
                int gbIndicator = gb_info.IndexOf("^GB");
                string gbrelateOCR = "-1";
                if (gbIndicator > 0)//its a font, extract font information
                {
                    string gbcoor = gb_info.Substring(3, gbIndicator - 6);
                    //split the coordinates
                    string[] gbaxis = gbcoor.Split(',');
                    int x = (int)(int.Parse(gbaxis[0]) * 2.017);
                    int y = (int)(int.Parse(gbaxis[1]) * 1.986);
                    gbaxis[0] = x.ToString();
                    gbaxis[1] = y.ToString();
                    string gbborder = gb_info.Substring(gbIndicator + 3);
                    string[] xyscale = gbborder.Split(',');
                    for (int i = 0; i < ocrbarcodelist.Count; i++)
                    {
                        int Xgap = x - int.Parse(ocrbarcodelist[i][0]);
                        int Ygap = int.Parse(ocrbarcodelist[i][1]) - y;
                        int gbgap = y + int.Parse(xyscale[2]);
                        if (Xgap > -5 && Xgap < 20 && Ygap > 0 && Ygap < 25 && gbgap > int.Parse(ocrbarcodelist[i][1]))
                        {
                            gbrelateOCR = i.ToString();
                        }
                        else
                        {
                            gbrelateOCR = "-1";
                        }
                        string[] array = { gbaxis[0], gbaxis[1], "0", "0", xyscale[0], xyscale[2], gbrelateOCR };
                        if (int.Parse(xyscale[2]) > 10 && gbrelateOCR != "-1")
                        {
                            GBlist.Add(array);
                        }
                    }
                }
                index = zplStrings.IndexOf("^FO", index + 3);
            }
        }

        public void BarcoderelatedOCRSearch()  //By GYLee,12082015
        {
            #region BarCode related OCR
            bool agotb, bgota;

            List<string> extrastr = new List<string>();
            for (int i = 0; i < barcodelist.Count; i++) //Go through all barcode array
            {
                for (int j = 0; j < ocrlist.Count; j++) //Go throught all ocr array
                {
                    agotb = barcodelist[i][3].Contains(ocrlist[j][4]);
                    bgota = ocrlist[j][4].Contains(barcodelist[i][3].Substring(2));

                    //if ((agotb || bgota) && ( barcodelist[i][3].Length - ocrlist[j][4].Length < 3) && (barcodelist[i][3].Length - ocrlist[j][4].Length >= 0))
                    if ((agotb && ocrlist[j][4].Length > 3) || (bgota && barcodelist[i][3].Length > 3))
                    {
                        //if (isxtra )   //Acticate to detect (XX)         
                        //{
                        //    extrastr.Add(searchstr.Substring(0, strlendiff));
                        //}
                        ocrbarcodelist.Add(ocrlist[j]);
                        ocrlist.RemoveAt(j); //Clear the ref to avoid duplication.
                        break;
                    }
                }

            }
            //if (extrastr.Count != 0)    //Acticate to detect (XX)         
            //{
            //    for (int i = 0; i < extrastr.Count; i++) //Go through all barcode array
            //    {
            //        for (int j = 0; j < ocrlist.Count; j++) //Go throught all ocr array
            //        {

            //            string searchstr2 = extrastr[i];

            //            int index = ocrlist[j][6].IndexOf(searchstr2);
            //            if (index >= 0)
            //            {
            //                ocrbarcodelist.Add(ocrlist[j]);
            //                break;
            //            }
            //        }

            //    }
            //}
            #endregion
        }
        public void MultiData_OCRLoc()
        {
            StringBuilder sb = new StringBuilder();
            int ocr_unit = 11;//11 12 13 14 15
            if (GBlist.Count > 0) //Process GBlist first
            {

                string[] tempGMocr = ocrbarcodelist[int.Parse(GBlist[0][6])];
                int temptxt = int.Parse(tempGMocr[0]) + 80; //OFFSET BYPASS BY
                tempGMocr[4] = tempGMocr[4].ToUpper();
                tempGMocr[4] = tempGMocr[4].Replace("+", "/");
                tempGMocr[4] = tempGMocr[4].Replace("*", "");
                tempGMocr[4] = tempGMocr[4].Replace("-", "");
                tempGMocr[4] = tempGMocr[4].Replace(" ", string.Empty);
                //sb.Append("10 148 " + GBlist[0][0] + " ");
                //sb.Append("10 149 " + GBlist[0][1] + " ");
                //sb.Append("10 150 " + GBlist[0][2] + " ");
                //sb.Append("10 151 " + GBlist[0][3] + " ");
                sb.Append("10 136 " + tempGMocr[4] + " "); //Verification String
                sb.Append("10 144 " + temptxt.ToString() + " "); // X Coordinate
                sb.Append("10 145 " + tempGMocr[1] + " "); //Y coordinate
                sb.Append("10 146 " + tempGMocr[7] + " "); //OCR upper/lower case
                sb.Append("10 147 0 "); //OCR Orientation
                linepackomronsystem.MultiData_Change(sb.ToString());
                sb.Clear();
                ocrbarcodelist.RemoveAt(int.Parse(GBlist[0][6])); //Remove the GB related barcode
            }
            bool verocr = false;
            foreach (string[] ocr in ocrbarcodelist)
            {
                ocr[4] = ocr[4].ToUpper();
                ocr[4] = ocr[4].Replace("+", "/");
                ocr[4] = ocr[4].Replace("*", "");
                // ocr[4] = ocr[4].Replace(":", "");
                ocr[4] = ocr[4].Replace("-", "");
                ocr[4] = ocr[4].Replace(" ", string.Empty);
                if (ocr[6] == "1" && verocr == false)
                {
                    sb.Append("23 136 " + ocr[4] + " "); //verification strings
                    sb.Append("23 144 " + ocr[0] + " "); //x coordinates
                    sb.Append("23 145 " + ocr[1] + " ");//y coordinates
                    sb.Append("23 146 " + ocr[7] + " ");//OCR upper/lower
                    sb.Append("23 147 1 ");//OCR orientation
                    verocr = true;
                }
                else
                {
                    sb.Append(ocr_unit.ToString() + " 136 " + ocr[4] + " "); //verification strings
                    sb.Append(ocr_unit.ToString() + " 144 " + ocr[0] + " "); //x coordinates
                    sb.Append(ocr_unit.ToString() + " 145 " + ocr[1] + " ");//y coordinates
                    sb.Append(ocr_unit.ToString() + " 146 " + ocr[7] + " ");//OCR upper/lower
                    sb.Append(ocr_unit.ToString() + " 147 0 ");//OCR orientation
                }
                linepackomronsystem.MultiData_Change(sb.ToString());
                sb.Clear();
                ocr_unit++;
            }
        }
        public void LoadOCRlocation7()  // Obsolete, Use MultiData_Change instead
        {
            //sending of OCRStrings
            #region ocrcodes
            int ocr_xy_index = 144;//everytime index by 2
            int ocr_string_index = 136;
            int ocr_unit = 11;//11 12 13 14 15
            if (GBlist.Count > 0)
            {
                string[] tempGMocr = ocrbarcodelist[int.Parse(GBlist[0][6])];
                tempGMocr[4] = tempGMocr[4].ToUpper();
                tempGMocr[4] = tempGMocr[4].Replace("+", "/");
                tempGMocr[4] = tempGMocr[4].Replace("*", "");
                // ocr[4] = ocr[4].Replace(":", "");
                tempGMocr[4] = tempGMocr[4].Replace("-", "");
                tempGMocr[4] = tempGMocr[4].Replace(" ", string.Empty);
                //linepackomronsystem.UnitData_Change(10, 148, GBlist[0][0]);//OCR GB x
                //linepackomronsystem.UnitData_Change(10, 149, GBlist[0][1]);//OCR GB Y
                //linepackomronsystem.UnitData_Change(10, 150, GBlist[0][2]);//OCR GB X1
                //linepackomronsystem.UnitData_Change(10, 151, GBlist[0][3]);//OCR GB Y1


                int temptxt = int.Parse(tempGMocr[0]) + 70;
                linepackomronsystem.UnitData_Change(10, 136, tempGMocr[4].Substring(2));//verification strings
                linepackomronsystem.UnitData_Change(10, 144, temptxt.ToString());//x coordinates
                //ocr_xy_index++;
                //ocr_string_index++;//dummy index
                linepackomronsystem.UnitData_Change(10, 145, tempGMocr[1]);//y coordinates
                linepackomronsystem.UnitData_Change(10, 146, tempGMocr[7]);//OCR upper/lower
                linepackomronsystem.UnitData_Change(10, 147, "0");//OCR orientation - TEMPORARY BYPASS
                ocrbarcodelist.RemoveAt(int.Parse(GBlist[0][6])); //Remove the GB related barcode
            }
            bool verocr = false;
            foreach (string[] ocr in ocrbarcodelist)
            {
                ocr[4] = ocr[4].ToUpper();
                ocr[4] = ocr[4].Replace("+", "/");
                ocr[4] = ocr[4].Replace("*", "");
                ocr[4] = ocr[4].Replace("-", "");
                ocr[4] = ocr[4].Replace(" ", string.Empty);
                if (ocr[6] != "N" && verocr == false)
                {
                    int test = int.Parse(ocr[0]) + 120;
                    ocr[0] = test.ToString();
                    linepackomronsystem.UnitData_Change(23, ocr_string_index, ocr[4]);//verification strings
                    linepackomronsystem.UnitData_Change(23, ocr_xy_index, ocr[0]);//x coordinates
                    //ocr_xy_index++;
                    //ocr_string_index++;//dummy index
                    linepackomronsystem.UnitData_Change(23, ocr_xy_index + 1, ocr[1]);//y coordinates
                    linepackomronsystem.UnitData_Change(23, ocr_xy_index + 2, ocr[7]);//OCR upper/lower
                    linepackomronsystem.UnitData_Change(23, ocr_xy_index + 3, "1");//OCR orientation - TEMPORARY BYPASS
                    verocr = true;
                }
                else
                {

                    linepackomronsystem.UnitData_Change(ocr_unit, ocr_string_index, ocr[4]);//verification strings
                    linepackomronsystem.UnitData_Change(ocr_unit, ocr_xy_index, ocr[0]);//x coordinates
                    //ocr_xy_index++;
                    //ocr_string_index++;//dummy index
                    linepackomronsystem.UnitData_Change(ocr_unit, ocr_xy_index + 1, ocr[1]);//y coordinates
                    linepackomronsystem.UnitData_Change(ocr_unit, ocr_xy_index + 2, ocr[7]);//OCR upper/lower
                    linepackomronsystem.UnitData_Change(ocr_unit, ocr_xy_index + 3, "0");//OCR orientation - TEMPORARY BYPASS
                }
                //place in code for writng to omron
                ocr_unit++;
                //ocr_xy_index = 144;
                //ocr_string_index = 136;
            }
            #endregion
        }
        public void LoadOCRlocation()  // Obsolete, Use MultiData_Change instead
        {
            //sending of OCRStrings
            #region ocrcodes
            int ocr_xy_index = 144;//everytime index by 2
            int ocr_string_index = 136;
            int ocr_unit = 11;//11 12 13 14 15
            if (GBlist.Count > 0)
            {
                string[] tempGMocr = ocrbarcodelist[int.Parse(GBlist[0][6])];
                tempGMocr[4] = tempGMocr[4].ToUpper();
                tempGMocr[4] = tempGMocr[4].Replace("+", "/");
                tempGMocr[4] = tempGMocr[4].Replace("*", "");
                // ocr[4] = ocr[4].Replace(":", "");
                tempGMocr[4] = tempGMocr[4].Replace("-", "");
                tempGMocr[4] = tempGMocr[4].Replace(" ", string.Empty);
                //linepackomronsystem.UnitData_Change(10, 148, GBlist[0][0]);//OCR GB x
                //linepackomronsystem.UnitData_Change(10, 149, GBlist[0][1]);//OCR GB Y
                //linepackomronsystem.UnitData_Change(10, 150, GBlist[0][2]);//OCR GB X1
                //linepackomronsystem.UnitData_Change(10, 151, GBlist[0][3]);//OCR GB Y1


                int temptxt = int.Parse(tempGMocr[0]) + 80;
                linepackomronsystem.UnitData_Change(10, 136, tempGMocr[4].Substring(2));//verification strings
                linepackomronsystem.UnitData_Change(10, 144, temptxt.ToString());//x coordinates
                                                                                 //ocr_xy_index++;
                                                                                 //ocr_string_index++;//dummy index
                linepackomronsystem.UnitData_Change(10, 145, tempGMocr[1]);//y coordinates
                linepackomronsystem.UnitData_Change(10, 146, tempGMocr[7]);//OCR upper/lower
                linepackomronsystem.UnitData_Change(10, 147, "0");//OCR orientation - TEMPORARY BYPASS
                ocrbarcodelist.RemoveAt(int.Parse(GBlist[0][6])); //Remove the GB related barcode
            }
            bool verocr = false;
            foreach (string[] ocr in ocrbarcodelist)
            {
                ocr[4] = ocr[4].ToUpper();
                ocr[4] = ocr[4].Replace("+", "/");
                ocr[4] = ocr[4].Replace("*", "");
                ocr[4] = ocr[4].Replace("-", "");
                ocr[4] = ocr[4].Replace(" ", string.Empty);
                int tempXoffset = int.Parse(ocr[0]) - 10;
                if (ocr[6] != "N" && verocr == false)
                {

                    linepackomronsystem.UnitData_Change(23, ocr_string_index, ocr[4]);//verification strings
                    linepackomronsystem.UnitData_Change(23, ocr_xy_index, ocr[0]);//x coordinates
                                                                                  //ocr_xy_index++;
                                                                                  //ocr_string_index++;//dummy index
                    linepackomronsystem.UnitData_Change(23, ocr_xy_index + 1, ocr[1]);//y coordinates
                    linepackomronsystem.UnitData_Change(23, ocr_xy_index + 2, ocr[7]);//OCR upper/lower
                    linepackomronsystem.UnitData_Change(23, ocr_xy_index + 3, "1");//OCR orientation - TEMPORARY BYPASS
                    verocr = true;
                }
                else
                {

                    linepackomronsystem.UnitData_Change(ocr_unit, ocr_string_index, ocr[4]);//verification strings
                    linepackomronsystem.UnitData_Change(ocr_unit, ocr_xy_index, tempXoffset.ToString());//x coordinates
                                                                                                        //ocr_xy_index++;
                                                                                                        //ocr_string_index++;//dummy index
                    linepackomronsystem.UnitData_Change(ocr_unit, ocr_xy_index + 1, ocr[1]);//y coordinates
                    linepackomronsystem.UnitData_Change(ocr_unit, ocr_xy_index + 2, ocr[7]);//OCR upper/lower
                    linepackomronsystem.UnitData_Change(ocr_unit, ocr_xy_index + 3, "0");//OCR orientation - TEMPORARY BYPASS
                }
                //place in code for writng to omron
                ocr_unit++;
                //ocr_xy_index = 144;
                //ocr_string_index = 136;
            }
            #endregion
        }
        public void spektek(bool SpektekOn, bool IntelOn, bool TrayOn)
        {
            if (SpektekOn && !TrayOn)
            {
                linepackomronsystem.UnitData_Change(30, 124, "185"); //360 degree inspection
                linepackomronsystem.UnitData_Change(30, 125, "175");
            }
            else
            {
                linepackomronsystem.UnitData_Change(30, 124, "3");//Normal degree inspection
                linepackomronsystem.UnitData_Change(30, 125, "-3");
            }

            if (SpektekOn)//border for spektek
            {
                linepackomronsystem.UnitData_Change(53, 121, "0");
                linepackomronsystem.UnitData_Change(53, 120, "999999");
                linepackomronsystem.UnitData_Change(53, 123, "0");
                linepackomronsystem.UnitData_Change(53, 122, "999999");
                linepackomronsystem.UnitData_Change(53, 125, "0");
                linepackomronsystem.UnitData_Change(53, 124, "999999");
                linepackomronsystem.UnitData_Change(53, 127, "0");
                linepackomronsystem.UnitData_Change(53, 126, "999999");
            }
            else if (IntelOn)//border for intel
            {
                linepackomronsystem.UnitData_Change(53, 121, "0");
                linepackomronsystem.UnitData_Change(53, 120, "999999");
                linepackomronsystem.UnitData_Change(53, 123, "0.1");
                linepackomronsystem.UnitData_Change(53, 122, "999999");
                linepackomronsystem.UnitData_Change(53, 125, "0.1");
                linepackomronsystem.UnitData_Change(53, 124, "999999");
                linepackomronsystem.UnitData_Change(53, 127, "0.1");
                linepackomronsystem.UnitData_Change(53, 126, "999999");
            }
            else //border for other
            {
                linepackomronsystem.UnitData_Change(53, 121, "0.14");
                linepackomronsystem.UnitData_Change(53, 120, "999999");
                linepackomronsystem.UnitData_Change(53, 123, "0.14");
                linepackomronsystem.UnitData_Change(53, 122, "999999");
                linepackomronsystem.UnitData_Change(53, 125, "0.14");
                linepackomronsystem.UnitData_Change(53, 124, "999999");
                linepackomronsystem.UnitData_Change(53, 127, "0.14");
                linepackomronsystem.UnitData_Change(53, 126, "999999");
            }
        }
        public void spektek2(bool SpektekOn, bool IntelOn)
        {
            string lblXmin = Settings.Default.lblXmin2.ToString();
            string lblXmax = Settings.Default.lblXmax2.ToString();
            string lblYmin = Settings.Default.lblYmin2.ToString();
            string lblYmax = Settings.Default.lblYmax2.ToString();
            string lbldegmin = Settings.Default.lblDegmin2.ToString();
            string llbldegmax = Settings.Default.lblDegmax2.ToString();
            string pleftmin = Settings.Default.printLefmin2.ToString();
            string plefmax = Settings.Default.printLefmax2.ToString();
            string pritmin = Settings.Default.printRitemin2.ToString();
            string pritemax = Settings.Default.printRitemax2.ToString();
            string ptopmin = Settings.Default.printTopmin2.ToString();
            string ptopmax = Settings.Default.printTopmax2.ToString();
            string pbtmmin = Settings.Default.printBtmmin2.ToString();
            string pbtmmax = Settings.Default.printBtmmax2.ToString();

            string Spektopmin = Settings.Default.SpekTopmin2.ToString();
            string Spektopmax = Settings.Default.SpekTopmax2.ToString();
            string Spekbtmmin = Settings.Default.SpekBtmmin2.ToString();
            string Spekbtmmax = Settings.Default.SpekBtmmax2.ToString();
            string Speklefmin = Settings.Default.SpekLefmin2.ToString();
            string Speklefmax = Settings.Default.SpekLefmax2.ToString();
            string Spekritemin = Settings.Default.SpekRitemin2.ToString();
            string Spekritemax = Settings.Default.SpekRitemax2.ToString();
            string Inttopmin = Settings.Default.IntTopmin2.ToString();
            string Inttopmax = Settings.Default.IntTopmax2.ToString();
            string Intbtmmin = Settings.Default.IntBtmmin2.ToString();
            string Intbtmmax = Settings.Default.IntBtmmax2.ToString();
            string Intlefmin = Settings.Default.IntLefmin2.ToString();
            string Intlefmax = Settings.Default.IntLefmax2.ToString();
            string Intritemin = Settings.Default.IntRitemin2.ToString();
            string Intritemax = Settings.Default.IntRitemax2.ToString();
            linepackomronsystem.UnitData_Change(30, 120, lblXmax);
            linepackomronsystem.UnitData_Change(30, 121, lblXmin);
            linepackomronsystem.UnitData_Change(30, 122, lblYmax);
            linepackomronsystem.UnitData_Change(30, 123, lblYmin);
            linepackomronsystem.UnitData_Change(30, 124, llbldegmax);//Normal degree inspection
            linepackomronsystem.UnitData_Change(30, 125, lbldegmin);


            if (SpektekOn)//border for spektek
            {
                linepackomronsystem.UnitData_Change(87, 121, Speklefmin);
                linepackomronsystem.UnitData_Change(87, 120, Speklefmax);
                linepackomronsystem.UnitData_Change(87, 123, Spekritemin);
                linepackomronsystem.UnitData_Change(87, 122, Spekritemax);
                linepackomronsystem.UnitData_Change(87, 125, Spektopmin);
                linepackomronsystem.UnitData_Change(87, 124, Spektopmax);
                linepackomronsystem.UnitData_Change(87, 127, Spekbtmmin);
                linepackomronsystem.UnitData_Change(87, 126, Spekbtmmax);
            }
            else if (IntelOn)//border for intel
            {
                linepackomronsystem.UnitData_Change(87, 121, Intlefmin);
                linepackomronsystem.UnitData_Change(87, 120, Intlefmax);
                linepackomronsystem.UnitData_Change(87, 123, Intritemin);
                linepackomronsystem.UnitData_Change(87, 122, Intritemax);
                linepackomronsystem.UnitData_Change(87, 125, Inttopmin);
                linepackomronsystem.UnitData_Change(87, 124, Inttopmax);
                linepackomronsystem.UnitData_Change(87, 127, Intbtmmin);
                linepackomronsystem.UnitData_Change(87, 126, Intbtmmax);
            }
            else //border for other
            {
                linepackomronsystem.UnitData_Change(87, 121, pleftmin);              //Left
                linepackomronsystem.UnitData_Change(87, 120, plefmax);
                linepackomronsystem.UnitData_Change(87, 123, pritmin);            //Right
                linepackomronsystem.UnitData_Change(87, 122, pritemax);
                linepackomronsystem.UnitData_Change(87, 125, ptopmin);          //TOp
                linepackomronsystem.UnitData_Change(87, 124, ptopmax);
                linepackomronsystem.UnitData_Change(87, 127, pbtmmin);            //bottom
                linepackomronsystem.UnitData_Change(87, 126, pbtmmax);
            }
        }
        public void spektek4(bool SpektekOn, bool IntelOn, bool TrayOn)
        {
            string lblXmin = Settings.Default.lblXmin4.ToString();
            string lblXmax = Settings.Default.lblXmax4.ToString();
            string lblYmin = Settings.Default.lblYmin4.ToString();
            string lblYmax = Settings.Default.lblYmax4.ToString();
            string lbldegmin = Settings.Default.lblDegmin4.ToString();
            string llbldegmax = Settings.Default.lblDegmax4.ToString();
            string pleftmin = Settings.Default.printLefmin4.ToString();
            string plefmax = Settings.Default.printLefmax4.ToString();
            string pritmin = Settings.Default.printRitemin4.ToString();
            string pritemax = Settings.Default.printRitemax4.ToString();
            string ptopmin = Settings.Default.printTopmin4.ToString();
            string ptopmax = Settings.Default.printTopmax4.ToString();
            string pbtmmin = Settings.Default.printBtmmin4.ToString();
            string pbtmmax = Settings.Default.printBtmmax4.ToString();


            string Spektopmin = Settings.Default.SpekTopmin4.ToString();
            string Spektopmax = Settings.Default.SpekTopmax4.ToString();
            string Spekbtmmin = Settings.Default.SpekBtmmin4.ToString();
            string Spekbtmmax = Settings.Default.SpekBtmmax4.ToString();
            string Speklefmin = Settings.Default.SpekLefmin4.ToString();
            string Speklefmax = Settings.Default.SpekLefmax4.ToString();
            string Spekritemin = Settings.Default.SpekRitemin4.ToString();
            string Spekritemax = Settings.Default.SpekRitemax4.ToString();
            string Inttopmin = Settings.Default.IntTopmin4.ToString();
            string Inttopmax = Settings.Default.IntTopmax4.ToString();
            string Intbtmmin = Settings.Default.IntBtmmin4.ToString();
            string Intbtmmax = Settings.Default.IntBtmmax4.ToString();
            string Intlefmin = Settings.Default.IntLefmin4.ToString();
            string Intlefmax = Settings.Default.IntLefmax4.ToString();
            string Intritemin = Settings.Default.IntRitemin4.ToString();
            string Intritemax = Settings.Default.IntRitemax4.ToString();
            string spekdegmin = Settings.Default.SpekDegmin4.ToString();
            string spekdegmax = Settings.Default.SpekDegmax4.ToString();
            linepackomronsystem.UnitData_Change(30, 120, lblXmax);
            linepackomronsystem.UnitData_Change(30, 121, lblXmin);
            linepackomronsystem.UnitData_Change(30, 122, lblYmax);
            linepackomronsystem.UnitData_Change(30, 123, lblYmin);
            if (SpektekOn && !TrayOn)
            {
                // linepackomronsystem.UnitData_Change(30, 124, spekdegmax); //360 degree inspection
                //linepackomronsystem.UnitData_Change(30, 125, spekdegmin);
            }
            else
            {
                linepackomronsystem.UnitData_Change(30, 124, llbldegmax);//Normal degree inspection
                linepackomronsystem.UnitData_Change(30, 125, lbldegmin);
            }
            if (TrayOn)
            {
                linepackomronsystem.UnitData_Change(46, 144, "1");
            }
            else
            {
                linepackomronsystem.UnitData_Change(46, 144, "0");
            }
            if (SpektekOn)//border for spektek
            {

                linepackomronsystem.UnitData_Change(57, 121, Speklefmin);
                linepackomronsystem.UnitData_Change(57, 120, Speklefmax);
                linepackomronsystem.UnitData_Change(57, 123, Spekritemin);
                linepackomronsystem.UnitData_Change(57, 122, Spekritemax);
                linepackomronsystem.UnitData_Change(57, 125, Spektopmin);
                linepackomronsystem.UnitData_Change(57, 124, Spektopmax);
                linepackomronsystem.UnitData_Change(57, 127, Spekbtmmin);
                linepackomronsystem.UnitData_Change(57, 126, Spekbtmmax);
            }
            else if (IntelOn)//border for intel
            {
                linepackomronsystem.UnitData_Change(57, 121, Intlefmin);
                linepackomronsystem.UnitData_Change(57, 120, Intlefmax);
                linepackomronsystem.UnitData_Change(57, 123, Intritemin);
                linepackomronsystem.UnitData_Change(57, 122, Intritemax);
                linepackomronsystem.UnitData_Change(57, 125, Inttopmin);
                linepackomronsystem.UnitData_Change(57, 124, Inttopmax);
                linepackomronsystem.UnitData_Change(57, 127, Intbtmmin);
                linepackomronsystem.UnitData_Change(57, 126, Intbtmmax);
            }
            else //border for other
            {
                linepackomronsystem.UnitData_Change(57, 121, pleftmin);              //Left
                linepackomronsystem.UnitData_Change(57, 120, plefmax);
                linepackomronsystem.UnitData_Change(57, 123, pritmin);            //Right
                linepackomronsystem.UnitData_Change(57, 122, pritemax);
                linepackomronsystem.UnitData_Change(57, 125, ptopmin);          //TOp
                linepackomronsystem.UnitData_Change(57, 124, ptopmax);
                linepackomronsystem.UnitData_Change(57, 127, pbtmmin);            //bottom
                linepackomronsystem.UnitData_Change(57, 126, pbtmmax);
            }
        }
        public void spektek7(bool SpektekOn, bool IntelOn, bool TrayOn)
        {
            string lblXmin, lblXmax, lblYmin, lblYmax, lbldegmin, llbldegmax, pleftmin, plefmax, pritmin, pritemax, ptopmin, ptopmax, pbtmmin, pbtmmax;
            if (!TrayOn)
            {
                lblXmin = Settings.Default.lblXmin7.ToString();
                lblXmax = Settings.Default.lblXmax7.ToString();
                lblYmin = Settings.Default.lblYmin7.ToString();
                lblYmax = Settings.Default.lblYmax7.ToString();
                lbldegmin = Settings.Default.lblDegmin7.ToString();
                llbldegmax = Settings.Default.lblDegmax7.ToString();
                pleftmin = Settings.Default.printLefmin7.ToString();
                plefmax = Settings.Default.printLefmax7.ToString();
                pritmin = Settings.Default.printRitemin7.ToString();
                pritemax = Settings.Default.printRitemax7.ToString();
                ptopmin = Settings.Default.printTopmin7.ToString();
                ptopmax = Settings.Default.printTopmax7.ToString();
                pbtmmin = Settings.Default.printBtmmin7.ToString();
                pbtmmax = Settings.Default.printBtmmax7.ToString();
            }
            else
            {
                lblXmin = Settings.Default.lblXmin7a.ToString();
                lblXmax = Settings.Default.lblXmax7a.ToString();
                lblYmin = Settings.Default.lblYmin7a.ToString();
                lblYmax = Settings.Default.lblYmax7a.ToString();
                lbldegmin = Settings.Default.lblDegmin7a.ToString();
                llbldegmax = Settings.Default.lblDegmax7a.ToString();
                pleftmin = Settings.Default.printLefmin7a.ToString();
                plefmax = Settings.Default.printLefmax7a.ToString();
                pritmin = Settings.Default.printRitemin7a.ToString();
                pritemax = Settings.Default.printRitemax7a.ToString();
                ptopmin = Settings.Default.printTopmin7a.ToString();
                ptopmax = Settings.Default.printTopmax7a.ToString();
                pbtmmin = Settings.Default.printBtmmin7a.ToString();
                pbtmmax = Settings.Default.printBtmmax7a.ToString();
            }

            string Spektopmin = Settings.Default.SpekTopmin7.ToString();
            string Spektopmax = Settings.Default.SpekTopmax7.ToString();
            string Spekbtmmin = Settings.Default.SpekBtmmin7.ToString();
            string Spekbtmmax = Settings.Default.SpekBtmmax7.ToString();
            string Speklefmin = Settings.Default.SpekLefmin7.ToString();
            string Speklefmax = Settings.Default.SpekLefmax7.ToString();
            string Spekritemin = Settings.Default.SpekRitemin7.ToString();
            string Spekritemax = Settings.Default.SpekRitemax7.ToString();
            string Inttopmin = Settings.Default.IntTopmin7.ToString();
            string Inttopmax = Settings.Default.IntTopmax7.ToString();
            string Intbtmmin = Settings.Default.IntBtmmin7.ToString();
            string Intbtmmax = Settings.Default.IntBtmmax7.ToString();
            string Intlefmin = Settings.Default.IntLefmin7.ToString();
            string Intlefmax = Settings.Default.IntLefmax7.ToString();
            string Intritemin = Settings.Default.IntRitemin7.ToString();
            string Intritemax = Settings.Default.IntRitemax7.ToString();
            linepackomronsystem.UnitData_Change(32, 120, lblXmax);
            linepackomronsystem.UnitData_Change(32, 121, lblXmin);
            linepackomronsystem.UnitData_Change(32, 122, lblYmax);
            linepackomronsystem.UnitData_Change(32, 123, lblYmin);
            linepackomronsystem.UnitData_Change(32, 124, llbldegmax);//Normal degree inspection
            linepackomronsystem.UnitData_Change(32, 125, lbldegmin);

            if (SpektekOn)//border for spektek
            {

                linepackomronsystem.UnitData_Change(104, 121, Speklefmin);
                linepackomronsystem.UnitData_Change(104, 120, Speklefmax);
                linepackomronsystem.UnitData_Change(104, 123, Spekritemin);
                linepackomronsystem.UnitData_Change(104, 122, Spekritemax);
                linepackomronsystem.UnitData_Change(104, 125, Spektopmin);
                linepackomronsystem.UnitData_Change(104, 124, Spektopmax);
                linepackomronsystem.UnitData_Change(104, 127, Spekbtmmin);
                linepackomronsystem.UnitData_Change(104, 126, Spekbtmmax);
            }
            else if (IntelOn)//border for intel
            {
                linepackomronsystem.UnitData_Change(104, 121, Intlefmin);
                linepackomronsystem.UnitData_Change(104, 120, Intlefmax);
                linepackomronsystem.UnitData_Change(104, 123, Intritemin);
                linepackomronsystem.UnitData_Change(104, 122, Intritemax);
                linepackomronsystem.UnitData_Change(104, 125, Inttopmin);
                linepackomronsystem.UnitData_Change(104, 124, Inttopmax);
                linepackomronsystem.UnitData_Change(104, 127, Intbtmmin);
                linepackomronsystem.UnitData_Change(104, 126, Intbtmmax);
            }
            else //border for other
            {
                linepackomronsystem.UnitData_Change(104, 121, pleftmin);              //Left
                linepackomronsystem.UnitData_Change(104, 120, plefmax);
                linepackomronsystem.UnitData_Change(104, 123, pritmin);            //Right
                linepackomronsystem.UnitData_Change(104, 122, pritemax);
                linepackomronsystem.UnitData_Change(104, 125, ptopmin);          //TOp
                linepackomronsystem.UnitData_Change(104, 124, ptopmax);
                linepackomronsystem.UnitData_Change(104, 127, pbtmmin);            //bottom
                linepackomronsystem.UnitData_Change(104, 126, pbtmmax);
            }
        }
        public void MultiData_OcrClear()
        {
            StringBuilder sb = new StringBuilder();
            for (int j = 10; j < 24; j++)
            {
                sb.Append(j.ToString() + " 136  ");
                for (int l = 144; l < 152; l++)
                {
                    sb.Append(j.ToString() + " " + l + "  ");
                }
                linepackomronsystem.MultiData_Change(sb.ToString());
                sb.Clear();
            }

        }
        public void BCClear2()  // Obsolete, Use MultiData_Change instead
        {
            //setting unit data for OCR
            string tmp = @"""""";
            string tmp2 = "0";
            try
            {
                for (int j = 10; j < 24; j++)
                {

                    linepackomronsystem.UnitData_Change(j, 136, tmp);

                    for (int l = 144; l < 152; l++)
                    {
                        linepackomronsystem.UnitData_Change(j, l, tmp2);
                    }
                }
                //setting unit data for barcode
                for (int i = 136; i < 152; i++)
                {
                    //linepackomronsystem.UnitData_Change(44, i, tmp);
                    //linepackomronsystem.UnitData_Change(47, i, tmp);
                    //linepackomronsystem.UnitData_Change(52, i, tmp);
                    //linepackomronsystem.UnitData_Change(54, i, tmp);
                    //linepackomronsystem.UnitData_Change(57, i, tmp);
                    //linepackomronsystem.UnitData_Change(55, i, tmp);
                    //linepackomronsystem.UnitData_Change(45, i, tmp);
                    //linepackomronsystem.UnitData_Change(50, i, tmp);
                }
                // MessageBox.Show("complete");
            }
            catch (Exception ex)
            {
                //  MessageBox.Show(ex.ToString());
            }
        }
        public void MultiData_BCClear()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 10; i < 16; i++)
            {
                for (int k = 136; k < 144; k++)
                {
                    sb.Append(i.ToString() + " " + k + "  ");
                }
                linepackomronsystem.MultiData_Change(sb.ToString());
                sb.Clear();
                for (int k = 144; k < 152; k++)
                {
                    sb.Append(i.ToString() + " " + k + "  ");
                }
                linepackomronsystem.MultiData_Change(sb.ToString());
                sb.Clear();
            }
            for (int k = 136; k < 144; k++)
            {
                sb.Append("45 " + k + "  ");
            }
            linepackomronsystem.MultiData_Change(sb.ToString());
            sb.Clear();

            for (int k = 136; k < 144; k++)
            {
                sb.Append("50 " + k + "  ");
            }
            linepackomronsystem.MultiData_Change(sb.ToString());
            sb.Clear();
            for (int k = 136; k < 144; k++)
            {
                sb.Append("55 " + k + "  ");
            }
            linepackomronsystem.MultiData_Change(sb.ToString());
            sb.Clear();
        }
        public void BCClear()    // Obsolete, Use MultiData_Change instead
        {
            //setting unit data for barcode
            string tmp = @"""""";
            try
            {
                for (int j = 10; j < 16; j++)
                {
                    for (int k = 136; k < 144; k++)
                    {

                        linepackomronsystem.UnitData_Change(j, k, tmp);
                    }
                    for (int l = 144; l < 152; l++)
                    {
                        linepackomronsystem.UnitData_Change(j, l, tmp);
                    }
                }
                //setting unit data for barcode
                for (int i = 136; i < 152; i++)
                {
                    //linepackomronsystem.UnitData_Change(44, i, tmp);
                    //linepackomronsystem.UnitData_Change(47, i, tmp);
                    //linepackomronsystem.UnitData_Change(52, i, tmp);
                    //linepackomronsystem.UnitData_Change(54, i, tmp);
                    //linepackomronsystem.UnitData_Change(57, i, tmp);
                    linepackomronsystem.UnitData_Change(45, i, tmp);
                    linepackomronsystem.UnitData_Change(55, i, tmp);
                    linepackomronsystem.UnitData_Change(50, i, tmp);
                }
                // MessageBox.Show("complete");
            }
            catch (Exception ex)
            {
                //  MessageBox.Show(ex.ToString());
            }
        }
        public void InspectionBCRead(string imagepath)
        {
            linepackomronsystem.Scene_Switch(0);
            linepackomronsystem.CalibrateImage(imagepath);
            linepackomronsystem.Scene_Switch(2);
            BCClear();
            SendBCData();
            linepackomronsystem.TeachImage2();
            //          linepackomronsystem.Measure_Once();
        }

        public void InspectionBCReadForSt7(string imagepath)
        {

            linepackomronsystem.Scene_Switch(2);
            BCClear();
            SendBCData();
            linepackomronsystem.TeachImage2();
            Thread.Sleep(500);
            string tmp = linepackomronsystem.UnitData_Get(62, 1000);
            tmp.Trim();
            double restdouble = double.Parse(tmp);
            int result = (int)restdouble;
            if (result < 0)
            {
                throw new Exception("barcode read fail Failed.");
            }

            //linepackomronsystem.Measure_Once();
        }




        //public void LoadOCRlocation()
        //{
        //    //sending of OCRStrings
        //    #region ocrcodes
        //    int ocr_xy_index = 144;//everytime index by 2
        //    int ocr_string_index = 136;
        //    int ocr_unit = 10;//11 12 13 14 15
        //    foreach (string[] ocr in ocrlist)
        //    {
        //        //place in code for writng to omron
        //        linepackomronsystem.UnitData_Change(ocr_unit, ocr_string_index, ocr[6]);//verification strings
        //        linepackomronsystem.UnitData_Change(ocr_unit, ocr_xy_index, ocr[0]);//x coordinates
        //        ocr_xy_index++;
        //        ocr_string_index++;//dummy index
        //        linepackomronsystem.UnitData_Change(ocr_unit, ocr_xy_index, ocr[1]);//y coordinates

        //        //end of code write
        //        if (ocr_xy_index < 151)
        //        {
        //            ocr_string_index++;
        //            ocr_xy_index++;
        //        }
        //        else
        //        {
        //            ocr_unit++;
        //            ocr_xy_index = 144;
        //            ocr_string_index = 136;
        //        }
        //    }
        //    #endregion

        //}
        public void InspectionOCRRead(string imagepath)
        {
            linepackomronsystem.Scene_Switch(3);
            LoadOCRlocation();
            linepackomronsystem.TeachImage3(imagepath);
            linepackomronsystem.Measure_Once();
        }
    }
}
