using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.VW.MAS.DataModels
{
    public interface IValidable
    {
        #region Methods

        void Validate();

        #endregion
    }
}
