using System.ComponentModel.DataAnnotations;

namespace Ferretto.VW.MAS_DataLayer
{
    public class RuntimeValue
    {
        #region Properties

        [Key]
        public RuntimeValueEnum VarName { get; set; }

        public string VarValue { get; set; }

        public DataTypeEnum VarType { get; set; }

        #endregion Properties

    }
}
