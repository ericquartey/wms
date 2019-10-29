using System;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;

namespace Ferretto.VW.App.Installation.ViewModels
{
    [Obsolete]
    internal sealed class ShutterHeightControlViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private int activeRaysQuantity;

        private double currentHeight;

        private double gateCorrection;

        private string noteText;

        private int speed;

        private double systemError;

        private double tolerance;

        #endregion

        #region Constructors

        public ShutterHeightControlViewModel(IEventAggregator eventAggregator)
            : base(Services.PresentationMode.Installer)
        {
            this.eventAggregator = eventAggregator;
            this.NavigationViewModel = null;
        }

        #endregion

        #region Properties

        public int ActiveRaysQuantity { get => this.activeRaysQuantity; set => this.SetProperty(ref this.activeRaysQuantity, value); }

        public double CurrentHeight { get => this.currentHeight; set => this.SetProperty(ref this.currentHeight, value); }

        public double GateCorrection { get => this.gateCorrection; set => this.SetProperty(ref this.gateCorrection, value); }

        public BindableBase NavigationViewModel { get; set; }

        public string NoteText { get => this.noteText; set => this.SetProperty(ref this.noteText, value); }

        public int Speed { get => this.speed; set => this.SetProperty(ref this.speed, value); }

        public double SystemError { get => this.systemError; set => this.SetProperty(ref this.systemError, value); }

        public double Tolerance { get => this.tolerance; set => this.SetProperty(ref this.tolerance, value); }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            // TODO
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.IsBackNavigationAllowed = true;
        }

        public Task OnEnterViewAsync()
        {
            return Task.CompletedTask;
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);

            this.IsBackNavigationAllowed = true;
        }

        public void UnSubscribeMethodFromEvent()
        {
            // TODO
        }

        #endregion
    }
}
