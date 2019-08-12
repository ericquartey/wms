using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Ferretto.WMS.App.Resources;
using Enums = Ferretto.Common.Resources.Enums;

namespace Ferretto.WMS.App.Core.Models
{
    public class MachineLive : BusinessObject
    {
        #region Fields

        private IEnumerable<BayDetails> bays;

        private decimal currentLoadingUnitPosition;

        private int? faultCode;

        private Enums.MachineStatus? status;

        #endregion

        #region Properties

        public IEnumerable<BayDetails> Bays { get => this.bays; set => this.SetProperty(ref this.bays, value); }

        public int? CurrentLoadingUnitId { get; set; }

        [Display(Name = nameof(BusinessObjects.MachineElevatorPosition), ResourceType = typeof(BusinessObjects))]
        public decimal CurrentLoadingUnitPosition { get => this.currentLoadingUnitPosition; set => this.SetProperty(ref this.currentLoadingUnitPosition, value); }

        [Display(Name = nameof(BusinessObjects.MachineFaultCode), ResourceType = typeof(BusinessObjects))]
        public int? FaultCode { get => this.faultCode; set => this.SetProperty(ref this.faultCode, value); }

        public bool IsOnLine => this.Status != Enums.MachineStatus.Offline;

        [Display(Name = nameof(BusinessObjects.MachineStatus), ResourceType = typeof(BusinessObjects))]
        public Enums.MachineStatus? Status
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

        #endregion
    }
}
