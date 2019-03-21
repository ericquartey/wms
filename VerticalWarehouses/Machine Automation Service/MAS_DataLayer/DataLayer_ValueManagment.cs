using System;
using System.Linq;
using System.Net;
using Ferretto.VW.Common_Utils;
using Ferretto.VW.MAS_DataLayer.Enumerations;

namespace Ferretto.VW.MAS_DataLayer
{
    public partial class DataLayer : IDataLayerValueManagment
    {
        /// <inheritdoc/>

        #region Methods

        /// <inheritdoc/>
        public bool GetBoolConfigurationValue(long configurationValueEnum, long categoryValueEnum)
        {
            var returnBoolValue = false;

            var configurationValue = this.inMemoryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum && s.CategoryName == categoryValueEnum);

            if (configurationValue != null)
            {
                if (configurationValue.VarType == DataTypeEnum.booleanType)
                {
                    if (!bool.TryParse(configurationValue.VarValue, out returnBoolValue))
                        throw new InMemoryDataLayerException(DataLayerExceptionEnum.PARSE_EXCEPTION);
                }
                else
                    throw new InMemoryDataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
            }
            else
                throw new ArgumentNullException();

            return returnBoolValue;
        }

        /// <inheritdoc/>
        public bool GetBoolRuntimeValue(RuntimeValueEnum runtimeValueEnum)
        {
            var returnBoolValue = false;

            var runtimeValue = this.inMemoryDataContext.RuntimeValues.FirstOrDefault(s => s.VarName == runtimeValueEnum);

            if (runtimeValue != null)
            {
                if (runtimeValue.VarType == DataTypeEnum.booleanType)
                {
                    if (!bool.TryParse(runtimeValue.VarValue, out returnBoolValue))
                        throw new InMemoryDataLayerException(DataLayerExceptionEnum.PARSE_EXCEPTION);
                }
                else
                    throw new InMemoryDataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
            }
            else
                throw new ArgumentNullException();

            return returnBoolValue;
        }

        /// <inheritdoc/>
        public DateTime GetDateTimeConfigurationValue(long configurationValueEnum, long categoryValueEnum)
        {
            DateTime returnDateTimeValue;

            var configurationValue = this.inMemoryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum && s.CategoryName == categoryValueEnum);

            if (configurationValue != null)
            {
                if (configurationValue.VarType == DataTypeEnum.dateTimeType)
                {
                    if (!DateTime.TryParse(configurationValue.VarValue, out returnDateTimeValue))
                        throw new InMemoryDataLayerException(DataLayerExceptionEnum.PARSE_EXCEPTION);
                }
                else
                    throw new InMemoryDataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
            }
            else
                throw new ArgumentNullException();

            return returnDateTimeValue;
        }

        /// <inheritdoc/>
        public DateTime GetDateTimeRuntimeValue(RuntimeValueEnum runtimeValueEnum)
        {
            DateTime returnDateTimeValue;

            var runtimeValue = this.inMemoryDataContext.RuntimeValues.FirstOrDefault(s => s.VarName == runtimeValueEnum);

            if (runtimeValue != null)
            {
                if (runtimeValue.VarType == DataTypeEnum.dateTimeType)
                {
                    if (!DateTime.TryParse(runtimeValue.VarValue, out returnDateTimeValue))
                        throw new InMemoryDataLayerException(DataLayerExceptionEnum.PARSE_EXCEPTION);
                }
                else
                    throw new InMemoryDataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
            }
            else
                throw new ArgumentNullException();

            return returnDateTimeValue;
        }

        /// <inheritdoc/>
        public decimal GetDecimalConfigurationValue(long configurationValueEnum, long categoryValueEnum)
        {
            decimal returnDecimalValue = 0;

            var configurationValue = this.inMemoryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum && s.CategoryName == categoryValueEnum);

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

        /// <inheritdoc/>
        public decimal GetDecimalRuntimeValue(RuntimeValueEnum runtimeValueEnum)
        {
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

        /// <inheritdoc/>
        public int GetIntegerConfigurationValue(long configurationValueEnum, long categoryValueEnum)
        {
            var returnIntegerValue = 0;

            var configurationValue =
                this.inMemoryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum && s.CategoryName == categoryValueEnum);

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

        /// <inheritdoc/>
        public int GetIntegerRuntimeValue(RuntimeValueEnum runtimeValueEnum)
        {
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
        public IPAddress GetIPAddressConfigurationValue(long configurationValueEnum, long categoryValueEnum)
        {
            IPAddress returnIPAddressValue;

            var configurationValue =
                this.inMemoryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum && s.CategoryName == categoryValueEnum);

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

        /// <inheritdoc/>
        public string GetStringConfigurationValue(long configurationValueEnum, long categoryValueEnum)
        {
            var returnStringValue = "";

            var configurationValue =
                this.inMemoryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum && s.CategoryName == categoryValueEnum);

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

        /// <inheritdoc/>
        public string GetStringRuntimeValue(RuntimeValueEnum runtimeValueEnum)
        {
            var returnStringValue = "";

            var runtimeValue = this.inMemoryDataContext.RuntimeValues.FirstOrDefault(s => s.VarName == runtimeValueEnum);

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

        /// <inheritdoc/>
        public void SetBoolConfigurationValue(long configurationValueEnum, long categoryValueEnum, bool value)
        {
            var configurationValue = this.inMemoryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum && s.CategoryName == categoryValueEnum);

            if (configurationValue == null)
            {
                var newConfigurationValue = new ConfigurationValue();
                newConfigurationValue.VarName = configurationValueEnum;
                newConfigurationValue.VarType = DataTypeEnum.booleanType;
                newConfigurationValue.VarValue = value.ToString();

                this.inMemoryDataContext.ConfigurationValues.Add(newConfigurationValue);
                this.inMemoryDataContext.SaveChanges();
            }
            else
            {
                if (configurationValue.VarType == DataTypeEnum.booleanType)
                {
                    configurationValue.VarValue = value.ToString();
                    this.inMemoryDataContext.SaveChanges();
                }
                else
                {
                    throw new InMemoryDataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
                }
            }
        }

        /// <inheritdoc/>
        public void SetBoolRuntimeValue(RuntimeValueEnum runtimeValueEnum, bool value)
        {
            var runtimeValue = this.inMemoryDataContext.RuntimeValues.FirstOrDefault(s => s.VarName == runtimeValueEnum);

            if (runtimeValue == null)
            {
                var newRuntimeValue = new RuntimeValue();
                newRuntimeValue.VarName = runtimeValueEnum;
                newRuntimeValue.VarType = DataTypeEnum.booleanType;
                newRuntimeValue.VarValue = value.ToString();

                this.inMemoryDataContext.RuntimeValues.Add(newRuntimeValue);
                this.inMemoryDataContext.SaveChanges();
            }
            else
            {
                if (runtimeValue.VarType == DataTypeEnum.booleanType)
                {
                    runtimeValue.VarValue = value.ToString();
                    this.inMemoryDataContext.SaveChanges();
                }
                else
                {
                    throw new InMemoryDataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
                }
            }
        }

        /// <inheritdoc/>
        public void SetDateTimeConfigurationValue(long configurationValueEnum, long categoryValueEnum, DateTime value)
        {
            var configurationValue = this.inMemoryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum && s.CategoryName == categoryValueEnum);

            if (configurationValue == null)
            {
                var newConfigurationValue = new ConfigurationValue();
                newConfigurationValue.VarName = configurationValueEnum;
                newConfigurationValue.VarType = DataTypeEnum.dateTimeType;
                newConfigurationValue.VarValue = value.ToString();

                this.inMemoryDataContext.ConfigurationValues.Add(newConfigurationValue);
                this.inMemoryDataContext.SaveChanges();
            }
            else
            {
                if (configurationValue.VarType == DataTypeEnum.dateTimeType)
                {
                    configurationValue.VarValue = value.ToString();
                    this.inMemoryDataContext.SaveChanges();
                }
                else
                {
                    throw new InMemoryDataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
                }
            }
        }

        /// <inheritdoc/>
        public void SetDateTimeRuntimeValue(RuntimeValueEnum runtimeValueEnum, DateTime value)
        {
            var runtimeValue = this.inMemoryDataContext.RuntimeValues.FirstOrDefault(s => s.VarName == runtimeValueEnum);

            if (runtimeValue == null)
            {
                var newRuntimeValue = new RuntimeValue();
                newRuntimeValue.VarName = runtimeValueEnum;
                newRuntimeValue.VarType = DataTypeEnum.dateTimeType;
                newRuntimeValue.VarValue = value.ToString();

                this.inMemoryDataContext.RuntimeValues.Add(newRuntimeValue);
                this.inMemoryDataContext.SaveChanges();
            }
            else
            {
                if (runtimeValue.VarType == DataTypeEnum.dateTimeType)
                {
                    runtimeValue.VarValue = value.ToString();
                    this.inMemoryDataContext.SaveChanges();
                }
                else
                {
                    throw new InMemoryDataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
                }
            }
        }

        /// <inheritdoc/>
        public void SetDecimalConfigurationValue(long configurationValueEnum, long categoryValueEnum, decimal value)
        {
            var configurationValue = this.inMemoryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum && s.CategoryName == categoryValueEnum);

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
                {
                    throw new InMemoryDataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
                }
            }
        }

        /// <inheritdoc/>
        public void SetDecimalRuntimeValue(RuntimeValueEnum runtimeValueEnum, decimal value)
        {
            var runtimeValue = this.inMemoryDataContext.RuntimeValues.FirstOrDefault(s => s.VarName == runtimeValueEnum);

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
                {
                    throw new InMemoryDataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
                }
            }
        }

        /// <inheritdoc/>
        public void SetIntegerConfigurationValue(long configurationValueEnum, long categoryValueEnum, int value)
        {
            var configurationValue =
                this.inMemoryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum && s.CategoryName == categoryValueEnum);

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
                {
                    throw new InMemoryDataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
                }
            }
        }

        /// <inheritdoc/>
        public void SetIntegerRuntimeValue(RuntimeValueEnum runtimeValueEnum, int value)
        {
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
                {
                    throw new InMemoryDataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
                }
            }
        }

        /// <inheritdoc/>
        public void SetStringConfigurationValue(long configurationValueEnum, long categoryValueEnum, string value)
        {
            var configurationValue = this.inMemoryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum && s.CategoryName == categoryValueEnum);

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
                {
                    throw new InMemoryDataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
                }
            }
        }

        /// <inheritdoc/>
        public void SetStringRuntimeValue(RuntimeValueEnum runtimeValueEnum, string value)
        {
            var runtimeValue = this.inMemoryDataContext.RuntimeValues.FirstOrDefault(s => s.VarName == runtimeValueEnum);

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
                {
                    throw new InMemoryDataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
                }
            }
        }

        #endregion
    }
}
