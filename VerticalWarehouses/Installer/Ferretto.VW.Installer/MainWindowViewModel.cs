using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Ferretto.VW.Installer
{
    internal sealed class MainWindowViewModel : BindableBase
    {
        #region Fields

        private readonly InstallationService installationService;

        private Step selectedStep;

        private FlowDocument selectedStepLog = new FlowDocument();

        #endregion

        #region Constructors

        public MainWindowViewModel()
        {
            if (File.Exists("steps-snapshot.json"))
            {
                //this.installationService = InstallationService.LoadAsync("steps-snapshot.json");
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

            this.installationService.PropertyChanged += this.InstallationService_PropertyChanged;

            new Thread(new ThreadStart(async () => await this.installationService.RunAsync())).Start();
        }

        #endregion

        #region Properties

        public Step SelectedStep
        {
            get => this.selectedStep;
            set => this.SetProperty(ref this.selectedStep, value);
        }

        public FlowDocument SelectedStepLog
        {
            get => this.selectedStepLog;
            set => this.SetProperty(ref this.selectedStepLog, value);
        }

        public IEnumerable<Step> Steps => this.installationService.Steps;

        #endregion

        #region Methods

        private void InstallationService_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(InstallationService.ActiveStep))
            {
                this.SelectedStep = this.installationService.ActiveStep;
            }
        }

        #endregion
    }
}
