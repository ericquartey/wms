using System;
using System.Linq;
using System.Net;
using Ferretto.VW.Common_Utils;

namespace Ferretto.VW.MAS_DataLayer
{
    public partial class DataLayer : IDataLayerValueManagment
    {
        #region Methods

        /// <inheritdoc/>
        public DataTypeEnum ConvertConfigurationValue(ConfigurationValueEnum configurationValueEnum)
        {
            DataTypeEnum returnValue;

            switch (configurationValueEnum)
            {
                // INFO General info variables
                case ConfigurationValueEnum.Address:
                case ConfigurationValueEnum.City:
                case ConfigurationValueEnum.Client_Code:
                case ConfigurationValueEnum.Client_Name:
                case ConfigurationValueEnum.Country:
                case ConfigurationValueEnum.Latitude:
                case ConfigurationValueEnum.Longitude:
                case ConfigurationValueEnum.Model:
                case ConfigurationValueEnum.Order:
                case ConfigurationValueEnum.Province:
                case ConfigurationValueEnum.Serial:
                    {
                        returnValue = DataTypeEnum.stringType;
                        break;
                    }

                // INFO General info variables
                case ConfigurationValueEnum.AlfaNum1:
                case ConfigurationValueEnum.AlfaNum2:
                case ConfigurationValueEnum.AlfaNum3:
                case ConfigurationValueEnum.Laser1:
                case ConfigurationValueEnum.Laser2:
                case ConfigurationValueEnum.Laser3:
                case ConfigurationValueEnum.WMS_ON:
                    {
                        returnValue = DataTypeEnum.booleanType;
                        break;
                    }

                case ConfigurationValueEnum.InverterPort:
                case ConfigurationValueEnum.IoPort:
                // INFO General info variables
                case ConfigurationValueEnum.Bays_Quantity:
                case ConfigurationValueEnum.GeneralInfoId:
                case ConfigurationValueEnum.Machine_Number_In_Area:
                case ConfigurationValueEnum.Type_Bay1:
                case ConfigurationValueEnum.Type_Bay2:
                case ConfigurationValueEnum.Type_Bay3:
                case ConfigurationValueEnum.Type_Shutter1:
                case ConfigurationValueEnum.Type_Shutter2:
                case ConfigurationValueEnum.Type_Shutter3:
                    {
                        returnValue = DataTypeEnum.integerType;
                        break;
                    }

                case ConfigurationValueEnum.cellSpacing:
                case ConfigurationValueEnum.resolution:
                // INFO General info variables
                case ConfigurationValueEnum.Height:
                case ConfigurationValueEnum.Height_Bay1:
                case ConfigurationValueEnum.Height_Bay2:
                case ConfigurationValueEnum.Height_Bay3:
                case ConfigurationValueEnum.Position_Bay1:
                case ConfigurationValueEnum.Position_Bay2:
                case ConfigurationValueEnum.Position_Bay3:
                    {
                        returnValue = DataTypeEnum.decimalType;
                        break;
                    }

                // INFO General info variables
                case ConfigurationValueEnum.Installation_Date:
                    {
                        returnValue = DataTypeEnum.dateTimeType;
                        break;
                    }

                case ConfigurationValueEnum.InverterAddress:
                case ConfigurationValueEnum.IoAddress:
                    {
                        returnValue = DataTypeEnum.IPAddressType;
                        break;
                    }

                // INFO Unknow variable type
                default:
                    {
                        returnValue = DataTypeEnum.UndefinedType;
                        break;
                    }
            }

            return returnValue;
        }

        /// <inheritdoc/>
        public bool GetBoolConfigurationValue(ConfigurationValueEnum configurationValueEnum)
        {
            var returnBoolValue = false;

            var configurationValue = this.inMemoryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum);

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
        public DateTime GetDateTimeConfigurationValue(ConfigurationValueEnum configurationValueEnum)
        {
            DateTime returnDateTimeValue;

            var configurationValue = this.inMemoryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum);

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
        public decimal GetDecimalConfigurationValue(ConfigurationValueEnum configurationValueEnum)
        {
            decimal returnDecimalValue = 0;

            var configurationValue = this.inMemoryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum);

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
        public int GetIntegerConfigurationValue(ConfigurationValueEnum configurationValueEnum)
        {
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

        /// <inheritdoc/>
        public string GetStringConfigurationValue(ConfigurationValueEnum configurationValueEnum)
        {
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
        public void SetBoolConfigurationValue(ConfigurationValueEnum configurationValueEnum, bool value)
        {
            var configurationValue = this.inMemoryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum);

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
        public void SetDateTimeConfigurationValue(ConfigurationValueEnum configurationValueEnum, DateTime value)
        {
            var configurationValue = this.inMemoryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum);

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
        public void SetDecimalConfigurationValue(ConfigurationValueEnum configurationValueEnum, decimal value)
        {
            var configurationValue = this.inMemoryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum);

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
        public void SetIntegerConfigurationValue(ConfigurationValueEnum configurationValueEnum, int value)
        {
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
        public void SetStringConfigurationValue(ConfigurationValueEnum configurationValueEnum, string value)
        {
            var configurationValue = this.inMemoryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum);

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
