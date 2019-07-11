using System;

namespace Ferretto.VW.App.Services.Models
{
    public class MachineIdentity : Utils.BaseModel
    {
        #region Fields

        private DateTime installationDate;

        private DateTime? lastServiceDate;

        private string modelName;

        private DateTime nextServiceDate;

        private string serialNumber;

        private MachineServiceStatus serviceStatus;

        private int trayCount;

        #endregion

        #region Properties

        public int AreaId { get; set; }

        public int BayId { get; set; }

        public int Id { get; set; }

        public System.DateTime InstallationDate
        {
            get => this.installationDate;
            set => this.SetProperty(ref this.installationDate, value);
        }

        public System.DateTime? LastServiceDate
        {
            get => this.lastServiceDate;
            set => this.SetProperty(ref this.lastServiceDate, value);
        }

        public string ModelName
        {
            get => this.modelName;
            set => this.SetProperty(ref this.modelName, value);
        }

        public System.DateTime NextServiceDate
        {
            get => this.nextServiceDate;
            set => this.SetProperty(ref this.nextServiceDate, value);
        }

        public string SerialNumber
        {
            get => this.serialNumber;
            set => this.SetProperty(ref this.serialNumber, value);
        }

        public MachineServiceStatus ServiceStatus
        {
            get => this.serviceStatus;
            set => this.SetProperty(ref this.serviceStatus, value);
        }

        public int TrayCount
        {
            get => this.trayCount;
            set => this.SetProperty(ref this.trayCount, value);
        }

        #endregion
    }
}
