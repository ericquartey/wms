namespace Ferretto.VW.MAS.DataModels
{
    public class ElevatorStructuralProperties : DataModel
    {
        #region Fields

        /// <summary>
        /// The sum of the distances, in millimeters, between the belt pulleys and the top and bottom parts of the machine.
        /// </summary>
        public const double PulleysMargin = 308;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the belt rigidity, in Newton.
        /// </summary>
        public int BeltRigidity { get; set; }

        /// <summary>
        /// Gets or sets the spacing, in millimeters, between the upper and lower attachments of the elevator to the belt.
        /// </summary>
        public double BeltSpacing { get; set; }

        /// <summary>
        /// Gets or sets the half-shaft length, in millimeters.
        /// </summary>
        public double HalfShaftLength { get; set; }

        /// <summary>
        /// Gets or sets the maximum weight, in kilograms, that can be loaded on the cradle.
        /// </summary>
        public double MaximumGrossWeightOnBoard { get; set; }

        /// <summary>
        /// Gets or sets the diameter, in millimeters, of the pulley (belt wheel).
        /// </summary>
        public double PulleyDiameter { get; set; }

        /// <summary>
        /// Gets or sets the shaft diameter, in millimeters.
        /// </summary>
        public double ShaftDiameter { get; set; }

        /// <summary>
        /// Gets or sets the elasticity module of the shaft, in MPa.
        /// </summary>
        public double ShaftElasticity { get; set; }

        #endregion
    }
}
