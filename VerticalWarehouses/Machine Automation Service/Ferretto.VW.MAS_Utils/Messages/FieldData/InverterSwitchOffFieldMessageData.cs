using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages.FieldInterfaces;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS_Utils.Messages.FieldData
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

        #region Methods

        public override string ToString()
        {
            return $"SystemIndex:{this.SystemIndex.ToString()} NextCommandMessage:{this.NextCommandMessage}";
        }

        #endregion
    }
}
