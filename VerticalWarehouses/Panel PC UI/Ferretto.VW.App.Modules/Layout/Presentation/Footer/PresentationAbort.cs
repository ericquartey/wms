using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;

namespace Ferretto.VW.App.Modules.Layout.Presentation
{
    public class PresentationAbort : BasePresentationViewModel
    {
        #region Fields

        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly INavigationService navigationService;

        #endregion

        #region Constructors

        public PresentationAbort(INavigationService navigationService)
            : base(PresentationTypes.Abort)
        {
            if (navigationService is null)
            {
                throw new System.ArgumentNullException(nameof(navigationService));
            }

            this.navigationService = navigationService;

            this.EventAggregator
                .GetEvent<PresentationChangedPubSubEvent>()
                .Subscribe(this.Update);
        }

        #endregion

        #region Methods

        public override Task ExecuteAsync()
        {
            this.logger.Debug($"Presentation abort");
            this.navigationService.GoBack();

            return Task.CompletedTask;
        }

        private void Update(PresentationChangedMessage message)
        {
            if (message.States != null
                &&
                message.States.FirstOrDefault(s => s.Type == this.Type) is Services.Presentation abort
                &&
                abort.IsVisible.HasValue)
            {
                this.IsVisible = abort.IsVisible;
            }
        }

        #endregion
    }
}
