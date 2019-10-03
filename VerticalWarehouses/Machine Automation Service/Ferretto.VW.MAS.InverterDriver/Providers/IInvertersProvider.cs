using System.Collections.Generic;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;

namespace Ferretto.VW.MAS.InverterDriver.Interface.Services
{
    public interface IInvertersProvider
    {
        #region Methods

        IEnumerable<IInverterStatusBase> GetAll();

        IInverterStatusBase GetByIndex(InverterIndex index);

        IAngInverterStatus GetMainInverter();

        #endregion
    }
}
