﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.VW.MAS.DataModels
{
    public sealed class External : DataModel
    {
        #region Properties 

        public ExternalBayManualParameters AssistedMovements { get; set; }

        public double HomingCreepSpeed { get; set; }

        public double HomingFastSpeed { get; set; }

        public double LastIdealPosition { get; set; }

        public ExternalBayManualParameters ManualMovements { get; set; }

        /// <summary>
        /// Gets or sets the race, in millimeters, from zero sensor to external position.
        /// </summary>
        public double Race { get; set; }

        #endregion
    }
}
