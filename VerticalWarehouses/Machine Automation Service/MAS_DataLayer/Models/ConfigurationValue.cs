using System.ComponentModel.DataAnnotations;
using Ferretto.VW.MAS_DataLayer.Enumerations;

namespace Ferretto.VW.MAS_DataLayer
{
    public class ConfigurationValue
    {
        #region Properties

        public long CategoryName { get; set; }

        public long VarName { get; set; }

        public DataType VarType { get; set; }

        public string VarValue { get; set; }

        #endregion
    }
}
