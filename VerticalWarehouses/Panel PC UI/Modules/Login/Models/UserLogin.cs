using Ferretto.VW.Utils;

namespace Ferretto.VW.App.Modules.Login.Models
{
    public class UserLogin : BaseModel
    {
        #region Fields

        private const int MinimumPasswordLength = 2;

        private string password;

        private string userName;

        #endregion

        #region Properties

        public string Password
        {
            get => this.password;
            set => this.SetProperty(ref this.password, value);
        }

        public string UserName
        {
            get => this.userName;
            set => this.SetProperty(ref this.userName, value);
        }

        #endregion

        #region Methods

        protected override string ValidateProperty(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(this.Password):
                    if (string.IsNullOrEmpty(this.Password))
                    {
                        return Resources.Errors.UserLogin_PasswordMustBeSpecified;
                    }

                    if (this.Password.Length < MinimumPasswordLength)
                    {
                        return Resources.Errors.UserLogin_PasswordIsTooShort;
                    }
                    break;

                case nameof(this.UserName):
                    if (string.IsNullOrWhiteSpace(this.UserName))
                    {
                        return Resources.Errors.UserLogin_UserNameMustBeSpecified;
                    }

                    break;
            }

            return base.ValidateProperty(propertyName);
        }

        #endregion
    }
}
