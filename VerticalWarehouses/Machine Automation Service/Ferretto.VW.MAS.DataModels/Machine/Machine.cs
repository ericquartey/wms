using System.Collections.Generic;

namespace Ferretto.VW.MAS.DataModels
{
    public sealed class Machine : DataModel
    {
        #region Properties

        /// <summary>
        /// Gets or sets the bays of the machine.
        /// </summary>
        public IEnumerable<Bay> Bays { get; set; }

        /// <summary>
        /// Gets or sets the machine's elevator.
        /// </summary>
        public Elevator Elevator { get; set; }

        /// <summary>
        /// Gets or sets the machine height, in millimeters.
        /// </summary>
        public double Height { get; set; }

        public double LoadUnitMaxHeight { get; set; }

        public double LoadUnitMaxNetWeight { get; set; }

        public double LoadUnitTare { get; set; }

        /// <summary>
        /// Gets or sets the maximum gross weight that the machine can have.
        /// </summary>
        public double MaxGrossWeight { get; set; }

        /// <summary>
        /// Gets or sets the machine's model name.
        /// </summary>
        public string ModelName { get; set; }

        /// <summary>
        /// Gets or sets the panels on which the cells are mounted.
        /// </summary>
        public IEnumerable<CellPanel> Panels { get; set; }

        /// <summary>
        /// Gets or sets the machine's serial number.
        /// </summary>
        public string SerialNumber { get; set; }

        #endregion
    }
}
