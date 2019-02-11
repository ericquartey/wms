using System.ComponentModel.DataAnnotations;

namespace Ferretto.VW.MAS_DataLayer
{
    public class ConfigurationValue
    {
        #region Properties

        [Key]
        public ConfigurationValueEnum VarName { get; set; }

        public string VarValue { get; set; }

        public DataTypeEnum VarType { get; set; }

        #endregion Properties
    }
}
