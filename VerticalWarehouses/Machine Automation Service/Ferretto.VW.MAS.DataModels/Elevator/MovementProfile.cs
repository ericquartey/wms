﻿using System.Collections.Generic;

namespace Ferretto.VW.MAS.DataModels
{
    public sealed class MovementProfile : DataModel
    {
        #region Properties

        public MovementProfileType Name { get; set; }

        public IEnumerable<StepMovementParameters> Steps { get; set; }

        public double TotalDistance { get; set; }

        #endregion
    }
}
