using System;
using System.Net;
using System.Threading.Tasks;

namespace Ferretto.VW.MAS.DataLayer.Interfaces
{
    public interface IDataLayerConfigurationValueManagment
    {
        #region Methods

        /// <summary>
        ///     Get a bool variable from the configuration table in the primary context
        /// </summary>
        /// <param name="configurationValueEnum">Variable to get the new value</param>
        /// <param name="categoryValueEnum">Variable category</param>
        /// <returns>Return the value of a bool configuration parameter</returns>
        /// <exception cref="DataLayerPersistentExceptionCode.DATA_CONTEXT_NOT_VALID">Exception when the DataContext is valid</exception>
        /// <exception cref="DataLayerPersistentExceptionCode.PARSE_VALUE">Exception when it is not possible parse a value</exception>
        /// <exception cref="DataLayerPersistentExceptionCode.VALUE_NOT_FOUND">Exception when is been requested a variable no in the DB</exception>
        bool GetBoolConfigurationValue(long configurationValueEnum, long categoryValueEnum);

        /// <summary>
        ///     Get a DateTime variable from the configuration table in the primary context
        /// </summary>
        /// <param name="configurationValueEnum">Variable to get the new value</param>
        /// <param name="categoryValueEnum">Variable category</param>
        /// <returns>Return the value of a DateTime configuration parameter</returns>
        /// <exception cref="DataLayerPersistentExceptionCode.DATA_CONTEXT_NOT_VALID">Exception when the DataContext is valid</exception>
        /// <exception cref="DataLayerPersistentExceptionCode.PARSE_VALUE">Exception when it is not possible parse a value</exception>
        /// <exception cref="DataLayerPersistentExceptionCode.VALUE_NOT_FOUND">Exception when is been requested a variable no in the DB</exception>
        DateTime GetDateTimeConfigurationValue(long configurationValueEnum, long categoryValueEnum);

        /// <summary>
        ///     Get a decimal variable from the configuration table in the primary context
        /// </summary>
        /// <param name="configurationValueEnum">Variable to get the new value</param>
        /// <param name="categoryValueEnum">Variable category</param>
        /// <returns>Return the value of a decimal configuration parameter</returns>
        /// <exception cref="DataLayerPersistentExceptionCode.DATA_CONTEXT_NOT_VALID">Exception when the DataContext is valid</exception>
        /// <exception cref="DataLayerPersistentExceptionCode.PARSE_VALUE">Exception when it is not possible parse a value</exception>
        /// <exception cref="DataLayerPersistentExceptionCode.VALUE_NOT_FOUND">Exception when is been requested a variable no in the DB</exception>
        decimal GetDecimalConfigurationValue(long configurationValueEnum, long categoryValueEnum);

        /// <summary>
        ///     Get an integer variable from the configuration table in the primary context
        /// </summary>
        /// <param name="configurationValueEnum">Variable to get the new value</param>
        /// <param name="categoryValueEnum">Variable category</param>
        /// <returns>Return the value of an integer configuration parameter</returns>
        /// <exception cref="DataLayerPersistentExceptionCode.DATA_CONTEXT_NOT_VALID">Exception when the DataContext is valid</exception>
        /// <exception cref="DataLayerPersistentExceptionCode.PARSE_VALUE">Exception when it is not possible parse a value</exception>
        /// <exception cref="DataLayerPersistentExceptionCode.VALUE_NOT_FOUND">Exception when is been requested a variable no in the DB</exception>
        int GetIntegerConfigurationValue(long configurationValueEnum, long categoryValueEnum);

        /// <summary>
        ///     Get an IPAddress variable from the runtime table in the primary context
        /// </summary>
        /// <param name="configurationValueEnum">Variable to get the new value</param>
        /// <param name="categoryValueEnum">Variable category</param>
        /// <returns>Return the value of an IPAddfress configuration parameter</returns>
        /// <exception cref="DataLayerPersistentExceptionCode.DATA_CONTEXT_NOT_VALID">Exception when the DataContext is valid</exception>
        /// <exception cref="DataLayerPersistentExceptionCode.PARSE_VALUE">Exception when it is not possible parse a value</exception>
        /// <exception cref="DataLayerPersistentExceptionCode.VALUE_NOT_FOUND">Exception when is been requested a variable no in the DB</exception>
        IPAddress GetIpAddressConfigurationValue(long configurationValueEnum, long categoryValueEnum);

        /// <summary>
        ///     Get a string variable from the configuration table in the primary context
        /// </summary>
        /// <param name="configurationValueEnum">Variable to get the new value</param>
        /// <param name="categoryValueEnum">Variable category</param>
        /// <returns>Return the value of a string configuration parameter</returns>
        /// <exception cref="DataLayerPersistentExceptionCode.DATA_CONTEXT_NOT_VALID">Exception when the DataContext is valid</exception>
        /// <exception cref="DataLayerPersistentExceptionCode.VALUE_NOT_FOUND">Exception when is been requested a variable no in the DB</exception>
        string GetStringConfigurationValue(long configurationValueEnum, long categoryValueEnum);

        /// <summary>
        ///     Set a bool variable in the configuration table to a new value or update it
        /// </summary>
        /// <param name="configurationValueEnum">Variable to set the new value</param>
        /// <param name="categoryValueEnum">Variable category</param>
        /// <param name="value">The new variable value</param>
        /// <exception cref="DataLayerExceptionEnum.DatatypeException">Exception for a wrong DataType</exception>
        void SetBoolConfigurationValue(long configurationValueEnum, long categoryValueEnum, bool value);

        /// <summary>
        ///     Set a DateTime variable in the configuration table to a new value or update it
        /// </summary>
        /// <param name="configurationValueEnum">Variable to set the new value</param>
        /// <param name="categoryValueEnum">Variable category</param>
        /// <param name="value">The new variable value</param>
        /// <exception cref="DataLayerExceptionEnum.DatatypeException">Exception for a wrong DataType</exception>
        void SetDateTimeConfigurationValue(long configurationValueEnum, long categoryValueEnum, DateTime value);

        /// <summary>
        ///     Set a decimal variable in the configuration table to a new value or update it
        /// </summary>
        /// <param name="configurationValueEnum">Variable to set the new value</param>
        /// <param name="categoryValueEnum">Variable category</param>
        /// <param name="value">The new variable value</param>
        /// <exception cref="DataLayerExceptionEnum.DatatypeException">Exception for a wrong DataType</exception>
        void SetDecimalConfigurationValue(long configurationValueEnum, long categoryValueEnum, decimal value);

        /// <summary>
        ///     Set an integer variable in the configuration table to a new value or update it
        /// </summary>
        /// <param name="configurationValueEnum">Variable to set the new value</param>
        /// <param name="categoryValueEnum">Variable category</param>
        /// <param name="value">The new variable value</param>
        /// <exception cref="DataLayerExceptionEnum.DatatypeException">Exception for a wrong DataType</exception>
        void SetIntegerConfigurationValue(long configurationValueEnum, long categoryValueEnum, int value);

        /// <summary>
        ///     Set a string variable in the configuration table to a new value or update it
        /// </summary>
        /// <param name="configurationValueEnum">Variable to set the new value</param>
        /// <param name="categoryValueEnum">Variable category</param>
        /// <param name="value">The new variable value</param>
        /// <exception cref="DataLayerExceptionEnum.DatatypeException">Exception for a wrong DataType</exception>
        void SetStringConfigurationValue(long configurationValueEnum, long categoryValueEnum, string value);

        #endregion
    }
}
