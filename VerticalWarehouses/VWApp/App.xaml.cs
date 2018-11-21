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
            NavigationService.ChangeSkinEventHandler += (Current as App).ChangeSkin;
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

        public void ChangeSkin()
        {
            (Current as App).Resources.MergedDictionaries.Clear();

            if ((Current as App).Skin == Skin.Dark)
            {
                (Current as App).Skin = Skin.Light;
            }
            else
            {
                (Current as App).Skin = Skin.Dark;
            }

            if ((Current as App).Skin == Skin.Dark)
            {
                var skinDictionary = new ResourceDictionary();
                var styleDictionary = new ResourceDictionary();
                skinDictionary.Source = new Uri("/Ferretto.VW.CustomControls;Component/Skins/DarkSkin.xaml", UriKind.Relative);
                styleDictionary.Source = new Uri("/Ferretto.VW.CustomControls;Component/Skins/Shared.xaml", UriKind.Relative);
                (Current as App).Resources.MergedDictionaries.Add(skinDictionary);
                (Current as App).Resources.MergedDictionaries.Add(styleDictionary);
            }
            else
            {
                var skinDictionary = new ResourceDictionary();
                var styleDictionary = new ResourceDictionary();
                skinDictionary.Source = new Uri("/Ferretto.VW.CustomControls;Component/Skins/LightSkin.xaml", UriKind.Relative);
                styleDictionary.Source = new Uri("/Ferretto.VW.CustomControls;Component/Skins/Shared.xaml", UriKind.Relative);
                (Current as App).Resources.MergedDictionaries.Add(skinDictionary);
                (Current as App).Resources.MergedDictionaries.Add(styleDictionary);
            }
        }

        #endregion Methods
    }
}

public enum Skin { Light, Dark }
