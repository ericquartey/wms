using Ferretto.VW.MAS.InverterDriver.Contracts;

namespace Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces
{
    public interface INordInverterStatus : IInverterStatusBase
    {
        #region Properties

        ushort SetPointFrequency { get; set; }

        int SetPointPosition { get; set; }

        ushort SetPointRampTime { get; set; }

        #endregion

        #region Methods

        bool UpdateInverterCurrentPosition(int position);

        #endregion
    }
}
