using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

using H.NotifyIcon;

using MahApps.Metro.Controls;

using Microsoft.Win32;

namespace SbslServiceManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private readonly DispatcherTimer _timer;

        private readonly string ServiceName = "SBSL ETL Service";
        private readonly string AppPath = "E:\\SBSL ETL Service";
        public MainWindow()
        {
            this.InitializeComponent();

            this.ServiceName = App.Configuration.GetSection("ServiceName").Value;
            this.AppPath = App.Configuration.GetSection("AppPath").Value;

            this._timer = new DispatcherTimer();

            this._timer.Interval = TimeSpan.FromSeconds(3);

            this._timer.Tick += this._timer_Tick;

            this._timer.Start();
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            (System.Windows.Media.Brush, string) mariadbStatus = CustomTaskBarIcon.CheckIfServiceIsRunning("MariaDB");
            (System.Windows.Media.Brush, string) sbslStatus = CustomTaskBarIcon.CheckIfServiceIsRunning();

            this.ServiceStatusText.Text = "ETL Service " + sbslStatus.Item2;
            this.ServiceStatusColour.Fill = sbslStatus.Item1;

            this.MariaDBStatusColour.Fill = mariadbStatus.Item1;
            this.MariaDBStatusText.Text = "MariaDB " + mariadbStatus.Item2;
        }



        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;

            Visibility = Visibility.Hidden;

            this.SbslTaskBarIcon.ShowStandardBalloon("Server Manager Window Closed", "You can still access the SBSL Server Manager from the system tray", TaskbarIcon.GetParentTaskbarIcon(this));
        }

        private void MetroWindow_Closed(object sender, EventArgs e)
        {
            this.SbslTaskBarIcon.Dispose();
        }

        private void SelectFiles_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                this.FilePath.Text = openFileDialog.FileName;
            }
        }

        private async void StartUpgrade_Click(object sender, RoutedEventArgs e)
        {

            if (string.IsNullOrEmpty(this.FilePath.Text) || !File.Exists(this.FilePath.Text))
            {
                this.ProgressMessage.Content = "Please select a valid file!";
                return;
            }

            ServiceController service = new ServiceController(this.ServiceName);

            int timeoutMilliseconds = 20000;

            try
            {
                int millisec1 = Environment.TickCount;
                TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);

                if (service.Status is ServiceControllerStatus.Running or ServiceControllerStatus.Paused)
                {
                    this.ProgressMessage.Content = $"Stopping Service {this.ServiceName}";
                    service.Stop();
                    service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
                }

                await this.ExtractFiles(this.FilePath.Text, this.AppPath);

                // count the rest of the timeout
                int millisec2 = Environment.TickCount;
                timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds - (millisec2 - millisec1));

                if (service.Status is ServiceControllerStatus.Paused or ServiceControllerStatus.Stopped)
                {
                    this.ProgressMessage.Content = $"Starting Service {this.ServiceName}";
                    service.Start();
                    service.WaitForStatus(ServiceControllerStatus.Running, timeout);
                }

                this.ProgressMessage.Content = $"Service {this.ServiceName} Updated Successfully";

                this.SbslTaskBarIcon.ShowStandardBalloon("Upgrade Successful!", "The new patch has been successfully applied", TaskbarIcon.GetParentTaskbarIcon(this));
            }
            catch (Exception ex)
            {
                this.ProgressMessage.Content = $"Service {this.ServiceName} failed to update Successfully {ex.Message}";

                this.SbslTaskBarIcon.ShowStandardBalloon("Upgrade Failed!", "The new patch has failed to apply", TaskbarIcon.GetParentTaskbarIcon(this));
            }


        }

        private async Task ExtractFiles(string zipFile, string outputPath)
        {
            if (!Directory.Exists(outputPath))
            {
                this.ProgressMessage.Content = "Please set a proper output directory!";
                this.SbslTaskBarIcon.ShowStandardBalloon("Upgrade Failed!", "The target directory does not exist", TaskbarIcon.GetParentTaskbarIcon(this));
                return;
            }

            using (FileStream zipToOpen = new FileStream(zipFile, FileMode.Open))
            using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Read))
            {
                int count = 0;
                int total = archive.Entries.Count;

                if (!archive.Entries.Any(e => e.FullName.Contains("SbslFileTransformer.exe")))
                {
                    this.ProgressMessage.Content = "Failed to find valid files in zip content!";
                    this.SbslTaskBarIcon.ShowStandardBalloon("Upgrade Failed!", "Upgrade file is not valid", TaskbarIcon.GetParentTaskbarIcon(this));
                    return;
                }

                this.StartUpgrade.IsEnabled = false;

                await Task.Run(() =>
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        count++;

                        Dispatcher.Invoke(() =>
                        {
                            this.ProgressMessage.Content = $"Extracting file: {entry.FullName}";

                            double current = count / (double)total;

                            this.UpgradeProgress.Value = current;
                        });

                        string destinationPath = Path.Combine(outputPath, entry.FullName);
                        _ = Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
                        entry.ExtractToFile(destinationPath, true);
                    }
                });

                this.StartUpgrade.IsEnabled = true;

            }
        }

        private async void RestartService_Click(object sender, RoutedEventArgs e)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                this.RestartService(this.ServiceName);//only etl service should be restarted
            });

            this.SbslTaskBarIcon.ShowStandardBalloon("Restart Service", "Service Restarted Successfully", TaskbarIcon.GetParentTaskbarIcon(this));
        }



        public void RestartService(string serviceName, int timeoutMilliseconds = 5000)
        {
            ServiceController service = new ServiceController(serviceName);
            try
            {
                int millisec1 = Environment.TickCount;
                TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);

                if (service.Status is ServiceControllerStatus.Running or ServiceControllerStatus.Paused)
                {
                    this.ProgressMessage.Content = $"Stopping Service {serviceName}";
                    service.Stop();
                    service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
                }

                // count the rest of the timeout
                int millisec2 = Environment.TickCount;
                timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds - (millisec2 - millisec1));

                if (service.Status is ServiceControllerStatus.Paused or ServiceControllerStatus.Stopped)
                {
                    this.ProgressMessage.Content = $"Starting Service {serviceName}";
                    service.Start();
                    service.WaitForStatus(ServiceControllerStatus.Running, timeout);
                }

                this.ProgressMessage.Content = $"Service {serviceName} Started Successfully";
            }
            catch
            {
                this.ProgressMessage.Content = $"Service {serviceName} failed to restart Successfully";
            }
        }

        private void OpenWebApp_Click(object sender, RoutedEventArgs e)
        {
            _ = Process.Start("cmd", "/c start http://localhost:5000");
        }
    }
}
