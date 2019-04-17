namespace Ferretto.VW.Common_Utils.Messages.Interfaces
{
    public interface IUpDownRepetitiveMessageData : IMessageData
    {
        #region Properties

        /// <summary>
        /// Current position of elevator in vertical movement
        /// </summary>
        decimal CurrentPosition { get; set; }

        /// <summary>
        /// Number of completed cycles
        /// </summary>
        int NumberOfCompletedCycles { get; set; }

        /// <summary>
        /// Number of required cycles
        /// </summary>
        int NumberOfRequiredCycles { get; }

        /// <summary>
        /// Target lower bound for vertical movement
        /// </summary>
        decimal TargetLowerBound { get; }

        /// <summary>
        /// Target upper bound for vertical movement
        /// </summary>
        decimal TargetUpperBound { get; }

        #endregion
    }
}
