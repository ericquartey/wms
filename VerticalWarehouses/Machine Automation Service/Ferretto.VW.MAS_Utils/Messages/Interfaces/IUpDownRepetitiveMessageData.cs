using Ferretto.VW.Common_Utils.Messages.Interfaces;

namespace Ferretto.VW.MAS_Utils.Messages.Interfaces
{
    public interface IUpDownRepetitiveMessageData : IMessageData
    {
        #region Properties

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
