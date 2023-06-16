namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface ISensorsChangedMessageData : IMessageData
    {
        #region Properties

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Performance",
            "CA1819:Properties should not return arrays",
            Justification = "Review the code to see if it is really necessary to return a plain array.")]
        bool[] SensorsStatesInput { get; set; }
        bool[] SensorsStatesOutput { get; set; }

        #endregion
    }
}
