using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Ferretto.Common.Resources;

namespace Ferretto.WMS.App.Core.Models
{
    public class Mission : BusinessObject
    {
        #region Properties

        [Display(Name = nameof(BusinessObjects.Bay), ResourceType = typeof(BusinessObjects))]
        public string BayDescription { get; set; }

        [Display(Name = nameof(General.CreationDate), ResourceType = typeof(General))]
        public DateTime CreationDate { get; set; }

        [Display(Name = nameof(General.LastModificationDate), ResourceType = typeof(General))]
        public DateTime? LastModificationDate { get; set; }

        [Display(Name = nameof(BusinessObjects.LoadingUnit), ResourceType = typeof(BusinessObjects))]
        public string LoadingUnitDescription { get; set; }

        public IEnumerable<MissionOperation> Operations { get; set; }

        [Display(Name = nameof(General.Priority), ResourceType = typeof(General))]
        public int? Priority { get; set; }

        [Display(Name = nameof(General.Status), ResourceType = typeof(General))]
        public MissionStatus? Status { get; set; } = MissionStatus.New;

        #endregion
    }
}
