namespace Ferretto.VW.MAS_DataLayer
{
    public interface IDataLayer
    {
        int GetIntegerConfigurationValue(ConfigurationValueEnum configurationValueEnum);

        decimal GetDecimalConfigurationValue(ConfigurationValueEnum configurationValueEnum);

        string GetStringConfigurationValue(ConfigurationValueEnum configurationValueEnum);

        int GetIntegerRuntimeValue(RuntimeValueEnum runtimeValueEnum);

        decimal GetDecimalRuntimeValue(RuntimeValueEnum runtimeValueEnum);

        string GetStringRuntimeValue(RuntimeValueEnum runtimeValueEnum);

        void SetIntegerConfigurationValue(ConfigurationValueEnum configurationValueEnum, int value);

        void SetDecimalConfigurationValue(ConfigurationValueEnum configurationValueEnum, decimal value);

        void SetStringConfigurationValue(ConfigurationValueEnum configurationValueEnum, string value);

        void SetIntegerRuntimeValue(RuntimeValueEnum runtimeValueEnum, int value);

        void SetDecimalRuntimeValue(RuntimeValueEnum runtimeValueEnum, decimal value);

        void SetStringRuntimeValue(RuntimeValueEnum runtimeValueEnum, string value);
    }
}
