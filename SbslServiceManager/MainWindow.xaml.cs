using H.NotifyIcon;
using Ionic.Zip;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace SbslServiceManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        DispatcherTimer _timer;

        string ServiceName = "SBSL ETL Service";
        string AppPath = "E:\\SBSL ETL Service";
        public MainWindow()
        {
            InitializeComponent();

            ServiceName = App.Configuration.GetSection("ServiceName").Value;
            AppPath = App.Configuration.GetSection("AppPath").Value;

            _timer = new DispatcherTimer();

            _timer.Interval = TimeSpan.FromSeconds(3);

            _timer.Tick += _timer_Tick;

            _timer.Start();
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            var mariadbStatus = CustomTaskBarIcon.CheckIfServiceIsRunning("MariaDB");
            var sbslStatus = CustomTaskBarIcon.CheckIfServiceIsRunning();

            ServiceStatusText.Text = "ETL Service " + sbslStatus.Item2;
            ServiceStatusColour.Fill = sbslStatus.Item1;

            MariaDBStatusColour.Fill = mariadbStatus.Item1;
            MariaDBStatusText.Text = "MariaDB " + mariadbStatus.Item2;
        }



        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;

            Visibility = Visibility.Hidden;

            SbslTaskBarIcon.ShowStandardBalloon("Server Manager Window Closed", "You can still access the SBSL Server Manager from the system tray", TaskbarIcon.GetParentTaskbarIcon(this));
        }

        private void MetroWindow_Closed(object sender, EventArgs e)
        {
            SbslTaskBarIcon.Dispose();
        }

        private void SelectFiles_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
                FilePath.Text = openFileDialog.FileName;
        }

        private async void StartUpgrade_Click(object sender, RoutedEventArgs e)
        {

            if (string.IsNullOrEmpty(FilePath.Text) || !File.Exists(FilePath.Text))
            {
                ProgressMessage.Content = "Please select a valid file!";
                return;
            }

            ServiceController service = new ServiceController(ServiceName);

            int timeoutMilliseconds = 20000;

            try
            {
                int millisec1 = Environment.TickCount;
                TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);

                if (service.Status == ServiceControllerStatus.Running || service.Status == ServiceControllerStatus.Paused)
                {
                    ProgressMessage.Content = $"Stopping Service {ServiceName}";
                    service.Stop();
                    service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
                }

                await ExtractFiles(FilePath.Text, AppPath);

                // count the rest of the timeout
                int millisec2 = Environment.TickCount;
                timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds - (millisec2 - millisec1));

                if (service.Status == ServiceControllerStatus.Paused || service.Status == ServiceControllerStatus.Stopped)
                {
                    ProgressMessage.Content = $"Starting Service {ServiceName}";
                    service.Start();
                    service.WaitForStatus(ServiceControllerStatus.Running, timeout);
                }

                ProgressMessage.Content = $"Service {ServiceName} Updated Successfully";

                SbslTaskBarIcon.ShowStandardBalloon("Upgrade Successful!", "The new patch has been successfully applied", TaskbarIcon.GetParentTaskbarIcon(this));
            }
            catch (Exception ex)
            {
                ProgressMessage.Content = $"Service {ServiceName} failed to update Successfully {ex.Message}";

                SbslTaskBarIcon.ShowStandardBalloon("Upgrade Failed!", "The new patch has failed to apply", TaskbarIcon.GetParentTaskbarIcon(this));
            }


        }

        private async Task ExtractFiles(string zipFile, string outputPath)
        {
            if (!Directory.Exists(outputPath))
            {
                ProgressMessage.Content = "Please set a proper output directory!";
                SbslTaskBarIcon.ShowStandardBalloon("Upgrade Failed!", "The target directory does not exist", TaskbarIcon.GetParentTaskbarIcon(this));
                return;
            }

            using (ZipFile zip = ZipFile.Read(zipFile))
            {
                int count = 0;
                int total = zip.Count();

                if (!zip.Any(e => e.FileName.Contains("SbslFileTransformer.exe")))
                {
                    ProgressMessage.Content = "Failed to find valid files in zip content!";
                    SbslTaskBarIcon.ShowStandardBalloon("Upgrade Failed!", "Upgrade file is not valid", TaskbarIcon.GetParentTaskbarIcon(this));
                    return;
                }

                StartUpgrade.IsEnabled = false;

                await Task.Run(() =>
                {
                    foreach (ZipEntry e in zip)
                    {
                        count++;

                        Dispatcher.Invoke(() =>
                        {
                            ProgressMessage.Content = $"Extracting file: {e.FileName}";

                            double current = count / (double)total;

                            UpgradeProgress.Value = current;
                        });

                        e.Extract(outputPath, ExtractExistingFileAction.OverwriteSilently);
                    }
                });

                StartUpgrade.IsEnabled = true;

            }
        }

        private async void RestartService_Click(object sender, RoutedEventArgs e)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                RestartService(ServiceName);//only etl service should be restarted
    });

            SbslTaskBarIcon.ShowStandardBalloon("Restart Service", "Service Restarted Successfully", TaskbarIcon.GetParentTaskbarIcon(this));
        }



        public void RestartService(string serviceName, int timeoutMilliseconds = 5000)
        {
            ServiceController service = new ServiceController(serviceName);
            try
            {
                int millisec1 = Environment.TickCount;
                TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);

                if (service.Status == ServiceControllerStatus.Running || service.Status == ServiceControllerStatus.Paused)
                {
                    ProgressMessage.Content = $"Stopping Service {serviceName}";
                    service.Stop();
                    service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
                }

                // count the rest of the timeout
                int millisec2 = Environment.TickCount;
                timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds - (millisec2 - millisec1));

                if (service.Status == ServiceControllerStatus.Paused || service.Status == ServiceControllerStatus.Stopped)
                {
                    ProgressMessage.Content = $"Starting Service {serviceName}";
                    service.Start();
                    service.WaitForStatus(ServiceControllerStatus.Running, timeout);
                }

                ProgressMessage.Content = $"Service {serviceName} Started Successfully";
            }
            catch
            {
                ProgressMessage.Content = $"Service {serviceName} failed to restart Successfully";
            }
        }

        private void OpenWebApp_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("cmd", "/c start http://localhost:5000");
        }
    }
}
