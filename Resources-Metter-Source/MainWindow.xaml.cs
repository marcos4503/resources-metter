using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LibreHardwareMonitor.Hardware;

namespace Resources_Metter
{
    /// <summary>
    /// Interação lógica para MainWindow.xam
    /// </summary>
    public partial class MainWindow : Window
    {
        //System methods

        public MainWindow()
        {
            InitializeComponent();
            OnStart();
        }

        //Window variables
        NotifyIcon notifyIcon = new NotifyIcon();
        Window thisParentWindow = null;
        bool isThisOverlayVisible = true;
        Computer thisMonitoredComputer = null;

        //Core methods

        public void OnStart()
        {
            //Set the interface as loading mode, before load all data to be shown
            SetInterfaceOnLoadingMode();

            //Try to load all preferences of user
            ApplicationPreferences applicationPreferences = new ApplicationPreferences();
            applicationPreferences.LoadPreferences();

            //Set the position of window
            SetWindowPosition(applicationPreferences.overlayPosition, applicationPreferences.screenToShow, applicationPreferences.overlayPositionX, applicationPreferences.overlayPositionY, true);
            //Set the metrics style
            SetMetricsStyle(applicationPreferences.metricsStyle);
            //Set the overlay theme
            SetOverlayTheme(applicationPreferences.overlayTheme);
            //Set the overlay opacity
            SetOverlayOpacity(applicationPreferences.overlayOpacity);
            //Set the text formatation
            SetTextFormatation(applicationPreferences.enableBoldText);
            //Register/Unregister on boot
            SetRegisterInStartup(applicationPreferences.startOnBoot);
            //Set top most enabled or not
            this.Topmost = applicationPreferences.enableTopMost;

            //Prepare the notify icon (Requires System.Drawing and System.Windows.Forms references on project)
            this.notifyIcon.Visible = true;
            this.notifyIcon.Text = "Resources Monitor\nUpdating In " + applicationPreferences.metricsUpdateInterval + "ms";
            this.notifyIcon.MouseClick += NotifyIcon_Click;
            this.notifyIcon.ContextMenuStrip = new ContextMenuStrip();
            this.notifyIcon.ContextMenuStrip.Items.Add("Reload Position", null, this.NotifyIcon_Reload);
            this.notifyIcon.ContextMenuStrip.Items.Add("Show Overlay", null, this.NotifyIcon_Show);
            this.notifyIcon.ContextMenuStrip.Items.Add("Hide Overlay", null, this.NotifyIcon_Hide);
            this.notifyIcon.ContextMenuStrip.Items.Add("Settings", null, this.NotifyIcon_Settings);
            this.notifyIcon.ContextMenuStrip.Items.Add("About", null, this.NotifyIcon_About);
            this.notifyIcon.ContextMenuStrip.Items.Add("Quit", null, this.NotifyIcon_Quit);
            this.notifyIcon.Icon = new Icon(@"../../resources/icon-tray-on.ico");

            //Create a parent window (hided) and set this window as child of parent window, to hide from Alt + TAB
            thisParentWindow = new Window();
            thisParentWindow.Icon = this.Icon;
            thisParentWindow.Top = -100;
            thisParentWindow.Left = -100;
            thisParentWindow.Width = 1;
            thisParentWindow.Height = 1;
            thisParentWindow.WindowStyle = WindowStyle.ToolWindow;
            thisParentWindow.ShowInTaskbar = false;
            thisParentWindow.Show();
            this.Owner = thisParentWindow;
            thisParentWindow.Hide();

            //Start the updater of the metrics in the overlay
            StartMetricsUpdater(applicationPreferences.metricsUpdateInterval, applicationPreferences.networkInterfacePreferred,
                                applicationPreferences.networkUnit, applicationPreferences.overheatWarningForCpu,
                                applicationPreferences.overheatWarningForGpu);

            //Start the another program fullscreen detector, if auto-hide is enabled
            if(applicationPreferences.enableAutoHide == true)
            {
                List<string> programsToIgnoreOnAutoHide = new List<string>();
                foreach (string item in applicationPreferences.ignoreFromAutoHide)
                    programsToIgnoreOnAutoHide.Add(item);
                StartFullscreenDetector(programsToIgnoreOnAutoHide);
            }

            //If the system clock is enabled, start the clock renderization
            StartClockRenderization(applicationPreferences.enableClock);

            //Start the window restorer for secondary monitor if is desired
            if (applicationPreferences.enableWindowRestorerForSecMonitor == true)
                StartWindowRestorerForSecondaryMonitor();
        }

        public void SetInterfaceOnLoadingMode()
        {
            //Set interface as load mode
            download.Text = "-";
            upload.Text = "-";
            cpuUsagePercent.Text = "-";
            cpuTemperature.Text = "-";
            gpuUsagePercent.Text = "-";
            gpuTemperature.Text = "-";
            ramUsagePercent.Text = "-";
            hddUsagePercent.Text = "-";
            cpuUsage.Value = 0.0f;
            gpuUsage.Value = 0.0f;
            ramUsage.Value = 0.0f;
            hddUsage.Value = 0.0f;
            clockUsage.Text = "--:--";
        }

        public void SetWindowPositionCalculations(int overlayPosition, int screenToShow, int customPositionX, int customPositionY)
        {
            //This method will only make calcs of positon of the overlay, and put the overlay in correct position

            //If is TopLeft
            if (overlayPosition == 0)
            {
                if (screenToShow == 0) //Main Monitor
                {
                    this.Left = 1;
                    this.Top = 1;
                }
                if (screenToShow == 1) //Secondary Monitor
                {
                    this.Left = Screen.PrimaryScreen.Bounds.Width + 1;
                    this.Top = 1;
                }
            }
            //If is TopMiddle
            if (overlayPosition == 1)
            {
                if (screenToShow == 0) //Main Monitor
                {
                    this.Left = (Screen.PrimaryScreen.Bounds.Width / 2) - (this.Width / 2);
                    this.Top = 1;
                }
                if (screenToShow == 1) //Secondary Monitor
                {
                    this.Left = (Screen.PrimaryScreen.Bounds.Width + (Screen.AllScreens[1].Bounds.Width / 2)) - (this.Width / 2);
                    this.Top = 1;
                }
            }
            //If is BottomLeft
            if (overlayPosition == 2)
            {
                if (screenToShow == 0) //Main Monitor
                {
                    this.Left = 1;
                    this.Top = Screen.PrimaryScreen.WorkingArea.Height - this.Height - 1;
                }
                if (screenToShow == 1) //Secondary Monitor
                {
                    this.Left = Screen.PrimaryScreen.Bounds.Width + 1;
                    this.Top = Screen.AllScreens[1].Bounds.Height - this.Height - 1;
                }
            }
            //If is BottomMiddle
            if (overlayPosition == 3)
            {
                if (screenToShow == 0) //Main Monitor
                {
                    this.Left = (Screen.PrimaryScreen.Bounds.Width / 2) - (this.Width / 2);
                    this.Top = Screen.PrimaryScreen.WorkingArea.Height - this.Height - 1;
                }
                if (screenToShow == 1) //Secondary Monitor
                {
                    this.Left = (Screen.PrimaryScreen.Bounds.Width + (Screen.AllScreens[1].Bounds.Width / 2)) - (this.Width / 2);
                    this.Top = Screen.AllScreens[1].Bounds.Height - this.Height - 1;
                }
            }
            //If is BottomRight
            if (overlayPosition == 4)
            {
                if (screenToShow == 0) //Main Monitor
                {
                    this.Left = Screen.PrimaryScreen.Bounds.Width - this.Width - 1;
                    this.Top = Screen.PrimaryScreen.WorkingArea.Height - this.Height - 1;
                }
                if (screenToShow == 1) //Secondary Monitor
                {
                    this.Left = (Screen.PrimaryScreen.Bounds.Width + Screen.AllScreens[1].Bounds.Width) - this.Width - 1;
                    this.Top = Screen.AllScreens[1].Bounds.Height - this.Height - 1;
                }
            }

            //If have a custom position, set this
            if(customPositionX > 0 && customPositionY > 0)
            {
                this.Left = customPositionX;
                this.Top = customPositionY;
            }
        }

        public void SetWindowPosition(int overlayPosition, int screenToShow, int customPositionX, int customPositionY, bool isOnStart)
        {
            //Change the position of the overlay window, according to preferences
            this.WindowStartupLocation = WindowStartupLocation.Manual;
            //If the interface is starting
            if (isOnStart == true)
            {
                this.Loaded += (s, a) =>
                {
                    SetWindowPositionCalculations(overlayPosition, screenToShow, customPositionX, customPositionY);
                };
            }
            //If the interface is already started
            if (isOnStart == false)
            {
                SetWindowPositionCalculations(overlayPosition, screenToShow, customPositionX, customPositionY);
            }
        }

        public void SetMetricsStyle(int metricsStyle)
        {
            //If is desired percent
            if(metricsStyle == 0)
            {
                cpuUsage.Visibility = Visibility.Collapsed;
                cpuUsagePercent.Visibility = Visibility.Visible;
                cpuTemperature.Margin = new Thickness(40, cpuTemperature.Margin.Top, cpuTemperature.Margin.Right, cpuTemperature.Margin.Bottom);
                gpuUsage.Visibility = Visibility.Collapsed;
                gpuUsagePercent.Visibility = Visibility.Visible;
                gpuTemperature.Margin = new Thickness(92, cpuTemperature.Margin.Top, cpuTemperature.Margin.Right, cpuTemperature.Margin.Bottom);
                ramUsage.Visibility = Visibility.Collapsed;
                ramUsagePercent.Visibility = Visibility.Visible;
                hddUsage.Visibility = Visibility.Collapsed;
                hddUsagePercent.Visibility = Visibility.Visible;
            }
            //If is desired bars
            if (metricsStyle == 1)
            {
                cpuUsage.Visibility = Visibility.Visible;
                cpuUsagePercent.Visibility = Visibility.Collapsed;
                cpuTemperature.Margin = new Thickness(32, cpuTemperature.Margin.Top, cpuTemperature.Margin.Right, cpuTemperature.Margin.Bottom);
                gpuUsage.Visibility = Visibility.Visible;
                gpuUsagePercent.Visibility = Visibility.Collapsed;
                gpuTemperature.Margin = new Thickness(84, cpuTemperature.Margin.Top, cpuTemperature.Margin.Right, cpuTemperature.Margin.Bottom);
                ramUsage.Visibility = Visibility.Visible;
                ramUsagePercent.Visibility = Visibility.Collapsed;
                hddUsage.Visibility = Visibility.Visible;
                hddUsagePercent.Visibility = Visibility.Collapsed;
            }
        }

        public void SetOverlayTheme(int overlayTheme)
        {
            //If is desired light theme
            if(overlayTheme == 0)
            {
                overlayBackground.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 255, 255));
                download.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 0, 0, 0));
                upload.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 0, 0, 0));
                cpuTemperature.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 0, 0, 0));
                gpuTemperature.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 0, 0, 0));
                cpuUsagePercent.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 0, 0, 0));
                gpuUsagePercent.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 0, 0, 0));
                ramUsagePercent.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 0, 0, 0));
                hddUsagePercent.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 0, 0, 0));
                cpuIcon.Source = new BitmapImage(new Uri(@"/resources/cpu-icon-black.png", UriKind.Relative));
                gpuIcon.Source = new BitmapImage(new Uri(@"/resources/gpu-icon-black.png", UriKind.Relative));
                ramIcon.Source = new BitmapImage(new Uri(@"/resources/ram-icon-black.png", UriKind.Relative));
                hddIcon.Source = new BitmapImage(new Uri(@"/resources/hdd-icon-black.png", UriKind.Relative));
                downloadIcon.Source = new BitmapImage(new Uri(@"/resources/arrow-icon-black.png", UriKind.Relative));
                uploadIcon.Source = new BitmapImage(new Uri(@"/resources/arrow-icon-black.png", UriKind.Relative));
                clockIcon.Source = new BitmapImage(new Uri(@"/resources/clock-icon-black.png", UriKind.Relative));
                cpuUsage.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 78, 78, 78));
                cpuUsage.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 0, 158, 17));
                gpuUsage.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 78, 78, 78));
                gpuUsage.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 0, 158, 17));
                ramUsage.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 78, 78, 78));
                ramUsage.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 0, 158, 17));
                hddUsage.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 78, 78, 78));
                hddUsage.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 0, 158, 17));
                clockUsage.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 0, 0, 0));
            }
            //If is desired dark theme
            if (overlayTheme == 1)
            {
                overlayBackground.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 0, 0, 0));
                download.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 255, 255));
                upload.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 255, 255));
                cpuTemperature.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 255, 255));
                gpuTemperature.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 255, 255));
                cpuUsagePercent.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 255, 255));
                gpuUsagePercent.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 255, 255));
                ramUsagePercent.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 255, 255));
                hddUsagePercent.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 255, 255));
                cpuIcon.Source = new BitmapImage(new Uri(@"/resources/cpu-icon-white.png", UriKind.Relative));
                gpuIcon.Source = new BitmapImage(new Uri(@"/resources/gpu-icon-white.png", UriKind.Relative));
                ramIcon.Source = new BitmapImage(new Uri(@"/resources/ram-icon-white.png", UriKind.Relative));
                hddIcon.Source = new BitmapImage(new Uri(@"/resources/hdd-icon-white.png", UriKind.Relative));
                downloadIcon.Source = new BitmapImage(new Uri(@"/resources/arrow-icon-white.png", UriKind.Relative));
                uploadIcon.Source = new BitmapImage(new Uri(@"/resources/arrow-icon-white.png", UriKind.Relative));
                clockIcon.Source = new BitmapImage(new Uri(@"/resources/clock-icon-white.png", UriKind.Relative));
                cpuUsage.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 230, 230, 230));
                cpuUsage.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 6, 176, 37));
                gpuUsage.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 230, 230, 230));
                gpuUsage.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 6, 176, 37));
                ramUsage.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 230, 230, 230));
                ramUsage.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 6, 176, 37));
                hddUsage.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 230, 230, 230));
                hddUsage.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 6, 176, 37));
                clockUsage.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 255, 255));
            }
        }

        public void SetOverlayOpacity(int overlayOpacity)
        {
            //Set the overlay opacity
            this.Opacity = (float)overlayOpacity / 100.0f;
        }

        public void SetTextFormatation(bool enableBoldText)
        {
            //If bold text is enabled
            if(enableBoldText == true)
            {
                download.FontWeight = FontWeights.Bold;
                download.Width = 16;
                download.FontSize = 9;
                download.FontFamily = new System.Windows.Media.FontFamily("Lato Black");

                upload.FontWeight = FontWeights.Bold;
                upload.Width = 16;
                upload.FontSize = 9;
                upload.FontFamily = new System.Windows.Media.FontFamily("Lato Black");

                cpuUsagePercent.FontWeight = FontWeights.Bold;

                cpuTemperature.FontWeight = FontWeights.Bold;
                cpuTemperature.Width = 18;
                cpuTemperature.FontSize = 10;
                cpuTemperature.Margin = new Thickness(cpuTemperature.Margin.Left, cpuTemperature.Margin.Top, cpuTemperature.Margin.Right, 1);
                cpuTemperature.FontFamily = new System.Windows.Media.FontFamily("Lato Black");

                gpuUsagePercent.FontWeight = FontWeights.Bold;

                gpuTemperature.FontWeight = FontWeights.Bold;
                gpuTemperature.Width = 18;
                gpuTemperature.FontSize = 10;
                gpuTemperature.Margin = new Thickness(gpuTemperature.Margin.Left, 4, gpuTemperature.Margin.Right, 6);
                gpuTemperature.FontFamily = new System.Windows.Media.FontFamily("Lato Black");

                ramUsagePercent.FontWeight = FontWeights.Bold;

                hddUsagePercent.FontWeight = FontWeights.Bold;

                clockUsage.FontWeight = FontWeights.Bold;
                clockUsage.Width = 26;
                clockUsage.FontSize = 10;
                clockUsage.Margin = new Thickness(258, 4, clockUsage.Margin.Right, 6);
                clockUsage.FontFamily = new System.Windows.Media.FontFamily("Lato Black");
                clockIcon.Margin = new Thickness(245, 4, 0, 4);
            }
            //If bold text is disabled
            if (enableBoldText == false)
            {
                download.FontWeight = FontWeights.Normal;
                download.Width = 16;
                download.FontSize = 8;
                download.FontFamily = new System.Windows.Media.FontFamily("Segoe UI");

                upload.FontWeight = FontWeights.Normal;
                upload.Width = 16;
                upload.FontSize = 8;
                upload.FontFamily = new System.Windows.Media.FontFamily("Segoe UI");

                cpuUsagePercent.FontWeight = FontWeights.Normal;

                cpuTemperature.FontWeight = FontWeights.Normal;
                cpuTemperature.Width = 13;
                cpuTemperature.FontSize = 8;
                cpuTemperature.Margin = new Thickness(cpuTemperature.Margin.Left, cpuTemperature.Margin.Top, cpuTemperature.Margin.Right, 0);
                cpuTemperature.FontFamily = new System.Windows.Media.FontFamily("Segoe UI");

                gpuUsagePercent.FontWeight = FontWeights.Normal;

                gpuTemperature.FontWeight = FontWeights.Normal;
                gpuTemperature.Width = 13;
                gpuTemperature.FontSize = 8;
                gpuTemperature.Margin = new Thickness(gpuTemperature.Margin.Left, 4, gpuTemperature.Margin.Right, 4);
                gpuTemperature.FontFamily = new System.Windows.Media.FontFamily("Segoe UI");

                ramUsagePercent.FontWeight = FontWeights.Normal;

                hddUsagePercent.FontWeight = FontWeights.Normal;

                clockUsage.FontWeight = FontWeights.Normal;
                clockUsage.Width = 24;
                clockUsage.FontSize = 8;
                clockUsage.Margin = new Thickness(262, 4, clockUsage.Margin.Right, 4);
                clockUsage.FontFamily = new System.Windows.Media.FontFamily("Segoe UI");
                clockIcon.Margin = new Thickness(250, 4, 0, 4);
            }
        }

        public void SetRegisterInStartup(bool startOnBoot)
        {
            //Register or unregister this app from boot
            StartupManager startupManager = new StartupManager("Resources Metter");
            bool success = startupManager.RegisterToStartOnBoot(startOnBoot);
            if(success == false)
                System.Windows.MessageBox.Show("Unable to apply your Boot settings correctly.", "Error");
        }

        //Updaters methods

        public void StartFullscreenDetector(List<string> programsToIgnoreOnAutoHide)
        {
            //Each 500ms check if have another program in fullscreen and hide the overlay
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer { Interval = 500 };
            timer.Enabled = true;
            timer.Tick += new EventHandler((object sender, EventArgs e) =>
            {
                //Only works if the overlay is defined to be show
                if (isThisOverlayVisible == true)
                {
                    //Get the title of current program in foreground
                    string currentProgramOnForeground = GetActiveWindowTitle();

                    //If the program not have a empty title, continue to the checks
                    if (String.IsNullOrEmpty(currentProgramOnForeground) == false && String.IsNullOrWhiteSpace(currentProgramOnForeground) == false)
                    {
                        //Check if the current foreground program is in the list of ignore in auto hide
                        bool currentProgramOnForegroundIsInTheListOfIgnoreInAutoHide = programsToIgnoreOnAutoHide.Contains(currentProgramOnForeground);

                        //If overlay is setted as visible and the current foreground program not is in the list of ignore in auto hide, continue
                        if (currentProgramOnForegroundIsInTheListOfIgnoreInAutoHide == false)
                        {
                            if (IsForegroundFullScreen() == true)          //<- If the current foreground app is in fullscreen, hide the overlay
                                this.Visibility = Visibility.Collapsed;
                            if (IsForegroundFullScreen() == false)         //<- If the current foreground app is not in fullscreen, show the overlay
                                this.Visibility = Visibility.Visible;
                        }
                        //If overlay is setted as visible, and the current foreground program is in the list of ignore in auto hide, force show overlay
                        if (currentProgramOnForegroundIsInTheListOfIgnoreInAutoHide == true)
                            this.Visibility = Visibility.Visible;
                    }
                    //If the program have a empty title, force to set as visible
                    if (String.IsNullOrEmpty(currentProgramOnForeground) == true || String.IsNullOrWhiteSpace(currentProgramOnForeground) == true)
                        this.Visibility = Visibility.Visible;
                } 
            });
        }

        public void StartMetricsUpdater(int updateInterval, int networkInstance, int networkUnit, int cpuOverheatWarn, int gpuOverheatWarn)
        {
            //Initialize the needed variables for metrics
            WindowOverheatWarn overheatWarningWindow = new WindowOverheatWarn();
            this.Loaded += (s, b) =>
            {
                float oldWindowWidth = (float)overheatWarningWindow.Width;
                float oldWindowHeight = (float)overheatWarningWindow.Height;
                overheatWarningWindow.Top = -100;
                overheatWarningWindow.Left = -100;
                overheatWarningWindow.Width = 1;
                overheatWarningWindow.Height = 1;
                overheatWarningWindow.Show();
                overheatWarningWindow.Owner = this;
                overheatWarningWindow.Visibility = Visibility.Collapsed;
                overheatWarningWindow.Width = oldWindowWidth;
                overheatWarningWindow.Height = oldWindowHeight;
                overheatWarningWindow.Left = (Screen.PrimaryScreen.Bounds.Width / 2) - (oldWindowWidth / 2);
                overheatWarningWindow.Top = Screen.PrimaryScreen.WorkingArea.Height - oldWindowHeight - 30;

            };
            //Initialize the LibreHardwareMonitor computer
            thisMonitoredComputer = new Computer { IsCpuEnabled = true, IsGpuEnabled = true };
            thisMonitoredComputer.Open();
            thisMonitoredComputer.Accept(new UpdateVisitor());
            //Cpu
            PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            float lastCpuNextValue = cpuCounter.NextValue();
            System.Windows.Media.Brush defaultCpuUsageBarColor = cpuUsage.Foreground;
            //GPU
            System.Windows.Media.Brush defaultGpuUsageBarColor = gpuUsage.Foreground;
            //RAM
            Microsoft.VisualBasic.Devices.ComputerInfo computerInfo = new Microsoft.VisualBasic.Devices.ComputerInfo();
            float totalRamMb = computerInfo.TotalPhysicalMemory / (1024 * 1024);
            PerformanceCounter ramCounter = new PerformanceCounter("Memory", "Available MBytes");
            System.Windows.Media.Brush defaultRamUsageBarColor = ramUsage.Foreground;
            //Disk
            PerformanceCounter diskCounter = new PerformanceCounter("PhysicalDisk", "% Disk Time", "_Total");
            System.Windows.Media.Brush defaultHddUsageBarColor = hddUsage.Foreground;
            //Network
            ImageSource defaultDownloadIconSource = downloadIcon.Source;
            ImageSource greenDownloadIconSource = new BitmapImage(new Uri(@"/resources/arrow-green-icon.png", UriKind.Relative));
            ImageSource defaultUploadIconSource = uploadIcon.Source;
            ImageSource redUploadIconSource = new BitmapImage(new Uri(@"/resources/arrow-red-icon.png", UriKind.Relative));
            PerformanceCounterCategory networkCounter;
            string instance = "";
            PerformanceCounter netSent = null;
            PerformanceCounter netReceive = null;
            if(networkInstance != -1)
            {
                networkCounter = new PerformanceCounterCategory("Network Interface");
                string[] instances = networkCounter.GetInstanceNames();
                if (instances.Length > networkInstance)
                    instance = instances[networkInstance];
                if (instances.Length <= networkInstance)
                    instance = instances[0];
                netSent = new PerformanceCounter("Network Interface", "Bytes Sent/sec", instance);
                netReceive = new PerformanceCounter("Network Interface", "Bytes Received/sec", instance);
            }
            if(networkInstance == -1) 
            {
                notifyIcon.BalloonTipText = "Please select a Network Interface to be monitored in Settings.";
                notifyIcon.ShowBalloonTip(3000);
            }

            //Create a timer with delay of 300ms to update the program
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer { Interval = updateInterval };
            timer.Enabled = true;
            timer.Tick += new EventHandler((object sender, EventArgs e) => {
                //Get information from LibreHardwareMonitor
                float cpuTemp = 0;
                float gpuEngineUsage = 0;
                float gpuTemp = 0;
                foreach(IHardware hardware in thisMonitoredComputer.Hardware)
                {
                    if(hardware.HardwareType == HardwareType.Cpu)
                    {
                        hardware.Update();
                        foreach (ISensor sensor in hardware.Sensors)
                            if (sensor.SensorType == SensorType.Temperature && sensor.Name == "Core Average")
                                if(sensor.Value != null)
                                    cpuTemp = (float)sensor.Value;
                    }
                    if (hardware.HardwareType == HardwareType.GpuNvidia)
                    {
                        hardware.Update();
                        foreach (ISensor sensor in hardware.Sensors)
                        {
                            if (sensor.SensorType == SensorType.Load && sensor.Name == "GPU Core")
                                if (sensor.Value != null)
                                    gpuEngineUsage = (float)sensor.Value;
                            if (sensor.SensorType == SensorType.Temperature && sensor.Name == "GPU Core")
                                if (sensor.Value != null)
                                    gpuTemp = (float)sensor.Value;
                        }
                    }
                    if (hardware.HardwareType == HardwareType.GpuAmd)
                    {
                        hardware.Update();
                        foreach (ISensor sensor in hardware.Sensors)
                        {
                            if (sensor.SensorType == SensorType.Load && sensor.Name == "GPU Core")
                                if (sensor.Value != null)
                                    gpuEngineUsage = (float)sensor.Value;
                            if (sensor.SensorType == SensorType.Temperature && sensor.Name == "GPU Core")
                                if (sensor.Value != null)
                                    gpuTemp = (float)sensor.Value;
                        }
                    }
                }

                //CPU
                float cpuUsageNextValue = cpuCounter.NextValue();
                float cpuUsageNextValueAverage = (cpuUsageNextValue + lastCpuNextValue) / 2.0f;
                cpuUsage.Value = cpuUsageNextValueAverage;
                cpuUsagePercent.Text = cpuUsageNextValueAverage.ToString("F0") + "%";
                cpuTemperature.Text = cpuTemp.ToString("F0") + "º";
                lastCpuNextValue = cpuUsageNextValue;
                if (cpuUsageNextValueAverage > 90)
                    cpuUsage.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 192, 0, 0));
                if (cpuUsageNextValueAverage <= 90)
                    cpuUsage.Foreground = defaultCpuUsageBarColor;
                //GPU
                float gpuUsageNextValue = gpuEngineUsage;
                gpuUsage.Value = gpuUsageNextValue;
                gpuUsagePercent.Text = gpuUsageNextValue.ToString("F0") + "%";
                gpuTemperature.Text = gpuTemp.ToString("F0") + "º";
                if (gpuUsageNextValue > 90)
                    gpuUsage.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 192, 0, 0));
                if (gpuUsageNextValue <= 90)
                    gpuUsage.Foreground = defaultGpuUsageBarColor;
                //RAM
                float ramInUse = totalRamMb - ramCounter.NextValue();
                float ramUsageNextValue = ramInUse / totalRamMb * 100.0f;
                ramUsage.Value = ramUsageNextValue;
                ramUsagePercent.Text = ramUsageNextValue.ToString("F0") + "%";
                if (ramUsageNextValue > 90)
                    ramUsage.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 192, 0, 0));
                if (ramUsageNextValue <= 90)
                    ramUsage.Foreground = defaultRamUsageBarColor;
                //Disk
                float diskUsageNextValue = diskCounter.NextValue();
                hddUsage.Value = diskUsageNextValue;
                hddUsagePercent.Text = diskUsageNextValue.ToString("F0") + "%";
                if (diskUsageNextValue > 90)
                    hddUsage.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 192, 0, 0));
                if (diskUsageNextValue <= 90)
                    hddUsage.Foreground = defaultHddUsageBarColor;

                //Network
                if (networkInstance != -1)
                {
                    float bytesReceived = netReceive.NextValue();
                    float bytesSended = netSent.NextValue();
                    if(networkUnit == 0) //<-- If is desired to show in kb/s
                    {
                        download.Text = (bytesReceived / (float)1024).ToString("F1");
                        upload.Text = (bytesSended / (float)1024).ToString("F1");
                    }
                    if (networkUnit == 1) //<-- If is desired to show in mb/s
                    {
                        download.Text = (bytesReceived / (float)1024 / (float)1024).ToString("F1");
                        upload.Text = (bytesSended / (float)1024/ (float)1024).ToString("F1");
                    }
                    //Paint the arrows of download and upload
                    if ((bytesReceived / (float)1024 / (float)1024) >= 1)
                        downloadIcon.Source = greenDownloadIconSource;
                    else
                        downloadIcon.Source = defaultDownloadIconSource;
                    if ((bytesSended / (float)1024 / (float)1024) >= 1)
                        uploadIcon.Source = redUploadIconSource;
                    else
                        uploadIcon.Source = defaultUploadIconSource;
                }
                if (networkInstance == -1)
                {
                    download.Text = "-";
                    upload.Text = "-";
                }

                //Warn if is having a overheat
                if(cpuTemp >= cpuOverheatWarn || gpuTemp >= gpuOverheatWarn)
                    if (overheatWarningWindow != null)
                        overheatWarningWindow.Visibility = Visibility.Visible;
                //Close the warn (if have) on normal temperature
                if (cpuTemp < cpuOverheatWarn && gpuTemp < gpuOverheatWarn)
                    if (overheatWarningWindow != null)
                        overheatWarningWindow.Visibility = Visibility.Collapsed;
            });
        }

        public void StartClockRenderization(bool enableClock)
        {
            //If the clock is disabled
            if(enableClock == false)
            {
                clockIcon.Visibility = Visibility.Collapsed;
                clockUsage.Visibility = Visibility.Collapsed;
                this.Width = 242;
            }
            //If the clock is enabled
            if (enableClock == true)
            {
                clockIcon.Visibility = Visibility.Visible;
                clockUsage.Visibility = Visibility.Visible;
                this.Width = 288;

                //Each 60000ms update the system clock in the overlay
                System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer { Interval = 5000 };
                timer.Enabled = true;
                timer.Tick += new EventHandler((object sender, EventArgs e) =>
                {
                    //Get current date time and update the clock
                    DateTime dateTime = DateTime.Now;
                    int hour = dateTime.Hour;
                    int minute = dateTime.Minute;
                    clockUsage.Text = ((hour > 9) ? hour.ToString() : "0"+hour) + ":" + ((minute > 9) ? minute.ToString() : "0" + minute);

                    //Set a interval of 60000ms
                    timer.Interval = 60000;
                    Console.WriteLine("Updated");
                });
            }
        }

        public void StartWindowRestorerForSecondaryMonitor()
        {
            //This method will get all running programs and restore all programs running on secondary monitor automatically
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer { Interval = 3000 };
            timer.Enabled = true;
            timer.Tick += new EventHandler((object sender, EventArgs e) =>
            {
                //List all current opened windows
                List<IntPtr> currentOpenedWindowsOnSecondaryMonitor = new List<IntPtr>();

                //Get all running processes with a interface
                Process[] processes = Process.GetProcesses();
                foreach (Process process in processes)
                    if (String.IsNullOrEmpty(process.MainWindowTitle) == false)
                    {
                        //Get all UWP windows opened in secondary monitor
                        if (process.ProcessName == "ApplicationFrameHost")
                            foreach (var handle in EnumerateProcessWindowHandles(process.Id))
                                if ((GetPlacement(handle).rcNormalPosition.Left + 8) > Screen.PrimaryScreen.Bounds.Width)
                                    currentOpenedWindowsOnSecondaryMonitor.Add(handle);

                        //Get all Win32 windows opened in secondary monitor
                        if (process.ProcessName != "ApplicationFrameHost")
                            if ((GetPlacement(process.MainWindowHandle).rcNormalPosition.Left + 8) > Screen.PrimaryScreen.Bounds.Width)
                                currentOpenedWindowsOnSecondaryMonitor.Add(process.MainWindowHandle);
                    }

                //Force to restore all windows on secondary monitor of list
                foreach (IntPtr windowHandle in currentOpenedWindowsOnSecondaryMonitor)
                    ShowWindow(windowHandle, SW_SHOWNOACTIVATE);
            });
        }

        //Notify icon methods

        private void NotifyIcon_Click(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            //If not is left click, skip
            if (e.Button != MouseButtons.Left)
                return;

            //Hide or show overlay on click in the tray icon
            if (isThisOverlayVisible == true)
            {
                NotifyIcon_Hide(sender, e);
                return;
            }
            if (isThisOverlayVisible == false)
            {
                NotifyIcon_Show(sender, e);
                return;
            }
        }

        private void NotifyIcon_Reload(object sender, EventArgs e)
        {
            //Reload the position saved of overlay
            ApplicationPreferences applicationPreferences = new ApplicationPreferences();
            applicationPreferences.LoadPreferences();
            //Apply the overlay position preference
            SetWindowPosition(applicationPreferences.overlayPosition, applicationPreferences.screenToShow, applicationPreferences.overlayPositionX, applicationPreferences.overlayPositionY, false);
        }

        private void NotifyIcon_Show(object sender, EventArgs e)
        {
            //Show the metter window
            this.Visibility = Visibility.Visible;
            isThisOverlayVisible = true;
            this.notifyIcon.Icon = new Icon(@"../../resources/icon-tray-on.ico");
        }

        private void NotifyIcon_Hide(object sender, EventArgs e)
        {
            //Show the metter window
            this.Visibility = Visibility.Collapsed;
            isThisOverlayVisible = false;
            this.notifyIcon.Icon = new Icon(@"../../resources/icon-tray-off.ico");
        }

        private void NotifyIcon_Settings(object sender, EventArgs e)
        {
            //Open the settings window
            WindowSettings windowSettings = new WindowSettings();
            windowSettings.Closed += (object s, EventArgs ea) => {
                //Wait 100 ms before continue, to wait the settings window save the file
                Thread.Sleep(100);

                //Load the preferences
                ApplicationPreferences applicationPreferences = new ApplicationPreferences();
                applicationPreferences.LoadPreferences();

                //Apply the overlay position preference
                SetWindowPosition(applicationPreferences.overlayPosition, applicationPreferences.screenToShow, applicationPreferences.overlayPositionX, applicationPreferences.overlayPositionY, false);
                //Apply the metrics style
                SetMetricsStyle(applicationPreferences.metricsStyle);
                //Apply the overlay theme
                SetOverlayTheme(applicationPreferences.overlayTheme);
                //Apply the overlay opacity
                SetOverlayOpacity(applicationPreferences.overlayOpacity);
                //Apply the register on boot
                SetRegisterInStartup(applicationPreferences.startOnBoot);
            };
            windowSettings.Show();
        }

        private void NotifyIcon_About(object sender, EventArgs e)
        {
            //Open the about window
            WindowAbout windowAbout = new WindowAbout();
            windowAbout.Show();
        }

        private void NotifyIcon_Quit(object sender, EventArgs e)
        {
            //Close this window and the parent window, to kill process completely
            thisMonitoredComputer.Close();
            Close();
            thisParentWindow.Close();
        }


        #region ForegroundOtherProgramsFullscrenDetector
        //Dependencies of DLLs
        
        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(HandleRef hWnd, [In, Out] ref RECT rect);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        //Detection of foreground app is in fullscreen

        public static bool IsForegroundFullScreen()
        {
            return IsForegroundFullScreen(null);
        }

        public static bool IsForegroundFullScreen(Screen screen)
        {
            if (screen == null)
            {
                screen = Screen.PrimaryScreen;
            }
            RECT rect = new RECT();
            GetWindowRect(new HandleRef(null, GetForegroundWindow()), ref rect);
            return new System.Drawing.Rectangle(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top).Contains(screen.Bounds);
        }

        //Detection of current app in foreground

        private string GetActiveWindowTitle()
        {
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);
            IntPtr handle = GetForegroundWindow();

            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return null;
        }
        #endregion

        #region LibreHardwareMonitorUpdateVisitor
        public class UpdateVisitor : IVisitor
        {
            public void VisitComputer(IComputer computer)
            {
                computer.Traverse(this);
            }
            public void VisitHardware(IHardware hardware)
            {
                hardware.Update();
                foreach (IHardware subHardware in hardware.SubHardware) subHardware.Accept(this);
            }
            public void VisitSensor(ISensor sensor) { }
            public void VisitParameter(IParameter parameter) { }
        }
        #endregion

        #region StartUpManagerUsingDLLTaskManager
        public class StartupManager
        {
            //Private variable
            public string programName = "DefaultProgramName";

            //Public variables
            public bool taskSchedulerAvailable = false;

            //Core methods
            public StartupManager(string programName)
            {
                //Fill needed variables
                this.programName = programName;

                //Check if task scheduler instance is connected
                bool isTaskSchedulerConnected = Microsoft.Win32.TaskScheduler.TaskService.Instance.Connected;

                //If task scheduler is available
                if (isAdministrator() == true && isTaskSchedulerConnected == true)
                    taskSchedulerAvailable = true;
                //If task scheduler is not available
                if (isAdministrator() == false || isTaskSchedulerConnected == false)
                    taskSchedulerAvailable = false;
            }

            public bool RegisterToStartOnBoot(bool startOnBoot)
            {
                //Try to register
                try
                {
                    //If is desired to register and task scheduler is available
                    if(startOnBoot == true && taskSchedulerAvailable == true)
                        if(isOnTaskScheduler() == false) //<- Register on task scheduler (if is not on the task scheduler)
                        {
                            //Create the task scheduling
                            Microsoft.Win32.TaskScheduler.TaskDefinition taskDefinition = Microsoft.Win32.TaskScheduler.TaskService.Instance.NewTask();
                            taskDefinition.RegistrationInfo.Description = "Starts Resources Metter on Windows startup.";
                            //Set the trigger
                            taskDefinition.Triggers.Add(new Microsoft.Win32.TaskScheduler.LogonTrigger());
                            //Set the preferences
                            taskDefinition.Settings.StartWhenAvailable = true;
                            taskDefinition.Settings.DisallowStartIfOnBatteries = false;
                            taskDefinition.Settings.StopIfGoingOnBatteries = false;
                            taskDefinition.Settings.ExecutionTimeLimit = TimeSpan.Zero;
                            taskDefinition.Settings.AllowHardTerminate = false;
                            //Set the levels
                            taskDefinition.Principal.RunLevel = Microsoft.Win32.TaskScheduler.TaskRunLevel.Highest;
                            taskDefinition.Principal.LogonType = Microsoft.Win32.TaskScheduler.TaskLogonType.InteractiveToken;
                            //Set the actions to run
                            taskDefinition.Actions.Add(new Microsoft.Win32.TaskScheduler.ExecAction(System.Reflection.Assembly.GetExecutingAssembly().Location, "",
                                                                                                    System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)));
                            //Register the task to be runned
                            Microsoft.Win32.TaskScheduler.TaskService.Instance.RootFolder.RegisterTaskDefinition(nameof(Resources_Metter), taskDefinition);
                        }
                    //If is desired to register and task scheduler is not available
                    if (startOnBoot == true && taskSchedulerAvailable == false)
                        SetRegisterInStartup(true); //<- Add to registry
                    //If is desired to unregister
                    if (startOnBoot == false)
                    {
                        //Remove from task scheduler (if is on task scheduler)
                        if(taskSchedulerAvailable == true && isOnTaskScheduler() == true)
                        {
                            Microsoft.Win32.TaskScheduler.Task task = Microsoft.Win32.TaskScheduler.TaskService.Instance.AllTasks.FirstOrDefault(x => x.Name.Equals(nameof(Resources_Metter), StringComparison.OrdinalIgnoreCase));
                            if (task != null)
                                task.Folder.DeleteTask(task.Name, false);
                        }
                        //Remove from registry
                        SetRegisterInStartup(false);
                    }

                    //Return success
                    return true;
                }
                catch (Exception ex)
                {
                    //Show log error
                    System.IO.File.WriteAllText("last-startupmanager-error.txt", ex.Message);
                    //Return error
                    return false;
                }
            }

            //Tools methods

            private bool isAdministrator()
            {
                try
                {
                    System.Security.Principal.WindowsIdentity identity = System.Security.Principal.WindowsIdentity.GetCurrent();
                    System.Security.Principal.WindowsPrincipal principal = new System.Security.Principal.WindowsPrincipal(identity);

                    return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
                }
                catch
                {
                    return false;
                }
            }

            private bool isOnTaskScheduler()
            {
                //Check if start on boot task is scheduled
                Microsoft.Win32.TaskScheduler.Task task = Microsoft.Win32.TaskScheduler.TaskService.Instance.AllTasks.FirstOrDefault(x => x.Name.Equals(nameof(Resources_Metter), StringComparison.OrdinalIgnoreCase));
                //If task object is null, cancel
                if (task == null)
                    return false;

                //Try to find a task scheduled to run this app
                foreach (Microsoft.Win32.TaskScheduler.Action action in task.Definition.Actions)
                    if (action.ActionType == Microsoft.Win32.TaskScheduler.TaskActionType.Execute && action is Microsoft.Win32.TaskScheduler.ExecAction execAction)
                        if (execAction.Path.Equals(System.Reflection.Assembly.GetExecutingAssembly().Location, StringComparison.OrdinalIgnoreCase))
                            return true;

                //Default return
                return false;
            }

            //Fallback method
            private void SetRegisterInStartup(bool startOnBoot)
            {
                //Register or unregister this app from boot
                Microsoft.Win32.RegistryKey registryKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                if (startOnBoot == true)
                {
                    registryKey.SetValue(programName, "\"" + System.Reflection.Assembly.GetExecutingAssembly().Location + "\"");
                    registryKey.Close();
                }
                if (startOnBoot == false && registryKey.GetValue(programName) != null)
                {
                    registryKey.DeleteValue(programName);
                    registryKey.Close();
                }
            }
        }
        #endregion

        #region DetectionAndRestorationOfAllWindowsOpenedOnSecondaryMonitor

        //Dependencies of DLLs
        private int SW_SHOWNOACTIVATE = 4;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        static extern bool EnumThreadWindows(int dwThreadId, EnumThreadDelegate lpfn, IntPtr lParam);
        delegate bool EnumThreadDelegate(IntPtr hWnd, IntPtr lParam);

        private IEnumerable<IntPtr> EnumerateProcessWindowHandles(int processId)
        {
            var handles = new List<IntPtr>();

            foreach (ProcessThread thread in Process.GetProcessById(processId).Threads)
                EnumThreadWindows(thread.Id, (hWnd, lParam) => { handles.Add(hWnd); return true; }, IntPtr.Zero);

            return handles;
        }

        private static WINDOWPLACEMENT GetPlacement(IntPtr hwnd)
        {
            WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
            placement.length = Marshal.SizeOf(placement);
            GetWindowPlacement(hwnd, ref placement);
            return placement;
        }

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        internal struct WINDOWPLACEMENT
        {
            public int length;
            public int flags;
            public ShowWindowCommands showCmd;
            public System.Drawing.Point ptMinPosition;
            public System.Drawing.Point ptMaxPosition;
            public System.Drawing.Rectangle rcNormalPosition;
        }

        internal enum ShowWindowCommands : int
        {
            Hide = 0,
            Normal = 1,
            Minimized = 2,
            Maximized = 3,
        }

        #endregion
    }
}
