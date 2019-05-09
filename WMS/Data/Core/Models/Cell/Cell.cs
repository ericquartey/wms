namespace Ferretto.WMS.Data.Core.Models
{
    public class Cell : BaseModel<int>
    {
        #region Properties

        public string AbcClassDescription { get; set; }

        public int AbcClassId { get; set; }

        public int AisleId { get; set; }

        public string AisleName { get; set; }

        public string AreaName { get; set; }

        public int? Column { get; set; }

        public int? Floor { get; set; }

        public int LoadingUnitsCount { get; set; }

        public string LoadingUnitsDescription { get; set; }

        public int? Number { get; set; }

        public int Priority { get; set; }

        public Side Side { get; set; }

        public string Status { get; set; }

        public string CellTypeDescription { get; set; }

        public double? XCoordinate { get; set; }

        public double? YCoordinate { get; set; }

        public double? ZCoordinate { get; set; }

        #endregion

    }
}
