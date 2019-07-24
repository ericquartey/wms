using Ferretto.VW.MAS.InverterDriver.Interface.InverterStatus;

namespace Ferretto.VW.MAS.InverterDriver.InverterStatus.StatusWord
{
    public class ProfileVelocityStatusWord : StatusWordBase, IProfileVelocityStatusWord
    {
        #region Constructors

        public ProfileVelocityStatusWord(ushort controlWordValue)
            : base(controlWordValue)
        {
        }

        public ProfileVelocityStatusWord(IStatusWord otherControlWord)
            : base(otherControlWord.Value)
        {
        }

        public ProfileVelocityStatusWord(IProfileVelocityStatusWord otherControlWord)
            : base(otherControlWord.Value)
        {
        }

        #endregion

        #region Properties

        public bool TargetReached { get; }

        #endregion
    }
}
