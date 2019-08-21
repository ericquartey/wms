using System.Threading.Tasks;
using Ferretto.VW.App.Services;
using Prism.Regions;

namespace Ferretto.VW.App.Controls
{
    public class BaseMainViewModel : BaseNavigationViewModel
    {
        #region Fields

        private PresentationMode mode;

        #endregion

        #region Constructors

        public BaseMainViewModel(PresentationMode mode)
        {
            this.mode = mode;
        }

        #endregion

        #region Properties

        public PresentationMode Mode
        {
            get => this.mode;
            set => this.SetProperty(ref this.mode, value);
        }

        #endregion

        #region Methods

        public override async Task OnNavigatedAsync()
        {
            this.UpdatePresentation();
            await base.OnNavigatedAsync();
        }

        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
            base.OnNavigatedFrom(navigationContext);
            this.UpdatePresentation();
        }

        public void ShowNotification(string message)
        {
            this.EventAggregator
                .GetEvent<PresentationChangedPubSubEvent>()
                .Publish(new PresentationChangedMessage(message));
        }

        public void ShowNotification(System.Exception exception)
        {
            if (exception == null)
            {
                throw new System.ArgumentNullException(nameof(exception));
            }

            this.EventAggregator
                .GetEvent<PresentationChangedPubSubEvent>()
                .Publish(new PresentationChangedMessage(exception));
        }

        private void UpdatePresentation()
        {
            this.EventAggregator
                .GetEvent<PresentationChangedPubSubEvent>()
                .Publish(new PresentationChangedMessage(this.Mode));
        }

        #endregion
    }
}
