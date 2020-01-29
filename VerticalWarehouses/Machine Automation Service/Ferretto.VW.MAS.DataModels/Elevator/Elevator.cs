using System.Collections.Generic;
using System.Linq;

namespace Ferretto.VW.MAS.DataModels
{
    public sealed class Elevator : DataModel, IValidable
    {
        #region Properties

        public IEnumerable<ElevatorAxis> Axes { get; set; }

        /// <summary>
        /// Gets or sets the bay located opposite to the elevator, or null if the elevator is not aligned to a bay.
        /// </summary>
        public BayPosition BayPosition { get; set; }

        /// <summary>
        /// Gets or sets the cell located opposite to the elevator, or null if the elevator is not aligned to a cell.
        /// </summary>
        public Cell Cell { get; set; }

        /// <summary>
        /// Gets or sets the loading unit currently loaded on the elevator.
        /// </summary>
        public LoadingUnit LoadingUnit { get; set; }

        /// <summary>
        /// Gets or sets the id of the loading unit currently loaded on the elevator.
        /// </summary>
        public int? LoadingUnitId { get; set; }

        /// <summary>
        /// Gets or sets the structural properties of the elevator.
        /// </summary>
        public ElevatorStructuralProperties StructuralProperties { get; set; }

        #endregion

        #region Methods

        public void Validate()
        {
            if (this.Axes.Count() != 2)
            {
                throw new System.Exception("L'elevatore deve avere due assi.");
            }

            if (!this.Axes.Any(a => a.Orientation == Orientation.Vertical))
            {
                throw new System.Exception("L'elevatore deve avere un asse verticale.");
            }

            if (!this.Axes.Any(a => a.Orientation == Orientation.Horizontal))
            {
                throw new System.Exception("L'elevatore deve avere un asse orizzontale.");
            }
        }

        #endregion
    }
}
