using Ferretto.VW.MAS.DataModels.Enumerations;

namespace Ferretto.VW.MAS.DataModels
{
    public sealed class ConfigurationValue
    {
        #region Properties

        public long CategoryName { get; set; }

        public long VarName { get; set; }

        public ConfigurationDataType VarType { get; set; }

        public string VarValue { get; set; }

        #endregion
    }
}
