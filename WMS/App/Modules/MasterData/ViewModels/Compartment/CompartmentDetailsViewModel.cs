using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using CommonServiceLocator;
using DevExpress.Mvvm;
using DevExpress.Xpf.Data;
using Ferretto.WMS.App.Controls;
using Ferretto.WMS.App.Controls.Services;
using Ferretto.WMS.App.Core.Interfaces;
using Ferretto.WMS.App.Core.Models;

namespace Ferretto.WMS.Modules.MasterData
{
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

        public LoadingUnitDetails LoadingUnitDetails => this.loadingUnit;

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

        protected override Task<bool> ExecuteCompleteCommandAsync()
        {
            throw new NotSupportedException();
        }

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
            if (!this.CheckValidModel())
            {
                return false;
            }

            if (!await base.ExecuteSaveCommandAsync())
            {
                return false;
            }

            this.IsBusy = true;

            var result = await this.compartmentProvider.UpdateAsync(this.Model);
            if (result.Success)
            {
                this.TakeModelSnapshot();

                this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.MasterData.CompartmentSavedSuccessfully, StatusType.Success));
            }
            else
            {
                this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.Errors.UnableToSaveChanges, StatusType.Error));
            }

            this.IsBusy = false;

            return true;
        }

        protected override async Task LoadDataAsync()
        {
            try
            {
                this.IsBusy = true;

                if (this.Data is int modelId)
                {
                    var compartment = await this.compartmentProvider.GetByIdAsync(modelId);
                    this.loadingUnit = await this.loadingUnitProvider.GetByIdAsync(compartment.LoadingUnitId.Value);
                    this.AllowedItemsDataSource = await this.itemProvider.GetAllowedByCompartmentIdAsync(compartment.Id);
                    this.LoadingUnitsDataSource = new InfiniteDataSourceService<LoadingUnit, int>(this.loadingUnitProvider).DataSource;

                    this.Model = compartment;

                    this.RaisePropertyChanged(nameof(this.LoadingUnitDetails));
                }

                this.IsBusy = false;
            }
            catch
            {
                this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.Errors.UnableToLoadData, StatusType.Error));
            }
        }

        protected override async Task OnAppearAsync()
        {
            await base.OnAppearAsync().ConfigureAwait(true);
            await this.LoadDataAsync().ConfigureAwait(true);
        }

        protected override void OnDispose()
        {
            this.EventService.Unsubscribe<ModelSelectionChangedPubSubEvent<Compartment>>(this.modelSelectionChangedSubscription);
            base.OnDispose();
        }

        private void EditCompartment()
        {
            var args = new LoadingUnitArgs { LoadingUnitId = this.Model.LoadingUnitId.Value, CompartmentId = this.Model.Id };
            this.HistoryViewService.Appear(nameof(Modules.MasterData), Common.Utils.Modules.MasterData.LOADINGUNITEDIT, args);
        }

        private void Initialize()
        {
            this.loadingUnit = new LoadingUnitDetails();

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
