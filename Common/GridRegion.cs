using System;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.Windows.Input;
using System.Diagnostics;
namespace HD2
{
    enum SwipeAction { 
        none,
        sendkey,
        click,

    }
     class GridRegion
    {
         int numberRegionPageWeb = 0;
         int xUpperLeft, yUpperLeft, xLowerRight, yLowerRight;
         int xIndexLeft, yIndexLeft, xIndexRight, yIndexRight;
        public  int pixel_x = 25;
        public  int pixel_y = 25;

        private const string defaultRegion = "region1";
         public int[,] arrayGridRegion;

         public int[] arrayXmouseDefault;
         public int[] arrayYmouseDefault;
        private  string s_layout = "";

         private string[,] arrayAction = new string[100, 2];
         private string[] arrayAction1 = new string[100];
         private string[] arrayAction2 = new string[100];

        private  int numberActionInRegion = 0;

        public  bool enClick = true;
        public  bool enHold = false;
        public  bool enSwipe = false;
        
        public GridRegion() { 
        
        }
        public  void readActiveRegion(string filenameRegionMap)
        {
            try {
                if (!File.Exists(filenameRegionMap))
                {
                    switch (s_layout)
                    {
                        case "Ver":
                            filenameRegionMap = "RegionActive/V/default.xml";
                            break;
                        case "Hor":
                            filenameRegionMap = "RegionActive/H/default.xml";
                            break;
                    }
                }
            }
            catch (IOException) { }
            try {
                using (XmlReader reader = XmlReader.Create(filenameRegionMap))
                {
                    while (reader.Read())
                    {
                        XmlNodeType type;
                        type = reader.NodeType;
                        if (type == XmlNodeType.Element)
                        {
                            switch (reader.Name)
                            {
                                case "REGION":
                                    numberActionInRegion = 0;
                                    reader.Read();
                                    numberRegionPageWeb++;
                                    if (reader.Value == defaultRegion && (numberRegionPageWeb > 1))
                                    {
                                        numberRegionPageWeb--;
                                    }
                                    break;
                                case "xUpperLeft":
                                    reader.Read();
                                    xUpperLeft = Convert.ToInt32(reader.Value);
                                    xIndexLeft = xUpperLeft / pixel_x;
                                    break;
                                case "yUpperLeft":
                                    reader.Read();
                                    yUpperLeft = Convert.ToInt32(reader.Value);
                                    yIndexLeft = yUpperLeft / pixel_y;
                                    break;
                                case "xLowerRight":
                                    reader.Read();
                                    xLowerRight = Convert.ToInt32(reader.Value);
                                    xIndexRight = xLowerRight / pixel_x;
                                    break;
                                case "yLowerRight":
                                    reader.Read();
                                    yLowerRight = Convert.ToInt32(reader.Value);
                                    yIndexRight = yLowerRight / pixel_y;
                                    for (int r = yIndexLeft; r < yIndexRight; r++)
                                        for (int c = xIndexLeft; c < xIndexRight; c++)
                                            arrayGridRegion[r, c] = numberRegionPageWeb;
                                    break;
                                case "xMouseDefault":
                                    reader.Read();
                                    arrayXmouseDefault[numberRegionPageWeb] = Convert.ToInt32(reader.Value);
                                    break;
                                case "yMouseDefault":
                                    reader.Read();
                                    arrayYmouseDefault[numberRegionPageWeb] = Convert.ToInt32(reader.Value);
                                    break;
                                case "action":
                                    reader.Read();
                                    arrayAction[numberRegionPageWeb, numberActionInRegion] = reader.Value;
                                    numberActionInRegion++;
                                    break;
                                case "action1":
                                    reader.Read();
                                    arrayAction1[numberRegionPageWeb] = reader.Value;
                                    break;
                                case "action2":
                                    reader.Read();
                                    arrayAction2[numberRegionPageWeb] = reader.Value;
                                    break;
                            }
                        }
                    }
                }
            }
            catch (IOException) { }
            
        }

        public  void matchActiveRegion()
        {
            MainWindow main = (MainWindow)App.Current.MainWindow ;
            main.CaptureMouse();
            var Position = Mouse.GetPosition(main);
            int x = (int)Position.X;
            int y = (int)Position.Y;
            main.ReleaseMouseCapture();

            DisableControlMouse();

            int i, j;
            i = y / pixel_y;
            j = x / pixel_x;

            if (i >= 0 && j >= 0 && i<100 && j<100)
            {
                for (int k = 0; k < numberActionInRegion; k++)
                {
                    try
                    {
                        if (arrayAction[arrayGridRegion[i, j], k] == "click")
                            enClick = true;
                        if (arrayAction[arrayGridRegion[i, j], k] == "hold")
                            enHold = true;
                        enHold = true;
                        if (arrayAction[arrayGridRegion[i, j], k] == "swipe")
                            enSwipe = true;
                    }
                    catch (Exception) { }
                }
            }
            if (enSwipe)
            {
                 //Ex:swiperight:sendkey:{RIGHT} | swiperight:click:23:430
                if (arrayAction1[numberRegionPageWeb].IndexOf(":") == -1) return;
                UserManager.SwipeRightArray = arrayAction1[numberRegionPageWeb].Split(':');
                string m_actionState2 = arrayAction2[numberRegionPageWeb]; //Ex:swipeleft:sendkey:{LEFT} | swipeleft:click:1249:430 
                if (m_actionState2.IndexOf(":") == -1) return;
                UserManager.SwipeLeftArray = m_actionState2.Split(':');
                switch (UserManager.SwipeRightArray[1])
                {
                    case "sendkey": //read action when swipe right 
                        UserManager.SwipeRightAction = SwipeAction.sendkey;
                        break;
                    case "click":
                        UserManager.SwipeRightAction = SwipeAction.click;
                        break;
                    default:
                        UserManager.SwipeRightAction = SwipeAction.none;
                        break;

                }
                switch (UserManager.SwipeLeftArray[1]) //read action when swipe left
                {
                    case "sendkey":
                        UserManager.SwipeLeftAction = SwipeAction.sendkey;
                        break;
                    case "click":
                        UserManager.SwipeLeftAction = SwipeAction.click;
                        break;
                    default:
                        UserManager.SwipeLeftAction = SwipeAction.none;
                        break;
                }
            }
           
        }
         void DisableControlMouse() {
            enHold = false;
            enClick = false;
            enSwipe = false;
        }
        public  string readWebPage()
        {
            
            numberActionInRegion = 0;
            s_layout = ConfigParams.arrConfig[(int)configList.layout];
            numberRegionPageWeb = 0;
            arrayGridRegion = new int[100, 100];

            arrayXmouseDefault = new int[100];
            arrayYmouseDefault = new int[100];

            xUpperLeft = 0;
            yUpperLeft = 0;
            xLowerRight = 0;
            yLowerRight = 0;
            xIndexLeft = 0;
            yIndexLeft = 0;
            xIndexRight = 0;
            yIndexRight = 0;
            string namePage = "home";
            if (!IsFileInUse("webPage.txt"))

            {
                try {
                    using (StreamReader readNamePage = new StreamReader("webPage.txt"))
                    {
                        if (readNamePage.Peek() >= 0)
                            namePage = readNamePage.ReadLine();
                    }
                }
                catch (IOException ) { }
            }
            string s_pathnameWeb = "";
            switch (s_layout)
            {  // Note: Hung added 19/3 ==> Co the gay loi neu thua ky tu white space, nen co ham Trim String !! 
                case "Ver":
                    s_pathnameWeb = "RegionActive/V/" + namePage + ".xml";
                    break;
                case "Hor":
                    s_pathnameWeb = "RegionActive/H/" + namePage + ".xml";
                    break;
            }
            return s_pathnameWeb;

        }
         bool IsFileInUse(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return false;
            }
            try
            {
                using (SafeHandle handleValue = NativeMethods.CreateFile(filePath, NativeMethods.GENERIC_WRITE, 0, IntPtr.Zero, NativeMethods.OPEN_EXISTING, 0, IntPtr.Zero))
                {
                    bool inUse = handleValue.IsInvalid;

                    return inUse;
                }
            }
            catch (IOException )
            {
                return false;
            }



        }
     
        

    }
     
}
