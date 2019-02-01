using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.InstallationApp.ServiceUtilities;
using Microsoft.AspNetCore.SignalR.Client;
using Prism.Commands;
using Prism.Mvvm;
using Ferretto.VW.InstallationApp;

namespace Ferretto.VW.InstallationApp
{
    public class BeltBurnishingViewModel : BindableBase, IViewModel, IBeltBurnishingViewModel
    {
        #region Methods

        public void ExitFromViewMethod()
        {
            throw new NotImplementedException();
        }

        public void SubscribeMethodToEvent()
        {
            throw new NotImplementedException();
        }

        public void UnSubscribeMethodFromEvent()
        {
            throw new NotImplementedException();
        }

        #endregion Methods
    }
}
