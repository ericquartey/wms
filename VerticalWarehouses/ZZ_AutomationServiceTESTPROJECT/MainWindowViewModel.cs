using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Prism.Commands;
using Prism.Mvvm;
using Microsoft.AspNetCore.SignalR.Client;

namespace ZZ_AutomationServiceTESTPROJECT
{
    internal class MainWindowViewModel : BindableBase
    {
        #region Fields

        private readonly AutomationServiceHubClient automationServiceConnection;

        private readonly HubConnection hubConnection;

        private ICommand connectToServiceCommand;

        private ICommand executeServerMethodCommand;

        #endregion Fields

        #region Constructors

        public MainWindowViewModel()
        {
            this.automationServiceConnection = new AutomationServiceHubClient("http://localhost:5000", "/automation-endpoint");
            this.hubConnection = new HubConnectionBuilder().WithUrl("http://localhost:5000/automation-endpoint").Build();
        }

        #endregion Constructors

        #region Properties

        public ICommand ConnectToServiceCommand => this.connectToServiceCommand ?? (this.connectToServiceCommand = new DelegateCommand(this.ExecuteServerMethodMethod));

        public ICommand ExecuteServerMethodCommand => this.executeServerMethodCommand ?? (this.executeServerMethodCommand = new DelegateCommand(this.ConnectToServerMethod));

        #endregion Properties

        #region Methods

        private void ConnectToServerMethod()
        {
            this.automationServiceConnection.ConnectAsync();
        }

        private void ExecuteServerMethodMethod()
        {
            this.hubConnection.InvokeAsync("testing", "ciao dal client");
        }

        #endregion Methods
    }
}
