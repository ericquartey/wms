using System;
using System.Windows;
using NLog;

namespace Ferretto.VW.VWApp
{
    public partial class App : Application
    {
        #region Fields

        private Logger logger;

        #endregion

        #region Constructors

        public App()
        {
            this.logger = LogManager.GetCurrentClassLogger();
            System.AppDomain.CurrentDomain.UnhandledException += this.CurrentDomain_UnhandledException;
            this.logger.Error(new Exception(), "CTOR!");
        }

        #endregion

        #region Properties

        public InstallationApp.MainWindow InstallationAppMainWindowInstance { get; set; }

        public InstallationApp.MainWindowViewModel InstallationAppMainWindowViewModel { get; set; }

        public bool MachineOk { get; set; }

        public OperatorApp.MainWindow OperatorAppMainWindowInstance { get; set; }

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

        private void CurrentDomain_UnhandledException(object sender, System.UnhandledExceptionEventArgs e)
        {
            this.logger.Error(e.ExceptionObject as System.Exception, "An unhandled exception was thrown.");
        }

        #endregion
    }
}




public enum Skin { Light, Dark, Medium }
