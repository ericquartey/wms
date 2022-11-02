using Newtonsoft.Json;

namespace Ferretto.VW.MAS.DataModels
{
    public sealed class Cell : DataModel, IValidable
    {
        #region Properties

        public BlockLevel BlockLevel { get; set; }

        public string Description { get; set; }

        public bool IsAvailable => this.IsFree && (this.BlockLevel == BlockLevel.None || this.BlockLevel == BlockLevel.SpaceOnly);

        public bool IsFree { get; set; }

        public bool IsNotAvailable => this.BlockLevel == BlockLevel.Blocked
            || this.BlockLevel == BlockLevel.UnderWeight
            || this.BlockLevel == BlockLevel.Reserved
            || this.BlockLevel == BlockLevel.Undefined;

        public bool IsNotAvailableFixed => this.BlockLevel == BlockLevel.Blocked
            || this.BlockLevel == BlockLevel.UnderWeight
            || this.BlockLevel == BlockLevel.Undefined;

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

        /// <summary>
        /// can be A, B or C
        /// </summary>
        public string RotationClass { get; set; }

        public WarehouseSide Side => this.Panel?.Side ?? WarehouseSide.NotSpecified;

        public SupportType SupportType => this.Support();

        #endregion

        #region Methods

        /// <summary>
        /// In some cells the LU support is inserted as a fork and the elevator top has the same level of the cell
        /// In other cells the LU support lays above the cell and the elevator top is 25mm higher than the cell
        ///
        /// front fork cells are: 1,  5,  9, 13, 17 ...
        /// back fork cells are: 14, 18, 22, 26, 30 ...
        /// </summary>
        /// <returns></returns>
        public SupportType Support()
        {
            switch (this.Side)
            {
                case WarehouseSide.Front:
                    return ((this.Id - 1) % 4 == 0) ? SupportType.Insert : SupportType.Above;

                case WarehouseSide.Back:
                    return ((this.Id - 2) % 4 == 0) ? SupportType.Insert : SupportType.Above;

                default:
                    return SupportType.Undefined;
            }
        }

        public void Validate()
        {
            // do nothing
        }

        #endregion
    }
}
