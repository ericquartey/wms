using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        private readonly List<LoadingUnit> loadingUnits = new List<LoadingUnit>();

        private readonly IMachineMissionsWebService machineMissionsWebService;

        private readonly IMachineService machineService;

        private bool isGridVisible;

        private string loadingUnitsInfo;

        private int loadingUnitsMovements;

        private IEnumerable<int> moveUnitId = new List<int>();

        private int pendingMissionOperationsCount;

        #endregion

        #region Constructors

        public ItemOperationWaitViewModel(IMachineMissionsWebService machineMissionsWebService,
            IMachineService machineService)
            : base(PresentationMode.Operator)
        {
            this.machineMissionsWebService = machineMissionsWebService ?? throw new ArgumentNullException(nameof(machineMissionsWebService));
            this.machineService = machineService ?? throw new ArgumentNullException(nameof(machineService));
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
                    this.RaisePropertyChanged();
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public IEnumerable<LoadingUnit> LoadingUnits => new BindingList<LoadingUnit>(this.loadingUnits);

        public string LoadingUnitsInfo
        {
            get => this.loadingUnitsInfo;
            set => this.SetProperty(ref this.loadingUnitsInfo, value);
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
                this.loadingUnits.Clear();
                this.moveUnitId = await this.machineMissionsWebService.GetAllUnitGoBayAsync();

                if (this.moveUnitId != null)
                {
                    foreach (var unit in this.moveUnitId)
                    {
                        this.loadingUnits.AddRange(this.machineService.Loadunits.Where(i => i.Id == unit));
                    }
                }
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
                this.loadingUnits.Clear();
            }
            finally
            {
                this.RaisePropertyChanged(nameof(this.LoadingUnits));

                if (this.loadingUnits.Count > 0)
                {
                    this.isGridVisible = true;
                }
            }
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.RaisePropertyChanged(nameof(this.isGridVisible));

            this.IsBackNavigationAllowed = true;

            Task.Run(async () =>
            {
                do
                {
                    await Task.Delay(5000);
                    await this.CheckForNewOperationCount();
                    await this.GetLoadingUnitsAsync();
                }
                while (this.IsVisible);
            });
        }

        protected override async Task OnDataRefreshAsync()
        {
            await base.OnDataRefreshAsync();

            await this.GetLoadingUnitsAsync();

            await this.CheckForNewOperationCount();
        }

        private async Task CheckForNewOperationCount()
        {
            var missions = await this.machineMissionsWebService.GetAllAsync();
            this.loadingUnitsMovements = missions.Count(m => m.MissionType == MissionType.OUT || m.MissionType == MissionType.WMS);
            this.LoadingUnitsInfo = this.ComputeLoadingUnitInfo();
        }

        private string ComputeLoadingUnitInfo()
        {
            if (this.loadingUnitsMovements == 0)
            {
                return OperatorApp.NoLoadingUnitsToMove;
            }
            else if (this.loadingUnitsMovements == 1)
            {
                return OperatorApp.LoadingUnitSendToBay;
            }

            return string.Format(OperatorApp.LoadingUnitsSendToBay, this.loadingUnitsMovements);
        }

        #endregion
    }
}
