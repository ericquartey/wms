namespace Ferretto.VW.MAS_Utils.Utilities
{
    public class DataLayerConfiguration
    {
        #region Constructors

        public DataLayerConfiguration(string secondaryConnectionString, string configurationFilePath)
        {
            this.SecondaryConnectionString = secondaryConnectionString;
            this.ConfigurationFilePath = configurationFilePath;
        }

        #endregion

        #region Properties

        public string ConfigurationFilePath { get; }

        public string SecondaryConnectionString { get; }

        #endregion
    }
}
