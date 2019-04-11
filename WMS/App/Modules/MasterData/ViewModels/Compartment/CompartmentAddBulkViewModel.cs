using System;
using System.Threading.Tasks;
using CommonServiceLocator;
using Ferretto.WMS.App.Controls;
using Ferretto.WMS.App.Controls.Services;
using Ferretto.WMS.App.Core.Interfaces;
using Ferretto.WMS.App.Core.Models;

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

        protected override Task ExecuteRevertCommandAsync() => throw new NotSupportedException();

        protected override async Task<bool> ExecuteSaveCommandAsync()
        {
            if (!await base.ExecuteSaveCommandAsync())
            {
                return false;
            }

            if (!this.IsModelValid)
            {
                return false;
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

            return true;
        }

        #endregion
    }
}
