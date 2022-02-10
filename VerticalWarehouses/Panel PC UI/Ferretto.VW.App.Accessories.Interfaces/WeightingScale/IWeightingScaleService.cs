using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Ferretto.VW.App.Accessories.Interfaces.WeightingScale;
using Ferretto.VW.Devices.WeightingScale;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Accessories.Interfaces
{
    public interface IWeightingScaleService : IAccessoryService
    {
        #region Events

        event System.EventHandler<WeightAcquiredEventArgs> WeighAcquired;

        #endregion

        #region Methods

        /// <summary>
        /// Clears the message on the display of the device.
        /// </summary>
        /// <exception cref="System.Exception" />
        Task ClearMessageAsync();

        /// <summary>
        /// Displays a message on the display of the device.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <exception cref="System.Exception" />
        Task DisplayMessageAsync(string message);

        /// <summary>
        /// Displays a message on the display of the device for a limited amount of time
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="duration">The duration of the message on the display.</param>
        /// <exception cref="System.Exception" />
        Task DisplayMessageAsync(string message, System.TimeSpan duration);

        /// <summary>
        /// Measures the current weight on the device.
        /// </summary>
        /// <returns>A <see cref="IWeightSample"/> containing the measurement information.</returns>
        /// <exception cref="System.Exception" />
        Task<IWeightSample> MeasureWeightAsync(bool poll);

        /// <summary>
        /// Resets the average unitary weight.
        /// </summary>
        /// <exception cref="System.Exception" />
        Task ResetAverageUnitaryWeightAsync();

        /// <summary>
        /// Sets the average unitary weight.
        /// </summary>
        /// <param name="weight">The weight of the item.</param>
        /// <exception cref="System.Exception" />
        Task SetAverageUnitaryWeightAsync(float weight);

        /// <summary>
        /// Starts the continuous weight acquisition.
        /// </summary>
        /// <exception cref="System.Exception" />
        Task StartWeightAcquisitionAsync();

        /// <summary>
        /// Stops the continuous weight acquisition.
        /// </summary>
        /// <exception cref="System.Exception" />
        void StopWeightAcquisitionAsync();

        /// <summary>
        /// Updates the average unitary weight of the specified item.
        /// </summary>
        /// <param name="itemId">The identifier of the item to update.</param>
        /// <param name="averageWeight">The new unitary average weight of the item, or null to leave it undefined.</param>
        /// <returns></returns>
        Task UpdateItemAverageWeightAsync(int itemId, double? averageWeight);

        /// <summary>
        /// Saves the device settings.
        /// </summary>
        /// <param name="isEnabled">If True, it means that the device is enabled and usable.</param>
        /// <param name="ipAddress">The ip address of the device.</param>
        /// <param name="port">The system name of the tcp port to which the device is listening.</param>
        /// <exception cref="System.Exception" />
        Task UpdateSettingsAsync(bool isEnabled, string ipAddress, int port, WeightingScaleModelNumber modelNumber);

        #endregion
    }
}
