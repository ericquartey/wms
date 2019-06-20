using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.OperatorApp.Interfaces;
using Ferretto.VW.OperatorApp.ViewsAndViewModels.DrawerOperations;
using Ferretto.VW.OperatorApp.ViewsAndViewModels.DrawerOperations.Details;
using Unity;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Ferretto.VW.OperatorApp.ServiceUtilities.Interfaces;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Ferretto.VW.MAS_AutomationService.Contracts;
using System;
using System.Collections.ObjectModel;
using Ferretto.VW.Utils.Source;
using System.Linq;
using Ferretto.Common.Controls.WPF;
using Ferretto.VW.Utils.Source.Filters;
using Ferretto.VW.OperatorApp.ServiceUtilities;
using Ferretto.VW.WmsCommunication.Source;

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels.DrawerOperations
{
    public class DrawerActivityInventoryViewModel : BindableBase, IDrawerActivityInventoryViewModel, IDrawerActivityViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private string compartmentPosition;

        private IUnityContainer container;

        private ICommand drawerActivityInventoryDetailsButtonCommand;

        private Func<IDrawableCompartment, IDrawableCompartment, string> filterColorFunc;

        private string itemCode;

        private string itemDescription;

        private IItemsDataService itemsDataService;

        private string listCode;

        private string listDescription;

        private ILoadingUnitsDataService loadingUnitsDataService;

        private IMaterialStatusesDataService materialStatusesDataService;

        private IOperatorService operatorService;

        private IPackageTypesDataService packageTypesDataService;

        private TrayControlCompartment selectedCompartment;

        private string storedQuantity;

        private ObservableCollection<TrayControlCompartment> viewCompartments;

        #endregion

        #region Constructors

        public DrawerActivityInventoryViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
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
            get { return this.filterColorFunc; }
            set { this.SetProperty<Func<IDrawableCompartment, IDrawableCompartment, string>>(ref this.filterColorFunc, value); }
        }

        public string ItemCode { get => this.itemCode; set => this.SetProperty(ref this.itemCode, value); }

        public string ItemDescription { get => this.itemDescription; set => this.SetProperty(ref this.itemDescription, value); }

        public string ListCode { get => this.listCode; set => this.SetProperty(ref this.listCode, value); }

        public string ListDescription { get => this.listDescription; set => this.SetProperty(ref this.listDescription, value); }

        public BindableBase NavigationViewModel { get; set; }

        public TrayControlCompartment SelectedCompartment { get => this.selectedCompartment; set => this.SetProperty(ref this.selectedCompartment, value); }

        public string StoredQuantity { get => this.storedQuantity; set => this.SetProperty(ref this.storedQuantity, value); }

        public ObservableCollection<TrayControlCompartment> ViewCompartments { get => this.viewCompartments; set => this.SetProperty(ref this.viewCompartments, value); }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            // TODO
        }

        public void InitializeViewModel(IUnityContainer container)
        {
            this.container = container;
            //this.loadingUnitsDataService = this.container.Resolve<ILoadingUnitsDataService>();
            //this.operatorService = this.container.Resolve<IOperatorService>();
            //this.itemsDataService = this.container.Resolve<IItemsDataService>();
            //this.materialStatusesDataService = this.container.Resolve<IMaterialStatusesDataService>();
            //this.packageTypesDataService = this.container.Resolve<IPackageTypesDataService>();
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
            var item = await this.itemsDataService.GetByIdAsync((int)bayManager.CurrentMission.ItemId);
            var compartments = await this.loadingUnitsDataService.GetCompartmentsAsync((int)bayManager.CurrentMission.LoadingUnitId);
            var compartment = compartments.First(x => x.Id == bayManager.CurrentMission.CompartmentId);
            var materialStatus = await this.materialStatusesDataService.GetByIdAsync((int)compartment.MaterialStatusId);
            var packageType = await this.packageTypesDataService.GetByIdAsync((int)compartment.PackageTypeId);
            var itemDetailObject = new DrawerActivityItemDetail
            {
                Batch = compartment.Lot,
                ItemCode = item.Code,
                ItemDescription = item.Description,
                ListCode = bayManager.CurrentMission.ItemListRowCode,
                ListDescription = bayManager.CurrentMission.ItemListDescription,
                ListRow = bayManager.CurrentMission.ItemListRowId.ToString(),
                MaterialStatus = materialStatus.Description,
                PackageType = packageType.Description,
                Position = $"{compartment.XPosition}, {compartment.YPosition}",
                ProductionDate = item.CreationDate.ToShortDateString(),
                RequestedQuantity = bayManager.CurrentMission.RequestedQuantity.ToString()
            };

            NavigationService.NavigateToView<DrawerActivityInventoryDetailViewModel, IDrawerActivityInventoryDetailViewModel>(itemDetailObject);
        }

        private async Task GetTrayControlDataAsync(IBayManager bayManager)
        {
            try
            {
                var loadingUnitId = (int)bayManager.CurrentMission.LoadingUnitId;
                var compartmentId = (int)bayManager.CurrentMission.CompartmentId;
                var compartments = await this.loadingUnitsDataService.GetCompartmentsAsync(loadingUnitId);
                if (compartments != null && compartments.Count > 0)
                {
                    this.ViewCompartments = new ObservableCollection<TrayControlCompartment>(compartments.Select(x => new TrayControlCompartment
                    {
                        Height = x.Height,
                        Id = x.Id,
                        LoadingUnitId = x.LoadingUnitId,
                        Width = x.Width,
                        XPosition = x.XPosition,
                        YPosition = x.YPosition
                    }));
                    this.SelectedCompartment = this.ViewCompartments.First(x => x.Id == compartmentId);
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException(ex.Message);
            }
        }

        private async Task GetViewDataAsync(IBayManager bayManager)
        {
            this.ListCode = bayManager.CurrentMission.ItemListId.ToString(); // TODO Check if it's the desired value (which is list's Id)
            this.ItemCode = bayManager.CurrentMission.ItemId.ToString();
            var compartments = await this.loadingUnitsDataService.GetCompartmentsAsync((int)bayManager.CurrentMission.LoadingUnitId);
            var compartment = compartments.First(x => x.Id == (int)bayManager.CurrentMission.CompartmentId);
            var compartmentXpos = compartment.XPosition;
            var compartmentYpos = compartment.YPosition;
            this.CompartmentPosition = $"{compartmentXpos}, {compartmentYpos}";
            this.ListDescription = bayManager.CurrentMission.ItemListDescription;
            this.ItemDescription = bayManager.CurrentMission.ItemDescription;
            this.StoredQuantity = bayManager.CurrentMission.RequestedQuantity.ToString();
        }

        #endregion
    }
}
