using System.Threading.Tasks;
using CommonServiceLocator;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Resources;
using Ferretto.WMS.App.Controls;
using Ferretto.WMS.App.Controls.Services;
using Ferretto.WMS.App.Core.Interfaces;
using Ferretto.WMS.App.Core.Models;

namespace Ferretto.WMS.Modules.MasterData
{
    public class CompartmentTypesViewModel : EntityPagedListViewModel<CompartmentType, int>
    {
        #region Fields

        private readonly ICompartmentTypeProvider compartmentTypeProvider = ServiceLocator.Current.GetInstance<ICompartmentTypeProvider>();

        #endregion

        #region Constructors

        public CompartmentTypesViewModel(IDataSourceService dataSourceService)
                                  : base(dataSourceService)
        {
        }

        #endregion

        #region Methods

        public override void ShowDetails()
        {
            // Method intentionally left empty.
        }

        protected override void ExecuteAddCommand()
        {
            this.NavigationService.Appear(
                nameof(Common.Utils.Modules.MasterData),
                Common.Utils.Modules.MasterData.COMPARTMENTTYPEDETAILS);
        }

        protected override async Task ExecuteDeleteCommandAsync()
        {
            var result = await this.compartmentTypeProvider.DeleteAsync(this.CurrentItem.Id);
            if (result.Success)
            {
                this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.MasterData.LoadingUnitDeletedSuccessfully, StatusType.Success));
                this.SelectedItem = null;
            }
            else
            {
                this.EventService.Invoke(new StatusPubSubEvent(Errors.UnableToSaveChanges, StatusType.Error));
            }
        }

        #endregion
    }
}
