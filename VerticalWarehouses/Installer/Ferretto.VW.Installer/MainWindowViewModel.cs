using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace Ferretto.VW.Installer
{
    internal sealed class MainWindowViewModel : BindableBase
    {
        #region Fields

        private readonly InstallationService installationService;

        private RelayCommand abortCommand;

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

        public ICommand AbortCommand =>
            this.abortCommand
            ??
            (this.abortCommand = new RelayCommand(this.Abort, this.CanAbort));

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

        public string SoftwareVersion => this.installationService.SoftwareVersion;

        public IEnumerable<Step> Steps => this.installationService.Steps;

        #endregion

        #region Methods

        private void Abort()
        {
            this.installationService.Abort();
            this.abortCommand?.RaiseCanExecuteChanged();
        }

        private bool CanAbort()
        {
            return !this.installationService.IsRollbackInProgress;
        }

        private void InstallationService_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(InstallationService.ActiveStep))
            {
                this.SelectedStep = this.installationService.ActiveStep;
                this.abortCommand.RaiseCanExecuteChanged();
            }
        }

        #endregion
    }
}
