using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CommonServiceLocator;
using DevExpress.Mvvm;
using DevExpress.Xpf.Data;
using Ferretto.Common.Utils;
using Ferretto.WMS.App.Controls;
using Ferretto.WMS.App.Controls.Services;
using Ferretto.WMS.App.Core.Interfaces;
using Ferretto.WMS.App.Core.Models;
using Ferretto.WMS.App.Resources;

namespace Ferretto.WMS.App.Modules.MasterData
{
    [Resource(nameof(Ferretto.WMS.Data.WebAPI.Contracts.Compartment))]
    [Resource(nameof(Ferretto.WMS.Data.WebAPI.Contracts.LoadingUnit), false)]
    [Resource(nameof(Ferretto.WMS.Data.WebAPI.Contracts.CompartmentType), false)]
    [Resource(nameof(Ferretto.WMS.Data.WebAPI.Contracts.Item), false)]
    [Resource(nameof(Ferretto.WMS.Data.WebAPI.Contracts.ItemCompartmentType), false)]
    public class CompartmentDetailsViewModel : DetailsViewModel<CompartmentDetails>
    {
        #region Fields

        private readonly ICompartmentProvider compartmentProvider = ServiceLocator.Current.GetInstance<ICompartmentProvider>();

        private readonly IItemProvider itemProvider = ServiceLocator.Current.GetInstance<IItemProvider>();

        private readonly ILoadingUnitProvider loadingUnitProvider = ServiceLocator.Current.GetInstance<ILoadingUnitProvider>();

        private IEnumerable<AllowedItemInCompartment> allowedItemsDataSource;

        private ICommand editCompartmentCommand;

        private bool isCompartmentSelectableTray;

        private LoadingUnitDetails loadingUnit;

        private InfiniteAsyncSource loadingUnitsDataSource;

        private object modelSelectionChangedSubscription;

        private bool readOnlyTray;

        #endregion

        #region Constructors

        public CompartmentDetailsViewModel()
        {
            this.Initialize();
        }

        #endregion

        #region Properties

        public IEnumerable<AllowedItemInCompartment> AllowedItemsDataSource
        {
            get => this.allowedItemsDataSource;
            set => this.SetProperty(ref this.allowedItemsDataSource, value);
        }

        public ICommand EditCompartmentCommand => this.editCompartmentCommand ??
            (this.editCompartmentCommand = new DelegateCommand(this.EditCompartment));

        public bool IsCompartmentSelectableTray
        {
            get => this.isCompartmentSelectableTray;
            set => this.SetProperty(ref this.isCompartmentSelectableTray, value);
        }

        public bool IsItemDetailsEnabled
        {
            get
            {
                if (this.Model == null ||
                    !this.Model.ItemId.HasValue)
                {
                    return false;
                }

                if (this.Model.Stock <= 0)
                {
                    return false;
                }

                return true;
            }
        }

        public LoadingUnitDetails LoadingUnitDetails
        {
            get => this.loadingUnit;
            set => this.SetProperty(ref this.loadingUnit, value);
        }

        public InfiniteAsyncSource LoadingUnitsDataSource
        {
            get => this.loadingUnitsDataSource;
            set => this.SetProperty(ref this.loadingUnitsDataSource, value);
        }

        public bool ReadOnlyTray
        {
            get => this.readOnlyTray;
            set => this.SetProperty(ref this.readOnlyTray, value);
        }

        #endregion

        #region Methods

        protected override async Task ExecuteRefreshCommandAsync()
        {
            await this.LoadDataAsync();
        }

        protected override async Task ExecuteRevertCommandAsync()
        {
            await this.LoadDataAsync();
        }

        protected override async Task<bool> ExecuteSaveCommandAsync()
        {
            var result = await this.compartmentProvider.UpdateAsync(this.Model);
            if (result.Success)
            {
                this.TakeModelSnapshot();

                this.EventService.Invoke(new StatusPubSubEvent(App.Resources.MasterData.CompartmentSavedSuccessfully, StatusType.Success));
            }
            else
            {
                this.EventService.Invoke(
                    new StatusPubSubEvent(
                        Errors.UnableToSaveChanges,
                        result.Description,
                        StatusType.Error));
            }

            return result.Success;
        }

        protected override async Task LoadDataAsync()
        {
            this.IsBusy = true;

            if (this.Data is int modelId)
            {
                var compartment = await this.compartmentProvider.GetByIdAsync(modelId);
                this.LoadingUnitDetails = await this.loadingUnitProvider.GetByIdAsync(compartment.LoadingUnitId.Value);

                var result = await this.itemProvider.GetAllowedByCompartmentIdAsync(compartment.Id);
                if (result.Success)
                {
                    this.AllowedItemsDataSource = result.Entity;
                }
                else
                {
                    this.EventService.Invoke(new StatusPubSubEvent(Errors.UnableToLoadData, StatusType.Error));
                }

                this.LoadingUnitsDataSource = new InfiniteDataSourceService<LoadingUnit, int>(this.loadingUnitProvider).DataSource;

                this.Model = compartment;
                this.RaisePropertyChanged(nameof(this.IsItemDetailsEnabled));
            }

            this.IsBusy = false;
        }

        protected override void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e == null)
            {
                return;
            }

            if (e.PropertyName == nameof(CompartmentDetails.ItemId))
            {
                this.RaisePropertyChanged(nameof(this.IsItemDetailsEnabled));
            }

            if (e.PropertyName == nameof(CompartmentDetails.Stock))
            {
                this.RaisePropertyChanged(nameof(this.IsItemDetailsEnabled));
            }

            base.Model_PropertyChanged(sender, e);
        }

        protected override async Task OnAppearAsync()
        {
            await base.OnAppearAsync().ConfigureAwait(true);
            await this.LoadDataAsync().ConfigureAwait(true);
        }

        protected override void OnDispose()
        {
            this.EventService.Unsubscribe<ModelSelectionChangedPubSubEvent<Compartment>>(this.modelSelectionChangedSubscription);
            if (this.Model != null)
            {
                this.Model.PropertyChanged -= this.Model_PropertyChanged;
            }

            base.OnDispose();
        }

        private void EditCompartment()
        {
            System.Diagnostics.Debug.Assert(
                this.Model.LoadingUnitId.HasValue,
                "A compartment should be editable only if it has a related loading unit.");

            var inputData = new LoadingUnitEditViewData(this.Model.LoadingUnitId.Value, null, this.Model.Id);

            this.HistoryViewService.Appear(
                nameof(MasterData),
                Common.Utils.Modules.MasterData.LOADINGUNITEDIT,
                inputData);
        }

        private void Initialize()
        {
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

        #endregion
    }
}
