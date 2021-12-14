using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using DevExpress.Xpf.Core.ReflectionExtensions.Internal;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    public struct Culture
    {
        #region Constructors

        public Culture(string image, string shortCut, string shortCutInfo)
        {
            this.Image = image;
            this.ShortCut = shortCut;
            this.ShortCutInfo = shortCutInfo;
        }

        #endregion

        #region Properties

        public string Image { get; set; }

        public string ShortCut { get; set; }

        public string ShortCutInfo { get; set; }

        #endregion
    }

    [Warning(WarningsArea.Information)]
    internal sealed class UserViewModel : BaseAboutMenuViewModel
    {
        #region Fields

        private readonly ILocalizationService localizationService;

        private readonly IMachineUsersWebService machineUsersWebService;

        private readonly ISessionService sessionService;

        private bool adminEnabled;

        private Culture adminLanguage;

        private bool installerEnabled;

        private Culture installerLanguage;

        private List<Culture> languageList;

        private bool operatorEnabled;

        private Culture operatorLanguage;

        private bool serviceEnabled;

        private Culture serviceLanguage;

        #endregion

        #region Constructors

        public UserViewModel(
            ILocalizationService localizationService,
            IMachineUsersWebService machineUsersWebService,
            ISessionService sessionService)
        {
            this.localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
            this.machineUsersWebService = machineUsersWebService ?? throw new ArgumentNullException(nameof(machineUsersWebService));
            this.sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
        }

        #endregion

        #region Properties

        public bool AdminEnabled
        {
            get => this.adminEnabled;
            set => this.SetProperty(ref this.adminEnabled, value, this.RaiseCanExecuteChanged);
        }

        public Culture AdminLanguage
        {
            get => this.adminLanguage;
            set
            {
                if (this.SetProperty(ref this.adminLanguage, value))
                {
                    this.localizationService.SetCulture(UserAccessLevel.Admin, value.ShortCut);

                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool InstallerEnabled
        {
            get => this.installerEnabled;
            set => this.SetProperty(ref this.installerEnabled, value, this.RaiseCanExecuteChanged);
        }

        public Culture InstallerLanguage
        {
            get => this.installerLanguage;
            set
            {
                if (this.SetProperty(ref this.installerLanguage, value))
                {
                    this.localizationService.SetCulture(UserAccessLevel.Installer, value.ShortCut);

                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public List<Culture> LanguageList
        {
            get => this.languageList;
            set => this.SetProperty(ref this.languageList, value, this.RaiseCanExecuteChanged);
        }

        public bool OperatorEnabled
        {
            get => this.operatorEnabled;
            set => this.SetProperty(ref this.operatorEnabled, value, this.RaiseCanExecuteChanged);
        }

        public Culture OperatorLanguage
        {
            get => this.operatorLanguage;
            set
            {
                if (this.SetProperty(ref this.operatorLanguage, value))
                {
                    this.localizationService.SetCulture(UserAccessLevel.Operator, value.ShortCut);

                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool ServiceEnabled
        {
            get => this.serviceEnabled;
            set => this.SetProperty(ref this.serviceEnabled, value, this.RaiseCanExecuteChanged);
        }

        public Culture ServiceLanguage
        {
            get => this.serviceLanguage;
            set
            {
                if (this.SetProperty(ref this.serviceLanguage, value))
                {
                    this.localizationService.SetCulture(UserAccessLevel.Support, value.ShortCut);

                    this.RaiseCanExecuteChanged();
                }
            }
        }

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            this.OperatorEnabled = true;

            this.InstallerEnabled = this.sessionService.UserAccessLevel == UserAccessLevel.Installer ||
                this.sessionService.UserAccessLevel == UserAccessLevel.Admin;

            this.ServiceEnabled = this.sessionService.UserAccessLevel == UserAccessLevel.Support ||
                this.sessionService.UserAccessLevel == UserAccessLevel.Admin;

            this.AdminEnabled = this.sessionService.UserAccessLevel == UserAccessLevel.Admin;

            this.LanguageList = this.SetLanguageList();

            this.GetLanguageFromFile();

            this.IsBackNavigationAllowed = true;

            await base.OnAppearedAsync();
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();
        }

        private Culture GetItemByCulture(string culture)
        {
            switch (culture)
            {
                case "it-IT":
                    return new Culture(
                        $"pack://application:,,,/Ferretto.VW.App.Themes;Component/Images/Flags/IT.png",
                        "it-IT",
                        "Italiano");

                case "en-EN":
                    return new Culture(
                            $"pack://application:,,,/Ferretto.VW.App.Themes;Component/Images/Flags/EN.png",
                            "en-EN",
                            "English");

                case "de-DE":
                    return new Culture(
                             $"pack://application:,,,/Ferretto.VW.App.Themes;Component/Images/Flags/DE.png",
                             "de-DE",
                             "Deutsche");

                case "es-ES":
                    return new Culture(
                            $"pack://application:,,,/Ferretto.VW.App.Themes;Component/Images/Flags/ES.png",
                            "es-ES",
                            "Español");

                case "fr-FR":
                    return new Culture(
                            $"pack://application:,,,/Ferretto.VW.App.Themes;Component/Images/Flags/FR.png",
                            "fr-FR",
                            "Français");

                case "pl-PL":
                    return new Culture(
                            $"pack://application:,,,/Ferretto.VW.App.Themes;Component/Images/Flags/PL.png",
                            "pl-PL",
                            "Polskie");

                case "ru-RU":
                    return new Culture(
                            $"pack://application:,,,/Ferretto.VW.App.Themes;Component/Images/Flags/RU.png",
                            "ru-RU",
                            "Pусский");

                //case "sk-SK":
                //    return new Culture(
                //            $"pack://application:,,,/Ferretto.VW.App.Themes;Component/Images/Flags/SK.png",
                //            "sk-SK",
                //            "Slovcco");

                case "sl-SI":
                    return new Culture(
                            $"pack://application:,,,/Ferretto.VW.App.Themes;Component/Images/Flags/SI.png",
                            "sl-SI",
                            "Slovenščina");

                case "el-GR":
                    return new Culture(
                            $"pack://application:,,,/Ferretto.VW.App.Themes;Component/Images/Flags/GR.png",
                            "el-GR",
                            "Ελληνικά");

                case "he-IL":
                    return new Culture(
                            $"pack://application:,,,/Ferretto.VW.App.Themes;Component/Images/Flags/IL.png",
                            "he-IL",
                            "יהודי");

                case "hr-HR":
                    return new Culture(
                             $"pack://application:,,,/Ferretto.VW.App.Themes;Component/Images/Flags/HR.png",
                             "hr-HR",
                             "Hrvatski");

                case "hu-HU":
                    return new Culture(
                            $"pack://application:,,,/Ferretto.VW.App.Themes;Component/Images/Flags/HU.png",
                            "hu-HU",
                            "Magyar");

                case "cs-CZ":
                    return new Culture(
                            $"pack://application:,,,/Ferretto.VW.App.Themes;Component/Images/Flags/CZ.png",
                            "cs-CZ",
                            "Čeština");

                default:
                    return new Culture(
                            $"pack://application:,,,/Ferretto.VW.App.Themes;Component/Images/Flags/EN.png",
                            "en-EN",
                            "English");
            }
        }

        private async void GetLanguageFromFile()
        {
            var users = await this.machineUsersWebService.GetAllUserWithCultureAsync();

            this.AdminLanguage = this.GetItemByCulture(users.Where(s => s.Name == "admin").Select(s => s.Language).FirstOrDefault());
            this.ServiceLanguage = this.GetItemByCulture(users.Where(s => s.Name == "service").Select(s => s.Language).FirstOrDefault());
            this.InstallerLanguage = this.GetItemByCulture(users.Where(s => s.Name == "installer").Select(s => s.Language).FirstOrDefault());
            this.OperatorLanguage = this.GetItemByCulture(users.Where(s => s.Name == "operator").Select(s => s.Language).FirstOrDefault());
        }

        private List<Culture> SetLanguageList()
        {
            List<Culture> res = new List<Culture>();

            res.Add(new Culture(
                        $"pack://application:,,,/Ferretto.VW.App.Themes;Component/Images/Flags/IT.png",
                        "it-IT",
                        "Italiano"));

            res.Add(new Culture(
                            $"pack://application:,,,/Ferretto.VW.App.Themes;Component/Images/Flags/CZ.png",
                            "cs-CZ",
                            "Čeština"));

            res.Add(new Culture(
                             $"pack://application:,,,/Ferretto.VW.App.Themes;Component/Images/Flags/DE.png",
                             "de-DE",
                             "Deutsche"));

            res.Add(new Culture(
                            $"pack://application:,,,/Ferretto.VW.App.Themes;Component/Images/Flags/GR.png",
                            "el-GR",
                            "Ελληνικά"));

            res.Add(new Culture(
                            $"pack://application:,,,/Ferretto.VW.App.Themes;Component/Images/Flags/EN.png",
                            "en-EN",
                            "English"));

            res.Add(new Culture(
                            $"pack://application:,,,/Ferretto.VW.App.Themes;Component/Images/Flags/ES.png",
                            "es-ES",
                            "Español"));

            res.Add(new Culture(
                            $"pack://application:,,,/Ferretto.VW.App.Themes;Component/Images/Flags/FR.png",
                            "fr-FR",
                            "Français"));

            //res.Add(new Culture(
            //                $"pack://application:,,,/Ferretto.VW.App.Themes;Component/Images/Flags/SK.png",
            //                "sk-SK",
            //                "Slovenský"));

            res.Add(new Culture(
                            $"pack://application:,,,/Ferretto.VW.App.Themes;Component/Images/Flags/IL.png",
                            "he-IL",
                            "יהודי"));

            res.Add(new Culture(
                             $"pack://application:,,,/Ferretto.VW.App.Themes;Component/Images/Flags/HR.png",
                             "hr-HR",
                             "Hrvatski"));

            res.Add(new Culture(
                            $"pack://application:,,,/Ferretto.VW.App.Themes;Component/Images/Flags/HU.png",
                            "hu-HU",
                            "Magyar"));

            res.Add(new Culture(
                            $"pack://application:,,,/Ferretto.VW.App.Themes;Component/Images/Flags/PL.png",
                            "pl-PL",
                            "Polskie"));

            res.Add(new Culture(
                            $"pack://application:,,,/Ferretto.VW.App.Themes;Component/Images/Flags/RU.png",
                            "ru-RU",
                            "Pусский"));

            res.Add(new Culture(
                            $"pack://application:,,,/Ferretto.VW.App.Themes;Component/Images/Flags/SI.png",
                            "sl-SI",
                            "Slovenščina"));

            return res;
        }

        #endregion
    }
}
