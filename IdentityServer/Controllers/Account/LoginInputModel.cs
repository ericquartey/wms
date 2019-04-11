using System.ComponentModel.DataAnnotations;

namespace Ferretto.IdentityServer
{
    public class LoginInputModel
    {
        #region Properties

        [Required]
        public string Password { get; set; }

        public bool RememberLogin { get; set; }

        public string ReturnUrl { get; set; }

        [Required]
        public string Username { get; set; }

        #endregion
    }
}
