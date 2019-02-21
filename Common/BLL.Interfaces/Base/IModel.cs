namespace Ferretto.Common.BLL.Interfaces.Base
{
    public interface IModel<TKey>
    {
        #region Properties

        TKey Id { get; set; }

        #endregion
    }
}
