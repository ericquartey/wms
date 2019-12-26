using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.VW.App.Services
{
    public interface IOperationalContextViewModel
    {
        #region Properties

        string ActiveContextName { get; }

        #endregion
    }
}
