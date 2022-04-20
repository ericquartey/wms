using Ferretto.VW.MAS.InverterDriver.Contracts;

namespace Ferretto.VW.MAS.NordDriver
{
    public interface INordStatusBase
    {
        #region Properties

        IControlWord CommonControlWord { get; }

        IStatusWord CommonStatusWord { get; }

        bool[] Inputs { get; }

        bool IsStarted { get; }

        ushort OperatingMode { get; set; }

        ushort SetPointFrequency { get; set; }

        int SetPointPosition { get; set; }

        ushort SetPointRampTime { get; set; }

        InverterIndex SystemIndex { get; }

        #endregion

        #region Methods

        bool UpdateInputsStates(bool[] newInputStates);

        #endregion
    }
}
