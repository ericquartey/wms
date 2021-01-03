using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using Ferretto.VW.Installer.Core;
using Ferretto.VW.Installer.Services;
using NLog;

#nullable enable

namespace Ferretto.VW.Installer.ViewModels
{
    internal sealed class StepsViewModel : BindableBase, IOperationResult, IViewModel
    {
        #region Fields

        private readonly Command abortCommand;

        private readonly Command closeCommand;

        private readonly IInstallationService installationService;

        private readonly ILogger logger = LogManager.GetCurrentClassLogger();

        private readonly INotificationService notificationService;

        private readonly Command retryCommand;

        private bool abortRequested;

        private bool isFinished;

        private bool isStarted;

        private bool isSuccessful;

        private Step? selectedStep;

        private FlowDocument selectedStepLog = new FlowDocument();

        #endregion

        #region Constructors

        public StepsViewModel(
            IInstallationService installationService,
            INotificationService notificationService)
        {
            if (installationService is null)
            {
                throw new ArgumentNullException(nameof(installationService));
            }

            if (notificationService is null)
            {
                throw new ArgumentNullException(nameof(notificationService));
            }

            this.notificationService = notificationService;
            this.installationService = installationService;
            this.installationService.PropertyChanged += this.InstallationService_PropertyChanged;
            this.installationService.Finished += this.OnInstallationFinished;

            this.abortCommand = new Command(this.Abort, this.CanAbort);
            this.closeCommand = new Command(this.Close, this.CanClose);
            this.retryCommand = new Command(this.Retry, this.CanRetry);
        }

        #endregion

        #region Properties

        public ICommand AbortCommand => this.abortCommand;

        public bool AbortRequested
        {
            get => this.abortRequested;
            set => this.SetProperty(ref this.abortRequested, value, this.RaiseCanExecuteChanged);
        }

        public ICommand CloseCommand => this.closeCommand;

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

        public ICommand RetryCommand => this.retryCommand;

        public Step? SelectedStep
        {
            get => this.selectedStep;
            set => this.SetProperty(ref this.selectedStep, value);
        }

        public FlowDocument SelectedStepLog
        {
            get => this.selectedStepLog;
            set => this.SetProperty(ref this.selectedStepLog, value);
        }

        public string? SoftwareVersion => this.installationService.InstallerVersion;

        public IEnumerable<Step> Steps => this.installationService.Steps;

        #endregion

        #region Methods

        public Task OnAppearAsync()
        {
            this.notificationService.ClearMessage();

            this.StartInstallation();

            return Task.CompletedTask;
        }

        public Task OnDisappearAsync()
        {
            // do nothing
            return Task.CompletedTask;
        }

        public void StartInstallation()
        {
            this.logger.Info("Starting installation ...");

            try
            {
                this.IsFinished = false;
                this.IsSuccessful = false;
                this.AbortRequested = false;
                this.SelectedStep = null;
                this.isStarted = true;

                this.installationService.Run();
            }
            catch (Exception ex)
            {
                this.IsSuccessful = false;
                this.IsFinished = true;

                this.logger.Error(ex, "Error while running installation.");
            }
        }

        private void Abort()
        {
            this.installationService.Abort();

            this.AbortRequested = true;
        }

        private bool CanAbort()
        {
            return
                !this.installationService.IsRollbackInProgress
                &&
                !this.AbortRequested;
        }

        private bool CanClose()
        {
            return this.IsFinished;
        }

        private bool CanRetry()
        {
            return this.isStarted
                && this.IsFinished
                && !this.installationService.IsRollbackInProgress;
        }

        private void Close()
        {
            Application.Current.Shutdown(this.IsSuccessful ? 0 : -1);
        }

        private void InstallationService_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (e.PropertyName == nameof(IInstallationService.ActiveStep))
                {
                    this.SelectedStep = this.installationService.ActiveStep;
                    this.RaiseCanExecuteChanged();
                }
            });
        }

        private void OnInstallationFinished(object? sender, InstallationFinishedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                this.IsFinished = true;
                this.IsSuccessful = e.Success;

                if (this.IsSuccessful)
                {
                    this.notificationService.SetMessage("Installation complete.");
                    //this.installationService.CleanSnapshot();
                }
                else
                {
                    this.notificationService.SetErrorMessage("Installation failed.");
                }
            });
        }

        private void RaiseCanExecuteChanged()
        {
            this.closeCommand.RaiseCanExecuteChanged();
            this.abortCommand.RaiseCanExecuteChanged();
            this.retryCommand.RaiseCanExecuteChanged();
        }

        private void Retry()
        {
            this.logger.Info("Retry installation");
            this.notificationService.ClearMessage();

            this.StartInstallation();
        }

        #endregion
    }
}
