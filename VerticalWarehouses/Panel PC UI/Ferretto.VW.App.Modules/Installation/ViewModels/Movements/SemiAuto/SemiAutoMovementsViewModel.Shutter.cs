using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Modules.Installation.Models;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public partial class SemiAutoMovementsViewModel
    {
        #region Fields

        private readonly ShutterSensors shutterSensors;

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
                this.CanExecuteClosedCommand));

        public ICommand IntermediateShutterCommand =>
            this.intermediateShutterCommand
            ??
            (this.intermediateShutterCommand = new DelegateCommand(
                async () => await this.IntermediateShutterAsync(),
                this.CanExecuteIntermediateCommand));

        public bool IsClosedShutterCommand => false;

        public bool IsIntermediateShutterCommand => false;

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
                this.CanExecuteOpenCommand));

        public ShutterSensors ShutterSensors => this.shutterSensors;

        #endregion

        #region Methods

        protected void RaiseCanExecuteChanged1()
        {
            this.openShutterCommand?.RaiseCanExecuteChanged();
            this.closedShutterCommand?.RaiseCanExecuteChanged();
        }

        private bool CanExecuteClosedCommand()
        {
            return !this.IsElevatorMoving
                &&
                !this.IsShutterMoving
                &&
                (this.ShutterSensors.Open || this.ShutterSensors.MidWay);
        }

        private bool CanExecuteIntermediateCommand()
        {
            return !this.IsElevatorMoving
                &&
                !this.IsShutterMoving
                &&
                (this.ShutterSensors.Open || this.ShutterSensors.Closed);
        }

        private bool CanExecuteOpenCommand()
        {
            return !this.IsElevatorMoving
                &&
                !this.IsShutterMoving
                &&
                (this.ShutterSensors.Closed || this.ShutterSensors.MidWay);
        }

        private async Task ClosedShutterAsync()
        {
            this.IsWaitingForResponse = true;

            try
            {
                await this.shuttersService.MoveToAsync(ShutterPosition.Closed);
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
                await this.shuttersService.MoveToAsync(ShutterPosition.Half);
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
                await this.shuttersService.MoveToAsync(ShutterPosition.Opened);
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

        #endregion
    }
}
