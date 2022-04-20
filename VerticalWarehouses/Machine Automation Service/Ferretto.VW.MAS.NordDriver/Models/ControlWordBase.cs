namespace Ferretto.VW.MAS.NordDriver
{
    public class ControlWordBase : IControlWord
    {
        #region Fields

        private readonly object controlWordLockObject;

        private ushort controlWord;

        #endregion

        #region Constructors

        public ControlWordBase()
        {
            this.controlWord = 0x0000;
            this.controlWordLockObject = new object();
        }

        public ControlWordBase(ushort controlWordValue)
        {
            this.controlWord = controlWordValue;
            this.controlWordLockObject = new object();
        }

        public ControlWordBase(ControlWordBase otherControlWord)
        {
            this.controlWord = otherControlWord.controlWord;
            this.controlWordLockObject = new object();
        }

        #endregion

        #region Properties

        public bool EnableOperation
        {
            get
            {
                lock (this.controlWordLockObject)
                {
                    return (this.controlWord & 0x0008) > 0;
                }
            }
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
            get
            {
                lock (this.controlWordLockObject)
                {
                    return (this.controlWord & 0x0002) > 0;
                }
            }
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
            get
            {
                lock (this.controlWordLockObject)
                {
                    return (this.controlWord & 0x0080) > 0;
                }
            }
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
            get
            {
                lock (this.controlWordLockObject)
                {
                    return (this.controlWord & 0x0100) > 0;
                }
            }
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

        public bool HorizontalAxis
        {
            get => (this.Value & 0x8000) > 0;
            set
            {
                if (value)
                {
                    this.Value |= 0x8000;
                }
                else
                {
                    this.Value &= 0x7FFF;
                }
            }
        }

        public bool NewSetPoint
        {
            get
            {
                lock (this.controlWordLockObject)
                {
                    return (this.controlWord & 0x0010) > 0;
                }
            }
            set
            {
                lock (this.controlWordLockObject)
                {
                    if (value)
                    {
                        this.controlWord |= 0x0010;
                    }
                    else
                    {
                        this.controlWord &= 0xFFEF;
                    }
                }
            }
        }

        public bool ParameterSet1
        {
            get
            {
                lock (this.controlWordLockObject)
                {
                    return (this.controlWord & 0x0020) > 0;
                }
            }
            set
            {
                lock (this.controlWordLockObject)
                {
                    if (value)
                    {
                        this.controlWord |= 0x0020;
                    }
                    else
                    {
                        this.controlWord &= 0xFFDF;
                    }
                }
            }
        }

        public bool ParameterSet2
        {
            get
            {
                lock (this.controlWordLockObject)
                {
                    return (this.controlWord & 0x0040) > 0;
                }
            }
            set
            {
                lock (this.controlWordLockObject)
                {
                    if (value)
                    {
                        this.controlWord |= 0x0040;
                    }
                    else
                    {
                        this.controlWord &= 0xFFBF;
                    }
                }
            }
        }

        public bool QuickStop
        {
            get
            {
                lock (this.controlWordLockObject)
                {
                    return (this.controlWord & 0x0004) > 0;
                }
            }
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
            get
            {
                lock (this.controlWordLockObject)
                {
                    return (this.controlWord & 0x0001) > 0;
                }
            }
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

        public ushort Value
        {
            get
            {
                lock (this.controlWordLockObject)
                {
                    return this.controlWord;
                }
            }
            set
            {
                lock (this.controlWordLockObject)
                {
                    this.controlWord = value;
                }
            }
        }

        #endregion
    }
}
