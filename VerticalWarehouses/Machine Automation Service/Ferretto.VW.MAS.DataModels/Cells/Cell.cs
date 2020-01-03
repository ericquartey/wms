using Newtonsoft.Json;

namespace Ferretto.VW.MAS.DataModels
{
    public sealed class Cell : DataModel
    {
        #region Properties

        public BlockLevel BlockLevel { get; set; }

        public bool IsFree { get; set; }

        /// <summary>
        /// The loading unit currently stored in the cell.
        /// </summary>
        [JsonIgnore]
        public LoadingUnit LoadingUnit { get; set; }

        [JsonIgnore]
        public CellPanel Panel { get; set; }

        public int PanelId { get; set; }

        /// <summary>
        /// The absolute vertical position, in millimeters, of the cell in the warehouse.
        /// </summary>
        public double Position { get; set; }

        public int Priority { get; set; }

        public WarehouseSide Side => this.Panel?.Side ?? WarehouseSide.NotSpecified;

        #endregion
    }
}
