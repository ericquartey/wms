using System;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.Interface.InverterStatus;

namespace Ferretto.VW.MAS.InverterDriver.InverterStatus.ControlWord
{
    public class NordControlWord : ControlWordBase, INordControlWord
    {
        #region Constructors

        public NordControlWord()
        {
        }

        public NordControlWord(ushort controlWordValue)
            : base(controlWordValue)
        {
        }

        public NordControlWord(IControlWord otherControlWord)
            : base(otherControlWord?.Value ?? throw new ArgumentNullException(nameof(otherControlWord)))
        {
        }

        public NordControlWord(INordControlWord otherControlWord)
         : base(otherControlWord?.Value ?? throw new ArgumentNullException(nameof(otherControlWord)))
        {
        }

        #endregion

        #region Properties

        public bool ControlWordValid
        {
            set
            {
                if (value)
                {
                    this.Value |= 0x0400;
                }
                else
                {
                    this.Value &= 0xFBFF;
                }
            }
        }

        public bool DisableVoltage
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

        public bool EnableAcceleration
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

        public bool EnableRamp
        {
            set
            {
                if (value)
                {
                    this.Value |= 0x0020;
                }
                else
                {
                    this.Value &= 0xFFDF;
                }
            }
        }

        public bool EnableSetPoint
        {
            set
            {
                if (value)
                {
                    this.Value |= 0x0040;
                }
                else
                {
                    this.Value &= 0xFFBF;
                }
            }
        }

        public bool NotReadyForOperation
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

        public bool ParameterSet1
        {
            set
            {
                if (value)
                {
                    this.Value |= 0x4000;
                }
                else
                {
                    this.Value &= 0xBFFF;
                }
            }
        }

        public bool ParameterSet2
        {
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

        public bool RotationLeft
        {
            set
            {
                if (value)
                {
                    this.Value |= 0x1000;
                }
                else
                {
                    this.Value &= 0xEFFF;
                }
            }
        }

        public bool RotationRight
        {
            set
            {
                if (value)
                {
                    this.Value |= 0x0800;
                }
                else
                {
                    this.Value &= 0xF7FF;
                }
            }
        }

        public bool Start480_11
        {
            set
            {
                if (value)
                {
                    this.Value |= 0x0200;
                }
                else
                {
                    this.Value &= 0xFDFF;
                }
            }
        }

        public bool Start480_12
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

        #endregion
    }
}
