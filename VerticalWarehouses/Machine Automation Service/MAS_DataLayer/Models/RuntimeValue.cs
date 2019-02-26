using System;
using System.ComponentModel.DataAnnotations;

namespace Ferretto.VW.MAS_DataLayer
{
    public class RuntimeValue
    {
        #region Properties

        [Key] public RuntimeValueEnum VarName { get; set; }

        public DataTypeEnum VarType { get; set; }

        public String VarValue { get; set; }

        #endregion
    }
}
