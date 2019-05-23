namespace Ferretto.WMS.Data.Core.Models
{
    public class Bay : BaseModel<int>
    {
        #region Fields

        private int? loadingUnitsBufferSize;

        #endregion

        #region Properties

        public int AreaId { get; set; }

        public string AreaName { get; set; }

        public string BayTypeDescription { get; set; }

        public string BayTypeId { get; set; }

        public string Description { get; set; }

        public bool IsActive { get; set; }

        public int? LoadingUnitsBufferSize
        {
            get => this.loadingUnitsBufferSize;
            set => this.loadingUnitsBufferSize = CheckIfStrictlyPositive(value);
        }

        public int? MachineId { get; set; }

        public string MachineNickname { get; set; }

        #endregion
    }
}
