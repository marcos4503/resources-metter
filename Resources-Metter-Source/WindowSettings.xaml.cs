using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Resources_Metter
{
    /// <summary>
    /// Lógica interna para WindowSettings.xaml
    /// </summary>
    public partial class WindowSettings : Window
    {
        public WindowSettings()
        {
            InitializeComponent();
            OnStart();
        }
        
        //Window variables
        public class NetworkInterface
        {
            public string Name { get; set; }
            public string Value { get; set; }
            public override string ToString() { return this.Name; }
        }
        public class IgnoreFromAutoHideListItem
        {
            public bool IsChecked { get; set; }
            public string ID { get; set; }
            public string Program { get; set; }
        }
        public System.Windows.Forms.Timer timer = null;

        //Window methods

        public void OnStart()
        {
            //Try to load all preferences of user
            ApplicationPreferences applicationPreferences = new ApplicationPreferences();
            applicationPreferences.LoadPreferences();

            //Prepare the network interface combo box
            PerformanceCounterCategory networkCounter = new PerformanceCounterCategory("Network Interface");
            string[] instances = networkCounter.GetInstanceNames();
            foreach (string instance in instances)
                networkInterface.Items.Add(new NetworkInterface { Name=instance, Value=instance });

            //Apply the preferences loaded to interface
            overlayPosition.SelectedIndex = applicationPreferences.overlayPosition;
            screenToShow.SelectedIndex = applicationPreferences.screenToShow;
            metricsStyle.SelectedIndex = applicationPreferences.metricsStyle;
            overlayTheme.SelectedIndex = applicationPreferences.overlayTheme;
            overlayOpacity.Value = applicationPreferences.overlayOpacity;
            customPositionX.Text = applicationPreferences.overlayPositionX.ToString();
            customPositionY.Text = applicationPreferences.overlayPositionY.ToString();
            enableAutoHide.IsChecked = applicationPreferences.enableAutoHide;
            enableBoldText.IsChecked = applicationPreferences.enableBoldText;
            updateInterval.Value = applicationPreferences.metricsUpdateInterval;
            if (applicationPreferences.networkInterfacePreferred != -1)
                networkInterface.SelectedIndex = applicationPreferences.networkInterfacePreferred;
            networkUnit.SelectedIndex = applicationPreferences.networkUnit;
            enableClock.IsChecked = applicationPreferences.enableClock;
            enableTopMost.IsChecked = applicationPreferences.enableTopMost;
            enableWindowRestorer.IsChecked = applicationPreferences.enableWindowRestorerForSecMonitor;
            overheatForCpu.Value = applicationPreferences.overheatWarningForCpu;
            overheatForGpu.Value = applicationPreferences.overheatWarningForGpu;
            gpuFan1Stopped.IsChecked = applicationPreferences.gpuFan1Stopped;
            gpuFan2Stopped.IsChecked = applicationPreferences.gpuFan2Stopped;
            autoStartBoot.IsChecked = applicationPreferences.startOnBoot;

            //Fill the ignore from auto hide list view
            FillIgnoreFromAutoHideListView(applicationPreferences.ignoreFromAutoHide);

            //Each 100ms check current program on foreground and show the title
            timer = new System.Windows.Forms.Timer { Interval = 100 };
            timer.Enabled = true;
            timer.Tick += new EventHandler((object sender, EventArgs e) => {
                //Get the title of current program in foreground
                string currentProgramOnForeground = GetActiveWindowTitle();

                //Show the current program on overlay
                if (String.IsNullOrEmpty(currentProgramOnForeground) == false && String.IsNullOrWhiteSpace(currentProgramOnForeground) == false) 
                {
                    currentOnForeground.Text = "\"" + currentProgramOnForeground + "\"";
                    currentOnForeground.FontStyle = FontStyles.Normal;
                    currentOnForeground.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                }
                if (String.IsNullOrEmpty(currentProgramOnForeground) == true || String.IsNullOrWhiteSpace(currentProgramOnForeground) == true)
                {
                    currentOnForeground.Text = "Program Without Title";
                    currentOnForeground.FontStyle = FontStyles.Italic;
                    currentOnForeground.Foreground = new SolidColorBrush(Color.FromRgb(120, 120, 120));
                }
            });
        }

        //Listview methods

        public void FillIgnoreFromAutoHideListView(string[] ignoreFromAutoHide)
        {
            //Fill the list view
            List<IgnoreFromAutoHideListItem> ignoreFromAutoHideListSource = new List<IgnoreFromAutoHideListItem>();
            for (int i = 0; i < ignoreFromAutoHide.Length; i++)
                ignoreFromAutoHideListSource.Add(new IgnoreFromAutoHideListItem() { IsChecked = false, ID = i.ToString(), Program = ignoreFromAutoHide[i] });
            ignoreFromAutoHideList.ItemsSource = ignoreFromAutoHideListSource;
        }

        //Window events

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            //Try to load all preferences of user
            ApplicationPreferences applicationPreferences = new ApplicationPreferences();
            applicationPreferences.LoadPreferences();

            //Reconstruct the list and add the new element
            List<string> itemsFromList = new List<string>();
            foreach (string item in applicationPreferences.ignoreFromAutoHide)
                itemsFromList.Add(item);
            itemsFromList.Add(programToAdd.Text);
            applicationPreferences.ignoreFromAutoHide = itemsFromList.ToArray();

            //Apply the preferences
            applicationPreferences.ApplyPreferences();

            //Fill the list again
            FillIgnoreFromAutoHideListView(applicationPreferences.ignoreFromAutoHide);

            //Reset the text box
            programToAdd.Text = "";
        }

        private void removeSelected_Click(object sender, RoutedEventArgs e)
        {
            //Try to load all preferences of user
            ApplicationPreferences applicationPreferences = new ApplicationPreferences();
            applicationPreferences.LoadPreferences();

            //Remove all selected items from list
            List<string> itemsFromList = new List<string>();
            foreach (IgnoreFromAutoHideListItem item in ignoreFromAutoHideList.Items)
                if (item.IsChecked == false)
                    itemsFromList.Add(item.Program);
            applicationPreferences.ignoreFromAutoHide = itemsFromList.ToArray();

            //Apply the preferences
            applicationPreferences.ApplyPreferences();

            //Fill the list again
            FillIgnoreFromAutoHideListView(applicationPreferences.ignoreFromAutoHide);
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            //Load all preferences of user
            ApplicationPreferences applicationPreferences = new ApplicationPreferences();
            applicationPreferences.LoadPreferences();

            //Validate custom positions
            if (customPositionX.Text == "")
                customPositionX.Text = "0";
            if (customPositionY.Text == "")
                customPositionY.Text = "0";

            //Put the new preferences
            applicationPreferences.overlayPosition = overlayPosition.SelectedIndex;
            applicationPreferences.screenToShow = screenToShow.SelectedIndex;
            applicationPreferences.metricsStyle = metricsStyle.SelectedIndex;
            applicationPreferences.overlayTheme = overlayTheme.SelectedIndex;
            applicationPreferences.overlayOpacity = (int)overlayOpacity.Value;
            applicationPreferences.overlayPositionX = int.Parse(customPositionX.Text);
            applicationPreferences.overlayPositionY = int.Parse(customPositionY.Text);
            applicationPreferences.enableAutoHide = (bool)enableAutoHide.IsChecked;
            applicationPreferences.enableBoldText = (bool)enableBoldText.IsChecked;
            applicationPreferences.metricsUpdateInterval = (int)updateInterval.Value;
            applicationPreferences.networkInterfacePreferred = networkInterface.SelectedIndex;
            applicationPreferences.networkUnit = networkUnit.SelectedIndex;
            applicationPreferences.enableClock = (bool)enableClock.IsChecked;
            applicationPreferences.enableTopMost = (bool)enableTopMost.IsChecked;
            applicationPreferences.enableWindowRestorerForSecMonitor = (bool)enableWindowRestorer.IsChecked;
            applicationPreferences.overheatWarningForCpu = (int)overheatForCpu.Value;
            applicationPreferences.overheatWarningForGpu = (int)overheatForGpu.Value;
            applicationPreferences.gpuFan1Stopped = (bool)gpuFan1Stopped.IsChecked;
            applicationPreferences.gpuFan2Stopped = (bool)gpuFan2Stopped.IsChecked;
            applicationPreferences.startOnBoot = (bool)autoStartBoot.IsChecked;

            //Save the new preferences
            applicationPreferences.ApplyPreferences();

            //Show a alert dialog
            MessageBox.Show("It may be that some changes made here will only be seen after restarting Resources Metter.", "All Done");

            //Close this window
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Stop the timer thread
            timer.Stop();
        }



        #region CurrentForegroundProgramTitle

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

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

        #region TextBoxValidationsWithPreviewEvents

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        #endregion
    }
}
