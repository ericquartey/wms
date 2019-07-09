namespace Ferretto.VW.Drivers.Inverter
{
    /// <summary>
    /// Interface definition for the event [LastRequestDone].
    /// </summary>
    public interface ILastRequestDone
    {
        #region Methods

        void LastRequestDone(object sender, LastRequestDoneEventArgs eventArgs);

        #endregion
    }
}
