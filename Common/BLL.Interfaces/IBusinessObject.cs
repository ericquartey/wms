namespace Ferretto.Common.BLL.Interfaces
{
    public interface IBusinessObject<TId>
    {
        #region Properties

        TId Id { get; }

        #endregion Properties
    }
}
