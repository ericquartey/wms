using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.BusinessProviders;
using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Services;
using Ferretto.Common.Modules.BLL.Models;
using Microsoft.Practices.ServiceLocation;

namespace Ferretto.WMS.Modules.MasterData
{
    public class CompartmentDetailsViewModel : DetailsViewModel<CompartmentDetails>
    {
        #region Fields

        private readonly ICompartmentProvider compartmentProvider = ServiceLocator.Current.GetInstance<ICompartmentProvider>();

        private readonly IItemProvider itemProvider = ServiceLocator.Current.GetInstance<IItemProvider>();

        private readonly ILoadingUnitProvider loadingUnitProvider = ServiceLocator.Current.GetInstance<ILoadingUnitProvider>();

        private IDataSource<AllowedItemInCompartment> allowedItemsDataSource;

        private bool isCompartmentSelectableTray;

        private IDataSource<LoadingUnit> loadingUnitsDataSource;

        private object modelChangedEventSubscription;

        private object modelRefreshSubscription;

        private object modelSelectionChangedSubscription;

        private bool readOnlyTray;

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

        public bool IsCompartmentSelectableTray
        {
            get => this.isCompartmentSelectableTray;
            set => this.SetProperty(ref this.isCompartmentSelectableTray, value);
        }

        public IDataSource<LoadingUnit> LoadingUnitsDataSource
        {
            get => this.loadingUnitsDataSource;
            set => this.SetProperty(ref this.loadingUnitsDataSource, value);
        }

        public bool ReadOnlyTray
        {
            get => this.readOnlyTray;
            set => this.SetProperty(ref this.readOnlyTray, value);
        }

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

        public override void RefreshData()
        {
            this.AllowedItemsDataSource = this.Model != null
                ? new DataSource<AllowedItemInCompartment>(() => this.itemProvider.GetAllowedByCompartmentId(this.Model.Id))
                : null;

            this.LoadingUnitsDataSource = new DataSource<LoadingUnit>(() => this.loadingUnitProvider.GetAll());

            base.RefreshData();
        }

        protected override async Task ExecuteRevertCommand()
        {
            await this.LoadData();
        }

        protected override async Task ExecuteSaveCommand()
        {
            this.IsBusy = true;

            var result = await this.compartmentProvider.SaveAsync(this.Model);
            if (result.Success)
            {
                this.TakeModelSnapshot();

                this.EventService.Invoke(new ModelChangedEvent<Compartment>(this.Model.Id));
                this.EventService.Invoke(new StatusEventArgs(Common.Resources.MasterData.CompartmentSavedSuccessfully, StatusType.Success));
            }
            else
            {
                this.EventService.Invoke(new StatusEventArgs(Common.Resources.Errors.UnableToSaveChanges, StatusType.Error));
            }

            this.IsBusy = false;
        }

        protected override async void OnAppear()
        {
            await this.LoadData();
            base.OnAppear();
        }

        protected override void OnDispose()
        {
            this.EventService.Unsubscribe<RefreshModelsEvent<Compartment>>(this.modelRefreshSubscription);
            this.EventService.Unsubscribe<ModelChangedEvent<Compartment>>(this.modelChangedEventSubscription);
            this.EventService.Unsubscribe<ModelSelectionChangedEvent<Compartment>>(this.modelSelectionChangedSubscription);
            base.OnDispose();
        }

        private void Initialize()
        {
            this.modelRefreshSubscription = this.EventService.Subscribe<RefreshModelsEvent<Compartment>>(
                async eventArgs => { await this.LoadData(); }, this.Token, true, true);

            this.modelChangedEventSubscription = this.EventService.Subscribe<ModelChangedEvent<Compartment>>(
                async eventArgs => { await this.LoadData(); });

            this.modelSelectionChangedSubscription = this.EventService.Subscribe<ModelSelectionChangedEvent<Compartment>>(
                async eventArgs =>
                {
                    if (eventArgs.ModelId.HasValue)
                    {
                        this.Data = eventArgs.ModelId.Value;
                        await this.LoadData();
                    }
                    else
                    {
                        this.Model = null;
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

        private async Task LoadData()
        {
            this.IsBusy = true;

            if (this.Data is int modelId)
            {
                var compartment = await this.compartmentProvider.GetById(modelId);
                var loadingUnit = await this.loadingUnitProvider.GetById(compartment.LoadingUnitId);
                this.Model = compartment;
                this.InitializeTray(loadingUnit);
            }

            this.IsBusy = false;
        }

        private void SetSelectedCompartment()
        {
            this.selectedCompartmentTray = this.Model;
            this.RaisePropertyChanged(nameof(this.SelectedCompartmentTray));
        }

        #endregion Methods
    }
}
