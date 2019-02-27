using System;
using System.Collections.Generic;
using System.Net;
using Ferretto.VW.Common_Utils;

namespace Ferretto.VW.MAS_DataLayer
{
    public interface IDataLayer
    {
        #region Methods

        /// <summary>
        ///     Get a list of cells from the configuration table
        /// </summary>
        /// <returns>Return a list of cell</returns>
        List<Cell> GetCellList();

        /// <summary>
        ///     Get a decimal variable from the configuration table
        /// </summary>
        /// <param name="configurationValueEnum">Configuration parameter to get</param>
        /// <returns>Return the value of a decimal configuration parameter</returns>
        /// <exception cref="DataLayerExceptionEnum.PARSE_EXCEPTION">Exception for a not possible parse</exception>
        /// <exception cref="DataLayerExceptionEnum.DATATYPE_EXCEPTION">Exception for a bad DataType request</exception>
        /// <exception cref="ArgumentNullException">Exception for variable not found</exception>
        decimal GetDecimalConfigurationValue(ConfigurationValueEnum configurationValueEnum);

        /// <summary>
        ///     Get a decimal variable from the runtime table
        /// </summary>
        /// <param name="runtimeValueEnum">Runtime parameter to get</param>
        /// <returns>Return the value of a decimal runtime parameter</returns>
        /// <exception cref="DataLayerExceptionEnum.PARSE_EXCEPTION">Exception for a not possible parse</exception>
        /// <exception cref="DataLayerExceptionEnum.DATATYPE_EXCEPTION">Exception for a bad DataType request</exception>
        /// <exception cref="ArgumentNullException">Exception for variable not found</exception>
        decimal GetDecimalRuntimeValue(RuntimeValueEnum runtimeValueEnum);

        /// <summary>
        ///     Get an object with the vertical position and side to place a drawer
        /// </summary>
        /// <param name="drawerHeight">Drawer height to insert in the magazine</param>
        /// <returns>An object with position and side for a return mission</returns>
        ReturnMissionPosition GetFreeBlockPosition(decimal drawerHeight);

        /// <summary>
        ///     Get an integer variable from the configuration table
        /// </summary>
        /// <param name="configurationValueEnum">Configuration parameter to get</param>
        /// <returns>Return the value of an integer configuration parameter</returns>
        /// <exception cref="DataLayerExceptionEnum.PARSE_EXCEPTION">Exception for a not possible parse</exception>
        /// <exception cref="DataLayerExceptionEnum.DATATYPE_EXCEPTION">Exception for a bad DataType request</exception>
        /// <exception cref="ArgumentNullException">Exception for variable not found</exception>
        int GetIntegerConfigurationValue(ConfigurationValueEnum configurationValueEnum);

        /// <summary>
        ///     Get an integer variable from the runtime table
        /// </summary>
        /// <param name="runtimeValueEnum">Runtime parameter to get</param>
        /// <returns>Return the value of an integer runtime parameter</returns>
        /// <exception cref="DataLayerExceptionEnum.PARSE_EXCEPTION">Exception for a not possible parse</exception>
        /// <exception cref="DataLayerExceptionEnum.DATATYPE_EXCEPTION">Exception for a bad DataType request</exception>
        /// <exception cref="ArgumentNullException">Exception for variable not found</exception>
        int GetIntegerRuntimeValue(RuntimeValueEnum runtimeValueEnum);

        /// <summary>
        /// Get an IPAddress variable from the runtime table
        /// </summary>
        /// <param name="configurationValueEnum">Configuration parameter to get</param>
        /// <returns>Return the value of an IPAddfress configuration parameter</returns>
        /// <exception cref="DataLayerExceptionEnum.PARSE_EXCEPTION">Exception for a not possible parse</exception>
        /// <exception cref="DataLayerExceptionEnum.DATATYPE_EXCEPTION">Exception for a bad DataType request</exception>
        /// <exception cref="ArgumentNullException">Exception for variable not found</exception>
        IPAddress GetIPAddressConfigurationValue(ConfigurationValueEnum configurationValueEnum);

        /// <summary>
        ///     Get a string variable from the configuration table
        /// </summary>
        /// <param name="configurationValueEnum">Configuration parameter to get</param>
        /// <returns>Return the value of a string configuration parameter</returns>
        /// <exception cref="DataLayerExceptionEnum.DATATYPE_EXCEPTION">Exception for a bad DataType request</exception>
        /// <exception cref="ArgumentNullException">Exception for variable not found</exception>
        string GetStringConfigurationValue(ConfigurationValueEnum configurationValueEnum);

        /// <summary>
        ///     Get a string variable from the runtime table
        /// </summary>
        /// <param name="runtimeValueEnum">Runtime parameter to get</param>
        /// <returns>Return the value of a string runtime parameter</returns>
        /// <exception cref="DataLayerExceptionEnum.DATATYPE_EXCEPTION">Exception for a bad DataType request</exception>
        /// <exception cref="ArgumentNullException">Exception for variable not found</exception>
        string GetStringRuntimeValue(RuntimeValueEnum runtimeValueEnum);

        /// <summary>
        ///     Set one or more cells to a list cell to new value
        /// </summary>
        /// <param name="listCells">A list of cells</param>
        /// <returns>A boolean value about the set outcome</returns>
        /// <exception cref="ArgumentNullException">Exception when there is not a variable of list in the table</exception>
        bool SetCellList(List<Cell> listCells);

        /// <summary>
        ///     Set a decimal variable in the configuration table to a new value
        /// </summary>
        /// <param name="configurationValueEnum">Configuration parameter to set</param>
        /// <param name="value">The new value</param>
        /// <exception cref="DataLayerExceptionEnum.DATATYPE_EXCEPTION">Exception for a wrong DataType</exception>
        void SetDecimalConfigurationValue(ConfigurationValueEnum configurationValueEnum, decimal value);

        /// <summary>
        ///     Set a decimal variable in the runtime table to a new value
        /// </summary>
        /// <param name="runtimeValueEnum">Runtime parameter to set</param>
        /// <param name="value">The new value</param>
        /// <exception cref="DataLayerExceptionEnum.DATATYPE_EXCEPTION">Exception for a wrong DataType</exception>
        void SetDecimalRuntimeValue(RuntimeValueEnum runtimeValueEnum, decimal value);

        /// <summary>
        ///     Set an integer variable in the configuration table to a new value
        /// </summary>
        /// <param name="configurationValueEnum">Configuration parameter to set</param>
        /// <param name="value">The new value</param>
        /// <exception cref="DataLayerExceptionEnum.DATATYPE_EXCEPTION">Exception for a wrong DataType</exception>
        void SetIntegerConfigurationValue(ConfigurationValueEnum configurationValueEnum, int value);

        /// <summary>
        ///     Set an integer variable in the runtime table to a new value
        /// </summary>
        /// <param name="runtimeValueEnum">Runtime parameter to set</param>
        /// <param name="value">The new value</param>
        /// <exception cref="DataLayerExceptionEnum.DATATYPE_EXCEPTION">Exception for a wrong DataType</exception>
        void SetIntegerRuntimeValue(RuntimeValueEnum runtimeValueEnum, int value);

        /// <summary>
        ///     Set a string variable in the configuration table to a new value
        /// </summary>
        /// <param name="configurationValueEnum">Configuration parameter to set</param>
        /// <param name="value">The new value</param>
        /// <exception cref="DataLayerExceptionEnum.DATATYPE_EXCEPTION">Exception for a wrong DataType</exception>
        void SetStringConfigurationValue(ConfigurationValueEnum configurationValueEnum, string value);

        /// <summary>
        ///     Set a string variable in the runtime table to a new value
        /// </summary>
        /// <param name="runtimeValueEnum">Runtime parameter to set</param>
        /// <param name="value">The new value</param>
        /// <exception cref="DataLayerExceptionEnum.DATATYPE_EXCEPTION">Exception for a wrong DataType</exception>
        void SetStringRuntimeValue(RuntimeValueEnum runtimeValueEnum, string value);

        #endregion
    }
}
