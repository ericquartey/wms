using System.Windows.Input;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.BusinessProviders;
using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Interfaces;
using Ferretto.Common.Controls.Services;
using Ferretto.Common.Modules.BLL.Models;
using Microsoft.Practices.ServiceLocation;
using Prism.Commands;

namespace Ferretto.WMS.Modules.MasterData
{
    public class CompartmentDetailsViewModel : BaseServiceNavigationViewModel, IRefreshDataEntityViewModel
    {
        #region Fields

        private readonly ICompartmentProvider compartmentProvider = ServiceLocator.Current.GetInstance<ICompartmentProvider>();
        private readonly ILoadingUnitProvider loadingUnitProvider = ServiceLocator.Current.GetInstance<ILoadingUnitProvider>();
        private readonly IItemProvider itemProvider = ServiceLocator.Current.GetInstance<IItemProvider>();
        private IDataSource<AllowedItemInCompartment> allowedItemsDataSource;
        private IDataSource<LoadingUnit> loadingUnitsDataSource;
        private CompartmentDetails compartment;
        private bool isCompartmentSelectableTray;
        private object modelChangedEventSubscription;
        private object modelRefreshSubscription;
        private object modelSelectionChangedSubscription;
        private bool readOnlyTray;
        private ICommand revertCommand;
        private ICommand saveCommand;

        private CompartmentDetails selectedCompartmentTray;
        private Tray tray;

        #endregion Fields

        #region Constructors

        public CompartmentDetailsViewModel()
        {
            this.Initialize();
        }

        #endregion Constructors

        #region Properties

        public IDataSource<AllowedItemInCompartment> AllowedItemsDataSource
        {
            get => this.allowedItemsDataSource;
            set => this.SetProperty(ref this.allowedItemsDataSource, value);
        }

        public IDataSource<LoadingUnit> LoadingUnitsDataSource
        {
            get => this.loadingUnitsDataSource;
            set => this.SetProperty(ref this.loadingUnitsDataSource, value);
        }

        public CompartmentDetails Compartment
        {
            get => this.compartment;
            set
            {
                if (this.SetProperty(ref this.compartment, value))
                {
                    var loadingUnit = this.loadingUnitProvider.GetById(this.compartment.LoadingUnitId);
                    this.InitializeTray(loadingUnit);
                    this.SetSelectedCompartment();
                    this.RefreshData();
                }
            }
        }

        public bool IsCompartmentSelectableTray
        {
            get => this.isCompartmentSelectableTray;
            set => this.SetProperty(ref this.isCompartmentSelectableTray, value);
        }

        public bool ReadOnlyTray
        {
            get => this.readOnlyTray;
            set => this.SetProperty(ref this.readOnlyTray, value);
        }

        public ICommand RevertCommand => this.revertCommand ??
                          (this.revertCommand = new DelegateCommand(this.LoadData));

        public ICommand SaveCommand => this.saveCommand ??
                  (this.saveCommand = new DelegateCommand(this.ExecuteSaveCommand));

        public CompartmentDetails SelectedCompartmentTray
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
            this.AllowedItemsDataSource = this.compartment != null
                ? new DataSource<AllowedItemInCompartment>(() => this.itemProvider.GetAllowedByCompartmentId(this.compartment.Id))
                : null;

            this.LoadingUnitsDataSource = new DataSource<LoadingUnit>(() => this.loadingUnitProvider.GetAll());
        }

        protected override void OnAppear()
        {
            this.LoadData();
            base.OnAppear();
        }

        protected override void OnDispose()
        {
            this.EventService.Unsubscribe<RefreshModelsEvent<Compartment>>(this.modelRefreshSubscription);
            this.EventService.Unsubscribe<ModelChangedEvent<Compartment>>(this.modelChangedEventSubscription);
            this.EventService.Unsubscribe<ModelSelectionChangedEvent<Compartment>>(this.modelSelectionChangedSubscription);
            base.OnDispose();
        }

        private void ExecuteSaveCommand()
        {
            var modifiedRowCount = this.compartmentProvider.Save(this.Compartment);

            if (modifiedRowCount > 0)
            {
                this.EventService.Invoke(new ModelChangedEvent<Compartment>(this.Compartment.Id));

                this.EventService.Invoke(new StatusEventArgs(Common.Resources.MasterData.CompartmentSavedSuccessfully));
            }
        }

        private void Initialize()
        {
            this.modelRefreshSubscription = this.EventService.Subscribe<RefreshModelsEvent<Compartment>>(eventArgs => { this.LoadData(); }, this.Token, true, true);
            this.modelChangedEventSubscription = this.EventService.Subscribe<ModelChangedEvent<Compartment>>(eventArgs => { this.LoadData(); });
            this.modelSelectionChangedSubscription = this.EventService.Subscribe<ModelSelectionChangedEvent<Compartment>>(
                eventArgs =>
                {
                    if (eventArgs.ModelId.HasValue)
                    {
                        this.Data = eventArgs.ModelId.Value;
                        this.LoadData();
                    }
                    else
                    {
                        this.Compartment = null;
                    }
                },
                this.Token,
                true,
                true);
        }

        private void InitializeTray(LoadingUnitDetails loadingUnit)
        {
            this.tray = new Tray
            {
                Dimension = new Dimension
                {
                    Height = loadingUnit.Length,
                    Width = loadingUnit.Width
                }
            };
            if (loadingUnit.Compartments != null)
            {
                this.tray.AddCompartmentsRange(loadingUnit.Compartments);
            }
            this.RaisePropertyChanged(nameof(this.Tray));

            this.readOnlyTray = true;
            this.isCompartmentSelectableTray = false;
            this.RaisePropertyChanged(nameof(this.ReadOnlyTray));
            this.RaisePropertyChanged(nameof(this.IsCompartmentSelectableTray));
        }

        private void LoadData()
        {
            if (this.Data is int modelId)
            {
                this.Compartment = this.compartmentProvider.GetById(modelId);
            }
        }

        private void SetSelectedCompartment()
        {
            this.selectedCompartmentTray = this.Compartment;
            this.RaisePropertyChanged(nameof(this.SelectedCompartmentTray));
        }

        #endregion Methods
    }
}
