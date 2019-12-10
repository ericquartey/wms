using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer
{
    public class ElevatorPositionMessageData : IMessageData
    {
        #region Constructors

        public ElevatorPositionMessageData(double verticalPosition, double horizontalPosition, int? cellId, int? bayPositionId, bool? bayPositionUpper)
        {
            this.VerticalPosition = verticalPosition;
            this.HorizontalPosition = horizontalPosition;
            this.CellId = cellId;
            this.BayPositionId = bayPositionId;
            this.BayPositionUpper = bayPositionUpper;
        }

        #endregion

        #region Properties

        public int? BayPositionId { get; }

        public bool? BayPositionUpper { get; }

        public int? CellId { get; }

        public double HorizontalPosition { get; }

        public MessageVerbosity Verbosity => MessageVerbosity.Info;

        public double VerticalPosition { get; }

        #endregion
    }
}
