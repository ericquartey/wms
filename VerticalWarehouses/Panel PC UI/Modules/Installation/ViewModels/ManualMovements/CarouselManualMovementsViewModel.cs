using System.Threading.Tasks;
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
            IMachineHomingProcedureService homingService)
            : base(homingService)
        {
            this.machineCarouselService = machineCarouselService;

            this.RefreshCanExecuteCommands();
        }

        #endregion

        #region Properties

        public bool CanExecuteCloseCommand
        {
            get => this.canExecuteCloseCommand;
            set => this.SetProperty(ref this.canExecuteCloseCommand, value);
        }

        public bool CanExecuteOpenCommand
        {
            get => this.canExecuteOpenCommand;
            set => this.SetProperty(ref this.canExecuteOpenCommand, value);
        }

        public DelegateCommand CloseCommand =>
            this.closeCommand
            ??
            (this.closeCommand = new DelegateCommand(async () => await this.CloseCarouselAsync()));

        public bool IsClosing
        {
            get => this.isClosing;
            set
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
            set
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
            set
            {
                if (this.SetProperty(ref this.isStopping, value))
                {
                    this.RefreshCanExecuteCommands();
                }
            }
        }

        public DelegateCommand OpenCommand =>
            this.openCommand
            ??
            (this.openCommand = new DelegateCommand(async () => await this.OpenCarouselAsync()));

        #endregion

        #region Methods

        public async Task CloseCarouselAsync()
        {
            this.IsClosing = true;
            this.IsOpening = false;

            await this.StartMovementAsync(-100);
        }

        public async Task OpenCarouselAsync()
        {
            this.IsOpening = true;
            this.IsClosing = false;

            await this.StartMovementAsync(100);
        }

        protected async Task StartMovementAsync(decimal displacement)
        {
            try
            {
                await this.machineCarouselService.MoveAsync(
                    new CarouselMovementParameters
                    {
                        MovementType = MovementType.Absolute,
                        SpeedPercentage = 50,
                        Displacement = displacement
                    });
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
