using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.VW.MAS.DataLayer.Providers.Interfaces
{
    public interface IElevatorProvider
    {
        #region Methods

        void Start(int loadingUnitId, decimal runToTest, decimal weight);

        #endregion
    }
}
