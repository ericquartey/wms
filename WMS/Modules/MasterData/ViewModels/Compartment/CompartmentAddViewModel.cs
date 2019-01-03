using System;
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

        private IDataSource<Item> itemsDataSource;

        #endregion Fields

        #region Constructors

        public CompartmentAddViewModel()
        {
            this.Title = Common.Resources.MasterData.AddCompartment;
            this.ItemsDataSource = new DataSource<Item>(() => this.itemProvider.GetAll());
            this.IsValidationEnabled = false;
        }

        #endregion Constructors

        #region Properties

        public IDataSource<Item> ItemsDataSource
        {
            get => this.itemsDataSource;
            set => this.SetProperty(ref this.itemsDataSource, value);
        }

        #endregion Properties

        #region Methods

        protected override Task ExecuteRevertCommand()
        {
            // do nothing here
            throw new NotImplementedException();
        }

        protected override async void ExecuteSaveCommand()
        {
            this.IsBusy = true;
            this.IsValidationEnabled = true;

            if (string.IsNullOrWhiteSpace(this.Model.Error) == false)
            {
                return;
            }

            var result = await this.compartmentProvider.Add(this.Model);
            if (result.Success)
            {
                this.TakeModelSnapshot();

                this.EventService.Invoke(new ModelChangedEvent<LoadingUnit>(this.Model.LoadingUnit.Id));
                this.EventService.Invoke(new StatusEventArgs(
                    Common.Resources.MasterData.LoadingUnitSavedSuccessfully,
                    StatusType.Success));

                this.CompleteOperation();
            }
            else
            {
                this.EventService.Invoke(new StatusEventArgs(result.Description, StatusType.Error));
            }

            this.IsBusy = false;
        }

        #endregion Methods
    }
}
