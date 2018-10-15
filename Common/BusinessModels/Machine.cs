using System;

namespace Ferretto.Common.BusinessModels
{
    public sealed class Machine : BusinessObject<int>
    {
        #region Properties

        public long? ActualWeight { get; set; }
        public string AisleDescription { get; set; }

        public long? AutomaticTime { get; set; }
        public DateTime? BuildDate { get; set; }
        public int? Cradles { get; set; }
        public string CustomerAddress { get; set; }
        public string CustomerCity { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerCountry { get; set; }
        public string CustomerName { get; set; }
        public long? ErrorTime { get; set; }
        public string Image { get; set; }
        public long? InputLoadingUnitsCount { get; set; }
        public DateTime? InstallationDate { get; set; }
        public DateTime? LastPoweOn { get; set; }
        public DateTime? LastServiceDate { get; set; }
        public double? Latitude { get; set; }
        public int? LoadingUnitsPerCradle { get; set; }
        public double? Longitude { get; set; }
        public string MachineType { get; set; }
        public long? ManualTime { get; set; }
        public long? MissionTime { get; set; }
        public long? MovedLoadingUnitsCount { get; set; }
        public DateTime? NextServiceDate { get; set; }
        public string Nickname { get; set; }
        public long? OutputLoadingUnitsCount { get; set; }
        public long? PowerOnTime { get; set; }
        public string RegistrationNumber { get; set; }
        public DateTime? TestDate { get; set; }
        public long? TotalMaxWeight { get; set; }

        #endregion Properties
    }
}
