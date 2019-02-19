using System.Linq;
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

        private IDataSource<AllowedItemInCompartment, int> allowedItemsDataSource;

        private bool isCompartmentSelectableTray;

        private LoadingUnitDetails loadingUnit;

        private IDataSource<LoadingUnit, int> loadingUnitsDataSource;

        private object modelChangedEventSubscription;

        private object modelRefreshSubscription;

        private object modelSelectionChangedSubscription;

        private bool readOnlyTray;

        private CompartmentDetails selectedCompartmentTray;

        #endregion

        #region Constructors

        public CompartmentDetailsViewModel()
        {
            this.Initialize();
        }

        #endregion

        #region Properties

        public IDataSource<AllowedItemInCompartment, int> AllowedItemsDataSource
        {
            get => this.allowedItemsDataSource;
            set => this.SetProperty(ref this.allowedItemsDataSource, value);
        }

        public bool IsCompartmentSelectableTray
        {
            get => this.isCompartmentSelectableTray;
            set => this.SetProperty(ref this.isCompartmentSelectableTray, value);
        }

        public LoadingUnitDetails LoadingUnitDetails => this.loadingUnit;

        public IDataSource<LoadingUnit, int> LoadingUnitsDataSource
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

        #endregion

        #region Methods

        public override async void LoadRelatedData()
        {
            var items = await this.itemProvider.GetAllowedByCompartmentIdAsync(this.Model.Id);
            this.AllowedItemsDataSource = this.Model != null
                ? new DataSource<AllowedItemInCompartment, int>(items.AsQueryable<AllowedItemInCompartment>)
                : null;

            var loadingUnits = await this.loadingUnitProvider.GetAllAsync(0, 0);
            this.LoadingUnitsDataSource = new DataSource<LoadingUnit, int>(loadingUnits.AsQueryable);

            base.LoadRelatedData();
        }

        protected override async Task ExecuteRefreshCommandAsync()
        {
            await this.LoadDataAsync();
        }

        protected override async Task ExecuteRevertCommand()
        {
            await this.LoadDataAsync();
        }

        protected override async Task ExecuteSaveCommand()
        {
            this.IsBusy = true;

            var result = await this.compartmentProvider.UpdateAsync(this.Model);
            if (result.Success)
            {
                this.TakeModelSnapshot();

                this.EventService.Invoke(new ModelChangedPubSubEvent<Compartment, int>(this.Model.Id));
                this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.MasterData.CompartmentSavedSuccessfully, StatusType.Success));
            }
            else
            {
                this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.Errors.UnableToSaveChanges, StatusType.Error));
            }

            this.IsBusy = false;
        }

        protected override async Task OnAppearAsync()
        {
            await base.OnAppearAsync().ConfigureAwait(true);
            await this.LoadDataAsync().ConfigureAwait(true);
        }

        protected override void OnDispose()
        {
            this.EventService.Unsubscribe<RefreshModelsPubSubEvent<Compartment>>(this.modelRefreshSubscription);
            this.EventService.Unsubscribe<ModelChangedPubSubEvent<Compartment, int>>(this.modelChangedEventSubscription);
            this.EventService.Unsubscribe<ModelSelectionChangedPubSubEvent<Compartment>>(this.modelSelectionChangedSubscription);
            base.OnDispose();
        }

        private void Initialize()
        {
            this.loadingUnit = new LoadingUnitDetails();
            this.modelRefreshSubscription = this.EventService.Subscribe<RefreshModelsPubSubEvent<Compartment>>(
                async eventArgs => { await this.LoadDataAsync(); }, this.Token, true, true);

            this.modelChangedEventSubscription = this.EventService.Subscribe<ModelChangedPubSubEvent<Compartment, int>>(
                async eventArgs => { await this.LoadDataAsync(); });

            this.modelSelectionChangedSubscription = this.EventService.Subscribe<ModelSelectionChangedPubSubEvent<Compartment>>(
                async eventArgs =>
                {
                    if (eventArgs.ModelId.HasValue)
                    {
                        this.Data = eventArgs.ModelId.Value;
                        await this.LoadDataAsync();
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

        private async Task LoadDataAsync()
        {
            try
            {
                this.IsBusy = true;

                if (this.Data is int modelId)
                {
                    var compartment = await this.compartmentProvider.GetByIdAsync(modelId);
                    this.loadingUnit = await this.loadingUnitProvider.GetByIdAsync(compartment.LoadingUnitId);
                    this.Model = compartment;
                    this.RaisePropertyChanged(nameof(this.LoadingUnitDetails));
                    this.SelectedCompartmentTray = this.Model;
                }

                this.IsBusy = false;
            }
            catch
            {
                this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.Errors.UnableToLoadData, StatusType.Error));
            }
        }

        #endregion
    }
}
