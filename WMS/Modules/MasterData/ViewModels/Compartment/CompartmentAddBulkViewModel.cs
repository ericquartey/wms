using System;
using System.Threading.Tasks;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.BusinessProviders;
using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Services;
using Microsoft.Practices.ServiceLocation;

namespace Ferretto.WMS.Modules.MasterData
{
    public class CompartmentAddBulkViewModel : SidePanelDetailsViewModel<BulkCompartment>
    {
        #region Fields

        private readonly ICompartmentProvider compartmentProvider = ServiceLocator.Current.GetInstance<ICompartmentProvider>();

        #endregion Fields

        #region Constructors

        public CompartmentAddBulkViewModel()
        {
            this.Title = Common.Resources.MasterData.BulkAddCompartment;
        }

        #endregion Constructors

        #region Methods

        protected override Task ExecuteRevertCommand()
        {
            throw new NotImplementedException();
        }

        protected override async void ExecuteSaveCommand()
        {
            this.IsValidationEnabled = true;

            if (string.IsNullOrWhiteSpace(this.Model.Error) == false)
            {
                return;
            }

            var newCompartments = this.Model.CreateBulk();
            var result = await this.compartmentProvider.AddRange(newCompartments);
            if (result.Success)
            {
                this.TakeModelSnapshot();

                this.EventService.Invoke(new ModelChangedEvent<LoadingUnit>(this.Model.LoadingUnit.Id));
                this.EventService.Invoke(new StatusEventArgs(Common.Resources.MasterData.LoadingUnitSavedSuccessfully));

                this.CompleteOperation();
            }
        }

        #endregion Methods
    }
}
