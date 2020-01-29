using System;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Ferretto.WMS.Data.WebAPI.Contracts;
using NLog;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Operator.Services
{
    public class OperatorNavigationService : IOperatorNavigationService
    {
        #region Fields

        private readonly IBayManager bayManager;

        private readonly IEventAggregator eventAggregator;

        private readonly SubscriptionToken loadingUnitToken;

        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly IMachineModeService machineModeService;

        private readonly IMachineService machineService;

        private readonly IMissionOperationsService missionOperationsService;

        private readonly SubscriptionToken missionToken;

        private readonly INavigationService navigationService;

        private readonly SubscriptionToken navigationToken;

        private bool autoNavigate;

        private bool autoNavigateOnMenu = true;

        private int? lastActiveMissionId;

        #endregion

        #region Constructors

        public OperatorNavigationService(
            INavigationService navigationService,
            IMissionOperationsService missionOperationsService,
            IEventAggregator eventAggregator,
            IBayManager bayManager,
            IMachineService machineService,
            IMachineModeService machineModeService)
        {
            this.navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            this.missionOperationsService = missionOperationsService ?? throw new ArgumentNullException(nameof(missionOperationsService));
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.bayManager = bayManager ?? throw new ArgumentNullException(nameof(bayManager));
            this.machineService = machineService ?? throw new ArgumentNullException(nameof(machineService));
            this.machineModeService = machineModeService ?? throw new ArgumentNullException(nameof(machineModeService));

            this.navigationToken = this.eventAggregator
                .GetEvent<PubSubEvent<NavigationCompletedEventArgs>>()
                .Subscribe(
                    async e => await this.OnNavigationCompletedAsync(e),
                    ThreadOption.UIThread,
                    false);

            this.missionToken = this.eventAggregator
                .GetEvent<PubSubEvent<AssignedMissionOperationChangedEventArgs>>()
                .Subscribe(
                    async e => await this.OnAssignedMissionOperationChangedAsync(e),
                    ThreadOption.UIThread,
                    false);

            this.loadingUnitToken = this.eventAggregator
                .GetEvent<NotificationEventUI<CommonUtils.Messages.Data.MoveLoadingUnitMessageData>>()
                .Subscribe(
                    this.OnLoadingUnitMoved,
                    ThreadOption.UIThread,
                    false);
        }

        #endregion

        #region Methods

        public async Task NavigateToDrawerViewAsync()
        {
            await this.CheckForNewOperationAsync(forceNavigation: true);
        }

        private async Task CheckForNewOperationAsync(bool forceNavigation = false)
        {
            if (!forceNavigation && !this.autoNavigate)
            {
                return;
            }

            var missionOperation = this.missionOperationsService.CurrentMissionOperation;
            var activeViewModelName = this.GetActiveViewModelName();
            if (this.missionOperationsService.CurrentMissionOperation is null)
            {
                var bay = await this.bayManager.GetBayAsync();
                var loadingUnit = bay.Positions
                    .Where(p => p.LoadingUnit != null)
                    .OrderByDescending(p => p.Height)
                    .Select(p => p.LoadingUnit)
                    .FirstOrDefault();
                if (loadingUnit != null)
                {
                    if (activeViewModelName is Utils.Modules.Operator.OPERATOR_MENU
                       ||
                       activeViewModelName is Utils.Modules.Operator.ItemOperations.WAIT)
                    {
                        this.NavigateToLoadingUnitDetails(loadingUnit.Id);
                    }
                }
                else
                {
                    this.lastActiveMissionId = null;
                    if (activeViewModelName == Utils.Modules.Operator.OPERATOR_MENU && forceNavigation)
                    {
                        this.navigationService.Appear(
                            nameof(Utils.Modules.Operator),
                            Utils.Modules.Operator.ItemOperations.WAIT,
                            null,
                            false);
                    }
                    else
                    {
                        this.navigationService.GoBackTo(nameof(Utils.Modules.Operator), Utils.Modules.Operator.ItemOperations.WAIT);
                    }
                }
            }
            else if (this.machineModeService.MachineMode is MachineMode.Automatic)
            {
                switch (this.missionOperationsService.CurrentMissionOperation.Type)
                {
                    case MissionOperationType.Inventory:
                        this.NavigateToOperationDetails(Utils.Modules.Operator.ItemOperations.INVENTORY);
                        break;

                    case MissionOperationType.Pick:
                        this.NavigateToOperationDetails(Utils.Modules.Operator.ItemOperations.PICK);
                        break;

                    case MissionOperationType.Put:
                        this.NavigateToOperationDetails(Utils.Modules.Operator.ItemOperations.PUT);
                        break;

                    case MissionOperationType.LoadingUnitCheck:
                        this.NavigateToOperationDetails(Utils.Modules.Operator.ItemOperations.LOADING_UNIT_CHECK);
                        break;
                }
            }
        }

        private string GetActiveViewModelName()
        {
            return this.navigationService.GetActiveViewModel().GetType().Name;
        }

        private void NavigateToLoadingUnitDetails(int loadingUnitId)
        {
            this.lastActiveMissionId = null;

            var activeViewModelName = this.GetActiveViewModelName();

            this.navigationService.Appear(
                nameof(Utils.Modules.Operator),
                Utils.Modules.Operator.ItemOperations.LOADING_UNIT,
                loadingUnitId,
                trackCurrentView: activeViewModelName != Utils.Modules.Operator.ItemOperations.WAIT);
        }

        private void NavigateToOperationDetails(string viewModelName)
        {
            this.lastActiveMissionId = this.missionOperationsService.CurrentMission.Id;

            var activeViewModelName = this.GetActiveViewModelName();

            this.navigationService.Appear(
                nameof(Utils.Modules.Operator),
                viewModelName,
                null,
                trackCurrentView: activeViewModelName != Utils.Modules.Operator.ItemOperations.WAIT);
        }

        private async Task OnAssignedMissionOperationChangedAsync(AssignedMissionOperationChangedEventArgs e)
        {
            this.logger.Info($"**** Assigned mission={e.MissionId} op={e.MissionOperationId}");
            await this.CheckForNewOperationAsync();
        }

        private void OnLoadingUnitMoved(NotificationMessageUI<CommonUtils.Messages.Data.MoveLoadingUnitMessageData> message)
        {
            this.logger.Info($"**** LU Moved: id={message.Data.LoadUnitId} type={message.Data.MissionType} status={message.Status} stage={message.Description}");

            if (message.Data.MissionType is CommonUtils.Messages.Enumerations.MissionType.OUT
               &&
               message.Status is CommonUtils.Messages.Enumerations.MessageStatus.OperationWaitResume
               &&
               message.Data.LoadUnitId.HasValue
               &&
               this.autoNavigate)
            {
                this.NavigateToLoadingUnitDetails(message.Data.LoadUnitId.Value);
            }
        }

        private async Task OnNavigationCompletedAsync(NavigationCompletedEventArgs e)
        {
            switch (e.ViewModelName)
            {
                case Utils.Modules.Operator.OPERATOR_MENU:
                    this.autoNavigate = this.autoNavigateOnMenu;
                    this.autoNavigateOnMenu = false;
                    if (this.autoNavigate)
                    {
                        await this.CheckForNewOperationAsync();
                    }
                    break;

                case Utils.Modules.Operator.ItemOperations.WAIT:
                case Utils.Modules.Operator.ItemOperations.LOADING_UNIT:
                case Utils.Modules.Operator.ItemOperations.LOADING_UNIT_CHECK:
                case Utils.Modules.Operator.ItemOperations.INVENTORY:
                case Utils.Modules.Operator.ItemOperations.INVENTORY_DETAILS:
                case Utils.Modules.Operator.ItemOperations.PICK:
                case Utils.Modules.Operator.ItemOperations.PICK_DETAILS:
                case Utils.Modules.Operator.ItemOperations.PUT:
                case Utils.Modules.Operator.ItemOperations.PUT_DETAILS:
                    this.autoNavigate = true;
                    break;

                case Utils.Modules.Operator.WaitingLists.MAIN:
                case Utils.Modules.Operator.ItemSearch.MAIN:
                case Utils.Modules.Operator.ItemSearch.ITEM_DETAILS:
                case Utils.Modules.Operator.WaitingLists.DETAIL:
                case Utils.Modules.Operator.Others.IMMEDIATELOADINGUNITCALL:
                case Utils.Modules.Operator.Others.LOADINGUNITSMISSIONS:
                    this.autoNavigate = false;
                    break;

                default:
                    this.autoNavigate = false;
                    this.autoNavigateOnMenu = true;
                    break;
            }

            //await this.CheckForNewOperationAsync();
        }

        #endregion
    }
}
