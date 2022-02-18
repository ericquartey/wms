using System.Net;
using System.Threading.Tasks;

namespace Ferretto.VW.Devices.WeightingScale
{
    public interface IWeightingScaleDriver
    {
        #region Properties

        bool ShowScaleNotResponding { get; }

        #endregion

        #region Methods

        Task ClearMessageAsync();

        Task ConnectAsync(IPAddress ipAddress, int port);

        Task DisconnectAsync();

        Task DisplayMessageAsync(string message);

        Task DisplayMessageAsync(string message, System.TimeSpan duration);

        Task<IWeightSample> MeasureWeightAsync(bool poll);

        Task ResetAverageUnitaryWeightAsync();

        Task<string> RetrieveVersionAsync();

        Task SetAverageUnitaryWeightAsync(float weight);

        #endregion
    }
}
