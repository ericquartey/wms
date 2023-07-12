using System;
using Ferretto.VW.MAS.DataModels.Enumerations;
using Newtonsoft.Json;

namespace Ferretto.VW.MAS.DataModels
{
    public sealed class LoadingUnit : DataModel, IValidable
    {
        #region Fields

        private double grossWeight;

        private double height;

        private double laserOffset;

        private double maxNetWeight;

        private int missionsCount;

        private double tare;

        #endregion

        #region Properties

        public Cell Cell { get; set; }

        public int? CellId { get; set; }

        public string Code => this.Id.ToString("000");

        public string Description { get; set; }

        /// <summary>
        /// the load unit can be stored only in this cell
        /// </summary>
        public int? FixedCell { get; set; }

        public int? StartingCellId { get; set; }

        /// <summary>
        /// the load unit cannot be higher than this, and when it is lower it will be forced to this height
        /// </summary>
        public double? FixedHeight { get; set; }

        /// <summary>
        /// Gets or sets the actual gross weight of the loading unit, in kilograms.
        /// </summary>
        public double GrossWeight
        {
            get => this.grossWeight;
            set
            {
                // TODO please move this check in calling process
                //if (value > this.tare)
                {
                    this.grossWeight = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the height of the loading unit, in millimeters.
        /// </summary>
        public double Height
        {
            get => this.height;
            set
            {
                if (value < 0)
                {
                    throw new System.ArgumentOutOfRangeException(nameof(value));
                }

                this.height = value;
            }
        }

        /// <summary>
        /// The load unit has fixed cell: it will always be stored in the same cell
        /// </summary>
        public bool IsCellFixed { get; set; }

        /// <summary>
        /// this property is always equal to IsCellFixed
        /// </summary>
        public bool IsHeightFixed { get; set; }

        public bool IsInFullTest { get; set; }

        [Obsolete("Use the IsIntoMachineOK field instead.")]
        [JsonIgnore]
        public bool IsIntoMachine { get; set; }

        public bool IsIntoMachineOK => this.Status == LoadingUnitStatus.InLocation;

        public bool IsIntoMachineOrBlocked => this.Status == LoadingUnitStatus.InLocation || this.Status == LoadingUnitStatus.Blocked;

        public bool IsLaserOffset => this.LaserOffset > 0;

        public bool IsRotationClassDifferent => !string.IsNullOrEmpty(this.RotationClass)
                            && this.Cell != null
                            && !string.IsNullOrEmpty(this.Cell.RotationClass)
                            && this.RotationClass != this.Cell.RotationClass;

        /// <summary>
        /// if enabled the rotation class is not automatically calculated - only the user can change it
        /// </summary>
        public bool IsRotationClassFixed { get; set; }

        /// <summary>
        /// distance to subtract to the ZOffset of the LaserPointer for all products in this LU
        /// </summary>
        public double LaserOffset
        {
            get => this.laserOffset;
            set
            {
                if (value < 0)
                {
                    throw new System.ArgumentOutOfRangeException(nameof(value));
                }

                this.laserOffset = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum weight,in kilograms, that the loading unit can carry.
        /// </summary>
        public double MaxNetWeight
        {
            get => this.maxNetWeight;
            set
            {
                if (value <= 0)
                {
                    throw new System.ArgumentOutOfRangeException(nameof(value));
                }

                this.maxNetWeight = value;
            }
        }

        /// <summary>
        /// used for statistic purpose
        /// </summary>
        public int MissionsCount
        {
            get => this.missionsCount;
            set
            {
                if (value < 0)
                {
                    throw new System.ArgumentOutOfRangeException(nameof(value));
                }

                this.missionsCount = value;
            }
        }

        /// <summary>
        /// used to calculate ABC rotation class
        /// </summary>
        public int MissionsCountRotation { get; set; }

        /// <summary>
        /// Gets the actual net weight of the loading unit's content.
        /// </summary>
        public double NetWeight => (System.Math.Max(0, this.GrossWeight - this.Tare) < 20) ? 0 : this.GrossWeight - this.Tare;

        /// <summary>
        /// can be A, B or C
        /// </summary>
        public string RotationClass { get; set; }

        public LoadingUnitStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the weight, in kilograms, of the empty loading unit.
        /// </summary>
        public double Tare
        {
            get => this.tare;
            set
            {
                if (value <= 0)
                {
                    throw new System.ArgumentOutOfRangeException(nameof(value));
                }

                this.tare = value;
            }
        }

        #endregion

        #region Methods

        public bool IsVeryHeavy(double veryHeavyPercent)
        {
            if (veryHeavyPercent <= 0)
            {
                return false;
            }
            return this.NetWeight > this.MaxNetWeight * veryHeavyPercent / 100;
        }

        public void Validate()
        {
            if (this.NetWeight < 0)
            {
                throw new System.Exception($"Cassetto {this.Id}: il peso netto non può essere negativo.");
            }
        }

        #endregion
    }
}
