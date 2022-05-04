using Ferretto.VW.MAS.InverterDriver.Contracts;

namespace Ferretto.VW.MAS.InverterDriver.Interface.InverterStatus
{
    public interface INordControlWord : IControlWord
    {
        #region Properties

        bool FreeBit10 { set; }

        bool NewSetPoint { set; }

        bool ParameterSet1 { set; }

        bool ParameterSet2 { set; }

        #endregion
    }
}
