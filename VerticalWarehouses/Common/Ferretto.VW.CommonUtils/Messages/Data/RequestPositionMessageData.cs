using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class RequestPositionMessageData : IRequestPositionMessageData
    {
        #region Constructors

        public RequestPositionMessageData()
        {
        }

        public RequestPositionMessageData(Axis currentAxis, int bayNumber, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.BayNumber = bayNumber;
            this.CurrentAxis = currentAxis;
            this.Verbosity = verbosity;
        }

        #endregion

        #region Properties

        public int BayNumber { get; }

        public Axis CurrentAxis { get; }

        public MessageVerbosity Verbosity { get; }

        #endregion
    }
}
