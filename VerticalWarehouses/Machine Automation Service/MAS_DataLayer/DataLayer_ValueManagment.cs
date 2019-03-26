using System;
using System.Linq;
using System.Net;
using Ferretto.VW.Common_Utils;
using Ferretto.VW.MAS_DataLayer.Enumerations;
using Ferretto.VW.MAS_DataLayer.Interfaces;

namespace Ferretto.VW.MAS_DataLayer
{
    public partial class DataLayer : IDataLayerValueManagment
    {
        #region Methods

        /// <inheritdoc/>
        public bool GetBoolConfigurationValue(long configurationValueEnum, long categoryValueEnum)
        {
            var returnBoolValue = false;

            if (!this.CheckConfigurationDataType(configurationValueEnum, categoryValueEnum, ConfigurationDataType.Boolean))
                throw new DataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);

            var configurationValue = this.primaryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum && s.CategoryName == categoryValueEnum);

            if (configurationValue != null)
            {
                if (configurationValue.VarType == ConfigurationDataType.Boolean)
                {
                    if (!bool.TryParse(configurationValue.VarValue, out returnBoolValue))
                        throw new DataLayerException(DataLayerExceptionEnum.PARSE_EXCEPTION);
                }
                else
                    throw new DataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
            }
            else
                throw new ArgumentNullException();

            return returnBoolValue;
        }

        /// <inheritdoc/>
        public bool GetBoolRuntimeValue(long runtimeValueEnum, long categoryValueEnum)
        {
            var returnBoolValue = false;

            if (!this.CheckConfigurationDataType(runtimeValueEnum, categoryValueEnum, ConfigurationDataType.Boolean))
                throw new DataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);

            var runtimeValue = this.primaryDataContext.RuntimeValues.FirstOrDefault(s => s.VarName == runtimeValueEnum && s.CategoryName == categoryValueEnum);

            if (runtimeValue != null)
            {
                if (runtimeValue.VarType == ConfigurationDataType.Boolean)
                {
                    if (!bool.TryParse(runtimeValue.VarValue, out returnBoolValue))
                        throw new DataLayerException(DataLayerExceptionEnum.PARSE_EXCEPTION);
                }
                else
                    throw new DataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
            }
            else
                throw new ArgumentNullException();

            return returnBoolValue;
        }

        /// <inheritdoc/>
        public DateTime GetDateTimeConfigurationValue(long configurationValueEnum, long categoryValueEnum)
        {
            DateTime returnDateTimeValue;

            if (!this.CheckConfigurationDataType(configurationValueEnum, categoryValueEnum, ConfigurationDataType.Date))
                throw new DataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);

            var configurationValue = this.primaryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum && s.CategoryName == categoryValueEnum);

            if (configurationValue != null)
            {
                if (configurationValue.VarType == ConfigurationDataType.Date)
                {
                    if (!DateTime.TryParse(configurationValue.VarValue, out returnDateTimeValue))
                        throw new DataLayerException(DataLayerExceptionEnum.PARSE_EXCEPTION);
                }
                else
                    throw new DataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
            }
            else
                throw new ArgumentNullException();

            return returnDateTimeValue;
        }

        /// <inheritdoc/>
        public DateTime GetDateTimeRuntimeValue(long runtimeValueEnum, long categoryValueEnum)
        {
            DateTime returnDateTimeValue;

            if (!this.CheckConfigurationDataType(runtimeValueEnum, categoryValueEnum, ConfigurationDataType.Date))
                throw new DataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);

            var runtimeValue = this.primaryDataContext.RuntimeValues.FirstOrDefault(s => s.VarName == runtimeValueEnum && s.CategoryName == categoryValueEnum);

            if (runtimeValue != null)
            {
                if (runtimeValue.VarType == ConfigurationDataType.Date)
                {
                    if (!DateTime.TryParse(runtimeValue.VarValue, out returnDateTimeValue))
                        throw new DataLayerException(DataLayerExceptionEnum.PARSE_EXCEPTION);
                }
                else
                    throw new DataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
            }
            else
                throw new ArgumentNullException();

            return returnDateTimeValue;
        }

        /// <inheritdoc/>
        public decimal GetDecimalConfigurationValue(long configurationValueEnum, long categoryValueEnum)
        {
            decimal returnDecimalValue = 0;

            if (!this.CheckConfigurationDataType(configurationValueEnum, categoryValueEnum, ConfigurationDataType.Float))
                throw new DataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);

            var configurationValue = this.primaryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum && s.CategoryName == categoryValueEnum);

            if (configurationValue != null)
            {
                if (configurationValue.VarType == ConfigurationDataType.Float)
                {
                    if (!decimal.TryParse(configurationValue.VarValue, out returnDecimalValue))
                        throw new DataLayerException(DataLayerExceptionEnum.PARSE_EXCEPTION);
                }
                else
                    throw new DataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
            }
            else
                throw new ArgumentNullException();

            return returnDecimalValue;
        }

        /// <inheritdoc/>
        public decimal GetDecimalRuntimeValue(long runtimeValueEnum, long categoryValueEnum)
        {
            decimal returnDecimalValue = 0;

            if (!this.CheckConfigurationDataType(runtimeValueEnum, categoryValueEnum, ConfigurationDataType.Float))
                throw new DataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);

            var runtimeValue =
                this.primaryDataContext.RuntimeValues.FirstOrDefault(s => s.VarName == runtimeValueEnum && s.CategoryName == categoryValueEnum);

            if (runtimeValue != null)
            {
                if (runtimeValue.VarType == ConfigurationDataType.Float)
                {
                    if (!decimal.TryParse(runtimeValue.VarValue, out returnDecimalValue))
                        throw new DataLayerException(DataLayerExceptionEnum.PARSE_EXCEPTION);
                }
                else
                    throw new DataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
            }
            else
                throw new ArgumentNullException();

            return returnDecimalValue;
        }

        /// <inheritdoc/>
        public int GetIntegerConfigurationValue(long configurationValueEnum, long categoryValueEnum)
        {
            var returnIntegerValue = 0;

            if (!this.CheckConfigurationDataType(configurationValueEnum, categoryValueEnum, ConfigurationDataType.Integer))
                throw new DataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);

            var configurationValue =
                this.primaryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum && s.CategoryName == categoryValueEnum);

            if (configurationValue != null)
            {
                if (configurationValue.VarType == ConfigurationDataType.Integer)
                {
                    if (!int.TryParse(configurationValue.VarValue, out returnIntegerValue))
                        throw new DataLayerException(DataLayerExceptionEnum.PARSE_EXCEPTION);
                }
                else
                    throw new DataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
            }
            else
                throw new ArgumentNullException();

            return returnIntegerValue;
        }

        /// <inheritdoc/>
        public int GetIntegerRuntimeValue(long runtimeValueEnum, long categoryValueEnum)
        {
            var returnIntegerValue = 0;

            if (!this.CheckConfigurationDataType(runtimeValueEnum, categoryValueEnum, ConfigurationDataType.Integer))
                throw new DataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);

            var runtimeValue =
                this.primaryDataContext.RuntimeValues.FirstOrDefault(s => s.VarName == runtimeValueEnum && s.CategoryName == categoryValueEnum);

            if (runtimeValue != null)
            {
                if (runtimeValue.VarType == ConfigurationDataType.Integer)
                {
                    if (!int.TryParse(runtimeValue.VarValue, out returnIntegerValue))
                        throw new DataLayerException(DataLayerExceptionEnum.PARSE_EXCEPTION);
                }
                else
                    throw new DataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
            }
            else
                throw new ArgumentNullException();

            return returnIntegerValue;
        }

        /// <inheritdoc/>
        public IPAddress GetIPAddressConfigurationValue(long configurationValueEnum, long categoryValueEnum)
        {
            IPAddress returnIPAddressValue;

            if (!this.CheckConfigurationDataType(configurationValueEnum, categoryValueEnum, ConfigurationDataType.IPAddress))
                throw new DataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);

            var configurationValue =
                this.primaryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum && s.CategoryName == categoryValueEnum);

            if (configurationValue != null)
            {
                if (configurationValue.VarType == ConfigurationDataType.IPAddress)
                    returnIPAddressValue = IPAddress.Parse(configurationValue.VarValue);
                else
                    throw new DataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
            }
            else
                throw new ArgumentNullException();

            return returnIPAddressValue;
        }

        /// <inheritdoc/>
        public string GetStringConfigurationValue(long configurationValueEnum, long categoryValueEnum)
        {
            var returnStringValue = "";

            if (!this.CheckConfigurationDataType(configurationValueEnum, categoryValueEnum, ConfigurationDataType.String))
                throw new DataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);

            var configurationValue = this.primaryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum && s.CategoryName == categoryValueEnum);

            if (configurationValue != null)
            {
                if (configurationValue.VarType == ConfigurationDataType.String)
                    returnStringValue = configurationValue.VarValue;
                else
                    throw new DataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
            }
            else
                throw new ArgumentNullException();

            return returnStringValue;
        }

        /// <inheritdoc/>
        public string GetStringRuntimeValue(long runtimeValueEnum, long categoryValueEnum)
        {
            var returnStringValue = "";

            if (!this.CheckConfigurationDataType(runtimeValueEnum, categoryValueEnum, ConfigurationDataType.String))
                throw new DataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);

            var runtimeValue = this.primaryDataContext.RuntimeValues.FirstOrDefault(s => s.VarName == runtimeValueEnum && s.CategoryName == categoryValueEnum);

            if (runtimeValue != null)
            {
                if (runtimeValue.VarType == ConfigurationDataType.String)
                {
                    returnStringValue = runtimeValue.VarValue;
                }
                else
                {
                    throw new DataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
                }
            }
            else
            {
                throw new ArgumentNullException();
            }

            return returnStringValue;
        }

        /// <inheritdoc/>
        public async void SetBoolConfigurationValue(long configurationValueEnum, long categoryValueEnum, bool value)
        {
            if (!this.CheckConfigurationDataType(configurationValueEnum, categoryValueEnum, ConfigurationDataType.Boolean))
                throw new DataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);

            var configurationValue = this.primaryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum && s.CategoryName == categoryValueEnum);

            if (configurationValue == null)
            {
                var newConfigurationValue = new ConfigurationValue();
                newConfigurationValue.CategoryName = categoryValueEnum;
                newConfigurationValue.VarName = configurationValueEnum;
                newConfigurationValue.VarType = ConfigurationDataType.Boolean;
                newConfigurationValue.VarValue = value.ToString();

                this.primaryDataContext.ConfigurationValues.Add(newConfigurationValue);
                await this.primaryDataContext.SaveChangesAsync();
            }
            else
            {
                if (configurationValue.VarType == ConfigurationDataType.Boolean)
                {
                    configurationValue.VarValue = value.ToString();
                    await this.primaryDataContext.SaveChangesAsync();
                }
                else
                {
                    throw new DataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
                }
            }
        }

        /// <inheritdoc/>
        public async void SetBoolRuntimeValue(long runtimeValueEnum, long categoryValueEnum, bool value)
        {
            if (!this.CheckConfigurationDataType(runtimeValueEnum, categoryValueEnum, ConfigurationDataType.Boolean))
                throw new DataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);

            var runtimeValue = this.primaryDataContext.RuntimeValues.FirstOrDefault(s => s.VarName == runtimeValueEnum && s.CategoryName == categoryValueEnum);

            if (runtimeValue == null)
            {
                var newRuntimeValue = new RuntimeValue();
                newRuntimeValue.CategoryName = categoryValueEnum;
                newRuntimeValue.VarName = runtimeValueEnum;
                newRuntimeValue.VarType = ConfigurationDataType.Boolean;
                newRuntimeValue.VarValue = value.ToString();

                this.primaryDataContext.RuntimeValues.Add(newRuntimeValue);
                await this.primaryDataContext.SaveChangesAsync();
            }
            else
            {
                if (runtimeValue.VarType == ConfigurationDataType.Boolean)
                {
                    runtimeValue.VarValue = value.ToString();
                    await this.primaryDataContext.SaveChangesAsync();
                }
                else
                {
                    throw new DataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
                }
            }
        }

        /// <inheritdoc/>
        public async void SetDateTimeConfigurationValue(long configurationValueEnum, long categoryValueEnum, DateTime value)
        {
            if (!this.CheckConfigurationDataType(configurationValueEnum, categoryValueEnum, ConfigurationDataType.Date))
                throw new DataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);

            var configurationValue = this.primaryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum && s.CategoryName == categoryValueEnum);

            if (configurationValue == null)
            {
                var newConfigurationValue = new ConfigurationValue();
                newConfigurationValue.CategoryName = categoryValueEnum;
                newConfigurationValue.VarName = configurationValueEnum;
                newConfigurationValue.VarType = ConfigurationDataType.Date;
                newConfigurationValue.VarValue = value.ToString();

                this.primaryDataContext.ConfigurationValues.Add(newConfigurationValue);
                await this.primaryDataContext.SaveChangesAsync();
            }
            else
            {
                if (configurationValue.VarType == ConfigurationDataType.Date)
                {
                    configurationValue.VarValue = value.ToString();
                    await this.primaryDataContext.SaveChangesAsync();
                }
                else
                {
                    throw new DataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
                }
            }
        }

        /// <inheritdoc/>
        public async void SetDateTimeRuntimeValue(long runtimeValueEnum, long categoryValueEnum, DateTime value)
        {
            if (!this.CheckConfigurationDataType(runtimeValueEnum, categoryValueEnum, ConfigurationDataType.Date))
                throw new DataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);

            var runtimeValue = this.primaryDataContext.RuntimeValues.FirstOrDefault(s => s.VarName == runtimeValueEnum && s.CategoryName == categoryValueEnum);

            if (runtimeValue == null)
            {
                var newRuntimeValue = new RuntimeValue();
                newRuntimeValue.CategoryName = categoryValueEnum;
                newRuntimeValue.VarName = runtimeValueEnum;
                newRuntimeValue.VarType = ConfigurationDataType.Date;
                newRuntimeValue.VarValue = value.ToString();

                this.primaryDataContext.RuntimeValues.Add(newRuntimeValue);
                await this.primaryDataContext.SaveChangesAsync();
            }
            else
            {
                if (runtimeValue.VarType == ConfigurationDataType.Date)
                {
                    runtimeValue.VarValue = value.ToString();
                    await this.primaryDataContext.SaveChangesAsync();
                }
                else
                {
                    throw new DataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
                }
            }
        }

        /// <inheritdoc/>
        public async void SetDecimalConfigurationValue(long configurationValueEnum, long categoryValueEnum, decimal value)
        {
            if (!this.CheckConfigurationDataType(configurationValueEnum, categoryValueEnum, ConfigurationDataType.Float))
                throw new DataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);

            var configurationValue = this.primaryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum && s.CategoryName == categoryValueEnum);

            if (configurationValue == null)
            {
                var newConfigurationValue = new ConfigurationValue();
                newConfigurationValue.CategoryName = categoryValueEnum;
                newConfigurationValue.VarName = configurationValueEnum;
                newConfigurationValue.VarType = ConfigurationDataType.Float;
                newConfigurationValue.VarValue = value.ToString();

                this.primaryDataContext.ConfigurationValues.Add(newConfigurationValue);
                await this.primaryDataContext.SaveChangesAsync();
            }
            else
            {
                if (configurationValue.VarType == ConfigurationDataType.Float)
                {
                    configurationValue.VarValue = value.ToString();
                    await this.primaryDataContext.SaveChangesAsync();
                }
                else
                {
                    throw new DataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
                }
            }
        }

        /// <inheritdoc/>
        public async void SetDecimalRuntimeValue(long runtimeValueEnum, long categoryValueEnum, decimal value)
        {
            if (!this.CheckConfigurationDataType(runtimeValueEnum, categoryValueEnum, ConfigurationDataType.Float))
                throw new DataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);

            var runtimeValue = this.primaryDataContext.RuntimeValues.FirstOrDefault(s => s.VarName == runtimeValueEnum);

            if (runtimeValue == null)
            {
                var newRuntimeValue = new RuntimeValue();
                newRuntimeValue.VarName = runtimeValueEnum;
                newRuntimeValue.VarType = ConfigurationDataType.Float;
                newRuntimeValue.VarValue = value.ToString();

                this.primaryDataContext.RuntimeValues.Add(newRuntimeValue);
                await this.primaryDataContext.SaveChangesAsync();
            }
            else
            {
                if (runtimeValue.VarType == ConfigurationDataType.Float)
                {
                    runtimeValue.VarValue = value.ToString();
                    await this.primaryDataContext.SaveChangesAsync();
                }
                else
                {
                    throw new DataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
                }
            }
        }

        /// <inheritdoc/>
        public async void SetIntegerConfigurationValue(long configurationValueEnum, long categoryValueEnum, int value)
        {
            if (!this.CheckConfigurationDataType(configurationValueEnum, categoryValueEnum, ConfigurationDataType.Integer))
                throw new DataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);

            var configurationValue = this.primaryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum && s.CategoryName == categoryValueEnum);

            if (configurationValue == null)
            {
                var newConfigurationValue = new ConfigurationValue();
                newConfigurationValue.CategoryName = categoryValueEnum;
                newConfigurationValue.VarName = configurationValueEnum;
                newConfigurationValue.VarType = ConfigurationDataType.Integer;
                newConfigurationValue.VarValue = value.ToString();

                this.primaryDataContext.ConfigurationValues.Add(newConfigurationValue);
                await this.primaryDataContext.SaveChangesAsync();
            }
            else
            {
                if (configurationValue.VarType == ConfigurationDataType.Integer)
                {
                    configurationValue.VarValue = value.ToString();
                    await this.primaryDataContext.SaveChangesAsync();
                }
                else
                {
                    throw new DataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
                }
            }
        }

        /// <inheritdoc/>
        public async void SetIntegerRuntimeValue(long runtimeValueEnum, long categoryValueEnum, int value)
        {
            if (!this.CheckConfigurationDataType(runtimeValueEnum, categoryValueEnum, ConfigurationDataType.Integer))
                throw new DataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);

            var runtimeValue = this.primaryDataContext.RuntimeValues.FirstOrDefault(s => s.VarName == runtimeValueEnum && s.CategoryName == categoryValueEnum);

            if (runtimeValue == null)
            {
                var newRuntimeValue = new RuntimeValue();
                newRuntimeValue.CategoryName = categoryValueEnum;
                newRuntimeValue.VarName = runtimeValueEnum;
                newRuntimeValue.VarType = ConfigurationDataType.Integer;
                newRuntimeValue.VarValue = value.ToString();

                this.primaryDataContext.RuntimeValues.Add(newRuntimeValue);
                await this.primaryDataContext.SaveChangesAsync();
            }
            else
            {
                if (runtimeValue.VarType == ConfigurationDataType.Integer)
                {
                    runtimeValue.VarValue = value.ToString();
                    await this.primaryDataContext.SaveChangesAsync();
                }
                else
                {
                    throw new DataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
                }
            }
        }

        /// <inheritdoc/>
        public async void SetIPAddressConfigurationValue(long configurationValueEnum, long categoryValueEnum, IPAddress value)
        {
            if (!this.CheckConfigurationDataType(configurationValueEnum, categoryValueEnum, ConfigurationDataType.IPAddress))
                throw new DataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);

            var configurationValue = this.primaryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum && s.CategoryName == categoryValueEnum);

            if (configurationValue == null)
            {
                var newConfigurationValue = new ConfigurationValue();
                newConfigurationValue.CategoryName = categoryValueEnum;
                newConfigurationValue.VarName = configurationValueEnum;
                newConfigurationValue.VarType = ConfigurationDataType.IPAddress;
                newConfigurationValue.VarValue = value.ToString();

                this.primaryDataContext.ConfigurationValues.Add(newConfigurationValue);
                await this.primaryDataContext.SaveChangesAsync();
            }
            else
            {
                if (configurationValue.VarType == ConfigurationDataType.IPAddress)
                {
                    configurationValue.VarValue = value.ToString();
                    await this.primaryDataContext.SaveChangesAsync();
                }
                else
                {
                    throw new DataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
                }
            }
        }

        /// <inheritdoc/>
        public async void SetStringConfigurationValue(long configurationValueEnum, long categoryValueEnum, string value)
        {
            if (!this.CheckConfigurationDataType(configurationValueEnum, categoryValueEnum, ConfigurationDataType.String))
                throw new DataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);

            var configurationValue = this.primaryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum && s.CategoryName == categoryValueEnum);

            if (configurationValue == null)
            {
                var newConfigurationValue = new ConfigurationValue();
                newConfigurationValue.CategoryName = categoryValueEnum;
                newConfigurationValue.VarName = configurationValueEnum;
                newConfigurationValue.VarType = ConfigurationDataType.String;
                newConfigurationValue.VarValue = value;

                this.primaryDataContext.ConfigurationValues.Add(newConfigurationValue);
                await this.primaryDataContext.SaveChangesAsync();
            }
            else
            {
                if (configurationValue.VarType == ConfigurationDataType.String)
                {
                    configurationValue.VarValue = value;
                    await this.primaryDataContext.SaveChangesAsync();
                }
                else
                {
                    throw new DataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
                }
            }
        }

        /// <inheritdoc/>
        public async void SetStringRuntimeValue(long runtimeValueEnum, long categoryValueEnum, string value)
        {
            if (!this.CheckConfigurationDataType(runtimeValueEnum, categoryValueEnum, ConfigurationDataType.String))
                throw new DataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);

            var runtimeValue = this.primaryDataContext.RuntimeValues.FirstOrDefault(s => s.VarName == runtimeValueEnum && s.CategoryName == categoryValueEnum);

            if (runtimeValue == null)
            {
                var newRuntimeValue = new RuntimeValue();
                newRuntimeValue.CategoryName = categoryValueEnum;
                newRuntimeValue.VarName = runtimeValueEnum;
                newRuntimeValue.VarType = ConfigurationDataType.String;
                newRuntimeValue.VarValue = value;

                this.primaryDataContext.RuntimeValues.Add(newRuntimeValue);
                await this.primaryDataContext.SaveChangesAsync();
            }
            else
            {
                if (runtimeValue.VarType == ConfigurationDataType.String)
                {
                    runtimeValue.VarValue = value;
                    await this.primaryDataContext.SaveChangesAsync();
                }
                else
                {
                    throw new DataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
                }
            }
        }

        #endregion
    }
}
