namespace Ferretto.VW.MAS.DataModels
{
    public class ElevatorStructuralProperties
    {
        #region Properties

        /// <summary>
        /// The belt rigidity.
        /// </summary>
        public decimal BeltRigidity { get; set; }

        /// <summary>
        /// The spacing between the upper and lower attachments of the elevator to the belt.
        /// </summary>
        public decimal BeltSpacing { get; set; }

        /// <summary>
        /// The machine height, in millimeters.
        /// </summary>
        public decimal Height { get; set; }

        /// <summary>
        /// The diameter, in millimeters, of the pulley (belt wheel).
        /// </summary>
        public decimal PulleyDiameter { get; set; }

        /// <summary>
        /// The shaft diameter, in millimeters.
        /// </summary>
        public decimal ShaftDiameter { get; set; }

        /// <summary>
        /// The shaft elasticity.
        /// </summary>
        public decimal ShaftElasticity { get; set; }

        /// <summary>
        /// The shaft length, in millimeters.
        /// </summary>
        public decimal ShaftLength { get; set; }

        #endregion
    }
}
