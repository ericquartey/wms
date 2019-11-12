using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Installation.ViewModels
{
    internal sealed partial class SemiAutoMovementsViewModel
    {
        #region Fields

        private DelegateCommand closedShutterCommand;

        private DelegateCommand intermediateShutterCommand;

        private bool isShutterMoving;

        private DelegateCommand openShutterCommand;

        #endregion

        #region Properties

        public ICommand ClosedShutterCommand =>
            this.closedShutterCommand
            ??
            (this.closedShutterCommand = new DelegateCommand(
                async () => await this.ClosedShutterAsync(),
                this.CanCloseShutter));

        public ICommand IntermediateShutterCommand =>
            this.intermediateShutterCommand
            ??
            (this.intermediateShutterCommand = new DelegateCommand(
                async () => await this.IntermediateShutterAsync(),
                this.CanExecuteIntermediateCommand));

        public bool IsShutterMoving
        {
            get => this.isShutterMoving;
            private set
            {
                if (this.SetProperty(ref this.isShutterMoving, value))
                {
                    this.RaisePropertyChanged(nameof(this.IsShutterMoving));
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand OpenShutterCommand =>
            this.openShutterCommand
            ??
            (this.openShutterCommand = new DelegateCommand(
                async () => await this.OpenShutterAsync(),
                this.CanOpenShutter));

        #endregion

        #region Methods

        private bool CanCloseShutter()
        {
            return
                !this.IsWaitingForResponse
                &&
                !this.IsMoving
                &&
                !this.IsShutterMoving
                &&
                (this.sensorsService.ShutterSensors != null && (this.sensorsService.ShutterSensors.Open || this.sensorsService.ShutterSensors.MidWay));
        }

        private bool CanExecuteIntermediateCommand()
        {
            return
                !this.IsWaitingForResponse
                &&
                !this.IsMoving
                &&
                !this.IsShutterMoving
                &&
                this.BayIsShutterThreeSensors
                &&
                (this.sensorsService.ShutterSensors != null && (this.sensorsService.ShutterSensors.Open || this.sensorsService.ShutterSensors.Closed));
        }

        private bool CanOpenShutter()
        {
            return
                !this.IsWaitingForResponse
                &&
                !this.IsMoving
                &&
                !this.IsShutterMoving
                &&
                (this.sensorsService.ShutterSensors != null && (this.sensorsService.ShutterSensors.Closed || this.sensorsService.ShutterSensors.MidWay));
        }

        private async Task ClosedShutterAsync()
        {
            this.IsWaitingForResponse = true;

            try
            {
                await this.shuttersWebService.MoveToAsync(ShutterPosition.Closed);
                this.IsShutterMoving = true;
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

        private async Task IntermediateShutterAsync()
        {
            this.IsWaitingForResponse = true;

            try
            {
                await this.shuttersWebService.MoveToAsync(ShutterPosition.Half);
                this.IsShutterMoving = true;
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

        private async Task OpenShutterAsync()
        {
            this.IsWaitingForResponse = true;

            try
            {
                await this.shuttersWebService.MoveToAsync(ShutterPosition.Opened);
                this.IsShutterMoving = true;
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

        private void RaiseCanExecuteChanged1()
        {
            this.openShutterCommand?.RaiseCanExecuteChanged();
            this.closedShutterCommand?.RaiseCanExecuteChanged();
        }

        #endregion
    }
}
