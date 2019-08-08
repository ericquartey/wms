using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public class VerticalEngineManualMovementsViewModel : BasePositioningViewModel
    {
        #region Fields

        private DelegateCommand moveDownCommand;

        private DelegateCommand moveUpCommand;

        #endregion

        #region Constructors

        public VerticalEngineManualMovementsViewModel(IPositioningMachineService positioningService)
            : base(positioningService)
        {
        }

        #endregion

        #region Properties

        public DelegateCommand MoveDownCommand =>
            this.moveDownCommand
            ??
            (this.moveDownCommand = new DelegateCommand(async () => await this.MoveDownAsync()));

        public DelegateCommand MoveUpCommand =>
            this.moveUpCommand
            ??
            (this.moveUpCommand = new DelegateCommand(async () => await this.MoveUpAsync()));

        #endregion

        #region Methods

        public async Task MoveDownAsync()
        {
            var messageData = CreatePositioningMessageData(-1.0m);

            await this.StartPositioningAsync(messageData);
        }

        public async Task MoveUpAsync()
        {
            var messageData = CreatePositioningMessageData(1.0m);

            await this.StartPositioningAsync(messageData);
        }

        private static MovementMessageDataDto CreatePositioningMessageData(decimal displacement)
        {
            return new MovementMessageDataDto
            {
                Axis = Axis.Vertical,
                MovementType = MovementType.Relative,
                SpeedPercentage = 0,
                Displacement = displacement
            };
        }

        #endregion
    }
}
