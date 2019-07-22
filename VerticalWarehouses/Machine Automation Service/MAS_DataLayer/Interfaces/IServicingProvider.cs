using System;
using System.Collections.Generic;
using System.Text;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer
{
    public interface IServicingProvider
    {
        #region Methods

        ServicingInfo GetInfo();

        #endregion
    }
}
