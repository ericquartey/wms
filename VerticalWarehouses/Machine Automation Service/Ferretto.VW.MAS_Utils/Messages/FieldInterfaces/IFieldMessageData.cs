using Ferretto.VW.Common_Utils.Enumerations;

namespace Ferretto.VW.MAS_Utils.Messages.FieldInterfaces
{
    /// <summary>
    ///     Base interface for Event Message Data property used to transfer message related data inside the message
    /// </summary>
    public interface IFieldMessageData
    {
        #region Properties

        MessageVerbosity Verbosity { get; }

        #endregion
    }
}
