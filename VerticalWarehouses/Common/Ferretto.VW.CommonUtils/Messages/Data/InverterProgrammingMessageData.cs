using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class InverterProgrammingMessageData : IInverterProgrammingMessageData
    {
        #region Constructors

        public InverterProgrammingMessageData()
        {
        }

        public InverterProgrammingMessageData(IEnumerable<InverterParametersData> inverterParametersData, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.InverterParametersData = inverterParametersData;
            this.Verbosity = verbosity;
        }

        #endregion

        #region Properties

        public IEnumerable<InverterParametersData> InverterParametersData { get; set; }

        public MessageVerbosity Verbosity { get; set; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"Parameters:{this.InverterParametersData} Verbosity:{this.Verbosity}";
        }

        #endregion
    }
}
