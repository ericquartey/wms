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

        private readonly IMachineLoadingUnitsWebService machineLoadingUnitsWebService;

        private readonly IMachineMissionsWebService machineMissionsWebService;

        private readonly IMachineService machineService;

        private readonly IMissionOperationsService missionOperationsService;

        private readonly List<LoadingUnit> moveUnits;

        private readonly IOperatorNavigationService operatorNavigationService;

        private readonly ISessionService sessionService;

        private int count;

        private bool isGridVisible;

        private string loadingUnitsInfo;

        private IEnumerable<int> moveUnitId;

        private IEnumerable<int> moveUnitIdToCell;

        private bool moveVisible;

        private int pendingMissionOperationsCount;

        #endregion

        #region Constructors

        public ItemOperationWaitViewModel(
            IMachineLoadingUnitsWebService machineLoadingUnitsWebService,
            ISessionService sessionService,
            IOperatorNavigationService operatorNavigationService,
            IMachineMissionsWebService machineMissionsWebService,
            IMissionOperationsService missionOperationsService,
            IMachineService machineService)
            : base(PresentationMode.Operator)
        {
            this.machineLoadingUnitsWebService = machineLoadingUnitsWebService ?? throw new ArgumentNullException(nameof(machineLoadingUnitsWebService));
            this.sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
            this.operatorNavigationService = operatorNavigationService ?? throw new ArgumentNullException(nameof(sessionService));
            this.machineMissionsWebService = machineMissionsWebService ?? throw new ArgumentNullException(nameof(machineMissionsWebService));
            this.machineService = machineService ?? throw new ArgumentNullException(nameof(machineService));
            this.missionOperationsService = missionOperationsService ?? throw new ArgumentNullException(nameof(missionOperationsService));

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
                await this.missionOperationsService.RefreshAsync();

                this.count = 0;

                this.loadingUnits.Clear();
                this.moveUnitId = await this.machineMissionsWebService.GetAllUnitGoBayAsync(this.machineService.BayNumber);

                if (this.moveUnitId != null)
                {
                    foreach (var unit in this.moveUnitId)
                    {
                        this.loadingUnits.AddRange(this.machineService.Loadunits.Where(i => i.Id == unit));

                        this.count++;
                    }
                }

                this.moveUnits.Clear();
                this.moveUnitIdToCell = await this.machineMissionsWebService.GetAllUnitGoCellAllAsync();

                if (this.moveUnitIdToCell != null)
                {
                    var userdifference = this.moveUnitIdToCell.Except(this.moveUnitId);

                    if (userdifference.Any())
                    {
                        foreach (var units in userdifference)
                        {
                            if (!this.moveUnits.Where(s => s.Id == units).Any())
                            {
                                this.moveUnits.AddRange(this.machineService.Loadunits.Where(i => i.Id == units));
                            }
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
                this.moveVisible = this.moveUnits.Count > 0 ? true : false;

                this.isGridVisible = this.loadingUnits.Count > 0 ? true : false;

                this.RaisePropertyChanged(nameof(this.LoadingUnits));

                this.RaisePropertyChanged(nameof(this.MoveVisible));

                this.RaisePropertyChanged(nameof(this.MoveUnits));

                this.RaisePropertyChanged(nameof(this.IsGridVisible));
            }
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.RaisePropertyChanged(nameof(this.MoveVisible));

            this.RaisePropertyChanged(nameof(this.IsGridVisible));

            this.IsBackNavigationAllowed = true;

            Task.Run(async () =>
            {
                do
                {
                    await Task.Delay(800);
                    await this.GetLoadingUnitsAsync();

                    this.LoadingUnitsInfo = this.ComputeLoadingUnitInfo();
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

                this.LoadingUnitsInfo = this.ComputeLoadingUnitInfo();
            }
            catch (Exception)
            {
            }
        }

        private string ComputeLoadingUnitInfo()
        {
            if (this.moveUnitId.Count() == 0)
            {
                return string.Format(Localized.Get("OperatorApp.NoLoadingUnitsToMove"), (int)this.MachineService.BayNumber);
            }
            else if (this.moveUnitId.Count() == 1)
            {
                return string.Format(Localized.Get("OperatorApp.LoadingUnitSendToBay"), (int)this.MachineService.BayNumber);
            }

            return string.Format(Localized.Get("OperatorApp.LoadingUnitsSendToBay"), this.moveUnitId.Count(), (int)this.MachineService.BayNumber);
        }

        #endregion
    }
}
