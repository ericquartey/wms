using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.Common.Controls.WPF;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.OperatorApp.Interfaces;
using Ferretto.VW.OperatorApp.ViewsAndViewModels.DrawerOperations.Details;
using Ferretto.VW.WmsCommunication.Interfaces;
using Ferretto.VW.WmsCommunication.Source;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels.DrawerOperations
{
    public class DrawerActivityPickingViewModel : BaseViewModel, IDrawerActivityPickingViewModel, IDrawerActivityViewModel
    {
        #region Fields

        private readonly IBayManager bayManager;

        private readonly IEventAggregator eventAggregator;

        private readonly IMainWindowViewModel mainWindowViewModel;

        private readonly INavigationService navigationService;

        private readonly IOperatorService operatorService;

        private readonly IStatusMessageService statusMessageService;

        private readonly IWmsDataProvider wmsDataProvider;

        private readonly IWmsImagesProvider wmsImagesProvider;

        private ICommand confirmCommand;

        private ICommand drawerDetailsButtonCommand;

        private string evadedQuantity;

        private Func<IDrawableCompartment, IDrawableCompartment, string> filterColorFunc;

        private Image image;

        private TrayControlCompartment selectedCompartment;

        private ObservableCollection<TrayControlCompartment> viewCompartments;

        #endregion

        #region Constructors

        public DrawerActivityPickingViewModel(
            IEventAggregator eventAggregator,
            IStatusMessageService statusMessageService,
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

            if (statusMessageService == null)
            {
                throw new ArgumentNullException(nameof(statusMessageService));
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
            this.statusMessageService = statusMessageService;
            this.mainWindowViewModel = mainWindowViewModel;
            this.wmsDataProvider = wmsDataProvider;
            this.wmsImagesProvider = wmsImagesProvider;
            this.operatorService = operatorService;
            this.navigationService = navigationService;
            this.bayManager = bayManager;

            this.NavigationViewModel = null;
            //this.filterColorFunc = new EditFilter().ColorFunc;
        }

        #endregion

        #region Properties

        public ICommand ConfirmCommand =>
            this.confirmCommand
            ??
            (this.confirmCommand = new DelegateCommand(async () => await this.ConfirmMethod()));

        public ICommand DrawerDetailsButtonCommand =>
            this.drawerDetailsButtonCommand
            ??
            (this.drawerDetailsButtonCommand = new DelegateCommand(this.NavigateToOperationDetails));

        public string EvadedQuantity
        {
            get => this.evadedQuantity;
            set => this.SetProperty(ref this.evadedQuantity, value);
        }

        public Func<IDrawableCompartment, IDrawableCompartment, string> FilterColorFunc
        {
            get => this.filterColorFunc;
            set => this.SetProperty<Func<IDrawableCompartment, IDrawableCompartment, string>>(ref this.filterColorFunc, value);
        }

        public Image Image { get => this.image; set => this.SetProperty(ref this.image, value); }

        public MissionOperation Operation { get; set; }

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

        public override void ExitFromViewMethod()
        {
            this.Image?.Dispose();
            this.image?.Dispose();
        }

        public override async Task OnEnterViewAsync()
        {
            this.statusMessageService.Notify($"Current mission ID: {this.bayManager.CurrentMission.Id}");
            await this.GetViewDataAsync(this.bayManager);
            await this.GetTrayControlDataAsync(this.bayManager);
        }

        public void UpdateView()
        {
            var operation = this.bayManager.CurrentMissionOperation;

            var mainWindowContentVM = this.mainWindowViewModel.ContentRegionCurrentViewModel;
            if (mainWindowContentVM is DrawerActivityInventoryViewModel ||
                mainWindowContentVM is DrawerActivityPickingViewModel ||
                mainWindowContentVM is DrawerActivityRefillingViewModel ||
                mainWindowContentVM is DrawerWaitViewModel)
            {
                if (operation != null)
                {
                    switch (operation.Type)
                    {
                        case MissionOperationType.Inventory:
                            this.navigationService.NavigateToViewWithoutNavigationStack<DrawerActivityInventoryViewModel, IDrawerActivityInventoryViewModel>();
                            break;

                        case MissionOperationType.Pick:
                            this.navigationService.NavigateToViewWithoutNavigationStack<DrawerActivityPickingViewModel, IDrawerActivityPickingViewModel>();
                            break;

                        case MissionOperationType.Put:
                            this.navigationService.NavigateToViewWithoutNavigationStack<DrawerActivityRefillingViewModel, IDrawerActivityRefillingViewModel>();
                            break;

                        default:
                            // TODO skip this mission operation and move to the next one
                            this.statusMessageService.Notify("Invalid mission operation type.");
                            break;
                    }
                }
                else
                {
                    this.navigationService.NavigateToViewWithoutNavigationStack<DrawerWaitViewModel, IDrawerWaitViewModel>();
                }
            }
        }

        private async Task GetTrayControlDataAsync(IBayManager bayManager)
        {
            try
            {
                this.ViewCompartments = await this.wmsDataProvider.GetTrayControlCompartmentsAsync(bayManager.CurrentMission);

                this.SelectedCompartment = this.ViewCompartments
                    .SingleOrDefault(c => c.Id == bayManager.CurrentMissionOperation.CompartmentId);
            }
            catch (Exception ex)
            {
                this.statusMessageService.Notify(ex, $"Cannot load data.");
            }
        }

        private async Task GetViewDataAsync(IBayManager bayManager)
        {
            this.Image?.Dispose();
            this.image?.Dispose();
            this.Image = null;
            this.image = null;
            var imageCode = await this.wmsDataProvider.GetItemImageCodeAsync(bayManager.CurrentMissionOperation.ItemImage);
            var imageStram = await this.wmsImagesProvider.GetImageAsync(imageCode);
            this.Image = Image.FromStream(imageStram);
        }

        private void NavigateToOperationDetails()
        {
            this.navigationService.NavigateToView<DrawerActivityPickingDetailViewModel, IDrawerActivityPickingDetailViewModel>();
        }

        #endregion
    }
}
