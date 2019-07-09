namespace Ferretto.VW.Drivers.Inverter
{
    /// <summary>
    /// Interface definition for the event [SelectTelegramDone]
    /// </summary>
    public interface ISelectTelegramDone
    {
        #region Methods

        void SelectTelegramDone(object sender, SelectTelegramDoneEventArgs eventArgs);

        #endregion
    }
}
