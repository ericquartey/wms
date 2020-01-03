using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;

namespace Ferretto.VW.App.Menu.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal sealed class BaysMenuViewModel : BaseInstallationMenuViewModel
    {
        #region Fields

        private DelegateCommand bayControlCommand;

        private DelegateCommand bayHeightCommand;

        private DelegateCommand testShutterCommand;

        #endregion

        #region Constructors

        public BaysMenuViewModel()
            : base()
        {
        }

        #endregion

        #region Enums

        private enum Menu
        {
            BayControl,

            BayHeight,

            TestShutter,
        }

        #endregion

        #region Properties

        public ICommand BayControlCommand =>
            this.bayControlCommand
            ??
            (this.bayControlCommand = new DelegateCommand(
                () => this.ExecuteCommand(Menu.BayControl),
                () => this.CanExecuteCommand() && this.MachineService.IsHoming));

        public ICommand BayHeightCommand =>
            this.bayHeightCommand
            ??
            (this.bayHeightCommand = new DelegateCommand(
                () => this.ExecuteCommand(Menu.BayHeight),
                () => this.CanExecuteCommand() && this.MachineService.IsHoming));

        public override EnableMask EnableMask => EnableMask.Any;

        public bool IsTestBayVisible => this.MachineService.HasBayExternal || this.MachineService.HasCarousel;

        public ICommand TestShutterCommand =>
            this.testShutterCommand
            ??
            (this.testShutterCommand = new DelegateCommand(
                () => this.ExecuteCommand(Menu.TestShutter),
                this.CanExecuteCommand));

        #endregion

        #region Methods

        public async override Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();
            this.RaiseCanExecuteChanged();
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.bayControlCommand?.RaiseCanExecuteChanged();
            this.bayHeightCommand?.RaiseCanExecuteChanged();
            this.testShutterCommand?.RaiseCanExecuteChanged();
        }

        private void ExecuteCommand(Menu menu)
        {
            switch (menu)
            {
                case Menu.BayControl:
                    this.NavigationService.Appear(
                        nameof(Utils.Modules.Installation),
                        Utils.Modules.Installation.Bays.BAYHEIGHTCHECK,
                        data: null,
                        trackCurrentView: true);
                    break;

                case Menu.BayHeight:
                    this.NavigationService.Appear(
                        nameof(Utils.Modules.Installation),
                        Utils.Modules.Installation.ProfileHeightCheck.STEP1,
                        data: null,
                        trackCurrentView: true);
                    break;

                case Menu.TestShutter:
                    this.NavigationService.Appear(
                        nameof(Utils.Modules.Installation),
                        Utils.Modules.Installation.SHUTTERENDURANCETEST,
                        data: null,
                        trackCurrentView: true);
                    break;
            }
        }

        #endregion
    }
}
