using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public class OtherSensorsViewModel : BaseSensorsViewModel
    {
        #region Constructors

        public OtherSensorsViewModel(IMachineSensorsService sensorsService, IBayManager bayManager)
            : base(sensorsService, bayManager)
        {
        }

        #endregion
    }
}
