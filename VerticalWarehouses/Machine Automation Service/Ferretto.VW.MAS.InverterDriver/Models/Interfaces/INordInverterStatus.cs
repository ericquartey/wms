using Ferretto.VW.MAS.InverterDriver.Contracts;

namespace Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces
{
    public interface INordInverterStatus : IInverterStatusBase
    {
        #region Properties

        ushort AnalogIn { get; }

        ushort Current { get; }

        ushort SetPointFrequency { get; set; }

        int SetPointPosition { get; set; }

        ushort SetPointRampTime { get; set; }

        #endregion

        #region Methods

        bool UpdateAnalogIn(ushort analogIn);

        bool UpdateCurrent(ushort current);

        bool UpdateInverterCurrentPosition(int position);

        #endregion
    }
}
