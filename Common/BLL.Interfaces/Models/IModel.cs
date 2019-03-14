namespace Ferretto.Common.BLL.Interfaces.Models
{
    public interface IModel<TKey>
    {
        #region Properties

        TKey Id { get; set; }

        #endregion
    }
}
