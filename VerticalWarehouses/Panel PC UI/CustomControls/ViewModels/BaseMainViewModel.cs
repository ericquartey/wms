using Ferretto.VW.App.Services;
using Prism.Regions;

namespace Ferretto.VW.App.Controls
{
    public class BaseMainViewModel : BaseServiceViewModel
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

        public override void OnNavigated()
        {
            base.OnNavigated();
            this.UpdatePresentation();
        }

        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
            base.OnNavigatedFrom(navigationContext);
            this.UpdatePresentation();
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);
        }

        public void ShowError(string error)
        {
            this.EventAggregator.GetEvent<PresentationChangedPubSubEvent>()
                        .Publish(new PresentationChangedMessage(error));
        }

        private void UpdatePresentation()
        {
            this.EventAggregator.GetEvent<PresentationChangedPubSubEvent>()
                        .Publish(new PresentationChangedMessage(this.Mode));
        }

        #endregion
    }
}
