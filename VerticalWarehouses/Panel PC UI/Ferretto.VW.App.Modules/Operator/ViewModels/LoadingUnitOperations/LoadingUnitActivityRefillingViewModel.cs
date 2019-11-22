using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Services;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Operator.ViewModels
{
    public class LoadingUnitActivityRefillingViewModel : BaseLoadingUnitOperationViewModel
    {
        #region Fields

        private ICommand drawerActivityRefillingDetailsButtonCommand;

        #endregion

        #region Constructors

        public LoadingUnitActivityRefillingViewModel(
                    IWmsImagesProvider wmsImagesProvider,
                    IMissionsDataService missionsDataService,
                    IBayManager bayManager)
                    : base(wmsImagesProvider, missionsDataService, bayManager)
        {
        }

        #endregion

        #region Properties

        public ICommand DrawerActivityRefillingDetailsButtonCommand =>
            this.drawerActivityRefillingDetailsButtonCommand
            ??
            (this.drawerActivityRefillingDetailsButtonCommand = new DelegateCommand(
                () => this.DrawerDetailsButtonMethod()));

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();
        }

        private void DrawerDetailsButtonMethod()
        {
            this.NavigationService.Appear(
                nameof(Utils.Modules.Operator),
                Utils.Modules.Operator.DrawerOperations.REFILLINGDETAIL,
                null,
                trackCurrentView: true);
        }

        #endregion
    }
}
