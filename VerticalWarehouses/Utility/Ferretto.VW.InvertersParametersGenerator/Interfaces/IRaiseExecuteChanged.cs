using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.VW.InvertersParametersGenerator.Interfaces
{
    public interface IRaiseExecuteChanged
    {
        #region Methods

        void RaiseCanExecuteChanged();

        #endregion
    }
}
