using System.Net;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.Exceptions;

namespace Ferretto.VW.MAS_IODriver.Interface
{
    public interface IModbusTransport
    {
        #region Properties
        /// <summary>
        /// IsConnected to ioClient 
        /// </summary>
        bool IsConnected { get; }

        #endregion

        #region Methods
        /// <summary>
        /// ModbusTransport Configuration Parameters to communicate with another layers
        /// </summary>
        /// <param name="hostAddress">Address of the IO host device</param>
        /// <param name="sendPort">TCP/IP Port for the IO device</param>
        void Configure(IPAddress hostAddress, int sendPort);

        /// <summary>
        /// Connect to ioClient
        /// </summary>
        /// <exception cref="IoDriverException">Invalid hostAddress and port: remote endpoint not connected</exception>
        /// <exception cref="IoDriverException">Invalid IpMaster: remote endpoint not connected</exception>
        /// <returns></returns>
        bool Connect();

        /// <summary>
        /// Disconnect to ioClient
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Asynchronously Read Inputs parameters from remote host device
        /// </summary>
        /// <exception cref="IoDriverException">Invalid Read request: remote endpoint not connected</exception>
        /// <exception cref="IoDriverException">Error reading data from Transport Stream</exception>
        /// <returns></returns>
        Task<bool[]> ReadAsync();

        /// <summary>
        /// Asynchronously Write Outputs parameters of remote host device
        /// </summary>
        /// <param name="outputs">bool array outputs of write method </param>
        /// <exception cref="IoDriverException">Invalid Write request: remote endpoint not connected</exception>
        /// <exception cref="IoDriverException">Error writing data to Transport Stream</exception>
        /// <returns></returns>
        Task WriteAsync(bool[] outputs);

        #endregion
    }
}
