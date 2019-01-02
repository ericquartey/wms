using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.BusinessProviders;
using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Interfaces;
using Microsoft.Practices.ServiceLocation;
using Prism.Commands;

namespace Ferretto.WMS.Modules.MasterData
{
    public class LoadingUnitEditViewModel : BaseServiceNavigationViewModel, IRefreshDataEntityViewModel
    {
        #region Fields

        private readonly ICompartmentProvider compartmentProvider = ServiceLocator.Current.GetInstance<ICompartmentProvider>();
        private readonly Func<ICompartment, ICompartment, string> filterColorFunc = new EditFilter().ColorFunc;
        private readonly ILoadingUnitProvider loadingUnitProvider = ServiceLocator.Current.GetInstance<ILoadingUnitProvider>();
        private BaseNavigationViewModel activeSideViewModel;
        private ICommand addCommand;
        private ICommand bulkAddCommand;
        private IEnumerable<CompartmentDetails> compartmentsDataSource;
        private ICommand editCommand;
        private bool isSidePanelOpen;
        private LoadingUnitDetails loadingUnit;
        private bool loadingUnitHasCompartments;
        private ICompartment selectedCompartmentTray;
        private Tray tray;

        #endregion Fields

        #region Constructors

        public LoadingUnitEditViewModel()
        {
            this.HideSidePanel();
        }

        #endregion Constructors

        #region Properties

        public BaseNavigationViewModel ActiveSideViewModel
        {
            get => this.activeSideViewModel;
            set
            {
                var prevValue = this.activeSideViewModel;
                if (this.SetProperty(ref this.activeSideViewModel, value))
                {
                    if (prevValue != null)
                    {
                        (prevValue as SidePanelDetailsViewModel<BusinessObject>).OperationComplete -= this.ActiveSideViewModel_OperationComplete;
                    }

                    if (value != null)
                    {
                        (value as SidePanelDetailsViewModel<BusinessObject>).OperationComplete += this.ActiveSideViewModel_OperationComplete;
                    }
                }
            }
        }

        public ICommand AddCommand => this.addCommand ??
            (this.addCommand = new DelegateCommand(this.ExecuteAddCompartmentCommand));

        public ICommand BulkAddCommand => this.bulkAddCommand ??
            (this.bulkAddCommand = new DelegateCommand(this.ExecuteBulkAddCommand));

        public IEnumerable<CompartmentDetails> CompartmentsDataSource
        {
            get => this.compartmentsDataSource;
            set => this.SetProperty(ref this.compartmentsDataSource, value);
        }

        public ICommand EditCommand => this.editCommand ??
            (this.editCommand = new DelegateCommand(async () => await this.ExecuteEditCompartmentCommand(), this.CanExecuteEditCommand)
            .ObservesProperty(() => this.SelectedCompartmentTray));

        public Func<ICompartment, ICompartment, string> FilterColorFunc => this.filterColorFunc;

        public bool IsSidePanelOpen
        {
            get => this.isSidePanelOpen;
            set => this.SetProperty(ref this.isSidePanelOpen, value);
        }

        public LoadingUnitDetails LoadingUnit
        {
            get => this.loadingUnit;
            set
            {
                if (this.SetProperty(ref this.loadingUnit, value))
                {
                    this.RefreshData();
                }
            }
        }

        public bool LoadingUnitHasCompartments
        {
            get => this.loadingUnitHasCompartments;
            set => this.SetProperty(ref this.loadingUnitHasCompartments, value);
        }

        public ICompartment SelectedCompartmentTray
        {
            get => this.selectedCompartmentTray;
            set => this.SetProperty(ref this.selectedCompartmentTray, value);
        }

        public Tray Tray
        {
            get => this.tray;
            set => this.SetProperty(ref this.tray, value);
        }

        #endregion Properties

        #region Methods

        public void RefreshData()
        {
            this.CompartmentsDataSource = this.loadingUnit != null
                ? this.compartmentProvider.GetByLoadingUnitId(this.loadingUnit.Id).ToList()
                : null;
        }

        protected override async void OnAppear()
        {
            await this.LoadData();
            base.OnAppear();
        }

        private async void ActiveSideViewModel_OperationComplete(Object sender, OperationEventArgs e)
        {
            this.HideSidePanel();

            if (e.IsCanceled == false)
            {
                this.SelectedCompartmentTray = null;
                await this.LoadData();

                if (e.Model is ICompartment compartment)
                {
                    this.SelectedCompartmentTray = compartment;
                }
                else if (e.Model is BulkCompartment bulk)
                {
                    this.SelectedCompartmentTray = bulk.LoadingUnit.Compartments.FirstOrDefault();
                }
            }
        }

        private bool CanExecuteEditCommand()
        {
            return this.selectedCompartmentTray != null;
        }

        private void ExecuteAddCompartmentCommand()
        {
            this.SelectedCompartmentTray = null;

            var model = this.compartmentProvider.GetNew();
            model.LoadingUnitId = this.loadingUnit.Id;
            model.LoadingUnit = this.loadingUnit;

            this.ShowSidePanel(new CompartmentAddViewModel { Model = model });
        }

        private void ExecuteBulkAddCommand()
        {
            this.SelectedCompartmentTray = null;

            var model = new BulkCompartment();
            model.LoadingUnit = this.loadingUnit;

            this.ShowSidePanel(new CompartmentAddBulkViewModel { Model = model });
        }

        private async Task ExecuteEditCompartmentCommand()
        {
            var model = await this.compartmentProvider.GetById(this.selectedCompartmentTray.Id);
            model.LoadingUnit = this.loadingUnit;

            this.ShowSidePanel(new CompartmentEditViewModel { Model = model });
        }

        private void HideSidePanel()
        {
            this.IsSidePanelOpen = false;
            this.ActiveSideViewModel = null;
        }

        private void InitializeTray()
        {
            this.tray = new Tray
            {
                Dimension = new Dimension
                {
                    Height = this.LoadingUnit.Length,
                    Width = this.LoadingUnit.Width
                },
                LoadingUnitId = this.LoadingUnit.Id,
            };

            if (this.LoadingUnit.Compartments != null)
            {
                this.tray.AddCompartmentsRange(this.LoadingUnit.Compartments);
            }

            this.RaisePropertyChanged(nameof(this.Tray));
        }

        private async Task LoadData()
        {
            if (this.Data is int modelId)
            {
                this.LoadingUnit = await this.loadingUnitProvider.GetById(modelId);
                this.InitializeTray();
            }
        }

        private void ShowSidePanel(BaseNavigationViewModel childViewModel)
        {
            this.ActiveSideViewModel = childViewModel;
            this.IsSidePanelOpen = true;
        }

        #endregion Methods
    }
}
