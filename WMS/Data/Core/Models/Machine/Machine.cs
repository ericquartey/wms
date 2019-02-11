using System;

namespace Ferretto.WMS.Data.Core.Models
{
    public class Machine : BaseModel<int>
    {
        #region Properties

        public bool IsOnLine
        {
            get => this.Status != MachineStatus.Offline;
        }

        public long? ActualWeight { get; set; }

        public int AisleId { get; set; }

        public string AisleName { get; set; }

        public string AreaName { get; set; }

        public long? AutomaticTime { get; set; }

        public DateTime? BuildDate { get; set; }

        public int? CradlesCount { get; set; }

        public string CustomerAddress { get; set; }

        public string CustomerCity { get; set; }

        public string CustomerCode { get; set; }

        public string CustomerCountry { get; set; }

        public string CustomerName { get; set; }

        public long? ErrorTime { get; set; }

        public int FillRate { get; set; }

        public string Image { get; set; }

        public long? InputLoadingUnitsCount { get; set; }

        public DateTime? InstallationDate { get; set; }

        public DateTime? LastPowerOn { get; set; }

        public DateTime? LastServiceDate { get; set; }

        public double? Latitude { get; set; }

        public int? LoadingUnitsPerCradle { get; set; }

        public double? Longitude { get; set; }

        public string MachineTypeDescription { get; set; }

        public string MachineTypeId { get; set; }

        public MaintenanceStatus MaintenanceStatus { get; set; }

        public long? ManualTime { get; set; }

        public long? MissionTime { get; set; }

        public string Model { get; set; }

        public long? MovedLoadingUnitsCount { get; set; }

        public DateTime? NextServiceDate { get; set; }

        public string Nickname { get; set; }

        public long? OutputLoadingUnitsCount { get; set; }

        public long? PowerOnTime { get; set; }

        public string RegistrationNumber { get; set; }

        public MachineStatus Status { get; set; }

        public DateTime? TestDate { get; set; }

        public long? TotalMaxWeight { get; set; }

        #endregion
    }
}
