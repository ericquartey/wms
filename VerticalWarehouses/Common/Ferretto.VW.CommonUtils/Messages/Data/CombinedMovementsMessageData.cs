using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class CombinedMovementsMessageData : ICombinedMovementsMessageData
    {
        #region Constructors

        public CombinedMovementsMessageData()
        {
        }

        public CombinedMovementsMessageData(
            IPositioningMessageData horizontalPositioningMessageData,
            IPositioningMessageData verticalPositioningMessageData,
            MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.HorizontalPositioningMessageData = horizontalPositioningMessageData;
            this.VerticalPositioningMessageData = verticalPositioningMessageData;
            this.Verbosity = verbosity;
        }

        public CombinedMovementsMessageData(ICombinedMovementsMessageData other)
        {
            if (other is null)
            {
                throw new System.ArgumentNullException(nameof(other));
            }

            this.HorizontalPositioningMessageData = other.HorizontalPositioningMessageData;
            this.VerticalPositioningMessageData = other.VerticalPositioningMessageData;
            this.Verbosity = other.Verbosity;
        }

        #endregion

        #region Properties

        public IPositioningMessageData HorizontalPositioningMessageData { get; }

        public MessageVerbosity Verbosity { get; set; } = MessageVerbosity.Debug;

        public IPositioningMessageData VerticalPositioningMessageData { get; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"Combined movements: {this.HorizontalPositioningMessageData.AxisMovement} MovementType:{this.HorizontalPositioningMessageData.MovementType} TargetPosition:{this.HorizontalPositioningMessageData.TargetPosition} TargetSpeed:{this.HorizontalPositioningMessageData.TargetSpeed} " +
                $"- {this.VerticalPositioningMessageData.AxisMovement} MovementType:{this.VerticalPositioningMessageData.MovementType} TargetPosition:{this.VerticalPositioningMessageData.TargetPosition} TargetSpeed:{this.VerticalPositioningMessageData.TargetSpeed} ";
        }

        #endregion
    }
}
