using Ferretto.VW.Utils;

namespace Ferretto.VW.App.Modules.Login.Models
{
    public class UserLogin : BaseModel
    {
        #region Fields

        private const int MinimumPasswordLength = 2;

        private string password;

        private string supportToken;

        private string userName;

        #endregion

        #region Properties

        public bool IsSupport => string.CompareOrdinal(this.UserName, "service") == 0;

        public string Password
        {
            get => this.password;
            set => this.SetProperty(ref this.password, value);
        }

        public string SupportToken
        {
            get => this.supportToken;
            set => this.SetProperty(ref this.supportToken, value);
        }

        public string UserName
        {
            get => this.userName;
            set
            {
                this.SetProperty(ref this.userName, value);
                if (!this.IsSupport)
                {
                    this.SupportToken = null;
                }

                this.RaisePropertyChanged(nameof(this.IsSupport));
            }
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
                        return Resources.Localized.Get("LoadLogin.PasswordMustBeSpecified");
                    }

                    if (this.Password.Length < MinimumPasswordLength)
                    {
                        return Resources.Localized.Get("LoadLogin.PasswordIsTooShort");
                    }

                    break;

                case nameof(this.UserName):
                    if (string.IsNullOrWhiteSpace(this.UserName))
                    {
                        return Resources.Localized.Get("LoadLogin.UserNameMustBeSpecified");
                    }

                    break;
            }

            return base.ValidateProperty(propertyName);
        }

        #endregion
    }
}
