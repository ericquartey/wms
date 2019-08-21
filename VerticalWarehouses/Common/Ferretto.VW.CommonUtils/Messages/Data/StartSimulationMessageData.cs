using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class StartSimulationMessageData : IMessageData
    {
        #region Constructors

        public StartSimulationMessageData(MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.Verbosity = verbosity;
        }

        #endregion

        #region Properties

        public MessageVerbosity Verbosity { get; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"Verbosity:{this.Verbosity}";
        }

        #endregion
    }
}
