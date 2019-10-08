using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.Utils.Messages.FieldInterfaces
{
    public interface IInverterSwitchOnFieldMessageData : IFieldMessageData
    {
        #region Properties

        Axis AxisToSwitchOn { get; }

        #endregion
    }
}
