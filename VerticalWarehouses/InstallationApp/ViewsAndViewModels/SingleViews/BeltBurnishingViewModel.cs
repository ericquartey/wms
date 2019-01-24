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
        #region Fields

        private AutomationServiceHubClient automationServiceConnection;

        private ICommand connectToService;

        private ICommand executeServerMethod;

        private SensorsStatesHubClient sensorStateConnection;

        #endregion Fields

        #region Properties

        public ICommand ConnectToService => this.connectToService ?? (this.connectToService = new DelegateCommand(this.ConnectToServiceMethod));

        public ICommand ExecuteServerMethod => this.executeServerMethod ?? (this.executeServerMethod = new DelegateCommand(this.ExecuteServerMethodMethod));

        #endregion Properties

        #region Methods

        public void ConnectToServiceMethod()
        {
            this.automationServiceConnection = new AutomationServiceHubClient("http://localhost:5000", "/automation-endpoint");
        }

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

        private async void ExecuteServerMethodMethod()
        {
            await this.automationServiceConnection.ConnectAsync();

            this.automationServiceConnection.ExecuteServerMethod();
        }

        #endregion Methods
    }
}
