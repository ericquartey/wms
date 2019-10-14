using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public class OtherSensorsViewModel : BaseSensorsViewModel
    {
        #region Constructors

        public OtherSensorsViewModel(
            IMachineSensorsWebService machineSensorsWebService,
            IMachineBaysWebService machineBaysWebService,
            IBayManager bayManager)
            : base(machineSensorsWebService, machineBaysWebService, bayManager)
        {
        }

        #endregion
    }
}
