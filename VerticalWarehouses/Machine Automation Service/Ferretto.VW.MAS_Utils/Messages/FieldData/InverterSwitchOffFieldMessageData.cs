using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.Utils.Messages.FieldData
{
    public class InverterSwitchOffFieldMessageData : IInverterSwitchOffFieldMessageData
    {
        #region Constructors

        public InverterSwitchOffFieldMessageData(InverterIndex systemIndex, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.SystemIndex = systemIndex;
            this.Verbosity = verbosity;
        }

        #endregion

        #region Properties

        public FieldCommandMessage NextCommandMessage { get; set; }

        public InverterIndex SystemIndex { get; }

        public MessageVerbosity Verbosity { get; }

        #endregion
    }
}
