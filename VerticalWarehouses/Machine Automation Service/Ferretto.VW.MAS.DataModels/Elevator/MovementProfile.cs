using System;
using System.Collections.Generic;

namespace Ferretto.VW.MAS.DataModels
{
    public sealed class MovementProfile : DataModel
    {
        #region Constructors

        public MovementProfile()
        {
        }

        public MovementProfile(MovementProfileType name, IEnumerable<StepMovementParameters> steps, double totalDistance)
        {
            this.Name = name;
            this.Steps = steps;
            this.TotalDistance = totalDistance;
        }

        #endregion

        #region Properties

        public int? ElevatorAxisId { get; set; }

        public MovementProfileType Name { get; set; }

        public IEnumerable<StepMovementParameters> Steps { get; set; }

        public double TotalDistance { get; set; }

        #endregion
    }
}
