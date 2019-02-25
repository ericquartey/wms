using Ferretto.VW.Common_Utils.Enumerations;

namespace Ferretto.VW.Common_Utils.Messages.Interfaces
{
    /// <summary>
    /// Base interface for Event Message Data property used to transfer message related data inside the message
    /// </summary>
    public interface IMessageData
    {
        MessageVerbosity Verbosity { get; }
    }
}
