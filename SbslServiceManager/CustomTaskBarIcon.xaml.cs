using Hardcodet.Wpf.TaskbarNotification;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace SbslServiceManager
{
    /// <summary>
    /// Interaction logic for CustomTaskBarIcon.xaml
    /// </summary>
    public partial class CustomTaskBarIcon : UserControl
    {
        DispatcherTimer _timer;

        public CustomTaskBarIcon()
        {
            InitializeComponent();

            _timer = new DispatcherTimer();

            _timer.Interval = TimeSpan.FromSeconds(3);

            _timer.Tick += _timer_Tick;

            _timer.Start();
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            var mariadbStatus = CheckIfServiceIsRunning("MariaDB");
            var sbslStatus = CheckIfServiceIsRunning();

            ServiceStatusText.Text = "ETL Service " + sbslStatus.Item2;
            ServiceStatusColour.Fill =sbslStatus.Item1;

            MariaDBStatusColour.Fill = mariadbStatus.Item1;
            MariaDBStatusText.Text = "MariaDB " + mariadbStatus.Item2;
        }

        public void Dispose()
        {
            _timer.Stop();
        }

        public static (ServiceControllerStatus,string) GetServiceStatus(string serviceName = "SBSL ETL Service")
        {
            try
            {
                ServiceController sc = new ServiceController(serviceName);

                return (sc.Status, "");
            }
            catch(Exception ex)
            {
                return (ServiceControllerStatus.Stopped, $"The SBSL ETL Service may not be installed in this machine {ex.Message}");
            }
        }

        public static (Brush, string) CheckIfServiceIsRunning(string serviceName = "SBSL ETL Service")
        {
            var scResult = GetServiceStatus(serviceName);

            var status = scResult.Item1;


            string text = scResult.Item2;

            if (string.IsNullOrEmpty(text))
            {
                text = "Pending";
            }

            Brush color = new SolidColorBrush(GetFromArgb(System.Drawing.Color.Yellow));

            if (status == ServiceControllerStatus.Running)
            {
                color = new SolidColorBrush(GetFromArgb(System.Drawing.Color.Green));
                if (string.IsNullOrEmpty(scResult.Item2))
                {
                    text = "Running";
                }
            }
            if (status == ServiceControllerStatus.ContinuePending)
            {
                color = new SolidColorBrush(GetFromArgb(System.Drawing.Color.Yellow));
            }
            if (status == ServiceControllerStatus.StartPending)
            {
                color = new SolidColorBrush(GetFromArgb(System.Drawing.Color.Yellow));
            }
            if (status == ServiceControllerStatus.PausePending)
            {
                color = new SolidColorBrush(GetFromArgb(System.Drawing.Color.Yellow));
            }
            if (status == ServiceControllerStatus.StopPending)
            {
                color = new SolidColorBrush(GetFromArgb(System.Drawing.Color.Yellow));
            }
            if (status == ServiceControllerStatus.Paused)
            {
                color = new SolidColorBrush(GetFromArgb(System.Drawing.Color.Yellow));
            }
            if (status == ServiceControllerStatus.Stopped)
            {
                color = new SolidColorBrush(GetFromArgb(System.Drawing.Color.Red));
                if (string.IsNullOrEmpty(scResult.Item2))
                {
                    text = "Stopped";
                }

            }

            return (color, text);
        }

        public static Color GetFromArgb(System.Drawing.Color color)
        {
            byte AVal = color.A;
            byte RVal = color.R;
            byte GVal = color.G;
            byte BVal = color.B;

            return Color.FromArgb(AVal, RVal, GVal, BVal);
        }

        public void ShowStandardBalloon(string title, string text, BalloonIcon balloon)
        {
            //show balloon with custom icon
            taskBarNotify.ShowBalloonTip(title, text, balloon);

            //hide balloon
            taskBarNotify.HideBalloonTip();
        }

        private async void RestartService_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() => Process.Start(Application.ResourceAssembly.Location));
            Application.Current.Shutdown();
        }

        private void OpenWebApp_Click(object sender, RoutedEventArgs e)
        {
            //if service is not running show balloon error

            Process.Start("cmd", "/c start http://localhost:5000");
        }

        private void ShowMainWindow_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Show();
        }

        private void ExitManager_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
