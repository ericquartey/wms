using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.Interface.InverterStatus;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.InverterDriver.InverterStatus.StatusWord
{
    public class HomingStatusWord : StatusWordBase, IHomingStatusWord
    {
        #region Constructors

        public HomingStatusWord(ushort controlWordValue)
            : base(controlWordValue)
        {
        }

        public HomingStatusWord(IStatusWord otherStatusWord)
            : base(otherStatusWord.Value)
        {
        }

        public HomingStatusWord(IHomingStatusWord otherControlWord)
            : base(otherControlWord.Value)
        {
        }

        #endregion

        #region Properties

        public bool HomingAttained => (this.Value & 0x1000) > 0;

        public bool HomingError => (this.Value & 0x2000) > 0;

        public bool InternalLimitActive => (this.Value & 0x0800) > 0;

        public bool TargetReached => (this.Value & 0x0400) > 0;

        #endregion
    }
}
