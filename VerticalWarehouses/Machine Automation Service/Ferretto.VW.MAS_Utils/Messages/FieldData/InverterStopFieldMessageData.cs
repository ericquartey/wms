using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.Utils.Messages.FieldData
{
    public class InverterStopFieldMessageData : FieldMessageData, IInverterStopFieldMessageData
    {
        #region Constructors

        public InverterStopFieldMessageData(
            InverterIndex inverterToStop,
            MessageVerbosity verbosity = MessageVerbosity.Debug)
            : base(verbosity)
        {
            this.InverterToStop = inverterToStop;
        }

        #endregion

        #region Properties

        public InverterIndex InverterToStop { get; set; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"InverterToStop:{this.InverterToStop.ToString()}";
        }

        #endregion
    }
}
