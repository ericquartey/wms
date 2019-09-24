namespace Ferretto.VW.MAS.DataModels
{
    public sealed class MachineIdentity : DataModel
    {
        #region Properties

        public int AreaId { get; set; }

        public decimal Depth { get; set; }

        public System.DateTime InstallationDate { get; set; }

        public bool IsOneTonMachine { get; set; }

        public System.DateTime? LastServiceDate { get; set; }

        public decimal MaxGrossWeight { get; set; }

        public string ModelName { get; set; }

        public System.DateTime? NextServiceDate { get; set; }

        public string SerialNumber { get; set; }

        public MachineServiceStatus ServiceStatus { get; set; } = MachineServiceStatus.Valid;

        public int TrayCount { get; set; }

        public decimal Width { get; set; }

        #endregion
    }
}
