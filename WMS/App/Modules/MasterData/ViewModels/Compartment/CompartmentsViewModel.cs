using Ferretto.Common.BLL.Interfaces;
using Ferretto.WMS.App.Controls;
using Ferretto.WMS.App.Core.Models;

namespace Ferretto.WMS.Modules.MasterData
{
    public class CompartmentsViewModel : EntityPagedListViewModel<Compartment, int>
    {
        #region Constructors

        public CompartmentsViewModel(IDataSourceService dataSourceService)
          : base(dataSourceService)
        {
        }

        #endregion

        #region Methods

        public override void ShowDetails()
        {
            this.HistoryViewService.Appear(nameof(Modules.MasterData), Common.Utils.Modules.MasterData.COMPARTMENTDETAILS, this.CurrentItem.Id);
        }

        #endregion
    }
}
