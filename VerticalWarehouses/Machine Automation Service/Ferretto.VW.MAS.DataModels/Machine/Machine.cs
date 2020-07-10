﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Ferretto.VW.MAS.DataModels
{
    public sealed class Machine : DataModel, IValidable
    {
        #region Properties

        /// <summary>
        /// Gets or sets the bays of the machine.
        /// </summary>
        public IEnumerable<Bay> Bays { get; set; }

        /// <summary>
        /// Gets or sets the machine's elevator.
        /// </summary>
        public Elevator Elevator { get; set; }

        public int ExpireCountPrecent { get; set; } = 10;

        public int ExpireDays { get; set; } = 14;

        /// <summary>
        /// Gets or sets the machine height, in millimeters.
        /// </summary>
        public double Height { get; set; }

        public int HorizontalCyclesToCalibrate { get; set; } = 10;

        public int HorizontalPositionToCalibrate { get; set; } = 3000;

        /// <summary>
        /// Load Unit maximum height, in millimeters
        /// </summary>
        public double LoadUnitMaxHeight { get; set; }

        public double LoadUnitMaxNetWeight { get; set; }

        /// <summary>
        /// Load Unit minimum height, in millimeters
        /// </summary>
        public double LoadUnitMinHeight { get; set; }

        /// <summary>
        /// Gets or sets the weight of an empty load unit.
        /// </summary>
        public double LoadUnitTare { get; set; }

        /// <summary>
        /// Gets or sets the percent of net weight to be considered very heavy in FindEmptyCell.
        /// </summary>
        public double LoadUnitVeryHeavyPercent { get; set; }

        /// <summary>
        /// Gets or sets the maximum gross weight that the machine can have.
        /// </summary>
        public double MaxGrossWeight { get; set; }

        /// <summary>
        /// Gets or sets the machine's model name.
        /// </summary>
        public string ModelName { get; set; }

        /// <summary>
        /// Gets or sets the panels on which the cells are mounted.
        /// </summary>
        public IEnumerable<CellPanel> Panels { get; set; }

        /// <summary>
        /// Gets or sets the machine's serial number.
        /// </summary>
        public string SerialNumber { get; set; }

        public bool Simulation { get; set; }

        #endregion

        #region Methods

        public void Validate()
        {
            if (this.Bays.Select(b => b.Number).Distinct().Count() != this.Bays.Count())
            {
                throw new Exception("C'è più di una baia definita con lo stesso numero.");
            }

            this.Bays.ForEach(b => b.Validate());

            this.Panels.ForEach(p => p.Validate());
            this.Elevator.Validate();
        }

        #endregion
    }
}
