using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.Common.Controls.WPF;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.App.Operator.Interfaces;
using Ferretto.VW.App.Operator.ServiceUtilities.Interfaces;
using Ferretto.VW.App.Operator.ViewsAndViewModels.DrawerOperations.Details;
using Ferretto.VW.App.Services.Interfaces;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Source.Filters;
using Ferretto.VW.WmsCommunication.Interfaces;
using Ferretto.VW.WmsCommunication.Source;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Operator.ViewsAndViewModels.DrawerOperations
{
    public class DrawerActivityInventoryViewModel : BaseViewModel, IDrawerActivityInventoryViewModel, IDrawerActivityViewModel
    {
        #region Fields

        private readonly IBayManager bayManager;

        private readonly IEventAggregator eventAggregator;

        private readonly INavigationService navigationService;

        private string compartmentPosition;

        private ICommand drawerActivityInventoryDetailsButtonCommand;

        private Func<IDrawableCompartment, IDrawableCompartment, string> filterColorFunc;

        private Image image;

        private string itemCode;

        private string itemDescription;

        private string listCode;

        private string listDescription;

        private IMainWindowViewModel mainWindowViewModel;

        private IOperatorService operatorService;

        private TrayControlCompartment selectedCompartment;

        private IStatusMessageService statusMessageService;

        private string storedQuantity;

        private ObservableCollection<TrayControlCompartment> viewCompartments;

        private IWmsDataProvider wmsDataProvider;

        private IWmsImagesProvider wmsImagesProvider;

        #endregion

        #region Constructors

        public DrawerActivityInventoryViewModel(
            IEventAggregator eventAggregator,
            INavigationService navigationService)
        {
            if (eventAggregator == null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            if (navigationService == null)
            {
                throw new ArgumentNullException(nameof(navigationService));
            }

            this.eventAggregator = eventAggregator;
            this.navigationService = navigationService;
            this.NavigationViewModel = null;
            this.filterColorFunc = new EditFilter().ColorFunc;
        }

        #endregion

        #region Properties

        public string CompartmentPosition { get => this.compartmentPosition; set => this.SetProperty(ref this.compartmentPosition, value); }

        public ICommand DrawerActivityInventoryDetailsButtonCommand => this.drawerActivityInventoryDetailsButtonCommand ?? (this.drawerActivityInventoryDetailsButtonCommand = new DelegateCommand(
            async () => await this.DrawerDetailsButtonMethod()));

        public Func<IDrawableCompartment, IDrawableCompartment, string> FilterColorFunc
        {
            get => this.filterColorFunc;
            set => this.SetProperty(ref this.filterColorFunc, value);
        }

        public Image Image { get => this.image; set => this.SetProperty(ref this.image, value); }

        public string ItemCode { get => this.itemCode; set => this.SetProperty(ref this.itemCode, value); }

        public string ItemDescription { get => this.itemDescription; set => this.SetProperty(ref this.itemDescription, value); }

        public string ListCode { get => this.listCode; set => this.SetProperty(ref this.listCode, value); }

        public string ListDescription { get => this.listDescription; set => this.SetProperty(ref this.listDescription, value); }

        public TrayControlCompartment SelectedCompartment { get => this.selectedCompartment; set => this.SetProperty(ref this.selectedCompartment, value); }

        public string StoredQuantity { get => this.storedQuantity; set => this.SetProperty(ref this.storedQuantity, value); }

        public ObservableCollection<TrayControlCompartment> ViewCompartments { get => this.viewCompartments; set => this.SetProperty(ref this.viewCompartments, value); }

        #endregion

        #region Methods

        public void InitializeViewModel(
            IBayManager bayManager,
            IStatusMessageService statusMessageService,
            IOperatorService operatorService,
            IWmsDataProvider wmsDataProvider,
            IWmsImagesProvider wmsImagesProvider,
            IMainWindowViewModel mainWindowViewModel)
        {
            if (bayManager == null)
            {
                throw new ArgumentNullException(nameof(bayManager));
            }

            if (statusMessageService == null)
            {
                throw new ArgumentNullException(nameof(statusMessageService));
            }

            if (operatorService == null)
            {
                throw new ArgumentNullException(nameof(operatorService));
            }

            if (wmsDataProvider == null)
            {
                throw new ArgumentNullException(nameof(wmsDataProvider));
            }

            if (mainWindowViewModel == null)
            {
                throw new ArgumentNullException(nameof(mainWindowViewModel));
            }

            this.operatorService = operatorService;
            this.wmsDataProvider = wmsDataProvider;
            this.wmsImagesProvider = wmsImagesProvider;
            this.statusMessageService = statusMessageService;
            this.mainWindowViewModel = mainWindowViewModel;
        }

        public override async Task OnEnterViewAsync()
        {
            this.statusMessageService.Notify($"Current mission ID: {this.bayManager.CurrentMission.Id}");
            await this.GetViewDataAsync(this.bayManager);
            await this.GetTrayControlDataAsync(this.bayManager);
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

            this.navigationService.NavigateToView<DrawerActivityInventoryDetailViewModel, IDrawerActivityInventoryDetailViewModel>(itemDetailObject);
        }

        private async Task GetTrayControlDataAsync(IBayManager bayManager)
        {
            try
            {
                this.ViewCompartments = await this.wmsDataProvider.GetTrayControlCompartmentsAsync(bayManager.CurrentMission);
                this.SelectedCompartment = this.wmsDataProvider.GetTrayControlSelectedCompartment(this.ViewCompartments, bayManager.CurrentMission);
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
            this.ListCode = bayManager.CurrentMission.ItemListId.ToString(); // TODO Check if it's the desired value (which is list's Id)
            this.ItemCode = bayManager.CurrentMission.ItemId.ToString();
            this.CompartmentPosition = await this.wmsDataProvider.GetCompartmentPosition(bayManager.CurrentMission);
            this.ListDescription = bayManager.CurrentMission.ItemListDescription;
            this.ItemDescription = bayManager.CurrentMission.ItemDescription;
            this.StoredQuantity = bayManager.CurrentMission.RequestedQuantity.ToString();
            var imageCode = await this.wmsDataProvider.GetItemImageCodeAsync((int)bayManager.CurrentMission.ItemId);
            var imageStram = await this.wmsImagesProvider.GetImageAsync(imageCode);
            this.Image = Image.FromStream(imageStram);
        }

        #endregion
    }
}
