using System;
using System.Collections.Generic;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.DataModels
{
    public class Bay : DataModel
    {
        #region Properties

        public BayAccessories Accessories { get; set; }

        public Carousel Carousel { get; set; }

        public double ChainOffset { get; set; }

        public Mission CurrentMission { get; set; }

        public int CyclesToCalibrate { get; set; }

        public MovementParameters EmptyLoadMovement { get; set; }

        public External External { get; set; }

        public MovementParameters FullLoadMovement { get; set; }

        public bool Inventory { get; set; }

        public Inverter Inverter { get; set; }

        public IoDevice IoDevice { get; set; }

        public bool IsActive { get; set; }

        public bool IsAdjustByWeight { get; set; }

        public bool IsCheckIntrusion { get; set; }

        public bool IsDouble => this.Positions?.Count() == 2;

        public bool IsExternal { get; set; }

        public bool IsFastDepositToBay { get; set; }

        public int LastCalibrationCycles { get; set; }

        public BayNumber Number { get; set; }

        public BayOperation Operation { get; set; }

        public bool Pick { get; set; }

        public IEnumerable<BayPosition> Positions { get; set; }

        public bool Put { get; set; }

        public double Resolution { get; set; }

        public Shutter Shutter { get; set; }

        public WarehouseSide Side { get; set; }

        public BayStatus Status
        {
            get
            {
                if (this.IsActive)
                {
                    return this.CurrentMission is null ? BayStatus.Idle : BayStatus.Busy;
                }

                return BayStatus.Disconnected;
            }
        }

        public int TotalCycles { get; set; }

        public bool View { get; set; }

        public bool IsTelescopic { get; set; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return this.Number.ToString();
        }

        internal void Validate()
        {
            var positionsCount = this.Positions.Count();
            switch (this.Positions.Count())
            {
                case 0:
                    throw new Exception($"Baia {this.Number}: deve avere almeno una posizione.");
                case 1:
                    if (!this.Positions.Single().IsUpper)
                    {
                        throw new Exception($"Baia {this.Number}: ha una sola posizione, che dovrebbe essere marcata come 'upper'.");
                    }
                    break;

                case 2:
                    var upperPosition = this.Positions.Single(p => p.Height == this.Positions.Max(pos => pos.Height));
                    var lowerPosition = this.Positions.Single(p => p.Height == this.Positions.Min(pos => pos.Height));

                    if (!upperPosition.IsUpper)
                    {
                        throw new Exception($"Baia {this.Number}: la posizione con quota superiore non è marcata come 'upper'.");
                    }

                    if (lowerPosition.IsUpper)
                    {
                        throw new Exception($"Baia {this.Number}: la posizione con quota inferiore non è marcata come 'lower'.");
                    }

                    if (upperPosition.Height == lowerPosition.Height)
                    {
                        throw new Exception($"Baia {this.Number}: le due posizioni di baia non possono avere la stessa quota.");
                    }

                    break;

                default:
                    throw new Exception($"Baia {this.Number}: non può avere più di due posizioni.");
            }

            if (this.Carousel != null && positionsCount != 2)
            {
                throw new Exception($"Baia {this.Number}: la giostra è definita, ma la baia non ha due posizioni.");
            }

            if (this.External != null && positionsCount != 1 && !this.IsDouble)
            {
                throw new Exception($"Baia {this.Number}: la baia esterna singola è definita, ma la baia deve avere una posizione.");
            }

            if (this.External != null && positionsCount != 2 && this.IsDouble)
            {
                throw new Exception($"Baia {this.Number}: la baia esterna doppia è definita, ma la baia non ha due posizioni.");
            }

            if (this.IoDevice is null)
            {
                throw new Exception($"Baia {this.Number}: il dispositivo di IO non è definito.");
            }
        }

        #endregion
    }
}
