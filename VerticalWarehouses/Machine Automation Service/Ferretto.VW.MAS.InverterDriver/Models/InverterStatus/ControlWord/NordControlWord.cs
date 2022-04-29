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

        public bool NewSetPoint
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

        public bool ParameterSet1
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

        public bool ParameterSet2
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

        #endregion
    }
}
