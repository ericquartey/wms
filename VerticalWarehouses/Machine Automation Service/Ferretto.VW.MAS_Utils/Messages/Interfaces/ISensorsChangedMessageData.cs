namespace Ferretto.VW.MAS_Utils.Messages.Interfaces
{
    public interface ISensorsChangedMessageData : IMessageData
    {
        #region Properties

        bool[] SensorsStates { get; set; }

        #endregion
    }
}
