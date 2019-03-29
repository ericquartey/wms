using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;

namespace Ferretto.VW.MAS_DataLayer.Interfaces
{
    public interface IDataLayerValueManagment
    {
        #region Methods

        /// <summary>
        ///     Get a bool variable from the configuration table
        /// </summary>
        /// <param name="configurationValueEnum"></param>
        /// <param name="categoryValueEnum"></param>
        /// <returns>Return the value of a bool configuration parameter</returns>
        /// <exception cref="DataLayerExceptionEnum.PARSE_EXCEPTION">Exception for a not possible parse</exception>
        /// <exception cref="DataLayerExceptionEnum.DATATYPE_EXCEPTION">Exception for a bad DataType request</exception>
        /// <exception cref="ArgumentNullException">Exception for variable not found</exception>
        Task<bool> GetBoolConfigurationValueAsync(long configurationValueEnum, long categoryValueEnum);

        /// <summary>
        ///     Get a bool variable from the runtime table
        /// </summary>
        /// <param name="RuntimeValue"></param>
        /// <param name="categoryValueEnum"></param>
        /// <returns>Return the value of a bool runtime parameter</returns>
        /// <exception cref="DataLayerExceptionEnum.PARSE_EXCEPTION">Exception for a not possible parse</exception>
        /// <exception cref="DataLayerExceptionEnum.DATATYPE_EXCEPTION">Exception for a bad DataType request</exception>
        /// <exception cref="ArgumentNullException">Exception for variable not found</exception>
        Task<bool> GetBoolRuntimeValueAsync(long runtimeValueEnum, long categoryValueEnum);

        /// <summary>
        ///     Get a DateTime variable from the configuration table
        /// </summary>
        /// <param name="configurationValueEnum"></param>
        /// <param name="categoryValueEnum"></param>
        /// <returns>Return the value of a DateTime configuration parameter</returns>
        /// <exception cref="DataLayerExceptionEnum.PARSE_EXCEPTION">Exception for a not possible parse</exception>
        /// <exception cref="DataLayerExceptionEnum.DATATYPE_EXCEPTION">Exception for a bad DataType request</exception>
        /// <exception cref="ArgumentNullException">Exception for variable not found</exception>
        Task<DateTime> GetDateTimeConfigurationValueAsync(long configurationValueEnum, long categoryValueEnum);

        /// <summary>
        ///     Get a DateTime variable from the runtime table
        /// </summary>
        /// <param name="runtimeValueEnum"></param>
        /// <param name="categoryValueEnum"></param>
        /// <param name="RuntimeValue"></param>
        /// <returns>Return the value of a DateTime runtime parameter</returns>
        /// <exception cref="DataLayerExceptionEnum.PARSE_EXCEPTION">Exception for a not possible parse</exception>
        /// <exception cref="DataLayerExceptionEnum.DATATYPE_EXCEPTION">Exception for a bad DataType request</exception>
        /// <exception cref="ArgumentNullException">Exception for variable not found</exception>
        Task<DateTime> GetDateTimeRuntimeValueAsync(long runtimeValueEnum, long categoryValueEnum);

        /// <summary>
        ///     Get a decimal variable from the configuration table
        /// </summary>
        /// <param name="configurationValueEnum">Configuration parameter to get</param>
        /// <param name="categoryValueEnum"></param>
        /// <returns>Return the value of a decimal configuration parameter</returns>
        /// <exception cref="DataLayerExceptionEnum.PARSE_EXCEPTION">Exception for a not possible parse</exception>
        /// <exception cref="DataLayerExceptionEnum.DATATYPE_EXCEPTION">Exception for a bad DataType request</exception>
        /// <exception cref="ArgumentNullException">Exception for variable not found</exception>
        Task<decimal> GetDecimalConfigurationValueAsync(long configurationValueEnum, long categoryValueEnum);

        /// <summary>
        ///     Get a decimal variable from the runtime table
        /// </summary>
        /// <param name="runtimeValueEnum"></param>
        /// <param name="categoryValueEnum"></param>
        /// <param name="RuntimeValue">Runtime parameter to get</param>
        /// <returns>Return the value of a decimal runtime parameter</returns>
        /// <exception cref="DataLayerExceptionEnum.PARSE_EXCEPTION">Exception for a not possible parse</exception>
        /// <exception cref="DataLayerExceptionEnum.DATATYPE_EXCEPTION">Exception for a bad DataType request</exception>
        /// <exception cref="ArgumentNullException">Exception for variable not found</exception>
        Task<decimal> GetDecimalRuntimeValueAsync(long runtimeValueEnum, long categoryValueEnum);

        /// <summary>
        ///     Get an integer variable from the configuration table
        /// </summary>
        /// <param name="configurationValueEnum">Configuration parameter to get</param>
        /// <param name="categoryValueEnum"></param>
        /// <returns>Return the value of an integer configuration parameter</returns>
        /// <exception cref="DataLayerExceptionEnum.PARSE_EXCEPTION">Exception for a not possible parse</exception>
        /// <exception cref="DataLayerExceptionEnum.DATATYPE_EXCEPTION">Exception for a bad DataType request</exception>
        /// <exception cref="ArgumentNullException">Exception for variable not found</exception>
        Task<int> GetIntegerConfigurationValueAsync(long configurationValueEnum, long categoryValueEnum);

        /// <summary>
        ///     Get an integer variable from the runtime table
        /// </summary>
        /// <param name="runtimeValueEnum"></param>
        /// <param name="categoryValueEnum"></param>
        /// <param name="RuntimeValue">Runtime parameter to get</param>
        /// <returns>Return the value of an integer runtime parameter</returns>
        /// <exception cref="DataLayerExceptionEnum.PARSE_EXCEPTION">Exception for a not possible parse</exception>
        /// <exception cref="DataLayerExceptionEnum.DATATYPE_EXCEPTION">Exception for a bad DataType request</exception>
        /// <exception cref="ArgumentNullException">Exception for variable not found</exception>
        Task<int> GetIntegerRuntimeValueAsync(long runtimeValueEnum, long categoryValueEnum);

        /// <summary>
        /// Get an IPAddress variable from the runtime table
        /// </summary>
        /// <param name="configurationValueEnum">Configuration parameter to get</param>
        /// <param name="categoryValueEnum"></param>
        /// <returns>Return the value of an IPAddfress configuration parameter</returns>
        /// <exception cref="DataLayerExceptionEnum.PARSE_EXCEPTION">Exception for a not possible parse</exception>
        /// <exception cref="DataLayerExceptionEnum.DATATYPE_EXCEPTION">Exception for a bad DataType request</exception>
        /// <exception cref="ArgumentNullException">Exception for variable not found</exception>
        Task<IPAddress> GetIPAddressConfigurationValueAsync(long configurationValueEnum, long categoryValueEnum);

        /// <summary>
        ///     Get a string variable from the configuration table
        /// </summary>
        /// <param name="configurationValueEnum">Configuration parameter to get</param>
        /// <param name="categoryValueEnum"></param>
        /// <returns>Return the value of a string configuration parameter</returns>
        /// <exception cref="DataLayerExceptionEnum.DATATYPE_EXCEPTION">Exception for a bad DataType request</exception>
        /// <exception cref="ArgumentNullException">Exception for variable not found</exception>
        Task<string> GetStringConfigurationValueAsync(long configurationValueEnum, long categoryValueEnum);

        /// <summary>
        ///     Get a string variable from the runtime table
        /// </summary>
        /// <param name="runtimeValueEnum"></param>
        /// <param name="categoryValueEnum"></param>
        /// <param name="RuntimeValue">Runtime parameter to get</param>
        /// <returns>Return the value of a string runtime parameter</returns>
        /// <exception cref="DataLayerExceptionEnum.DATATYPE_EXCEPTION">Exception for a bad DataType request</exception>
        /// <exception cref="ArgumentNullException">Exception for variable not found</exception>
        Task<string> GetStringRuntimeValueAsync(long runtimeValueEnum, long categoryValueEnum);

        /// <summary>
        ///     Set a bool variable in the configuration table to a new value or update it
        /// </summary>
        /// <param name="configurationValueEnum">Configuration parameter to set</param>
        /// <param name="value">The new value</param>
        /// <exception cref="DataLayerExceptionEnum.DATATYPE_EXCEPTION">Exception for a wrong DataType</exception>
        Task SetBoolConfigurationValueAsync(long configurationValueEnum, long categoryValueEnum, bool value);

        /// <summary>
        ///     Set a bool variable in the runtime table to a new value or update it
        /// </summary>
        /// <param name="RuntimeValue">Runtime parameter to set</param>
        /// <param name="value">The new value</param>
        /// <exception cref="DataLayerExceptionEnum.DATATYPE_EXCEPTION">Exception for a wrong DataType</exception>
        Task SetBoolRuntimeValueAsync(long runtimeValueEnum, long categoryValueEnum, bool value);

        /// <summary>
        ///     Set a DateTime variable in the configuration table to a new value or update it
        /// </summary>
        /// <param name="RuntimeValue">Configuration parameter to set</param>
        /// <param name="value">The new value</param>
        /// <exception cref="DataLayerExceptionEnum.DATATYPE_EXCEPTION">Exception for a wrong DataType</exception>
        Task SetDateTimeConfigurationValueAsync(long configurationValueEnum, long categoryValueEnum, DateTime value);

        /// <summary>
        ///     Set a DateTime variable in the runtime table to a new value or update it
        /// </summary>
        /// <param name="RuntimeValue">Runtime parameter to set</param>
        /// <param name="value">The new value</param>
        /// <exception cref="DataLayerExceptionEnum.DATATYPE_EXCEPTION">Exception for a wrong DataType</exception>
        Task SetDateTimeRuntimeValueAsync(long runtimeValueEnum, long categoryValueEnum, DateTime value);

        /// <summary>
        ///     Set a decimal variable in the configuration table to a new value or update it
        /// </summary>
        /// <param name="configurationValueEnum">Configuration parameter to set</param>
        /// <param name="value">The new value</param>
        /// <exception cref="DataLayerExceptionEnum.DATATYPE_EXCEPTION">Exception for a wrong DataType</exception>
        Task SetDecimalConfigurationValueAsync(long configurationValueEnum, long categoryValueEnum, decimal value);

        /// <summary>
        ///     Set a decimal variable in the runtime table to a new value or update it
        /// </summary>
        /// <param name="RuntimeValue">Runtime parameter to set</param>
        /// <param name="value">The new value</param>
        /// <exception cref="DataLayerExceptionEnum.DATATYPE_EXCEPTION">Exception for a wrong DataType</exception>
        Task SetDecimalRuntimeValueAsync(long runtimeValueEnum, long categoryValueEnum, decimal value);

        /// <summary>
        ///     Set an integer variable in the configuration table to a new value or update it
        /// </summary>
        /// <param name="configurationValueEnum">Configuration parameter to set</param>
        /// <param name="value">The new value</param>
        /// <exception cref="DataLayerExceptionEnum.DATATYPE_EXCEPTION">Exception for a wrong DataType</exception>
        Task SetIntegerConfigurationValueAsync(long configurationValueEnum, long categoryValueEnum, int value);

        /// <summary>
        ///     Set an integer variable in the runtime table to a new value or update it
        /// </summary>
        /// <param name="RuntimeValue">Runtime parameter to set</param>
        /// <param name="value">The new value</param>
        /// <exception cref="DataLayerExceptionEnum.DATATYPE_EXCEPTION">Exception for a wrong DataType</exception>
        Task SetIntegerRuntimeValueAsync(long runtimeValueEnum, long categoryValueEnum, int value);

        /// <summary>
        ///     Set a string variable in the configuration table to a new value or update it
        /// </summary>
        /// <param name="configurationValueEnum">Configuration parameter to set</param>
        /// <param name="value">The new value</param>
        /// <exception cref="DataLayerExceptionEnum.DATATYPE_EXCEPTION">Exception for a wrong DataType</exception>
        Task SetStringConfigurationValueAsync(long configurationValueEnum, long categoryValueEnum, string value);

        /// <summary>
        ///     Set a string variable in the runtime table to a new value or update it
        /// </summary>
        /// <param name="RuntimeValue">Runtime parameter to set</param>
        /// <param name="value">The new value</param>
        /// <exception cref="DataLayerExceptionEnum.DATATYPE_EXCEPTION">Exception for a wrong DataType</exception>
        Task SetStringRuntimeValueAsync(long runtimeValueEnum, long categoryValueEnum, string value);

        #endregion
    }
}
