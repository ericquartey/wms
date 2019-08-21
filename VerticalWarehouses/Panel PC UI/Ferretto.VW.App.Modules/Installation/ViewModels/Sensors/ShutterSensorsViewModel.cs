using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public class ShutterSensorsViewModel : BaseSensorsViewModel
    {
        #region Constructors

        public ShutterSensorsViewModel(IMachineSensorsService sensorsService)
            : base(sensorsService)
        {
        }

        #endregion
    }
}
