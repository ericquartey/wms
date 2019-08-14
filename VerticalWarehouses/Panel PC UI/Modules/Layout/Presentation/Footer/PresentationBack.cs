using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.App.Services.Interfaces;

namespace Ferretto.VW.App.Modules.Layout.Presentation
{
    public class PresentationBack : BasePresentation
    {
        #region Fields

        private readonly INavigationService navigationService;

        #endregion

        #region Constructors

        public PresentationBack(INavigationService navigationService)
            : base(PresentationTypes.Back)
        {
            if (navigationService == null)
            {
                throw new System.ArgumentNullException(nameof(navigationService));
            }

            this.navigationService = navigationService;

            this.EventAggregator
                .GetEvent<PresentationChangedPubSubEvent>()
                .Subscribe(this.Update);

            this.EventAggregator
                .GetEvent<PresentationChangedPubSubEvent>()
                .Subscribe(this.Update);
        }

        #endregion

        #region Methods

        public override Task ExecuteAsync()
        {
            this.navigationService.GoBack();

            return Task.CompletedTask;
        }

        private void Update(PresentationChangedMessage message)
        {
            if (message.States != null
                &&
                message.States.FirstOrDefault(s => s.Type == this.Type) is Services.Presentation back
                &&
                back.IsVisible.HasValue)
            {
                this.IsVisible = back.IsVisible;
            }
        }

        #endregion
    }
}
