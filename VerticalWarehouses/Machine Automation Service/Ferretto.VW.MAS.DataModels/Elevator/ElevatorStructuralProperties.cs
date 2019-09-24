namespace Ferretto.VW.MAS.DataModels
{
    public class ElevatorStructuralProperties : DataModel
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
        /// The half-shaft length, in millimeters.
        /// </summary>
        public decimal HalfShaftLength { get; set; }

        /// <summary>
        /// The maximum weight that can be loaded on the cradle.
        /// </summary>
        public decimal MaximumLoadOnBoard { get; set; }

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

        #endregion
    }
}
