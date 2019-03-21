using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Ferretto.Common.Resources;

namespace Ferretto.WMS.App.Core.Models
{
    public sealed class Bay : BusinessObject
    {
        #region Properties

        public IEnumerable<Enumeration> AreaChoices { get; set; }

        [Display(Name = nameof(BusinessObjects.Area), ResourceType = typeof(BusinessObjects))]
        public int AreaId { get; set; }

        [Display(Name = nameof(BusinessObjects.Area), ResourceType = typeof(BusinessObjects))]
        public string AreaName { get; set; }

        public IEnumerable<EnumerationString> BayTypeChoices { get; set; }

        [Display(Name = nameof(BusinessObjects.BayType), ResourceType = typeof(BusinessObjects))]
        public string BayTypeDescription { get; set; }

        [Display(Name = nameof(BusinessObjects.BayType), ResourceType = typeof(BusinessObjects))]
        public string BayTypeId { get; set; }

        [Display(Name = nameof(BusinessObjects.BayDescription), ResourceType = typeof(BusinessObjects))]
        public string Description { get; set; }

        [Display(Name = nameof(BusinessObjects.BayLoadingUnitsBufferSize), ResourceType = typeof(BusinessObjects))]
        public int? LoadingUnitsBufferSize { get; set; }

        public IEnumerable<Enumeration> MachineChoices { get; set; }

        [Display(Name = nameof(BusinessObjects.Machine), ResourceType = typeof(BusinessObjects))]
        public int? MachineId { get; set; }

        [Display(Name = nameof(BusinessObjects.Machine), ResourceType = typeof(BusinessObjects))]
        public string MachineNickname { get; set; }

        #endregion
    }
}
