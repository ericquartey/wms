using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Ferretto.WMS.App.Resources;

namespace Ferretto.WMS.App.Core.Models
{
    public class MachineLive : BusinessObject
    {
        #region Fields

        private IEnumerable<BayDetails> bays;

        private decimal currentLoadingUnitPosition;

        private int? faultCode;

        private MachineStatus? status;

        private int userLogged;

        #endregion

        #region Properties

        public IEnumerable<BayDetails> Bays { get => this.bays; set => this.SetProperty(ref this.bays, value); }

        public int? CurrentLoadingUnitId { get; set; }

        [Display(Name = nameof(BusinessObjects.MachineElevatorPosition), ResourceType = typeof(BusinessObjects))]
        public decimal CurrentLoadingUnitPosition { get => this.currentLoadingUnitPosition; set => this.SetProperty(ref this.currentLoadingUnitPosition, value); }

        [Display(Name = nameof(BusinessObjects.MachineFaultCode), ResourceType = typeof(BusinessObjects))]
        public int? FaultCode { get => this.faultCode; set => this.SetProperty(ref this.faultCode, value); }

        public long? GrossWeight { get; set; }

        public bool IsOnLine => this.Status != MachineStatus.Offline;

        public long? NetWeight { get; set; }

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

        public int UserLogged { get => this.userLogged; set => this.SetProperty(ref this.userLogged, value); }

        [Display(Name = nameof(BusinessObjects.MachineWeightFillRate), ResourceType = typeof(BusinessObjects))]
        public int WeightFillRate { get; set; }

        #endregion

        #region Methods

        public void CalculateWeightFillRate()
        {
            if (this.GrossWeight.HasValue && this.NetWeight.HasValue)
            {
                this.WeightFillRate = (int)(this.GrossWeight.Value / this.NetWeight.Value);
                this.RaisePropertyChanged(nameof(this.WeightFillRate));
                return;
            }

            this.WeightFillRate = 0;
        }

        #endregion
    }
}
