using System;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;

namespace Ferretto.VW.Common_Utils.Messages.Data
{
    public class MovementMessageData : IMovementMessageData
    {
        #region Constructors

        public MovementMessageData(decimal mm, int axis, int movementType, uint speedPercentage = 100)
        {
            try
            {
                this.Mm = mm;
                this.SpeedPercentage = speedPercentage;
                this.Axis = (MovementDirections)axis;
                this.MovementType = (MovementType)movementType;
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

        public uint SpeedPercentage { get => this.SpeedPercentage; set => this.SpeedPercentage = (value > 100) ? 100 : value; }

        public MessageVerbosity Verbosity { get; set; }

        #endregion
    }
}
