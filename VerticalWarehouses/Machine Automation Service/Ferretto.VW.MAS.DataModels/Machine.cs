using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.VW.MAS.DataModels
{
    public sealed class Machine : DataModel
    {
        #region Properties

        /// <summary>
        /// The bays of the machine.
        /// </summary>
        public IEnumerable<Bay> Bays { get; set; }

        /// <summary>
        /// The machine's elevator.
        /// </summary>
        public Elevator Elevator { get; set; }

        /// <summary>
        /// The machine height, in millimeters.
        /// </summary>
        public decimal Height { get; set; }

        /// <summary>
        /// The maximum gross weight that the machine can have.
        /// </summary>
        public decimal MaxGrossWeight { get; set; }

        /// <summary>
        /// The machine's model name.
        /// </summary>
        public string ModelName { get; set; }

        /// <summary>
        /// The machine's serial number.
        /// </summary>
        public string SerialNumber { get; set; }

        #endregion
    }
}
