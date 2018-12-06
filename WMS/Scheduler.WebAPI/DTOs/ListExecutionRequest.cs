using System.ComponentModel.DataAnnotations;

namespace Ferretto.WMS.Scheduler.WebAPI
{
    public class ListExecutionRequest
    {
        #region Properties

        [Required]
        public int BayId { get; set; }

        [Required]
        public int ListId { get; set; }

        #endregion Properties
    }
}
