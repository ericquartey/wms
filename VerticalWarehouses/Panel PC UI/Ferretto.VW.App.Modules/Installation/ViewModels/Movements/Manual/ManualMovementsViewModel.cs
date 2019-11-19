using System;
using System.Threading.Tasks;
using System.Windows.Input;
using CommonServiceLocator;
using DevExpress.Mvvm;
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

        private readonly ElevatorManualMovementsViewModel elevatorManualMovementsViewModel;

        private readonly ExternalBayManualMovementsViewModel externalBayManualMovementsViewModel;

        private readonly IHealthProbeService healthProbeService;

        private readonly IMachineBaysWebService machineBayWebService;

        private readonly IMachineCarouselWebService machineCarouselWebService;

        private readonly IMachineElevatorWebService machineElevatorWebService;

        private readonly IMachineSensorsWebService machineSensorsWebService;

        private readonly IMachineShuttersWebService machineShutterWebService;

        private readonly ShutterEngineManualMovementsViewModel shutterEngineManualMovementsViewModel;

        private Bay bay;

        private bool hasCarousel;

        private bool hasShutter;

        private bool isBayExternal;

        private bool unsafeRelease;

        private DelegateCommand unsafeReleaseCommand;

        #endregion

        #region Constructors

        public ManualMovementsViewModel(
            IMachineShuttersWebService shutterWebService,
            IMachineCarouselWebService machineCarouselWebService,
            IMachineElevatorWebService machineElevatorWebService,
            IMachineBaysWebService machineBayWebService,
            IMachineSensorsWebService machineSensorsWebService,
            IHealthProbeService healthProbeService,
            IBayManager bayManager)
            : base(PresentationMode.Installer)
        {
            this.machineShutterWebService = shutterWebService ?? throw new System.ArgumentNullException(nameof(shutterWebService));
            this.machineCarouselWebService = machineCarouselWebService ?? throw new System.ArgumentNullException(nameof(machineCarouselWebService));
            this.machineElevatorWebService = machineElevatorWebService ?? throw new System.ArgumentNullException(nameof(machineElevatorWebService));
            this.machineBayWebService = machineBayWebService ?? throw new System.ArgumentNullException(nameof(machineBayWebService));
            this.bayManager = bayManager ?? throw new System.ArgumentNullException(nameof(bayManager));
            this.machineSensorsWebService = machineSensorsWebService ?? throw new ArgumentNullException(nameof(machineSensorsWebService));
            this.healthProbeService = healthProbeService ?? throw new ArgumentNullException(nameof(healthProbeService));

            this.carouselManualMovementsViewModel = new CarouselManualMovementsViewModel(this.machineCarouselWebService, this.machineElevatorWebService, this.machineSensorsWebService, this.healthProbeService, this.bayManager);
            this.engineManualMovementsViewModel = new ElevatorManualMovementsViewModel(this.machineElevatorWebService, this.machineSensorsWebService, this.healthProbeService, this.bayManager);
            this.externalBayManualMovementsViewModel = new ExternalBayManualMovementsViewModel(this.machineBayWebService, this.machineElevatorWebService, this.machineSensorsWebService, this.healthProbeService, this.bayManager);
            this.shutterEngineManualMovementsViewModel = new ShutterEngineManualMovementsViewModel(this.machineShutterWebService, this.machineElevatorWebService, this.machineSensorsWebService, this.healthProbeService, this.bayManager);
        }

        #endregion

        #region Properties

        public CarouselManualMovementsViewModel CarouselManualMovementsViewModel => this.carouselManualMovementsViewModel;

        public ElevatorManualMovementsViewModel ElevatorManualMovementsViewModel => this.engineManualMovementsViewModel;

        public ExternalBayManualMovementsViewModel ExternalBayManualMovementsViewModel => this.externalBayManualMovementsViewModel;

        public bool HasCarousel
        {
            get => this.hasCarousel;
            set => this.SetProperty(ref this.hasCarousel, value);
        }

        public bool HasShutter
        {
            get => this.hasShutter;
            set => this.SetProperty(ref this.hasShutter, value);
        }

        public bool IsBayExternal
        {
            get => this.isBayExternal;
            set => this.SetProperty(ref this.isBayExternal, value);
        }

        public ShutterEngineManualMovementsViewModel ShutterEngineManualMovementsViewModel => this.shutterEngineManualMovementsViewModel;

        public bool UnsafeRelease
        {
            get => this.unsafeRelease;
            private set => this.SetProperty(ref this.unsafeRelease, value, this.UpdateUnsafeRelease);
        }

        public ICommand UnsafeReleaseCommand =>
            this.unsafeReleaseCommand
            ??
            (this.unsafeReleaseCommand = new DelegateCommand(this.UnsafeReleaseAsync));

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();

            this.carouselManualMovementsViewModel.Disappear();
            this.engineManualMovementsViewModel.Disappear();
            this.externalBayManualMovementsViewModel.Disappear();
            this.shutterEngineManualMovementsViewModel.Disappear();
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            try
            {
                this.bay = await this.bayManager.GetBayAsync();

                this.HasCarousel = this.bay.Carousel != null;

                this.HasShutter = this.bay.Shutter.Type != ShutterType.NotSpecified;

                this.IsBayExternal = this.bay.IsExternal;
            }
            catch (System.Exception ex)
            {
                this.ShowNotification(ex);
            }

            this.RaisePropertyChanged(nameof(this.IsBayExternal));
        }

        public void UnsafeReleaseAsync()
        {
            var dialogService = ServiceLocator.Current.GetInstance<Controls.Interfaces.IDialogService>();
            var messageBoxResult = dialogService.ShowMessage(
                InstallationApp.ConfirmationOperation,
                "Movimenti manuali",
                Controls.Interfaces.DialogType.Question,
                Controls.Interfaces.DialogButtons.YesNo);
            if (messageBoxResult == Controls.Interfaces.DialogResult.Yes)
            {
                this.UnsafeRelease = !this.UnsafeRelease;
            }
        }

        public override void UpdateNotifications()
        {
            this.ShowNotification(InstallationApp.LSMTComandNote, Services.Models.NotificationSeverity.Info);
        }

        private void UpdateUnsafeRelease()
        {
            this.carouselManualMovementsViewModel.UnsafeRelease = this.unsafeRelease;
            this.engineManualMovementsViewModel.UnsafeRelease = this.unsafeRelease;
            this.externalBayManualMovementsViewModel.UnsafeRelease = this.unsafeRelease;
            this.shutterEngineManualMovementsViewModel.UnsafeRelease = this.unsafeRelease;
        }

        #endregion
    }
}
