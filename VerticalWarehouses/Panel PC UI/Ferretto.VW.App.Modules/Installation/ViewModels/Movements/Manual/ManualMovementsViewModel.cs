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

        private readonly IMachineBaysWebService machineBayWebService;

        private readonly IMachineCarouselWebService machineCarouselWebService;

        private readonly IMachineElevatorWebService machineElevatorWebService;

        private readonly IMachineShuttersWebService machineShutterWebService;

        private readonly ShutterEngineManualMovementsViewModel shutterEngineManualMovementsViewModel;

        #endregion

        #region Constructors

        public ManualMovementsViewModel(
            IMachineShuttersWebService shutterWebService,
            IMachineCarouselWebService machineCarouselWebService,
            IMachineElevatorWebService machineElevatorWebService,
            IMachineBaysWebService machineBayWebService,
            IBayManager bayManager)
            : base(PresentationMode.Installer)
        {
            if (shutterWebService == null)
            {
                throw new System.ArgumentNullException(nameof(shutterWebService));
            }

            if (machineCarouselWebService == null)
            {
                throw new System.ArgumentNullException(nameof(machineCarouselWebService));
            }

            if (machineElevatorWebService == null)
            {
                throw new System.ArgumentNullException(nameof(machineElevatorWebService));
            }

            if (machineBayWebService == null)
            {
                throw new System.ArgumentNullException(nameof(machineBayWebService));
            }

            if (bayManager == null)
            {
                throw new System.ArgumentNullException(nameof(bayManager));
            }

            this.machineShutterWebService = shutterWebService;
            this.machineCarouselWebService = machineCarouselWebService;
            this.machineElevatorWebService = machineElevatorWebService;
            this.machineBayWebService = machineBayWebService;
            this.bayManager = bayManager;

            this.carouselManualMovementsViewModel = new CarouselManualMovementsViewModel(this.machineCarouselWebService, this.machineElevatorWebService, this.bayManager);
            this.engineManualMovementsViewModel = new EngineManualMovementsViewModel(this.machineElevatorWebService, this.bayManager);
            this.externalBayManualMovementsViewModel = new ExternalBayManualMovementsViewModel(this.machineElevatorWebService, this.machineBayWebService, this.bayManager);
            this.shutterEngineManualMovementsViewModel = new ShutterEngineManualMovementsViewModel(this.machineShutterWebService, this.machineElevatorWebService, this.bayManager);
        }

        #endregion

        #region Properties

        public CarouselManualMovementsViewModel CarouselManualMovementsViewModel => this.carouselManualMovementsViewModel;

        public EngineManualMovementsViewModel EngineManualMovementsViewModel => this.engineManualMovementsViewModel;

        public ExternalBayManualMovementsViewModel ExternalBayManualMovementsViewModel => this.externalBayManualMovementsViewModel;

        public bool HasCarousel => this.bayManager.Bay.Carousel != null;

        public bool IsBayExternal => this.bayManager.Bay.IsExternal;

        public ShutterEngineManualMovementsViewModel ShutterEngineManualMovementsViewModel => this.shutterEngineManualMovementsViewModel;

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();
        }

        public override void UpdateNotifications()
        {
            this.ShowNotification(InstallationApp.LSMTComandNote, Services.Models.NotificationSeverity.Info);
        }

        #endregion
    }
}
