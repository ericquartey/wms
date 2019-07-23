using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Operator.ViewsAndViewModels;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.OperatorApp.Interfaces;
using Ferretto.VW.OperatorApp.ViewsAndViewModels.DrawerOperations.Details;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels.DrawerOperations
{
    public class DrawerActivityPickingViewModel : DrawerOperationBaseViewModel, IDrawerActivityPickingViewModel, IDrawerActivityViewModel
    {
        #region Fields

        private ICommand confirmCommand;

        private ICommand drawerDetailsButtonCommand;

        #endregion

        #region Constructors

        public DrawerActivityPickingViewModel(
            IEventAggregator eventAggregator,
            IStatusMessageService statusMessageService,
            IMainWindowViewModel mainWindowViewModel,
            IWmsDataProvider wmsDataProvider,
            IWmsImagesProvider wmsImagesProvider,
            IMissionOperationsService missionOperationsService,
            INavigationService navigationService,
            IBayManager bayManager)
            : base(eventAggregator,
                    navigationService,
                    statusMessageService,
                    mainWindowViewModel,
                    wmsDataProvider,
                    wmsImagesProvider,
                    missionOperationsService,
                    bayManager)
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

        #endregion

        #region Methods

        public override void ExitFromViewMethod()
        {
            this.ItemImage?.Dispose();
        }

        public override async Task OnEnterViewAsync()
        {
            this.StatusMessageService.Notify($"Current mission ID: {this.BayManager.CurrentMission.Id}");
            await this.GetViewDataAsync(this.BayManager);
            await this.GetTrayControlDataAsync(this.BayManager);
        }

        private async Task GetViewDataAsync(IBayManager bayManager)
        {
            var imageStram = await this.WmsImagesProvider.GetImageAsync(this.BayManager.CurrentMissionOperation.ItemImage);
            this.ItemImage = Image.FromStream(imageStram);
        }

        private void NavigateToOperationDetails()
        {
            this.NavigationService.NavigateToView<DrawerActivityPickingDetailViewModel, IDrawerActivityPickingDetailViewModel>();
        }

        #endregion
    }
}
