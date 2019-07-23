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
    public class DrawerActivityInventoryViewModel : DrawerOperationBaseViewModel, IDrawerActivityInventoryViewModel, IDrawerActivityViewModel
    {
        #region Fields

        private ICommand drawerActivityInventoryDetailsButtonCommand;

        #endregion

        #region Constructors

        public DrawerActivityInventoryViewModel(
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

        public ICommand DrawerActivityInventoryDetailsButtonCommand =>
            this.drawerActivityInventoryDetailsButtonCommand
            ??
            (this.drawerActivityInventoryDetailsButtonCommand = new DelegateCommand(async () => await this.DrawerDetailsButtonMethod()));

        #endregion

        #region Methods

        public override async Task OnEnterViewAsync()
        {
            this.StatusMessageService.Notify($"Current mission ID: {this.BayManager.CurrentMission.Id}");
            await this.GetViewDataAsync(this.BayManager);
            await this.GetTrayControlDataAsync(this.BayManager);
        }

        private Task DrawerDetailsButtonMethod()
        {
            this.NavigationService.NavigateToView<DrawerActivityInventoryDetailViewModel, IDrawerActivityInventoryDetailViewModel>();

            return Task.CompletedTask;
        }

        private async Task GetViewDataAsync(IBayManager bayManager)
        {
            var imageStram = await this.WmsImagesProvider.GetImageAsync(this.BayManager.CurrentMissionOperation.ItemImage);
            this.ItemImage = Image.FromStream(imageStram);
        }

        #endregion
    }
}
