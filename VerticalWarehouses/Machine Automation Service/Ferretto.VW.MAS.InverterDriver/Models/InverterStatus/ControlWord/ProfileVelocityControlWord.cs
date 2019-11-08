using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.Interface.InverterStatus;

namespace Ferretto.VW.MAS.InverterDriver.InverterStatus.ControlWord
{
    public class ProfileVelocityControlWord : ControlWordBase, IProfileVelocityControlWord
    {
        #region Constructors

        public ProfileVelocityControlWord(ushort controlWordValue)
            : base(controlWordValue)
        {
        }

        public ProfileVelocityControlWord(IControlWord otherControlWord)
            : base(otherControlWord.Value)
        {
        }

        public ProfileVelocityControlWord(IProfileVelocityControlWord otherControlWord)
            : base(otherControlWord.Value)
        {
        }

        #endregion
    }
}
