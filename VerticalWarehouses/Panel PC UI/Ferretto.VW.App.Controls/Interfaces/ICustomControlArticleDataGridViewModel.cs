using System.Collections.Generic;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Interfaces;

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
