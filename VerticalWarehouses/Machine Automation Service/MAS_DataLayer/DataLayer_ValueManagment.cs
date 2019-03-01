using System;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using Ferretto.VW.Common_Utils;

namespace Ferretto.VW.MAS_DataLayer
{
    public partial class DataLayer : IDataLayer
    {
        #region Methods

        public decimal GetDecimalConfigurationValue(ConfigurationValueEnum configurationValueEnum)
        {
            // TEMP
            // Comments to keep until the method won't be tested
            // Search the var in the DB
            // Check if the type in the DB is decimal
            // If it is not decimal i throw an exception for "Invalid Data Type"
            // If they are the same i convert the return value
            decimal returnDecimalValue = 0;

            var configurationValue =
                this.inMemoryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum);

            if (configurationValue != null)
            {
                if (configurationValue.VarType == DataTypeEnum.decimalType)
                {
                    if (!decimal.TryParse(configurationValue.VarValue, out returnDecimalValue))
                        throw new InMemoryDataLayerException(DataLayerExceptionEnum.PARSE_EXCEPTION);
                }
                else
                    throw new InMemoryDataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
            }
            else
                throw new ArgumentNullException();

            return returnDecimalValue;
        }

        public decimal GetDecimalRuntimeValue(RuntimeValueEnum runtimeValueEnum)
        {
            // TEMP
            // Comments to keep until the method won't be tested
            // Search the var in the DB
            // Check if the type in the DB is decimal
            // If it is not decimal i throw an exception for "Invalid Data Type"
            // If they are the same i convert the return value
            decimal returnDecimalValue = 0;

            var runtimeValue =
                this.inMemoryDataContext.RuntimeValues.FirstOrDefault(s => s.VarName == runtimeValueEnum);

            if (runtimeValue != null)
            {
                if (runtimeValue.VarType == DataTypeEnum.decimalType)
                {
                    if (!decimal.TryParse(runtimeValue.VarValue, out returnDecimalValue))
                        throw new InMemoryDataLayerException(DataLayerExceptionEnum.PARSE_EXCEPTION);
                }
                else
                    throw new InMemoryDataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
            }
            else
                throw new ArgumentNullException();

            return returnDecimalValue;
        }

        public int GetIntegerConfigurationValue(ConfigurationValueEnum configurationValueEnum)
        {
            // TEMP
            // Comments to keep until the method won't be tested
            // Search the var in the DB
            // Check if the type in the DB is integer
            // If it is not integer i throw an exception for "Invalid Data Type"
            // If they are the same i convert the return value
            var returnIntegerValue = 0;

            var configurationValue =
                this.inMemoryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum);

            if (configurationValue != null)
            {
                if (configurationValue.VarType == DataTypeEnum.integerType)
                {
                    if (!int.TryParse(configurationValue.VarValue, out returnIntegerValue))
                        throw new InMemoryDataLayerException(DataLayerExceptionEnum.PARSE_EXCEPTION);
                }
                else
                    throw new InMemoryDataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
            }
            else
                throw new ArgumentNullException();

            return returnIntegerValue;
        }

        public int GetIntegerRuntimeValue(RuntimeValueEnum runtimeValueEnum)
        {
            // TEMP
            // Comments to keep until the method won't be tested
            // Search the var in the DB
            // Check if the type in the DB is integer
            // If it is not decimal i throw an exception for "Invalid Data Type"
            // If they are the same i convert the return value
            var returnIntegerValue = 0;

            var runtimeValue =
                this.inMemoryDataContext.RuntimeValues.FirstOrDefault(s => s.VarName == runtimeValueEnum);

            if (runtimeValue != null)
            {
                if (runtimeValue.VarType == DataTypeEnum.integerType)
                {
                    if (!int.TryParse(runtimeValue.VarValue, out returnIntegerValue))
                        throw new InMemoryDataLayerException(DataLayerExceptionEnum.PARSE_EXCEPTION);
                }
                else
                    throw new InMemoryDataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
            }
            else
                throw new ArgumentNullException();

            return returnIntegerValue;
        }

        /// <inheritdoc/>
        public IPAddress GetIPAddressConfigurationValue(ConfigurationValueEnum configurationValueEnum)
        {
            IPAddress returnIPAddressValue;

            var configurationValue =
                this.inMemoryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum);

            if (configurationValue != null)
            {
                if (configurationValue.VarType == DataTypeEnum.IPAddressType)
                    returnIPAddressValue = IPAddress.Parse(configurationValue.VarValue);
                else
                    throw new InMemoryDataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
            }
            else
                throw new ArgumentNullException();

            return returnIPAddressValue;
        }

        public string GetStringConfigurationValue(ConfigurationValueEnum configurationValueEnum)
        {
            // TEMP
            // Comments to keep until the method won't be tested
            // Search the var in the DB
            // Check if the type in the DB is string
            // If it is not string i throw an exception for "Invalid Data Type"
            // If they are the same i convert the return value
            var returnStringValue = "";

            var configurationValue =
                this.inMemoryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum);

            if (configurationValue != null)
            {
                if (configurationValue.VarType == DataTypeEnum.stringType)
                    returnStringValue = configurationValue.VarValue;
                else
                    throw new InMemoryDataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
            }
            else
                throw new ArgumentNullException();

            return returnStringValue;
        }

        public string GetStringRuntimeValue(RuntimeValueEnum runtimeValueEnum)
        {
            // TEMP
            // Comments to keep until the method won't be tested
            // Search the var in the DB
            // Check if the type in the DB is string
            // If it is not string i throw an exception for "Invalid Data Type"
            // If they are the same i convert the return value
            var returnStringValue = "";

            var runtimeValue =
                this.inMemoryDataContext.RuntimeValues.FirstOrDefault(s => s.VarName == runtimeValueEnum);

            if (runtimeValue != null)
            {
                if (runtimeValue.VarType == DataTypeEnum.stringType)
                    returnStringValue = runtimeValue.VarValue;
                else
                    throw new InMemoryDataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
            }
            else
                throw new ArgumentNullException();

            return returnStringValue;
        }

        public void SetDecimalConfigurationValue(ConfigurationValueEnum configurationValueEnum, decimal value)
        {
            // TEMP
            // Comments to keep until the method won't be tested
            // Get an input value
            // DB var search in DB
            // if the var exist check the type in the record
            // if the type is different from decimal, throw an invalid data type excpetion
            // if the var exist i update it
            // if the var doesn't exist i create it
            var configurationValue =
                this.inMemoryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum);

            if (configurationValue == null)
            {
                var newConfigurationValue = new ConfigurationValue();
                newConfigurationValue.VarName = configurationValueEnum;
                newConfigurationValue.VarType = DataTypeEnum.decimalType;
                newConfigurationValue.VarValue = value.ToString();

                this.inMemoryDataContext.ConfigurationValues.Add(newConfigurationValue);
                this.inMemoryDataContext.SaveChanges();
            }
            else
            {
                if (configurationValue.VarType == DataTypeEnum.decimalType)
                {
                    configurationValue.VarValue = value.ToString();
                    this.inMemoryDataContext.SaveChanges();
                }
                else
                    throw new InMemoryDataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
            }
        }

        public void SetDecimalRuntimeValue(RuntimeValueEnum runtimeValueEnum, decimal value)
        {
            // TEMP
            // Comments to keep until the method won't be tested
            // Get an input value
            // DB var search in DB
            // if the var exist check the type in the record
            // if the type is different from decimal, throw an invalid data type excpetion
            // if the var exist i update it
            // if the var doesn't exist i create it
            var runtimeValue =
                this.inMemoryDataContext.RuntimeValues.FirstOrDefault(s => s.VarName == runtimeValueEnum);

            if (runtimeValue == null)
            {
                var newRuntimeValue = new RuntimeValue();
                newRuntimeValue.VarName = runtimeValueEnum;
                newRuntimeValue.VarType = DataTypeEnum.decimalType;
                newRuntimeValue.VarValue = value.ToString();

                this.inMemoryDataContext.RuntimeValues.Add(newRuntimeValue);
                this.inMemoryDataContext.SaveChanges();
            }
            else
            {
                if (runtimeValue.VarType == DataTypeEnum.decimalType)
                {
                    runtimeValue.VarValue = value.ToString();
                    this.inMemoryDataContext.SaveChanges();
                }
                else
                    throw new InMemoryDataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
            }
        }

        public void SetIntegerConfigurationValue(ConfigurationValueEnum configurationValueEnum, int value)
        {
            // TEMP
            // Comments to keep until the method won't be tested
            // Get an input value
            // DB var search in DB
            // if the var exist check the type in the record
            // if the type is different from integer, throw an invalid data type excpetion
            // if the var exist i update it
            // if the var doesn't exist i create it
            var configurationValue =
                this.inMemoryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum);

            if (configurationValue == null)
            {
                var newConfigurationValue = new ConfigurationValue();
                newConfigurationValue.VarName = configurationValueEnum;
                newConfigurationValue.VarType = DataTypeEnum.integerType;
                newConfigurationValue.VarValue = value.ToString();

                this.inMemoryDataContext.ConfigurationValues.Add(newConfigurationValue);
                this.inMemoryDataContext.SaveChanges();
            }
            else
            {
                if (configurationValue.VarType == DataTypeEnum.integerType)
                {
                    configurationValue.VarValue = value.ToString();
                    this.inMemoryDataContext.SaveChanges();
                }
                else
                    throw new InMemoryDataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
            }
        }

        public void SetIntegerRuntimeValue(RuntimeValueEnum runtimeValueEnum, int value)
        {
            // TEMP
            // Comments to keep until the method won't be tested
            // Get an input value
            // DB var search in DB
            // if the var exist check the type in the record
            // if the type is different from int, throw an invalid data type excpetion
            // if the var exist i update it
            // if the var doesn't exist i create it
            var runtimeValue =
                this.inMemoryDataContext.RuntimeValues.FirstOrDefault(s => s.VarName == runtimeValueEnum);

            if (runtimeValue == null)
            {
                var newRuntimeValue = new RuntimeValue();
                newRuntimeValue.VarName = runtimeValueEnum;
                newRuntimeValue.VarType = DataTypeEnum.integerType;
                newRuntimeValue.VarValue = value.ToString();

                this.inMemoryDataContext.RuntimeValues.Add(newRuntimeValue);
                this.inMemoryDataContext.SaveChanges();
            }
            else
            {
                if (runtimeValue.VarType == DataTypeEnum.integerType)
                {
                    runtimeValue.VarValue = value.ToString();
                    this.inMemoryDataContext.SaveChanges();
                }
                else
                    throw new InMemoryDataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
            }
        }

        public void SetStringConfigurationValue(ConfigurationValueEnum configurationValueEnum, string value)
        {
            // TEMP
            // Comments to keep until the method won't be tested
            // Get an input value
            // DB var search in DB
            // if the var exist check the type in the record
            // if the type is different from string, throw an invalid data type excpetion
            // if the var exist i update it
            // if the var doesn't exist i create it
            var configurationValue =
                this.inMemoryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum);

            if (configurationValue == null)
            {
                var newConfigurationValue = new ConfigurationValue();
                newConfigurationValue.VarName = configurationValueEnum;
                newConfigurationValue.VarType = DataTypeEnum.stringType;
                newConfigurationValue.VarValue = value;

                this.inMemoryDataContext.ConfigurationValues.Add(newConfigurationValue);
                this.inMemoryDataContext.SaveChanges();
            }
            else
            {
                if (configurationValue.VarType == DataTypeEnum.stringType)
                {
                    configurationValue.VarValue = value;
                    this.inMemoryDataContext.SaveChanges();
                }
                else
                    throw new InMemoryDataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
            }
        }

        public void SetStringRuntimeValue(RuntimeValueEnum runtimeValueEnum, string value)
        {
            // TEMP
            // Comments to keep until the method won't be tested
            // Get an input value
            // DB var search in DB
            // if the var exist check the type in the record
            // if the type is different from string, throw an invalid data type excpetion
            // if the var exist i update it
            // if the var doesn't exist i create it
            var runtimeValue =
                this.inMemoryDataContext.RuntimeValues.FirstOrDefault(s => s.VarName == runtimeValueEnum);

            if (runtimeValue == null)
            {
                var newRuntimeValue = new RuntimeValue();
                newRuntimeValue.VarName = runtimeValueEnum;
                newRuntimeValue.VarType = DataTypeEnum.stringType;
                newRuntimeValue.VarValue = value;

                this.inMemoryDataContext.RuntimeValues.Add(newRuntimeValue);
                this.inMemoryDataContext.SaveChanges();
            }
            else
            {
                if (runtimeValue.VarType == DataTypeEnum.stringType)
                {
                    runtimeValue.VarValue = value;
                    this.inMemoryDataContext.SaveChanges();
                }
                else
                    throw new InMemoryDataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
            }
        }

        #endregion
    }
}
