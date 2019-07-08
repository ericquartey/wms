namespace Ferretto.VW.Drivers.Inverter
{
    /// <summary>
    /// Interface definition for the event [EnquiryTelegramDone]
    /// </summary>
    public interface IEnquiryTelegramDone
    {
        #region Methods

        void EnquiryTelegramDone(object sender, EnquiryTelegramDoneEventArgs eventArgs);

        #endregion
    }
}
