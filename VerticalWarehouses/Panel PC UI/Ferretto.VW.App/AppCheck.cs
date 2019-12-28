using System;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading;

namespace Ferretto.VW.App
{
    public static class AppCheck
    {
        #region Fields

        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private static Mutex appMutex;

        #endregion

        #region Methods

        public static void End()
        {
            appMutex?.Close();
        }

        public static bool Start()
        {
            var canStart = true;

            // Get windows Product Type
            const string queryString = "SELECT ProductType FROM Win32_OperatingSystem";
            using (var managementObjectSearcher = new ManagementObjectSearcher(queryString))
            {
                var productType = (from ManagementObject managementObject in managementObjectSearcher.Get()
                                   from PropertyData propertyData in managementObject.Properties
                                   where propertyData.Name == "ProductType"
                                   select (uint)propertyData.Value).First();

                var panelPc = productType == 1 ? "Local\\PanelPc" : "Global\\PanelPc";

                appMutex = new Mutex(true, panelPc, out var createdNew);
                if (!createdNew)
                {
                    var current = Process.GetCurrentProcess();
                    foreach (var process in Process.GetProcessesByName("Ferretto.VW.App"))
                    {
                        if (process.Id != current.Id)
                        {
                            SetForegroundWindow(process.MainWindowHandle);
                            logger.Info("*** Application already started ***");
                            canStart = false;
                            break;
                        }
                    }
                }
            }

            return canStart;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        #endregion
    }
}
