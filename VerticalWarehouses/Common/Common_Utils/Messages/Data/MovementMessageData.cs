using System;
using Ferretto.VW.Common_Utils.DTOs;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;

namespace Ferretto.VW.Common_Utils.Messages.Data
{
    public class MovementMessageData : IMovementMessageData
    {
        #region Constructors

        public MovementMessageData(decimal mm, MovementDirections axis, MovementType movementType, uint speedPercentage = 100)
        {
            try
            {
                this.Mm = mm;
                this.SpeedPercentage = speedPercentage;
                this.Axis = axis;
                this.MovementType = movementType;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public MovementMessageData(MovementMessageDataDTO dto)
        {
            try
            {
                this.Mm = dto.Mm;
                this.Axis = (MovementDirections)dto.Axis;
                this.MovementType = (MovementType)dto.MovementType;
                this.SpeedPercentage = dto.SpeedPercentage;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region Properties

        public MovementDirections Axis { get; set; }

        public decimal Mm { get; set; }

        public MovementType MovementType { get; set; }

        public uint SpeedPercentage { get; set; }

        public MessageVerbosity Verbosity { get; set; }

        #endregion
    }
}
