using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.Utils.Messages.FieldData
{
    public class InverterStopFieldMessageData : FieldMessageData, IInverterStopFieldMessageData
    {


        #region Constructors

        public InverterStopFieldMessageData(
            MessageVerbosity verbosity = MessageVerbosity.Debug)
            : base(verbosity)
        {
        }

        #endregion
    }
}
