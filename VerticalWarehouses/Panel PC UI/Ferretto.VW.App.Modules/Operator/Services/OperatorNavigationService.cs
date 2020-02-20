using System;
using System.Linq;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using NLog;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Operator
{
    public sealed class OperatorNavigationService : IOperatorNavigationService, IDisposable
    {
        #region Fields

        private readonly IBayManager bayManager;

        private readonly IEventAggregator eventAggregator;

        private readonly SubscriptionToken loadingUnitToken;

        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly IMachineMissionsWebService machineMissionsWebService;

        private readonly IMachineModeService machineModeService;

        private readonly IMachineService machineService;

        private readonly IMissionOperationsService missionOperationsService;

        private readonly SubscriptionToken missionToken;

        private readonly INavigationService navigationService;

        private readonly SubscriptionToken navigationToken;

        private bool autoNavigate;

        private bool autoNavigateOnMenu = true;

        private int? lastActiveMissionId;

        private string previousModuleName;

        #endregion

        #region Constructors

        public OperatorNavigationService(
            INavigationService navigationService,
            IMissionOperationsService missionOperationsService,
            IEventAggregator eventAggregator,
            IMachineMissionsWebService machineMissionsWebService,
            IBayManager bayManager,
            IMachineService machineService,
            IMachineModeService machineModeService)
        {
            this.navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            this.missionOperationsService = missionOperationsService ?? throw new ArgumentNullException(nameof(missionOperationsService));
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.machineMissionsWebService = machineMissionsWebService ?? throw new ArgumentNullException(nameof(machineMissionsWebService));
            this.bayManager = bayManager ?? throw new ArgumentNullException(nameof(bayManager));
            this.machineService = machineService ?? throw new ArgumentNullException(nameof(machineService));
            this.machineModeService = machineModeService ?? throw new ArgumentNullException(nameof(machineModeService));

            this.navigationToken = this.eventAggregator
                .GetEvent<PubSubEvent<NavigationCompletedEventArgs>>()
                .Subscribe(
                    this.OnNavigationCompleted,
                    ThreadOption.UIThread,
                    false);

            this.missionToken = this.eventAggregator
                .GetEvent<PubSubEvent<MissionChangedEventArgs>>()
                .Subscribe(
                    this.OnMissionChanged,
                    ThreadOption.UIThread,
                    false);
        }

        #endregion

        #region Methods

        public void Dispose()
        {
            this.loadingUnitToken.Dispose();
            this.missionToken.Dispose();
            this.navigationToken.Dispose();
        }

        public void NavigateToDrawerView()
        {
            this.NavigateToDrawerView(goToWaitViewIfBayIsEmpty: true);
        }

        public void NavigateToOperatorMenu()
        {
            if (this.missionOperationsService.ActiveWmsOperation != null)
            {
                this.NavigateToOperationDetails(this.missionOperationsService.ActiveWmsOperation.Type);
            }
            else
            {
                var currentMission = this.missionOperationsService.ActiveMachineMission;
                if (this.machineService.Loadunits.SingleOrDefault(l => l.Id == currentMission?.LoadUnitId) is LoadingUnit loadingUnit)
                {
                    this.NavigateToLoadingUnitDetails(loadingUnit.Id);
                }
                else
                {
                    this.navigationService.Appear(
                        nameof(Utils.Modules.Operator),
                        Utils.Modules.Operator.OPERATOR_MENU,
                        null,
                        true);
                }
            }
        }

        private string GetActiveViewModelName()
        {
            return this.navigationService.GetActiveViewModel().GetType().Name;
        }

        private bool IsViewTrackable()
        {
            var activeViewModelName = this.GetActiveViewModelName();
            return activeViewModelName != Utils.Modules.Operator.ItemOperations.WAIT;
        }

        private void NavigateToDrawerView(bool goToWaitViewIfBayIsEmpty)
        {
            var activeViewModelName = this.GetActiveViewModelName();
            if (activeViewModelName != Utils.Modules.Operator.OPERATOR_MENU
                &&
                activeViewModelName != Utils.Modules.Operator.ItemOperations.WAIT)
            {
                return;
            }

            if (this.missionOperationsService.ActiveWmsOperation != null)
            {
                this.NavigateToOperationDetails(this.missionOperationsService.ActiveWmsOperation.Type);
            }
            else
            {
                var currentMission = this.missionOperationsService.ActiveMachineMission;
                var loadingUnit = this.machineService.Loadunits.SingleOrDefault(l => l.Id == currentMission?.LoadUnitId);
                if (loadingUnit != null)
                {
                    this.NavigateToLoadingUnitDetails(loadingUnit.Id);
                }
                else if (activeViewModelName != Utils.Modules.Operator.ItemOperations.WAIT && goToWaitViewIfBayIsEmpty)
                {
                    this.logger.Trace("No operation and no loading unit in bay, navigation to wait view.");

                    this.navigationService.Appear(
                        nameof(Utils.Modules.Operator),
                        Utils.Modules.Operator.ItemOperations.WAIT,
                        null,
                        trackCurrentView: this.IsViewTrackable());
                }
            }
        }

        private void NavigateToLoadingUnitDetails(int loadingUnitId)
        {
            this.lastActiveMissionId = null;

            var activeViewModelName = this.GetActiveViewModelName();

            this.navigationService.Appear(
                nameof(Utils.Modules.Operator),
                Utils.Modules.Operator.ItemOperations.LOADING_UNIT,
                loadingUnitId,
                trackCurrentView: this.IsViewTrackable());
        }

        private void NavigateToOperationDetails(MissionOperationType operationType)
        {
            string viewModelName = null;
            switch (operationType)
            {
                case MissionOperationType.Inventory:
                    viewModelName = Utils.Modules.Operator.ItemOperations.INVENTORY;
                    break;

                case MissionOperationType.Pick:
                    viewModelName = Utils.Modules.Operator.ItemOperations.PICK;
                    break;

                case MissionOperationType.Put:
                    viewModelName = Utils.Modules.Operator.ItemOperations.PUT;
                    break;

                case MissionOperationType.LoadingUnitCheck:
                    viewModelName = Utils.Modules.Operator.ItemOperations.LOADING_UNIT;
                    break;

                default:
                    throw new Exception("Operation type is not supported");
            }

            this.lastActiveMissionId = this.missionOperationsService.ActiveWmsMission.Id;

            this.navigationService.Appear(
                nameof(Utils.Modules.Operator),
                viewModelName,
                this.missionOperationsService.ActiveWmsMission?.LoadingUnit?.Id,
                trackCurrentView: this.IsViewTrackable());
        }

        private void OnMissionChanged(MissionChangedEventArgs e)
        {
            var activeViewModelName = this.GetActiveViewModelName();
            if (activeViewModelName is Utils.Modules.Operator.OPERATOR_MENU
                ||
                activeViewModelName is Utils.Modules.Operator.ItemOperations.WAIT)
            {
                this.NavigateToDrawerView();
            }
        }

        private void OnNavigationCompleted(NavigationCompletedEventArgs e)
        {
            this.autoNavigateOnMenu = this.previousModuleName != nameof(Utils.Modules.Operator);
            this.previousModuleName = e.ModuleName;

            if (e.ModuleName != nameof(Utils.Modules.Operator))
            {
                this.autoNavigate = false;
                return;
            }

            switch (e.ViewModelName)
            {
                case Utils.Modules.Operator.OPERATOR_MENU:
                    if (this.autoNavigateOnMenu)
                    {
                        this.NavigateToDrawerView(goToWaitViewIfBayIsEmpty: false);
                    }

                    break;

                case Utils.Modules.Operator.ItemOperations.WAIT:
                case Utils.Modules.Operator.ItemOperations.LOADING_UNIT:
                case Utils.Modules.Operator.ItemOperations.INVENTORY:
                case Utils.Modules.Operator.ItemOperations.INVENTORY_DETAILS:
                case Utils.Modules.Operator.ItemOperations.PICK:
                case Utils.Modules.Operator.ItemOperations.PICK_DETAILS:
                case Utils.Modules.Operator.ItemOperations.PUT:
                case Utils.Modules.Operator.ItemOperations.PUT_DETAILS:
                    this.autoNavigate = true;
                    break;

                default:
                    this.autoNavigate = false;
                    break;
            }
        }

        #endregion
    }
}
