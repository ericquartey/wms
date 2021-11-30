namespace Ferretto.VW.MAS.DataModels
{
    public sealed class Carousel : DataModel
    {
        #region Properties

        public CarouselManualParameters AssistedMovements { get; set; }

        public int BayFindZeroLimit { get; set; }

        /// <summary>
        /// Gets or sets the distance, in millimeters, of the elevator from the carousel.
        /// </summary>
        public double ElevatorDistance { get; set; }

        public double HomingCreepSpeed { get; set; }

        public double HomingFastSpeed { get; set; }

        public double LastIdealPosition { get; set; }

        public CarouselManualParameters ManualMovements { get; set; }

        #endregion
    }
}
