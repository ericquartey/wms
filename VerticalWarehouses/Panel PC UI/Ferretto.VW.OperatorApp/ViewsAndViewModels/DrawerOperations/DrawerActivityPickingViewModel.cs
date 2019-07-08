using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.Common.Controls.WPF;
using Ferretto.VW.MAS.AutomationService.Contracts;
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

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels.DrawerOperations
{
    public class DrawerActivityPickingViewModel : BindableBase, IDrawerActivityPickingViewModel, IDrawerActivityViewModel
    {
        #region Fields

        private readonly IBayManager bayManager;

        private readonly IEventAggregator eventAggregator;

        private readonly IFeedbackNotifier feedbackNotifier;

        private readonly IMainWindowViewModel mainWindowViewModel;

        private readonly INavigationService navigationService;

        private readonly IOperatorService operatorService;

        private readonly IWmsDataProvider wmsDataProvider;

        private readonly IWmsImagesProvider wmsImagesProvider;

        private string compartmentPosition;

        private ICommand confirmCommand;

        private ICommand drawerDetailsButtonCommand;

        private string evadedQuantity;

        private Func<IDrawableCompartment, IDrawableCompartment, string> filterColorFunc;

        private Image image;

        private string itemCode;

        private string itemDescription;

        private string listCode;

        private string listDescription;

        private string requestedQuantity;

        private TrayControlCompartment selectedCompartment;

        private ObservableCollection<TrayControlCompartment> viewCompartments;

        #endregion

        #region Constructors

        public DrawerActivityPickingViewModel(
            IEventAggregator eventAggregator,
            IFeedbackNotifier feedbackNotifier,
            IMainWindowViewModel mainWindowViewModel,
            IWmsDataProvider wmsDataProvider,
            IWmsImagesProvider wmsImagesProvider,
            IOperatorService operatorService,
            INavigationService navigationService,
            IBayManager bayManager)
        {
            if (eventAggregator == null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            if (feedbackNotifier == null)
            {
                throw new ArgumentNullException(nameof(feedbackNotifier));
            }

            if (mainWindowViewModel == null)
            {
                throw new ArgumentNullException(nameof(mainWindowViewModel));
            }

            if (wmsDataProvider == null)
            {
                throw new ArgumentNullException(nameof(wmsDataProvider));
            }

            if (wmsImagesProvider == null)
            {
                throw new ArgumentNullException(nameof(wmsImagesProvider));
            }

            if (operatorService == null)
            {
                throw new ArgumentNullException(nameof(operatorService));
            }

            if (navigationService == null)
            {
                throw new ArgumentNullException(nameof(navigationService));
            }

            if (bayManager == null)
            {
                throw new ArgumentNullException(nameof(bayManager));
            }

            this.eventAggregator = eventAggregator;
            this.feedbackNotifier = feedbackNotifier;
            this.mainWindowViewModel = mainWindowViewModel;
            this.wmsDataProvider = wmsDataProvider;
            this.wmsImagesProvider = wmsImagesProvider;
            this.operatorService = operatorService;
            this.navigationService = navigationService;
            this.bayManager = bayManager;

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
                await this.operatorService.PickAsync(this.bayManager.BayId, this.bayManager.CurrentMission.Id, quantity);
                this.bayManager.CurrentMission = null;

                this.UpdateView();
                this.EvadedQuantity = string.Empty;
            }
        }

        public void ExitFromViewMethod()
        {
            this.Image?.Dispose();
            this.image?.Dispose();
        }

        public async Task OnEnterViewAsync()
        {
            this.feedbackNotifier.Notify($"Current mission ID: {this.bayManager.CurrentMission.Id}");
            await this.GetViewDataAsync(this.bayManager);
            await this.GetTrayControlDataAsync(this.bayManager);
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
            var mission = this.bayManager.CurrentMission;
            var mainWindowContentVM = this.mainWindowViewModel.ContentRegionCurrentViewModel;
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
                            this.navigationService.NavigateToViewWithoutNavigationStack<DrawerActivityInventoryViewModel, IDrawerActivityInventoryViewModel>();
                            break;

                        case MissionType.Pick:
                            this.navigationService.NavigateToViewWithoutNavigationStack<DrawerActivityPickingViewModel, IDrawerActivityPickingViewModel>();
                            break;

                        case MissionType.Put:
                            this.navigationService.NavigateToViewWithoutNavigationStack<DrawerActivityRefillingViewModel, IDrawerActivityRefillingViewModel>();
                            break;
                    }
                }
                else
                {
                    this.navigationService.NavigateToViewWithoutNavigationStack<DrawerWaitViewModel, IDrawerWaitViewModel>();
                }
            }
        }

        private async Task DrawerDetailsButtonMethod()
        {
            var itemDetailObject = await this.wmsDataProvider.GetDrawerActivityItemDetailAsync(this.bayManager.CurrentMission);

            this.navigationService.NavigateToView<DrawerActivityPickingDetailViewModel, IDrawerActivityPickingDetailViewModel>(itemDetailObject);
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
                this.feedbackNotifier.Notify($"Cannot load data. {ex.Message}");
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
