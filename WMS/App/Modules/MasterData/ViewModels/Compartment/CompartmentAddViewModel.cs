using System;
using System.ComponentModel;
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
    public class CompartmentAddViewModel : SidePanelDetailsViewModel<CompartmentDetails>
    {
        #region Fields

        private readonly ICompartmentProvider compartmentProvider = ServiceLocator.Current.GetInstance<ICompartmentProvider>();

        private readonly IItemProvider itemProvider = ServiceLocator.Current.GetInstance<IItemProvider>();

        private bool itemIdHasValue;

        private IDataSource<Item> itemsDataSource;

        #endregion

        #region Constructors

        public CompartmentAddViewModel()
        {
            this.Title = Common.Resources.MasterData.AddCompartment;

            this.IsValidationEnabled = false;
        }

        #endregion

        #region Properties

        public bool ItemIdHasValue
        {
            get => this.itemIdHasValue;
            set => this.SetProperty(ref this.itemIdHasValue, value);
        }

        public IDataSource<Item> ItemsDataSource
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
            this.IsValidationEnabled = true;

            if (string.IsNullOrWhiteSpace(this.Model.Error) == false)
            {
                return;
            }

            this.IsBusy = true;

            var result = await this.compartmentProvider.AddAsync(this.Model);
            if (result.Success)
            {
                this.TakeModelSnapshot();

                this.EventService.Invoke(new ModelChangedPubSubEvent<LoadingUnit>(this.Model.LoadingUnit.Id));
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

        protected override async Task OnAppearAsync()
        {
            await this.LoadDataAsync();
            await base.OnAppearAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                this.IsBusy = true;
                var items = await this.itemProvider.GetAllAsync(0, 0);
                this.ItemsDataSource = new DataSource<Item>(() => items.AsQueryable());

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
