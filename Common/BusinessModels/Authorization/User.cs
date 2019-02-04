using System.ComponentModel.DataAnnotations;
using Ferretto.Common.Resources;

namespace Ferretto.Common.BusinessModels
{
    public class User : BusinessObject
    {
        #region Fields

        private string login;
        private string password;

        #endregion

        #region Properties

        [Display(Name = nameof(BusinessObjects.Username), ResourceType = typeof(BusinessObjects))]
        public string Login
        {
            get => this.login;
            set => this.SetProperty(ref this.login, value);
        }

        [Display(Name = nameof(BusinessObjects.Password), ResourceType = typeof(BusinessObjects))]
        public string Password
        {
            get => this.password;
            set => this.SetProperty(ref this.password, value);
        }

        #endregion
    }
}
