using System.Windows.Threading;
using System.Threading.Tasks;
using System;

namespace HD2
{
    static class ActivateThis //dua cua so chuong trinh len top, hien tai ko dung
    {
        static MainWindow app=(MainWindow)App.Current.MainWindow;
        public static bool isHolding = false;
        public static void Activatethis()
        {
            if (!isHolding)
            {
                App.Current.Dispatcher.BeginInvoke(new Action(delegate
                {
                    app.Activate();
                }), System.Windows.Threading.DispatcherPriority.ContextIdle, null);
            }
        } 
    }
}
