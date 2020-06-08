using System;

namespace Ferretto.VW.MAS.DataModels
{
    public class ElevatorStructuralProperties : DataModel
    {
        #region Fields

        public const double PulleyCircleMeters = 0.504;

        /// <summary>
        /// The sum of the distances, in millimeters, between the belt pulleys and the top and bottom parts of the machine.
        /// </summary>
        public const double PulleysMargin = 308;

        private int beltRigidity;

        private double beltSpacing;

        private double elevatorWeight;

        private double halfShaftLength;

        private double pulleyDiameter;

        private double shaftDiameter;

        private double shaftElasticity;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the belt rigidity, in Newton.
        /// </summary>
        public int BeltRigidity
        {
            get => this.beltRigidity;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                this.beltRigidity = value;
            }
        }

        /// <summary>
        /// Gets or sets the spacing, in millimeters, between the upper and lower attachments of the elevator to the belt.
        /// </summary>
        public double BeltSpacing
        {
            get => this.beltSpacing;
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                this.beltSpacing = value;
            }
        }

        /// <summary>
        /// Gets or sets the weight of the elevator structure in kg.
        /// </summary>
        public double ElevatorWeight
        {
            get => this.elevatorWeight;
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                this.elevatorWeight = value;
            }
        }

        /// <summary>
        /// Gets or sets the half-shaft length, in millimeters.
        /// </summary>
        public double HalfShaftLength
        {
            get => this.halfShaftLength;
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                this.halfShaftLength = value;
            }
        }

        /// <summary>
        /// Gets or sets the diameter, in millimeters, of the pulley (belt wheel).
        /// </summary>
        public double PulleyDiameter
        {
            get => this.pulleyDiameter;
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                this.pulleyDiameter = value;
            }
        }

        public int SecondTermMultiplier { get; set; }

        /// <summary>
        /// Gets or sets the shaft diameter, in millimeters.
        /// </summary>
        public double ShaftDiameter
        {
            get => this.shaftDiameter;
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                this.shaftDiameter = value;
            }
        }

        /// <summary>
        /// Gets or sets the elasticity module of the shaft, in MPa.
        /// </summary>
        public double ShaftElasticity
        {
            get => this.shaftElasticity;
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                this.shaftElasticity = value;
            }
        }

        #endregion
    }
}
