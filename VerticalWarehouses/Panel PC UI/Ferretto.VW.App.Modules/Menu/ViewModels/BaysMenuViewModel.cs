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
    internal sealed class BaysMenuViewModel : BaseMainViewModel
    {
        #region Fields

        private DelegateCommand bayControlCommand;

        private DelegateCommand bayHeightCommand;

        private bool isWaitingForResponse;

        private DelegateCommand testShutterCommand;

        #endregion

        #region Constructors

        public BaysMenuViewModel()
            : base(PresentationMode.Menu)
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
                this.CanExecuteCommand));

        public ICommand BayHeightCommand =>
            this.bayHeightCommand
            ??
            (this.bayHeightCommand = new DelegateCommand(
                () => this.ExecuteCommand(Menu.BayHeight),
                this.CanExecuteCommand));

        public override EnableMask EnableMask => EnableMask.Any;

        public bool IsWaitingForResponse
        {
            get => this.isWaitingForResponse;
            set => this.SetProperty(ref this.isWaitingForResponse, value, this.RaiseCanExecuteChanged);
        }

        public ICommand TestShutterCommand =>
            this.testShutterCommand
            ??
            (this.testShutterCommand = new DelegateCommand(
                () => this.ExecuteCommand(Menu.TestShutter),
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

        private void RaiseCanExecuteChanged()
        {
            this.bayControlCommand?.RaiseCanExecuteChanged();
            this.bayHeightCommand?.RaiseCanExecuteChanged();
            this.testShutterCommand?.RaiseCanExecuteChanged();
        }

        #endregion
    }
}
