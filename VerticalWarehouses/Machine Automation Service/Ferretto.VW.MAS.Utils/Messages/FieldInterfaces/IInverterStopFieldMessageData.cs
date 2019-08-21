using Ferretto.VW.MAS.Utils.Enumerations;

namespace Ferretto.VW.MAS.Utils.Messages.FieldInterfaces
{
    public interface IInverterStopFieldMessageData : IFieldMessageData
    {
        #region Properties

        InverterIndex InverterToStop { get; set; }

        #endregion
    }
}
