using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using Ferretto.VW.Installer.Core;

namespace Ferretto.VW.Installer
{
    internal sealed class MainWindowViewModel : BindableBase
    {
        #region Fields

        private readonly InstallationService installationService;

        private RelayCommand abortCommand;

        private bool abortRequested;

        private RelayCommand closeCommand;

        private bool isFinished;

        private bool isSuccessful;

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
            this.installationService.Finished += this.OnInstallationServiceFinished;
        }

        #endregion

        #region Properties

        public ICommand AbortCommand =>
            this.abortCommand
            ??
            (this.abortCommand = new RelayCommand(this.Abort, this.CanAbort));

        public bool AbortRequested
        {
            get => this.abortRequested;
            set => this.SetProperty(ref this.abortRequested, value, this.RaiseCanExecuteChanged);
        }

        public ICommand CloseCommand =>
            this.closeCommand
            ??
            (this.closeCommand = new RelayCommand(this.Close, this.CanClose));

        public bool IsFinished
        {
            get => this.isFinished;
            set => this.SetProperty(ref this.isFinished, value, this.RaiseCanExecuteChanged);
        }

        public bool IsSuccessful
        {
            get => this.isSuccessful;
            set => this.SetProperty(ref this.isSuccessful, value);
        }

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

        public void StartInstallation()
        {
            new Thread(
                new ThreadStart(async () =>
                {
                    try
                    {
                        await this.installationService.RunAsync();
                    }
                    catch
                    {
                        this.IsSuccessful = false;
                        this.IsFinished = true;
                    }
                })).Start();
        }

        private void Abort()
        {
            this.AbortRequested = true;

            this.installationService.Abort();

            this.RaiseCanExecuteChanged();
        }

        private bool CanAbort()
        {
            return !this.installationService.IsRollbackInProgress;
        }

        private bool CanClose()
        {
            return this.IsFinished;
        }

        private void Close()
        {
            Application.Current.Shutdown(this.IsSuccessful ? 0 : -1);
        }

        private void InstallationService_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Core.InstallationService.ActiveStep))
            {
                this.SelectedStep = this.installationService.ActiveStep;
                this.RaiseCanExecuteChanged();
            }
        }

        private void OnInstallationServiceFinished(object sender, InstallationFinishedEventArgs e)
        {
            this.IsFinished = true;
            this.IsSuccessful = e.Success;
        }

        private void RaiseCanExecuteChanged()
        {
            this.closeCommand?.RaiseCanExecuteChanged();
            this.abortCommand?.RaiseCanExecuteChanged();
        }

        #endregion
    }
}
