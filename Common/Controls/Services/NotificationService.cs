using System;
using System.Configuration;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Controls.Interfaces;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.Practices.ServiceLocation;

namespace Ferretto.Common.Controls.Services
{
    public class NotificationServiceClient : INotificationServiceClient
    {
        #region Fields

        private const string HealthIsOnLine = "IsOnLine";
        private const int startConnEvery = 10000;
        private readonly IEventService eventService = ServiceLocator.Current.GetInstance<IEventService>();
        private readonly string healthPath;
        private readonly string url;
        private HubConnection connection;
        private bool isConnected;
        private IHubProxy proxy;

        #endregion Fields

        #region Constructors

        public NotificationServiceClient()
        {
            this.url = ConfigurationManager.AppSettings["NotificationHubEndpoint"];
            this.healthPath = ConfigurationManager.AppSettings["NotificationHubStatus"];
        }

        #endregion Constructors

        #region Methods

        public void End()
        {
            this.connection.Stop();
        }

        public void Start()
        {
            try
            {
                this.Initialize();
            }
            catch (Exception)
            {
                System.Diagnostics.Debug.WriteLine("Hub connection failed.");
                System.Threading.Thread.Sleep(startConnEvery);
                this.connection.Start();
            }
        }

        private void Initialize()
        {
            this.connection = new HubConnection(this.url);
            this.proxy = this.connection.CreateHubProxy(this.healthPath);
            this.proxy.On(HealthIsOnLine, this.OnIsOnLine_MessageReceived);
            this.connection.StateChanged += this.OnConnectionStateChanged;

            this.connection.Closed += () =>
            {
                if (this.isConnected)
                {
                    this.eventService.Invoke(new StatusEventArgs() { IsSchedulerOnline = false });
                }
                this.isConnected = false;
                System.Diagnostics.Debug.WriteLine("Retrying connection to hub...");
                System.Threading.Thread.Sleep(startConnEvery);
                this.connection.Start();
            };
            this.connection.Start();
        }

        private void OnConnectionStateChanged(StateChange obj)
        {
            if (obj.NewState == ConnectionState.Disconnected)
            {
                System.Diagnostics.Debug.WriteLine("Hub disconnected.");
                this.eventService.Invoke(new StatusEventArgs() { IsSchedulerOnline = false });
            }
            else if (obj.NewState == ConnectionState.Connecting)
            {
                System.Diagnostics.Debug.WriteLine("Hub connecting...");
                this.eventService.Invoke(new StatusEventArgs() { IsSchedulerOnline = false });
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Hub Online");
                this.proxy.Invoke(HealthIsOnLine);
            }
        }

        private void OnIsOnLine_MessageReceived()
        {
            this.isConnected = true;
            System.Diagnostics.Debug.WriteLine("Hub connection successful");
            this.eventService.Invoke(new StatusEventArgs() { IsSchedulerOnline = true });
        }

        #endregion Methods
    }
}
