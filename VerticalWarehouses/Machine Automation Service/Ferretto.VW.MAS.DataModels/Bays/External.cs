namespace Ferretto.VW.MAS.DataModels
{
    public sealed class External : DataModel
    {
        #region Properties

        public ExternalBayManualParameters AssistedMovements { get; set; }

        /// <summary>
        /// Gets or sets the extra race, in millimeters, used in the insertion/extraction drawer movements.
        /// </summary>
        public double ExtraRace { get; set; }

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
