using Ferretto.VW.MAS.InverterDriver.Contracts;

namespace Ferretto.VW.MAS.InverterDriver.Interface.InverterStatus
{
    public interface IHomingControlWord : IControlWord
    {
        #region Properties

        bool HomingOperation { set; }

        #endregion
    }
}
