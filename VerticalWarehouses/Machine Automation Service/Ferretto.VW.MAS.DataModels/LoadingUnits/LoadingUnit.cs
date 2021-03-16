using Ferretto.VW.MAS.DataModels.Enumerations;
using Newtonsoft.Json;

namespace Ferretto.VW.MAS.DataModels
{
    public sealed class LoadingUnit : DataModel, IValidable
    {
        #region Fields

        private double grossWeight;

        private double height;

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

        public bool IsInFullTest { get; set; }

        public bool IsIntoMachine { get; set; }

        public bool IsLaserOffset => this.LaserOffset > 0;

        /// <summary>
        /// distance to subtract to the ZOffset of the LaserPointer for all products in this LU
        /// </summary>
        public double LaserOffset { get; set; }

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
        /// to be used to calculate ABC rotation class
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
        /// Gets the actual net weight of the loading unit's content.
        /// </summary>
        public double NetWeight => (System.Math.Max(0, this.GrossWeight - this.Tare) < 20) ? 0 : this.GrossWeight - this.Tare;

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
