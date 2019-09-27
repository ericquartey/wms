using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Regions;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public class ManualMovementsViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IBayManager bayManager;

        private readonly CarouselManualMovementsViewModel carouselManualMovementsViewModel;

        private readonly EngineManualMovementsViewModel engineManualMovementsViewModel;

        private readonly ExternalBayManualMovementsViewModel externalBayManualMovementsViewModel;

        private readonly IMachineBaysService machineBayService;

        private readonly IMachineCarouselService machineCarouselService;

        private readonly IMachineElevatorService machineElevatorService;

        private readonly IMachineShuttersService machineShutterService;

        private readonly ShutterEngineManualMovementsViewModel shutterEngineManualMovementsViewModel;

        #endregion

        #region Constructors

        public ManualMovementsViewModel(
            IMachineShuttersService shutterService,
            IMachineCarouselService machineCarouselService,
            IMachineElevatorService machineElevatorService,
            IMachineBaysService machineBayService,
            IBayManager bayManager)
            : base(PresentationMode.Installer)
        {
            if (shutterService == null)
            {
                throw new System.ArgumentNullException(nameof(shutterService));
            }

            if (machineCarouselService == null)
            {
                throw new System.ArgumentNullException(nameof(machineCarouselService));
            }

            if (machineElevatorService == null)
            {
                throw new System.ArgumentNullException(nameof(machineElevatorService));
            }

            if (machineBayService == null)
            {
                throw new System.ArgumentNullException(nameof(machineBayService));
            }

            if (bayManager == null)
            {
                throw new System.ArgumentNullException(nameof(bayManager));
            }

            this.machineShutterService = shutterService;
            this.machineCarouselService = machineCarouselService;
            this.machineElevatorService = machineElevatorService;
            this.machineBayService = machineBayService;
            this.bayManager = bayManager;

            this.carouselManualMovementsViewModel = new CarouselManualMovementsViewModel(this.machineCarouselService, this.machineElevatorService, this.bayManager);
            this.engineManualMovementsViewModel = new EngineManualMovementsViewModel(this.machineElevatorService, this.bayManager);
            this.externalBayManualMovementsViewModel = new ExternalBayManualMovementsViewModel(this.machineElevatorService, this.machineBayService, this.bayManager);
            this.shutterEngineManualMovementsViewModel = new ShutterEngineManualMovementsViewModel(this.machineShutterService, this.machineElevatorService, this.bayManager);
        }

        #endregion

        #region Properties

        public CarouselManualMovementsViewModel CarouselManualMovementsViewModel => this.carouselManualMovementsViewModel;

        public EngineManualMovementsViewModel EngineManualMovementsViewModel => this.engineManualMovementsViewModel;

        public ExternalBayManualMovementsViewModel ExternalBayManualMovementsViewModel => this.externalBayManualMovementsViewModel;

        public bool HasCarousel => this.bayManager.Bay.Type == BayType.Carousel;

        public bool IsBayExternal => this.bayManager.Bay.IsExternal;

        public ShutterEngineManualMovementsViewModel ShutterEngineManualMovementsViewModel => this.shutterEngineManualMovementsViewModel;

        #endregion

        #region Methods

        public override async Task OnNavigatedAsync()
        {
            await base.OnNavigatedAsync();
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);
        }

        public override void UpdateNotifications()
        {
            this.ShowNotification(InstallationApp.LSMTComandNote, Services.Models.NotificationSeverity.Info);
        }

        #endregion
    }
}
