using System;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.Interface.InverterStatus;


namespace Ferretto.VW.MAS.InverterDriver.InverterStatus.ControlWord
{
    public class HomingControlWord : ControlWordBase, IHomingControlWord
    {
        #region Constructors

        public HomingControlWord()
        {
        }

        public HomingControlWord(ushort controlWordValue)
            : base(controlWordValue)
        {
        }

        public HomingControlWord(IControlWord otherControlWord)
            : base(otherControlWord?.Value ?? throw new ArgumentNullException(nameof(otherControlWord)))
        {
        }

        public HomingControlWord(IHomingControlWord otherControlWord)
         : base(otherControlWord?.Value ?? throw new ArgumentNullException(nameof(otherControlWord)))
        {
        }

        #endregion

        #region Properties

        public bool HomingOperation
        {
            set
            {
                if (value)
                {
                    this.Value |= 0x0010;
                }
                else
                {
                    this.Value &= 0xFFEF;
                }
            }
        }

        #endregion
    }
}
