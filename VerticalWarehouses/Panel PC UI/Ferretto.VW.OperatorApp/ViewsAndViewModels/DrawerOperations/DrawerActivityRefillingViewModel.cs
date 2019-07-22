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
using Ferretto.VW.Utils.Source.Filters;
using Ferretto.VW.WmsCommunication.Interfaces;
using Ferretto.VW.WmsCommunication.Source;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels.DrawerOperations
{
    public class DrawerActivityRefillingViewModel : BaseViewModel, IDrawerActivityRefillingViewModel, IDrawerActivityViewModel
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

        private int actualQuantity;

        private ICommand confirmCommand;

        private ICommand drawerActivityRefillingDetailsButtonCommand;

        private Func<IDrawableCompartment, IDrawableCompartment, string> filterColorFunc;

        private Image itemImage;

        private MissionOperationInfo missionOperation;

        private TrayControlCompartment selectedCompartment;

        private ObservableCollection<TrayControlCompartment> viewCompartments;

        #endregion

        #region Constructors

        public DrawerActivityRefillingViewModel(
            IEventAggregator eventAggregator,
            INavigationService navigationService,
            IStatusMessageService statusMessageService,
            IMainWindowViewModel mainWindowViewModel,
            IWmsDataProvider wmsDataProvider,
            IWmsImagesProvider wmsImagesProvider,
            IOperatorService operatorService,
            IBayManager bayManager)
        {
            if (eventAggregator == null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            if (navigationService == null)
            {
                throw new ArgumentNullException(nameof(navigationService));
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

            if (bayManager == null)
            {
                throw new ArgumentNullException(nameof(bayManager));
            }

            this.eventAggregator = eventAggregator;
            this.navigationService = navigationService;
            this.statusMessageService = statusMessageService;
            this.mainWindowViewModel = mainWindowViewModel;
            this.wmsDataProvider = wmsDataProvider;
            this.wmsImagesProvider = wmsImagesProvider;
            this.operatorService = operatorService;
            this.bayManager = bayManager;

            this.NavigationViewModel = null;
            this.filterColorFunc = new EditFilter().ColorFunc;
        }

        #endregion

        #region Properties

        public int ActualQuantity { get => this.actualQuantity; set => this.SetProperty(ref this.actualQuantity, value); }

        public string CompartmentPosition => null;

        public ICommand ConfirmCommand =>
            this.confirmCommand
            ??
            (this.confirmCommand = new DelegateCommand(async () => await this.ExecuteConfirmCommand()));

        public ICommand DrawerActivityRefillingDetailsButtonCommand => this.drawerActivityRefillingDetailsButtonCommand ?? (this.drawerActivityRefillingDetailsButtonCommand = new DelegateCommand(
            async () => await this.DrawerDetailsButtonMethod()));

        public Func<IDrawableCompartment, IDrawableCompartment, string> FilterColorFunc
        {
            get => this.filterColorFunc;
            set => this.SetProperty<Func<IDrawableCompartment, IDrawableCompartment, string>>(ref this.filterColorFunc, value);
        }

        public Image ItemImage { get => this.itemImage; set => this.SetProperty(ref this.itemImage, value); }

        public MissionOperationInfo MissionOperation { get => this.missionOperation; set => this.SetProperty(ref this.missionOperation, value); }

        public TrayControlCompartment SelectedCompartment { get => this.selectedCompartment; set => this.SetProperty(ref this.selectedCompartment, value); }

        public ObservableCollection<TrayControlCompartment> ViewCompartments { get => this.viewCompartments; set => this.SetProperty(ref this.viewCompartments, value); }

        #endregion

        #region Methods

        public async Task ExecuteConfirmCommand()
        {
            // TODO add validation
            if (this.ActualQuantity >= 0)
            {
                await this.operatorService.CompleteAsync(this.bayManager.BayId, this.bayManager.CurrentMission.Id, this.ActualQuantity, this.MissionOperation.Id);
                this.bayManager.CurrentMission = null;

                this.UpdateView();
                this.EvadedQuantity = string.Empty;
            }
        }

        public override async Task OnEnterViewAsync()
        {
            this.statusMessageService.Notify($"Current mission ID: {this.bayManager.CurrentMission.Id}");
            await this.GetViewDataAsync(this.bayManager);
            await this.GetTrayControlDataAsync(this.bayManager);
        }

        public void UpdateView()
        {
            var missionOperation = this.bayManager.CurrentMissionOperation;
            var mainWindowContentVM = this.mainWindowViewModel.ContentRegionCurrentViewModel;
            if (mainWindowContentVM is DrawerActivityInventoryViewModel ||
                mainWindowContentVM is DrawerActivityPickingViewModel ||
                mainWindowContentVM is DrawerActivityRefillingViewModel ||
                mainWindowContentVM is DrawerWaitViewModel)
            {
                if (missionOperation != null)
                {
                    switch (missionOperation.Type)
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
            this.navigationService.NavigateToView<DrawerActivityRefillingDetailViewModel, IDrawerActivityRefillingDetailViewModel>();
        }

        private async Task GetTrayControlDataAsync(IBayManager bayManager)
        {
            try
            {
                this.ViewCompartments = await this.wmsDataProvider.GetTrayControlCompartmentsAsync(bayManager.CurrentMission);
                this.SelectedCompartment = this.ViewCompartments.SingleOrDefault(c => c.Id == bayManager.CurrentMissionOperation.CompartmentId);
            }
            catch (Exception ex)
            {
                this.statusMessageService.Notify(ex);
            }
        }

        private async Task GetViewDataAsync(IBayManager bayManager)
        {
            this.ItemImage?.Dispose();
            this.itemImage?.Dispose();
            this.ItemImage = null;
            this.itemImage = null;

            if (this.bayManager.CurrentMissionOperation != null)
            {
                var imageStram = await this.wmsImagesProvider.GetImageAsync(this.bayManager.CurrentMissionOperation.ItemImage);
                this.ItemImage = Image.FromStream(imageStram);
            }
        }

        #endregion
    }
}
