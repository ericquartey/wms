using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Regions;

namespace Ferretto.VW.App.Installation.ViewModels
{
    internal sealed class ManualMovementsViewModel : BaseMainViewModel
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

        private Bay bay;

        private bool hasCarousel;

        private bool isBayExternal;

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
            this.machineShutterWebService = shutterWebService ?? throw new System.ArgumentNullException(nameof(shutterWebService));
            this.machineCarouselWebService = machineCarouselWebService ?? throw new System.ArgumentNullException(nameof(machineCarouselWebService));
            this.machineElevatorWebService = machineElevatorWebService ?? throw new System.ArgumentNullException(nameof(machineElevatorWebService));
            this.machineBayWebService = machineBayWebService ?? throw new System.ArgumentNullException(nameof(machineBayWebService));
            this.bayManager = bayManager ?? throw new System.ArgumentNullException(nameof(bayManager));

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

        public bool HasCarousel
        {
            get => this.hasCarousel;
            set => this.SetProperty(ref this.hasCarousel, value);
        }

        public bool IsBayExternal
        {
            get => this.isBayExternal;
            set => this.SetProperty(ref this.isBayExternal, value);
        }

        public ShutterEngineManualMovementsViewModel ShutterEngineManualMovementsViewModel => this.shutterEngineManualMovementsViewModel;

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            try
            {
                this.bay = await this.bayManager.GetBayAsync();

                this.HasCarousel = this.bay.Carousel != null;
                this.IsBayExternal = this.bay.IsExternal;
            }
            catch (System.Exception ex)
            {
                this.ShowNotification(ex);
            }

            this.RaisePropertyChanged(nameof(this.IsBayExternal));
        }

        public override void UpdateNotifications()
        {
            this.ShowNotification(InstallationApp.LSMTComandNote, Services.Models.NotificationSeverity.Info);
        }

        #endregion
    }
}
