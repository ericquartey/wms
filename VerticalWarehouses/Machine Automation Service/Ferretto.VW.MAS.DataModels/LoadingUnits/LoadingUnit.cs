using Ferretto.VW.MAS.DataModels.Enumerations;

namespace Ferretto.VW.MAS.DataModels
{
    public sealed class LoadingUnit : DataModel
    {
        #region Properties

        public Cell Cell { get; set; }

        public int? CellId { get; set; }

        public string Code { get; set; }

        public string Description { get; set; }

        public double GrossWeight { get; set; }

        public double Height { get; set; }

        public bool IsIntoMachine { get; set; }

        public double MaxNetWeight { get; set; }

        public int MissionsCount { get; set; }

        public LoadingUnitStatus Status { get; set; }

        public double Tare { get; set; }

        #endregion
    }
}
