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

        private DelegateCommand allarmCommand;

        private DelegateCommand diagnosticsCommand;

        private DelegateCommand generalCommand;

        private bool isAlarmActive;

        private bool isDiagnosticsActive;

        private bool isGeneralActive;

        private bool isStatisticsActive;

        private bool isWaitingForResponse;

        private DelegateCommand statisticsCommand;

        #endregion

        #region Constructors

        public BaseAboutMenuViewModel()
            : base(PresentationMode.Operator)
        {
        }

        #endregion

        #region Enums

        private enum Menu
        {
            Allarm,

            General,

            Statistics,

            Diagnostincs,
        }

        #endregion

        #region Properties

        public ICommand AllarmCommand =>
            this.allarmCommand
            ??
            (this.allarmCommand = new DelegateCommand(
                () => this.ExecuteCommand(Menu.Allarm),
                this.CanExecuteCommand));

        public ICommand DiagnosticsCommand =>
            this.diagnosticsCommand
            ??
            (this.diagnosticsCommand = new DelegateCommand(
                () => this.ExecuteCommand(Menu.Diagnostincs),
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

        public bool IsWaitingForResponse
        {
            get => this.isWaitingForResponse;
            set => this.SetProperty(ref this.isWaitingForResponse, value, this.RaiseCanExecuteChanged);
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
            this.IsWaitingForResponse = true;

            await base.OnAppearedAsync();

            if (this.IsVisible)
            {
                this.IsAlarmActive = false;
                this.IsDiagnosticsActive = false;
                this.IsGeneralActive = false;
                this.IsStatisticsActive = false;

                switch ((Menu)(this.Data ?? Menu.General))
                {
                    case Menu.Allarm:
                        this.IsAlarmActive = true;
                        break;

                    case Menu.General:
                        this.IsGeneralActive = true;
                        break;

                    case Menu.Statistics:
                        this.IsStatisticsActive = true;
                        break;

                    case Menu.Diagnostincs:
                        this.IsDiagnosticsActive = true;
                        break;
                }
            }

            this.IsBackNavigationAllowed = true;

            this.IsWaitingForResponse = false;
        }

        private bool CanExecuteCommand()
        {
            return !this.IsWaitingForResponse;
        }

        private void ExecuteCommand(Menu menu)
        {
            switch (menu)
            {
                case Menu.Allarm:
                    this.NavigationService.Appear(
                        nameof(Utils.Modules.Operator),
                        Utils.Modules.Operator.About.ALLARM,
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

                case Menu.Diagnostincs:
                    this.NavigationService.Appear(
                        nameof(Utils.Modules.Operator),
                        Utils.Modules.Operator.About.DIAGNOSTICS,
                        data: menu,
                        trackCurrentView: false);
                    break;
            }
        }

        private void RaiseCanExecuteChanged()
        {
            this.allarmCommand?.RaiseCanExecuteChanged();
            this.diagnosticsCommand?.RaiseCanExecuteChanged();
            this.generalCommand?.RaiseCanExecuteChanged();
            this.statisticsCommand?.RaiseCanExecuteChanged();
        }

        #endregion
    }
}
