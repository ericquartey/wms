using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Prism.Commands;

namespace Ferretto.VW.App.Operator.ViewModels
{
    internal abstract class BaseAboutMenuViewModel : BaseMainViewModel
    {
        #region Fields

        private DelegateCommand alarm;

        private DelegateCommand diagnosticsCommand;

        private DelegateCommand generalCommand;

        private bool isAlarmActive;

        private bool isDiagnosticsActive;

        private bool isGeneralActive;

        private bool isStatisticsActive;

        private DelegateCommand statisticsCommand;

        #endregion

        #region Constructors

        public BaseAboutMenuViewModel()
            : base(PresentationMode.Menu)
        {
        }

        #endregion

        #region Enums

        private enum Menu
        {
            Alarm,

            General,

            Statistics,

            Diagnostics,
        }

        #endregion

        #region Properties

        public ICommand AlarmCommand =>
            this.alarm
            ??
            (this.alarm = new DelegateCommand(
                () => this.ExecuteCommand(Menu.Alarm),
                this.CanExecuteCommand));

        public ICommand DiagnosticsCommand =>
            this.diagnosticsCommand
            ??
            (this.diagnosticsCommand = new DelegateCommand(
                () => this.ExecuteCommand(Menu.Diagnostics),
                this.CanExecuteCommand));

        public override EnableMask EnableMask => EnableMask.Any;

        public ICommand GeneralCommand =>
            this.generalCommand
            ??
            (this.generalCommand = new DelegateCommand(
                () => this.ExecuteCommand(Menu.General),
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

        public bool IsGeneralActive
        {
            get => this.isGeneralActive;
            set => this.SetProperty(ref this.isGeneralActive, value, this.RaiseCanExecuteChanged);
        }

        public bool IsStatisticsActive
        {
            get => this.isStatisticsActive;
            set => this.SetProperty(ref this.isStatisticsActive, value, this.RaiseCanExecuteChanged);
        }

        public ICommand StatisticsCommand =>
            this.statisticsCommand
            ??
            (this.statisticsCommand = new DelegateCommand(
                () => this.ExecuteCommand(Menu.Statistics),
                this.CanExecuteCommand));

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();
        }

        public override async Task OnAppearedAsync()
        {
            this.IsBackNavigationAllowed = true;

            this.IsAlarmActive = false;
            this.IsDiagnosticsActive = false;
            this.IsGeneralActive = false;
            this.IsStatisticsActive = false;

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
            }

            await base.OnAppearedAsync();
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.alarm?.RaiseCanExecuteChanged();
            this.diagnosticsCommand?.RaiseCanExecuteChanged();
            this.generalCommand?.RaiseCanExecuteChanged();
            this.statisticsCommand?.RaiseCanExecuteChanged();
        }

        private bool CanExecuteCommand()
        {
            return true;
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
            }
        }

        #endregion
    }
}
