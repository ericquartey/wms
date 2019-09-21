using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.VW.MAS.DataModels
{
    public class StepMovementParameters : MovementParameters
    {
        #region Properties

        public decimal Correction { get; set; }

        public int Step { get; set; }

        public decimal TotalDistance { get; set; }

        public MovementProfileType TypeName { get; set; }

        #endregion
    }
}
