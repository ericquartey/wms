using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using CommonServiceLocator;
using DevExpress.Xpf.Data;
using Ferretto.Common.Utils.Expressions;
using Ferretto.WMS.App.Controls;
using Ferretto.WMS.App.Controls.Services;
using Ferretto.WMS.App.Core.Interfaces;
using Ferretto.WMS.App.Core.Models;
using Ferretto.WMS.Data.Hubs;

namespace Ferretto.WMS.Modules.MasterData
{
    public class CompartmentAddViewModel : SidePanelDetailsViewModel<CompartmentDetails>
    {
        #region Fields

        private readonly ICompartmentProvider compartmentProvider = ServiceLocator.Current.GetInstance<ICompartmentProvider>();

        private readonly IItemProvider itemProvider = ServiceLocator.Current.GetInstance<IItemProvider>();

        private bool canChooseItem;

        private bool itemIdHasValue;

        private InfiniteAsyncSource itemsDataSource;

        #endregion

        #region Constructors

        public CompartmentAddViewModel()
        {
            this.Title = Common.Resources.MasterData.AddCompartment;
            this.ColorRequired = ColorRequired.CreateMode;

            this.LoadDataAsync();
        }

        #endregion

        #region Properties

        public bool CanChooseItem
        {
            get => this.canChooseItem;
            set => this.SetProperty(ref this.canChooseItem, value);
        }

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

        #endregion

        #region Methods

        protected override Task ExecuteRefreshCommandAsync() => throw new NotSupportedException();

        protected override Task ExecuteRevertCommandAsync() => throw new NotSupportedException();

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

            var result = await this.compartmentProvider.CreateAsync(this.Model);
            if (result.Success)
            {
                this.TakeModelSnapshot();

                this.EventService.Invoke(new StatusPubSubEvent(
                   Common.Resources.MasterData.LoadingUnitSavedSuccessfully,
                   StatusType.Success));
                this.EventService.Invoke(new ModelChangedPubSubEvent(typeof(CompartmentDetails).ToString(), this.Model.Id, HubEntityOperation.Updated));

                this.CompleteOperation();
            }
            else
            {
                this.EventService.Invoke(new StatusPubSubEvent(result.Description, StatusType.Error));
            }

            this.IsBusy = false;

            return true;
        }

        protected override Task LoadDataAsync()
        {
            Func<int, int, IEnumerable<SortOption>, Task<IEnumerable<Item>>> getAllAllowedByLoadingUnitId = this.GetAllAllowedByLoadingUnitIdAsync;
            this.ItemsDataSource = new InfiniteDataSourceService<Item, int>(
            this.itemProvider, getAllAllowedByLoadingUnitId).DataSource;

            return Task.CompletedTask;
        }

        protected override async void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e == null)
            {
                return;
            }

            if (e.PropertyName == nameof(CompartmentDetails.ItemId))
            {
                this.ItemIdHasValue = this.Model.ItemId.HasValue;
            }

            if (this.Model.ItemId.HasValue
              &&
              (
              e.PropertyName == nameof(CompartmentDetails.ItemId)
              ||
              e.PropertyName == nameof(CompartmentDetails.Width)
              ||
              e.PropertyName == nameof(CompartmentDetails.Height)))
            {
                var result = await this.compartmentProvider.GetMaxCapacityAsync(
                        this.Model.Width,
                        this.Model.Height,
                        this.Model.ItemId.Value);

                if (result.Success)
                {
                    this.Model.MaxCapacity = result.Entity;
                }
            }

            base.Model_PropertyChanged(sender, e);
        }

        private async Task<IEnumerable<Item>> GetAllAllowedByLoadingUnitIdAsync(int skip, int pageSize, IEnumerable<SortOption> sortOrder)
        {
            var result = await this.itemProvider.GetAllAllowedByLoadingUnitIdAsync(this.Model.LoadingUnitId.Value, skip, pageSize, sortOrder);
            return !result.Success ? null : result.Entity;
        }

        #endregion
    }
}
