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

        private readonly IMachineCarouselService machineCarouselService;

        private DelegateCommand carouselDownCommand;

        private DelegateCommand carouselUpCommand;

        private bool isCarouselMoving;

        #endregion

        #region Properties

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

        public bool HasCarousel
        {
            get
            {
                var bay = this.bayManagerService.Bay;
                return bay.Type == BayType.Carousel
                        ||
                       bay.Type == BayType.ExternalCarousel;
            }
        }

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
              !this.IsWaitingForResponse
             //&&
             //(this.Sensors.LUPresentMiddleBottomBay1); //IoStatus.LoadingUnitExistenceInBay
        }

        private bool CanExecuteCarouselUpCommand()
        {
            return !this.IsElevatorMoving
              &&
              !this.IsWaitingForResponse;
            // &&
            // IoStatus.LoadingUnitInLowerBay;
        }

        private async Task CarouselDownAsync()
        {
            this.IsWaitingForResponse = true;

            try
            {
                await this.machineCarouselService.MoveAsync(HorizontalMovementDirection.Backwards);
                this.IsCarouselMoving = true;
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
                await this.machineCarouselService.MoveAsync(HorizontalMovementDirection.Forwards);
                this.IsCarouselMoving = true;
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
