using System.ComponentModel.DataAnnotations;
using Ferretto.Common.Resources;

namespace Ferretto.WMS.App.Core.Models
{
    public class BayDetails : BusinessObject
    {
        #region Properties

        [Display(Name = nameof(BusinessObjects.Area), ResourceType = typeof(BusinessObjects))]
        public int AreaId { get; set; }

        [Display(Name = nameof(BusinessObjects.BayType), ResourceType = typeof(BusinessObjects))]
        public string BayTypeDescription { get; set; }

        [Display(Name = nameof(BusinessObjects.BayType), ResourceType = typeof(BusinessObjects))]
        public string BayTypeId { get; set; }

        [Display(Name = nameof(BusinessObjects.BayDescription), ResourceType = typeof(BusinessObjects))]
        public string Description { get; set; }

        public string Image { get; set; }

        [Display(Name = nameof(BusinessObjects.BayIsActive), ResourceType = typeof(BusinessObjects))]
        public bool IsActive { get; set; }

        [Display(Name = nameof(BusinessObjects.BayLoadingUnitInBay), ResourceType = typeof(BusinessObjects))]
        public string LoadingUnitInBayDescription { get; set; }

        public int? LoadingUnitInBayId { get; set; }

        [Display(Name = nameof(BusinessObjects.BayLoadingUnitsBufferSize), ResourceType = typeof(BusinessObjects))]
        public int? LoadingUnitsBufferSize { get; set; }

        [Display(Name = nameof(BusinessObjects.Machine), ResourceType = typeof(BusinessObjects))]
        public int? MachineId { get; set; }

        [Display(Name = nameof(General.Priority), ResourceType = typeof(General))]
        public int Priority { get; set; }

        [Display(Name = nameof(BusinessObjects.BayUserLogged), ResourceType = typeof(BusinessObjects))]
        public int? UserLogged { get; set; }

        #endregion
    }
}
