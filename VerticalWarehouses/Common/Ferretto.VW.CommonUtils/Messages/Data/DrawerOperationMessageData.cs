using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class DrawerOperationMessageData : IDrawerOperationMessageData
    {
        #region Constructors

        public DrawerOperationMessageData(DrawerOperation operation, DrawerOperationStep step,
            MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.Operation = operation;
            this.Step = step;
            this.Verbosity = verbosity;
        }

        #endregion

        #region Properties

        public DrawerDestination Destination { get; set; }

        public DrawerOperation Operation { get; set; }

        public DrawerDestination Source { get; set; }

        public DrawerOperationStep Step { get; set; }

        public MessageVerbosity Verbosity { get; }

        #endregion
    }
}
