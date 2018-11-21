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
            NavigationService.ChangeSkinToLightEventHandler += (Current as App).ChangeSkinToLight;
            NavigationService.ChangeSkinToMediumEventHandler += (Current as App).ChangeSkinToMedium;
            NavigationService.ChangeSkinToDarkEventHandler += (Current as App).ChangeSkinToDark;
        }

        #endregion Constructors

        #region Properties

        public InstallationApp.MainWindow InstallationAppMainWindowInstance { get; set; }
        public InstallationApp.MainWindowViewModel InstallationAppMainWindowViewModel { get; set; }
        public Boolean MachineOk { get => this.machineOk; set => this.machineOk = value; }
        public OperatorApp.MainWindow OperatorMainWindowInstance { get; set; }
        public Skin Skin { get; set; } = Skin.Light;

        #endregion Properties

        #region Methods

        public void ChangeSkin(Skin newSkin)
        {
            (Current as App).Resources.MergedDictionaries.Clear();

            (Current as App).Skin = newSkin;

            var skinDictionary = new ResourceDictionary();

            switch ((Current as App).Skin)
            {
                case Skin.Dark:
                    skinDictionary.Source = new Uri("/Ferretto.VW.CustomControls;Component/Skins/DarkSkin.xaml", UriKind.Relative);
                    break;

                case Skin.Medium:
                    skinDictionary.Source = new Uri("/Ferretto.VW.CustomControls;Component/Skins/MediumSkin.xaml", UriKind.Relative);
                    break;

                case Skin.Light:
                default:
                    skinDictionary.Source = new Uri("/Ferretto.VW.CustomControls;Component/Skins/LightSkin.xaml", UriKind.Relative);
                    break;
            }
            (Current as App).Resources.MergedDictionaries.Add(skinDictionary);
        }

        private void ChangeSkinToDark() => (Current as App).ChangeSkin(Skin.Dark);

        private void ChangeSkinToLight() => (Current as App).ChangeSkin(Skin.Light);

        private void ChangeSkinToMedium() => (Current as App).ChangeSkin(Skin.Medium);

        #endregion Methods
    }
}

public enum Skin { Light, Dark, Medium }
