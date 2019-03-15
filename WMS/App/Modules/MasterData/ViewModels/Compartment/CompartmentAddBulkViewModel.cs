using System;
using System.Threading.Tasks;
using CommonServiceLocator;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.BusinessProviders;
using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Services;

namespace Ferretto.WMS.Modules.MasterData
{
    public class CompartmentAddBulkViewModel : SidePanelDetailsViewModel<BulkCompartment>
    {
        #region Fields

        private readonly ICompartmentProvider compartmentProvider = ServiceLocator.Current.GetInstance<ICompartmentProvider>();

        #endregion

        #region Constructors

        public CompartmentAddBulkViewModel()
        {
            this.Title = Common.Resources.MasterData.BulkAddCompartment;
            this.ColorRequired = ColorRequired.CreateMode;
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

            var newCompartments = this.Model.CreateBulk();
            var result = await this.compartmentProvider.AddRangeAsync(newCompartments);
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

        #endregion
    }
}
