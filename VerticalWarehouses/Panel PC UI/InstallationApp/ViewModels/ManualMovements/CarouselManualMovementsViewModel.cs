using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public class CarouselManualMovementsViewModel : BasePositioningViewModel
    {
        #region Fields

        private DelegateCommand closeCarouselCommand;

        private DelegateCommand openCarouselCommand;

        #endregion

        #region Constructors

        public CarouselManualMovementsViewModel(IPositioningMachineService positioningService)
            : base(positioningService)
        {
        }

        #endregion

        #region Properties

        public DelegateCommand CloseCarouselCommand =>
            this.closeCarouselCommand
            ??
            (this.closeCarouselCommand = new DelegateCommand(async () => await this.CloseCarouselAsync()));

        public DelegateCommand OpenCarouselCommand =>
            this.openCarouselCommand
            ??
            (this.openCarouselCommand = new DelegateCommand(async () => await this.OpenCarouselAsync()));

        #endregion

        #region Methods

        public async Task CloseCarouselAsync()
        {
            var messageData = CreateMovementMessageData(-100);

            await this.StartPositioningAsync(messageData);
        }

        public async Task OpenCarouselAsync()
        {
            var messageData = CreateMovementMessageData(100);

            await this.StartPositioningAsync(messageData);
        }

        private static MovementMessageDataDto CreateMovementMessageData(decimal displacement)
        {
            return new MovementMessageDataDto
            {
                Axis = Axis.Both,
                MovementType = MovementType.Absolute,
                SpeedPercentage = 50,
                Displacement = displacement
            };
        }

        #endregion
    }
}
