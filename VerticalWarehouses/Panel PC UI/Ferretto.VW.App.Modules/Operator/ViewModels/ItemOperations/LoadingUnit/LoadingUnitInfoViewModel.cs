using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    public class LoadingUnitInfoViewModel : BaseLoadingUnitViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly IMachineMissionsWebService machineMissionsWebService;

        private readonly IMachineSensorsWebService machineSensorsWebService;

        private readonly IOperatorNavigationService operatorNavigationService;

        private readonly Sensors sensors = new Sensors();

        private DelegateCommand moveToLoadingUnitCommand;

        private SubscriptionToken subscriptionToken;

        #endregion

        #region Constructors

        public LoadingUnitInfoViewModel(IMachineLoadingUnitsWebService machineLoadingUnitsWebService,
            IMachineMissionsWebService machineMissionsWebService,
            IMachineSensorsWebService machineSensorsWebService,
            IOperatorNavigationService operatorNavigationService,
            IMissionOperationsService missionOperationsService, IEventAggregator eventAggregator,
            IWmsDataProvider wmsDataProvider)
            : base(machineLoadingUnitsWebService, missionOperationsService, eventAggregator, wmsDataProvider)
        {
            this.machineSensorsWebService = machineSensorsWebService ?? throw new System.ArgumentNullException(nameof(machineSensorsWebService));
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.machineMissionsWebService = machineMissionsWebService ?? throw new ArgumentNullException(nameof(machineMissionsWebService));
            this.operatorNavigationService = operatorNavigationService ?? throw new ArgumentNullException(nameof(operatorNavigationService));
        }

        #endregion

        #region Properties

        public ICommand MoveToLoadingUnitCommand =>
            this.moveToLoadingUnitCommand
            ??
            (this.moveToLoadingUnitCommand = new DelegateCommand(
                async () => await this.MoveToLoadingUnitAsync(),
                this.CanMoveToLoadingUnit));

        public Sensors Sensors => this.sensors;

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();
        }

        public async Task MoveToLoadingUnitAsync()
        {
            try
            {
                this.NavigationService.GoBack();

                this.Reset();

                this.operatorNavigationService.NavigateToDrawerView();
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        public override async Task OnAppearedAsync()
        {
            this.SubscribeToEvents();

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

            this.moveToLoadingUnitCommand?.RaiseCanExecuteChanged();
        }

        private bool CanMoveToLoadingUnit()
        {
            return (this.sensors.LUPresentInBay1 == true && this.sensors.LUPresentMiddleBottomBay1 == false) || (this.sensors.LUPresentInBay1 == false && this.sensors.LUPresentMiddleBottomBay1 == true);
        }

        private void OnSensorsChanged(NotificationMessageUI<SensorsChangedMessageData> message)
        {
            this.sensors.Update(message.Data.SensorsStates);
        }

        private void SubscribeToEvents()
        {
            //this.unitToken = this.unitToken
            //   ??
            //   this.eventAggregator
            //       .GetEvent<NotificationEventUI<PositioningMessageData>>()
            //       .Subscribe(
            //           this.OnPositioningMessageReceived,
            //           ThreadOption.UIThread,
            //           false);

            this.subscriptionToken = this.subscriptionToken
                ??
                this.EventAggregator
                    .GetEvent<NotificationEventUI<SensorsChangedMessageData>>()
                    .Subscribe(
                        this.OnSensorsChanged,
                        ThreadOption.UIThread,
                        false,
                        m => m.Data?.SensorsStates != null);
        }

        #endregion
    }
}
