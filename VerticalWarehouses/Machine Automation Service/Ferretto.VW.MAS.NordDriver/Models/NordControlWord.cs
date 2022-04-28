using Ferretto.VW.MAS.InverterDriver.Contracts;

namespace Ferretto.VW.MAS.NordDriver
{
    public class NordControlWord : ControlWordBase, INordControlWord
    {
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
