using Ferretto.VW.MAS.DataLayer.Enumerations;

namespace Ferretto.VW.MAS.DataLayer.Models
{
    public class RuntimeValue
    {
        #region Properties

        public long CategoryName { get; set; }

        public long VarName { get; set; }

        public ConfigurationDataType VarType { get; set; }

        public string VarValue { get; set; }

        #endregion
    }
}
