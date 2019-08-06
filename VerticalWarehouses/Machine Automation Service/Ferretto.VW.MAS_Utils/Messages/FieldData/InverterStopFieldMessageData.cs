using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.Utils.Messages.FieldData
{
    public class InverterStopFieldMessageData : IInverterStopFieldMessageData
    {
        #region Constructors

        public InverterStopFieldMessageData(InverterIndex inverterToStop, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.InverterToStop = inverterToStop;
            this.Verbosity = verbosity;
        }

        #endregion

        #region Properties

        public InverterIndex InverterToStop { get; set; }

        public MessageVerbosity Verbosity { get; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"InverterToStop:{this.InverterToStop.ToString()}";
        }

        #endregion
    }
}
