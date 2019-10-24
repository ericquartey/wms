using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.App.Modules.Operator.Interfaces;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.App.Modules.Operator.Models
{
    public class WaitListSelectedModel : IWaitListSelectedModel
    {
        #region Properties

        public ItemList SelectedList { get; set; }

        #endregion
    }
}
