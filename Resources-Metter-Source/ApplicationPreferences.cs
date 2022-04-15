using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Resources_Metter
{
    /*
     * This class defines settings of a user, save (serialize) and load (deserealize) too 
     */

    public class ApplicationPreferences
    {
        //Constants
        private const string fileName = "settings.xml";

        //Variables
        public int overlayPosition = 3;
        public int screenToShow = 0;
        public int metricsStyle = 1;
        public int overlayTheme = 1;
        public int overlayOpacity = 75;
        public int overlayPositionX = 0;
        public int overlayPositionY = 0;
        public bool enableAutoHide = true;
        public bool enableBoldText = false;
        public int metricsUpdateInterval = 300;
        public int networkInterfacePreferred = -1;
        public int networkUnit = 1;
        public bool enableClock = false;
        public bool enableTopMost = true;
        public bool enableWindowRestorerForSecMonitor = true;
        public string[] ignoreFromAutoHide = new string[] { "Program Manager", " menu", "menu" };
        public int overheatWarningForCpu = 70;
        public int overheatWarningForGpu = 70;
        public bool gpuFan1Stopped = false;
        public bool gpuFan2Stopped = false;
        public bool startOnBoot = false;

        //Core methods

        public void LoadPreferences()
        {
            //If the save file not exists, create one and return
            if (File.Exists(fileName) == false)
            {
                ApplyPreferences();
                return;
            }

            //Deserealize settings
            using (StreamReader sw = new StreamReader(fileName))
            {
                XmlSerializer xmls = new XmlSerializer(typeof(ApplicationPreferences));
                ApplicationPreferences loadedPreferences = xmls.Deserialize(sw) as ApplicationPreferences;

                //Get the loaded preferences
                overlayPosition = loadedPreferences.overlayPosition;
                screenToShow = loadedPreferences.screenToShow;
                metricsStyle = loadedPreferences.metricsStyle;
                overlayTheme = loadedPreferences.overlayTheme;
                overlayOpacity = loadedPreferences.overlayOpacity;
                overlayPositionX = loadedPreferences.overlayPositionX;
                overlayPositionY = loadedPreferences.overlayPositionY;
                enableAutoHide = loadedPreferences.enableAutoHide;
                enableBoldText = loadedPreferences.enableBoldText;
                metricsUpdateInterval = loadedPreferences.metricsUpdateInterval;
                networkInterfacePreferred = loadedPreferences.networkInterfacePreferred;
                networkUnit = loadedPreferences.networkUnit;
                enableClock = loadedPreferences.enableClock;
                enableTopMost = loadedPreferences.enableTopMost;
                enableWindowRestorerForSecMonitor = loadedPreferences.enableWindowRestorerForSecMonitor;
                ignoreFromAutoHide = loadedPreferences.ignoreFromAutoHide;
                overheatWarningForCpu = loadedPreferences.overheatWarningForCpu;
                overheatWarningForGpu = loadedPreferences.overheatWarningForGpu;
                gpuFan1Stopped = loadedPreferences.gpuFan1Stopped;
                gpuFan2Stopped = loadedPreferences.gpuFan2Stopped;
                startOnBoot = loadedPreferences.startOnBoot;
            }
        }

        public void ApplyPreferences()
        {
            //Save all preferences to file (serialization)
            using (StreamWriter sw = new StreamWriter(fileName))
            {
                XmlSerializer xmls = new XmlSerializer(typeof(ApplicationPreferences));
                xmls.Serialize(sw, this);
            }
        }
    }
}
