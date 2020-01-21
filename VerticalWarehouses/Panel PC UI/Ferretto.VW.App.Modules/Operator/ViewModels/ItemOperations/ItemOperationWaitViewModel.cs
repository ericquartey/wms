using System;
using System.Linq;
using System.Threading.Tasks;
using DevExpress.Xpf.Data.Native;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Prism.Events;
using Prism.Regions;

namespace Ferretto.VW.App.Operator.ViewModels
{
    [Warning(WarningsArea.Picking)]
    public class ItemOperationWaitViewModel : BaseOperatorViewModel
    {
        #region Fields

        private readonly IBayManager bayManager;

        private readonly IEventAggregator eventAggregator;

        private readonly IMachineMissionOperationsWebService machineMissionOperationsWebService;

        private readonly IMachineModeService machineModeService;

        private readonly IMissionOperationsService missionOperationsService;

        private bool isPerformingOperation;

        private int? lastActiveMissionId;

        private int loadingUnitsMovements;

        private SubscriptionToken loadingUnitToken;

        private SubscriptionToken missionToken;

        private int pendingMissionOperationsCount;

        #endregion

        #region Constructors

        public ItemOperationWaitViewModel(
            IMissionOperationsService missionOperationsService,
            IMachineMissionOperationsWebService machineMissionOperationsWebService,
            IMachineModeService machineModeService,
            IBayManager bayManager,
            IEventAggregator eventAggregator)
            : base(PresentationMode.Operator)
        {
            this.missionOperationsService = missionOperationsService ?? throw new ArgumentNullException(nameof(missionOperationsService));
            this.machineMissionOperationsWebService = machineMissionOperationsWebService ?? throw new ArgumentNullException(nameof(machineMissionOperationsWebService));
            this.machineModeService = machineModeService ?? throw new ArgumentNullException(nameof(machineModeService));
            this.bayManager = bayManager ?? throw new ArgumentNullException(nameof(bayManager));
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
        }

        #endregion

        #region Properties

        public string LoadingUnitsInfo
        {
            get
            {
                if (this.loadingUnitsMovements == 0)
                {
                    return OperatorApp.NoLoadingUnitsToMove;
                }

                if (this.loadingUnitsMovements == 1)
                {
                    return OperatorApp.LoadingUnitSendToBay;
                }

                return string.Format(OperatorApp.LoadingUnitsSendToBay, this.loadingUnitsMovements);
            }
        }

        public int PendingMissionOperationsCount
        {
            get => this.pendingMissionOperationsCount;
            set => this.SetProperty(ref this.pendingMissionOperationsCount, value);
        }

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();

            this.missionToken?.Dispose();
            this.missionToken = null;
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.IsBackNavigationAllowed = true;

            this.missionToken =
                this.missionToken
                ??
                this.eventAggregator.GetEvent<PubSubEvent<AssignedMissionOperationChangedEventArgs>>()
                    .Subscribe(
                        async e => await this.OnAssignedMissionOperationChangedAsync(e),
                        ThreadOption.UIThread,
                        false);

            this.loadingUnitToken =
                this.loadingUnitToken
                ??
                this.eventAggregator.GetEvent<NotificationEventUI<MoveLoadingUnitMessageData>>()
                    .Subscribe(
                        async m => await this.OnLoadingUnitMovedAsync(m),
                        ThreadOption.UIThread,
                        false);

            if (this.isPerformingOperation
                &&
                this.machineModeService.MachineMode == MachineMode.Automatic
                &&
                this.lastActiveMissionId.HasValue
                &&
                this.lastActiveMissionId != this.missionOperationsService.CurrentMission?.Id)
            {
                this.NavigationService.GoBack();
                this.isPerformingOperation = false;
                this.lastActiveMissionId = null;
                return;
            }
            else
            {
                // needs to be a fresh bay instance
                var bay = await this.bayManager.GetBayAsync();
                var loadingUnit = bay.Positions.OrderByDescending(p => p.Height).Select(p => p.LoadingUnit).FirstOrDefault(l => l != null);
                if (loadingUnit != null)
                {
                    this.NavigateToLoadingUnitDetails(loadingUnit.Id);
                }
            }

            await Task.Run(async () =>
            {
                do
                {
                    await this.CheckForNewOperationCount();
                    await Task.Delay(5000);
                }
                while (this.IsVisible);
            });

            await this.CheckForNewOperationAsync();
        }

        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
            base.OnNavigatedFrom(navigationContext);
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);
        }

        private async Task CheckForNewOperationAsync()
        {
            if (this.machineModeService.MachineMode != MachineMode.Automatic)
            {
                return;
            }

            this.PendingMissionOperationsCount = this.missionOperationsService.PendingMissionOperationsCount;

            var missionOperation = this.missionOperationsService.CurrentMissionOperation;
            if (this.missionOperationsService.CurrentMissionOperation is null)
            {
                var bay = await this.bayManager.GetBayAsync();
                var loadingUnit = bay.Positions.OrderByDescending(p => p.Height).Select(p => p.LoadingUnit).FirstOrDefault();
                if (loadingUnit != null)
                {
                    this.NavigationService.Appear(
                           nameof(Utils.Modules.Operator),
                           Utils.Modules.Operator.ItemOperations.LOADING_UNIT,
                           loadingUnit.Id,
                           trackCurrentView: false);
                    this.lastActiveMissionId = null;
                    this.isPerformingOperation = true;
                }
            }
            else
            {
                switch (this.missionOperationsService.CurrentMissionOperation.Type)
                {
                    case MissionOperationType.Inventory:
                        this.NavigationService.Appear(
                            nameof(Utils.Modules.Operator),
                            Utils.Modules.Operator.ItemOperations.INVENTORY,
                            null,
                            trackCurrentView: true);
                        this.lastActiveMissionId = this.missionOperationsService.CurrentMission.Id;
                        this.isPerformingOperation = true;
                        break;

                    case MissionOperationType.Pick:
                        this.NavigationService.Appear(
                            nameof(Utils.Modules.Operator),
                            Utils.Modules.Operator.ItemOperations.PICK,
                            null,
                            trackCurrentView: true);
                        this.lastActiveMissionId = this.missionOperationsService.CurrentMission.Id;
                        this.isPerformingOperation = true;
                        break;

                    case MissionOperationType.Put:
                        this.NavigationService.Appear(
                            nameof(Utils.Modules.Operator),
                            Utils.Modules.Operator.ItemOperations.PUT,
                            null,
                            trackCurrentView: true);
                        this.lastActiveMissionId = this.missionOperationsService.CurrentMission.Id;
                        this.isPerformingOperation = true;
                        break;

                    case MissionOperationType.LoadingUnitCheck:
                        this.NavigationService.Appear(
                            nameof(Utils.Modules.Operator),
                            Utils.Modules.Operator.ItemOperations.LOADING_UNIT_CHECK,
                            null,
                            trackCurrentView: true);
                        this.lastActiveMissionId = this.missionOperationsService.CurrentMission.Id;
                        this.isPerformingOperation = true;
                        break;
                }
            }
        }

        private async Task CheckForNewOperationCount()
        {
            this.loadingUnitsMovements = await this.machineMissionOperationsWebService.GetByBayCountAsync();
            this.RaisePropertyChanged(nameof(this.LoadingUnitsInfo));
        }

        private void NavigateToLoadingUnitDetails(int loadingUnitId)
        {
            this.NavigationService.Appear(
                nameof(Utils.Modules.Operator),
                Utils.Modules.Operator.ItemOperations.LOADING_UNIT,
                loadingUnitId,
                trackCurrentView: true);
        }

        private async Task OnAssignedMissionOperationChangedAsync(AssignedMissionOperationChangedEventArgs e)
        {
            await this.CheckForNewOperationAsync();
        }

        private async Task OnLoadingUnitMovedAsync(NotificationMessageUI<MoveLoadingUnitMessageData> message)
        {
            if (message.Data.MissionType is CommonUtils.Messages.Enumerations.MissionType.OUT
                &&
                message.Status is CommonUtils.Messages.Enumerations.MessageStatus.OperationWaitResume)
            {
                var bay = await this.bayManager.GetBayAsync();
                var loadingUnit = this.MachineService.Loadunits.SingleOrDefault(l => l.Id == message.Data.LoadUnitId);

                if (loadingUnit is null)
                {
                    return;
                }

                this.NavigateToLoadingUnitDetails(loadingUnit.Id);

                this.lastActiveMissionId = bay.CurrentMission?.Id;
                this.isPerformingOperation = true;
            }
        }

        #endregion
    }
}
