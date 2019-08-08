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
    }
}
