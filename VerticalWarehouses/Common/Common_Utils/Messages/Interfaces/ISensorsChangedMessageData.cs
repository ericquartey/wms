namespace Ferretto.VW.Common_Utils.Messages.Interfaces
{
    public interface ISensorsChangedMessageData : IMessageData
    {
        #region Properties

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Performance",
            "CA1819:Properties should not return arrays",
            Justification = "Review the code to see if it is really necessary to return a plain array.")]
        bool[] SensorsStates { get; set; }

        #endregion
    }
}
