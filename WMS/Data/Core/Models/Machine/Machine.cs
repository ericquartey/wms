using System;

namespace Ferretto.WMS.Data.Core.Models
{
    public class Machine : BaseModel<int>, IMachineLiveData
    {
        #region Properties

        [Positive]
        public long? ActualWeight { get; set; }

        public int AisleId { get; set; }

        public string AisleName { get; set; }

        public string AreaName { get; set; }

        [PositiveOrZero]
        public long? AutomaticTime { get; set; }

        public DateTime? BuildDate { get; set; }

        [PositiveOrZero]
        public int? CradlesCount { get; set; }

        public string CustomerAddress { get; set; }

        public string CustomerCity { get; set; }

        public string CustomerCode { get; set; }

        public string CustomerCountry { get; set; }

        public string CustomerName { get; set; }

        [PositiveOrZero]
        public long? ErrorTime { get; set; }

        [PositiveOrZero]
        public int FillRate { get; set; }

        [PositiveOrZero]
        public long? GrossMaxWeight { get; set; }

        [PositiveOrZero]
        public long? GrossWeight { get; set; }

        public string Image { get; set; }

        [PositiveOrZero]
        public long? InputLoadingUnitsCount { get; set; }

        public DateTime? InstallationDate { get; set; }

        public bool IsOnLine => this.Status != MachineStatus.Offline;

        public DateTime? LastPowerOn { get; set; }

        public DateTime? LastServiceDate { get; set; }

        public double? Latitude { get; set; }

        [PositiveOrZero]
        public int? LoadingUnitsPerCradle { get; set; }

        public double? Longitude { get; set; }

        public string MachineTypeDescription { get; set; }

        public string MachineTypeId { get; set; }

        public MaintenanceStatus MaintenanceStatus { get; set; }

        [PositiveOrZero]
        public long? ManualTime { get; set; }

        [PositiveOrZero]
        public long? MissionTime { get; set; }

        public string Model { get; set; }

        [PositiveOrZero]
        public long? MovedLoadingUnitsCount { get; set; }

        [PositiveOrZero]
        public long? NetMaxWeight { get; set; }

        [PositiveOrZero]
        public long? NetWeight { get; set; }

        public DateTime? NextServiceDate { get; set; }

        public string Nickname { get; set; }

        [PositiveOrZero]
        public long? OutputLoadingUnitsCount { get; set; }

        public long? PowerOnTime { get; set; }

        public string RegistrationNumber { get; set; }

        public MachineStatus Status { get; set; }

        public DateTime? TestDate { get; set; }

        [PositiveOrZero]
        public long? TotalMaxWeight { get; set; }

        #endregion
    }
}
