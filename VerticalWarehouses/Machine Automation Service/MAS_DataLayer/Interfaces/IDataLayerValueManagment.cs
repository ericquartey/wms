using System;
using System.Net;

namespace Ferretto.VW.MAS_DataLayer.Interfaces
{
    public interface IDataLayerValueManagment
    {
        #region Methods

        /// <summary>
        ///     Get a bool variable from the configuration table
        /// </summary>
        /// <param name="configurationValueEnum"></param>
        /// <returns>Return the value of a bool configuration parameter</returns>
        /// <exception cref="DataLayerExceptionEnum.PARSE_EXCEPTION">Exception for a not possible parse</exception>
        /// <exception cref="DataLayerExceptionEnum.DATATYPE_EXCEPTION">Exception for a bad DataType request</exception>
        /// <exception cref="ArgumentNullException">Exception for variable not found</exception>
        bool GetBoolConfigurationValue(long configurationValueEnum, long categoryValueEnum);

        /// <summary>
        ///     Get a bool variable from the runtime table
        /// </summary>
        /// <param name="RuntimeValue"></param>
        /// <returns>Return the value of a bool runtime parameter</returns>
        /// <exception cref="DataLayerExceptionEnum.PARSE_EXCEPTION">Exception for a not possible parse</exception>
        /// <exception cref="DataLayerExceptionEnum.DATATYPE_EXCEPTION">Exception for a bad DataType request</exception>
        /// <exception cref="ArgumentNullException">Exception for variable not found</exception>
        bool GetBoolRuntimeValue(RuntimeValueEnum RuntimeValue);

        /// <summary>
        ///     Get a DateTime variable from the configuration table
        /// </summary>
        /// <param name="configurationValueEnum"></param>
        /// <returns>Return the value of a DateTime configuration parameter</returns>
        /// <exception cref="DataLayerExceptionEnum.PARSE_EXCEPTION">Exception for a not possible parse</exception>
        /// <exception cref="DataLayerExceptionEnum.DATATYPE_EXCEPTION">Exception for a bad DataType request</exception>
        /// <exception cref="ArgumentNullException">Exception for variable not found</exception>
        DateTime GetDateTimeConfigurationValue(long configurationValueEnum, long categoryValueEnum);

        /// <summary>
        ///     Get a DateTime variable from the runtime table
        /// </summary>
        /// <param name="RuntimeValue"></param>
        /// <returns>Return the value of a DateTime runtime parameter</returns>
        /// <exception cref="DataLayerExceptionEnum.PARSE_EXCEPTION">Exception for a not possible parse</exception>
        /// <exception cref="DataLayerExceptionEnum.DATATYPE_EXCEPTION">Exception for a bad DataType request</exception>
        /// <exception cref="ArgumentNullException">Exception for variable not found</exception>
        DateTime GetDateTimeRuntimeValue(RuntimeValueEnum RuntimeValue);

        /// <summary>
        ///     Get a decimal variable from the configuration table
        /// </summary>
        /// <param name="configurationValueEnum">Configuration parameter to get</param>
        /// <returns>Return the value of a decimal configuration parameter</returns>
        /// <exception cref="DataLayerExceptionEnum.PARSE_EXCEPTION">Exception for a not possible parse</exception>
        /// <exception cref="DataLayerExceptionEnum.DATATYPE_EXCEPTION">Exception for a bad DataType request</exception>
        /// <exception cref="ArgumentNullException">Exception for variable not found</exception>
        decimal GetDecimalConfigurationValue(long configurationValueEnum, long categoryValueEnum);

        /// <summary>
        ///     Get a decimal variable from the runtime table
        /// </summary>
        /// <param name="RuntimeValue">Runtime parameter to get</param>
        /// <returns>Return the value of a decimal runtime parameter</returns>
        /// <exception cref="DataLayerExceptionEnum.PARSE_EXCEPTION">Exception for a not possible parse</exception>
        /// <exception cref="DataLayerExceptionEnum.DATATYPE_EXCEPTION">Exception for a bad DataType request</exception>
        /// <exception cref="ArgumentNullException">Exception for variable not found</exception>
        decimal GetDecimalRuntimeValue(RuntimeValueEnum RuntimeValue);

        /// <summary>
        ///     Get an integer variable from the configuration table
        /// </summary>
        /// <param name="configurationValueEnum">Configuration parameter to get</param>
        /// <returns>Return the value of an integer configuration parameter</returns>
        /// <exception cref="DataLayerExceptionEnum.PARSE_EXCEPTION">Exception for a not possible parse</exception>
        /// <exception cref="DataLayerExceptionEnum.DATATYPE_EXCEPTION">Exception for a bad DataType request</exception>
        /// <exception cref="ArgumentNullException">Exception for variable not found</exception>
        int GetIntegerConfigurationValue(long configurationValueEnum, long categoryValueEnum);

        /// <summary>
        ///     Get an integer variable from the runtime table
        /// </summary>
        /// <param name="RuntimeValue">Runtime parameter to get</param>
        /// <returns>Return the value of an integer runtime parameter</returns>
        /// <exception cref="DataLayerExceptionEnum.PARSE_EXCEPTION">Exception for a not possible parse</exception>
        /// <exception cref="DataLayerExceptionEnum.DATATYPE_EXCEPTION">Exception for a bad DataType request</exception>
        /// <exception cref="ArgumentNullException">Exception for variable not found</exception>
        int GetIntegerRuntimeValue(RuntimeValueEnum RuntimeValue);

        /// <summary>
        /// Get an IPAddress variable from the runtime table
        /// </summary>
        /// <param name="configurationValueEnum">Configuration parameter to get</param>
        /// <returns>Return the value of an IPAddfress configuration parameter</returns>
        /// <exception cref="DataLayerExceptionEnum.PARSE_EXCEPTION">Exception for a not possible parse</exception>
        /// <exception cref="DataLayerExceptionEnum.DATATYPE_EXCEPTION">Exception for a bad DataType request</exception>
        /// <exception cref="ArgumentNullException">Exception for variable not found</exception>
        IPAddress GetIPAddressConfigurationValue(long configurationValueEnum, long categoryValueEnum);

        /// <summary>
        ///     Get a string variable from the configuration table
        /// </summary>
        /// <param name="configurationValueEnum">Configuration parameter to get</param>
        /// <returns>Return the value of a string configuration parameter</returns>
        /// <exception cref="DataLayerExceptionEnum.DATATYPE_EXCEPTION">Exception for a bad DataType request</exception>
        /// <exception cref="ArgumentNullException">Exception for variable not found</exception>
        string GetStringConfigurationValue(long configurationValueEnum, long categoryValueEnum);

        /// <summary>
        ///     Get a string variable from the runtime table
        /// </summary>
        /// <param name="RuntimeValue">Runtime parameter to get</param>
        /// <returns>Return the value of a string runtime parameter</returns>
        /// <exception cref="DataLayerExceptionEnum.DATATYPE_EXCEPTION">Exception for a bad DataType request</exception>
        /// <exception cref="ArgumentNullException">Exception for variable not found</exception>
        string GetStringRuntimeValue(RuntimeValueEnum RuntimeValue);

        /// <summary>
        ///     Set a bool variable in the configuration table to a new value or update it
        /// </summary>
        /// <param name="configurationValueEnum">Configuration parameter to set</param>
        /// <param name="value">The new value</param>
        /// <exception cref="DataLayerExceptionEnum.DATATYPE_EXCEPTION">Exception for a wrong DataType</exception>
        void SetBoolConfigurationValue(long configurationValueEnum, long categoryValueEnum, bool value);

        /// <summary>
        ///     Set a bool variable in the runtime table to a new value or update it
        /// </summary>
        /// <param name="RuntimeValue">Runtime parameter to set</param>
        /// <param name="value">The new value</param>
        /// <exception cref="DataLayerExceptionEnum.DATATYPE_EXCEPTION">Exception for a wrong DataType</exception>
        void SetBoolRuntimeValue(RuntimeValueEnum RuntimeValue, bool value);

        /// <summary>
        ///     Set a DateTime variable in the configuration table to a new value or update it
        /// </summary>
        /// <param name="RuntimeValue">Configuration parameter to set</param>
        /// <param name="value">The new value</param>
        /// <exception cref="DataLayerExceptionEnum.DATATYPE_EXCEPTION">Exception for a wrong DataType</exception>
        void SetDateTimeConfigurationValue(long configurationValueEnum, long categoryValueEnum, DateTime value);

        /// <summary>
        ///     Set a DateTime variable in the runtime table to a new value or update it
        /// </summary>
        /// <param name="RuntimeValue">Runtime parameter to set</param>
        /// <param name="value">The new value</param>
        /// <exception cref="DataLayerExceptionEnum.DATATYPE_EXCEPTION">Exception for a wrong DataType</exception>
        void SetDateTimeRuntimeValue(RuntimeValueEnum RuntimeValue, DateTime value);

        /// <summary>
        ///     Set a decimal variable in the configuration table to a new value or update it
        /// </summary>
        /// <param name="configurationValueEnum">Configuration parameter to set</param>
        /// <param name="value">The new value</param>
        /// <exception cref="DataLayerExceptionEnum.DATATYPE_EXCEPTION">Exception for a wrong DataType</exception>
        void SetDecimalConfigurationValue(long configurationValueEnum, long categoryValueEnum, decimal value);

        /// <summary>
        ///     Set a decimal variable in the runtime table to a new value or update it
        /// </summary>
        /// <param name="RuntimeValue">Runtime parameter to set</param>
        /// <param name="value">The new value</param>
        /// <exception cref="DataLayerExceptionEnum.DATATYPE_EXCEPTION">Exception for a wrong DataType</exception>
        void SetDecimalRuntimeValue(RuntimeValueEnum RuntimeValue, decimal value);

        /// <summary>
        ///     Set an integer variable in the configuration table to a new value or update it
        /// </summary>
        /// <param name="configurationValueEnum">Configuration parameter to set</param>
        /// <param name="value">The new value</param>
        /// <exception cref="DataLayerExceptionEnum.DATATYPE_EXCEPTION">Exception for a wrong DataType</exception>
        void SetIntegerConfigurationValue(long configurationValueEnum, long categoryValueEnum, int value);

        /// <summary>
        ///     Set an integer variable in the runtime table to a new value or update it
        /// </summary>
        /// <param name="RuntimeValue">Runtime parameter to set</param>
        /// <param name="value">The new value</param>
        /// <exception cref="DataLayerExceptionEnum.DATATYPE_EXCEPTION">Exception for a wrong DataType</exception>
        void SetIntegerRuntimeValue(RuntimeValueEnum RuntimeValue, int value);

        /// <summary>
        ///     Set a string variable in the configuration table to a new value or update it
        /// </summary>
        /// <param name="configurationValueEnum">Configuration parameter to set</param>
        /// <param name="value">The new value</param>
        /// <exception cref="DataLayerExceptionEnum.DATATYPE_EXCEPTION">Exception for a wrong DataType</exception>
        void SetStringConfigurationValue(long configurationValueEnum, long categoryValueEnum, string value);

        /// <summary>
        ///     Set a string variable in the runtime table to a new value or update it
        /// </summary>
        /// <param name="RuntimeValue">Runtime parameter to set</param>
        /// <param name="value">The new value</param>
        /// <exception cref="DataLayerExceptionEnum.DATATYPE_EXCEPTION">Exception for a wrong DataType</exception>
        void SetStringRuntimeValue(RuntimeValueEnum RuntimeValue, string value);

        #endregion
    }
}
