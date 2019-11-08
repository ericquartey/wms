using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.Interface.InverterStatus;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.InverterDriver.InverterStatus.ControlWord
{
    public class PositionControlWord : ControlWordBase, IPositionControlWord
    {
        #region Constructors

        public PositionControlWord(ushort controlWordValue)
            : base(controlWordValue)
        {
        }

        public PositionControlWord(IControlWord otherControlWord)
            : base(otherControlWord.Value)
        {
        }

        public PositionControlWord(IPositionControlWord otherControlWord)
            : base(otherControlWord.Value)
        {
        }

        #endregion

        #region Properties

        public bool ChangeSetPoint
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

        public bool ImmediateChangeSet
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

        public bool RelativeMovement
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
