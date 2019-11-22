using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
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
            IWmsDataProvider wmsDataProvider,
            IWmsImagesProvider wmsImagesProvider,
            IMachineMissionOperationsWebService missionOperationsService,
            IBayManager bayManager)
            : base(wmsDataProvider, wmsImagesProvider, missionOperationsService, bayManager)
        {
        }

        #endregion

        #region Properties

        public ICommand DrawerActivityRefillingDetailsButtonCommand =>
            this.drawerActivityRefillingDetailsButtonCommand
            ??
            (this.drawerActivityRefillingDetailsButtonCommand = new DelegateCommand(
                () => this.DrawerDetailsButtonMethod()));

        public override EnableMask EnableMask => EnableMask.Any;

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.IsBackNavigationAllowed = true;

            await this.GetViewDataAsync(this.BayManager);
            await this.GetTrayControlDataAsync(this.BayManager);
        }

        private void DrawerDetailsButtonMethod()
        {
            this.NavigationService.Appear(
                nameof(Utils.Modules.Operator),
                Utils.Modules.Operator.DrawerOperations.REFILLINGDETAIL,
                null,
                trackCurrentView: true);
        }

        private async Task GetViewDataAsync(IBayManager bayManager)
        {
            this.ItemImage = null;

            if (this.BayManager.CurrentMissionOperation != null)
            {
                // TODO   var imageStram = await this.WmsImagesProvider.GetImageAsync(this.BayManager.CurrentMissionOperation.ItemImage);
                // TODO    this.ItemImage = Image.FromStream(imageStram);
            }
        }

        #endregion
    }
}
