using System;
using System.ComponentModel.DataAnnotations;
using Ferretto.Common.Resources;

namespace Ferretto.Common.BusinessModels
{
    public sealed class Area : BusinessObject
    {
        #region Properties

        [Display(Name = nameof(BusinessObjects.AreaDescription), ResourceType = typeof(BusinessObjects))]
        public string Name { get; set; }

        #endregion Properties
    }
}
