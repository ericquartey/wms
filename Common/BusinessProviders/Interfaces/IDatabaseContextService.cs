using Ferretto.Common.EF;

namespace Ferretto.Common.BusinessProviders
{
    public interface IDatabaseContextService
    {
        #region Properties

        DatabaseContext Current { get; }

        #endregion Properties
    }
}
