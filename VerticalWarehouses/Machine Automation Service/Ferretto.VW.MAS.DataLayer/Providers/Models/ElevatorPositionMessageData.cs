using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer
{
    public class ElevatorPositionMessageData : IMessageData
    {
        #region Constructors

        public ElevatorPositionMessageData(double verticalPosition, double horizontalPosition, int? cellId, int? bayPositionId)
        {
            this.VerticalPosition = verticalPosition;
            this.HorizontalPosition = horizontalPosition;
            this.CellId = cellId;
            this.BayPositionId = bayPositionId;
        }

        #endregion

        #region Properties

        public int? BayPositionId { get; }

        public int? CellId { get; }

        public double HorizontalPosition { get; }

        public MessageVerbosity Verbosity => MessageVerbosity.Info;

        public double VerticalPosition { get; }

        #endregion
    }
}
