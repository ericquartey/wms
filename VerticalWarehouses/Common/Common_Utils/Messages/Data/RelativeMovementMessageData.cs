using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;

namespace Ferretto.VW.Common_Utils.Messages.Data
{
    public class RelativeMovementMessageData : IRelativeMovementMessageData
    {
        #region Constructors

        public RelativeMovementMessageData()
        {
        }

        public RelativeMovementMessageData(decimal desiredMovement, MovementDirections movementDirection, MessageVerbosity verbosity = MessageVerbosity.Info)
        {
            this.DesiredMovement = desiredMovement;
            this.MovementDirection = movementDirection;
            this.Verbosity = verbosity;
        }

        #endregion

        #region Properties

        public decimal DesiredMovement { get; set; }

        public MovementDirections MovementDirection { get; set; }

        public MessageVerbosity Verbosity { get; set; }

        #endregion
    }
}
