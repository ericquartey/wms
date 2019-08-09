using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public class HorizontalAxisManualMovementsViewModel : BasePositioningViewModel
    {
        #region Fields

        private DelegateCommand moveBackwardsCommand;

        private DelegateCommand moveForwardsCommand;

        #endregion

        #region Constructors

        public HorizontalAxisManualMovementsViewModel(IPositioningMachineService positioningService)
            : base(positioningService)
        {
        }

        #endregion

        #region Properties

        public DelegateCommand MoveBackwardsCommand =>
            this.moveBackwardsCommand
            ??
            (this.moveBackwardsCommand = new DelegateCommand(async () => await this.MoveBackwardsAsync()));

        public DelegateCommand MoveForwardButtonCommand =>
            this.moveForwardsCommand
            ??
            (this.moveForwardsCommand = new DelegateCommand(async () => await this.MoveForwardsAsync()));

        #endregion

        #region Methods

        private static MovementMessageDataDto CreateMovementMessageData(decimal displacement)
        {
            return new MovementMessageDataDto
            {
                Axis = Axis.Horizontal,
                MovementType = MovementType.Relative,
                SpeedPercentage = 0,
                Displacement = displacement
            };
        }

        private async Task MoveBackwardsAsync()
        {
            var messageData = CreateMovementMessageData(-1);

            await this.StartPositioningAsync(messageData);
        }

        private async Task MoveForwardsAsync()
        {
            var messageData = CreateMovementMessageData(1);

            await this.StartPositioningAsync(messageData);
        }

        #endregion
    }
}
