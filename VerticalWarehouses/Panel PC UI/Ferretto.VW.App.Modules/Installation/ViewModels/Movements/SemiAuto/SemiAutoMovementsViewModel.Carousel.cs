using System.Linq;
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

        private DelegateCommand carouselDownCommand;

        private DelegateCommand carouselUpCommand;

        private bool isCarouselMoving;

        #endregion

        #region Properties

        public bool BayIsCarousel => this.bayManagerService.Bay.Positions.Count() > 1;

        public ICommand CarouselDownCommand =>
                    this.carouselDownCommand
            ??
            (this.carouselDownCommand = new DelegateCommand(
                async () => await this.CarouselDownAsync(),
                this.CanExecuteCarouselDownCommand));

        public ICommand CarouselUpCommand =>
                    this.carouselUpCommand
            ??
            (this.carouselUpCommand = new DelegateCommand(
                async () => await this.CarouselUpAsync(),
                this.CanExecuteCarouselUpCommand));

        public bool IsCarouselMoving
        {
            get => this.isCarouselMoving;
            private set
            {
                if (this.SetProperty(ref this.isCarouselMoving, value))
                {
                    this.RaisePropertyChanged(nameof(this.IsCarouselMoving));
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        #endregion

        #region Methods

        private bool CanExecuteCarouselDownCommand()
        {
            return !this.IsElevatorMoving
                &&
                !this.IsCarouselMoving;
            // &&
            // (this.ShutterSensors.Closed || this.ShutterSensors.MidWay);
        }

        private bool CanExecuteCarouselUpCommand()
        {
            return !this.IsElevatorMoving
                &&
                !this.IsCarouselMoving;
            // &&
            // (this.ShutterSensors.Open || this.ShutterSensors.MidWay);
        }

        private async Task CarouselDownAsync()
        {
            this.IsWaitingForResponse = true;

            try
            {
                await this.shuttersService.MoveToAsync(this.BayNumber, ShutterPosition.Opened);
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

        private async Task CarouselUpAsync()
        {
            this.IsWaitingForResponse = true;

            try
            {
                await this.shuttersService.MoveToAsync(this.BayNumber, ShutterPosition.Closed);
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
