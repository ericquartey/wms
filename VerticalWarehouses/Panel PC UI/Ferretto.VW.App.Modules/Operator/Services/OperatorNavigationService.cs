using System;
using System.Linq;
using System.Threading.Tasks;
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

        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly IMachineMissionsWebService machineMissionsWebService;

        private readonly IMachineModeService machineModeService;

        private readonly IMachineService machineService;

        private readonly IMissionOperationsService missionOperationsService;

        private readonly SubscriptionToken missionToken;

        private readonly INavigationService navigationService;

        private readonly SubscriptionToken navigationToken;

        private bool isDisposed;

        private int? lastActiveMissionId;

        private int? lastActiveUnitId;

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
            if (!this.isDisposed)
            {
                this.missionToken.Dispose();
                this.navigationToken.Dispose();

                this.isDisposed = true;
            }
        }

        public void NavigateToDrawerView()
        {
            this.NavigateToDrawerView(goToWaitViewIfBayIsEmpty: true);
        }

        public void NavigateToDrawerViewConfirmPresent()
        {
            this.NavigateToDrawerViewConfirmPresent(goToWaitViewIfBayIsEmpty: true);
        }

        public void NavigateToOperationOrGoBack()
        {
            if (this.missionOperationsService.ActiveWmsOperation != null &&
                this.missionOperationsService.ActiveWmsMission != null &&
                this.missionOperationsService.ActiveWmsMission.Id == this.missionOperationsService.ActiveWmsOperation.MissionId &&
                 this.missionOperationsService.ActiveWmsMission.BayId == this.machineService.Bay.Id)
            {
                this.NavigateToOperationDetails(this.missionOperationsService.ActiveWmsOperation.Type);
            }
            else
            {
                var currentMission = this.missionOperationsService.ActiveMachineMission;
                var loadingUnit = this.machineService.Loadunits.SingleOrDefault(l => l.Id == currentMission?.LoadUnitId);

                if (loadingUnit != null &&
                    currentMission != null &&
                    this.machineService.MachineStatus.LoadingUnitPositionUpInBay?.Id == loadingUnit.Id)
                {
                    this.lastActiveUnitId = loadingUnit.Id;
                    this.NavigateToLoadingUnitDetails(loadingUnit.Id);
                }
                else
                {
                    this.navigationService.GoBackTo(
                              nameof(Utils.Modules.Operator),
                              Utils.Modules.Operator.OPERATOR_MENU,
                              "NavigateToOperatorMenuAsync");
                }
            }
        }

        public async void NavigateToOperatorMenuAsync()
        {
            this.logger.Debug($"Navigate 2 wmsMission {this.missionOperationsService.ActiveWmsOperation?.MissionId}, " +
                $"machineMission {this.missionOperationsService.ActiveMachineMission?.Id}, " +
                $"LU {this.machineService.MachineStatus.LoadingUnitPositionUpInBay?.Id}, " +
                $"Type {this.missionOperationsService.ActiveWmsOperation?.Type}");

            if (this.machineService.MachineStatus.LoadingUnitPositionUpInBay is null && this.machineService.MachineStatus.LoadingUnitPositionDownInBay is null)
            {
                await this.machineService.UpdateLoadUnitInBayAsync();
            }

            if (this.missionOperationsService.ActiveWmsOperation != null &&
                this.missionOperationsService.ActiveWmsMission != null &&
                this.missionOperationsService.ActiveWmsMission.Id == this.missionOperationsService.ActiveWmsOperation.MissionId &&
                 this.missionOperationsService.ActiveWmsMission.BayId == this.machineService.Bay.Id)
            {
                this.NavigateToOperationDetails(this.missionOperationsService.ActiveWmsOperation.Type);
            }
            else
            {
                var currentMission = this.missionOperationsService.ActiveMachineMission;
                var loadingUnit = this.machineService.Loadunits.SingleOrDefault(l => l.Id == currentMission?.LoadUnitId);

                if (loadingUnit != null &&
                    currentMission != null &&
                    this.machineService.MachineStatus.LoadingUnitPositionUpInBay?.Id == loadingUnit.Id)
                {
                    this.lastActiveUnitId = loadingUnit.Id;
                    this.NavigateToLoadingUnitDetails(loadingUnit.Id);
                }
                else
                {
                    this.logger.Debug($"Auto-navigating to '{Utils.Modules.Operator.OPERATOR_MENU}'.");
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

        private bool IsOperationOrLoadingUnitViewModel(string activeViewModelName)
        {
            return
                activeViewModelName is Utils.Modules.Operator.ItemOperations.PICK
                ||
                activeViewModelName is Utils.Modules.Operator.ItemOperations.PICK_DETAILS
                ||
                activeViewModelName is Utils.Modules.Operator.ItemOperations.PUT
                ||
                activeViewModelName is Utils.Modules.Operator.ItemOperations.PUT_DETAILS
                ||
                activeViewModelName is Utils.Modules.Operator.ItemOperations.INVENTORY
                ||
                activeViewModelName is Utils.Modules.Operator.ItemOperations.INVENTORY_DETAILS
                ||
                activeViewModelName is Utils.Modules.Operator.ItemOperations.LOADING_UNIT
                ||
                activeViewModelName is Utils.Modules.Operator.ItemOperations.LOADING_UNIT_INFO
                ||
                activeViewModelName is Utils.Modules.Operator.ItemOperations.ITEMADD
                ||
                activeViewModelName is Utils.Modules.Operator.ItemOperations.SIGNALLINGDEFECT
                ||
                activeViewModelName is Utils.Modules.Operator.ItemOperations.SOCKETLINKOPERATION
                ||
                activeViewModelName is Utils.Modules.Operator.ItemOperations.ADD_DRAPERYITEM_INTO_LOADINGUNIT
                ||
                activeViewModelName is Utils.Modules.Operator.ItemOperations.DRAPERYCONFIRM
                ||
                activeViewModelName is Utils.Modules.Operator.ItemOperations.WEIGHT
                ||
                activeViewModelName is Utils.Modules.Operator.ItemOperations.ADDITEMINTOLOADINGUNIT
                ;
        }

        private bool IsOperatorViewModel(string activeViewModelName)
        {
            return
                activeViewModelName is Utils.Modules.Operator.OPERATOR_MENU
                ||
                activeViewModelName is Utils.Modules.Operator.ItemOperations.WAIT
                ||
                this.IsOperationOrLoadingUnitViewModel(activeViewModelName);
        }

        private bool IsViewTrackable()
        {
            var activeViewModelName = this.GetActiveViewModelName();

            return
                activeViewModelName != Utils.Modules.Operator.ItemOperations.WAIT
                &&
                activeViewModelName != Utils.Modules.Operator.ItemOperations.PICK
                &&
                activeViewModelName != Utils.Modules.Operator.ItemOperations.PUT
                &&
                activeViewModelName != Utils.Modules.Operator.ItemOperations.INVENTORY;
        }

        private async void NavigateToDrawerView(bool goToWaitViewIfBayIsEmpty)
        {
            var activeViewModelName = this.GetActiveViewModelName();
            if (!this.IsOperatorViewModel(activeViewModelName))
            {
                return;
            }

            if (this.machineService.MachineStatus.LoadingUnitPositionUpInBay is null && this.machineService.MachineStatus.LoadingUnitPositionDownInBay is null)
            {
                await this.machineService.UpdateLoadUnitInBayAsync();
            }

            this.logger.Debug($"Navigate 3 wmsMission {this.missionOperationsService.ActiveWmsOperation?.MissionId}, " +
                $"machineMission {this.missionOperationsService.ActiveMachineMission?.Id}, " +
                $"operation {this.missionOperationsService.ActiveWmsOperation?.Id}, " +
                $"Type {this.missionOperationsService.ActiveWmsOperation?.Type}");

            if (this.missionOperationsService.ActiveWmsOperation != null &&
                this.missionOperationsService.ActiveWmsMission != null &&
                this.missionOperationsService.ActiveWmsMission.Id == this.missionOperationsService.ActiveWmsOperation.MissionId &&
                 this.missionOperationsService.ActiveWmsMission.BayId == this.machineService.Bay.Id)
            {
                this.NavigateToOperationDetails(this.missionOperationsService.ActiveWmsOperation.Type);
            }
            else
            {
                //var missions = await this.machineMissionsWebService.GetAllAsync();
                var loadingUnitInBay = this.machineService.Loadunits.Where(l => l.Status == LoadingUnitStatus.InBay);

                var currentMission = this.missionOperationsService.ActiveMachineMission;
                var loadingUnit = this.machineService.Loadunits.SingleOrDefault(l => l.Id == currentMission?.LoadUnitId);

                if (loadingUnit != null &&
                    currentMission != null &&
                    loadingUnitInBay.Any(lu => lu.Id == loadingUnit.Id))
                {
                    this.lastActiveUnitId = loadingUnit.Id;
                    this.NavigateToLoadingUnitDetails(loadingUnit.Id);
                }
                //else if (this.machineService.Bay.IsDouble &&
                //    this.machineService.Bay.IsExternal &&
                //    loadingUnit != null &&
                //   currentMission != null &&
                //   this.machineService.MachineStatus.LoadingUnitPositionDownInBay?.Id == loadingUnit.Id)
                //{
                //    this.lastActiveUnitId = loadingUnit.Id;
                //    this.NavigateToLoadingUnitDetails(loadingUnit.Id);
                //}
                //else if (missions != null &&
                //    loadingUnitInBay != null &&
                //    currentMission != null &&
                //    (this.machineService.MachineStatus.LoadingUnitPositionUpInBay?.Id == loadingUnitInBay.Id ||
                //    this.machineService.MachineStatus.LoadingUnitPositionDownInBay?.Id == loadingUnitInBay.Id) &&
                //    missions.Any(s => s.LoadUnitId == loadingUnitInBay.Id))
                //{
                //    this.lastActiveUnitId = loadingUnitInBay.Id;
                //    this.NavigateToLoadingUnitDetails(loadingUnitInBay.Id);
                //}
                //else if (missions == null &&
                //    loadingUnitInBay != null &&
                //    (this.machineService.MachineStatus.LoadingUnitPositionUpInBay?.Id == loadingUnitInBay.Id ||
                //    this.machineService.MachineStatus.LoadingUnitPositionDownInBay?.Id == loadingUnitInBay.Id))
                //{
                //    this.lastActiveUnitId = loadingUnitInBay.Id;
                //    this.NavigateToLoadingUnitDetails(loadingUnitInBay.Id);
                //}
                else if (
                    activeViewModelName != Utils.Modules.Operator.ItemOperations.WAIT
                    &&
                    goToWaitViewIfBayIsEmpty)
                {
                    this.logger.Debug("No WMS operation and no loading unit in bay, navigation to wait view.");

                    //if (this.IsOperationOrLoadingUnitViewModel(activeViewModelName))
                    //{
                    //    this.navigationService.GoBackTo(
                    //       nameof(Utils.Modules.Operator),
                    //       Utils.Modules.Operator.ItemOperations.WAIT,
                    //       "NavigateToDrawerView");
                    //}
                    //else
                    {
                        this.navigationService.Appear(
                            nameof(Utils.Modules.Operator),
                            Utils.Modules.Operator.ItemOperations.WAIT,
                            null,
                            trackCurrentView: false);
                    }
                }
            }
        }

        private void NavigateToDrawerViewConfirmPresent(bool goToWaitViewIfBayIsEmpty)
        {
            var activeViewModelName = this.GetActiveViewModelName();
            if (!this.IsOperatorViewModel(activeViewModelName))
            {
                return;
            }
            this.logger.Debug($"Navigate 1 wmsMission {this.missionOperationsService.ActiveWmsOperation?.MissionId}, machineMission {this.missionOperationsService.ActiveMachineMission?.Id}");
            if (this.missionOperationsService.ActiveWmsOperation != null)
            {
                this.NavigateToOperationDetails(this.missionOperationsService.ActiveWmsOperation.Type);
            }
            else
            {
                var currentMission = this.missionOperationsService.ActiveMachineMission;
                var loadingUnit = this.machineService.Loadunits.SingleOrDefault(l => l.Id == currentMission?.LoadUnitId);
                if (loadingUnit != null &&
                    currentMission != null
                    )
                {
                    this.lastActiveUnitId = loadingUnit.Id;
                    this.NavigateToLoadingUnitDetails(loadingUnit.Id);
                }
                else if (currentMission != null)
                {
                    this.navigationService.Appear(
                            nameof(Utils.Modules.Operator),
                            Utils.Modules.Operator.ItemOperations.WAIT,
                            null,
                            trackCurrentView: true);
                }
            }
        }

        private void NavigateToLoadingUnitDetails(int loadingUnitId)
        {
            this.lastActiveMissionId = null;

            var activeViewModelName = this.GetActiveViewModelName();

            this.logger.Debug($"Auto-navigating LU to '{Utils.Modules.Operator.ItemOperations.LOADING_UNIT}' with loading unit '{loadingUnitId}'.");
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
                    if (this.GetActiveViewModelName() == Utils.Modules.Operator.ItemOperations.ITEMADD)
                    {
                        viewModelName = Utils.Modules.Operator.ItemOperations.ITEMADD;
                    }
                    else
                    {
                        viewModelName = Utils.Modules.Operator.ItemOperations.INVENTORY;
                    }
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
                    this.logger.Warn($"Operation type is not supported (enum value = {(int)operationType}).");
                    break;
            }

            this.lastActiveMissionId = this.missionOperationsService.ActiveWmsMission?.Id;

            var loadingUnitId = this.missionOperationsService.ActiveWmsMission?.LoadingUnit?.Id;
            this.logger.Debug($"Auto-navigating OP to '{viewModelName}' with loading unit '{loadingUnitId}'.");

            this.navigationService.Appear(
                nameof(Utils.Modules.Operator),
                viewModelName,
                loadingUnitId,
                trackCurrentView: this.IsViewTrackable());
        }

        private void OnMissionChanged(MissionChangedEventArgs e)
        {
            this.NavigateToDrawerView();
        }

        private void OnNavigationCompleted(NavigationCompletedEventArgs e)
        {
            var navigatingFromOtherModule = this.previousModuleName != nameof(Utils.Modules.Operator);
            this.previousModuleName = e.ModuleName;

            // auto navigate to drawer view if:
            // - there is a loading unit in the bay
            // - we are now in the operator pages but we are coming from outside of the operator pages
            if (e.ModuleName == nameof(Utils.Modules.Operator)
                &&
                e.ViewModelName == Utils.Modules.Operator.OPERATOR_MENU
                &&
                navigatingFromOtherModule)
            {
                this.NavigateToDrawerView(goToWaitViewIfBayIsEmpty: false);
            }
        }

        #endregion
    }
}
