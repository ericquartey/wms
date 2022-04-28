using Ferretto.VW.MAS.InverterDriver.Contracts;

namespace Ferretto.VW.MAS.NordDriver
{
    public interface INordInverterStatus : IInverterStatusBase
    {
        #region Properties

        ushort SetPointFrequency { get; set; }

        int SetPointPosition { get; set; }

        ushort SetPointRampTime { get; set; }

        #endregion
    }
}
