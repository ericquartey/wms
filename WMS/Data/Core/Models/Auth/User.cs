using System.ComponentModel.DataAnnotations;

namespace Ferretto.WMS.Data.Core.Models
{
    public class User : BaseModel<int>
    {
        #region Properties

        [Required]
        public string Login { get; set; }

        [Required]
        public string Password { get; set; }

        #endregion
    }
}
