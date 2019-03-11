using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.MAS_InverterDriver.Interface;

namespace Ferretto.VW.InverterDriver
{
    public class SocketTransportMock : ISocketTransport
    {
        #region Properties

        public bool IsConnected => true;

        #endregion

        #region Methods

        public void Configure(IPAddress inverterAddress, int sendPort)
        {
        }

        public async Task<bool> ConnectAsync()
        {
            return true;
        }

        public void Disconnect(int delay)
        {
        }

        public async Task<byte[]> ReadAsync(CancellationToken stoppingToken)
        {
            return null;
        }

        public async Task WriteAsync(byte[] inverterMessage, CancellationToken stoppingToken)
        {
        }

        #endregion
    }
}
