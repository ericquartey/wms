using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public class VerticalAxisSensorsViewModel : BaseSensorsViewModel
    {
        #region Constructors

        public VerticalAxisSensorsViewModel(ISensorsMachineService sensorsService)
            : base(sensorsService)
        {
        }

        #endregion
    }
}
