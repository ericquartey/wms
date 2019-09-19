using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public class VerticalAxisSensorsViewModel : BaseSensorsViewModel
    {
        #region Constructors

        public VerticalAxisSensorsViewModel(IMachineSensorsService sensorsService, IBayManager bayManager)
            : base(sensorsService, bayManager)
        {
        }

        #endregion
    }
}
