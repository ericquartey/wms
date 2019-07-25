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

        #region Properties

        public bool EnableOperation
        {
            set
            {
                if (value)
                {
                    this.Value |= 0x0008;
                }
                else
                {
                    this.Value &= 0xFFF7;
                }
            }
        }

        public bool EnableVoltage
        {
            set
            {
                if (value)
                {
                    this.Value |= 0x0002;
                }
                else
                {
                    this.Value &= 0xFFFD;
                }
            }
        }

        public bool FaultReset
        {
            set
            {
                if (value)
                {
                    this.Value |= 0x0080;
                }
                else
                {
                    this.Value &= 0xFF7F;
                }
            }
        }

        public bool Halt
        {
            set
            {
                if (value)
                {
                    this.Value |= 0x0100;
                }
                else
                {
                    this.Value &= 0xFEFF;
                }
            }
        }

        public bool QuickStop
        {
            set
            {
                if (value)
                {
                    this.Value |= 0x0004;
                }
                else
                {
                    this.Value &= 0xFFFB;
                }
            }
        }

        public bool SwitchOn
        {
            set
            {
                if (value)
                {
                    this.Value |= 0x0001;
                }
                else
                {
                    this.Value &= 0xFFFE;
                }
            }
        }

        #endregion
    }
}
