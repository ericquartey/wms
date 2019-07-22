using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.Common.Controls.WPF;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.App.Operator.ViewsAndViewModels;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.OperatorApp.Interfaces;
using Ferretto.VW.OperatorApp.ViewsAndViewModels.DrawerOperations.Details;
using Ferretto.VW.Utils.Source.Filters;
using Ferretto.VW.WmsCommunication.Interfaces;
using Ferretto.VW.WmsCommunication.Source;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels.DrawerOperations
{
    public class DrawerActivityRefillingViewModel : DrawerOperationBaseViewModel, IDrawerActivityRefillingViewModel, IDrawerActivityViewModel
    {
        #region Fields

        private int? actualQuantity;

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

        public ICommand DrawerActivityRefillingDetailsButtonCommand => this.drawerActivityRefillingDetailsButtonCommand ?? (this.drawerActivityRefillingDetailsButtonCommand = new DelegateCommand(
            async () => await this.DrawerDetailsButtonMethod()));

        #endregion

        #region Methods

        public override async Task OnEnterViewAsync()
        {
            this.StatusMessageService.Notify($"Current mission ID: {this.BayManager.CurrentMission.Id}");
            await this.GetViewDataAsync(this.BayManager);
            await this.GetTrayControlDataAsync(this.BayManager);
        }

        private async Task DrawerDetailsButtonMethod()
        {
            this.NavigationService.NavigateToView<DrawerActivityRefillingDetailViewModel, IDrawerActivityRefillingDetailViewModel>();
        }

        private async Task GetTrayControlDataAsync(IBayManager bayManager)
        {
            try
            {
                this.Compartments = await this.WmsDataProvider.GetTrayControlCompartmentsAsync(bayManager.CurrentMission);
                this.SelectedCompartment = this.Compartments.SingleOrDefault(c => c.Id == bayManager.CurrentMissionOperation.CompartmentId);
            }
            catch (Exception ex)
            {
                this.StatusMessageService.Notify(ex);
            }
        }

        private async Task GetViewDataAsync(IBayManager bayManager)
        {
            this.ItemImage?.Dispose();
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
