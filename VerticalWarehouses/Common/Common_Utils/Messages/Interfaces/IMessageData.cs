using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    /// <summary>
    ///     Base interface for Event Message Data property used to transfer message related data inside the message
    /// </summary>
    public interface IMessageData
    {
        #region Properties

        MessageVerbosity Verbosity { get; }

        #endregion
    }
}
