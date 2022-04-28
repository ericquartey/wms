using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.Interface.InverterStatus;

namespace Ferretto.VW.MAS.InverterDriver.InverterStatus.StatusWord
{
    public class PositionStatusWord : StatusWordBase, IPositionStatusWord
    {
        #region Constructors

        public PositionStatusWord(ushort controlWordValue)
            : base(controlWordValue)
        {
        }

        public PositionStatusWord(IStatusWord otherControlWord)
            : base(otherControlWord?.Value ?? throw new System.ArgumentNullException(nameof(otherControlWord)))
        {
        }

        public PositionStatusWord(IPositionStatusWord otherControlWord)
            : base(otherControlWord?.Value ?? throw new System.ArgumentNullException(nameof(otherControlWord)))
        {
        }

        #endregion

        #region Properties

        public bool FollowingError => (this.Value & 0x2000) > 0;

        public bool PositioningAttained => (this.Value & 0x0400) > 0;

        public bool SetPointAcknowledge => (this.Value & 0x1000) > 0;

        #endregion
    }
}
