namespace Ferretto.Common.EF
{
    public interface IDatabaseContextService
    {
        #region Properties

        DatabaseContext Current { get; }

        #endregion
    }
}
