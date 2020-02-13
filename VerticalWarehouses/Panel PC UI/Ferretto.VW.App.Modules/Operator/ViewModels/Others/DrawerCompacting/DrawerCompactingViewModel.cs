using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;

namespace Ferretto.VW.App.Operator.ViewModels
{
    [Warning(WarningsArea.Maintenance)]
    public class DrawerCompactingViewModel : BaseOperatorViewModel
    {
        #region Fields

        private readonly IMachineCompactingWebService machineCompactingWebService;

        private DelegateCommand compactingStartCommand;

        private DelegateCommand compactingStopCommand;

        private DelegateCommand detailButtonCommand;

        private bool isStopPressed;

        #endregion

        #region Constructors

        public DrawerCompactingViewModel(
            IMachineCompactingWebService machineCompactingWebService)
            : base(PresentationMode.Operator)
        {
            this.machineCompactingWebService = machineCompactingWebService ?? throw new ArgumentNullException(nameof(machineCompactingWebService));
        }

        #endregion

        #region Properties

        public ICommand CompactingStartCommand =>
            this.compactingStartCommand
            ??
            (this.compactingStartCommand =
                new DelegateCommand(
                    async () => await this.StartAsync(),
                    this.CanCompactingStart));

        public ICommand CompactingStopCommand =>
            this.compactingStopCommand
            ??
            (this.compactingStopCommand =
                new DelegateCommand(
                    async () => await this.StopAsync(),
                    this.CanCompactingStop));

        public ICommand DetailButtonCommand =>
            this.detailButtonCommand
            ??
            (this.detailButtonCommand =
                new DelegateCommand(
                    () => this.Detail(),
                    this.CanDetailCommand));

        public override EnableMask EnableMask => EnableMask.Any;

        public bool IsStopPressed
        {
            get => this.isStopPressed;
            protected set => this.SetProperty(ref this.isStopPressed, value, this.RaiseCanExecuteChanged);
        }

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            this.IsBackNavigationAllowed = true;

            await base.OnAppearedAsync();
        }

        protected override async Task OnMachineStatusChangedAsync(MachineStatusChangedMessage e)
        {
            await base.OnMachineStatusChangedAsync(e);

            if (!this.IsMoving)
            {
                this.IsStopPressed = false;
            }
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.detailButtonCommand?.RaiseCanExecuteChanged();
            this.compactingStartCommand?.RaiseCanExecuteChanged();
            this.compactingStopCommand?.RaiseCanExecuteChanged();
        }

        private bool CanCompactingStart()
        {
            return !this.IsWaitingForResponse &&
                   this.MachineModeService.MachineMode == MachineMode.Manual &&
                   this.MachineService.MachinePower == MachinePowerState.Powered &&
                   !this.IsMoving;
        }

        private bool CanCompactingStop()
        {
            return !this.IsWaitingForResponse &&
                   this.IsMoving &&
                   !this.IsStopPressed;
        }

        private bool CanDetailCommand()
        {
            return !this.IsWaitingForResponse;
        }

        private void Detail()
        {
            this.IsWaitingForResponse = true;

            try
            {
                this.NavigationService.Appear(
                    nameof(Utils.Modules.Operator),
                    Utils.Modules.Operator.Others.DrawerCompacting.DETAIL,
                    null,
                    trackCurrentView: true);
            }
            catch (System.Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private async Task StartAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                await this.machineCompactingWebService.CompactingAsync();
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private async Task StopAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                await this.machineCompactingWebService.StopAsync();

                this.IsStopPressed = true;
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        #endregion
    }
}
