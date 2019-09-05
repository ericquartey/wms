using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public class OtherSensorsViewModel : BaseSensorsViewModel
    {
        #region Constructors

        public OtherSensorsViewModel(IMachineSensorsService sensorsService)
            : base(sensorsService)
        {
        }

        #endregion
    }
}
