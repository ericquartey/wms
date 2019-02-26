using System;
using System.Collections.Generic;

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
        Decimal GetDecimalConfigurationValue(ConfigurationValueEnum configurationValueEnum);

        /// <summary>
        ///     Get a decimal variable from the runtime table
        /// </summary>
        /// <param name="runtimeValueEnum">Runtime parameter to get</param>
        /// <returns>Return the value of a decimal runtime parameter</returns>
        /// <exception cref="DataLayerExceptionEnum.PARSE_EXCEPTION">Exception for a not possible parse</exception>
        /// <exception cref="DataLayerExceptionEnum.DATATYPE_EXCEPTION">Exception for a bad DataType request</exception>
        /// <exception cref="ArgumentNullException">Exception for variable not found</exception>
        Decimal GetDecimalRuntimeValue(RuntimeValueEnum runtimeValueEnum);

        /// <summary>
        ///     Get an object with the vertical position and side to place a drawer
        /// </summary>
        /// <param name="drawerHeight">Drawer height to insert in the magazine</param>
        /// <returns>An object with position and side for a return mission</returns>
        ReturnMissionPosition GetFreeBlockPosition(Decimal drawerHeight);

        /// <summary>
        ///     Get an integer variable from the configuration table
        /// </summary>
        /// <param name="configurationValueEnum">Configuration parameter to get</param>
        /// <returns>Return the value of an integer configuration parameter</returns>
        /// <exception cref="DataLayerExceptionEnum.PARSE_EXCEPTION">Exception for a not possible parse</exception>
        /// <exception cref="DataLayerExceptionEnum.DATATYPE_EXCEPTION">Exception for a bad DataType request</exception>
        /// <exception cref="ArgumentNullException">Exception for variable not found</exception>
        Int32 GetIntegerConfigurationValue(ConfigurationValueEnum configurationValueEnum);

        /// <summary>
        ///     Get an integer variable from the runtime table
        /// </summary>
        /// <param name="runtimeValueEnum">Runtime parameter to get</param>
        /// <returns>Return the value of an integer runtime parameter</returns>
        /// <exception cref="DataLayerExceptionEnum.PARSE_EXCEPTION">Exception for a not possible parse</exception>
        /// <exception cref="DataLayerExceptionEnum.DATATYPE_EXCEPTION">Exception for a bad DataType request</exception>
        /// <exception cref="ArgumentNullException">Exception for variable not found</exception>
        Int32 GetIntegerRuntimeValue(RuntimeValueEnum runtimeValueEnum);

        /// <summary>
        ///     Get a string variable from the configuration table
        /// </summary>
        /// <param name="configurationValueEnum">Configuration parameter to get</param>
        /// <returns>Return the value of a string configuration parameter</returns>
        /// <exception cref="DataLayerExceptionEnum.DATATYPE_EXCEPTION">Exception for a bad DataType request</exception>
        /// <exception cref="ArgumentNullException">Exception for variable not found</exception>
        String GetStringConfigurationValue(ConfigurationValueEnum configurationValueEnum);

        /// <summary>
        ///     Get a string variable from the runtime table
        /// </summary>
        /// <param name="runtimeValueEnum">Runtime parameter to get</param>
        /// <returns>Return the value of a string runtime parameter</returns>
        /// <exception cref="DataLayerExceptionEnum.DATATYPE_EXCEPTION">Exception for a bad DataType request</exception>
        /// <exception cref="ArgumentNullException">Exception for variable not found</exception>
        String GetStringRuntimeValue(RuntimeValueEnum runtimeValueEnum);

        /// <summary>
        ///     Set one or more cells to a list cell to new value
        /// </summary>
        /// <param name="listCells">A list of cells</param>
        /// <returns>A boolean value about the set outcome</returns>
        /// <exception cref="ArgumentNullException">Exception when there is not a variable of list in the table</exception>
        Boolean SetCellList(List<Cell> listCells);

        /// <summary>
        ///     Set a decimal variable in the configuration table to a new value
        /// </summary>
        /// <param name="configurationValueEnum">Configuration parameter to set</param>
        /// <param name="value">The new value</param>
        /// <exception cref="DataLayerExceptionEnum.DATATYPE_EXCEPTION">Exception for a wrong DataType</exception>
        void SetDecimalConfigurationValue(ConfigurationValueEnum configurationValueEnum, Decimal value);

        /// <summary>
        ///     Set a decimal variable in the runtime table to a new value
        /// </summary>
        /// <param name="runtimeValueEnum">Runtime parameter to set</param>
        /// <param name="value">The new value</param>
        /// <exception cref="DataLayerExceptionEnum.DATATYPE_EXCEPTION">Exception for a wrong DataType</exception>
        void SetDecimalRuntimeValue(RuntimeValueEnum runtimeValueEnum, Decimal value);

        /// <summary>
        ///     Set an integer variable in the configuration table to a new value
        /// </summary>
        /// <param name="configurationValueEnum">Configuration parameter to set</param>
        /// <param name="value">The new value</param>
        /// <exception cref="DataLayerExceptionEnum.DATATYPE_EXCEPTION">Exception for a wrong DataType</exception>
        void SetIntegerConfigurationValue(ConfigurationValueEnum configurationValueEnum, Int32 value);

        /// <summary>
        ///     Set an integer variable in the runtime table to a new value
        /// </summary>
        /// <param name="runtimeValueEnum">Runtime parameter to set</param>
        /// <param name="value">The new value</param>
        /// <exception cref="DataLayerExceptionEnum.DATATYPE_EXCEPTION">Exception for a wrong DataType</exception>
        void SetIntegerRuntimeValue(RuntimeValueEnum runtimeValueEnum, Int32 value);

        /// <summary>
        ///     Set a string variable in the configuration table to a new value
        /// </summary>
        /// <param name="configurationValueEnum">Configuration parameter to set</param>
        /// <param name="value">The new value</param>
        /// <exception cref="DataLayerExceptionEnum.DATATYPE_EXCEPTION">Exception for a wrong DataType</exception>
        void SetStringConfigurationValue(ConfigurationValueEnum configurationValueEnum, String value);

        /// <summary>
        ///     Set a string variable in the runtime table to a new value
        /// </summary>
        /// <param name="runtimeValueEnum">Runtime parameter to set</param>
        /// <param name="value">The new value</param>
        /// <exception cref="DataLayerExceptionEnum.DATATYPE_EXCEPTION">Exception for a wrong DataType</exception>
        void SetStringRuntimeValue(RuntimeValueEnum runtimeValueEnum, String value);

        #endregion
    }
}
