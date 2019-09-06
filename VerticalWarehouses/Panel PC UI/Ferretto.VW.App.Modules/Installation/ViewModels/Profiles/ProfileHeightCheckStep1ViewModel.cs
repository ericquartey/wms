using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Modules.Installation.Models;
using Ferretto.VW.App.Services;
using Ferretto.VW.App.Services.Interfaces;
using Ferretto.VW.App.Services.Models;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.MAStoUIMessages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;
using Prism.Events;
using ShutterMovementDirection = Ferretto.VW.MAS.AutomationService.Contracts.ShutterMovementDirection;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public class ProfileHeightCheckStep1ViewModel : BaseProfileHeightCheckViewModel
    {
        #region Fields

        private readonly IMachineSensorsService machineSensorsService;

        private readonly ShutterSensors sensors;

        private readonly IMachineShuttersService shuttersService;

        private DelegateCommand closedCommand;

        private DelegateCommand openCommand;

        private SubscriptionToken receivedActionUpdateErrorToken;

        private SubscriptionToken receivedSensorsToken;

        #endregion

        #region Constructors

        public ProfileHeightCheckStep1ViewModel(
            IMachineShuttersService shuttersService,
            IMachineSensorsService machineSensorsService,
            IEventAggregator eventAggregator,
            IMachineProfileProcedureService profileProcedureService,
            IMachineModeService machineModeService,
            IBayManager bayManager)
            : base(eventAggregator, profileProcedureService, machineModeService, bayManager)
        {
            if (shuttersService is null)
            {
                throw new ArgumentNullException(nameof(shuttersService));
            }

            if (machineSensorsService is null)
            {
                throw new ArgumentNullException(nameof(machineSensorsService));
            }

            this.machineSensorsService = machineSensorsService;
            this.shuttersService = shuttersService;
            this.sensors = new ShutterSensors(this.BayNumber);
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

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();

            if (this.receivedSensorsToken != null)
            {
                this.EventAggregator
                    .GetEvent<NotificationEventUI<SensorsChangedMessageData>>()
                    .Unsubscribe(this.receivedSensorsToken);

                this.receivedSensorsToken = null;
            }

            if (this.receivedActionUpdateErrorToken != null)
            {
                this.EventAggregator
                 .GetEvent<MachineAutomationErrorPubSubEvent>()
                 .Unsubscribe(this.receivedActionUpdateErrorToken);

                this.receivedActionUpdateErrorToken = null;
            }
        }

        public override async Task OnNavigatedAsync()
        {
            await base.OnNavigatedAsync();

            this.receivedActionUpdateErrorToken = this.EventAggregator
                .GetEvent<MachineAutomationErrorPubSubEvent>()
                .Subscribe(
                    msg => this.UpdateError(),
                    ThreadOption.UIThread,
                    false,
                    message =>
                    message.NotificationType == NotificationType.Error &&
                    message.ActionType == ActionType.ShutterPositioning &&
                    message.ActionStatus == ActionStatus.Error);

            this.receivedSensorsToken = this.EventAggregator
                .GetEvent<NotificationEventUI<SensorsChangedMessageData>>()
                .Subscribe(
                    message => this.UpdateLayout(message?.Data?.SensorsStates),
                    ThreadOption.UIThread,
                    false);

            try
            {
                var sensorsStates = await this.machineSensorsService.GetAsync();
                this.UpdateLayout(sensorsStates);
            }
            catch (System.Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        protected override bool CanExecuteStepCommand()
        {
            return base.CanExecuteStepCommand()
                && this.Sensors.Open;
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();
            this.openCommand.RaiseCanExecuteChanged();
            this.closedCommand.RaiseCanExecuteChanged();
        }

        private bool CanExecuteClosedCommand()
        {
            return !this.IsWaitingForResponse
                && string.IsNullOrWhiteSpace(this.Error)
                && this.Sensors.Open;
        }

        private bool CanExecuteOpenCommand()
        {
            return !this.IsWaitingForResponse
                && string.IsNullOrWhiteSpace(this.Error)
                && this.Sensors.Closed;
        }

        private async Task ClosedAsync()
        {
            this.IsWaitingForResponse = true;

            try
            {
                await this.shuttersService.MoveAsync(this.BayNumber, ShutterMovementDirection.Down);
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

        private async Task OpenAsync()
        {
            this.IsWaitingForResponse = true;

            try
            {
                await this.shuttersService.MoveAsync(this.BayNumber, ShutterMovementDirection.Up);
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

        private void UpdateLayout(System.Collections.Generic.IEnumerable<bool> sensorsStates)
        {
            this.sensors.Update(sensorsStates.ToArray());
            this.RaiseCanExecuteChanged();
        }

        #endregion
    }
}
