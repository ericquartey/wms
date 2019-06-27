using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.OperatorApp.ServiceUtilities;
using Ferretto.VW.Utils.Interfaces;
using Ferretto.VW.WmsCommunication.Source;

namespace Ferretto.VW.OperatorApp.Interfaces
{
    public interface IDrawerActivityDetailViewModel : IViewModel
    {
        #region Properties

        DrawerActivityItemDetail ItemDetail { get; set; }

        #endregion
    }
}
