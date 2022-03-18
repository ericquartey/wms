namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface IDiagOutChangedMessageData : IMessageData
    {
        #region Properties

        int[] CurrentStates { get; set; }

        bool[] FaultStates { get; set; }

        #endregion
    }
}
