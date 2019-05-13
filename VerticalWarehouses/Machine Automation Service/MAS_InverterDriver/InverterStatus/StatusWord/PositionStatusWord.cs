using Ferretto.VW.MAS_InverterDriver.Interface.InverterStatus;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_InverterDriver.InverterStatus.StatusWord
{
    public class PositionStatusWord : StatusWordBase, IPositionStatusWord
    {
        #region Constructors

        public PositionStatusWord(ushort controlWordValue)
            : base(controlWordValue)
        {
        }

        public PositionStatusWord(IStatusWord otherControlWord)
            : base(otherControlWord.Value)
        {
        }

        public PositionStatusWord(IPositionStatusWord otherControlWord)
            : base(otherControlWord.Value)
        {
        }

        #endregion

        #region Properties

        public bool FollowingError => (this.Value & 0x2000) > 0;

        public bool PositioningAttained { get; }

        public bool SetPointAcknowledge => (this.Value & 0x1000) > 0;

        #endregion
    }
}
