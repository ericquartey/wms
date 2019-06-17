using System.ComponentModel.DataAnnotations;
using Ferretto.Common.Utils;

namespace Ferretto.WMS.Data.Core.Models
{
    [Resource(nameof(User))]
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
