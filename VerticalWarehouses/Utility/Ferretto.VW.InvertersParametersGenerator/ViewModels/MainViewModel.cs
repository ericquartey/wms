using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.InvertersParametersGenerator.Interfaces;
using Ferretto.VW.InvertersParametersGenerator.Models;
using Ferretto.VW.InvertersParametersGenerator.Services;
using NLog;
using Prism.Commands;
using Prism.Mvvm;

namespace Ferretto.VW.InvertersParametersGenerator.ViewModels
{
    public class MainViewModel : BindableBase, IParentActionChanged
    {
        #region Fields

        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        private ConfigurationService configurationService;

        private IOperationResult currentMode;

        private IOperationResult exportConfigurationViewModel;

        private IOperationResult invertersViewModel;

        private DelegateCommand nextCommand;

        private string notificationMessage;

        private NotificationSeverity notificationSeverity;

        private DelegateCommand previousCommand;

        private IOperationResult setParametersViewModel;

        private IOperationResult vertimagConfigurationViewModel;

        #endregion

        #region Properties

        public IOperationResult CurrentMode
        {
            get => this.currentMode;
            set => this.SetProperty(ref this.currentMode, value);
        }

        public ICommand NextCommand =>
                this.nextCommand
                ??
                (this.nextCommand = new DelegateCommand(this.Next, this.CanNext));

        public string NotificationMessage
        {
            get => this.notificationMessage;
            set => this.SetProperty(ref this.notificationMessage, value);
        }

        public NotificationSeverity NotificationSeverity
        {
            get => this.notificationSeverity;
            set => this.SetProperty(ref this.notificationSeverity, value);
        }

        public ICommand PreviousCommand =>
                        this.previousCommand
                        ??
                        (this.previousCommand = new DelegateCommand(this.Previous, this.CanPrevious));

        #endregion

        #region Methods

        public void Notify(string message, NotificationSeverity notificationSeverity)
        {
            if (message is null)
            {
                return;
            }

            this.NotificationSeverity = notificationSeverity;
            this.NotificationMessage = message;

            Task.Delay(10000).ContinueWith(t => this.NotificationMessage = null, TaskScheduler.Current);
        }

        public void Notify(Exception exception, NotificationSeverity notificationSeverity)
        {
            if (exception is null)
            {
                return;
            }

            this.NotificationSeverity = notificationSeverity;
            this.NotificationMessage = exception.Message;

            this.logger.Error(exception, this.NotificationMessage);

            Task.Delay(10000).ContinueWith(t => this.NotificationMessage = null, TaskScheduler.Current);
        }

        public void RaiseCanExecuteChanged()
        {
            this.nextCommand?.RaiseCanExecuteChanged();
            this.previousCommand?.RaiseCanExecuteChanged();
        }

        public void StartInstallation()
        {
            this.configurationService = ConfigurationService.GetInstance;
            this.configurationService.Start();
            this.ChangeMode(true);
        }

        private bool CanNext()
        {
            return this.currentMode?.CanNext == true;
        }

        private bool CanPrevious()
        {
            return this.currentMode?.CanPrevious == true;
        }

        private void ChangeMode(bool isNext)
        {
            switch (this.configurationService.WizardMode)
            {
                case WizardMode.None:
                    throw new InvalidOperationException("Can't change on current mode:Wizard not supported.");

                case WizardMode.ImportConfiguration:
                    if (isNext)
                    {
                        this.CurrentMode = this.vertimagConfigurationViewModel = new VertimagConfigurationViewModel(this.configurationService, this);
                    }
                    this.CurrentMode = this.vertimagConfigurationViewModel;
                    break;

                case WizardMode.Inverters:
                    if (isNext)
                    {
                        this.CurrentMode = this.invertersViewModel = new InvertersViewModel(this.configurationService, this);
                    }

                    this.CurrentMode = this.invertersViewModel;
                    break;

                case WizardMode.Parameters:
                    if (isNext)
                    {
                        this.CurrentMode = this.setParametersViewModel = new SetParametersViewModel(this.configurationService, this);
                    }

                    this.CurrentMode = this.setParametersViewModel;
                    break;

                case WizardMode.ExportConfiguration:
                    if (isNext)
                    {
                        this.CurrentMode = this.exportConfigurationViewModel = new ExportConfigurationViewModel(this.configurationService, this);
                    }

                    this.CurrentMode = this.exportConfigurationViewModel;
                    break;

                default:
                    break;
            }

            this.RaiseCanExecuteChanged();
        }

        private void Next()
        {
            if (!this.CurrentMode.Next())
            {
                return;
            }

            this.ChangeMode(true);
        }

        private void Previous()
        {
            this.CurrentMode.Previous();
            this.ChangeMode(false);
        }

        #endregion
    }
}
