namespace Ferretto.VW.MAS_DataLayer.Interfaces
{
    public interface IDataLayer
    {
        #region Methods

        /// <summary>
        ///     Exchamge the primary and secondary context, to call when the primary DB can't work.
        /// </summary>
        void switchDBContext();

        #endregion
    }
}
