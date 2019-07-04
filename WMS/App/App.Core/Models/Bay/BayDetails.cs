using System.ComponentModel.DataAnnotations;
using Ferretto.WMS.App.Resources;

namespace Ferretto.WMS.App.Core.Models
{
    public class BayDetails : BusinessObject
    {
        #region Fields

        private bool isActive;

        private int? loadingUnitInBayId;

        private int? userLogged;

        #endregion

        #region Properties

        [Display(Name = nameof(BusinessObjects.Area), ResourceType = typeof(BusinessObjects))]
        public int AreaId { get; set; }

        [Display(Name = nameof(BusinessObjects.BayType), ResourceType = typeof(BusinessObjects))]
        public string BayTypeDescription { get; set; }

        [Display(Name = nameof(BusinessObjects.BayType), ResourceType = typeof(BusinessObjects))]
        public string BayTypeId { get; set; }

        [Display(Name = nameof(BusinessObjects.Description), ResourceType = typeof(BusinessObjects))]
        public string Description { get; set; }

        public string Image { get; set; }

        [Display(Name = nameof(BusinessObjects.BayIsActive), ResourceType = typeof(BusinessObjects))]
        public bool IsActive { get => this.isActive; set => this.SetProperty(ref this.isActive, value); }

        [Display(Name = nameof(BusinessObjects.BayLoadingUnitInBay), ResourceType = typeof(BusinessObjects))]
        public string LoadingUnitInBayDescription { get; set; }

        [Display(Name = nameof(BusinessObjects.BayLoadingUnitInBay), ResourceType = typeof(BusinessObjects))]
        public int? LoadingUnitInBayId { get => this.loadingUnitInBayId; set => this.SetProperty(ref this.loadingUnitInBayId, value); }

        [Display(Name = nameof(BusinessObjects.BayLoadingUnitsBufferSize), ResourceType = typeof(BusinessObjects))]
        public int? LoadingUnitsBufferSize { get; set; }

        [Display(Name = nameof(BusinessObjects.Machine), ResourceType = typeof(BusinessObjects))]
        public int? MachineId { get; set; }

        [Display(Name = nameof(BusinessObjects.Priority), ResourceType = typeof(BusinessObjects))]
        public int Priority { get; set; }

        [Display(Name = nameof(BusinessObjects.BayUserLogged), ResourceType = typeof(BusinessObjects))]
        public int? UserLogged { get => this.userLogged; set => this.SetProperty(ref this.userLogged, value); }

        #endregion
    }
}
