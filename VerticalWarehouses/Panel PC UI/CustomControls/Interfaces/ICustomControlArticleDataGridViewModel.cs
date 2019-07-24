using System.Collections.Generic;
using Ferretto.VW.Utils.Interfaces;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.App.Controls.Interfaces
{
    public interface ICustomControlArticleDataGridViewModel : IViewModel
    {
        #region Properties

        IEnumerable<Item> Items { get; set; }

        Item SelectedItem { get; set; }

        #endregion
    }
}
