﻿using Ferretto.VW.MAS_InverterDriver.Interface.InverterStatus;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS_InverterDriver.InverterStatus.ControlWord
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
            : base(otherControlWord.Value)
        {
        }

        public HomingControlWord(IHomingControlWord otherControlWord)
         : base(otherControlWord.Value)
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
