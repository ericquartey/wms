using System.Threading.Tasks;
using Ferretto.WMS.Scheduler.Core.Models;

namespace Ferretto.WMS.Scheduler.Core.Interfaces
{
    public interface IBaySchedulerProvider
    {
        #region Methods

        Task<Bay> GetByIdAsync(int id);

        /// <summary>
        /// Increments the bay priority of the given amount, or of one unit, if no increment is specified.
        /// </summary>
        /// <param name="id">The id of the bay.</param>
        /// <param name="increment">
        /// The amount to add to the current priority value.
        /// If null, a default amount of one unit is added.
        /// </param>
        /// <returns>The updated bay priority.</returns>
        Task<int> UpdatePriorityAsync(int id, int? increment);

        #endregion
    }
}
