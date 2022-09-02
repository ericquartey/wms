using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Newtonsoft.Json;

namespace Ferretto.VW.MAS.DataModels
{
    public sealed class BayPosition : DataModel
    {
        #region Properties

        [JsonIgnore]
        public Bay Bay { get; set; }

        public double Height { get; set; }

        public bool IsBlocked { get; set; }

        public bool IsPreferred => !this.Bay.IsDouble           // single bays: always preferred
            || (this.Bay.Carousel != null && this.IsUpper)      // carousel: upper position
            || (this.Bay.Carousel is null && !this.IsUpper);    // double bays external or internal: lower position

        public bool IsUpper => this.Location is LoadingUnitLocation.CarouselBay1Up
            || this.Location is LoadingUnitLocation.CarouselBay2Up
            || this.Location is LoadingUnitLocation.CarouselBay3Up
            || this.Location is LoadingUnitLocation.ExternalBay1Up
            || this.Location is LoadingUnitLocation.ExternalBay2Up
            || this.Location is LoadingUnitLocation.ExternalBay3Up
            || this.Location is LoadingUnitLocation.InternalBay1Up
            || this.Location is LoadingUnitLocation.InternalBay2Up
            || this.Location is LoadingUnitLocation.InternalBay3Up;

        public LoadingUnit LoadingUnit { get; set; }

        public LoadingUnitLocation Location { get; set; }

        public LoadingUnitLocation LocationUpDown =>
            (this.Location is LoadingUnitLocation.CarouselBay1Up
             || this.Location is LoadingUnitLocation.CarouselBay2Up
             || this.Location is LoadingUnitLocation.CarouselBay3Up
             || this.Location is LoadingUnitLocation.ExternalBay1Up
             || this.Location is LoadingUnitLocation.ExternalBay2Up
             || this.Location is LoadingUnitLocation.ExternalBay3Up
             || this.Location is LoadingUnitLocation.InternalBay1Up
             || this.Location is LoadingUnitLocation.InternalBay2Up
             || this.Location is LoadingUnitLocation.InternalBay3Up) ?
            LoadingUnitLocation.Up :
            LoadingUnitLocation.Down;

        public double MaxDoubleHeight { get; set; }

        public double MaxSingleHeight { get; set; }

        public double ProfileOffset { get; set; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"{this.Bay} {this.Location}";
        }

        #endregion
    }
}
