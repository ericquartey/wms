using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Installation.ViewModels
{
    internal sealed class VerticalAxisSensorsViewModel : BaseSensorsViewModel
    {
        #region Constructors

        public VerticalAxisSensorsViewModel(
            IMachineSensorsWebService machineSensorsWebService,
            IMachineBaysWebService machineBaysWebService,
            IBayManager bayManager)
            : base(machineSensorsWebService, machineBaysWebService, bayManager)
        {
        }

        #endregion
    }
}
