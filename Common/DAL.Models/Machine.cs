using System;

namespace Ferretto.Common.DAL.Models
{
    // Macchina
    public sealed class Machine
    {
        public int Id { get; set; }
        public int AisleId { get; set; }
        public string MachineTypeId { get; set; }
        public string Nickname { get; set; }
        public string RegistrationNumber { get; set; }
        public int? Cradles { get; set; }
        public int? LoadingUnitsPerCradle { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string CustomerName { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerAddress { get; set; }
        public string CustomerCity { get; set; }
        public string CustomerCountry { get; set; }
        public DateTime? BuildDate { get; set; }
        public DateTime? InstallationDate { get; set; }
        public DateTime? TestDate { get; set; }
        public DateTime? LastServiceDate { get; set; }
        public DateTime? NextServiceDate { get; set; }
        public string Image { get; set; }
        public long? TotalMaxWeight { get; set; }
        public long? ActualWeight { get; set; }
        public DateTime? LastPoweOn { get; set; }
        public long? PowerOnTime { get; set; }
        public long? AutomaticTime { get; set; }
        public long? ManualTime { get; set; }
        public long? ErrorTime { get; set; }
        public long? MissionTime { get; set; }
        public long? MovedLoadingUnitsCount { get; set; }
        public long? InputLoadingUnitsCount { get; set; }
        public long? OutputLoadingUnitsCount { get; set; }

        public Aisle Aisle { get; set; }
        public MachineType MachineType { get; set; }
    }
}
