using Ferretto.VW.MAS_Utils.Enumerations;

namespace Ferretto.VW.MAS_Utils.Messages.FieldData
{
    public interface IInverterStartFieldMessageData
    {
        #region Properties

        InverterIndex InverterToStart { get; set; }

        #endregion
    }
}
