using System.Windows.Input;
using Ferretto.VW.App.Services;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Operator.ViewModels
{
    public class LoadingUnitActivityInventoryViewModel : BaseLoadingUnitOperationViewModel
    {
        #region Fields

        private ICommand drawerActivityInventoryDetailsButtonCommand;

        #endregion

        #region Constructors

        public LoadingUnitActivityInventoryViewModel(
                    IWmsImagesProvider wmsImagesProvider,
                    IMissionsDataService missionsDataService,
                    IBayManager bayManager)
                    : base(wmsImagesProvider, missionsDataService, bayManager)
        {
        }

        #endregion

        #region Properties

        public ICommand DrawerActivityInventoryDetailsButtonCommand =>
            this.drawerActivityInventoryDetailsButtonCommand
            ??
            (this.drawerActivityInventoryDetailsButtonCommand = new DelegateCommand(() => this.DrawerDetailsButtonMethod()));

        #endregion

        #region Methods

        private void DrawerDetailsButtonMethod()
        {
            this.NavigationService.Appear(
                nameof(Utils.Modules.Operator),
                Utils.Modules.Operator.DrawerOperations.INVENTORYDETAIL,
                null,
                trackCurrentView: true);
        }

        #endregion
    }
}
