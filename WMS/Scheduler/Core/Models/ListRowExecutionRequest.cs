using System.ComponentModel.DataAnnotations;

namespace Ferretto.WMS.Scheduler.Core.Models
{
    public class ListRowExecutionRequest
    {
        #region Properties

        [Required]
        public int AreaId { get; set; }

        public int? BayId { get; set; }

        [Required]
        public int ListRowId { get; set; }

        #endregion
    }
}
