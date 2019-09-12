using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Modules.Installation.Models;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public partial class SemiAutoMovementsViewModel
    {
        #region Fields

        private DelegateCommand closedShutterCommand;

        private DelegateCommand intermediateShutterCommand;

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

        public bool IsOpenShutterCommand => false;

        public ICommand OpenShutterCommand =>
                                    this.openShutterCommand
            ??
            (this.openShutterCommand = new DelegateCommand(
                async () => await this.OpenShutterAsync(),
                this.CanExecuteOpenCommand));

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

                // && this.Sensors.Open
                ;
        }

        private bool CanExecuteIntermediateCommand()
        {
            return !this.IsElevatorMoving

                // && this.Sensors.Open
                ;
        }

        private bool CanExecuteOpenCommand()
        {
            return !this.IsElevatorMoving

                // && this.Sensors.Closed
                ;
        }

        private async Task ClosedShutterAsync()
        {
            this.IsWaitingForResponse = true;

            try
            {
                await this.shuttersService.MoveAsync(this.BayNumber, ShutterMovementDirection.Down);
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
                await this.shuttersService.MoveAsync(this.BayNumber, ShutterMovementDirection.Up);
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
                await this.shuttersService.MoveAsync(this.BayNumber, ShutterMovementDirection.Up);
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
