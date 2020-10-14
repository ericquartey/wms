using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Threading.Tasks;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    [Warning(WarningsArea.Information)]
    internal sealed class UserViewModel : BaseAboutMenuViewModel
    {
        #region Fields

        private readonly ILocalizationService localizationService;

        private readonly ISessionService sessionService;

        private bool adminEnabled;

        private string adminLanguage;

        private bool installerEnabled;

        private string installerLanguage;

        private ObservableCollection<string> languageList = new ObservableCollection<string> { "ITA", "EN", "CS", "DE", "ES" /*, "FR", "PL", "RU", "SK", "SI"*/, "HR" };

        private bool operatorEnabled;

        private string operatorLanguage;

        private bool serviceEnabled;

        private string serviceLanguage;

        #endregion

        #region Constructors

        public UserViewModel(ILocalizationService localizationService,
            ISessionService sessionService)
        {
            this.localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
            this.sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));

            this.GetLanguageFromFile();
        }

        #endregion

        #region Properties

        public bool AdminEnabled
        {
            get => this.adminEnabled;
            set => this.SetProperty(ref this.adminEnabled, value, this.RaiseCanExecuteChanged);
        }

        public string AdminLanguage
        {
            get => this.adminLanguage;
            set
            {
                if (this.SetProperty(ref this.adminLanguage, value))
                {
                    this.localizationService.SetCulture(UserAccessLevel.Admin, this.GetCultureFromShortcut(value));

                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool InstallerEnabled
        {
            get => this.installerEnabled;
            set => this.SetProperty(ref this.installerEnabled, value, this.RaiseCanExecuteChanged);
        }

        public string InstallerLanguage
        {
            get => this.installerLanguage;
            set
            {
                if (this.SetProperty(ref this.installerLanguage, value))
                {
                    this.localizationService.SetCulture(UserAccessLevel.Installer, this.GetCultureFromShortcut(value));

                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public ObservableCollection<string> LanguageList
        {
            get => this.languageList;
            set => this.SetProperty(ref this.languageList, value, this.RaiseCanExecuteChanged);
        }

        public bool OperatorEnabled
        {
            get => this.operatorEnabled;
            set => this.SetProperty(ref this.operatorEnabled, value, this.RaiseCanExecuteChanged);
        }

        public string OperatorLanguage
        {
            get => this.operatorLanguage;
            set
            {
                if (this.SetProperty(ref this.operatorLanguage, value))
                {
                    this.localizationService.SetCulture(UserAccessLevel.Operator, this.GetCultureFromShortcut(value));

                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool ServiceEnabled
        {
            get => this.serviceEnabled;
            set => this.SetProperty(ref this.serviceEnabled, value, this.RaiseCanExecuteChanged);
        }

        public string ServiceLanguage
        {
            get => this.serviceLanguage;
            set
            {
                if (this.SetProperty(ref this.serviceLanguage, value))
                {
                    this.localizationService.SetCulture(UserAccessLevel.Support, this.GetCultureFromShortcut(value));

                    this.RaiseCanExecuteChanged();
                }
            }
        }

        #endregion

        #region Methods

        public string GetCultureFromShortcut(string shortcut)
        {
            switch (shortcut)
            {
                case "ITA":
                    return "it-IT";

                case "EN":
                    return "en-EN";

                //case "DE":
                //    return "de-DE";

                //case "ES":
                //    return "es-ES";

                //case "FR":
                //    return "fr-FR";

                //case "PL":
                //    return "pl-PL";

                //case "RU":
                //    return "ru-RU";

                //case "SK":
                //    return "sk-SK";

                //case "SI":
                //    return "si-SI";

                case "HR":
                    return "hr-HR";

                default:
                    return "en-EN";
            }
        }

        public string GetShortcutFromCulture(string language)
        {
            switch (language)
            {
                case "it-IT":
                    return "ITA";

                case "en-EN":
                    return "EN";

                case "de-DE":
                    return "DE";

                case "es-ES":
                    return "ES";

                //case "fr-FR":
                //    return "FR";

                //case "pl-PL":
                //    return "PL";

                //case "ru-RU":
                //    return "RU";

                //case "sk-SK":
                //    return "SK";

                //case "si-SI":
                //    return "SI";

                case "hr-HR":
                    return "HR";

                case "cs-CZ":
                    return "CZ";

                default:
                    return "EN";
            }
        }

        public override async Task OnAppearedAsync()
        {
            this.IsBackNavigationAllowed = true;

            await base.OnAppearedAsync();

            this.OperatorEnabled = true;

            this.InstallerEnabled = this.sessionService.UserAccessLevel == UserAccessLevel.Installer ||
                this.sessionService.UserAccessLevel == UserAccessLevel.Admin;

            this.ServiceEnabled = this.sessionService.UserAccessLevel == UserAccessLevel.Support ||
                this.sessionService.UserAccessLevel == UserAccessLevel.Admin;

            this.AdminEnabled = this.sessionService.UserAccessLevel == UserAccessLevel.Admin;
        }

        internal bool CanExecuteCommand()
        {
            return true;
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();
        }

        private void GetLanguageFromFile()
        {
            var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var settings = configFile.AppSettings.Settings;

            this.adminLanguage = this.GetShortcutFromCulture(settings[this.localizationService.AdminLanguageKey].Value);
            this.serviceLanguage = this.GetShortcutFromCulture(settings[this.localizationService.ServiceLanguageKey].Value);
            this.installerLanguage = this.GetShortcutFromCulture(settings[this.localizationService.InstallerLanguageKey].Value);
            this.operatorLanguage = this.GetShortcutFromCulture(settings[this.localizationService.OperatorLanguageKey].Value);
        }

        #endregion
    }
}
