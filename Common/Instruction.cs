using System;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;
using System.IO;
using System.Diagnostics;
using System.Windows.Media.Animation;
using WpfAnimatedGif;
using System.Media;
namespace HD2
{
    static class StateInstruction  //class de hien instructions va bat am thanh huong dan tuong tac
    {

        static MainWindow main = (MainWindow)App.Current.MainWindow;
        public static void writeHumanDetect(bool m_bHDT)
        {

            BitmapImage srcH1G0I0 = new BitmapImage();

            srcH1G0I0.BeginInit();
            srcH1G0I0.UriSource = new Uri(AssetSource.imgH1G0I0, UriKind.Relative);
            srcH1G0I0.CacheOption = BitmapCacheOption.OnLoad;
            srcH1G0I0.EndInit();
            BitmapImage srcH1G1I1 = new BitmapImage();
            srcH1G1I1.BeginInit();
            srcH1G1I1.UriSource = new Uri(AssetSource.imgH1G1I1, UriKind.Relative);
            srcH1G1I1.CacheOption = BitmapCacheOption.OnLoad;
            srcH1G1I1.EndInit();
            BitmapImage srcdrag = new BitmapImage();
            srcdrag.BeginInit();
            srcdrag.UriSource = new Uri(AssetSource.drag, UriKind.Relative);
            srcdrag.CacheOption = BitmapCacheOption.OnLoad;
            srcdrag.EndInit();
            FileStream fs = null;
            StreamWriter hDT = null;
            try
            {
                fs = new FileStream("Log\\HDT_" + DateTime.Now.ToString("yyyyMMdd") + ".txt", FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);

                hDT = new StreamWriter(fs);

                if (m_bHDT == true)
                {
                    hDT.WriteLine(DateTime.Now.ToString("hh:mm:ss") + ",HDT,human_detect,yes,Found human");
                    ImageBehavior.SetAnimatedSource(main.pictureText, srcH1G0I0);
                }
                else
                {
                    hDT.WriteLine(DateTime.Now.ToString("hh:mm:ss") + ",HDT,human_detect,no,Not found any human");
                    main.pictureText.Source = null;
                    //Write("window", "HIDE", false); //Ghi file de an cua so chuong trinh AirServer Tuanld added 19/6/2014
                }
                if (hDT != null)
                    hDT.Dispose();
                if (fs != null)
                    fs.Dispose();
            }
            catch (Exception e) { Trace.WriteLine(e.ToString()); }
        }
        public static void PlaySound(string filePath)
        {
            using (SoundPlayer Sound = new SoundPlayer(filePath))
            {
                Sound.Play();

            }

        }

        public static void writeInteractiveState(bool m_bHMI)
        {
            BitmapImage srcH1G0I0 = new BitmapImage();
            srcH1G0I0.BeginInit();
            srcH1G0I0.UriSource = new Uri(AssetSource.imgH1G0I0, UriKind.Relative);
            srcH1G0I0.CacheOption = BitmapCacheOption.OnLoad;
            srcH1G0I0.EndInit();
            BitmapImage srcH1G1I1 = new BitmapImage();
            srcH1G1I1.BeginInit();
            srcH1G1I1.UriSource = new Uri(AssetSource.imgH1G1I1, UriKind.Relative);
            srcH1G1I1.CacheOption = BitmapCacheOption.OnLoad;
            srcH1G1I1.EndInit();
            BitmapImage srcdrag = new BitmapImage();
            srcdrag.BeginInit();
            srcdrag.UriSource = new Uri(AssetSource.drag, UriKind.Relative);
            srcdrag.CacheOption = BitmapCacheOption.OnLoad;
            srcdrag.EndInit();

            FileStream fs = null;
            StreamWriter wHI = null;
            try
            {
                fs = new FileStream("Log\\HMI_" + DateTime.Now.ToString("yyyyMMdd") + ".txt", FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
                wHI = new StreamWriter(fs);

                if (m_bHMI == true)
                {
                    wHI.WriteLine(DateTime.Now.ToString("hh:mm:ss") + ",HMI,checkInteractive,yes,Start interative");
                    ImageBehavior.SetAnimatedSource(main.pictureText, srcH1G1I1);
                }
                else
                {
                    wHI.WriteLine(DateTime.Now.ToString("hh:mm:ss") + ",HMI,checkInteractive,no,Stop interative");
                    ImageBehavior.SetAnimatedSource(main.pictureText, srcH1G0I0);
                }
                if (wHI != null)
                {
                    wHI.Dispose();
                }
                if (fs != null)
                    fs.Dispose();
            }
            catch (Exception e) { Trace.WriteLine(e.ToString()); }
        }
    }
}