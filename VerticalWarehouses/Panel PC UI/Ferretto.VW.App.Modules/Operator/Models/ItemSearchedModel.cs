using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.App.Modules.Operator.Interfaces;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Prism.Mvvm;

namespace Ferretto.VW.App.Modules.Operator.Models
{
    public class ItemSearchedModel : IItemSearchedModel
    {
        #region Properties

        public Item SelectedItem { get; set; }

        #endregion
    }
}
