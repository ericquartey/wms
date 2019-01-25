using Ferretto.Common.EF;

namespace Ferretto.Common.Modules.BLL.Services
{
    public class DatabaseContextService : IDatabaseContextService
    {
        #region Properties

        public DatabaseContext Current => new DatabaseContext();

        #endregion Properties
    }
}
