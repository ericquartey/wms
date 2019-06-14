using System.ComponentModel.DataAnnotations;
using Ferretto.Common.Resources;
using Ferretto.Common.Utils;

namespace Ferretto.WMS.Data.Core.Models
{
    [Resource(nameof(Bay))]
    internal class BayDetails : BaseModel<int>
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

        [Display(Name = nameof(BusinessObjects.BayIsActive), ResourceType = typeof(BusinessObjects))]
        public bool IsActive { get; set; }

        [Display(Name = nameof(BusinessObjects.BayLoadingUnitInBay), ResourceType = typeof(BusinessObjects))]
        public string LoadingUnitInBayDescription { get; set; }

        [Display(Name = nameof(BusinessObjects.BayLoadingUnitInBay), ResourceType = typeof(BusinessObjects))]
        public int LoadingUnitInBayId { get; set; }

        [Display(Name = nameof(BusinessObjects.BayLoadingUnitsBufferSize), ResourceType = typeof(BusinessObjects))]
        public int? LoadingUnitsBufferSize { get; set; }

        [Display(Name = nameof(BusinessObjects.Machine), ResourceType = typeof(BusinessObjects))]
        public int? MachineId { get; set; }

        [Display(Name = nameof(BusinessObjects.BayUserLogged), ResourceType = typeof(BusinessObjects))]
        public string UserLogged { get; set; }

        #endregion
    }
}
