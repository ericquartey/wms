using System;
using System.Collections.Generic;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.MAS.AutomationService.Models
{
    public class MachineDetails
    {
        #region Constructors

        public MachineDetails()
        {
        }

        #endregion

        #region Properties

        public int AisleId { get; set; }

        public string AisleName { get; set; }

        public int AreaFillRate { get; set; }

        public string AreaName { get; set; }

        public long? AutomaticTime { get; set; }

        public IEnumerable<Bay> Bays { get; set; }

        public DateTimeOffset? BuildDate { get; set; }

        public int? CellsCount { get; set; }

        public int? CompartmentsCount { get; set; }

        public int? CradlesCount { get; set; }

        public string CustomerAddress { get; set; }

        public string CustomerCity { get; set; }

        public string CustomerCode { get; set; }

        public string CustomerCountry { get; set; }

        public string CustomerName { get; set; }

        public long? ErrorTime { get; set; }

        public long? GrossMaxWeight { get; set; }

        public long? GrossWeight { get; set; }

        public int Id { get; set; }

        public string Image { get; set; }

        public long? InputLoadingUnitsCount { get; set; }

        public DateTimeOffset? InstallationDate { get; set; }

        public bool IsOnLine { get; set; }

        public int? ItemsCount { get; set; }

        public DateTimeOffset? LastPowerOn { get; set; }

        public DateTimeOffset? LastServiceDate { get; set; }

        public double? Latitude { get; set; }

        public int? LoadingUnitsCount { get; set; }

        public int? LoadingUnitsPerCradle { get; set; }

        public double? Longitude { get; set; }

        public string MachineTypeDescription { get; set; }

        public string MachineTypeId { get; set; }

        public MaintenanceStatus MaintenanceStatus { get; set; }

        public long? ManualTime { get; set; }

        public int? MissionsCount { get; set; }

        public long? MissionTime { get; set; }

        public string Model { get; set; }

        public long? MovedLoadingUnitsCount { get; set; }

        public long? NetMaxWeight { get; set; }

        public long? NetWeight { get; set; }

        public DateTimeOffset? NextServiceDate { get; set; }

        public string Nickname { get; set; }

        public long? OutputLoadingUnitsCount { get; set; }

        public long? PowerOnTime { get; set; }

        public string RegistrationNumber { get; set; }

        public string ServiceUrl { get; set; }

        public MachineStatus Status { get; set; }

        public DateTimeOffset? TestDate { get; set; }

        public long? TotalMaxWeight { get; set; }

        public int WeightFillRate { get; set; }

        #endregion
    }
}
