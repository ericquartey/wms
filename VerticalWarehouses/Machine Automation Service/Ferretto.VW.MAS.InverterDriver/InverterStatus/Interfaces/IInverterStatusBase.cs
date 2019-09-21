using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.Utils.Enumerations;

namespace Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces
{
    public interface IInverterStatusBase
    {
        #region Properties

        IControlWord CommonControlWord { get; }

        IStatusWord CommonStatusWord { get; }

        bool[] Inputs { get; }

        InverterType InverterType { get; }

        ushort OperatingMode { get; set; }

        byte SystemIndex { get; }

        #endregion

        #region Methods

        bool UpdateInputsStates(bool[] newInputStates);

        #endregion
    }
}
