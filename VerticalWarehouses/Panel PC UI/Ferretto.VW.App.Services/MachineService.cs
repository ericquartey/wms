using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Services.Models;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.MAS.AutomationService.Hubs;
using NLog;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;

namespace Ferretto.VW.App.Services
{
    internal sealed class MachineService : BindableBase, IMachineService, IDisposable
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly IMachineCarouselWebService machineCarouselWebService;

        private readonly IMachineElevatorWebService machineElevatorWebService;

        private readonly IMachineLoadingUnitsWebService machineLoadingUnitsWebService;

        private readonly IMachineModeService machineModeService;

        private readonly IMachinePowerWebService machinePowerWebService;

        private readonly INavigationService navigationService;

        private readonly IRegionManager regionManager;

        private readonly ISensorsService sensorsService;

        private readonly IMachineShuttersWebService shuttersWebService;

        private SubscriptionToken bayChainPositionChangedToken;

        private SubscriptionToken elevatorPositionChangedToken;

        private SubscriptionToken fsmExceptionToken;

        private bool isDisposed;

        private bool isHoming;

        private SubscriptionToken machineModeChangedToken;

        private SubscriptionToken machinePowerChangedToken;

        private MachineStatus machineStatus;

        private SubscriptionToken moveLoadingUnitToken;

        private string notification;

        private SubscriptionToken positioningOperationChangedToken;

        private SubscriptionToken receiveHomingUpdateToken;

        private SubscriptionToken shutterPositionToken;

        #endregion

        #region Constructors

        public MachineService(
            IRegionManager regionManager,
            IEventAggregator eventAggregator,
            INavigationService navigationService,
            IMachineElevatorWebService machineElevatorWebService,
            IMachineShuttersWebService shuttersWebService,
            IMachineCarouselWebService machineCarouselWebService,
            IMachineLoadingUnitsWebService machineLoadingUnitsWebService,
            IMachinePowerWebService machinePowerWebService,
            IMachineModeService machineModeService,
            ISensorsService sensorsService)
        {
            this.regionManager = regionManager ?? throw new ArgumentNullException(nameof(regionManager));
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            this.machineElevatorWebService = machineElevatorWebService ?? throw new ArgumentNullException(nameof(machineElevatorWebService));
            this.shuttersWebService = shuttersWebService ?? throw new ArgumentNullException(nameof(shuttersWebService));
            this.machineCarouselWebService = machineCarouselWebService ?? throw new ArgumentNullException(nameof(machineCarouselWebService));
            this.machineLoadingUnitsWebService = machineLoadingUnitsWebService ?? throw new ArgumentNullException(nameof(machineLoadingUnitsWebService));
            this.machinePowerWebService = machinePowerWebService ?? throw new ArgumentNullException(nameof(machinePowerWebService));
            this.machineModeService = machineModeService ?? throw new ArgumentNullException(nameof(machineModeService));
            this.sensorsService = sensorsService ?? throw new ArgumentNullException(nameof(sensorsService));

            this.machineStatus = new MachineStatus();
            this.InitializatioHoming().ConfigureAwait(false);
            this.InitializatioBay().ConfigureAwait(false);

            this.receiveHomingUpdateToken = this.eventAggregator
                    .GetEvent<NotificationEventUI<HomingMessageData>>()
                    .Subscribe(
                        this.OnDataChanged,
                        ThreadOption.UIThread,
                        false);

            this.elevatorPositionChangedToken = this.elevatorPositionChangedToken
                ??
                this.eventAggregator
                    .GetEvent<PubSubEvent<ElevatorPositionChangedEventArgs>>()
                    .Subscribe(
                        this.OnElevatorPositionChanged,
                        ThreadOption.UIThread,
                        false);

            this.bayChainPositionChangedToken = this.bayChainPositionChangedToken
                ??
                this.eventAggregator
                    .GetEvent<PubSubEvent<BayChainPositionChangedEventArgs>>()
                    .Subscribe(
                        this.OnBayChainPositionChanged,
                        ThreadOption.UIThread,
                        false);

            this.positioningOperationChangedToken = this.positioningOperationChangedToken
                ??
                this.eventAggregator
                    .GetEvent<NotificationEventUI<PositioningMessageData>>()
                    .Subscribe(
                        this.OnDataChanged,
                        ThreadOption.UIThread,
                        false);

            this.shutterPositionToken = this.shutterPositionToken
                ??
                this.eventAggregator
                    .GetEvent<NotificationEventUI<ShutterPositioningMessageData>>()
                    .Subscribe(
                        this.OnDataChanged,
                        ThreadOption.UIThread,
                        false);

            this.machineModeChangedToken = this.machineModeChangedToken
                ??
                this.eventAggregator
                    .GetEvent<PubSubEvent<MachineModeChangedEventArgs>>()
                    .Subscribe(
                       this.OnChangedEventArgs,
                       ThreadOption.UIThread,
                       false);

            this.machinePowerChangedToken = this.machinePowerChangedToken
                ??
                this.eventAggregator
                    .GetEvent<PubSubEvent<MachinePowerChangedEventArgs>>()
                    .Subscribe(
                       this.OnChangedEventArgs,
                       ThreadOption.UIThread,
                       false);

            this.navigationService.SubscribeToNavigationCompleted(
               e => this.WarningsManagement(e.ViewModelName.Replace("Model", "")));

            this.moveLoadingUnitToken = this.moveLoadingUnitToken
                ??
                this.eventAggregator
                    .GetEvent<NotificationEventUI<MoveLoadingUnitMessageData>>()
                    .Subscribe(
                        this.OnDataChanged,
                        ThreadOption.UIThread,
                        false);

            this.fsmExceptionToken = this.fsmExceptionToken
                ??
                this.eventAggregator
                    .GetEvent<NotificationEventUI<FsmExceptionMessageData>>()
                    .Subscribe(
                        this.OnDataChanged,
                        ThreadOption.UIThread,
                        false);
        }

        #endregion

        #region Properties

        public bool IsHoming
        {
            get => this.isHoming;
            set => this.SetProperty(ref this.isHoming, value);
        }

        protected NLog.Logger Logger => this.logger;

        public MachineStatus MachineStatus
        {
            get => this.machineStatus;
            set => this.SetProperty(ref this.machineStatus, value, this.MachineStatusNotificationProperty);
        }

        public string Notification
        {
            get => this.notification;
            set => this.SetProperty(ref this.notification, value, () => this.ShowNotification(this.notification, NotificationSeverity.Info));
        }

        #endregion

        #region Methods

        public void ClearNotifications()
        {
            this.eventAggregator
                .GetEvent<PresentationNotificationPubSubEvent>()
                .Publish(new PresentationNotificationMessage(true));
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing).
            this.Dispose(true);
        }

        public void ShowNotification(Exception exception)
        {
            if (exception is null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            this.Logger.Error(exception);

            this.eventAggregator
                .GetEvent<PresentationNotificationPubSubEvent>()
                .Publish(new PresentationNotificationMessage(exception));
        }

        public async Task StopMovingByAllAsync()
        {
            //this.machineLoadingUnitsWebService?.StopAsync();
            this.machineElevatorWebService?.StopAsync();
            this.machineCarouselWebService?.StopAsync();
            this.shuttersWebService?.StopAsync();
            this.StopMoving();
        }

        private void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                this.receiveHomingUpdateToken?.Dispose();
                this.receiveHomingUpdateToken = null;

                this.elevatorPositionChangedToken?.Dispose();
                this.elevatorPositionChangedToken = null;

                this.bayChainPositionChangedToken?.Dispose();
                this.bayChainPositionChangedToken = null;

                this.positioningOperationChangedToken?.Dispose();
                this.positioningOperationChangedToken = null;

                this.shutterPositionToken?.Dispose();
                this.shutterPositionToken = null;

                this.machineModeChangedToken?.Dispose();
                this.machineModeChangedToken = null;

                this.machinePowerChangedToken?.Dispose();
                this.machinePowerChangedToken = null;

                this.moveLoadingUnitToken?.Dispose();
                this.moveLoadingUnitToken = null;

                this.fsmExceptionToken?.Dispose();
                this.fsmExceptionToken = null;
            }

            this.isDisposed = true;
        }

        private string GetActiveView()
        {
            var activeView = this.regionManager.Regions[Utils.Modules.Layout.REGION_MAINCONTENT].ActiveViews.FirstOrDefault();
            return activeView?.GetType()?.Name;
        }

        private async Task InitializatioBay()
        {
            var ms = (MachineStatus)this.MachineStatus.Clone();
            ms.BayChainPosition = await this.machineCarouselWebService.GetPositionAsync();
            this.MachineStatus = ms;
        }

        private async Task InitializatioHoming()
        {
            this.IsHoming = await this.machinePowerWebService.GetIsHomingAsync();

            this.eventAggregator
                .GetEvent<HomingChangedPubSubEvent>()
                .Publish(new HomingChangedMessage(this.IsHoming));
        }

        private void MachineStatusNotificationProperty()
        {
            this.eventAggregator
                .GetEvent<MachineStatusChangedPubSubEvent>()
                .Publish(new MachineStatusChangedMessage(this.MachineStatus));
        }

        private void OnBayChainPositionChanged(BayChainPositionChangedEventArgs e)
        {
            this.UpdateMachineStatus(e);
        }

        private void OnChangedEventArgs(EventArgs e)
        {
            this.WarningsManagement(this.GetActiveView());

            if (e is MachinePowerChangedEventArgs eventPower)
            {
                if (eventPower.MachinePowerState == MachinePowerState.Unpowered)
                {
                    this.StopMoving();
                }
            }
        }

        private void OnDataChanged<TData>(NotificationMessageUI<TData> message)
            where TData : class, IMessageData
        {
            Debug.WriteLine($"message.Status: {message.Status}");

            if (message?.Data is HomingMessageData dataHoming)
            {
                this.machinePowerWebService.GetIsHomingAsync()
                    .ContinueWith((m) =>
                    {
                        if (!m.IsFaulted)
                        {
                            bool isHoming = m.Result;
                            if (isHoming != this.IsHoming ||
                                isHoming && message?.Status == MessageStatus.OperationEnd ||
                                !isHoming && message?.Status == MessageStatus.OperationError)
                            {
                                this.eventAggregator
                                    .GetEvent<HomingChangedPubSubEvent>()
                                    .Publish(new HomingChangedMessage(isHoming));
                            }

                            this.IsHoming = isHoming;
                        }
                    });
            }

            switch (message.Status)
            {
                case MessageStatus.OperationStart:
                case MessageStatus.OperationStepStart:
                    {
                        var ms = (MachineStatus)this.MachineStatus.Clone();

                        ms.IsError = false;
                        ms.IsMoving = true;

                        if (message?.Data is PositioningMessageData dataPositioning)
                        {
                            ms.IsMovingElevator = true;

                            this.WriteInfo(dataPositioning?.AxisMovement);
                        }

                        if (message?.Data is ShutterPositioningMessageData)
                        {
                            ms.IsMovingShutter = true;
                        }

                        this.MachineStatus = ms;
                        break;
                    }

                case MessageStatus.OperationExecuting:
                    {
                        if (this.MachineStatus.IsMoving)
                        {
                            if (message?.Data is PositioningMessageData dataPositioningInfo)
                            {
                                this.WriteInfo(dataPositioningInfo?.AxisMovement);
                            }
                        }
                        break;
                    }

                case MessageStatus.OperationEnd:
                case MessageStatus.OperationStop:
                case MessageStatus.OperationStepStop:
                    {
                        var ms = (MachineStatus)this.MachineStatus.Clone();

                        ms.IsMoving = false;

                        if (message?.Data is PositioningMessageData)
                        {
                            ms.IsMovingElevator = false;
                        }

                        if (message?.Data is ShutterPositioningMessageData)
                        {
                            ms.IsMovingShutter = false;
                        }

                        this.ClearNotifications();

                        this.MachineStatus = ms;
                        break;
                    }

                case MessageStatus.OperationError:
                    {
                        var ms = (MachineStatus)this.MachineStatus.Clone();

                        ms.IsMoving = false;
                        ms.IsError = true;
                        ms.ErrorDescription = message.Description;

                        if (message?.Data is PositioningMessageData)
                        {
                            ms.IsMovingElevator = false;
                        }

                        if (message?.Data is ShutterPositioningMessageData)
                        {
                            ms.IsMovingShutter = false;
                        }

                        this.ShowNotification(message.Description, NotificationSeverity.Error);

                        this.MachineStatus = ms;
                        break;
                    }
            }

            this.WarningsManagement(this.GetActiveView());
        }

        private void OnElevatorPositionChanged(ElevatorPositionChangedEventArgs e)
        {
            this.UpdateMachineStatus(e);
        }

        private void ShowNotification(string message, NotificationSeverity severity = NotificationSeverity.Info)
        {
            this.eventAggregator
             .GetEvent<PresentationNotificationPubSubEvent>()
             .Publish(new PresentationNotificationMessage(message, severity));
        }

        private void StopMoving()
        {
            var ms = (MachineStatus)this.MachineStatus.Clone();

            ms.IsMoving = false;
            ms.IsError = false;
            ms.IsMovingElevator = false;
            ms.IsMovingShutter = false;
            ms.ErrorDescription = string.Empty;

            this.MachineStatus = ms;
        }

        private void UpdateMachineStatus(EventArgs e)
        {
            if (e is ElevatorPositionChangedEventArgs dataElevatorPosition)
            {
                this.sensorsService.RetrieveElevatorPosition(
                       new ElevatorPosition
                       {
                           Horizontal = dataElevatorPosition.HorizontalPosition,
                           Vertical = dataElevatorPosition.VerticalPosition,
                           BayPositionId = dataElevatorPosition.BayPositionId,
                           CellId = dataElevatorPosition.CellId,
                           BayPositionUpper = dataElevatorPosition.BayPositionUpper
                       });
                if (this.MachineStatus.ElevatorLogicalPosition != this.sensorsService?.ElevatorLogicalPosition)
                {
                    var ms = (MachineStatus)this.MachineStatus.Clone();

                    ms.ElevatorLogicalPosition = this.sensorsService?.ElevatorLogicalPosition;
                    this.MachineStatus = ms;
                }
            }

            if (e is BayChainPositionChangedEventArgs dataBayChainPosition)
            {
                if (this.MachineStatus.BayChainPosition == dataBayChainPosition.Position)
                {
                    var ms = (MachineStatus)this.MachineStatus.Clone();
                    ms.BayChainPosition = dataBayChainPosition.Position;
                    this.MachineStatus = ms;
                }
            }
        }

        private void WarningsManagement(string view)
        {
            if (!(view is null) && !this.MachineStatus.IsMoving)
            {
                switch (true)
                {
                    case var b1 when view.Equals("LoginView", StringComparison.InvariantCultureIgnoreCase):
                    case var b2 when view.Equals("LoaderView", StringComparison.InvariantCultureIgnoreCase):
                    case var b3 when view.Equals("MainMenuView", StringComparison.InvariantCultureIgnoreCase):
                        this.ClearNotifications();
                        break;

                    default:
                        if (this.machineModeService.MachinePower != MachinePowerState.Powered)
                        {
                            this.ShowNotification("Manca marcia.", NotificationSeverity.Warning);
                        }
                        else if (!this.IsHoming)
                        {
                            this.ShowNotification("Homing non eseguito.", NotificationSeverity.Error);
                        }
                        else if (this.MachineStatus.BayChainPosition is null)
                        {
                            this.ShowNotification("Posizione catena sconosciuta.", NotificationSeverity.Error);
                        }
                        else if (string.IsNullOrEmpty(this.MachineStatus.ElevatorLogicalPosition))
                        {
                            this.ShowNotification("Posizione elevatore sconosciuta.", NotificationSeverity.Low);
                        }
                        else
                        {
                            this.ClearNotifications();
                        }
                        break;
                }
            }
            else
            {
                this.ClearNotifications();
            }
        }

        private void WriteInfo(Axis? axisMovement)
        {
            if (this.machineModeService.MachineMode == MachineMode.Manual)
            {
                if (axisMovement.HasValue && axisMovement == Axis.Vertical)
                {
                    this.Notification = "Movimento verticale in corso...";
                }
                else if (axisMovement.HasValue && axisMovement == Axis.Horizontal)
                {
                    this.Notification = "Movimento orizzontale in corso...";
                }
                else if (axisMovement.HasValue && axisMovement == Axis.BayChain)
                {
                    this.Notification = "Movimento catena baia in corso...";
                }
            }
            else
            {
                this.Notification = "Movimento in corso...";
            }
        }

        #endregion
    }
}
