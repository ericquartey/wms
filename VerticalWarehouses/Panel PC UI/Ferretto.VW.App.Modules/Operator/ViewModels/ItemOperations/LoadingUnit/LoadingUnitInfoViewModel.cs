﻿using System;
using System.Threading.Tasks;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    public class LoadingUnitInfoViewModel : BaseLoadingUnitViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly IMachineMissionsWebService machineMissionsWebService;

        private readonly IMachineSensorsWebService machineSensorsWebService;

        private readonly IMachineService machineService;

        private readonly IMissionOperationsService missionOperationsService;

        private readonly IOperatorNavigationService operatorNavigationService;

        private readonly Sensors sensors = new Sensors();

        private int count;

        private SubscriptionToken positioningMessageReceivedToken;

        private SubscriptionToken subscriptionToken;

        #endregion

        #region Constructors

        public LoadingUnitInfoViewModel(IMachineLoadingUnitsWebService machineLoadingUnitsWebService,
            IMachineMissionsWebService machineMissionsWebService,
            IMachineSensorsWebService machineSensorsWebService,
            IOperatorNavigationService operatorNavigationService,
            IMachineService machineService,
            IMissionOperationsService missionOperationsService, IEventAggregator eventAggregator,
            IWmsDataProvider wmsDataProvider)
            : base(machineLoadingUnitsWebService, missionOperationsService, eventAggregator, wmsDataProvider)
        {
            this.machineSensorsWebService = machineSensorsWebService ?? throw new System.ArgumentNullException(nameof(machineSensorsWebService));
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.machineService = machineService ?? throw new ArgumentNullException(nameof(machineService));
            this.machineMissionsWebService = machineMissionsWebService ?? throw new ArgumentNullException(nameof(machineMissionsWebService));
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
            this.NavigationService.GoBack();
            this.Reset();

            this.operatorNavigationService.NavigateToDrawerView();
        }

        private async void OnPositioningMessageReceived(NotificationMessageUI<PositioningMessageData> message)
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
