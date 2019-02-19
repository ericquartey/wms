using System.Collections.Generic;

namespace Ferretto.VW.MAS_DataLayer
{
    public interface IDataLayer
    {
        #region Methods

        List<Cell> GetCellList(int drawerHeight);

        decimal GetDecimalConfigurationValue(ConfigurationValueEnum configurationValueEnum);

        decimal GetDecimalRuntimeValue(RuntimeValueEnum runtimeValueEnum);

        int GetIntegerConfigurationValue(ConfigurationValueEnum configurationValueEnum);

        int GetIntegerRuntimeValue(RuntimeValueEnum runtimeValueEnum);

        string GetStringConfigurationValue(ConfigurationValueEnum configurationValueEnum);

        string GetStringRuntimeValue(RuntimeValueEnum runtimeValueEnum);

        bool SetCellList(List<Cell> listCells);

        void SetDecimalConfigurationValue(ConfigurationValueEnum configurationValueEnum, decimal value);

        void SetDecimalRuntimeValue(RuntimeValueEnum runtimeValueEnum, decimal value);

        void SetIntegerConfigurationValue(ConfigurationValueEnum configurationValueEnum, int value);

        void SetIntegerRuntimeValue(RuntimeValueEnum runtimeValueEnum, int value);

        void SetStringConfigurationValue(ConfigurationValueEnum configurationValueEnum, string value);

        void SetStringRuntimeValue(RuntimeValueEnum runtimeValueEnum, string value);

        #endregion
    }
}
