namespace Ferretto.VW.MAS_Utils.Messages.FieldInterfaces
{
    public interface ISensorsChangedFiledMessageData : IFieldMessageData
    {
        #region Properties

        bool[] SensorsStates { get; set; }

        #endregion
    }
}
