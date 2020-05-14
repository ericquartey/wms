using System;
using System.Configuration;
using Ferretto.VW.MAS.AutomationService.Contracts;
using System.Globalization;
using Ferretto.VW.App.Resources;
using DevExpress.CodeParser;
using DevExpress.XtraRichEdit.Services;

namespace Ferretto.VW.App.Services
{
    internal sealed class LocalizationService : ILocalizationService
    {
        #region Fields

        private const string actualLanguageKey = "Language";

        private const string adminLanguageKey = "AdminLanguage";

        private const string installerLanguageKey = "InstallerLanguage";

        private const string operatorLanguageKey = "OperatorLanguage";

        private const string serviceLanguageKey = "ServiceLanguage";

        private readonly ISessionService sessionService;

        private readonly IMachineUsersWebService usersService;

        #endregion

        #region Constructors

        public LocalizationService(ISessionService sessionService, IMachineUsersWebService usersService)
        {
            this.sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
            this.usersService = usersService ?? throw new ArgumentNullException(nameof(usersService));
        }

        #endregion

        #region Properties

        public string ActualLanguageKey => actualLanguageKey;

        public string AdminLanguageKey => adminLanguageKey;

        public string InstallerLanguageKey => installerLanguageKey;

        public string OperatorLanguageKey => operatorLanguageKey;

        public string ServiceLanguageKey => serviceLanguageKey;

        #endregion

        #region Methods

        /// <summary>
        /// Activate language for selected user
        /// </summary>
        public void ActivateCulture(UserAccessLevel userAccessLevel)
        {
            try
            {
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = configFile.AppSettings.Settings;
                string key = this.GetLanguageKey(userAccessLevel);
                if (settings.Count == 0 | settings[key] == null)
                {
                    ;
                }
                else
                {
                    settings[actualLanguageKey].Value = settings[key].Value;

                    Localized.Instance.CurrentCulture = CultureInfo.GetCultureInfo(settings[key].Value);

                    this.usersService.SetMASCultureAsync(settings[key].Value);
                }
                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            }
            catch (ConfigurationErrorsException exc)
            {
                //Logger.WriteLog(exc.Message, LoggingLevel.Error);
            }
        }

        /// <summary>
        /// Set language for selected user
        /// </summary>
        public void SetCulture(UserAccessLevel userAccessLevel, string culture)
        {
            try
            {
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = configFile.AppSettings.Settings;
                string key = this.GetLanguageKey(userAccessLevel);
                if (settings.Count == 0 | settings[key] == null)
                {
                    ;
                }
                else
                {
                    if (this.sessionService.UserAccessLevel == userAccessLevel)
                    {
                        settings[actualLanguageKey].Value = culture;

                        Localized.Instance.CurrentCulture = CultureInfo.GetCultureInfo(culture);

                        this.usersService.SetMASCultureAsync(culture);
                    }
                    settings[key].Value = culture;
                }
                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            }
            catch (ConfigurationErrorsException exc)
            {
                //Logger.WriteLog(exc.Message, LoggingLevel.Error);
            }
        }

        private string GetLanguageKey(UserAccessLevel userAccessLevel)
        {
            string key = "";
            if (userAccessLevel == UserAccessLevel.Admin)
            { key = adminLanguageKey; }
            else if (userAccessLevel == UserAccessLevel.Installer)
            { key = installerLanguageKey; }
            else if (userAccessLevel == UserAccessLevel.Support)
            { key = serviceLanguageKey; }
            else if (userAccessLevel == UserAccessLevel.Operator)
            { key = operatorLanguageKey; }

            return key.ToString();
        }

        #endregion
    }
}
