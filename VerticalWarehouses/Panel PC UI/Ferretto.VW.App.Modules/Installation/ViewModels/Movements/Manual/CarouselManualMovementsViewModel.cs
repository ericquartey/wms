using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public class CarouselManualMovementsViewModel : BaseManualMovementsViewModel
    {
        #region Fields

        private readonly IMachineCarouselService machineCarouselService;

        private bool canExecuteCloseCommand;

        private bool canExecuteOpenCommand;

        private DelegateCommand closeCommand;

        private bool isClosing;

        private bool isOpening;

        private bool isStopping;

        private DelegateCommand openCommand;

        #endregion

        #region Constructors

        public CarouselManualMovementsViewModel(
            IMachineCarouselService machineCarouselService,
            IMachineElevatorService machineElevatorService,
            IBayManager bayManagerService)
            : base(machineElevatorService, bayManagerService)
        {
            this.machineCarouselService = machineCarouselService;

            this.RefreshCanExecuteCommands();
        }

        #endregion

        #region Properties

        public bool CanExecuteCloseCommand
        {
            get => this.canExecuteCloseCommand;
            private set => this.SetProperty(ref this.canExecuteCloseCommand, value);
        }

        public bool CanExecuteOpenCommand
        {
            get => this.canExecuteOpenCommand;
            private set => this.SetProperty(ref this.canExecuteOpenCommand, value);
        }

        public ICommand CloseCommand =>
            this.closeCommand
            ??
            (this.closeCommand = new DelegateCommand(async () => await this.CloseCarouselAsync()));

        public bool IsClosing
        {
            get => this.isClosing;
            private set
            {
                if (this.SetProperty(ref this.isClosing, value))
                {
                    this.RefreshCanExecuteCommands();
                }
            }
        }

        public bool IsOpening
        {
            get => this.isOpening;
            private set
            {
                if (this.SetProperty(ref this.isOpening, value))
                {
                    this.RefreshCanExecuteCommands();
                }
            }
        }

        public bool IsStopping
        {
            get => this.isStopping;
            private set
            {
                if (this.SetProperty(ref this.isStopping, value))
                {
                    this.RefreshCanExecuteCommands();
                }
            }
        }

        public ICommand OpenCommand =>
            this.openCommand
            ??
            (this.openCommand = new DelegateCommand(async () => await this.OpenCarouselAsync()));

        #endregion

        #region Methods

        public async Task CloseCarouselAsync()
        {
            this.IsClosing = true;
            this.IsOpening = false;

            this.DisableAllExceptThis();

            await this.StartMovementAsync(HorizontalMovementDirection.Backwards);
        }

        public async Task OpenCarouselAsync()
        {
            this.IsOpening = true;
            this.IsClosing = false;

            this.DisableAllExceptThis();

            await this.StartMovementAsync(HorizontalMovementDirection.Forwards);
        }

        protected async Task StartMovementAsync(HorizontalMovementDirection direction)
        {
            try
            {
                await this.machineCarouselService.MoveAsync(direction);
            }
            catch (System.Exception ex)
            {
                this.IsClosing = false;
                this.IsOpening = false;

                this.ShowNotification(ex);
            }
        }

        protected override async Task StopMovementAsync()
        {
            try
            {
                this.IsStopping = true;

                await this.machineCarouselService.StopAsync();
            }
            catch (System.Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsOpening = false;
                this.IsClosing = false;
                this.IsStopping = false;
                this.EnableAll();
            }
        }

        private void RefreshCanExecuteCommands()
        {
            this.CanExecuteCloseCommand = !this.IsOpening && !this.IsStopping;
            this.CanExecuteOpenCommand = !this.IsClosing && !this.IsStopping;
        }

        #endregion
    }
}
