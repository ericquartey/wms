using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using Ferretto.VW.App.Resources;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Services
{
    internal sealed class LocalizationService : ILocalizationService
    {
        #region Fields

        private const string actualLanguageKey = "Language";

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

                var users = this.usersService.GetAllUserWithCultureAsync().Result;

                if (settings.Count == 0 | users == null)
                {
                    ;
                }
                else
                {
                    if (userAccessLevel == UserAccessLevel.Support)
                    {
                        settings[actualLanguageKey].Value = users.Where(s => s.Name == "service").Select(s => s.Language).FirstOrDefault();

                        Localized.Instance.CurrentCulture = CultureInfo.GetCultureInfo(users.Where(s => s.Name == "service").Select(s => s.Language).FirstOrDefault());
                        Localized.Instance.CurrentKeyboardCulture = CultureInfo.GetCultureInfo(users.Where(s => s.Name == "service").Select(s => s.Language).FirstOrDefault());

                        this.usersService.SetMASCultureAsync(users.Where(s => s.Name == "service").Select(s => s.Language).FirstOrDefault());
                    }
                    else
                    {
                        settings[actualLanguageKey].Value = users.Where(s => (UserAccessLevel)s.AccessLevel == userAccessLevel).Select(s => s.Language).FirstOrDefault();

                        Localized.Instance.CurrentCulture = CultureInfo.GetCultureInfo(users.Where(s => (UserAccessLevel)s.AccessLevel == userAccessLevel).Select(s => s.Language).FirstOrDefault());
                        Localized.Instance.CurrentKeyboardCulture = CultureInfo.GetCultureInfo(users.Where(s => (UserAccessLevel)s.AccessLevel == userAccessLevel).Select(s => s.Language).FirstOrDefault());

                        this.usersService.SetMASCultureAsync(users.Where(s => (UserAccessLevel)s.AccessLevel == userAccessLevel).Select(s => s.Language).FirstOrDefault());
                    }
                }
                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            }
            catch (ConfigurationErrorsException)
            {
                // do nothing
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

                if (settings.Count == 0)
                {
                    ;
                }
                else
                {
                    if (this.sessionService.UserAccessLevel == userAccessLevel)
                    {
                        settings[actualLanguageKey].Value = culture;

                        Localized.Instance.CurrentCulture = CultureInfo.GetCultureInfo(culture);
                        Localized.Instance.CurrentKeyboardCulture = CultureInfo.GetCultureInfo(culture);

                        this.usersService.SetMASCultureAsync(culture);
                    }

                    if (userAccessLevel == UserAccessLevel.Support)
                    {
                        this.usersService.SetUserCultureAsync(culture, "service");
                    }
                    else
                    {
                        this.usersService.SetUserCultureAsync(culture, userAccessLevel.ToString().ToLower());
                    }
                }
                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            }
            catch (ConfigurationErrorsException)
            {
                // do nothing
            }
        }

        #endregion
    }
}
