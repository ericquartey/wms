using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.VW.MAS.DataLayer.Providers.Interfaces
{
    public interface IElevatorProvider
    {
        #region Methods

        void Start(int id, decimal runToTest, decimal weight);

        #endregion
    }
}
