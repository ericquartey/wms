namespace Ferretto.VW.MAS_InverterDriver.Interface.InverterStatus
{
    public interface IHomingControlWord : IControlWord
    {
        #region Properties

        bool HomingOperation { set; }

        #endregion
    }
}
