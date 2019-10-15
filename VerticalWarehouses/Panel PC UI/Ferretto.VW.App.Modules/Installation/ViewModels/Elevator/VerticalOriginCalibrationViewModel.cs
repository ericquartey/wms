﻿using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Prism.Commands;
using Prism.Events;
using Axis = Ferretto.VW.CommonUtils.Messages.Enumerations.Axis;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public class VerticalOriginCalibrationViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IMachineElevatorWebService machineElevatorWebService;

        private readonly IMachineVerticalOriginProcedureWebService verticalOriginProcedureWebService;

        private double? currentHorizontalPosition;

        private double? currentVerticalPosition;

        private bool isExecutingProcedure;

        private bool isExecutingVerticalOperation;

        private bool isWaitingForResponse;

        private double lowerBound;

        private string noteString;

        private double offset;

        private SubscriptionToken receivedCalibrateAxisUpdateToken;

        private SubscriptionToken receivedSwitchAxisUpdateToken;

        private SubscriptionToken receiveHomingUpdateToken;

        private decimal resolution;

        private DelegateCommand startCommand;

        private DelegateCommand stopCommand;

        private SubscriptionToken updateCurrentPositionToken;

        private double upperBound;

        #endregion

        #region Constructors

        public VerticalOriginCalibrationViewModel(
            IMachineVerticalOriginProcedureWebService verticalOriginProcedureWebService,
            IMachineElevatorWebService machineElevatorWebService)
            : base(Services.PresentationMode.Installer)
        {
            if (verticalOriginProcedureWebService is null)
            {
                throw new ArgumentNullException(nameof(verticalOriginProcedureWebService));
            }

            if (machineElevatorWebService is null)
            {
                throw new ArgumentNullException(nameof(machineElevatorWebService));
            }

            this.verticalOriginProcedureWebService = verticalOriginProcedureWebService;
            this.machineElevatorWebService = machineElevatorWebService;
        }

        #endregion

        #region Properties

        public double? CurrentHorizontalPosition
        {
            get => this.currentHorizontalPosition;
            private set => this.SetProperty(ref this.currentHorizontalPosition, value);
        }

        public double? CurrentVerticalPosition
        {
            get => this.currentVerticalPosition;
            private set => this.SetProperty(ref this.currentVerticalPosition, value);
        }

        public bool IsExecutingProcedure
        {
            get => this.isExecutingProcedure;
            set
            {
                if (this.SetProperty(ref this.isExecutingProcedure, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsWaitingForResponse
        {
            get => this.isWaitingForResponse;
            set
            {
                if (this.SetProperty(ref this.isWaitingForResponse, value))
                {
                    if (this.isWaitingForResponse)
                    {
                        this.ClearNotifications();
                    }

                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public double LowerBound
        {
            get => this.lowerBound;
            set => this.SetProperty(ref this.lowerBound, value);
        }

        public string NoteString
        {
            get => this.noteString;
            set => this.SetProperty(ref this.noteString, value);
        }

        public double Offset
        {
            get => this.offset;
            set => this.SetProperty(ref this.offset, value);
        }

        public decimal Resolution
        {
            get => this.resolution;
            set => this.SetProperty(ref this.resolution, value);
        }

        public ICommand StartCommand =>
            this.startCommand
            ??
            (this.startCommand = new DelegateCommand(
                async () => await this.StartAsync(),
                this.CanExecuteStartCommand));

        public ICommand StopCommand =>
            this.stopCommand
            ??
            (this.stopCommand = new DelegateCommand(
                async () => await this.StopAsync(),
                this.CanExecuteStopCommand));

        public double UpperBound
        {
            get => this.upperBound;
            set => this.SetProperty(ref this.upperBound, value);
        }

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();

            this.EventAggregator
                .GetEvent<NotificationEventUI<SwitchAxisMessageData>>()
                .Unsubscribe(this.receivedSwitchAxisUpdateToken);

            this.EventAggregator
                .GetEvent<NotificationEventUI<CalibrateAxisMessageData>>()
                .Unsubscribe(this.receivedCalibrateAxisUpdateToken);

            this.EventAggregator
                .GetEvent<NotificationEventUI<HomingMessageData>>()
                .Unsubscribe(this.receiveHomingUpdateToken);
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.IsBackNavigationAllowed = true;

            await this.RetrieveProcedureInformationAsync();

            this.SubscribeToEvents();
        }

        public async Task RetrieveProcedureInformationAsync()
        {
            try
            {
                var procedureParameters = await this.verticalOriginProcedureWebService.GetParametersAsync();

                this.UpperBound = procedureParameters.UpperBound;
                this.LowerBound = procedureParameters.LowerBound;
                this.Offset = procedureParameters.Offset;
                this.Resolution = procedureParameters.Resolution;

                this.CurrentVerticalPosition = await this.machineElevatorWebService.GetVerticalPositionAsync();
                this.CurrentHorizontalPosition = await this.machineElevatorWebService.GetHorizontalPositionAsync();
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        protected override void OnMachineModeChanged(MachineModeChangedEventArgs e)
        {
            base.OnMachineModeChanged(e);
            if (e.MachinePower == Services.Models.MachinePowerState.Unpowered)
            {
                this.IsExecutingProcedure = false;
                this.IsWaitingForResponse = false;
            }
        }

        private bool CanExecuteStartCommand()
        {
            return !this.isExecutingProcedure
                &&
                !this.IsWaitingForResponse;
        }

        private bool CanExecuteStopCommand()
        {
            return this.isExecutingProcedure
                &&
                !this.IsWaitingForResponse;
        }

        private string GetStringByCalibrateAxisMessageData(Axis axisToCalibrate, MessageStatus status)
        {
            var res = string.Empty;
            switch (status)
            {
                case MessageStatus.OperationExecuting:
                    res = axisToCalibrate == Axis.Horizontal ?
                        VW.App.Resources.InstallationApp.HorizontalHomingExecuting :
                        VW.App.Resources.InstallationApp.VerticalHomingExecuting;
                    break;

                case MessageStatus.OperationError:
                    res = axisToCalibrate == Axis.Horizontal ?
                        VW.App.Resources.InstallationApp.HorizontalHomingError :
                        VW.App.Resources.InstallationApp.VerticalHomingError;
                    break;
            }

            return res;
        }

        private void OnAxisSwitched(NotificationMessageUI<SwitchAxisMessageData> message)
        {
            if (message is null || message.Data is null)
            {
                return;
            }

            switch (message.Status)
            {
                case MessageStatus.OperationStart:
                    this.ShowNotification(VW.App.Resources.InstallationApp.SwitchEngineStarted);
                    break;

                case MessageStatus.OperationEnd:
                    this.ShowNotification(VW.App.Resources.InstallationApp.SwitchEngineCompleted);

                    break;

                case MessageStatus.OperationError:
                    this.ShowNotification(VW.App.Resources.InstallationApp.SwitchEngineError);

                    break;
            }
        }

        private void OnCalibrationStepCompleted(NotificationMessageUI<CalibrateAxisMessageData> message)
        {
            if (message.Status == MessageStatus.OperationExecuting)
            {
                this.isExecutingVerticalOperation = !(message.Data.AxisToCalibrate == Axis.Horizontal);

                this.ShowNotification(
                    string.Format(
                        this.GetStringByCalibrateAxisMessageData(message.Data.AxisToCalibrate, message.Status),
                        message.Data.CurrentStepCalibrate,
                        message.Data.MaxStepCalibrate));
            }

            if (message.Status == MessageStatus.OperationError)
            {
                this.isExecutingVerticalOperation = !(message.Data.AxisToCalibrate == Axis.Horizontal);

                this.ShowNotification(message.Description);

                this.IsExecutingProcedure = false;
            }
        }

        private void OnHomingProcedureStatusChanged(NotificationMessageUI<HomingMessageData> message)
        {
            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            this.isExecutingVerticalOperation = message.Data.AxisToCalibrate == Axis.Vertical;

            switch (message.Status)
            {
                case MessageStatus.OperationStart:
                    this.ShowNotification(VW.App.Resources.InstallationApp.HorizontalHomingStarted);

                    this.IsWaitingForResponse = false;
                    this.IsExecutingProcedure = true;
                    break;

                case MessageStatus.OperationEnd:
                    this.ShowNotification(
                        VW.App.Resources.InstallationApp.HorizontalHomingCompleted,
                        Services.Models.NotificationSeverity.Success);

                    this.IsWaitingForResponse = false;
                    this.IsExecutingProcedure = false;
                    break;

                case MessageStatus.OperationError:
                    this.ShowNotification(
                        VW.App.Resources.InstallationApp.HorizontalHomingError,
                        Services.Models.NotificationSeverity.Error);

                    this.IsWaitingForResponse = false;
                    this.IsExecutingProcedure = false;
                    break;
            }
        }

        private void RaiseCanExecuteChanged()
        {
            this.startCommand.RaiseCanExecuteChanged();
            this.stopCommand.RaiseCanExecuteChanged();
        }

        private async Task StartAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;
                this.isExecutingVerticalOperation = false;

                await this.verticalOriginProcedureWebService.StartAsync();

                this.IsExecutingProcedure = true;
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
                this.IsExecutingProcedure = false;
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private async Task StopAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                await this.verticalOriginProcedureWebService.StopAsync();

                this.ShowNotification(
                    VW.App.Resources.InstallationApp.SetOriginVerticalAxisNotCompleted,
                    Services.Models.NotificationSeverity.Warning);
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
                this.IsExecutingProcedure = false;
            }
            finally
            {
                this.IsWaitingForResponse = false; // TODO missing notification from service, to confirm abort of operation
                this.IsExecutingProcedure = false;
            }
        }

        private void SubscribeToEvents()
        {
            this.receivedSwitchAxisUpdateToken = this.EventAggregator
                .GetEvent<NotificationEventUI<SwitchAxisMessageData>>()
                .Subscribe(
                    this.OnAxisSwitched,
                    ThreadOption.UIThread,
                    false);

            this.receivedCalibrateAxisUpdateToken = this.EventAggregator
                .GetEvent<NotificationEventUI<CalibrateAxisMessageData>>()
                .Subscribe(
                    this.OnCalibrationStepCompleted,
                    ThreadOption.UIThread,
                    false);

            this.receiveHomingUpdateToken = this.EventAggregator
                .GetEvent<NotificationEventUI<HomingMessageData>>()
                .Subscribe(
                    this.OnHomingProcedureStatusChanged,
                    ThreadOption.UIThread,
                    false);

            this.updateCurrentPositionToken = this.EventAggregator
                .GetEvent<NotificationEventUI<PositioningMessageData>>()
                .Subscribe(
                    this.UpdatePositions,
                    ThreadOption.UIThread,
                    false);
        }

        private void UpdatePositions(NotificationMessageUI<PositioningMessageData> message)
        {
            if (message is null
                ||
                message.Data is null)
            {
                return;
            }

            switch (message.Data.AxisMovement)
            {
                case Axis.Horizontal:
                    this.CurrentHorizontalPosition = message.Data.CurrentPosition;
                    break;

                case Axis.Vertical:
                    this.CurrentVerticalPosition = message.Data.CurrentPosition;
                    break;
            }
        }

        #endregion
    }
}
