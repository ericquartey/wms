using Ferretto.VW.MAS.InverterDriver.Interface.InverterStatus;
using Ferretto.VW.MAS.Utils.Enumerations;

namespace Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces
{
    public interface IInverterStatusBase
    {
        #region Properties

        IControlWord CommonControlWord { get; }

        IStatusWord CommonStatusWord { get; }

        InverterType InverterType { get; }

        ushort OperatingMode { get; set; }

        byte SystemIndex { get; }

        #endregion
    }
}
