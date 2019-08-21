using Ferretto.VW.Utils.Interfaces;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.App.Modules.Operator.Interfaces
{
    public interface IListsInWaitViewModel : IViewModel
    {
        #region Properties

        ItemList SelectedList { get; set; }

        #endregion
    }
}
