using Ferretto.VW.MAS_Utils.Enumerations;

namespace Ferretto.VW.MAS_Utils.Messages.FieldInterfaces
{
    public interface IStopAxisFieldMessageData : IFieldMessageData
    {
        #region Properties

        Axis AxisToStop { get; }

        #endregion
    }
}
