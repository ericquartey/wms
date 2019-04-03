using Ferretto.VW.Common_Utils.DTOs;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages.Interfaces;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_Utils.Messages.Data
{
    public class MovementMessageData : IMovementMessageData
    {
        #region Constructors

        public MovementMessageData(decimal displacement, Axis axis, MovementType movementType, uint speedPercentage = 100)
        {
            this.Displacement = displacement;
            this.SpeedPercentage = speedPercentage;
            this.Axis = axis;
            this.MovementType = movementType;
        }

        public MovementMessageData(MovementMessageDataDTO dto)
        {
            this.Displacement = dto.Displacement;
            this.Axis = (Axis)dto.Axis;
            this.MovementType = (MovementType)dto.MovementType;
            this.SpeedPercentage = dto.SpeedPercentage;
        }

        #endregion

        #region Properties

        public Axis Axis { get; set; }

        public decimal Displacement { get; set; }

        public MovementType MovementType { get; set; }

        public uint SpeedPercentage { get; set; }

        public MessageVerbosity Verbosity { get; set; }

        #endregion
    }
}
