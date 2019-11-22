using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Operator.ViewModels
{
    public class LoadingUnitActivityPickingViewModel : BaseLoadingUnitOperationViewModel
    {
        #region Fields

        private ICommand drawerDetailsButtonCommand;

        #endregion

        #region Constructors

        public LoadingUnitActivityPickingViewModel(
                    IWmsImagesProvider wmsImagesProvider,
                    IMissionsDataService missionsDataService,
                    IBayManager bayManager)
                    : base(wmsImagesProvider, missionsDataService, bayManager)
        {
        }

        #endregion

        #region Properties

        public ICommand DrawerDetailsButtonCommand =>
            this.drawerDetailsButtonCommand
            ??
            (this.drawerDetailsButtonCommand = new DelegateCommand(this.NavigateToOperationDetails));

        public override EnableMask EnableMask => EnableMask.Any;

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();
            this.ItemImage?.Dispose();
        }

        private void NavigateToOperationDetails()
        {
            this.NavigationService.Appear(
                nameof(Utils.Modules.Operator),
                Utils.Modules.Operator.DrawerOperations.PICKINGDETAIL,
                null,
                trackCurrentView: true);
        }

        #endregion
    }
}
