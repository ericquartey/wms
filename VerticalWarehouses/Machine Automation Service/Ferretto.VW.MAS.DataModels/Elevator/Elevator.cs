using System.Collections.Generic;

namespace Ferretto.VW.MAS.DataModels
{
    public sealed class Elevator : DataModel
    {
        #region Properties

        public IEnumerable<ElevatorAxis> Axes { get; set; }

        public int? LoadingUnitOnBoard { get; set; }

        public ElevatorStructuralProperties StructuralProperties { get; set; }

        #endregion
    }
}
