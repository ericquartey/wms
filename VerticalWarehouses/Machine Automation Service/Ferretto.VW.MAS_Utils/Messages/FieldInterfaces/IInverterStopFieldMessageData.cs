using Ferretto.VW.MAS_Utils.Enumerations;

namespace Ferretto.VW.MAS_Utils.Messages.FieldInterfaces
{
    public interface IInverterStopFieldMessageData : IFieldMessageData
    {
        #region Properties

        InverterIndex InverterToStop { get; set; }

        #endregion
    }
}
