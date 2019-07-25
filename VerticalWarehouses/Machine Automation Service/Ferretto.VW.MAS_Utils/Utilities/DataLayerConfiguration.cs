// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.Utils.Utilities
{
    public class DataLayerConfiguration
    {
        #region Constructors

        public DataLayerConfiguration(string primaryConnectionString, string secondaryConnectionString, string configurationFilePath)
        {
            this.PrimaryConnectionString = primaryConnectionString;
            this.SecondaryConnectionString = secondaryConnectionString;
            this.ConfigurationFilePath = configurationFilePath;
        }

        #endregion

        #region Properties

        public string ConfigurationFilePath { get; }

        public string PrimaryConnectionString { get; }

        public string SecondaryConnectionString { get; }

        #endregion
    }
}
