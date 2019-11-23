using System;
using Ferretto.VW.MAS.InverterDriver.Contracts;
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
            : base(otherControlWord?.Value ?? throw new ArgumentNullException(nameof(otherControlWord)))
        {
        }

        public ProfileVelocityStatusWord(IProfileVelocityStatusWord otherControlWord)
            : base(otherControlWord?.Value ?? throw new ArgumentNullException(nameof(otherControlWord)))
        {
        }

        #endregion

        #region Properties

        public bool TargetReached => (this.Value & 0x0400) > 0;

        #endregion
    }
}
