using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public class ShutterSensorsViewModel : BaseSensorsViewModel
    {
        #region Fields

        private readonly bool gateSensorA;

        private readonly bool gateSensorB;

        #endregion

        #region Constructors

        public ShutterSensorsViewModel(IMachineSensorsService sensorsService)
            : base(sensorsService)
        {
        }

        #endregion
    }
}
