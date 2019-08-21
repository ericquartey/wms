using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.Utils.Messages.FieldData
{
    public class InverterSwitchOffFieldMessageData : FieldMessageData, IInverterSwitchOffFieldMessageData
    {
        #region Constructors

        public InverterSwitchOffFieldMessageData(InverterIndex systemIndex, MessageVerbosity verbosity = MessageVerbosity.Debug)
            : base(verbosity)
        {
            this.SystemIndex = systemIndex;
        }

        #endregion

        #region Properties

        public FieldCommandMessage NextCommandMessage { get; set; }

        public InverterIndex SystemIndex { get; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"SystemIndex:{this.SystemIndex.ToString()} NextCommandMessage:{this.NextCommandMessage}";
        }

        #endregion
    }
}
