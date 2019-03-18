using Ferretto.VW.Common_Utils.Messages.Interfaces;

namespace Ferretto.VW.MAS_Utils.Messages.Interfaces
{
    public interface IUpDownRepetitiveNotificationMessageData : IMessageData
    {
        #region Properties

        /// <summary>
        /// Number of completed cycles
        /// </summary>
        int NumberOfCompletedCycles { get; }

        #endregion
    }
}
