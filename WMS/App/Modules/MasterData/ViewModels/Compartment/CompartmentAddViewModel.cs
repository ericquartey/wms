using System;
using System.ComponentModel;
using System.Threading.Tasks;
using CommonServiceLocator;
using DevExpress.Xpf.Data;
using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Services;
using Ferretto.WMS.App.Core.Interfaces;
using Ferretto.WMS.App.Core.Models;

namespace Ferretto.WMS.Modules.MasterData
{
    public class CompartmentAddViewModel : SidePanelDetailsViewModel<CompartmentDetails>
    {
        #region Fields

        private readonly ICompartmentProvider compartmentProvider = ServiceLocator.Current.GetInstance<ICompartmentProvider>();

        private readonly IItemProvider itemProvider = ServiceLocator.Current.GetInstance<IItemProvider>();

        private bool itemIdHasValue;

        private InfiniteAsyncSource itemsDataSource;

        #endregion

        #region Constructors

        public CompartmentAddViewModel()
        {
            this.Title = Common.Resources.MasterData.AddCompartment;
            this.ColorRequired = ColorRequired.CreateMode;

            this.LoadData();
        }

        #endregion

        #region Properties

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

        protected override Task ExecuteRefreshCommandAsync()
        {
            throw new NotSupportedException();
        }

        protected override Task ExecuteRevertCommand() => throw new NotSupportedException();

        protected override async Task ExecuteSaveCommand()
        {
            if (!this.IsModelValid)
            {
                return;
            }

            this.IsBusy = true;

            var result = await this.compartmentProvider.CreateAsync(this.Model);
            if (result.Success)
            {
                this.TakeModelSnapshot();

                this.EventService.Invoke(new ModelChangedPubSubEvent<LoadingUnit, int>(this.Model.LoadingUnit.Id));
                this.EventService.Invoke(new StatusPubSubEvent(
                    Common.Resources.MasterData.LoadingUnitSavedSuccessfully,
                    StatusType.Success));

                this.CompleteOperation();
            }
            else
            {
                this.EventService.Invoke(new StatusPubSubEvent(result.Description, StatusType.Error));
            }

            this.IsBusy = false;
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
                var capacity = await this.compartmentProvider.GetMaxCapacityAsync(
                        this.Model.Width,
                        this.Model.Height,
                        this.Model.ItemId.Value);

                this.Model.MaxCapacity = capacity ?? this.Model.MaxCapacity;
            }

            base.Model_PropertyChanged(sender, e);
        }

        private void LoadData()
        {
            this.ItemsDataSource = new InfiniteDataSourceService<Item, int>(this.itemProvider).DataSource;
        }

        #endregion
    }
}
