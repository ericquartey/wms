using System.Net;
using System.Threading.Tasks;
using Ferretto.VW.MAS_IODriver.Interface;

namespace Ferretto.VW.MAS_IODriver
{
    public class ModbusTransportMock : IModbusTransport
    {
        #region Properties

        public bool IsConnected => true;

        #endregion

        #region Methods

        public void Configure(IPAddress hostAddress, int sendPort)
        {
        }

        public bool Connect()
        {
            return true;
        }

        public void Disconnect()
        {
        }

        public async Task<bool[]> ReadAsync()
        {
            await Task.Delay(500);
            return null;
        }

        public async Task WriteAsync(bool[] outputs)
        {
            await Task.Delay(500);
        }

        #endregion
    }
}
