namespace Ferretto.VW.MAS_DataLayer
{
    public interface IDataLayer
    {
        #region Methods

        /// <summary>
        /// This method is been invoked during the installation, to load the general_info.json file
        /// </summary>
        /// <exception cref="DataLayerExceptionEnum.UNDEFINED_TYPE_EXCEPTION">Exception for an unknown data type</exception>
        void LoadGeneralInfo();

        #endregion
    }
}
