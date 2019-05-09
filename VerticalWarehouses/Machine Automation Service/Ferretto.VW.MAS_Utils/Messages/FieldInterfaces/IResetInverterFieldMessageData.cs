using Ferretto.VW.Common_Utils.Messages.Enumerations;

namespace Ferretto.VW.MAS_Utils.Messages.FieldInterfaces
{
    public interface IResetInverterFieldMessageData : IFieldMessageData
    {
        #region Properties

        Axis AxisToStop { get; }

        ShutterPosition ShutterPosition { get; }

        #endregion
    }
}
