﻿// ReSharper disable ArrangeThisQualifier

using Ferretto.VW.MAS.InverterDriver.Interface.InverterStatus;

namespace Ferretto.VW.MAS.InverterDriver.InverterStatus.ControlWord
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

        public bool HeartBeat
        {
            get
            {
                lock (this.controlWordLockObject)
                {
                    return (this.controlWord & 0x0400) > 0;
                }
            }
            set
            {
                lock (this.controlWordLockObject)
                {
                    if (value)
                    {
                        this.controlWord |= 0x0400;
                    }
                    else
                    {
                        this.controlWord &= 0xFBFF;
                    }
                }
            }
        }

        public bool HorizontalAxis
        {
            get
            {
                return (this.Value & 0x8000) > 0;
            }
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
