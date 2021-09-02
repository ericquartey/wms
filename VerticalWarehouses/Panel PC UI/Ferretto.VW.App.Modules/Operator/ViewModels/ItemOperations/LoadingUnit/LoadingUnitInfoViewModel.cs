using System;
using System.Threading.Tasks;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.Devices.LaserPointer;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    public class LoadingUnitInfoViewModel : BaseLoadingUnitViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly IOperatorNavigationService operatorNavigationService;

        private readonly Sensors sensors = new Sensors();

        private int count;

        private SubscriptionToken positioningMessageReceivedToken;

        private SubscriptionToken subscriptionToken;

        #endregion

        #region Constructors

        public LoadingUnitInfoViewModel(
            IMachineIdentityWebService machineIdentityWebService,
            IMachineLoadingUnitsWebService machineLoadingUnitsWebService,
            IOperatorNavigationService operatorNavigationService,
            IMissionOperationsService missionOperationsService,
            IEventAggregator eventAggregator,
            IBayManager bayManager,
            ILaserPointerDriver laserPointerDriver,
            ISessionService sessionService,
            IWmsDataProvider wmsDataProvider)
            : base(machineIdentityWebService, machineLoadingUnitsWebService, missionOperationsService, eventAggregator, bayManager, laserPointerDriver, sessionService, wmsDataProvider)
        {
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.operatorNavigationService = operatorNavigationService ?? throw new ArgumentNullException(nameof(operatorNavigationService));
        }

        #endregion

        #region Properties

        public Sensors Sensors => this.sensors;

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();

            if (this.positioningMessageReceivedToken != null)
            {
                this.EventAggregator.GetEvent<StepChangedPubSubEvent>().Unsubscribe(this.positioningMessageReceivedToken);
                this.positioningMessageReceivedToken.Dispose();
                this.positioningMessageReceivedToken = null;
            }

            if (this.subscriptionToken != null)
            {
                this.EventAggregator.GetEvent<StepChangedPubSubEvent>().Unsubscribe(this.subscriptionToken);
                this.subscriptionToken.Dispose();
                this.subscriptionToken = null;
            }

            this.count = 0;
        }

        public override async Task OnAppearedAsync()
        {
            this.SubscribeToEvents();

            this.count = 0;

            await base.OnAppearedAsync();
        }

        public override void RaisePropertyChanged()
        {
            base.RaisePropertyChanged();
        }

        protected override async Task OnDataRefreshAsync()
        {
            await base.OnDataRefreshAsync();
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();
        }

        private void ChangePage()
        {
            this.logger.Debug($"Change page");
            this.NavigationService.GoBack();
            this.Reset();

            this.operatorNavigationService.NavigateToDrawerView();
        }

        private void OnPositioningMessageReceived(NotificationMessageUI<PositioningMessageData> message)
        {
            try
            {
                if (message.Status == MessageStatus.OperationStop || message.Status == MessageStatus.OperationError)
                {
                    this.ChangePage();
                }

                if (message.Data != null &&
                    message.Data.LoadingUnitId.HasValue &&
                    this.LoadingUnit != null &&
                    message.Data.LoadingUnitId == this.LoadingUnit.Id)
                {
                    if (message.Data.AxisMovement == CommonUtils.Messages.Enumerations.Axis.Horizontal)
                    {
                        if (message.Status == MessageStatus.OperationEnd)
                        {
                            this.count++;
                            if (this.count == 2)
                            {
                                this.ChangePage();
                                this.RaiseCanExecuteChanged();
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // do nothing
            }
        }

        private void OnSensorsChanged(NotificationMessageUI<SensorsChangedMessageData> message)
        {
            this.sensors.Update(message.Data.SensorsStates);
        }

        private void SubscribeToEvents()
        {
            this.subscriptionToken = this.subscriptionToken
                ??
                this.EventAggregator
                    .GetEvent<NotificationEventUI<SensorsChangedMessageData>>()
                    .Subscribe(
                        this.OnSensorsChanged,
                        ThreadOption.UIThread,
                        false,
                        m => m.Data?.SensorsStates != null);

            this.positioningMessageReceivedToken = this.positioningMessageReceivedToken
              ??
              this.eventAggregator
                  .GetEvent<NotificationEventUI<PositioningMessageData>>()
                  .Subscribe(
                      this.OnPositioningMessageReceived,
                      ThreadOption.UIThread,
                      false);
        }

        #endregion
    }
}
