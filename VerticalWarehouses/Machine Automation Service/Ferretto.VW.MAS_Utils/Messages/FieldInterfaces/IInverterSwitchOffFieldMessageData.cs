using Ferretto.VW.MAS_Utils.Enumerations;

namespace Ferretto.VW.MAS_Utils.Messages.FieldInterfaces
{
    public interface IInverterSwitchOffFieldMessageData : IFieldMessageData
    {
        #region Properties

        InverterIndex SystemIndex { get; }

        #endregion
    }
}
