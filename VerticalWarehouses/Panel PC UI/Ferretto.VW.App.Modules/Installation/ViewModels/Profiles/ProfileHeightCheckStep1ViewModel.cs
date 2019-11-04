using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.App.Modules.Installation.Models;
using Ferretto.VW.App.Services;
using Ferretto.VW.App.Services.Models;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using ShutterMovementDirection = Ferretto.VW.MAS.AutomationService.Contracts.ShutterMovementDirection;

namespace Ferretto.VW.App.Installation.ViewModels
{
    internal sealed class ProfileHeightCheckStep1ViewModel : BaseProfileHeightCheckViewModel
    {
        #region Fields

        private readonly IMachineSensorsWebService machineSensorsWebService;

        private readonly IMachineShuttersWebService shuttersService;

        private DelegateCommand closedCommand;

        private DelegateCommand openCommand;

        private ShutterSensors sensors;

        private SubscriptionToken sensorsChangedToken;

        private SubscriptionToken shutterPositionToken;

        private DelegateCommand stopCommand;

        #endregion

        #region Constructors

        public ProfileHeightCheckStep1ViewModel(
            IMachineShuttersWebService shuttersService,
            IEventAggregator eventAggregator,
            IMachineProfileProcedureWebService profileProcedureService,
            IMachineModeService machineModeService,
            IMachineSensorsWebService machineSensorsWebService,
            IBayManager bayManager)
            : base(eventAggregator, profileProcedureService, machineModeService, bayManager)
        {
            this.shuttersService = shuttersService ?? throw new ArgumentNullException(nameof(shuttersService));
            this.machineSensorsWebService = machineSensorsWebService ?? throw new ArgumentNullException(nameof(machineSensorsWebService));
        }

        #endregion

        #region Properties

        public ICommand ClosedCommand =>
                    this.closedCommand
            ??
            (this.closedCommand = new DelegateCommand(
                async () => await this.ClosedAsync(),
                this.CanExecuteClosedCommand));

        public ICommand OpenCommand =>
            this.openCommand
            ??
            (this.openCommand = new DelegateCommand(
                async () => await this.OpenAsync(),
                this.CanExecuteOpenCommand));

        public ShutterSensors Sensors => this.sensors;

        public ICommand StopCommand =>
            this.stopCommand
            ??
            (this.stopCommand = new DelegateCommand(
                async () => await this.StopAsync(),
                this.CanExecuteStopCommand));

        private bool IsCanExecuteStepCommand => !this.IsExecutingProcedure
                && !this.IsWaitingForResponse
                && string.IsNullOrWhiteSpace(this.Error)
                && this.sensors != null
                && this.sensors.Open;

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.currentStep = ProfileHeightCheckStep.Initialize;

            this.sensorsChangedToken = this.sensorsChangedToken
                ??
                this.EventAggregator
                    .GetEvent<NotificationEventUI<SensorsChangedMessageData>>()
                    .Subscribe(
                        this.OnSensorsChanged,
                        ThreadOption.UIThread,
                        false,
                        m => m.Data != null);

            this.shutterPositionToken = this.shutterPositionToken
                ??
                this.EventAggregator
                    .GetEvent<NotificationEventUI<ShutterPositioningMessageData>>()
                    .Subscribe(
                        this.OnShutterPositionChanged,
                        ThreadOption.UIThread,
                        false);

            try
            {
                this.sensors = new ShutterSensors((int)this.BayNumber);

                var sensorsStates = await this.machineSensorsWebService.GetAsync();
                this.sensors.Update(sensorsStates.ToArray());

                this.RaiseCanExecuteChanged();
            }
            catch (System.Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();
            this.openCommand?.RaiseCanExecuteChanged();
            this.closedCommand?.RaiseCanExecuteChanged();
            this.stopCommand?.RaiseCanExecuteChanged();

            this.ShowNextStep(true, this.IsCanExecuteStepCommand, nameof(Utils.Modules.Installation), Utils.Modules.Installation.ProfileHeightCheck.STEP2);

            this.RaisePropertyChanged(nameof(this.Sensors));
        }

        protected override void ShowSteps()
        {
            this.ShowPrevStep(false, false);
            this.ShowNextStep(true, this.IsCanExecuteStepCommand, nameof(Utils.Modules.Installation), Utils.Modules.Installation.ProfileHeightCheck.STEP2);
            this.ShowAbortStep(true, true);
        }

        private bool CanExecuteClosedCommand()
        {
            return !this.IsWaitingForResponse
                 && !this.IsExecutingProcedure
                 && string.IsNullOrWhiteSpace(this.Error)
                 && this.sensors != null
                 && this.sensors.Open;
        }

        private bool CanExecuteOpenCommand()
        {
            return !this.IsWaitingForResponse
                 && !this.IsExecutingProcedure
                 && string.IsNullOrWhiteSpace(this.Error)
                 && this.sensors != null
                 && this.sensors.Closed;
        }

        private bool CanExecuteStopCommand()
        {
            return !this.IsWaitingForResponse
                 && this.IsExecutingProcedure;
        }

        private async Task ClosedAsync()
        {
            this.IsWaitingForResponse = true;

            try
            {
                await this.shuttersService.MoveAsync(ShutterMovementDirection.Down);

                this.IsExecutingProcedure = true;
            }
            catch (System.Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private void OnSensorsChanged(NotificationMessageUI<SensorsChangedMessageData> message)
        {
            this.sensors.Update(message.Data.SensorsStates);
            this.RaiseCanExecuteChanged();
        }

        private void OnShutterPositionChanged(NotificationMessageUI<ShutterPositioningMessageData> message)
        {
            switch (message.Status)
            {
                case CommonUtils.Messages.Enumerations.MessageStatus.OperationStart:
                case CommonUtils.Messages.Enumerations.MessageStatus.OperationExecuting:
                    {
                        this.IsExecutingProcedure = true;
                        break;
                    }

                case CommonUtils.Messages.Enumerations.MessageStatus.OperationEnd:
                case CommonUtils.Messages.Enumerations.MessageStatus.OperationError:
                case CommonUtils.Messages.Enumerations.MessageStatus.OperationStop:
                    {
                        this.IsExecutingProcedure = false;
                        break;
                    }
            }
        }

        private async Task OpenAsync()
        {
            this.IsWaitingForResponse = true;

            try
            {
                await this.shuttersService.MoveAsync(ShutterMovementDirection.Up);

                this.IsExecutingProcedure = true;
            }
            catch (System.Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private async Task StopAsync()
        {
            this.IsWaitingForResponse = true;

            try
            {
                await this.shuttersService.StopAsync();
            }
            catch (System.Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        #endregion
    }
}
