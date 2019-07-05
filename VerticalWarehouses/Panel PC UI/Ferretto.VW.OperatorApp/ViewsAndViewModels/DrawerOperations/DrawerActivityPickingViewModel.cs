using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.Common.Controls.WPF;
using Ferretto.VW.MAS_AutomationService.Contracts;
using Ferretto.VW.OperatorApp.Interfaces;
using Ferretto.VW.OperatorApp.ServiceUtilities.Interfaces;
using Ferretto.VW.OperatorApp.ViewsAndViewModels.DrawerOperations.Details;
using Ferretto.VW.Utils.Source.Filters;
using Ferretto.VW.WmsCommunication.Interfaces;
using Ferretto.VW.WmsCommunication.Source;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Unity;

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels.DrawerOperations
{
    public class DrawerActivityPickingViewModel : BindableBase, IDrawerActivityPickingViewModel, IDrawerActivityViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private string compartmentPosition;

        private ICommand confirmCommand;

        private IUnityContainer container;

        private ICommand drawerDetailsButtonCommand;

        private string evadedQuantity;

        private Func<IDrawableCompartment, IDrawableCompartment, string> filterColorFunc;

        private Image image;

        private string itemCode;

        private string itemDescription;

        private string listCode;

        private string listDescription;

        private IOperatorService operatorService;

        private string requestedQuantity;

        private TrayControlCompartment selectedCompartment;

        private ObservableCollection<TrayControlCompartment> viewCompartments;

        private IWmsDataProvider wmsDataProvider;

        private IWmsImagesProvider wmsImagesProvider;

        #endregion

        #region Constructors

        public DrawerActivityPickingViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.NavigationViewModel = null;
            this.filterColorFunc = new EditFilter().ColorFunc;
        }

        #endregion

        #region Properties

        public string CompartmentPosition { get => this.compartmentPosition; set => this.SetProperty(ref this.compartmentPosition, value); }

        public ICommand ConfirmCommand => this.confirmCommand ?? (this.confirmCommand = new DelegateCommand(() => this.ConfirmMethod()));

        public ICommand DrawerDetailsButtonCommand => this.drawerDetailsButtonCommand ?? (this.drawerDetailsButtonCommand = new DelegateCommand(
            async () => await this.DrawerDetailsButtonMethod()));

        public string EvadedQuantity { get => this.evadedQuantity; set => this.SetProperty(ref this.evadedQuantity, value); }

        public Func<IDrawableCompartment, IDrawableCompartment, string> FilterColorFunc
        {
            get => this.filterColorFunc;
            set => this.SetProperty<Func<IDrawableCompartment, IDrawableCompartment, string>>(ref this.filterColorFunc, value);
        }

        public Image Image { get => this.image; set => this.SetProperty(ref this.image, value); }

        public string ItemCode { get => this.itemCode; set => this.SetProperty(ref this.itemCode, value); }

        public string ItemDescription { get => this.itemDescription; set => this.SetProperty(ref this.itemDescription, value); }

        public string ListCode { get => this.listCode; set => this.SetProperty(ref this.listCode, value); }

        public string ListDescription { get => this.listDescription; set => this.SetProperty(ref this.listDescription, value); }

        public BindableBase NavigationViewModel { get; set; }

        public string RequestedQuantity { get => this.requestedQuantity; set => this.SetProperty(ref this.requestedQuantity, value); }

        public TrayControlCompartment SelectedCompartment { get => this.selectedCompartment; set => this.SetProperty(ref this.selectedCompartment, value); }

        public ObservableCollection<TrayControlCompartment> ViewCompartments { get => this.viewCompartments; set => this.SetProperty(ref this.viewCompartments, value); }

        #endregion

        #region Methods

        public async Task ConfirmMethod()
        {
            if (int.TryParse(this.EvadedQuantity, out var quantity) && quantity >= 0)
            {
                var bay = this.container.Resolve<IBayManager>();
                await this.operatorService.PickAsync(bay.BayId, bay.CurrentMission.Id, quantity);
                this.container.Resolve<IBayManager>().CurrentMission = null;
                this.UpdateView();
                this.EvadedQuantity = string.Empty;
            }
        }

        public void ExitFromViewMethod()
        {
            this.Image?.Dispose();
            this.image?.Dispose();
        }

        public void InitializeViewModel(IUnityContainer container)
        {
            this.container = container;
            this.wmsDataProvider = this.container.Resolve<IWmsDataProvider>();
            this.operatorService = this.container.Resolve<IOperatorService>();
            this.wmsImagesProvider = this.container.Resolve<IWmsImagesProvider>();
        }

        public async Task OnEnterViewAsync()
        {
            var bayManager = this.container.Resolve<IBayManager>();
            this.container.Resolve<IFeedbackNotifier>().Notify($"Current mission ID: {this.container.Resolve<IBayManager>().CurrentMission.Id}");
            await this.GetViewDataAsync(bayManager);
            await this.GetTrayControlDataAsync(bayManager);
        }

        public void SubscribeMethodToEvent()
        {
            // TODO
        }

        public void UnSubscribeMethodFromEvent()
        {
            // TODO
        }

        public void UpdateView()
        {
            var mission = this.container.Resolve<IBayManager>().CurrentMission;
            var mainWindowContentVM = this.container.Resolve<IMainWindowViewModel>().ContentRegionCurrentViewModel;
            if (mainWindowContentVM is DrawerActivityInventoryViewModel ||
                mainWindowContentVM is DrawerActivityPickingViewModel ||
                mainWindowContentVM is DrawerActivityRefillingViewModel ||
                mainWindowContentVM is DrawerWaitViewModel)
            {
                if (mission != null)
                {
                    switch (mission.Type)
                    {
                        case MissionType.Inventory:
                            NavigationService.NavigateToViewWithoutNavigationStack<DrawerActivityInventoryViewModel, IDrawerActivityInventoryViewModel>();
                            break;

                        case MissionType.Pick:
                            NavigationService.NavigateToViewWithoutNavigationStack<DrawerActivityPickingViewModel, IDrawerActivityPickingViewModel>();
                            break;

                        case MissionType.Put:
                            NavigationService.NavigateToViewWithoutNavigationStack<DrawerActivityRefillingViewModel, IDrawerActivityRefillingViewModel>();
                            break;
                    }
                }
                else
                {
                    NavigationService.NavigateToViewWithoutNavigationStack<DrawerWaitViewModel, IDrawerWaitViewModel>();
                }
            }
        }

        private async Task DrawerDetailsButtonMethod()
        {
            var bayManager = this.container.Resolve<IBayManager>();
            var itemDetailObject = await this.wmsDataProvider.GetDrawerActivityItemDetailAsync(bayManager.CurrentMission);

            NavigationService.NavigateToView<DrawerActivityPickingDetailViewModel, IDrawerActivityPickingDetailViewModel>(itemDetailObject);
        }

        private async Task GetTrayControlDataAsync(IBayManager bayManager)
        {
            try
            {
                this.ViewCompartments = await this.wmsDataProvider.GetTrayControlCompartmentsAsync(bayManager.CurrentMission);
                this.SelectedCompartment = await this.wmsDataProvider.GetTrayControlSelectedCompartment(this.ViewCompartments, bayManager.CurrentMission);
            }
            catch (Exception ex)
            {
                throw new ApplicationException(ex.Message);
            }
        }

        private async Task GetViewDataAsync(IBayManager bayManager)
        {
            this.Image?.Dispose();
            this.image?.Dispose();
            this.Image = null;
            this.image = null;
            this.ListCode = bayManager.CurrentMission.ItemListId.ToString();
            this.ItemCode = bayManager.CurrentMission.ItemId.ToString();
            this.CompartmentPosition = await this.wmsDataProvider.GetCompartmentPosition(bayManager.CurrentMission);
            this.ListDescription = bayManager.CurrentMission.ItemListDescription;
            this.ItemDescription = bayManager.CurrentMission.ItemDescription;
            this.RequestedQuantity = bayManager.CurrentMission.RequestedQuantity.ToString();
            var imageCode = await this.wmsDataProvider.GetItemImageCodeAsync((int)bayManager.CurrentMission.ItemId);
            var imageStram = await this.wmsImagesProvider.GetImageAsync(imageCode);
            this.Image = Image.FromStream(imageStram);
        }

        #endregion
    }
}
