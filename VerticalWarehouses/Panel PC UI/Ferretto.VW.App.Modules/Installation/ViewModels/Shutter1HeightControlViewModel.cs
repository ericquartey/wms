using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;

namespace Ferretto.VW.App.Modules.Installation.ViewModels
{
    public class Shutter1HeightControlViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private int activeRaysQuantity;

        private decimal currentHeight;

        private decimal gateCorrection;

        private string noteText;

        private int speed;

        private decimal systemError;

        private decimal tolerance;

        #endregion

        #region Constructors

        public Shutter1HeightControlViewModel(IEventAggregator eventAggregator) : base(Services.PresentationMode.Installer)
        {
            this.eventAggregator = eventAggregator;
            this.NavigationViewModel = null;
        }

        #endregion

        #region Properties

        public int ActiveRaysQuantity { get => this.activeRaysQuantity; set => this.SetProperty(ref this.activeRaysQuantity, value); }

        public decimal CurrentHeight { get => this.currentHeight; set => this.SetProperty(ref this.currentHeight, value); }

        public decimal GateCorrection { get => this.gateCorrection; set => this.SetProperty(ref this.gateCorrection, value); }

        public BindableBase NavigationViewModel { get; set; }

        public string NoteText { get => this.noteText; set => this.SetProperty(ref this.noteText, value); }

        public int Speed { get => this.speed; set => this.SetProperty(ref this.speed, value); }

        public decimal SystemError { get => this.systemError; set => this.SetProperty(ref this.systemError, value); }

        public decimal Tolerance { get => this.tolerance; set => this.SetProperty(ref this.tolerance, value); }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            // TODO
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
