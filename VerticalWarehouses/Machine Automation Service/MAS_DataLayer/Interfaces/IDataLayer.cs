namespace Ferretto.VW.MAS_DataLayer.Interfaces
{
    public interface IDataLayer
    {
        #region Methods

        /// <summary>
        ///     Exchange method between primary and secondary context.
        /// </summary>
        void SwitchDBContext();

        #endregion
    }
}
