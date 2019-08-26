using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Modules.Installation.Models;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public class VerticalResolutionCalibrationStep1ViewModel : BaseVerticalResolutionCalibrationViewModel, IDataErrorInfo
    {
        #region Fields

        private ResolutionCalibrationParameters defaultParameters;

        private decimal? inputInitialPosition;

        private DelegateCommand startCommand;

        #endregion

        #region Constructors

        public VerticalResolutionCalibrationStep1ViewModel(
            IEventAggregator eventAggregator,
            IMachineResolutionCalibrationProcedureService resolutionCalibrationService)
            : base(eventAggregator, resolutionCalibrationService)
        {
        }

        #endregion

        #region Properties

        public string Error => string.Join(
              System.Environment.NewLine,
              this[nameof(this.InputInitialPosition)]);

        public decimal? InputInitialPosition
        {
            get => this.inputInitialPosition;
            set
            {
                if (this.SetProperty(ref this.inputInitialPosition, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand StartCommand =>
            this.startCommand
            ??
            (this.startCommand = new DelegateCommand(
                async () => await this.ExecuteStartCommandAsync(),
                this.CanExecuteStartCommand));

        #endregion

        #region Indexers

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(this.InputInitialPosition):
                        if (!this.InputInitialPosition.HasValue)
                        {
                            return $"InputInitialPosition is required.";
                        }

                        if (this.InputInitialPosition.Value <= 0)
                        {
                            return "InputInitialPosition must be strictly positive.";
                        }
                        break;
                }

                return null;
            }
        }

        #endregion

        #region Methods

        public async Task GetParametersAsync()
        {
            try
            {
                this.defaultParameters = await this.ResolutionCalibrationService.GetParametersAsync();

                this.CurrentResolution = this.defaultParameters.CurrentResolution;
                this.InputInitialPosition = this.defaultParameters.InitialPosition;
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        public override async Task OnNavigatedAsync()
        {
            await base.OnNavigatedAsync();

            await this.GetParametersAsync();
        }

        protected override void OnAutomationMessageReceived(NotificationMessageUI<PositioningMessageData> message)
        {
            if (message.Status == MessageStatus.OperationEnd
                ||
                message.Status == MessageStatus.OperationStop) // TODO why OperationStop as well and not only OperationEnd?
            {
                this.IsExecutingProcedure = false;

                this.NavigateToNextStep();
            }
        }

        protected override void RaiseCanExecuteChanged()
        {
            this.startCommand?.RaiseCanExecuteChanged();
        }

        private bool CanExecuteStartCommand()
        {
            return !this.IsExecutingProcedure
              && !this.IsWaitingForResponse
              && string.IsNullOrWhiteSpace(this.Error);
        }

        private async Task ExecuteStartCommandAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;
                this.IsExecutingProcedure = true;

                await this.ResolutionCalibrationService.StartAsync(this.InputInitialPosition.Value);
            }
            catch (Exception ex)
            {
                this.IsExecutingProcedure = false;
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private void NavigateToNextStep()
        {
            var procedureParameters = new VerticalResolutionCalibrationData
            {
                CurrentResolution = this.CurrentResolution.Value,
                FinalPosition = this.defaultParameters.FinalPosition,
                InitialPosition = this.InputInitialPosition.Value
            };

            this.NavigationService.Appear(
                nameof(Utils.Modules.Installation),
                Utils.Modules.Installation.VerticalResolutionCalibration.STEP2,
                procedureParameters,
                trackCurrentView: false);
        }

        #endregion
    }
}
