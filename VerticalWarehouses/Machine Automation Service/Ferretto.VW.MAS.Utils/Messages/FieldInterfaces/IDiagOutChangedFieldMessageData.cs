namespace Ferretto.VW.MAS.Utils.Messages.FieldInterfaces
{
    public interface IDiagOutChangedFieldMessageData : IFieldMessageData
    {
        #region Properties

        int[] CurrentStates { get; set; }

        bool[] FaultStates { get; set; }

        #endregion
    }
}
