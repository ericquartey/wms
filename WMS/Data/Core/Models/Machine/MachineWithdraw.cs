namespace Ferretto.WMS.Data.Core.Models
{
    public class MachineWithdraw : BaseModel<int>
    {
        #region Properties

        public double? AvailableQuantityItem { get; set; }

        public string Nickname { get; set; }

        #endregion
    }
}
