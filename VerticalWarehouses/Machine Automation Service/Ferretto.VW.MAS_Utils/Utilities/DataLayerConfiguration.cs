namespace Ferretto.VW.MAS_Utils.Utilities
{
    public class DataLayerConfiguration
    {
        #region Constructors

        public DataLayerConfiguration(string connectionString, string configurationFilePath)
        {
            this.ConnectionString = connectionString;
            this.ConfigurationFilePath = configurationFilePath;
        }

        #endregion

        #region Properties

        public string ConfigurationFilePath { get; }

        public string ConnectionString { get; }

        #endregion
    }
}
