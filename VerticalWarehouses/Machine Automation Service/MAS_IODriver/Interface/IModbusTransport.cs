using System.Net;
using System.Threading.Tasks;

namespace Ferretto.VW.MAS_IODriver.Interface
{
    public interface IModbusTransport
    {
        #region Properties

        bool IsConnected { get; }

        #endregion

        #region Methods

        void Configure(IPAddress hostAddress, int sendPort);

        bool Connect();

        void Disconnect();

        Task<bool[]> ReadAsync();

        Task WriteAsync(bool[] outputs);

        #endregion
    }
}
