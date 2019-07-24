using Ferretto.VW.MAS.DataModels.Enumerations;

namespace Ferretto.VW.MAS.DataModels.LoadingUnit
{
    public class LoadingUnit
    {
        #region Properties

        public Cell.Cell Cell { get; set; }

        public int? CellId { get; set; }

        public string Code { get; set; }

        public string Description { get; set; }

        public decimal GrossWeight { get; set; }

        public decimal Height { get; set; }

        public int Id { get; set; }

        public decimal MaxNetWeight { get; set; }

        public int MissionsCount { get; set; }

        public LoadingUnitStatus Status { get; set; }

        public decimal Tare { get; set; }

        #endregion
    }
}
