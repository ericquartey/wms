﻿using System;
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

        private readonly ILocalizationService localizationService;

        private string adminLanguage;

        private string installerLanguage;

        private ObservableCollection<string> languageList = new ObservableCollection<string> { "ITA", "EN" };

        private string operatorLanguage;

        private string serviceLanguage;

        #endregion

        #region Constructors

        public UserViewModel(ILocalizationService localizationService) : base()
        {
            this.localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));

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
                    this.localizationService.SetCulture(UserAccessLevel.Admin, this.GetCultureFromShortcut(value));

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

            this.adminLanguage = this.GetShortcutFromCulture(settings[this.localizationService.AdminLanguageKey].Value);
            this.serviceLanguage = this.GetShortcutFromCulture(settings[this.localizationService.ServiceLanguageKey].Value);
            this.installerLanguage = this.GetShortcutFromCulture(settings[this.localizationService.InstallerLanguageKey].Value);
            this.operatorLanguage = this.GetShortcutFromCulture(settings[this.localizationService.OperatorLanguageKey].Value);
        }

        #endregion
    }
}
