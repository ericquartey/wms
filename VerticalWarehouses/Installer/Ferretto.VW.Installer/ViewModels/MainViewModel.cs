using System;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Windows;
using Ferretto.VW.Installer.Core;

namespace Ferretto.VW.Installer.ViewModels
{
    public class MainViewModel : BindableBase
    {
        #region Fields
        private const string INSTALLER = "installer";

        private IOperationResult currentMode;

        private InstallationService installationService;

        private IOperationResult installBayViewModel;

        private IOperationResult installTypeViewModel;

        private IOperationResult updateViewModel;

        #endregion

        #region Properties

        public IOperationResult CurrentMode
        {
            get => this.currentMode;
            set => this.SetProperty(ref this.currentMode, value);
        }

        #endregion

        #region Methods

        public void StartInstallation()
        {
            //var currExeLocation = System.Reflection.Assembly.GetEntryAssembly().Location;
            var exeLocation = $"{ConfigurationManager.AppSettings.GetUpdateTempPath()}\\{INSTALLER}";
            // Directory.SetCurrentDirectory(System.IO.Path.GetDirectoryName(exeLocation));
            Directory.SetCurrentDirectory(exeLocation);

            if (File.Exists("steps-snapshot.json"))
            {
                this.installationService = InstallationService.GetInstance("steps-snapshot.json");
                //this.installationService = InstallationService.LoadAsync("steps.json");
            }
            else if (File.Exists("steps.json"))
            {
                this.installationService = InstallationService.GetInstance("steps.json");
            }
            else
            {
                // no configuration file found
            }

            this.installationService.LoadSteps();

            this.installationService.PropertyChanged += this.InstallationService_PropertyChanged;

            if (!this.installationService.CanStart())
            {
                this.Close();
            }

            this.installationService.Start();
        }

        private void Close()
        {
            Application.Current.Shutdown(this.CurrentMode?.IsSuccessful == true? 0 : -1);
        }

        private void InstallationService_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(OperationMode))
            {
                switch (this.installationService.OperationMode)
                {
                    case OperationMode.None:
                        throw new InvalidOperationException("Can't change on current mode:Operation type not supported.");

                    case OperationMode.ImstallType:
                        this.CurrentMode = this.installTypeViewModel ?? (this.installTypeViewModel = new InstallTypeViewModel(this.installationService));
                        break;

                    case OperationMode.InstallBay:
                        this.CurrentMode = this.installBayViewModel ?? (this.installBayViewModel = new InstallBayViewModel(this.installationService));
                        break;

                    case OperationMode.Update:
                        this.CurrentMode = this.updateViewModel ?? (this.updateViewModel = new UpdateViewModel(this.installationService));
                        break;

                    default:
                        break;
                }
            }
        }

        #endregion
    }
}
