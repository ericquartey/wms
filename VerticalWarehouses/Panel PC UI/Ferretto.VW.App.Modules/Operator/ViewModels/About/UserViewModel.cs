using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DevExpress.CodeParser;
using DevExpress.Xpf.Bars.Themes;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Ferretto.VW.App.Services;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    [Warning(WarningsArea.Information)]
    internal sealed class UserViewModel : BaseAboutMenuViewModel
    {
        #region Fields

        private const string adminLanguageKey = "AdminLanguage";

        private const string installerLanguageKey = "InstallerLanguage";

        private const string operatorLanguageKey = "OperatorLanguage";

        private const string serviceLanguageKey = "ServiceLanguage";

        private readonly ISessionService sessionService;

        private string adminLanguage;

        private string installerLanguage;

        private ObservableCollection<string> languageList = new ObservableCollection<string> { "ITA", "EN" };

        private string operatorLanguage;

        private string serviceLanguage;

        #endregion

        #region Constructors

        public UserViewModel(ISessionService sessionService) : base()
        {
            this.sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));

            this.GetLanguageFromFile();
        }

        #endregion

        #region Properties

        public string AdminLanguage
        {
            get => this.adminLanguage;
            set
            {
                if (this.SetProperty(ref this.adminLanguage, value))
                {
                    this.Update(adminLanguageKey, this.GetCultureFromShortcut(value));

                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public string InstallerLanguage
        {
            get => this.installerLanguage;
            set
            {
                if (this.SetProperty(ref this.installerLanguage, value))
                {
                    this.Update(installerLanguageKey, this.GetCultureFromShortcut(value));

                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public ObservableCollection<string> LanguageList
        {
            get => this.languageList;
            set => this.SetProperty(ref this.languageList, value, this.RaiseCanExecuteChanged);
        }

        public string OperatorLanguage
        {
            get => this.operatorLanguage;
            set
            {
                if (this.SetProperty(ref this.operatorLanguage, value))
                {
                    this.Update(operatorLanguageKey, this.GetCultureFromShortcut(value));

                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public string ServiceLanguage
        {
            get => this.serviceLanguage;
            set
            {
                if (this.SetProperty(ref this.serviceLanguage, value))
                {
                    this.Update(serviceLanguageKey, this.GetCultureFromShortcut(value));

                    this.RaiseCanExecuteChanged();
                }
            }
        }

        #endregion

        #region Methods

        public string GetCultureFromShortcut(string shortcut)
        {
            if (shortcut == "ITA")
            {
                return "it-IT";
            }
            else if (shortcut == "EN")
            {
                return "en-EN";
            }
            else
            {
                return "en-EN";
            }
        }

        public string GetShortcutFromCulture(string language)
        {
            if (language == "it-IT")
            {
                return "ITA";
            }
            else if (language == "en-EN")
            {
                return "EN";
            }
            else
            {
                return "EN";
            }
        }

        public override async Task OnAppearedAsync()
        {
            this.IsBackNavigationAllowed = true;

            await base.OnAppearedAsync();
        }

        public void Update(string key, string value)
        {
            try
            {
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = configFile.AppSettings.Settings;
                if (settings.Count == 0 | settings[key] == null)
                {
                    ;
                }
                else
                {
                    //Update actual language
                    if (
                            ((this.sessionService.UserAccessLevel == UserAccessLevel.Admin) && (key == adminLanguageKey))
                            ||
                            ((this.sessionService.UserAccessLevel == UserAccessLevel.Installer) && (key == installerLanguageKey))
                            ||
                            ((this.sessionService.UserAccessLevel == UserAccessLevel.Support) && (key == serviceLanguageKey))
                            ||
                            ((this.sessionService.UserAccessLevel == UserAccessLevel.Operator) && (key == operatorLanguageKey))
                       )
                    {
                    }
                    settings[key].Value = value;
                }
                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            }
            catch (ConfigurationErrorsException exc)
            {
                //Logger.WriteLog(exc.Message, LoggingLevel.Error);
            }

            //Configuration config = ConfigurationManager. OpenExeConfiguration(ConfigurationUserLevel.None);
            //config.AppSettings.Settings[key].Value = value;
            //config.Save();
            //ConfigurationManager.RefreshSection("appSettings");

            //this.RaiseCanExecuteChanged();
        }

        internal bool CanExecuteCommand()
        {
            return true;
        }

        protected override async Task OnDataRefreshAsync()
        {
            try
            {
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
            catch (Exception)
            {
                throw;
            }
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();
        }

        private void GetLanguageFromFile()
        {
            var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var settings = configFile.AppSettings.Settings;

            this.adminLanguage = this.GetShortcutFromCulture(settings[adminLanguageKey].Value);
            this.serviceLanguage = this.GetShortcutFromCulture(settings[serviceLanguageKey].Value);
            this.installerLanguage = this.GetShortcutFromCulture(settings[installerLanguageKey].Value);
            this.operatorLanguage = this.GetShortcutFromCulture(settings[operatorLanguageKey].Value);
        }

        #endregion
    }
}
