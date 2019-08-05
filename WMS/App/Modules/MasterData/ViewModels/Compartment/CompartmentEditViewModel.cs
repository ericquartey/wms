using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using CommonServiceLocator;
using DevExpress.Xpf.Data;
using Ferretto.Common.Controls.WPF;
using Ferretto.Common.Utils;
using Ferretto.Common.Utils.Expressions;
using Ferretto.WMS.App.Controls;
using Ferretto.WMS.App.Controls.Services;
using Ferretto.WMS.App.Core.Interfaces;
using Ferretto.WMS.App.Core.Models;
using Ferretto.WMS.App.Resources;

namespace Ferretto.WMS.App.Modules.MasterData
{
    [Resource(nameof(Ferretto.WMS.Data.WebAPI.Contracts.Compartment))]
    [Resource(nameof(Ferretto.WMS.Data.WebAPI.Contracts.Item), false)]
    public class CompartmentEditViewModel : SidePanelDetailsViewModel<CompartmentDetails>
    {
        #region Fields

        private readonly ICompartmentProvider compartmentProvider = ServiceLocator.Current.GetInstance<ICompartmentProvider>();

        private readonly IGlobalSettingsProvider globalSettingsProvider = ServiceLocator.Current.GetInstance<IGlobalSettingsProvider>();

        private readonly IItemProvider itemProvider = ServiceLocator.Current.GetInstance<IItemProvider>();

        private GlobalSettings globalSettings;

        private bool isAdd;

        private bool isErrorsVisible;

        private bool isHeaderVisible;

        private bool isItemLookUpEnabled;

        private bool itemIdHasValue;

        private InfiniteAsyncSource itemsDataSource;

        private AppearMode mode;

        private Item selectedItem;

        private bool showDetails;

        #endregion

        #region Constructors

        public CompartmentEditViewModel()
        {
            this.IsHeaderVisible = true;
            this.IsErrorsVisible = true;
            this.IsItemLookUpEnabled = true;
        }

        #endregion

        #region Enums

        public enum AppearMode
        {
            Edit,

            Add,
        }

        #endregion

        #region Properties

        public GlobalSettings GlobalSettings { get => this.globalSettings; set => this.SetProperty(ref this.globalSettings, value); }

        public bool IsAdd
        {
            get => this.isAdd;
            set => this.SetProperty(ref this.isAdd, value);
        }

        public bool IsErrorsVisible
        {
            get => this.isErrorsVisible;
            set => this.SetProperty(ref this.isErrorsVisible, value);
        }

        public bool IsHeaderVisible
        {
            get => this.isHeaderVisible;
            set => this.SetProperty(ref this.isHeaderVisible, value);
        }

        public bool IsItemLookUpEnabled
        {
            get => this.isItemLookUpEnabled;
            set => this.SetProperty(ref this.isItemLookUpEnabled, value);
        }

        public bool IsItemDetailsEnabled
        {
            get
            {
                if (this.Model?.ItemId == null)
                {
                    return false;
                }

                return this.Model.Stock.HasValue && this.Model.Stock.Value > 0;
            }
        }

        public bool IsValidModel => this.CheckValidModel();

        public bool ItemIdHasValue
        {
            get => this.itemIdHasValue;
            set => this.SetProperty(ref this.itemIdHasValue, value);
        }

        public InfiniteAsyncSource ItemsDataSource
        {
            get => this.itemsDataSource;
            set => this.SetProperty(ref this.itemsDataSource, value);
        }

        public AppearMode Mode
        {
            get => this.mode;
            set
            {
                if (this.SetProperty(ref this.mode, value))
                {
                    this.IsAdd = this.mode == AppearMode.Add;
                }
            }
        }

        public Item SelectedItem
        {
            get => this.selectedItem;
            set => this.SetProperty(ref this.selectedItem, value);
        }

        public bool ShowDetails { get => this.showDetails; set => this.SetProperty(ref this.showDetails, value); }

        #endregion

        #region Methods

        public async Task InitializeDataAsync()
        {
            if (this.mode == AppearMode.Add)
            {
                this.Title = App.Resources.MasterData.AddCompartment;
                this.ColorRequired = ColorRequired.CreateMode;
                this.ShowDetails = false;
            }
            else
            {
                this.Title = App.Resources.MasterData.EditCompartment;
                this.ShowDetails = this.Model.HasDetails;
            }

            Func<int, int, IEnumerable<SortOption>, Task<IEnumerable<Item>>> getAllAllowedByLoadingUnitId = this.GetAllAllowedByLoadingUnitIdAsync;
            this.ItemsDataSource = new InfiniteDataSourceService<Item, int>(
            this.itemProvider, getAllAllowedByLoadingUnitId).DataSource;

            this.GlobalSettings = await this.globalSettingsProvider.GetAllAsync();
        }

        protected override Task ExecuteRefreshCommandAsync() => throw new NotSupportedException();

        protected override async Task ExecuteRevertCommandAsync()
        {
            await this.LoadDataAsync();
        }

        protected override async Task<bool> ExecuteSaveCommandAsync()
        {
            var result = this.IsAdd ?
                await this.compartmentProvider.CreateAsync(this.Model) :
                await this.compartmentProvider.UpdateAsync(this.Model);
            if (result.Success)
            {
                this.TakeModelSnapshot();

                this.EventService.Invoke(new StatusPubSubEvent(
                    App.Resources.MasterData.LoadingUnitSavedSuccessfully,
                    StatusType.Success));

                this.CompleteOperation();
            }
            else
            {
                this.EventService.Invoke(new StatusPubSubEvent(result.Description, StatusType.Error));
            }

            return result.Success;
        }

        protected override async Task LoadDataAsync()
        {
            this.IsBusy = true;

            this.Model = await this.compartmentProvider.GetByIdAsync(this.Model.Id);

            this.IsBusy = false;
        }

        protected override async void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e == null || this.Model == null)
            {
                return;
            }

            if (e.PropertyName == nameof(CompartmentDetails.ItemId))
            {
                if (this.selectedItem != null &&
                    this.Model.ItemId.HasValue)
                {
                    this.Model.ItemMeasureUnit = this.SelectedItem.MeasureUnitDescription;
                }
                else
                {
                    this.Model.ItemMeasureUnit = null;
                }

                this.RaisePropertyChanged(nameof(this.IsItemDetailsEnabled));
            }

            if (e.PropertyName == nameof(CompartmentDetails.Stock))
            {
                this.RaisePropertyChanged(nameof(this.IsItemDetailsEnabled));
            }

            if (this.Model.ItemId.HasValue
                &&
                this.Model.Width.HasValue
                &&
                this.Model.Depth.HasValue
                &&
                (
                e.PropertyName == nameof(CompartmentDetails.ItemId)
                ||
                e.PropertyName == nameof(CompartmentDetails.Width)
                ||
                e.PropertyName == nameof(CompartmentDetails.Depth)))
            {
                var result = await this.compartmentProvider.GetMaxCapacityAsync(
                    this.Model.Width,
                    this.Model.Depth,
                    this.Model.ItemId.Value);

                if (result.Success && result.Entity.HasValue)
                {
                    this.Model.MaxCapacity = result.Entity;
                }
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
            if (this.Model != null)
            {
                this.Model.PropertyChanged -= this.Model_PropertyChanged;
            }

            base.OnDispose();
        }

        protected override async Task<bool> ExecuteDeleteCommandAsync()
        {
            var loadingUnit = this.Model.LoadingUnit;
            var result = await this.compartmentProvider.DeleteAsync(this.Model.Id);
            if (result.Success)
            {
                loadingUnit.Compartments.Remove(this.Model as IDrawableCompartment);

                this.EventService.Invoke(new StatusPubSubEvent(App.Resources.MasterData.CompartmentDeletedSuccessfully, StatusType.Success));

                this.CompleteOperation();
            }
            else
            {
                this.EventService.Invoke(
                    new StatusPubSubEvent(
                        Errors.UnableToSaveChanges,
                        StatusType.Error));
            }

            return result.Success;
        }

        private async Task<IEnumerable<Item>> GetAllAllowedByLoadingUnitIdAsync(int skip, int pageSize, IEnumerable<SortOption> sortOrder)
        {
            var result = await this.itemProvider.GetAllAllowedByLoadingUnitIdAsync(this.Model.LoadingUnitId.Value, skip, pageSize, sortOrder);
            return !result.Success ? null : result.Entity;
        }

        #endregion
    }
}
