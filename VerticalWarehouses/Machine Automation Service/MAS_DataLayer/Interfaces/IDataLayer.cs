namespace Ferretto.VW.MAS_DataLayer
{
    public interface IDataLayer
    {
        #region Methods

        /// <summary>
        /// This method is been invoked during the installation, to load the general_info.json file
        /// </summary>
        /// <param name="configurationValueRequest">Configuration parameters to load</param>
        /// <exception cref="DataLayerExceptionEnum.UNKNOWN_INFO_FILE_EXCEPTION">Exception for a wrong info file input name</exception>
        /// <exception cref="DataLayerExceptionEnum.UNDEFINED_TYPE_EXCEPTION">Exception for an unknown data type</exception>
        void LoadConfigurationValuesInfo(InfoFilesEnum configurationValueRequest);

        #endregion
    }
}
