using Ferretto.VW.App.Modules.Operator.Interfaces;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.App.Modules.Operator.Models
{
    public class ItemSearchedModel : IItemSearchedModel
    {
        #region Properties

        public Item SelectedItem { get; set; }

        #endregion
    }
}
