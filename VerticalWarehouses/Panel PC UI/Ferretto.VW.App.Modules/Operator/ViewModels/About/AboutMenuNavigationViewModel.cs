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
    internal sealed class AboutMenuNavigationViewModel : BaseMainViewModel
    {
        #region Fields

        private DelegateCommand allarmCommand;

        private DelegateCommand countersCommand;

        private DelegateCommand diagnosticsCommand;

        private DelegateCommand generalCommand;

        private bool isWaitingForResponse;

        private DelegateCommand statisticsCommand;

        #endregion

        #region Constructors

        public AboutMenuNavigationViewModel()
            : base(PresentationMode.Operator)
        {
        }

        #endregion

        #region Enums

        private enum Menu
        {
            Allarm,

            Counters,

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

        public ICommand CountersCommand =>
            this.countersCommand
            ??
            (this.countersCommand = new DelegateCommand(
                () => this.ExecuteCommand(Menu.Counters),
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
                        data: null,
                        trackCurrentView: false);
                    break;

                case Menu.Counters:
                    this.NavigationService.Appear(
                        nameof(Utils.Modules.Operator),
                        Utils.Modules.Operator.About.COUNTERS,
                        data: null,
                        trackCurrentView: false);
                    break;

                case Menu.General:
                    this.NavigationService.Appear(
                        nameof(Utils.Modules.Operator),
                        Utils.Modules.Operator.About.GENERAL,
                        data: null,
                        trackCurrentView: false);
                    break;

                case Menu.Statistics:
                    this.NavigationService.Appear(
                        nameof(Utils.Modules.Operator),
                        Utils.Modules.Operator.About.STATISTICS,
                        data: null,
                        trackCurrentView: false);
                    break;

                case Menu.Diagnostincs:
                    this.NavigationService.Appear(
                        nameof(Utils.Modules.Operator),
                        Utils.Modules.Operator.About.DIAGNOSTICS,
                        data: null,
                        trackCurrentView: false);
                    break;
            }
        }

        private void RaiseCanExecuteChanged()
        {
            this.allarmCommand?.RaiseCanExecuteChanged();
            this.countersCommand?.RaiseCanExecuteChanged();
            this.generalCommand?.RaiseCanExecuteChanged();
            this.statisticsCommand?.RaiseCanExecuteChanged();
        }

        #endregion
    }
}
