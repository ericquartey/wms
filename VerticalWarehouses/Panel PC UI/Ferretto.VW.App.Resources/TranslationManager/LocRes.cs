using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Resources;
using System.Globalization;
using System.ComponentModel;

namespace Ferretto.VW.App.Resources
{
    /// <summary>
    /// Classe that manage multilanguage Resources files
    /// </summary>
    public class LocRes
    {
        #region Fields

        /// <summary>
        /// Dictionary of resources. It's our collection of (Name, file) contained in Ferretto.VW.App.Resources
        /// </summary>
        private static readonly Dictionary<string, ResourceManager> resourceManagerDictionary = new Dictionary<string, ResourceManager>();

        private static CultureInfo currentCulture = CultureInfo.InstalledUICulture;

        #endregion

        #region Properties

        /// <summary>
        /// Get/Set accessor to read or modify current culture
        /// </summary>
        public static CultureInfo CurrentCulture
        {
            get { return currentCulture; }

            set

            {
                if (currentCulture != value)
                {
                    currentCulture = value;
                }
            }
        }

        /// <summary>
        /// Class Instance (to be available for markup extension method)
        /// </summary>
        public static LocRes Instance { get; } = new LocRes();

        #endregion

        #region Indexers

        public string this[string key] { get => Get(key); }

        #endregion

        #region Methods

        /// <summary>
        /// Return Localized value for the resource path
        /// Path must be "ResFileName.ResKey" (X Ex: InstallationApp.ActualValue)
        /// </summary>
        public static string Get(string path)
        {
            try
            {
                (string baseName, string stringName) = SplitName(path);

                resourceManagerDictionary.TryGetValue(baseName, out ResourceManager rm);

                return rm.GetString(stringName, currentCulture);
            }
            catch (Exception ex)
            {
                return path + ": InvalidCulture";
            }
        }

        public static void Setup()
        {
            // Setup resource manager
            AddResourceManager(Errors.ResourceManager);
            AddResourceManager(ErrorsApp.ResourceManager);
            AddResourceManager(EventArgsMissionChanged.ResourceManager);
            AddResourceManager(General.ResourceManager);
            AddResourceManager(HelpDescriptions.ResourceManager);
            AddResourceManager(InstallationApp.ResourceManager);
            AddResourceManager(LoadLogin.ResourceManager);
            AddResourceManager(MainMenu.ResourceManager);
            AddResourceManager(MaintenanceMenu.ResourceManager);
            AddResourceManager(Menu.ResourceManager);
            AddResourceManager(OperatorApp.ResourceManager);
            AddResourceManager(SensorCard.ResourceManager);
            AddResourceManager(ServiceHealthProbe.ResourceManager);
            AddResourceManager(ServiceMachine.ResourceManager);

            //To be completed...
        }

        public static (string baseName, string stringName) SplitName(string name)
        {
            int idx = name.LastIndexOf('.');
            return (name.Substring(0, idx), name.Substring(idx + 1));
        }

        // WPF bindings register PropertyChanged event if the object supports it and update themselves when it is raised
        private static void AddResourceManager(ResourceManager resourceManager)
        {
            string name = resourceManager.BaseName.ToString().Split('.').Last();
            resourceManagerDictionary.Add(name, resourceManager);
        }

        #endregion
    }
}
