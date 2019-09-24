using System.Collections.Generic;

namespace Ferretto.VW.MAS.DataModels
{
    public sealed class ElevatorAxis : DataModel
    {
        #region Properties

        public MovementParameters EmptyLoad { get; set; }

        public decimal LowerBound { get; set; }

        public MovementParameters MaximumLoad { get; set; }

        public decimal Offset { get; set; }

        public Orientation Orientation { get; set; }

        /// <summary>
        /// The last known position of the elevator's axis.
        /// </summary>
        public decimal Position { get; set; }

        public IEnumerable<MovementProfile> Profiles { get; set; }

        public decimal Resolution { get; set; }

        public decimal UpperBound { get; set; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return this.Orientation.ToString();
        }

        #endregion
    }
}
