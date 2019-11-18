using System.Collections.Generic;

namespace Ferretto.VW.MAS.DataModels
{
    public sealed class Elevator : DataModel
    {
        #region Properties

        public IEnumerable<ElevatorAxis> Axes { get; set; }

        /// <summary>
        /// Gets or sets the loading unit currently loaded on the elevator.
        /// </summary>
        public LoadingUnit LoadingUnit { get; set; }

        public int? LoadingUnitId { get; set; }

        public Machine Machine { get; set; }

        public int MachineId { get; set; }

        /// <summary>
        /// Gets or sets the structural properties of the elevator.
        /// </summary>
        public ElevatorStructuralProperties StructuralProperties { get; set; }

        #endregion
    }
}
