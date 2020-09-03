using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class MoveTestMessageData : IMoveTestMessageData
    {
        #region Constructors

        public MoveTestMessageData()
        {
        }

        public MoveTestMessageData(int executedCycles, int requiredCycles, List<int> loadUnitsToTest, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.ExecutedCycles = executedCycles;
            this.RequiredCycles = requiredCycles;
            this.LoadUnitsToTest = loadUnitsToTest;

            this.Verbosity = verbosity;
        }

        #endregion

        #region Properties

        public int ExecutedCycles { get; set; }

        public List<int> LoadUnitsToTest { get; set; }

        public int RequiredCycles { get; set; }

        public MessageVerbosity Verbosity { get; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"RequiredCycles:{this.RequiredCycles} ExecutedCycles:{this.ExecutedCycles} ";
        }

        #endregion
    }
}
