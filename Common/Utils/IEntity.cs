namespace Ferretto.Common.Utils
{
    public interface IEntity<TId>
    {
        #region Properties

        TId Id { get; set; }

        #endregion Properties
    }
}
