namespace Ferretto.Common.DataModels
{
    public interface IDataModel<TKey>
    {
        #region Properties

        TKey Id { get; set; }

        #endregion
    }
}
