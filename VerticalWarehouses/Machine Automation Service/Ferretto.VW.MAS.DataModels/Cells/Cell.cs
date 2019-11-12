using Newtonsoft.Json;

namespace Ferretto.VW.MAS.DataModels
{
    public sealed class Cell : DataModel
    {
        #region Properties

        [JsonIgnore]
        public Elevator Elevator { get; set; }

        [JsonIgnore]
        public LoadingUnit LoadingUnit { get; set; }

        [JsonIgnore]
        public CellPanel Panel { get; set; }

        public int PanelId { get; set; }

        public double Position { get; set; }

        public int Priority { get; set; }

        public WarehouseSide Side => this.Panel?.Side ?? WarehouseSide.NotSpecified;

        public CellStatus Status { get; set; }

        #endregion
    }
}
