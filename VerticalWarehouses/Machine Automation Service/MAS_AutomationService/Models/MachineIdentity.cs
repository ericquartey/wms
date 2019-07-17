namespace Ferretto.VW.MAS.AutomationService.Models
{
    public class MachineIdentity : BaseModel
    {
        #region Properties

        public int AreaId { get; set; }

        public int BayId { get; set; }

        public System.DateTime InstallationDate { get; set; }

        public System.DateTime? LastServiceDate { get; set; }

        public string ModelName { get; set; }

        public System.DateTime NextServiceDate { get; set; }

        public string SerialNumber { get; set; }

        public MachineServiceStatus ServiceStatus { get; set; } = MachineServiceStatus.Valid;

        public int TrayCount { get; set; }

        #endregion
    }
}
