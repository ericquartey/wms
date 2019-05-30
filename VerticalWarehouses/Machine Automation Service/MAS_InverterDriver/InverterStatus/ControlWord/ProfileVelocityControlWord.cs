using Ferretto.VW.MAS_InverterDriver.Interface.InverterStatus;

namespace Ferretto.VW.MAS_InverterDriver.InverterStatus.ControlWord
{
    public class ProfileVelocityControlWord : ControlWordBase, IProfileVelocityControlWord
    {
        #region Fields

        private readonly object controlWordLockObject = new object();

        private ushort controlWord;

        #endregion

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
                lock (this.controlWordLockObject)
                {
                    if (value)
                    {
                        this.controlWord |= 0x0008;
                    }
                    else
                    {
                        this.controlWord &= 0xFFF7;
                    }
                }
            }
        }

        public bool EnableVoltage
        {
            set
            {
                lock (this.controlWordLockObject)
                {
                    if (value)
                    {
                        this.controlWord |= 0x0002;
                    }
                    else
                    {
                        this.controlWord &= 0xFFFD;
                    }
                }
            }
        }

        public bool FaultReset
        {
            set
            {
                lock (this.controlWordLockObject)
                {
                    if (value)
                    {
                        this.controlWord |= 0x0080;
                    }
                    else
                    {
                        this.controlWord &= 0xFF7F;
                    }
                }
            }
        }

        public bool Halt
        {
            set
            {
                lock (this.controlWordLockObject)
                {
                    if (value)
                    {
                        this.controlWord |= 0x0100;
                    }
                    else
                    {
                        this.controlWord &= 0xFEFF;
                    }
                }
            }
        }

        public bool QuickStop
        {
            set
            {
                lock (this.controlWordLockObject)
                {
                    if (value)
                    {
                        this.controlWord |= 0x0004;
                    }
                    else
                    {
                        this.controlWord &= 0xFFFB;
                    }
                }
            }
        }

        public bool SwitchOn
        {
            set
            {
                lock (this.controlWordLockObject)
                {
                    if (value)
                    {
                        this.controlWord |= 0x0001;
                    }
                    else
                    {
                        this.controlWord &= 0xFFFE;
                    }
                }
            }
        }

        #endregion
    }
}
