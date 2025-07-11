﻿using System.Threading.Tasks;
using System.Windows.Input;
using CommonServiceLocator;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    internal abstract class BaseAboutMenuViewModel : BaseMainViewModel
    {
        #region Fields

        public ISessionService sessionService;

        private DelegateCommand alarm;

        private DelegateCommand diagnosticsCommand;

        private DelegateCommand generalCommand;

        private DelegateCommand inverterDiagnosticsCommand;

        private bool isAdmin;

        private bool isAlarmActive;

        private bool isDiagnosticsActive;

        private bool isGeneralActive;

        private bool isInverterDiagnosticsActive;

        private bool isNetworkAdaptersActive;

        private bool isStatisticsActive;

        private bool isUserActive;

        private DelegateCommand networkAdaptersCommand;

        private DelegateCommand statisticsCommand;

        private DelegateCommand userCommand;

        #endregion

        #region Constructors

        public BaseAboutMenuViewModel()
            : base(PresentationMode.Menu)
        {
            this.sessionService = ServiceLocator.Current.GetInstance<ISessionService>();
        }

        #endregion

        #region Enums

        private enum Menu
        {
            Alarm,

            General,

            Statistics,

            Diagnostics,

            User,

            NetworkAdapters,

            InverterDiagnostics,
        }

        #endregion

        #region Properties

        public ICommand AlarmCommand =>
            this.alarm
            ??
            (this.alarm = new DelegateCommand(
                () => this.ExecuteCommand(Menu.Alarm),
                this.CanExecuteCommand))
            ;

        public ICommand DiagnosticsCommand =>
            this.diagnosticsCommand
            ??
            (this.diagnosticsCommand = new DelegateCommand(
                () => this.ExecuteCommand(Menu.Diagnostics),
                this.CanExecuteDiagnosticCommand));

        public override EnableMask EnableMask => EnableMask.Any;

        public ICommand GeneralCommand =>
            this.generalCommand
            ??
            (this.generalCommand = new DelegateCommand(
                () => this.ExecuteCommand(Menu.General)));

        public ICommand InverterDiagnosticsCommand =>
            this.inverterDiagnosticsCommand
            ??
            (this.inverterDiagnosticsCommand = new DelegateCommand(
                () => this.ExecuteCommand(Menu.InverterDiagnostics),
                this.CanExecuteCommand));

        public bool IsAlarmActive
        {
            get => this.isAlarmActive;
            set => this.SetProperty(ref this.isAlarmActive, value, this.RaiseCanExecuteChanged);
        }

        public bool IsDiagnosticsActive
        {
            get => this.isDiagnosticsActive;
            set => this.SetProperty(ref this.isDiagnosticsActive, value, this.RaiseCanExecuteChanged);
        }

        public bool IsAdmin
        {
            get => this.isAdmin;
            set => this.SetProperty(ref this.isAdmin, value, this.RaiseCanExecuteChanged);
        }

        public bool IsGeneralActive
        {
            get => this.isGeneralActive;
            set => this.SetProperty(ref this.isGeneralActive, value, this.RaiseCanExecuteChanged);
        }

        public bool IsInverterDiagnosticsActive
        {
            get => this.isInverterDiagnosticsActive;
            set => this.SetProperty(ref this.isInverterDiagnosticsActive, value, this.RaiseCanExecuteChanged);
        }

        public bool IsNetworkAdaptersActive
        {
            get => this.isNetworkAdaptersActive;
            set => this.SetProperty(ref this.isNetworkAdaptersActive, value, this.RaiseCanExecuteChanged);
        }

        public bool IsStatisticsActive
        {
            get => this.isStatisticsActive;
            set => this.SetProperty(ref this.isStatisticsActive, value, this.RaiseCanExecuteChanged);
        }

        public bool IsUserActive
        {
            get => this.isUserActive;
            set => this.SetProperty(ref this.isUserActive, value, this.RaiseCanExecuteChanged);
        }

        public ICommand NetworkAdaptersCommand =>
            this.networkAdaptersCommand
            ??
            (this.networkAdaptersCommand = new DelegateCommand(
                () => this.ExecuteCommand(Menu.NetworkAdapters),
                this.CanExecuteCommand));

        public ICommand StatisticsCommand =>
                    this.statisticsCommand
            ??
            (this.statisticsCommand = new DelegateCommand(
                () => this.ExecuteCommand(Menu.Statistics),
                this.CanExecuteCommand));

        public ICommand UserCommand =>
            this.userCommand
            ??
            (this.userCommand = new DelegateCommand(
                () => this.ExecuteCommand(Menu.User),
                this.CanExecuteCommand));

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();
        }

        public override async Task OnAppearedAsync()
        {
            this.sessionService = ServiceLocator.Current.GetInstance<ISessionService>();

            var accessLevel = this.sessionService.UserAccessLevel;

            this.IsAdmin = accessLevel >= MAS.AutomationService.Contracts.UserAccessLevel.Admin;

            this.IsBackNavigationAllowed = true;

            this.IsAlarmActive = false;
            this.IsDiagnosticsActive = false;
            this.isUserActive = false;
            this.IsGeneralActive = false;
            this.IsStatisticsActive = false;
            this.IsNetworkAdaptersActive = false;
            this.IsInverterDiagnosticsActive = false;

            switch ((Menu)(this.Data ?? Menu.General))
            {
                case Menu.Alarm:
                    this.IsAlarmActive = true;
                    break;

                case Menu.General:
                    this.IsGeneralActive = true;
                    break;

                case Menu.Statistics:
                    this.IsStatisticsActive = true;
                    break;

                case Menu.Diagnostics:
                    this.IsDiagnosticsActive = true;
                    break;

                case Menu.User:
                    this.IsUserActive = true;
                    break;

                case Menu.NetworkAdapters:
                    this.IsNetworkAdaptersActive = true;
                    break;

                case Menu.InverterDiagnostics:
                    this.IsInverterDiagnosticsActive = true;
                    break;
            }

            await base.OnAppearedAsync();
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.alarm?.RaiseCanExecuteChanged();
            this.diagnosticsCommand?.RaiseCanExecuteChanged();
            this.userCommand?.RaiseCanExecuteChanged();
            this.generalCommand?.RaiseCanExecuteChanged();
            this.statisticsCommand?.RaiseCanExecuteChanged();
            this.networkAdaptersCommand?.RaiseCanExecuteChanged();
            this.inverterDiagnosticsCommand?.RaiseCanExecuteChanged();
        }

        private bool CanExecuteCommand()
        {
            return true; // temp 20200212 : prevent to display empty pages alarms, diagnostics and statistics
        }

        private bool CanExecuteDiagnosticCommand()
        {
            return this.sessionService.UserAccessLevel > MAS.AutomationService.Contracts.UserAccessLevel.Movement;
        }

        private bool CanExecuteUserCommand()
        {
            return this.sessionService.UserAccessLevel == MAS.AutomationService.Contracts.UserAccessLevel.Admin || this.sessionService.UserAccessLevel == MAS.AutomationService.Contracts.UserAccessLevel.Support;
        }

        private void ExecuteCommand(Menu menu)
        {
            switch (menu)
            {
                case Menu.Alarm:
                    this.NavigationService.Appear(
                        nameof(Utils.Modules.Operator),
                        Utils.Modules.Operator.About.ALARM,
                        data: menu,
                        trackCurrentView: false);
                    break;

                case Menu.General:
                    this.NavigationService.Appear(
                        nameof(Utils.Modules.Operator),
                        Utils.Modules.Operator.About.GENERAL,
                        data: menu,
                        trackCurrentView: false);
                    break;

                case Menu.Statistics:
                    this.NavigationService.Appear(
                        nameof(Utils.Modules.Operator),
                        Utils.Modules.Operator.About.STATISTICS,
                        data: menu,
                        trackCurrentView: false);
                    break;

                case Menu.Diagnostics:
                    this.NavigationService.Appear(
                        nameof(Utils.Modules.Operator),
                        Utils.Modules.Operator.About.DIAGNOSTICS,
                        data: menu,
                        trackCurrentView: false);
                    break;

                case Menu.User:
                    this.NavigationService.Appear(
                        nameof(Utils.Modules.Operator),
                        Utils.Modules.Operator.About.USER,
                        data: menu,
                        trackCurrentView: false);
                    break;

                case Menu.NetworkAdapters:
                    this.NavigationService.Appear(
                        nameof(Utils.Modules.Operator),
                        Utils.Modules.Operator.About.NETWORKADAPTERS,
                        data: menu,
                        trackCurrentView: false);
                    break;

                case Menu.InverterDiagnostics:
                    this.NavigationService.Appear(
                        nameof(Utils.Modules.Operator),
                        Utils.Modules.Operator.About.INVERTERDIAGNOSTICS,
                        data: menu,
                        trackCurrentView: false);
                    break;
            }
        }

        #endregion
    }
}
