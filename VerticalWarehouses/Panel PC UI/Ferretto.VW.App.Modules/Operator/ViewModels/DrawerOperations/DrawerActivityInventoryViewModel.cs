using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Operator.ViewModels
{
    public class DrawerActivityInventoryViewModel : BaseDrawerOperationViewModel
    {
        #region Fields

        private ICommand drawerActivityInventoryDetailsButtonCommand;

        #endregion

        #region Constructors

        public DrawerActivityInventoryViewModel(
            IWmsDataProvider wmsDataProvider,
            IWmsImagesProvider wmsImagesProvider,
            IMachineMissionOperationsWebService missionOperationsService,
            IBayManager bayManager)
            : base(wmsDataProvider, wmsImagesProvider, missionOperationsService, bayManager)
        {
        }

        #endregion

        #region Properties

        public ICommand DrawerActivityInventoryDetailsButtonCommand =>
            this.drawerActivityInventoryDetailsButtonCommand
            ??
            (this.drawerActivityInventoryDetailsButtonCommand = new DelegateCommand(() => this.DrawerDetailsButtonMethod()));

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

        public override void UpdateView()
        {
            base.UpdateView();
        }

        private void DrawerDetailsButtonMethod()
        {
            this.NavigationService.Appear(
                nameof(Utils.Modules.Operator),
                Utils.Modules.Operator.DrawerOperations.INVENTORYDETAIL,
                null,
                trackCurrentView: true);
        }

        private async Task GetViewDataAsync(IBayManager bayManager)
        {
            // TODO  var imageStram = await this.WmsImagesProvider.GetImageAsync(this.BayManager.CurrentMissionOperation.ItemImage);
            // TODO   this.ItemImage = Image.FromStream(imageStram);
        }

        #endregion
    }
}
