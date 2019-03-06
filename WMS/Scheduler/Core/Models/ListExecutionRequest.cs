using System.ComponentModel.DataAnnotations;

namespace Ferretto.WMS.Scheduler.Core.Models
{
    public class ListExecutionRequest
    {
        #region Properties

        [Required]
        public int AreaId { get; set; }

        public int? BayId { get; set; }

        [Required]
        public int ListId { get; set; }

        #endregion
    }
}
