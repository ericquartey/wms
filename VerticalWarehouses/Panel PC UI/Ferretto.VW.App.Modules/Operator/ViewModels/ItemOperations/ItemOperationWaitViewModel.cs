using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    [Warning(WarningsArea.Picking)]
    public class ItemOperationWaitViewModel : BaseOperatorViewModel
    {
        #region Fields

        private readonly List<LoadingUnit> loadingUnits;

        private readonly IMachineMissionsWebService machineMissionsWebService;

        private readonly IMachineService machineService;

        private readonly List<LoadingUnit> moveUnits;

        private readonly IOperatorNavigationService operatorNavigationService;

        private readonly ISessionService sessionService;

        private int count;

        private bool isGridVisible;

        private string loadingUnitsInfo;

        private int loadingUnitsMovements;

        private IEnumerable<int> moveUnitId;

        private IEnumerable<int> moveUnitIdToCell;

        private bool moveVisible;

        private int pendingMissionOperationsCount;

        #endregion

        #region Constructors

        public ItemOperationWaitViewModel(
            ISessionService sessionService,
            IOperatorNavigationService operatorNavigationService,
            IMachineMissionsWebService machineMissionsWebService,
            IMissionOperationsService missionOperationsService,
            IMachineService machineService)
            : base(PresentationMode.Operator)
        {
            this.sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
            this.operatorNavigationService = operatorNavigationService ?? throw new ArgumentNullException(nameof(sessionService));
            this.machineMissionsWebService = machineMissionsWebService ?? throw new ArgumentNullException(nameof(machineMissionsWebService));
            this.machineService = machineService ?? throw new ArgumentNullException(nameof(machineService));
            this.MissionOperationsService = missionOperationsService ?? throw new ArgumentNullException(nameof(missionOperationsService));

            this.loadingUnits = new List<LoadingUnit>();
            this.moveUnits = new List<LoadingUnit>();
            this.moveUnitId = new List<int>();
            this.moveUnitIdToCell = new List<int>();
        }

        #endregion

        #region Properties

        public bool IsGridVisible
        {
            get => this.isGridVisible;
            set
            {
                if (this.SetProperty(ref this.isGridVisible, value) && value)
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public List<LoadingUnit> LoadingUnits => new List<LoadingUnit>(this.loadingUnits);

        public string LoadingUnitsInfo
        {
            get => this.loadingUnitsInfo;
            set => this.SetProperty(ref this.loadingUnitsInfo, value);
        }

        public List<LoadingUnit> MoveUnits => new List<LoadingUnit>(this.moveUnits);

        public bool MoveVisible
        {
            get => this.moveVisible;
            set
            {
                if (this.SetProperty(ref this.moveVisible, value) && value)
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public int PendingMissionOperationsCount
        {
            get => this.pendingMissionOperationsCount;
            set => this.SetProperty(ref this.pendingMissionOperationsCount, value);
        }

        protected IMissionOperationsService MissionOperationsService { get; }

        #endregion

        #region Methods

        public override void Disappear()
        {
            this.LoadingUnitsInfo = null;

            base.Disappear();
        }

        public async Task GetLoadingUnitsAsync()
        {
            try
            {
                this.count = 0;

                this.loadingUnits.Clear();
                this.moveUnitId = await this.machineMissionsWebService.GetAllUnitGoBayAsync();

                if (this.moveUnitId != null)
                {
                    foreach (var unit in this.moveUnitId)
                    {
                        this.loadingUnits.AddRange(this.machineService.Loadunits.Where(i => i.Id == unit));
                        if (this.machineService.Loadunits.Any(i => i.Id == unit && i.Status == LoadingUnitStatus.InBay)
                            && !this.machineService.Loadunits.Any(i => i.Status == LoadingUnitStatus.OnMovementToLocation)
                            )
                        {
                            await this.MissionOperationsService.RefreshAsync();
                        }
                        this.count++;
                    }
                }

                this.moveUnits.Clear();
                this.moveUnitIdToCell = await this.machineMissionsWebService.GetAllUnitGoCellAsync();

                if (this.moveUnitIdToCell != null)
                {
                    var userdifference = this.moveUnitIdToCell.Except(this.moveUnitId);

                    if (userdifference.Any())
                    {
                        foreach (var units in userdifference)
                        {
                            this.moveUnits.AddRange(this.machineService.Loadunits.Where(i => i.Id == units));
                        }
                    }
                }
            }
            catch (Exception)
            {
                this.loadingUnits.Clear();
                this.moveUnits.Clear();
            }
            finally
            {
                if (this.moveUnits.Count > 0)
                {
                    this.moveVisible = true;
                }
                else
                {
                    this.moveVisible = false;
                }

                if (this.loadingUnits.Count > 0)
                {
                    this.isGridVisible = true;
                }
                else
                {
                    this.isGridVisible = false;
                }

                this.RaisePropertyChanged(nameof(this.LoadingUnits));

                this.RaisePropertyChanged(nameof(this.MoveVisible));

                this.RaisePropertyChanged(nameof(this.MoveUnits));

                this.RaisePropertyChanged(nameof(this.IsGridVisible));
            }
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.operatorNavigationService.NavigateToDrawerViewUnit();

            this.RaisePropertyChanged(nameof(this.MoveVisible));

            this.RaisePropertyChanged(nameof(this.IsGridVisible));

            this.IsBackNavigationAllowed = true;

            Task.Run(async () =>
            {
                do
                {
                    await Task.Delay(800);
                    await this.CheckForNewOperationCount();
                    await this.GetLoadingUnitsAsync();
                }
                while (this.IsVisible);
            });
        }

        protected override async Task OnDataRefreshAsync()
        {
            try
            {
                await base.OnDataRefreshAsync();

                await this.GetLoadingUnitsAsync();

                await this.CheckForNewOperationCount();
            }
            catch (Exception)
            {
            }
        }

        private async Task CheckForNewOperationCount()
        {
            try
            {
                var missions = await this.machineMissionsWebService.GetAllAsync();
                this.loadingUnitsMovements = missions.Count(m => m.MissionType == MissionType.OUT || m.MissionType == MissionType.WMS);
                this.LoadingUnitsInfo = this.ComputeLoadingUnitInfo();
            }
            catch (Exception)
            {
            }
        }

        private string ComputeLoadingUnitInfo()
        {
            if (this.loadingUnitsMovements == 0)
            {
                return Localized.Get("OperatorApp.NoLoadingUnitsToMove");
            }
            else if (this.loadingUnitsMovements == 1)
            {
                return Localized.Get("OperatorApp.LoadingUnitSendToBay");
            }

            return string.Format(Localized.Get("OperatorApp.LoadingUnitsSendToBay"), this.loadingUnitsMovements);
        }

        #endregion
    }
}
