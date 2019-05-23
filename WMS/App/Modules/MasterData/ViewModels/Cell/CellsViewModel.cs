using Ferretto.Common.BLL.Interfaces;
using Ferretto.WMS.App.Controls;
using Ferretto.WMS.App.Core.Models;

namespace Ferretto.WMS.Modules.MasterData
{
    public class CellsViewModel : EntityPagedListViewModel<Cell, int>
    {
        #region Constructors

        public CellsViewModel(IDataSourceService dataSourceService)
          : base(dataSourceService)
        {
        }

        #endregion

        #region Methods

        public override void ShowDetails()
        {
            this.HistoryViewService.Appear(nameof(Modules.MasterData), Common.Utils.Modules.MasterData.CELLDETAILS, this.CurrentItem.Id);
        }

        #endregion
    }
}
