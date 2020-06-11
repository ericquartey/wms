using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;
using Ferretto.VW.Installer.Core;

namespace Ferretto.VW.Installer.ViewModels
{
    internal sealed class UpdateViewModel : BindableBase, IOperationResult
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

        public UpdateViewModel(InstallationService installationService)
        {
            this.installationService = installationService ?? throw new ArgumentNullException(nameof(installationService));

            this.installationService.PropertyChanged += this.InstallationService_PropertyChanged;
            this.installationService.Finished += this.OnInstallationServiceFinished;

            this.StartInstallation();
        }

        #endregion

        #region Properties

        public ICommand AbortCommand => this.abortCommand ??= new RelayCommand(this.Abort, this.CanAbort);

        public bool AbortRequested
        {
            get => this.abortRequested;
            set => this.SetProperty(ref this.abortRequested, value, this.RaiseCanExecuteChanged);
        }

        public ICommand CloseCommand => this.closeCommand ??= new RelayCommand(this.Close, this.CanClose);

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
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    this.closeCommand?.RaiseCanExecuteChanged();
                    this.abortCommand?.RaiseCanExecuteChanged();
                }), DispatcherPriority.Background);
        }

        #endregion
    }
}
