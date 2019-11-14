using Newtonsoft.Json;

namespace Ferretto.VW.MAS.DataModels
{
    public sealed class Cell : DataModel
    {
        #region Properties

        [JsonIgnore]
        public Elevator Elevator { get; set; }

        /// <summary>
        /// When set to True, it means that no loading unit can be layed on the cell, but a loading unit content can be placed in front of the cell.
        /// </summary>
        /// <remarks>
        /// It shall be used for e.g. broken cells.
        /// </remarks>
        public bool IsDeactivated { get; set; }

        /// <summary>
        /// When set to True, it means that mo loading unit or its content shall be placed in front of the cell.
        /// </summary>
        /// <remarks>
        /// It shall be used to identify structural features of the warehouse (e.g. bays, beams, etc.).
        /// </remarks>
        public bool IsUnusable { get; set; }

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

        public CellStatus Status { get; set; }

        #endregion
    }
}
