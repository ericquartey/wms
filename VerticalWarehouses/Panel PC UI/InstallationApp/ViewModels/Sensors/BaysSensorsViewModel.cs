using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public class BaysSensorsViewModel : BaseSensorsViewModel
    {
        #region Constructors

        public BaysSensorsViewModel(ISensorsMachineService sensorsMachineService)
            : base(sensorsMachineService)
        {
        }

        #endregion

        #region Methods

        public override void OnNavigated()
        {
            base.OnNavigated();
            var state = new Presentation()
            {
                Type = PresentationTypes.Back,
                IsVisible = true
            };

            this.EventAggregator.GetEvent<PresentationChangedPubSubEvent>()?.Publish(new PresentationChangedMessage(state));
        }

        #endregion
    }
}
