using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.Interface.InverterStatus;

namespace Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces
{
    public interface IHomingInverterStatus : IInverterStatusBase
    {
        #region Properties

        IHomingControlWord HomingControlWord { get; }

        IHomingStatusWord HomingStatusWord { get; }

        #endregion
    }
}
