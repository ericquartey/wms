namespace Ferretto.VW.Drivers.Inverter
{
    /// <summary>
    /// IConnectedEvent: interface definition for the event [Connected]
    /// </summary>
    public interface IConnectedEvent
    {
        #region Methods

        void Connected(object sender, ConnectedEventArgs eventArgs);

        #endregion
    }
}
