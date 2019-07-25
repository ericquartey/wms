using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.Utils.Messages.FieldInterfaces
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
