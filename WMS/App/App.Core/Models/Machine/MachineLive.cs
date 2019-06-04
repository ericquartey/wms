using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.Common.Resources;

namespace Ferretto.WMS.App.Core.Models
{
    public class MachineLive : BusinessObject
    {
        #region Fields

        private MachineStatus? status;

        #endregion

        #region Properties

        public IEnumerable<BayDetails> Bays { get; set; }

        public int? CurrentLoadingUnitId { get; set; }

        [Display(Name = nameof(BusinessObjects.MachineElevatorPosition), ResourceType = typeof(BusinessObjects))]
        public decimal CurrentLoadingUnitPosition { get; set; }

        [Display(Name = nameof(BusinessObjects.MachineFaultCode), ResourceType = typeof(BusinessObjects))]
        public int? FaultCode { get; set; }

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
