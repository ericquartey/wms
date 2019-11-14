using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Newtonsoft.Json;

namespace Ferretto.VW.MAS.DataModels
{
    public sealed class BayPosition : DataModel
    {
        #region Properties

        [JsonIgnore]
        public Elevator Elevator { get; set; }

        public double Height { get; set; }

        public bool IsUpper =>
            this.Location is LoadingUnitLocation.CarouselBay1Up
            ||
            this.Location is LoadingUnitLocation.CarouselBay2Up
            ||
            this.Location is LoadingUnitLocation.CarouselBay3Up
            ||
            this.Location is LoadingUnitLocation.ExternalBay1Up
            ||
            this.Location is LoadingUnitLocation.ExternalBay2Up
            ||
            this.Location is LoadingUnitLocation.ExternalBay3Up
            ||
            this.Location is LoadingUnitLocation.InternalBay1Up
            ||
            this.Location is LoadingUnitLocation.InternalBay2Up
            ||
            this.Location is LoadingUnitLocation.InternalBay3Up;

        public LoadingUnit LoadingUnit { get; set; }

        public LoadingUnitLocation Location { get; set; }

        #endregion
    }
}
