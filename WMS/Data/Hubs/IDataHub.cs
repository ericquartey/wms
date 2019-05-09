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

        Task EntityUpdated(EntityChangedHubEvent entityChangedHubEvent);

#pragma warning disable S125 // Sections of code should not be commented out

        // Task LiveMachineDataUpdated(MachineStatus machine);
        #endregion
    }

#pragma warning restore S125 // Sections of code should not be commented out
}
