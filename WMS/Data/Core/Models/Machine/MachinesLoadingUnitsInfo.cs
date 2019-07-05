namespace Ferretto.WMS.Data.Core.Models
{
    public class MachinesLoadingUnitsInfo : BaseModel<int>
    {
        #region Properties

        public int CompartmentsCount { get; set; }

        public long EmptyWeight { get; set; }

        public int LoadingUnitsCount { get; set; }

        public int MissionsCount { get; set; }

        public int Weight { get; set; }

        #endregion
    }
}
