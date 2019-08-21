using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.Utils.Messages.FieldData
{
    public class InverterFaultFieldMessageData : FieldMessageData,  IInverterFaultFieldMessageData
    {
        #region Constructors

        public InverterFaultFieldMessageData(
            InverterIndex inverterToReset,
            MessageVerbosity verbosity = MessageVerbosity.Debug)
            : base(verbosity)
        {
            this.InverterToReset = inverterToReset;
        }

        #endregion

        #region Properties

        public InverterIndex InverterToReset { get; set; }

        #endregion
    }
}
