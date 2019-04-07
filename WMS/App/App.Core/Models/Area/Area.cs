using System.ComponentModel.DataAnnotations;
using Ferretto.Common.Resources;
using Ferretto.Common.Utils;

namespace Ferretto.WMS.App.Core.Models
{
    [Resource(nameof(Data.WebAPI.Contracts.Area))]
    public sealed class Area : BusinessObject
    {
        #region Properties

        [Display(Name = nameof(BusinessObjects.AreaDescription), ResourceType = typeof(BusinessObjects))]
        public string Name { get; set; }

        #endregion
    }
}
