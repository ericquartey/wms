using System.Threading.Tasks;

namespace Ferretto.WMS.Data.Hubs
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Major Code Smell",
        "S4261: Add the 'Async' suffix to the name of this method.",
        Justification = "The methods names here will be exposed by SignalR, so we want that names are clean")]
    public interface IDataHub
    {
        #region Methods

        Task EntityUpdated(Models.EntityChangedHubEvent entityChangedHubEvent);

        Task MachineStatusUpdated(Models.MachineStatus machine);

        #endregion
    }
}
