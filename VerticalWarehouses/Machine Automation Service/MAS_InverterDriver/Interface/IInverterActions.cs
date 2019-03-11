namespace Ferretto.VW.MAS_InverterDriver.Interface
{
    public interface IInverterActions
    {
        #region Events

        event EndEventHandler EndEvent;

        event ErrorEventHandler ErrorEvent;

        #endregion
    }
}
