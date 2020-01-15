using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace Ferretto.VW.Installer
{
    internal sealed class MainWindowViewModel : BindableBase
    {
        #region Fields

        private readonly InstallationService installationService;

        private Step selectedStep;

        #endregion

        #region Constructors

        public MainWindowViewModel()
        {
            if (File.Exists("steps-snapshot.json"))
            {
                this.installationService = InstallationService.LoadAsync("steps.json");
            }
            else if (File.Exists("steps.json"))
            {
                this.installationService = InstallationService.LoadAsync("steps.json");
            }
            else
            {
                // no configuration file found
            }

            this.installationService.RunAsync();
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Properties

        public Step SelectedStep
        {
            get => this.selectedStep;
            set => this.SetProperty(ref this.selectedStep, value);
        }

        public IEnumerable<Step> Steps => this.installationService.Steps;

        #endregion
    }
}
