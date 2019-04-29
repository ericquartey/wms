using Ferretto.VW.MAS_Utils.Enumerations;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_Utils.Messages.FieldData
{
    public class InverterStartFieldMessageData : IInverterStartFieldMessageData
    {
        #region Constructors

        private InverterStartFieldMessageData(InverterIndex inverterToStart)
        {
            this.InverterToStart = inverterToStart;
        }

        #endregion

        #region Properties

        public InverterIndex InverterToStart { get; set; }

        #endregion
    }
}
