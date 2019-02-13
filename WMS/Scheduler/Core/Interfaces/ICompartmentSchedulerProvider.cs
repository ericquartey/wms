using System.Threading.Tasks;

namespace Ferretto.WMS.Scheduler.Core.Interfaces
{
    public interface ICompartmentSchedulerProvider
    {
        #region Methods

        Task<Compartment> UpdateAsync(Compartment compartment);

        #endregion
    }
}
