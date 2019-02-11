using System;
using System.Configuration;
using System.Windows;

namespace Ferretto.VW.VWApp
{
    public partial class App : Application
    {
        #region Fields

        private static readonly string JSON_GENERAL_INFO_PATH = string.Concat(Environment.CurrentDirectory, ConfigurationManager.AppSettings["GeneralInfoFilePath"]);

        private static readonly string JSON_INSTALLATION_INFO_PATH = string.Concat(Environment.CurrentDirectory, ConfigurationManager.AppSettings["InstallationInfoFilePath"]);

        #endregion

        #region Properties

        public InstallationApp.MainWindow InstallationAppMainWindowInstance { get; set; }

        public InstallationApp.MainWindowViewModel InstallationAppMainWindowViewModel { get; set; }

        public bool MachineOk { get; set; }

        public Skin Skin { get; set; } = Skin.Dark;

        #endregion

        #region Methods

        public void ChangeSkin()
        {
            (Current as App).Resources.MergedDictionaries.Clear();
            var skinDictionary = new ResourceDictionary();
            if ((Current as App).Skin == Skin.Light)
            {
                (Current as App).Skin = Skin.Dark;
                skinDictionary.Source = new Uri("/Ferretto.VW.CustomControls;Component/Skins/DarkSkin.xaml", UriKind.Relative);
            }
            else
            {
                (Current as App).Skin = Skin.Light;
                skinDictionary.Source = new Uri("/Ferretto.VW.CustomControls;Component/Skins/LightSkin.xaml", UriKind.Relative);
            }
            (Current as App).Resources.MergedDictionaries.Add(skinDictionary);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var bootstrapper = new Bootstrapper();

            bootstrapper.Run();
        }

        #endregion
    }
}




public enum Skin { Light, Dark, Medium }
