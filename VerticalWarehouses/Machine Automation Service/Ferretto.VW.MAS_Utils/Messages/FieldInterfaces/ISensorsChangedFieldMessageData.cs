namespace Ferretto.VW.MAS_Utils.Messages.FieldInterfaces
{
    public interface ISensorsChangedFieldMessageData : IFieldMessageData
    {
        #region Properties

        bool[] SensorsStates { get; set; }

        bool SensorsStatus { get; set; }

        #endregion
    }
}
