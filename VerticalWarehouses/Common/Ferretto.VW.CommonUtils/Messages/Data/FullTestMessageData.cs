using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class FullTestMessageData : IFullTestMessageData
    {
        #region Constructors

        public FullTestMessageData()
        {
        }

        public FullTestMessageData(
            CommandAction commandAction,
            List<int> loadUnitIds,
            int? cyclesTodo,
            int? cyclesDone,
            MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.CommandAction = commandAction;
            this.LoadUnitIds = loadUnitIds;
            if (cyclesTodo.HasValue)
            {
                this.CyclesTodo = cyclesTodo.Value;
            }
            if (cyclesDone.HasValue)
            {
                this.CyclesDone = cyclesDone.Value;
            }
            this.Verbosity = verbosity;
        }

        #endregion

        #region Properties

        public CommandAction CommandAction { get; }

        public int CyclesDone { get; set; }

        public int CyclesTodo { get; }

        public List<int> LoadUnitIds { get; }

        public MessageVerbosity Verbosity { get; }

        #endregion
    }
}
