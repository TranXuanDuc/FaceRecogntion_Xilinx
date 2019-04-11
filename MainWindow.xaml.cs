//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace HD2
{
    using System;

    using System.Windows;
    using Microsoft.Kinect;
    using Microsoft.Kinect.Wpf.Controls;
    using MySql.Data;
    using MySql.Data.MySqlClient;
    using System.Diagnostics;
    using System.ComponentModel;
    using System.Windows.Threading;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.IO;
    using System.Threading.Tasks;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Windows.Interop;
    using Emgu.CV;
    using Emgu.CV.Util;
    using Emgu.CV.Structure;

    using System.Runtime.InteropServices;
    public partial class MainWindow : Window
    {
        private KinectSensor kinectSensor = null;
        private ushort[] depthFrameData = null;
        private readonly int bytesPerPixel = (PixelFormats.Bgr32.BitsPerPixel + 7) / 8;
        private CoordinateMapper coordinateMapper = null;
        public MultiSourceFrameReader multiFrameSourceReader = null;
        public WriteableBitmap bitmap = null;
        private byte[] colorFrameData = null;
        private byte[] bodyIndexFrameData = null;
        private byte[] displayPixels = null;
        private ColorSpacePoint[] colorPoints = null;
        Process recordProcess;
        public bool isRecording = false;

        UserFrameManager userFrameManager;
        LoadConfig loadConfig;
        /*...............................................................
         * Cac bien cho chup anh thay nen va xu ly anh 
        */
        DispatcherTimer bgRemoveTimer;
        DispatcherTimer PhotoCountdownTimer;
        int countDown = 0;
        // Bien Xu ly anh
        Image<Bgra, Byte> img;
        Image<Gray, Byte> src_gray;
        Image<Bgra, Byte> imgRmBackground;
        Image<Gray, Byte> edge_detected;
        Image<Bgra, Byte> blurred;
       //..................................................................
        public MainWindow()
        {
            kinectSensor = KinectSensor.GetDefault();
            kinectSensor.Open();

            //open multisource frame reader for karaoke
            multiFrameSourceReader = kinectSensor.OpenMultiSourceFrameReader(FrameSourceTypes.Depth | FrameSourceTypes.Color | FrameSourceTypes.BodyIndex);
            this.StateChanged += MainWindow_StateChanged;
            // wire handler for frames arrival

            multiFrameSourceReader.IsPaused = true;
            multiFrameSourceReader.MultiSourceFrameArrived += Reader_MultiSourceFrameArrived;

            // get the coordinate mapper
            coordinateMapper = kinectSensor.CoordinateMapper;

            // get FrameDescription from DepthFrameSource
            FrameDescription depthFrameDescription = kinectSensor.DepthFrameSource.FrameDescription;

            int depthWidth = depthFrameDescription.Width;
            int depthHeight = depthFrameDescription.Height;

            // allocate space to put the pixels being received and converted
            depthFrameData = new ushort[depthWidth * depthHeight];
            bodyIndexFrameData = new byte[depthWidth * depthHeight];
            this.displayPixels = new byte[depthWidth * depthHeight * bytesPerPixel];
            colorPoints = new ColorSpacePoint[depthWidth * depthHeight];


            // create the bitmap to display
            bitmap = new WriteableBitmap(depthWidth, depthHeight, 96.0, 96.0, PixelFormats.Bgra32, null);

            // get FrameDescription from ColorFrameSource
            FrameDescription colorFrameDescription = kinectSensor.ColorFrameSource.FrameDescription;
            int colorWidth = colorFrameDescription.Width;
            int colorHeight = colorFrameDescription.Height;
            // allocate space to put the pixels being received
            colorFrameData = new byte[colorWidth * colorHeight * bytesPerPixel];


            /*..............................................................................
             * Author: Tran Xuan Duc - 21/12/2016
             * Xu Ly anh Emgu
            */
            try
            {
                img = new Image<Bgra, byte>(depthWidth, depthHeight);
            }
            catch (Exception e)
            {
                Trace.WriteLine("Loi khoi tao Emgu");
                Trace.WriteLine(e.ToString());
            }
            try
            {
                src_gray = new Image<Gray, byte>(depthWidth, depthHeight);
            }
            catch (Exception f)
            {
                Trace.WriteLine(f.ToString());
            }
            try
            {
                imgRmBackground = new Image<Bgra, Byte>(depthWidth, depthHeight);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString());
            }
            //******End***********************************************************************************************************
            this.DataContext = this;
            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.LowQuality);
            this.InitializeComponent();
            //UserView
            this.UserView.Width = SystemParameters.PrimaryScreenWidth / 6;
            this.UserView.Height = SystemParameters.PrimaryScreenHeight / 6;

            this.Loaded += OnLoaded;
            recordProcess = new Process();
            /*..............................................................................
             * Author: Tran Xuan Duc - 21/12/2016
             * Chup anh thay nen 
            */
            //tao timer dieu khien anh thay nen va chup anh
            PhotoCountdownTimer = new DispatcherTimer();
            //timer chup anh
            PhotoCountdownTimer.Interval = new TimeSpan(0, 0, 1);
            PhotoCountdownTimer.Tick += PhotoCountdownTimer_Tick;

            //timer dieu khien anh thay nen
            bgRemoveTimer = new DispatcherTimer();
            bgRemoveTimer.Interval = new TimeSpan(0, 0, 1);
            bgRemoveTimer.Tick += bgRemoveTimer_Tick;
            bgRemoveTimer.Start();
            //***********End******************************************************************************************************
        }

        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            Trace.WriteLine(this.WindowState);
            if (this.WindowState == WindowState.Minimized)
                this.WindowState = WindowState.Maximized;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {

            loadConfig = new LoadConfig();
            userFrameManager = new UserFrameManager(kinectSensor);
        }
        private void Reader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            int depthWidth = 0;
            int depthHeight = 0;

            int colorWidth = 0;
            int colorHeight = 0;

            int bodyIndexWidth = 0;
            int bodyIndexHeight = 0;

            bool multiSourceFrameProcessed = false;
            bool colorFrameProcessed = false;
            bool depthFrameProcessed = false;
            bool bodyIndexFrameProcessed = false;

            MultiSourceFrame multiSourceFrame = e.FrameReference.AcquireFrame();

            if (multiSourceFrame != null)
            {
                // Frame Acquisition should always occur first when using multiSourceFrameReader
                using (DepthFrame depthFrame = multiSourceFrame.DepthFrameReference.AcquireFrame())
                {
                    using (ColorFrame colorFrame = multiSourceFrame.ColorFrameReference.AcquireFrame())
                    {
                        using (BodyIndexFrame bodyIndexFrame = multiSourceFrame.BodyIndexFrameReference.AcquireFrame())
                        {
                            if (depthFrame != null)
                            {
                                FrameDescription depthFrameDescription = depthFrame.FrameDescription;
                                depthWidth = depthFrameDescription.Width;
                                depthHeight = depthFrameDescription.Height;

                                if ((depthWidth * depthHeight) == depthFrameData.Length)
                                {
                                    depthFrame.CopyFrameDataToArray(depthFrameData);

                                    depthFrameProcessed = true;
                                }
                            }

                            if (colorFrame != null)
                            {
                                FrameDescription colorFrameDescription = colorFrame.FrameDescription;
                                colorWidth = colorFrameDescription.Width;
                                colorHeight = colorFrameDescription.Height;

                                if ((colorWidth * colorHeight * bytesPerPixel) == colorFrameData.Length)
                                {
                                    if (colorFrame.RawColorImageFormat == ColorImageFormat.Bgra)
                                    {
                                        colorFrame.CopyRawFrameDataToArray(colorFrameData);
                                    }
                                    else
                                    {
                                        colorFrame.CopyConvertedFrameDataToArray(colorFrameData, ColorImageFormat.Bgra);
                                    }

                                    colorFrameProcessed = true;
                                }
                            }

                            if (bodyIndexFrame != null)
                            {
                                FrameDescription bodyIndexFrameDescription = bodyIndexFrame.FrameDescription;
                                bodyIndexWidth = bodyIndexFrameDescription.Width;
                                bodyIndexHeight = bodyIndexFrameDescription.Height;

                                if ((bodyIndexWidth * bodyIndexHeight) == bodyIndexFrameData.Length)
                                {
                                    bodyIndexFrame.CopyFrameDataToArray(bodyIndexFrameData);

                                    bodyIndexFrameProcessed = true;
                                }
                            }

                            multiSourceFrameProcessed = true;
                        }
                    }
                }
            }

            // we got all frames
            if (multiSourceFrameProcessed && depthFrameProcessed && colorFrameProcessed && bodyIndexFrameProcessed)
            {
                coordinateMapper.MapDepthFrameToColorSpace(depthFrameData, colorPoints);

                Array.Clear(displayPixels, 0, displayPixels.Length);

                // loop over each row and column of the depth
                for (int y = 0; y < depthHeight; ++y)
                {
                    for (int x = 0; x < depthWidth; ++x)
                    {
                        // calculate index into depth array
                        int depthIndex = (y * depthWidth) + x;

                        byte player = bodyIndexFrameData[depthIndex];

                        // if we're tracking a player for the current pixel, sets its color and alpha to full
                        if (player != 0xff)
                        {
                            // retrieve the depth to color mapping for the current depth pixel
                            ColorSpacePoint colorPoint = colorPoints[depthIndex];

                            // make sure the depth pixel maps to a valid point in color space
                            int colorX = (int)Math.Floor(colorPoint.X + 0.5);
                            int colorY = (int)Math.Floor(colorPoint.Y + 0.5);
                            if ((colorX >= 0) && (colorX < colorWidth) && (colorY >= 0) && (colorY < colorHeight))
                            {
                                // calculate index into color array
                                int colorIndex = ((colorY * colorWidth) + colorX) * bytesPerPixel;

                                // set source for copy to the color pixel
                                int displayIndex = depthIndex * bytesPerPixel;

                                // write out blue byte
                                this.displayPixels[displayIndex++] = colorFrameData[colorIndex++];

                                // write out green byte
                                this.displayPixels[displayIndex++] = colorFrameData[colorIndex++];

                                // write out red byte
                                this.displayPixels[displayIndex++] = colorFrameData[colorIndex];

                                // write out alpha byte
                                this.displayPixels[displayIndex] = 0xff;
                            }

                        }
                    }
                }
                RenderColorPixels();
            }

        }

        private void RenderColorPixels()
        {

            if (imgRemoveBackground.Source != this.bitmap)
                imgRemoveBackground.Source = this.bitmap;
            this.bitmap.WritePixels(
    new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight),
    this.displayPixels,
    bitmap.PixelWidth * bytesPerPixel,
    0);

        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (userFrameManager != null)
                userFrameManager.Dispose();
            if (this.kinectSensor != null)
            {
                this.kinectSensor.Close();
                this.kinectSensor = null;
            }
        }
        //.............................................................
        /*Author: Tran Xuan Duc 21/12/2016
         * Them chu nang chup anh thay nen
         * Co xu ly anh bang EmguCV
         */

        void HideText()
        {

            CountDownScreenShot.Visibility = Visibility.Hidden;
        }
        void ShowText()
        {

            CountDownScreenShot.Visibility = Visibility.Visible;
        }
        async void AsyncHideText(int millisec)
        {
            await Task.Delay(millisec);
            HideText();
        }
        string currentState = "";
        void bgRemoveTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                using (FileStream fs = new FileStream(ConfigParams.bgpath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite))
                using (StreamReader sr_ = new StreamReader(fs))
                {
                    currentState = sr_.ReadLine();
                    if (currentState == "1") // 1: karaoke, 2 chup anh + chup xong quay lai so 1, 3 vien phi,4  chup anh trong vai giay
                    {

                        turnMultisourceReader(false); //hien anh thay nen
                        turnScreenShot(Visibility.Hidden); //an giao dien screenshot


                    }
                    else if (currentState == "2")
                    {
                        if (!PhotoCountdownTimer.IsEnabled)
                        {
                            turnMultisourceReader(false); //
                            turnScreenShot(Visibility.Hidden);//
                            countDown = ConfigParams.ScreenShotTime;
                            CountDownScreenShot.Content = "";
                            ShowText();
                            PhotoCountdownTimer.Start();
                        }
                    }
                    else if (currentState == "3")
                    {

                        turnMultisourceReader(false);
                        turnScreenShot(Visibility.Hidden);


                    }
                    else if (currentState == "4")
                    {
                        if (!PhotoCountdownTimer.IsEnabled)
                        {
                            turnMultisourceReader(false);
                            turnScreenShot(Visibility.Hidden);
                            countDown = ConfigParams.ScreenShotTime;
                            CountDownScreenShot.Content = "";
                            ShowText();
                            PhotoCountdownTimer.Start();
                        }
                    }
                    else
                    { //pause the reader to turn off bgremove
                        if (!multiFrameSourceReader.IsPaused)
                        {

                            turnOffBgRemove();
                        }

                        if (isRecording) //stop recording if is running
                        {
                            StopRecording();
                        }
                    }
                }
            }
            catch (IOException)
            {

            }

            try
            {
                using (FileStream fs1 = new FileStream(ConfigParams.repath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite))
                using (StreamReader sr = new StreamReader(fs1))
                {
                    string state = sr.ReadLine();
                    //bgremove + no emgu =1, chup anh 1=2, bgremove+emgu=3,chup anh 3 =4
                    if (state == "1")
                    {
                        if (!isRecording && !multiFrameSourceReader.IsPaused) //start record
                        {
                            string date = DateTime.Now.ToString(@"yyyy_MM_dd_hh_mm_ss");
                            recordProcess.StartInfo.FileName = ConfigParams.vlcpath + "vlc.exe";
                            recordProcess.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
                            recordProcess.StartInfo.Arguments = ConfigParams.vlcstr + "\"" + ConfigParams.VideoPath + date + ".mp4\",no-overwrite}} :sout-keep --high-priority --run-time=" + ConfigParams.RecordTime;
                            Trace.WriteLine(recordProcess.StartInfo.Arguments);
                            Trace.WriteLine(recordProcess.StartInfo.Arguments);
                            try
                            {
                                recordProcess.Start();
                            }
                            catch (Exception)
                            {

                            }

                            isRecording = true;
                        }
                    }

                    else if (isRecording)
                    {
                        StopRecording();
                    }

                }
            }
            catch (IOException)
            {

            }
        }
        void turnMultisourceReader(bool enable)
        {
            if (multiFrameSourceReader.IsPaused != enable)
            {
                multiFrameSourceReader.IsPaused = enable;
            }
        }
        void turnScreenShot(Visibility visibleState)
        {
            if (ScreenShot.Visibility != visibleState)
                ScreenShot.Visibility = visibleState;
        }
        void turnOffBgRemove()
        {
            turnMultisourceReader(true);
            turnScreenShot(Visibility.Hidden);
            Array.Clear(displayPixels, 0, displayPixels.Length);
            RenderColorPixels();
        }
        void StopRecording()
        {
            try
            {
                IntPtr h = recordProcess.MainWindowHandle;
                NativeMethods.SetForegroundWindow(h);
                System.Windows.Forms.SendKeys.SendWait("a");
                isRecording = false;
            }
            catch (Exception)
            {

            }
        }
        private void PhotoCountdownTimer_Tick(object sender, EventArgs e)
        {
            countDown--;
            CountDownScreenShot.Content = " CHỤP ẢNH TRONG: " + (countDown - 1); //thay doi noi dung text hien thi

            if ((countDown - 1) == 0)
            {
                //AsyncHideText(300);
                HideText();
            }
            if (countDown <= 0)
            {
                HideText(); //an chu huong dan
                TakeScreenShot(); //chup anh
                countDown = ConfigParams.ScreenShotTime;

                string state;
                using (FileStream fs = new FileStream(ConfigParams.bgpath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        state = sr.ReadLine();

                    }
                }
                using (FileStream fs = new FileStream(ConfigParams.bgpath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    //create empty text file



                    using (StreamWriter sw = new StreamWriter(fs))
                    {

                        if (state == "4")
                            sw.Write("3");
                        else if (state == "2")
                            sw.Write("1");
                    }




                }
                PhotoCountdownTimer.Stop();
            }
        }
        void TakeScreenShot()
        {
            Trace.WriteLine("Screen Shot");

            if (!Directory.Exists(ConfigParams.PhotoPreviewPath))
                Directory.CreateDirectory(ConfigParams.PhotoPreviewPath);
            string[] filenames = Directory.GetFiles(ConfigParams.PhotoPreviewPath);
            foreach (string file in filenames)
            {
                try
                {
                    Directory.Delete(file);
                }
                catch (Exception e) { Trace.WriteLine(e.ToString());}
            }

            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(CopyScreen()));

            string time = System.DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");

            bool personDetected = false;
            foreach (Body bd in UserManager.bodies)
            {
                if (bd != null)
                {
                    string cs = @"server=localhost;userid=" + ConfigParams.id + ";password=" + ConfigParams.pass + ";database=gds_client";
                    using (MySqlConnection conn = new MySqlConnection(cs))
                    {
                        try
                        {

                            conn.Open();
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine(ex.ToString());
                            return;
                        }
                        MySqlDataReader rdr = null;
                        try
                        {
                            string stm = "SELECT * FROM `detected` WHERE `ID`=" + bd.TrackingId;
                            MySqlCommand cmd = new MySqlCommand(stm, conn);
                            rdr = cmd.ExecuteReader();
                            while (rdr.Read())
                            {
                                Trace.WriteLine(rdr.GetName(0) + ": " + rdr.GetString(0));
                                string personName = rdr.GetString(0);
                                if (personName != null && personName != "unknown")
                                {
                                    string path = Path.Combine(ConfigParams.photoPath, personName + "/");
                                    if (!Directory.Exists(path))
                                        Directory.CreateDirectory(path);
                                    SaveScreen(path + time + ".png");
                                    personDetected = true;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine(ex.ToString());
                            return;
                        }
                    }
                }
            }
            if (!personDetected)
            {
                string path = ConfigParams.photoPath;
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                SaveScreen(path + time + ".png");
            }
            SaveScreen(ConfigParams.PhotoPreviewPath + "temp.png");
        }
        void SaveScreen(string path)
        {
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(CopyScreen()));
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    encoder.Save(fs);
                }
            }
            catch (Exception) { }

        }
        private BitmapSource CopyScreen()
        {
            using (var screenBmp = new Bitmap(
                (int)SystemParameters.PrimaryScreenWidth,
                (int)SystemParameters.PrimaryScreenHeight, System.Drawing.Imaging.PixelFormat.Format32bppPArgb
              ))
            {
                using (var bmpGraphics = Graphics.FromImage(screenBmp))
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

        private void Image_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!PhotoCountdownTimer.IsEnabled)
           {
                countDown = ConfigParams.ScreenShotTime;
                PhotoCountdownTimer.Start();
            }
        }
        //.............End................................................................................................

        /*..............................................................................
             * Author: Tran Xuan Duc - 21/12/2016
             * Xu Ly anh Emgu
            */
        Image<Bgra, Byte> smoothEdge(Image<Bgra, Byte> img)
        {

            src_gray.Bytes = bodyIndexFrameData;
            src_gray = src_gray.Not();

            src_gray = src_gray.SmoothMedian(5);

            imgRmBackground = img.Copy(src_gray);

            edge_detected = src_gray.Canny(1, 2);
            edge_detected = edge_detected.Dilate(1);
            //Mat verticalstructure = CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Rectangle, new System.Drawing.Size(1, 21),new System.Drawing.Point());
            //CvInvoke.Dilate(edge_detected, edge_detected, verticalstructure, new System.Drawing.Point(-1, -1), 1,Emgu.CV.CvEnum.BorderType.Default,new MCvScalar());
            blurred = imgRmBackground.Copy(src_gray);
            blurred = blurred.SmoothGaussian(3, 3, 0, 0);

            blurred.Copy(imgRmBackground, edge_detected);
            BitmapSource bmps = ToBitmapSource(imgRmBackground);
            if (imgRemoveBackground.Source != bmps)
                imgRemoveBackground.Source = bmps;

            return imgRmBackground;
        }
        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);
        public BitmapSource ToBitmapSource(IImage image)
        {
            using (System.Drawing.Bitmap source = image.Bitmap)
            {
                IntPtr ptr = source.GetHbitmap(); //obtain the Hbitmap

                BitmapSource bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    ptr,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

                DeleteObject(ptr); //release the HBitmap
                return bs;
            }
        }
        //***********End******************************************************************************************************
    }
}

