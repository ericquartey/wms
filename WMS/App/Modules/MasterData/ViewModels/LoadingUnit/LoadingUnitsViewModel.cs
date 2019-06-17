using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.Utils;
using Ferretto.WMS.App.Controls;
using Ferretto.WMS.App.Controls.Services;
using Ferretto.WMS.App.Core.Interfaces;
using Ferretto.WMS.App.Core.Models;
using Prism.Commands;

namespace Ferretto.WMS.Modules.MasterData
{
    [Resource(nameof(Ferretto.WMS.Data.WebAPI.Contracts.LoadingUnit), false)]
    [Resource(nameof(Ferretto.WMS.Data.WebAPI.Contracts.Compartment), false)]
    public class LoadingUnitsViewModel : EntityPagedListViewModel<LoadingUnit, int>
    {
        #region Fields

        private readonly ILoadingUnitProvider loadingUnitProvider;

        private ICommand withdrawLoadingUnitCommand;

        private string withdrawReason;

        #endregion

        #region Constructors

        public LoadingUnitsViewModel(
            IDataSourceService dataSourceService,
            ILoadingUnitProvider loadingUnitProvider)
            : base(dataSourceService)
        {
            this.loadingUnitProvider = loadingUnitProvider;
        }

        #endregion

        #region Properties

        public ICommand WithdrawLoadingUnitCommand => this.withdrawLoadingUnitCommand ??
                (this.withdrawLoadingUnitCommand = new DelegateCommand(
                    this.WithdrawLoadingUnit,
                    this.CanWithdrawLoadingUnit));

        public string WithdrawReason
        {
            get => this.withdrawReason;
            set => this.SetProperty(ref this.withdrawReason, value);
        }

        #endregion

        #region Methods

        public override void ShowDetails()
        {
            this.HistoryViewService.Appear(nameof(MasterData), Common.Utils.Modules.MasterData.LOADINGUNITDETAILS, this.CurrentItem.Id);
        }

        public override void UpdateReasons()
        {
            base.UpdateReasons();
            this.WithdrawReason = this.CurrentItem?.GetCanExecuteOperationReason(nameof(LoadingUnitPolicy.Withdraw));
        }

        protected override void EvaluateCanExecuteCommands()
        {
            base.EvaluateCanExecuteCommands();
            ((DelegateCommand)this.WithdrawLoadingUnitCommand)?.RaiseCanExecuteChanged();
        }

        protected override void ExecuteAddCommand()
        {
            this.NavigationService.Appear(
                nameof(MasterData),
                Common.Utils.Modules.MasterData.LOADINGUNITADD);
        }

        protected override async Task ExecuteDeleteCommandAsync()
        {
            var result = await this.loadingUnitProvider.DeleteAsync(this.CurrentItem.Id);
            if (result.Success)
            {
                this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.MasterData.LoadingUnitDeletedSuccessfully, StatusType.Success));
                this.SelectedItem = null;
            }
            else
            {
                this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.Errors.UnableToSaveChanges, StatusType.Error));
            }
        }

        private bool CanWithdrawLoadingUnit()
        {
            return this.SelectedItem != null;
        }

        private void WithdrawLoadingUnit()
        {
            if (this.CurrentItem is IPolicyDescriptor<IPolicy> selectedItem)
            {
                if (!selectedItem.CanExecuteOperation(nameof(LoadingUnitPolicy.Withdraw)))
                {
                    this.ShowErrorDialog(selectedItem.GetCanExecuteOperationReason(nameof(LoadingUnitPolicy.Withdraw)));
                    return;
                }

                this.NavigationService.Appear(
                    nameof(MasterData),
                    Common.Utils.Modules.MasterData.LOADINGUNITWITHDRAW,
                    new
                    {
                        LoadingUnitId = this.CurrentItem.Id
                    });
            }
        }

        #endregion
    }
}
