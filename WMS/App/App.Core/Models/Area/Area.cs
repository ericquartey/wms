using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Ferretto.WMS.App.Resources;

namespace Ferretto.WMS.App.Core.Models
{
    public sealed class Area : BusinessObject
    {
        #region Properties

        [Display(Name = nameof(BusinessObjects.Area), ResourceType = typeof(BusinessObjects))]
        public string Name { get; set; }

        public IEnumerable<Bay> Bays { get; set; }

        #endregion
    }
}
