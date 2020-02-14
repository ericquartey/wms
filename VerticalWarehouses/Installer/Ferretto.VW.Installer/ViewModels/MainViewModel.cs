using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using Ferretto.VW.Installer.Core;

namespace Ferretto.VW.Installer.ViewModels
{
    public class MainViewModel : BindableBase
    {

        private InstallationService installationService;

        private IOperationResult currentMode;
        private IOperationResult installViewModel;
        private IOperationResult updateViewModel;

        public IOperationResult CurrentMode
        {
            get => this.currentMode;
            set => this.SetProperty(ref this.currentMode, value);
        }


        public void StartInstallation()
        {
            var currExeLocation = System.Reflection.Assembly.GetEntryAssembly().Location;
            Directory.SetCurrentDirectory(System.IO.Path.GetDirectoryName(currExeLocation));

            if (File.Exists("steps-snapshot.json"))
            {
                this.installationService = InstallationService.LoadAsync("steps-snapshot.json");
                //this.installationService = InstallationService.LoadAsync("steps.json");
            }
            else if (File.Exists("steps.json"))
            {
                this.installationService = InstallationService.LoadAsync("steps.json");
            }
            else
            {
                // no configuration file found
            }

            this.installationService.PropertyChanged += this.InstallationService_PropertyChanged;

            if (!this.installationService.CanStart())
            {
                this.Close();
            }

            this.installationService.Start();
        }

        private void InstallationService_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(OperationMode))
            {
                switch (this.installationService.OperationMode)
                {
                    case OperationMode.None:
                        throw new InvalidOperationException("Can't change on current mode:Operation type not supported.");                                                
                    case OperationMode.Imstall:
                        this.CurrentMode = this.installViewModel ?? (this.installViewModel = new InstallViewModel(this.installationService));                        
                        break;
                    case OperationMode.Update:
                        this.CurrentMode = this.updateViewModel ?? (this.updateViewModel = new UpdateViewModel(this.installationService));                        
                        break;
                    default:
                        break;
                }
            }
        }

        private void Close()
        {
            Application.Current.Shutdown(this.CurrentMode.IsSuccessful ? 0 : -1);
        }
    }
}
