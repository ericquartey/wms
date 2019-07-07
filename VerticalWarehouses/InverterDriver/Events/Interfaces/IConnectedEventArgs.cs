namespace Ferretto.VW.Drivers.Inverter
{
    /// <summary>
    /// Interface definition for [Connected] event arguments.
    /// </summary>
    public interface IConnectedEventArgs
    {
        #region Properties

        bool State { get; }

        #endregion
    }
}
