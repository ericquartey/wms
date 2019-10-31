using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Operator.ViewModels
{
    public class DrawerActivityPickingViewModel : BaseDrawerOperationViewModel
    {
        #region Fields

        private ICommand confirmCommand;

        private ICommand drawerDetailsButtonCommand;

        #endregion

        #region Constructors

        public DrawerActivityPickingViewModel(
            IWmsDataProvider wmsDataProvider,
            IWmsImagesProvider wmsImagesProvider,
            IMachineMissionOperationsWebService missionOperationsService,
            IBayManager bayManager)
            : base(wmsDataProvider, wmsImagesProvider, missionOperationsService, bayManager)
        {
        }

        #endregion

        #region Properties

        public ICommand ConfirmCommand =>
            this.confirmCommand
            ??
            (this.confirmCommand = new DelegateCommand(async () => await this.ExecuteConfirmCommand()));

        public ICommand DrawerDetailsButtonCommand =>
            this.drawerDetailsButtonCommand
            ??
            (this.drawerDetailsButtonCommand = new DelegateCommand(this.NavigateToOperationDetails));

        public override EnableMask EnableMask => EnableMask.None;

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();
            this.ItemImage?.Dispose();
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.IsBackNavigationAllowed = true;

            await this.GetViewDataAsync(this.BayManager);
            await this.GetTrayControlDataAsync(this.BayManager);
        }

        private async Task GetViewDataAsync(IBayManager bayManager)
        {
            // TODO       var imageStram = await this.WmsImagesProvider.GetImageAsync(this.BayManager.CurrentMissionOperation.ItemImage);
            // TODO       this.ItemImage = Image.FromStream(imageStram);
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
