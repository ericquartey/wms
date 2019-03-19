using System.Threading.Tasks;

namespace Ferretto.WMS.Data.WebAPI.Hubs
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Major Code Smell",
        "S4261: Add the 'Async' suffix to the name of this method.",
        Justification = "The methods names here will be exposed by SignalR, so we want that names are clean")]
    public interface ISchedulerHub
    {
        #region Methods

        Task MissionUpdated(int id);

        #endregion
    }
}
