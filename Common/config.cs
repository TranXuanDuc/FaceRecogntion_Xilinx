using System;
using System.Windows.Interop;
using System.Windows;
using System.IO;
using System.Xml;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using System.Text;
using System.Diagnostics;
using System.Windows.Media;
namespace HD2
{
    public class Config //class doc file config
    {
        private string path = "";
        //dung lenh da co trong kernel32 cua windows

        public Config(string thePath)
        {
            this.path = thePath;
        }

        // doc thong tin trong file ini

        public string ReadValue(string section, string key)
        {
            StringBuilder tmp = new StringBuilder(255);
            long i = NativeMethods.GetPrivateProfileString(section, key, "", tmp, 255, this.path);
            return tmp.ToString();
        }
    }

    public enum configList //danh sach cac tham so config
    {
        //[DeviceName]
        DeviceName,
        //[SizeScreen]
        widthHor,
        heightHor,
        widthVer,
        heightVer,
        layout,
        //[LOG]
        ScreenCap_Path,
        DepthImg_Path,
        LOG_PATH,
        WRITE_LOG_PATH,
        Time_Counter,
        HD_TIMER,
        //[Time]
        TimerInteractive,
        TimerGesture,
        TimerClick,
        TimerHold,
        TimerAllowInteractive,
        //[ClickEvent]
        deltaz,
        MIN_HAND_STILL,
        MAX_DELTAXY,
        MAX_SUBTRACTION,
        MIN_DELTA_Y,
        MIN_DELTA_X,
        MIN_SWIPE_X,
        MIN_SWIPE_Y,
        MIN_THRESHOLD_SWIPE_HOR,
        MIN_THRESHOLD_SWIPE_VER,
        MIN_COUNT_SWIPE,
        //[InitSensor]
        Debug,
        COUNT_FRAME,
        LCD_Center,
        SENSOR_HIGH,
        //[mouse]
        arrayAdaptiveD1,
        arrayAdaptiveD2,
        arrayAdaptiveD3,
        arrayAdaptiveD4,
        //[CaptureRGBImage]
        PATH,
        EnableGrid,
        ScaleX,
        ScaleY,
        bgpath,
        repath,
        VideoPath,
        vlcpath,
        photopath,
        vlcstr,
        sound,
        ScreenShotTime,
        VlcRecordTime,
        UserView,
        MysqlId,
        MysqlPass,
        photopreviewpath,
        refreshtimer,
    }

    static class ConfigParams
    {
        public static string[] arrConfig = new string[70];// Mang de luu cac thong so trong file config.ini
        public static string id = "root";
        public static string pass = "";
        public static bool handGrip = false;
        public static string VideoPath = Environment.CurrentDirectory + "\\OutputVideo\\";
        public static string vlcpath = "d:/VLC/";
        public static string photoPath = "E:/ScreenShot";
        public static string vlcstr;
        public static bool EnableSound = false;
        public static int ScreenShotTime = 6;
        public static bool UserView = false;
        public static bool EnableGrid = false;
        public static string RecordTime = "360";
        public static string PhotoPreviewPath = "c:/xampp/htdocs/OneDrive/OUTPUT/example/preview/";
        public static int refreshtime = 5;
        /*..............................................................................
            * Author: Tran Xuan Duc - 21/12/2016
            * Chup anh thay nen 
           */
        public static string bgpath; //path to read background remove text
        public static string repath;  //path to read record text
        //*****************************************************************************************************************
    }

    class LoadConfig
    {
        bool LastDebugState = false;
        public LoadConfig()
        {  //call in main window
            InitializeLoadConfig();
            InitializeWriteLogTimer();
        }
        void InitializeLoadConfig()
        {
            loadConfig();
            LoadDebug();
            DispatcherTimer ReadConfigTimer = new DispatcherTimer();
            ReadConfigTimer.Tick += readConfigTimer_Tick;
            /*..............................................................................
            * Author: Tran Xuan Duc - 21/12/2016
            * Chup anh thay nen 
           */

            try
            {
                using (StreamReader sr = new StreamReader("repath.txt"))
                {
                    string path = sr.ReadLine();
                    if (path != null)
                    {
                        ConfigParams.repath = path;
                    }
                    else ConfigParams.repath = Environment.CurrentDirectory + "\\re.txt";
                }
            }
            catch (Exception) { }
            try
            {
                using (StreamReader sr = new StreamReader("bgpath.txt"))
                {
                    string path = sr.ReadLine();
                    if (path != null)
                    {
                        ConfigParams.bgpath = path;
                    }
                    else ConfigParams.bgpath = Environment.CurrentDirectory + "\\bg.txt";
                    using (FileStream fs = new FileStream(ConfigParams.bgpath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        using (StreamWriter sw = new StreamWriter(fs))
                        {
                            sw.Write("");
                        }
                    }
                }
            }
            catch (Exception) { }
            //******End***********************************************************************************************************
            int time = Int32.Parse(ConfigParams.arrConfig[(int)configList.Time_Counter]); //5s
            ReadConfigTimer.Interval = new TimeSpan(0, 0, time);
            ReadConfigTimer.Start();
        }
       
        void InitializeWriteLogTimer()
        {
            DispatcherTimer WriteHTMLLogTimer = new DispatcherTimer();
            WriteHTMLLogTimer.Tick += WriteHTMLLogTimer_Tick;
            int time = Int32.Parse(ConfigParams.arrConfig[(int)configList.Time_Counter]);
            WriteHTMLLogTimer.Interval = new TimeSpan(0, 0, time);
            WriteHTMLLogTimer.Start();
        }
        void WriteHTMLLogTimer_Tick(object sender, EventArgs e)
        {
            if (LastDebugState && UserManager.Users.Count > 0 && ConfigParams.UserView)
            {
                writeHtmlLog();
            }
        }

        private void readConfigTimer_Tick(object sender, EventArgs e)
        {
            loadConfig();
            CheckDebug();
        }

        void loadConfig()
        {
            string path = Environment.CurrentDirectory + "\\config.ini";
            Config file = new Config(path);
            //[DeviceName]
            ConfigParams.arrConfig[(int)configList.DeviceName] = file.ReadValue("DeviceName", "name");
            //[SizeScreen]
            ConfigParams.arrConfig[(int)configList.widthHor] = file.ReadValue("SizeScreen", "widthHor");
            ConfigParams.arrConfig[(int)configList.heightHor] = file.ReadValue("SizeScreen", "heightHor");
            ConfigParams.arrConfig[(int)configList.widthVer] = file.ReadValue("SizeScreen", "widthVer");
            ConfigParams.arrConfig[(int)configList.heightVer] = file.ReadValue("SizeScreen", "heightVer");
            ConfigParams.arrConfig[(int)configList.layout] = file.ReadValue("SizeScreen", "layout");
            //[LOG]
            ConfigParams.arrConfig[(int)configList.ScreenCap_Path] = file.ReadValue("LOG", "ScreenCap_Path");
            ConfigParams.arrConfig[(int)configList.DepthImg_Path] = file.ReadValue("LOG", "DepthImg_Path");
            ConfigParams.arrConfig[(int)configList.LOG_PATH] = file.ReadValue("LOG", "LOG_PATH");
            ConfigParams.arrConfig[(int)configList.WRITE_LOG_PATH] = file.ReadValue("LOG", "WRITE_LOG_PATH");
            ConfigParams.arrConfig[(int)configList.Time_Counter] = file.ReadValue("LOG", "Time_Counter");
            ConfigParams.arrConfig[(int)configList.HD_TIMER] = file.ReadValue("LOG", "HD_TIMER");

            //[Time]
            ConfigParams.arrConfig[(int)configList.TimerInteractive] = file.ReadValue("Time", "TimerInteractive");
            ConfigParams.arrConfig[(int)configList.TimerGesture] = file.ReadValue("Time", "TimerGesture");
            ConfigParams.arrConfig[(int)configList.TimerClick] = file.ReadValue("Time", "TimerClick");
            ConfigParams.arrConfig[(int)configList.TimerHold] = file.ReadValue("Time", "TimerHold");
            ConfigParams.arrConfig[(int)configList.TimerAllowInteractive] = file.ReadValue("Time", "TimerAllowInteractive");
            //[ClickEvent]
            ConfigParams.arrConfig[(int)configList.deltaz] = file.ReadValue("ClickEvent", "deltaz");
            ConfigParams.arrConfig[(int)configList.MIN_HAND_STILL] = file.ReadValue("ClickEvent", "MIN_HAND_STILL");
            ConfigParams.arrConfig[(int)configList.MAX_DELTAXY] = file.ReadValue("ClickEvent", "MAX_DELTAXY");
            ConfigParams.arrConfig[(int)configList.MAX_SUBTRACTION] = file.ReadValue("ClickEvent", "MAX_SUBTRACTION");
            ConfigParams.arrConfig[(int)configList.MIN_DELTA_Y] = file.ReadValue("ClickEvent", "MIN_DELTA_Y");
            ConfigParams.arrConfig[(int)configList.MIN_DELTA_X] = file.ReadValue("ClickEvent", "MIN_DELTA_X");
            ConfigParams.arrConfig[(int)configList.MIN_SWIPE_X] = file.ReadValue("ClickEvent", "MIN_SWIPE_X");
            ConfigParams.arrConfig[(int)configList.MIN_SWIPE_Y] = file.ReadValue("ClickEvent", "MIN_SWIPE_Y");
            ConfigParams.arrConfig[(int)configList.MIN_THRESHOLD_SWIPE_HOR] = file.ReadValue("ClickEvent", "MIN_THRESHOLD_SWIPE_HOR");
            ConfigParams.arrConfig[(int)configList.MIN_THRESHOLD_SWIPE_VER] = file.ReadValue("ClickEvent", "MIN_THRESHOLD_SWIPE_VER");
            ConfigParams.arrConfig[(int)configList.MIN_COUNT_SWIPE] = file.ReadValue("ClickEvent", "MIN_COUNT_SWIPE");
            //[InitSensor]
            ConfigParams.arrConfig[(int)configList.Debug] = file.ReadValue("InitSensor", "Debug");
            ConfigParams.arrConfig[(int)configList.COUNT_FRAME] = file.ReadValue("InitSensor", "COUNT_FRAME ");
            ConfigParams.arrConfig[(int)configList.LCD_Center] = file.ReadValue("InitSensor", "LCD_Center");
            ConfigParams.arrConfig[(int)configList.SENSOR_HIGH] = file.ReadValue("InitSensor", "SENSOR_HIGH");
            ConfigParams.arrConfig[(int)configList.EnableGrid] = file.ReadValue("InitSensor", "EnableGrid");

            //[Mouse]
            ConfigParams.arrConfig[(int)configList.arrayAdaptiveD1] = file.ReadValue("mouse", "arrayAdaptiveD1");
            ConfigParams.arrConfig[(int)configList.arrayAdaptiveD2] = file.ReadValue("mouse", "arrayAdaptiveD2");
            ConfigParams.arrConfig[(int)configList.arrayAdaptiveD3] = file.ReadValue("mouse", "arrayAdaptiveD3");
            ConfigParams.arrConfig[(int)configList.arrayAdaptiveD4] = file.ReadValue("mouse", "arrayAdaptiveD4");
            ConfigParams.arrConfig[(int)configList.ScaleX] = file.ReadValue("mouse", "ScaleX");
            ConfigParams.arrConfig[(int)configList.ScaleY] = file.ReadValue("mouse", "ScaleY");
            //[CaptureRGBImage]
            ConfigParams.arrConfig[(int)configList.PATH] = file.ReadValue("CaptureRGBImage", "PATH");
            ConfigParams.arrConfig[(int)configList.VideoPath] = file.ReadValue("LOG", "VideoPath");
            ConfigParams.arrConfig[(int)configList.vlcpath] = file.ReadValue("LOG", "vlcpath");
            ConfigParams.arrConfig[(int)configList.photopath] = file.ReadValue("LOG", "photopath");
            ConfigParams.arrConfig[(int)configList.vlcstr] = file.ReadValue("LOG", "vlcstr");
            ConfigParams.arrConfig[(int)configList.sound] = file.ReadValue("LOG", "Sound");
            ConfigParams.arrConfig[(int)configList.ScreenShotTime] = file.ReadValue("Time", "ScreenShotTime");
            ConfigParams.arrConfig[(int)configList.VlcRecordTime] = file.ReadValue("Time", "RecordTime");
            ConfigParams.arrConfig[(int)configList.UserView] = file.ReadValue("InitSensor", "UserView");
            //Mysql
            //ConfigParams.arrConfig[(int)configList.MysqlId] = file.ReadValue("MYSQL", "id");
            //ConfigParams.arrConfig[(int)configList.MysqlPass] = file.ReadValue("MYSQL", "pass");
            ConfigParams.arrConfig[(int)configList.photopreviewpath] = file.ReadValue("LOG", "photopreview");
            ConfigParams.arrConfig[(int)configList.refreshtimer] = file.ReadValue("LOG", "RefreshTime");
        }

        void LoadDebug()
        {
            string debug = ConfigParams.arrConfig[(int)configList.Debug];
            if (debug == "on")
            {
                LastDebugState = true;
                //ShowLabels(LastDebugState);
            }
            else
            {
                LastDebugState = false;
                //ShowLabels(LastDebugState);
            }
            string userView = ConfigParams.arrConfig[(int)configList.UserView];
            if (userView == "on")
            {
                ConfigParams.UserView = true;
            }
            else ConfigParams.UserView = false;
            string sound = ConfigParams.arrConfig[(int)configList.sound];
            if (sound == "on")
            {
                ConfigParams.EnableSound = true;
            }
            else ConfigParams.EnableSound = false;
            string screenshot = ConfigParams.arrConfig[(int)configList.ScreenShotTime];
            try
            {
                int result = Convert.ToInt16(screenshot);
                ConfigParams.ScreenShotTime = result;
            }
            catch (Exception e) { Trace.WriteLine(e.ToString()); }
            string refreshtime = ConfigParams.arrConfig[(int)configList.refreshtimer];
            try
            {
                int result = Convert.ToInt16(refreshtime);
                ConfigParams.refreshtime = result;
            }
            catch (Exception e) { Trace.WriteLine(e.ToString()); }
            ConfigParams.RecordTime = ConfigParams.arrConfig[(int)configList.VlcRecordTime];
            //ConfigParams.id = ConfigParams.arrConfig[(int)configList.MysqlId];
            //ConfigParams.pass = ConfigParams.arrConfig[(int)configList.MysqlPass];
            ConfigParams.PhotoPreviewPath = ConfigParams.arrConfig[(int)configList.photopreviewpath];
        }

        void CheckDebug()
        {
            string debug = ConfigParams.arrConfig[(int)configList.Debug];
            if (debug == "on")
            {
                if (!LastDebugState)
                {
                    LastDebugState = true;
                    //ShowLabels(LastDebugState);
                }
            }
            else
            {
                if (LastDebugState)
                {
                    LastDebugState = false;
                    //ShowLabels(LastDebugState);
                }
            }
            string userView = ConfigParams.arrConfig[(int)configList.UserView];
            if (userView == "on")
            {
                ConfigParams.UserView = true;
            }
            else ConfigParams.UserView = false;
            string enableGrid = ConfigParams.arrConfig[(int)configList.EnableGrid];
            if (enableGrid == "on")
            {
                ConfigParams.EnableGrid = true;
            }
            else
            {
                ConfigParams.EnableGrid = false;
            }
            string sound = ConfigParams.arrConfig[(int)configList.sound];
            if (sound == "on")
            {
                ConfigParams.EnableSound = true;
            }
            else ConfigParams.EnableSound = false;
        }

        private void writeHtmlLog()
        {
            string ScreenCapPath = ConfigParams.arrConfig[(int)configList.ScreenCap_Path];
            string DepthImgPath = ConfigParams.arrConfig[(int)configList.DepthImg_Path];
            string DeviceName = ConfigParams.arrConfig[(int)configList.DeviceName];
            string LogPath = ConfigParams.arrConfig[(int)configList.LOG_PATH];

            int bodyCount = UserManager.Users.Count;

            string currentDepthImg = "imageDepth" + DateTime.Now.ToString("yyyy_MM_dd_hh_mm_ss") + ".jpg";
            string currentScreen = "Screen_" + DateTime.Now.ToString("yyyy_MM_dd.hh_mm_ss") + ".jpg";
            // Capture Depth Image
            string filepath = Path.Combine(LogPath, DepthImgPath);
            if (!Directory.Exists(filepath))
                Directory.CreateDirectory(filepath);
            using (var fileStream = new FileStream(filepath + currentDepthImg, FileMode.Create))
            {
                MainWindow main = (MainWindow)App.Current.MainWindow;
                RenderTargetBitmap renderBitmap = new RenderTargetBitmap((int)main.UserView.ActualWidth, (int)main.UserView.ActualHeight, 96.0, 96.0, PixelFormats.Pbgra32);

                DrawingVisual dv = new DrawingVisual();
                using (DrawingContext dc = dv.RenderOpen())
                {
                    VisualBrush brush = new VisualBrush(main.UserView);
                    dc.DrawRectangle(brush, null, new Rect(new Point(), new Size(main.UserView.ActualWidth, main.UserView.ActualHeight)));
                }

                renderBitmap.Render(dv);
                JpegBitmapEncoder encoder = new JpegBitmapEncoder();

                encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
                encoder.Save(fileStream);
            }
            string screencappathCombined = Path.Combine(LogPath + ScreenCapPath);
            if (!Directory.Exists(screencappathCombined))
            {
                Directory.CreateDirectory(screencappathCombined);
            }
            // Capture Screen and Save for HTML log 
            using (var fileStream1 = new FileStream(screencappathCombined + currentScreen, FileMode.Create))
            {
                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(CopyScreen()));
                encoder.Save(fileStream1);
            }

            using (StreamWriter html = new StreamWriter(LogPath + "gds_log_" + DateTime.Now.ToString("yyyy_MM_dd.hh") + ".html", true))
            {

                html.WriteLine("<table border='1'>");
                html.WriteLine("<title>GDS Log</title>");
                html.WriteLine("<tr>");
                html.WriteLine("<td><img src='" + DepthImgPath + currentDepthImg + "' width=160 height=120></img></td>");//anh capture Depth image
                html.WriteLine("<td><img src='" + ScreenCapPath + currentScreen + "' width=160 height=120></img></td>");//anh capture man hinh
                html.WriteLine("</tr>");
                html.WriteLine("<br>");
                html.WriteLine("<tr>");
                html.WriteLine("<td><font size ='3'>Thoi gian: " + DateTime.Now.ToString("hh:mm:ss") + "</font></td>");
                html.WriteLine("<td> <font size='3' >So nguoi:</font> <font size ='3'color ='green'>" + bodyCount + "</font></td>");
                html.WriteLine("</tr>");
                html.WriteLine("</table>");
                html.WriteLine("<br>");
                html.WriteLine("SUM|" + DateTime.Now.ToString("hh:mm:ss") + "|" + bodyCount + "|ext");
                Write(DeviceName + "_log" + DateTime.Now.ToString("yyyy_MM_dd.hh"), "SUM|" + DateTime.Now.ToString("yyyy_MM_dd hh:mm:ss") + "|" + bodyCount + "|" + ScreenCapPath + currentScreen + "|ext");
            }

        }

        private void Write(string fileName, string sline, bool writeOption = true)
        {
            string writeLogPath = ConfigParams.arrConfig[(int)configList.WRITE_LOG_PATH];
            string Debug = ConfigParams.arrConfig[(int)configList.Debug];
            if (Debug == "on")
            {
                using (StreamWriter file = new StreamWriter(writeLogPath + fileName + ".txt", writeOption))
                { // TODO: Fix, must put file path into config.ini 
                    file.WriteLine(sline);
                }
            }
        }

        private BitmapSource CopyScreen()
        {

            using (var screenBmp = new System.Drawing.Bitmap(

                (int)SystemParameters.PrimaryScreenWidth,

                (int)SystemParameters.PrimaryScreenHeight,

                System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {

                using (var bmpGraphics = System.Drawing.Graphics.FromImage(screenBmp))
                {

                    bmpGraphics.CopyFromScreen(0, 0, 0, 0, screenBmp.Size);

                    return Imaging.CreateBitmapSourceFromHBitmap(

                        screenBmp.GetHbitmap(),

                        IntPtr.Zero,

                        Int32Rect.Empty,

                        BitmapSizeOptions.FromEmptyOptions());
                }
            }
        }
    }
}
