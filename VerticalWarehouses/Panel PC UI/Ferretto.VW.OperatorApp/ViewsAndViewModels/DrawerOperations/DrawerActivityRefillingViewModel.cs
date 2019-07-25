using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Operator.ViewsAndViewModels;
using Ferretto.VW.App.Operator.Interfaces;
using Ferretto.VW.App.Operator.ServiceUtilities.Interfaces;
using Ferretto.VW.App.Operator.ViewsAndViewModels.DrawerOperations.Details;
using Ferretto.VW.App.Services.Interfaces;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.OperatorApp.Interfaces;
using Ferretto.VW.OperatorApp.ViewsAndViewModels.DrawerOperations.Details;
using Ferretto.VW.Utils.Source.Filters;
using Ferretto.VW.WmsCommunication.Interfaces;
using Ferretto.VW.WmsCommunication.Source;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Operator.ViewsAndViewModels.DrawerOperations
{
    public class DrawerActivityRefillingViewModel : DrawerOperationBaseViewModel, IDrawerActivityRefillingViewModel, IDrawerActivityViewModel
    {
        #region Fields

        private ICommand confirmCommand;

        private ICommand drawerActivityRefillingDetailsButtonCommand;

        #endregion

        #region Constructors

        public DrawerActivityRefillingViewModel(
            IEventAggregator eventAggregator,
            INavigationService navigationService,
            IStatusMessageService statusMessageService,
            IMainWindowViewModel mainWindowViewModel,
            IWmsDataProvider wmsDataProvider,
            IWmsImagesProvider wmsImagesProvider,
            IMissionOperationsService missionOperationsService,
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

        public ICommand DrawerActivityRefillingDetailsButtonCommand =>
            this.drawerActivityRefillingDetailsButtonCommand
            ??
            (this.drawerActivityRefillingDetailsButtonCommand = new DelegateCommand(
                async () => await this.DrawerDetailsButtonMethod()));

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
            this.NavigationService.NavigateToView<DrawerActivityRefillingDetailViewModel, IDrawerActivityRefillingDetailViewModel>();

            return Task.CompletedTask;
        }

        private async Task GetViewDataAsync(IBayManager bayManager)
        {
            this.ItemImage = null;

            if (this.BayManager.CurrentMissionOperation != null)
            {
                var imageStram = await this.WmsImagesProvider.GetImageAsync(this.BayManager.CurrentMissionOperation.ItemImage);
                this.ItemImage = Image.FromStream(imageStram);
            }
        }

        #endregion
    }
}
