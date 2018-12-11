using System;
using System.Collections.Generic;

namespace Ferretto.Common.DataModels
{
    // Macchina
    public sealed class Machine : IDataModel

    {
        #region Properties

        public long? ActualWeight { get; set; }

        public Aisle Aisle { get; set; }

        public int AisleId { get; set; }

        public long? AutomaticTime { get; set; }

        public IEnumerable<Bay> Bays { get; set; }

        public DateTime? BuildDate { get; set; }

        public int? CradlesCount { get; set; }

        public string CustomerAddress { get; set; }

        public string CustomerCity { get; set; }

        public string CustomerCode { get; set; }

        public string CustomerCountry { get; set; }

        public string CustomerName { get; set; }

        public long? ErrorTime { get; set; }

        public int Id { get; set; }

        public string Image { get; set; }

        public long? InputLoadingUnitsCount { get; set; }

        public DateTime? InstallationDate { get; set; }

        public DateTime? LastPowerOn { get; set; }

        public DateTime? LastServiceDate { get; set; }

        public double? Latitude { get; set; }

        public int? LoadingUnitsPerCradle { get; set; }

        public double? Longitude { get; set; }

        public MachineType MachineType { get; set; }

        public string MachineTypeId { get; set; }

        public long? ManualTime { get; set; }

        public long? MissionTime { get; set; }

        public string Model { get; set; }

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
