using System.Linq;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface IMachineProvider : IBusinessProvider<Machine, MachineDetails>
    {
        #region Methods

        IQueryable<Machine> GetAllTraslo();

        int GetAllTrasloCount();

        IQueryable<Machine> GetAllVertimag();

        int GetAllVertimagCount();

        IQueryable<Machine> GetAllVertimagModelM();

        int GetAllVertimagModelMCount();

        IQueryable<Machine> GetAllVertimagModelXs();

        int GetAllVertimagModelXsCount();

        #endregion
    }
}
