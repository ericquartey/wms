using System;
using System.Threading.Tasks;

namespace Ferretto.VW.MAS.DataLayer.Interfaces
{
    public interface IDataLayerRuntimeValueManagment
    {
        #region Methods

        /// <summary>
        ///     Get a bool variable from the runtime table in the primary context
        /// </summary>
        /// <param name="runtimeValueEnum">Variable to get value</param>
        /// <param name="categoryValueEnum">Variable category</param>
        /// <returns>Return the value of a bool runtime parameter</returns>
        /// <exception cref="DataLayerPersistentExceptionCode.DATA_CONTEXT_NOT_VALID">Exception when the DataContext is valid</exception>
        /// <exception cref="DataLayerPersistentExceptionCode.PARSE_VALUE">Exception when it is not possible parse a value</exception>
        /// <exception cref="DataLayerPersistentExceptionCode.VALUE_NOT_FOUND">Exception when is been requested a variable no in the DB</exception>
        Task<bool> GetBoolRuntimeValueAsync(long runtimeValueEnum, long categoryValueEnum);

        /// <summary>
        ///     Get a DateTime variable from the runtime table in the primary context
        /// </summary>
        /// <param name="runtimeValueEnum">Variable to get value</param>
        /// <param name="categoryValueEnum">Variable category</param>
        /// <returns>Return the value of a DateTime runtime parameter</returns>
        /// <exception cref="DataLayerPersistentExceptionCode.DATA_CONTEXT_NOT_VALID">Exception when the DataContext is valid</exception>
        /// <exception cref="DataLayerPersistentExceptionCode.PARSE_VALUE">Exception when it is not possible parse a value</exception>
        /// <exception cref="DataLayerPersistentExceptionCode.VALUE_NOT_FOUND">Exception when is been requested a variable no in the DB</exception>
        Task<DateTime> GetDateTimeRuntimeValueAsync(long runtimeValueEnum, long categoryValueEnum);

        /// <summary>
        ///     Get a decimal variable from the runtime table in the primary context
        /// </summary>
        /// <param name="runtimeValueEnum">Variable to get value</param>
        /// <param name="categoryValueEnum">Variable category</param>
        /// <returns>Return the value of a decimal runtime parameter</returns>
        /// <exception cref="DataLayerPersistentExceptionCode.DATA_CONTEXT_NOT_VALID">Exception when the DataContext is valid</exception>
        /// <exception cref="DataLayerPersistentExceptionCode.PARSE_VALUE">Exception when it is not possible parse a value</exception>
        /// <exception cref="DataLayerPersistentExceptionCode.VALUE_NOT_FOUND">Exception when is been requested a variable no in the DB</exception>
        Task<decimal> GetDecimalRuntimeValueAsync(long runtimeValueEnum, long categoryValueEnum);

        /// <summary>
        ///     Get an integer variable from the runtime table in the primary context
        /// </summary>
        /// <param name="runtimeValueEnum">Variable to get value</param>
        /// <param name="categoryValueEnum">Variable category</param>
        /// <returns>Return the value of an integer runtime parameter</returns>
        /// <exception cref="DataLayerPersistentExceptionCode.DATA_CONTEXT_NOT_VALID">Exception when the DataContext is valid</exception>
        /// <exception cref="DataLayerPersistentExceptionCode.PARSE_VALUE">Exception when it is not possible parse a value</exception>
        /// <exception cref="DataLayerPersistentExceptionCode.VALUE_NOT_FOUND">Exception when is been requested a variable no in the DB</exception>
        Task<int> GetIntegerRuntimeValueAsync(long runtimeValueEnum, long categoryValueEnum);

        /// <summary>
        ///     Get a string variable from the runtime table in the primary context
        /// </summary>
        /// <param name="runtimeValueEnum">Variable to get value</param>
        /// <param name="categoryValueEnum">Variable category</param>
        /// <returns>Return the value of a string runtime parameter</returns>
        /// <exception cref="DataLayerPersistentExceptionCode.DATA_CONTEXT_NOT_VALID">Exception when the DataContext is valid</exception>
        /// <exception cref="DataLayerPersistentExceptionCode.VALUE_NOT_FOUND">Exception when is been requested a variable no in the DB</exception>
        Task<string> GetStringRuntimeValueAsync(long runtimeValueEnum, long categoryValueEnum);

        /// <summary>
        ///     Set a bool variable in the runtime table to a new value or update it
        /// </summary>
        /// <param name="runtimeValueEnum">Variable to set the new value</param>
        /// <param name="categoryValueEnum">Variable category</param>
        /// <param name="value">The new variable value</param>
        /// <exception cref="DataLayerExceptionEnum.DatatypeException">Exception for a wrong DataType</exception>
        Task SetBoolRuntimeValueAsync(long runtimeValueEnum, long categoryValueEnum, bool value);

        /// <summary>
        ///     Set a DateTime variable in the runtime table to a new value or update it
        /// </summary>
        /// <param name="runtimeValueEnum">Variable to set the new value</param>
        /// <param name="categoryValueEnum">Variable category</param>
        /// <param name="value">The new variable value</param>
        /// <exception cref="DataLayerExceptionEnum.DatatypeException">Exception for a wrong DataType</exception>
        Task SetDateTimeRuntimeValueAsync(long runtimeValueEnum, long categoryValueEnum, DateTime value);

        /// <summary>
        ///     Set a decimal variable in the runtime table to a new value or update it
        /// </summary>
        /// <param name="runtimeValueEnum">Variable to set the new value</param>
        /// <param name="categoryValueEnum">Variable category</param>
        /// <param name="value">The new variable value</param>
        /// <exception cref="DataLayerExceptionEnum.DatatypeException">Exception for a wrong DataType</exception>
        Task SetDecimalRuntimeValueAsync(long runtimeValueEnum, long categoryValueEnum, decimal value);

        /// <summary>
        ///     Set an integer variable in the runtime table to a new value or update it
        /// </summary>
        /// <param name="runtimeValueEnum">Variable to set the new value</param>
        /// <param name="categoryValueEnum">Variable category</param>
        /// <param name="value">The new variable value</param>
        /// <exception cref="DataLayerExceptionEnum.DatatypeException">Exception for a wrong DataType</exception>
        Task SetIntegerRuntimeValueAsync(long runtimeValueEnum, long categoryValueEnum, int value);

        /// <summary>
        ///     Set a string variable in the runtime table to a new value or update it
        /// </summary>
        /// <param name="runtimeValueEnum">Variable to set the new value</param>
        /// <param name="categoryValueEnum">Variable category</param>
        /// <param name="value">The new variable value</param>
        /// <exception cref="DataLayerExceptionEnum.DatatypeException">Exception for a wrong DataType</exception>
        Task SetStringRuntimeValueAsync(long runtimeValueEnum, long categoryValueEnum, string value);

        #endregion
    }
}
