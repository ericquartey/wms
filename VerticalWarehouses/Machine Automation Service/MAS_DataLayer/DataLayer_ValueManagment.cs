using System;
using System.Linq;
using Ferretto.VW.Common_Utils;

namespace Ferretto.VW.MAS_DataLayer
{
    public partial class DataLayer : IDataLayer
    {
        #region Methods

        public decimal GetDecimalConfigurationValue(ConfigurationValueEnum configurationValueEnum)
        {
            // Comments to keep until the method won't be tested
            // Search the var in the DB
            // Check if the type in the DB is decimal
            // If it is not decimal i throw an exception for "Invalid Data Type"
            // If they are the same i convert the return value
            decimal returnDecimalValue = 0;

            var configurationValue = inMemoryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum);

            if (configurationValue != null)
            {
                if (configurationValue.VarType == DataTypeEnum.decimalType)
                {
                    if (!Decimal.TryParse(configurationValue.VarValue, out returnDecimalValue))
                    {
                        throw new InMemoryDataLayerException(DataLayerExceptionEnum.PARSE_EXCEPTION);
                    }
                }
                else
                {
                    throw new InMemoryDataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
                }
            }
            else
            {
                throw new ArgumentNullException();
            }

            return returnDecimalValue;
        }

        public decimal GetDecimalRuntimeValue(RuntimeValueEnum runtimeValueEnum)
        {
            // Comments to keep until the method won't be tested
            // Search the var in the DB
            // Check if the type in the DB is decimal
            // If it is not decimal i throw an exception for "Invalid Data Type"
            // If they are the same i convert the return value
            decimal returnDecimalValue = 0;

            var runtimeValue = inMemoryDataContext.RuntimeValues.FirstOrDefault(s => s.VarName == runtimeValueEnum);

            if (runtimeValue != null)
            {
                if (runtimeValue.VarType == DataTypeEnum.decimalType)
                {
                    if (!Decimal.TryParse(runtimeValue.VarValue, out returnDecimalValue))
                    {
                        throw new InMemoryDataLayerException(DataLayerExceptionEnum.PARSE_EXCEPTION);
                    }
                }
                else
                {
                    throw new InMemoryDataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
                }
            }
            else
            {
                throw new ArgumentNullException();
            }

            return returnDecimalValue;
        }

        public int GetIntegerConfigurationValue(ConfigurationValueEnum configurationValueEnum)
        {
            // Comments to keep until the method won't be tested
            // Search the var in the DB
            // Check if the type in the DB is integer
            // If it is not integer i throw an exception for "Invalid Data Type"
            // If they are the same i convert the return value
            int returnIntegerValue = 0;

            var configurationValue = inMemoryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum);

            if (configurationValue != null)
            {
                if (configurationValue.VarType == DataTypeEnum.integerType)
                {
                    if (!Int32.TryParse(configurationValue.VarValue, out returnIntegerValue))
                    {
                        throw new InMemoryDataLayerException(DataLayerExceptionEnum.PARSE_EXCEPTION);
                    }
                }
                else
                {
                    throw new InMemoryDataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
                }
            }
            else
            {
                throw new ArgumentNullException();
            }

            return returnIntegerValue;
        }

        public int GetIntegerRuntimeValue(RuntimeValueEnum runtimeValueEnum)
        {
            // Comments to keep until the method won't be tested
            // Search the var in the DB
            // Check if the type in the DB is integer
            // If it is not decimal i throw an exception for "Invalid Data Type"
            // If they are the same i convert the return value
            int returnIntegerValue = 0;

            var runtimeValue = inMemoryDataContext.RuntimeValues.FirstOrDefault(s => s.VarName == runtimeValueEnum);

            if (runtimeValue != null)
            {
                if (runtimeValue.VarType == DataTypeEnum.integerType)
                {
                    if (!Int32.TryParse(runtimeValue.VarValue, out returnIntegerValue))
                    {
                        throw new InMemoryDataLayerException(DataLayerExceptionEnum.PARSE_EXCEPTION);
                    }
                }
                else
                {
                    throw new InMemoryDataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
                }
            }
            else
            {
                throw new ArgumentNullException();
            }

            return returnIntegerValue;
        }

        public string GetStringConfigurationValue(ConfigurationValueEnum configurationValueEnum)
        {
            // Comments to keep until the method won't be tested
            // Search the var in the DB
            // Check if the type in the DB is string
            // If it is not string i throw an exception for "Invalid Data Type"
            // If they are the same i convert the return value
            string returnStringValue = "";

            var configurationValue = inMemoryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum);

            if (configurationValue != null)
            {
                if (configurationValue.VarType == DataTypeEnum.stringType)
                {
                    returnStringValue = configurationValue.VarValue;
                }
                else
                {
                    throw new InMemoryDataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
                }
            }
            else
            {
                throw new ArgumentNullException();
            }

            return returnStringValue;
        }

        public string GetStringRuntimeValue(RuntimeValueEnum runtimeValueEnum)
        {
            // Comments to keep until the method won't be tested
            // Search the var in the DB
            // Check if the type in the DB is string
            // If it is not string i throw an exception for "Invalid Data Type"
            // If they are the same i convert the return value
            string returnStringValue = "";

            var runtimeValue = inMemoryDataContext.RuntimeValues.FirstOrDefault(s => s.VarName == runtimeValueEnum);

            if (runtimeValue != null)
            {
                if (runtimeValue.VarType == DataTypeEnum.stringType)
                {
                    returnStringValue = runtimeValue.VarValue;
                }
                else
                {
                    throw new InMemoryDataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
                }
            }
            else
            {
                throw new ArgumentNullException();
            }

            return returnStringValue;
        }

        public void SetDecimalConfigurationValue(ConfigurationValueEnum configurationValueEnum, decimal value)
        {
            // Comments to keep until the method won't be tested
            // Get an input value
            // DB var search in DB
            // if the var exist check the type in the record
            // if the type is different from decimal, throw an invalid data type excpetion
            // if the var exist i update it
            // if the var doesn't exist i create it
            var configurationValue = inMemoryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum);

            if (configurationValue == null)
            {
                ConfigurationValue newConfigurationValue = new ConfigurationValue();
                newConfigurationValue.VarName = configurationValueEnum;
                newConfigurationValue.VarType = DataTypeEnum.decimalType;
                newConfigurationValue.VarValue = value.ToString();

                inMemoryDataContext.ConfigurationValues.Add(newConfigurationValue);
                inMemoryDataContext.SaveChanges();
            }
            else
            {
                if (configurationValue.VarType == DataTypeEnum.decimalType)
                {
                    configurationValue.VarValue = value.ToString();
                    inMemoryDataContext.SaveChanges();
                }
                else
                {
                    throw new InMemoryDataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
                }
            }
        }

        public void SetDecimalRuntimeValue(RuntimeValueEnum runtimeValueEnum, decimal value)
        {
            // Comments to keep until the method won't be tested
            // Get an input value
            // DB var search in DB
            // if the var exist check the type in the record
            // if the type is different from decimal, throw an invalid data type excpetion
            // if the var exist i update it
            // if the var doesn't exist i create it
            var runtimeValue = inMemoryDataContext.RuntimeValues.FirstOrDefault(s => s.VarName == runtimeValueEnum);

            if (runtimeValue == null)
            {
                RuntimeValue newRuntimeValue = new RuntimeValue();
                newRuntimeValue.VarName = runtimeValueEnum;
                newRuntimeValue.VarType = DataTypeEnum.decimalType;
                newRuntimeValue.VarValue = value.ToString();

                inMemoryDataContext.RuntimeValues.Add(newRuntimeValue);
                inMemoryDataContext.SaveChanges();
            }
            else
            {
                if (runtimeValue.VarType == DataTypeEnum.decimalType)
                {
                    runtimeValue.VarValue = value.ToString();
                    inMemoryDataContext.SaveChanges();
                }
                else
                {
                    throw new InMemoryDataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
                }
            }
        }

        public void SetIntegerConfigurationValue(ConfigurationValueEnum configurationValueEnum, int value)
        {
            // Comments to keep until the method won't be tested
            // Get an input value
            // DB var search in DB
            // if the var exist check the type in the record
            // if the type is different from integer, throw an invalid data type excpetion
            // if the var exist i update it
            // if the var doesn't exist i create it
            var configurationValue = inMemoryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum);

            if (configurationValue == null)
            {
                ConfigurationValue newConfigurationValue = new ConfigurationValue();
                newConfigurationValue.VarName = configurationValueEnum;
                newConfigurationValue.VarType = DataTypeEnum.integerType;
                newConfigurationValue.VarValue = value.ToString();

                inMemoryDataContext.ConfigurationValues.Add(newConfigurationValue);
                inMemoryDataContext.SaveChanges();
            }
            else
            {
                if (configurationValue.VarType == DataTypeEnum.integerType)
                {
                    configurationValue.VarValue = value.ToString();
                    inMemoryDataContext.SaveChanges();
                }
                else
                {
                    throw new InMemoryDataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
                }
            }
        }

        public void SetIntegerRuntimeValue(RuntimeValueEnum runtimeValueEnum, int value)
        {
            // Comments to keep until the method won't be tested
            // Get an input value
            // DB var search in DB
            // if the var exist check the type in the record
            // if the type is different from int, throw an invalid data type excpetion
            // if the var exist i update it
            // if the var doesn't exist i create it
            var runtimeValue = inMemoryDataContext.RuntimeValues.FirstOrDefault(s => s.VarName == runtimeValueEnum);

            if (runtimeValue == null)
            {
                RuntimeValue newRuntimeValue = new RuntimeValue();
                newRuntimeValue.VarName = runtimeValueEnum;
                newRuntimeValue.VarType = DataTypeEnum.integerType;
                newRuntimeValue.VarValue = value.ToString();

                inMemoryDataContext.RuntimeValues.Add(newRuntimeValue);
                inMemoryDataContext.SaveChanges();
            }
            else
            {
                if (runtimeValue.VarType == DataTypeEnum.integerType)
                {
                    runtimeValue.VarValue = value.ToString();
                    inMemoryDataContext.SaveChanges();
                }
                else
                {
                    throw new InMemoryDataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
                }
            }
        }

        public void SetStringConfigurationValue(ConfigurationValueEnum configurationValueEnum, string value)
        {
            // Comments to keep until the method won't be tested
            // Get an input value
            // DB var search in DB
            // if the var exist check the type in the record
            // if the type is different from string, throw an invalid data type excpetion
            // if the var exist i update it
            // if the var doesn't exist i create it
            var configurationValue = inMemoryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum);

            if (configurationValue == null)
            {
                ConfigurationValue newConfigurationValue = new ConfigurationValue();
                newConfigurationValue.VarName = configurationValueEnum;
                newConfigurationValue.VarType = DataTypeEnum.stringType;
                newConfigurationValue.VarValue = value;

                inMemoryDataContext.ConfigurationValues.Add(newConfigurationValue);
                inMemoryDataContext.SaveChanges();
            }
            else
            {
                if (configurationValue.VarType == DataTypeEnum.stringType)
                {
                    configurationValue.VarValue = value;
                    inMemoryDataContext.SaveChanges();
                }
                else
                {
                    throw new InMemoryDataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
                }
            }
        }

        public void SetStringRuntimeValue(RuntimeValueEnum runtimeValueEnum, string value)
        {
            // Comments to keep until the method won't be tested
            // Get an input value
            // DB var search in DB
            // if the var exist check the type in the record
            // if the type is different from string, throw an invalid data type excpetion
            // if the var exist i update it
            // if the var doesn't exist i create it
            var runtimeValue = inMemoryDataContext.RuntimeValues.FirstOrDefault(s => s.VarName == runtimeValueEnum);

            if (runtimeValue == null)
            {
                RuntimeValue newRuntimeValue = new RuntimeValue();
                newRuntimeValue.VarName = runtimeValueEnum;
                newRuntimeValue.VarType = DataTypeEnum.stringType;
                newRuntimeValue.VarValue = value;

                inMemoryDataContext.RuntimeValues.Add(newRuntimeValue);
                inMemoryDataContext.SaveChanges();
            }
            else
            {
                if (runtimeValue.VarType == DataTypeEnum.stringType)
                {
                    runtimeValue.VarValue = value;
                    inMemoryDataContext.SaveChanges();
                }
                else
                {
                    throw new InMemoryDataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
                }
            }
        }

        #endregion
    }
}
