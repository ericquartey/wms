using System;
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

        private readonly IMachineMissionsWebService machineMissionsWebService;

        private int loadingUnitsMovements;

        private int pendingMissionOperationsCount;
        private string loadingUnitsInfo;

        #endregion

        #region Constructors

        public ItemOperationWaitViewModel(IMachineMissionsWebService machineMissionsWebService)
            : base(PresentationMode.Operator)
        {
            this.machineMissionsWebService = machineMissionsWebService ?? throw new ArgumentNullException(nameof(machineMissionsWebService));
        }

        #endregion

        #region Properties

        public string LoadingUnitsInfo
        {
            get => this.loadingUnitsInfo;
            set => this.SetProperty(ref this.loadingUnitsInfo, value);
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

        public int PendingMissionOperationsCount
        {
            get => this.pendingMissionOperationsCount;
            set => this.SetProperty(ref this.pendingMissionOperationsCount, value);
        }

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.IsBackNavigationAllowed = true;

            Task.Run(async () =>
            {
                do
                {
                    await Task.Delay(5000);
                    await this.CheckForNewOperationCount();
                }
                while (this.IsVisible);
            });
        }

        public override void Disappear()
        {
            this.LoadingUnitsInfo = null;

            base.Disappear();
        }

        protected override async Task OnDataRefreshAsync()
        {
            await base.OnDataRefreshAsync();

            await this.CheckForNewOperationCount();
        }

        private async Task CheckForNewOperationCount()
        {
            var missions = await this.machineMissionsWebService.GetAllAsync();
            this.loadingUnitsMovements = missions.Count(m => m.MissionType == MissionType.OUT || m.MissionType == MissionType.WMS);
            this.LoadingUnitsInfo = this.ComputeLoadingUnitInfo();

        }

        #endregion
    }
}
