﻿using System;
using System.Collections.Generic;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.DataModels
{
    public class Bay : DataModel
    {
        #region Properties

        public Carousel Carousel { get; set; }

        public double ChainOffset { get; set; }

        public Mission CurrentMission { get; set; }

        public MovementParameters EmptyLoadMovement { get; set; }

        public MovementParameters FullLoadMovement { get; set; }

        public Inverter Inverter { get; set; }

        public IoDevice IoDevice { get; set; }

        public bool IsActive { get; set; }

        public bool IsDouble => this.Positions?.Count() == 2;

        public bool IsExternal { get; set; }

        public Laser Laser { get; set; }

        public BayNumber Number { get; set; }

        public BayOperation Operation { get; set; }

        public IEnumerable<BayPosition> Positions { get; set; }

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

            if (this.IoDevice is null)
            {
                throw new Exception($"Baia {this.Number}: il dispositivo di IO non è definito.");
            }
        }

        #endregion
    }
}
