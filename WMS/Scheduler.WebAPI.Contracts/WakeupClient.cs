namespace Ferretto.WMS.Scheduler.WebAPI.Contracts
{
    /*
    public class WakeupClient
    {
        private readonly HubConnection connection;

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

            await this.connection.StartAsync();
        }

        public async Task SendAsync()
        {
            await this.connection.InvokeAsync("SendMessage",
                "salomone", "Hello World!");
        }
    }*/
}
