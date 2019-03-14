namespace Ferretto.VW.MAS_DataLayer
{
    public interface IDataLayer
    {
        #region Methods

        /// <summary>
        /// This method is been invoked during the installation, to load the general_info.json file
        /// </summary>
        void LoadGeneralInfo();

        #endregion
    }
}
