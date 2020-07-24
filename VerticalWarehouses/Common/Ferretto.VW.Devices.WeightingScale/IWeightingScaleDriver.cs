using System.Threading.Tasks;

namespace Ferretto.VW.Devices.WeightingScale
{
    public interface IWeightingScaleDriver
    {
        #region Methods

        Task ClearMessageAsync();

        void Connect(SerialPortOptions options);

        void Disconnect();

        Task DisplayMessageAsync(string message);

        Task DisplayMessageAsync(string message, System.TimeSpan duration);

        Task<IWeightSample> MeasureWeightAsync();

        Task ResetAverageUnitaryWeightAsync();

        Task<string> RetrieveVersionAsync();

        Task SetAverageUnitaryWeightAsync(float weight);

        #endregion
    }
}
