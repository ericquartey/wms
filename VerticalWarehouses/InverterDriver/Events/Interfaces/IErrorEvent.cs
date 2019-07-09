namespace Ferretto.VW.Drivers.Inverter
{
    /// <summary>
    /// Interface definition for the event [Error]
    /// </summary>
    public interface IErrorEvent
    {
        #region Methods

        void Error(object sender, ErrorEventArgs eventArgs);

        #endregion
    }
}
