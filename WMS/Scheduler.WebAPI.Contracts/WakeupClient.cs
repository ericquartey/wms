namespace Ferretto.WMS.Scheduler.WebAPI.Contracts
{
    public class WakeupClient
    {
        #region Fields

        private readonly HubConnection connection;

        #endregion Fields

        #region Constructors

        public WakeupClient(string hubUrl)
        {
            this.connection = new HubConnectionBuilder()
              .WithUrl(hubUrl)
              .Build();

            this.connection.Closed += async (error) =>
            {
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await this.connection.StartAsync();
            };
        }

        #endregion Constructors

        #region Methods

        public async Task ConnectAsync()
        {
            this.connection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    var newMessage = $"{user}: {message}";
                    messagesList.Items.Add(newMessage);
                });
            });

            try
            {
                await this.connection.StartAsync();
            }
            catch (Exception ex)
            {
                messagesList.Items.Add(ex.Message);
            }
        }

        public async Task SendAsync()
        {
            try
            {
                await this.connection.InvokeAsync("SendMessage",
                    "salomone", "Hello World!");
            }
            catch (Exception ex)
            {
                messagesList.Items.Add(ex.Message);
            }
        }

        #endregion Methods
    }
}
