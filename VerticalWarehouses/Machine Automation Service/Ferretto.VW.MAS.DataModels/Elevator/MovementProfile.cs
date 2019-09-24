using System.Collections.Generic;

namespace Ferretto.VW.MAS.DataModels
{
    public sealed class MovementProfile : DataModel
    {
        #region Properties

        public decimal Correction { get; set; }

        public MovementProfileType Name { get; set; }

        public IEnumerable<StepMovementParameters> Steps { get; set; }

        public decimal TotalDistance { get; set; }

        #endregion
    }
}
