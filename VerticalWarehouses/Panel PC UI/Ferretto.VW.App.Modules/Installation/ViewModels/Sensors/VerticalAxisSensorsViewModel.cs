using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public class VerticalAxisSensorsViewModel : BaseSensorsViewModel
    {
        #region Constructors

        public VerticalAxisSensorsViewModel(
            IMachineSensorsService machineSensorsService,
            IMachineBaysService machineBaysService,
            IBayManager bayManager)
            : base(machineSensorsService, machineBaysService, bayManager)
        {
        }

        #endregion
    }
}
