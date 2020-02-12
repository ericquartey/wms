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
            this.compactingStartCommand ??
            (this.compactingStartCommand = new DelegateCommand(async () => this.StartAsync(), this.CanCompactingStart));

        public ICommand CompactingStopCommand =>
            this.compactingStopCommand ??
            (this.compactingStopCommand = new DelegateCommand(async () => this.StopAsync(), this.CanCompactingStop));

        public ICommand DetailButtonCommand =>
                            this.detailButtonCommand ??
            (this.detailButtonCommand = new DelegateCommand(() => this.Detail(), this.CanDetailCommand));

        public override EnableMask EnableMask => EnableMask.MachineManualMode | EnableMask.MachinePoweredOn;

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            this.IsWaitingForResponse = true;

            await base.OnAppearedAsync();

            this.IsBackNavigationAllowed = true;

            this.IsWaitingForResponse = false;
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
                   !this.IsMoving;
        }

        private bool CanCompactingStop()
        {
            return !this.IsWaitingForResponse &&
                   this.IsMoving;
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
