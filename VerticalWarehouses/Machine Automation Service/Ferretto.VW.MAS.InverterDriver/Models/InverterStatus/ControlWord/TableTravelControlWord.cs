using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.Interface.InverterStatus;


namespace Ferretto.VW.MAS.InverterDriver.InverterStatus.ControlWord
{
    public class TableTravelControlWord : ControlWordBase, ITableTravelControlWord
    {
        #region Constructors

        public TableTravelControlWord(ushort controlWordValue)
            : base(controlWordValue)
        {
        }

        public TableTravelControlWord(IControlWord otherControlWord)
            : base(otherControlWord.Value)
        {
        }

        public TableTravelControlWord(ITableTravelControlWord otherControlWord)
            : base(otherControlWord.Value)
        {
        }

        #endregion

        #region Properties

        public bool MotionBlockSelect0
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

        public bool MotionBlockSelect1
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

        public bool MotionBlockSelect2
        {
            set
            {
                if (value)
                {
                    this.Value |= 0x2000;
                }
                else
                {
                    this.Value &= 0xDFFF;
                }
            }
        }

        public bool MotionBlockSelect3
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

        // do not use: it overlaps the horizontal axis
        public bool MotionBlockSelect4
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

        public bool Resume
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

        public bool SequenceMode
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

        public bool StartMotionBlock
        {
            get => (this.Value & 0x0200) > 0;
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

        #endregion
    }
}
