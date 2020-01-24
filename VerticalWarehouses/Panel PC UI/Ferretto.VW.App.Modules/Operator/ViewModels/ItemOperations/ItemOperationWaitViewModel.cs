using System;
using System.Linq;
using System.Threading.Tasks;
using DevExpress.Xpf.Data.Native;
using Ferretto.VW.App.Modules.Operator.Services;
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

        private readonly IMachineMissionOperationsWebService machineMissionOperationsWebService;

        private int loadingUnitsMovements;

        private int pendingMissionOperationsCount;

        #endregion

        #region Constructors

        public ItemOperationWaitViewModel(IMachineMissionOperationsWebService machineMissionOperationsWebService)
            : base(PresentationMode.Operator)
        {
            this.machineMissionOperationsWebService = machineMissionOperationsWebService ?? throw new ArgumentNullException(nameof(machineMissionOperationsWebService));
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

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.IsBackNavigationAllowed = true;

            Task.Run(async () =>
            {
                do
                {
                    await this.CheckForNewOperationCount();
                    await Task.Delay(5000);
                }
                while (this.IsVisible);
            });
        }

        private async Task CheckForNewOperationCount()
        {
            this.loadingUnitsMovements = await this.machineMissionOperationsWebService.GetByBayCountAsync();
            this.RaisePropertyChanged(nameof(this.LoadingUnitsInfo));
        }

        #endregion
    }
}
