using System.ComponentModel.DataAnnotations;
using Ferretto.VW.MAS_DataLayer.Enumerations;

namespace Ferretto.VW.MAS_DataLayer
{
    public class ConfigurationValue
    {
        #region Properties

        [Key] public long CategoryName { get; set; }

        [Key] public long VarName { get; set; }

        public DataTypeEnum VarType { get; set; }

        public string VarValue { get; set; }

        #endregion
    }
}
