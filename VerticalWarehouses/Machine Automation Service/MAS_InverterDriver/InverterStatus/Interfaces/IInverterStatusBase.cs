using Ferretto.VW.MAS_InverterDriver.Interface.InverterStatus;

namespace Ferretto.VW.MAS_InverterDriver.InverterStatus.Interfaces
{
    public interface IInverterStatusBase
    {
        #region Properties

        IControlWord CommonControlWord { get; }

        IStatusWord CommonStatusWord { get; }

        ushort OperatingMode { get; set; }

        byte SystemIndex { get; }

        #endregion
    }
}
