using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Ferretto.Common.Resources;

namespace Ferretto.WMS.App.Core.Models
{
    public sealed class MachineDetails : BusinessObject
    {
        #region Fields

        private long? grossMaxWeight;

        private long? grossWeight;

        private MachineStatus? status;

        #endregion

        #region Properties

        [Display(Name = nameof(BusinessObjects.MachineActualWeight), ResourceType = typeof(BusinessObjects))]
        public long? ActualWeight { get; set; }

        [Display(Name = nameof(BusinessObjects.MachineAisle), ResourceType = typeof(BusinessObjects))]
        public string AisleName { get; set; }

        [Display(Name = nameof(BusinessObjects.MachineAreaFillRate), ResourceType = typeof(BusinessObjects))]
        public int AreaFillRate { get; set; }

        [Display(Name = nameof(BusinessObjects.MachineArea), ResourceType = typeof(BusinessObjects))]
        public string AreaName { get; set; }

        [Display(Name = nameof(BusinessObjects.MachineAutomaticTime), ResourceType = typeof(BusinessObjects))]
        public long? AutomaticTime { get; set; }

        public IEnumerable<BayDetails> Bays { get; set; }

        [Display(Name = nameof(BusinessObjects.MachineBuildDate), ResourceType = typeof(BusinessObjects))]
        public DateTime? BuildDate { get; set; }

        [Display(Name = nameof(BusinessObjects.MachineCellCount), ResourceType = typeof(BusinessObjects))]
        public int? CellCount { get; set; }

        [Display(Name = nameof(BusinessObjects.MachineCompartmentCount), ResourceType = typeof(BusinessObjects))]
        public int? CompartmentCount { get; set; }

        [Display(Name = nameof(BusinessObjects.MachineCradlesCount), ResourceType = typeof(BusinessObjects))]
        public int? CradlesCount { get; set; }

        public int? CurrentLoadingUnitId { get; set; }

        [Display(Name = nameof(BusinessObjects.MachineElevatorPosition), ResourceType = typeof(BusinessObjects))]
        public decimal CurrentLoadingUnitPosition { get; set; }

        [Display(Name = nameof(BusinessObjects.MachineCustomerAddress), ResourceType = typeof(BusinessObjects))]
        public string CustomerAddress { get; set; }

        [Display(Name = nameof(BusinessObjects.MachineCustomerCity), ResourceType = typeof(BusinessObjects))]
        public string CustomerCity { get; set; }

        [Display(Name = nameof(BusinessObjects.MachineCustomerCode), ResourceType = typeof(BusinessObjects))]
        public string CustomerCode { get; set; }

        [Display(Name = nameof(BusinessObjects.MachineCustomerCountry), ResourceType = typeof(BusinessObjects))]
        public string CustomerCountry { get; set; }

        [Display(Name = nameof(BusinessObjects.MachineCustomerName), ResourceType = typeof(BusinessObjects))]
        public string CustomerName { get; set; }

        [Display(Name = nameof(BusinessObjects.MachineErrorTime), ResourceType = typeof(BusinessObjects))]
        public long? ErrorTime { get; set; }

        [Display(Name = nameof(BusinessObjects.MachineFaultCode), ResourceType = typeof(BusinessObjects))]
        public int? FaultCode { get; set; }

        [Display(Name = nameof(BusinessObjects.MachineGrossMaxWeight), ResourceType = typeof(BusinessObjects))]
        public long? GrossMaxWeight { get => this.grossMaxWeight; set => this.SetProperty(ref this.grossMaxWeight, value); }

        [Display(Name = nameof(BusinessObjects.MachineGrossWeight), ResourceType = typeof(BusinessObjects))]
        public long? GrossWeight { get => this.grossWeight; set => this.SetProperty(ref this.grossWeight, value); }

        [Display(Name = nameof(BusinessObjects.MachineImage), ResourceType = typeof(BusinessObjects))]
        public string Image { get; set; }

        [Display(Name = nameof(BusinessObjects.MachineInputLoadingUnitsCount), ResourceType = typeof(BusinessObjects))]
        public long? InputLoadingUnitsCount { get; set; }

        [Display(Name = nameof(BusinessObjects.MachineInstallationDate), ResourceType = typeof(BusinessObjects))]
        public DateTime? InstallationDate { get; set; }

        public bool IsOnLine => this.Status != MachineStatus.Offline;

        [Display(Name = nameof(BusinessObjects.MachineItemCount), ResourceType = typeof(BusinessObjects))]
        public int? ItemCount { get; set; }

        [Display(Name = nameof(BusinessObjects.MachineItemListCount), ResourceType = typeof(BusinessObjects))]
        public int? ItemListCount { get; set; }

        [Display(Name = nameof(BusinessObjects.MachineLastPowerOn), ResourceType = typeof(BusinessObjects))]
        public DateTime? LastPowerOn { get; set; }

        [Display(Name = nameof(BusinessObjects.MachineLastServiceDate), ResourceType = typeof(BusinessObjects))]
        public DateTime? LastServiceDate { get; set; }

        [Display(Name = nameof(BusinessObjects.MachineLatitude), ResourceType = typeof(BusinessObjects))]
        public double? Latitude { get; set; }

        [Display(Name = nameof(BusinessObjects.MachineLoadingUnitCount), ResourceType = typeof(BusinessObjects))]
        public int? LoadingUnitCount { get; set; }

        [Display(Name = nameof(BusinessObjects.MachineLoadingUnitsPerCradle), ResourceType = typeof(BusinessObjects))]
        public int? LoadingUnitsPerCradle { get; set; }

        [Display(Name = nameof(BusinessObjects.MachineLongitude), ResourceType = typeof(BusinessObjects))]
        public double? Longitude { get; set; }

        [Display(Name = nameof(BusinessObjects.MachineTypeDescription), ResourceType = typeof(BusinessObjects))]
        public string MachineTypeDescription { get; set; }

        [Display(Name = nameof(BusinessObjects.MachineMaintenanceStatus), ResourceType = typeof(BusinessObjects))]
        public MaintenanceStatus? MaintenanceStatus { get; set; }

        [Display(Name = nameof(BusinessObjects.MachineManualTime), ResourceType = typeof(BusinessObjects))]
        public long? ManualTime { get; set; }

        [Display(Name = nameof(BusinessObjects.MachineMissionCount), ResourceType = typeof(BusinessObjects))]
        public int? MissionCount { get; set; }

        [Display(Name = nameof(BusinessObjects.MachineMissionTime), ResourceType = typeof(BusinessObjects))]
        public long? MissionTime { get; set; }

        [Display(Name = nameof(BusinessObjects.MachineModel), ResourceType = typeof(BusinessObjects))]
        public string Model { get; set; }

        [Display(Name = nameof(BusinessObjects.MachineMovedLoadingUnitsCount), ResourceType = typeof(BusinessObjects))]
        public long? MovedLoadingUnitsCount { get; set; }

        [Display(Name = nameof(BusinessObjects.MachineNetMaxWeight), ResourceType = typeof(BusinessObjects))]
        public long? NetMaxWeight { get; set; }

        [Display(Name = nameof(BusinessObjects.MachineNetWeight), ResourceType = typeof(BusinessObjects))]
        public long? NetWeight { get; set; }

        [Display(Name = nameof(BusinessObjects.MachineNextServiceDate), ResourceType = typeof(BusinessObjects))]
        public DateTime? NextServiceDate { get; set; }

        [Display(Name = nameof(BusinessObjects.MachineNickname), ResourceType = typeof(BusinessObjects))]
        public string Nickname { get; set; }

        [Display(Name = nameof(BusinessObjects.MachineOutputLoadingUnitsCount), ResourceType = typeof(BusinessObjects))]
        public long? OutputLoadingUnitsCount { get; set; }

        [Display(Name = nameof(BusinessObjects.MachinePowerOnTime), ResourceType = typeof(BusinessObjects))]
        public long? PowerOnTime { get; set; }

        [Display(Name = nameof(BusinessObjects.MachineRegistrationNumber), ResourceType = typeof(BusinessObjects))]
        public string RegistrationNumber { get; set; }

        [Display(Name = nameof(BusinessObjects.MachineServiceUrl), ResourceType = typeof(BusinessObjects))]
        public string ServiceUrl { get; set; }

        [Display(Name = nameof(BusinessObjects.MachineStatus), ResourceType = typeof(BusinessObjects))]
        public MachineStatus? Status
        {
            get => this.status;
            set
            {
                if (this.SetProperty(ref this.status, value))
                {
                    this.RaisePropertyChanged(nameof(this.IsOnLine));
                }
            }
        }

        [Display(Name = nameof(BusinessObjects.MachineTestDate), ResourceType = typeof(BusinessObjects))]
        public DateTime? TestDate { get; set; }

        [Display(Name = nameof(BusinessObjects.MachineTotalMaxWeight), ResourceType = typeof(BusinessObjects))]
        public long? TotalMaxWeight { get; set; }

        [Display(Name = nameof(BusinessObjects.MachineWeightFillRate), ResourceType = typeof(BusinessObjects))]
        public int WeightFillRate { get; set; }

        #endregion
    }
}
