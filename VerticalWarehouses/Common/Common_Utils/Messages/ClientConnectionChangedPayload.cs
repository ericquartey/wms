using Prism.Events;

namespace Ferretto.VW.CommonUtils.Messages
{
    public class ClientConnectionChangedPayload : EventBase
    {
        #region Constructors

        public ClientConnectionChangedPayload(
            string connectionId,
            System.Net.IPAddress clientIpAddress,
            bool isConnected)
        {
            if (string.IsNullOrEmpty(connectionId))
            {
                throw new System.ArgumentNullException(nameof(connectionId));
            }

            if (clientIpAddress == null)
            {
                throw new System.ArgumentNullException(nameof(clientIpAddress));
            }

            this.ConnectionId = connectionId;
            this.ClientIpAddress = clientIpAddress;
            this.IsConnected = isConnected;
        }

        #endregion

        #region Properties

        public System.Net.IPAddress ClientIpAddress { get; }

        public string ConnectionId { get; }

        public bool IsConnected { get; }

        #endregion
    }
}
