using System;
using System.Configuration;
using System.Windows;
using Ferretto.VW.Utils.Source.CellsManagement;
using Ferretto.VW.Utils.Source.Configuration;
using Newtonsoft.Json;
using System.IO;
using Ferretto.VW.Utils.Source;
using Ferretto.VW.Navigation;
using System.Diagnostics;

namespace Ferretto.VW.VWApp
{
    public partial class App : Application
    {
        #region Fields

        private static readonly string JSON_GENERAL_INFO_PATH = string.Concat(Environment.CurrentDirectory, ConfigurationManager.AppSettings["GeneralInfoFilePath"]);
        private static readonly string JSON_INSTALLATION_INFO_PATH = string.Concat(Environment.CurrentDirectory, ConfigurationManager.AppSettings["InstallationInfoFilePath"]);
        private bool machineOk;

        #endregion Fields

        #region Constructors

        public App()
        {
            this.InitializeComponent();
            NavigationService.InitializeEvents();
            DataManager.CurrentData = new DataManager();
            NavigationService.ChangeSkinEventHandler += this.ChangeSkin;
        }

        #endregion Constructors

        #region Properties

        public static Skin Skin { get; set; } = Skin.Dark;

        public InstallationApp.MainWindow InstallationAppMainWindowInstance { get; set; }

        public InstallationApp.MainWindowViewModel InstallationAppMainWindowViewModel { get; set; }

        public Boolean MachineOk { get => this.machineOk; set => this.machineOk = value; }

        public OperatorApp.MainWindow OperatorMainWindowInstance { get; set; }

        #endregion Properties

        #region Methods

        public void ChangeSkin()
        {
            Debug.Print("FLAG\n");

            if (Skin == Skin.Light)
            {
                Skin = Skin.Dark;
            }
            else
            {
                Skin = Skin.Light;
            }
            //this.Resources.Clear();
            this.Resources.MergedDictionaries[0].MergedDictionaries.Clear();
            if (Skin == Skin.Dark)
                this.ApplyResources("Ferretto.VW.CustomControls;Component/Skins/DarkSkin.xaml");
            else if (Skin == Skin.Light)
                this.ApplyResources("Ferretto.VW.CustomControls;Component/Skins/LightSkin.xaml");
            this.ApplyResources("Ferretto.VW.CustomControls;Component/Skins/Shared.xaml");
            Debug.Print("FLAG\n");
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            this.ChangeSkin();
        }

        private void ApplyResources(string src)
        {
            var dict = new ResourceDictionary() { Source = new Uri(src, UriKind.Relative) };
            foreach (var mergeDict in dict.MergedDictionaries)
            {
                Application.Current.Resources.MergedDictionaries[0].MergedDictionaries.Add(dict);
            }
            //foreach (var key in dict.Keys)
            //{
            //    Application.Current.Resources[key] = dict[key];
            //}
        }

        #endregion Methods
    }
}

public enum Skin { Light, Dark }
