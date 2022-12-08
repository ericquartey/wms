using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class StatisticsMessageData : IStatisticsMessageData
    {
        #region Constructors

        public StatisticsMessageData()
        {
        }

        public StatisticsMessageData(
            MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.Verbosity = verbosity;
        }

        #endregion

        #region Properties

        public MessageVerbosity Verbosity { get; }

        #endregion
    }
}
