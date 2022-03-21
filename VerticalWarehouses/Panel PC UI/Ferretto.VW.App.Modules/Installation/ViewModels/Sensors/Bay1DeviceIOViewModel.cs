using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Installation.ViewModels
{
    internal class Bay1DeviceIOViewModel : BaseSensorsViewModel
    {
        #region Constructors

        public Bay1DeviceIOViewModel(
            IMachineSensorsWebService machineSensorsWebService,
            IMachineBaysWebService machineBaysWebService,
            IBayManager bayManager,
            IMachineIdentityWebService machineIdentityWebService)
            : base(machineSensorsWebService, machineBaysWebService, bayManager, machineIdentityWebService)
        {
        }

        #endregion
    }
}
