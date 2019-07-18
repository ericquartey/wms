using System.Collections.Generic;

namespace Ferretto.VW.MAS.DataModels
{
    public class LoadingUnit
    {
        #region Properties

        public int Id { get; set; }

        public string Code { get; set; }

        public Cell Cell { get; set; }

        public int? CellId { get; set; }

        public string Description { get; set; }

        public decimal Height { get; set; }

        public int LoadingUnitId { get; set; }

        public decimal MaxNetWeight { get; set; }

        public decimal Tare { get; set; }

        public LoadingUnitStatus Status { get; set; }

        public decimal GrossWeight { get; set; }

        #endregion
    }
}
