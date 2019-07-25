namespace Ferretto.VW.MAS.Utils.Messages.FieldInterfaces
{
    public interface ISensorsChangedFieldMessageData : IFieldMessageData
    {
        #region Properties

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Performance",
            "CA1819:Properties should not return arrays",
            Justification = "Review the code to see if it is really necessary to return a plain array.")]
        bool[] SensorsStates { get; set; }

        bool SensorsStatus { get; set; }

        #endregion
    }
}
