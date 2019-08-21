using Ferretto.VW.MAS.Utils.Enumerations;

namespace Ferretto.VW.MAS.Utils.Messages.FieldInterfaces
{
    public interface IInverterSwitchOffFieldMessageData : IFieldMessageData
    {
        #region Properties

        InverterIndex SystemIndex { get; }

        #endregion
    }
}
