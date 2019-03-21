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

            var configurationValue = this.primaryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum && s.CategoryName == categoryValueEnum);

            if (configurationValue != null)
            {
                if (configurationValue.VarType == DataType.Boolean)
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
        public bool GetBoolRuntimeValue(RuntimeValueEnum runtimeValueEnum)
        {
            var returnBoolValue = false;

            var runtimeValue = this.primaryDataContext.RuntimeValues.FirstOrDefault(s => s.VarName == runtimeValueEnum);

            if (runtimeValue != null)
            {
                if (runtimeValue.VarType == DataType.Boolean)
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

            var configurationValue = this.primaryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum && s.CategoryName == categoryValueEnum);

            if (configurationValue != null)
            {
                if (configurationValue.VarType == DataType.Date)
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
        public DateTime GetDateTimeRuntimeValue(RuntimeValueEnum runtimeValueEnum)
        {
            DateTime returnDateTimeValue;

            var runtimeValue = this.primaryDataContext.RuntimeValues.FirstOrDefault(s => s.VarName == runtimeValueEnum);

            if (runtimeValue != null)
            {
                if (runtimeValue.VarType == DataType.Date)
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

            var configurationValue = this.primaryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum && s.CategoryName == categoryValueEnum);

            if (configurationValue != null)
            {
                if (configurationValue.VarType == DataType.Float)
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
        public decimal GetDecimalRuntimeValue(RuntimeValueEnum runtimeValueEnum)
        {
            decimal returnDecimalValue = 0;

            var runtimeValue =
                this.primaryDataContext.RuntimeValues.FirstOrDefault(s => s.VarName == runtimeValueEnum);

            if (runtimeValue != null)
            {
                if (runtimeValue.VarType == DataType.Float)
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

            var configurationValue =
                this.primaryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum && s.CategoryName == categoryValueEnum);

            if (configurationValue != null)
            {
                if (configurationValue.VarType == DataType.Integer)
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
        public int GetIntegerRuntimeValue(RuntimeValueEnum runtimeValueEnum)
        {
            var returnIntegerValue = 0;

            var runtimeValue =
                this.primaryDataContext.RuntimeValues.FirstOrDefault(s => s.VarName == runtimeValueEnum);

            if (runtimeValue != null)
            {
                if (runtimeValue.VarType == DataType.Integer)
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

            var configurationValue =
                this.primaryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum && s.CategoryName == categoryValueEnum);

            if (configurationValue != null)
            {
                if (configurationValue.VarType == DataType.IPAddress)
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

            var configurationValue =
                this.primaryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum && s.CategoryName == categoryValueEnum);

            if (configurationValue != null)
            {
                if (configurationValue.VarType == DataType.String)
                    returnStringValue = configurationValue.VarValue;
                else
                    throw new DataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
            }
            else
                throw new ArgumentNullException();

            return returnStringValue;
        }

        /// <inheritdoc/>
        public string GetStringRuntimeValue(RuntimeValueEnum runtimeValueEnum)
        {
            var returnStringValue = "";

            var runtimeValue = this.primaryDataContext.RuntimeValues.FirstOrDefault(s => s.VarName == runtimeValueEnum);

            if (runtimeValue != null)
            {
                if (runtimeValue.VarType == DataType.String)
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
        public void SetBoolConfigurationValue(long configurationValueEnum, long categoryValueEnum, bool value)
        {
            var configurationValue = this.primaryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum && s.CategoryName == categoryValueEnum);

            if (configurationValue == null)
            {
                var newConfigurationValue = new ConfigurationValue();
                newConfigurationValue.CategoryName = categoryValueEnum;
                newConfigurationValue.VarName = configurationValueEnum;
                newConfigurationValue.VarType = DataType.Boolean;
                newConfigurationValue.VarValue = value.ToString();

                this.primaryDataContext.ConfigurationValues.Add(newConfigurationValue);
                this.primaryDataContext.SaveChanges();
            }
            else
            {
                if (configurationValue.VarType == DataType.Boolean)
                {
                    configurationValue.VarValue = value.ToString();
                    this.primaryDataContext.SaveChanges();
                }
                else
                {
                    throw new DataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
                }
            }
        }

        /// <inheritdoc/>
        public void SetBoolRuntimeValue(RuntimeValueEnum runtimeValueEnum, bool value)
        {
            var runtimeValue = this.primaryDataContext.RuntimeValues.FirstOrDefault(s => s.VarName == runtimeValueEnum);

            if (runtimeValue == null)
            {
                var newRuntimeValue = new RuntimeValue();
                newRuntimeValue.VarName = runtimeValueEnum;
                newRuntimeValue.VarType = DataType.Boolean;
                newRuntimeValue.VarValue = value.ToString();

                this.primaryDataContext.RuntimeValues.Add(newRuntimeValue);
                this.primaryDataContext.SaveChanges();
            }
            else
            {
                if (runtimeValue.VarType == DataType.Boolean)
                {
                    runtimeValue.VarValue = value.ToString();
                    this.primaryDataContext.SaveChanges();
                }
                else
                {
                    throw new DataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
                }
            }
        }

        /// <inheritdoc/>
        public void SetDateTimeConfigurationValue(long configurationValueEnum, long categoryValueEnum, DateTime value)
        {
            var configurationValue = this.primaryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum && s.CategoryName == categoryValueEnum);

            if (configurationValue == null)
            {
                var newConfigurationValue = new ConfigurationValue();
                newConfigurationValue.CategoryName = categoryValueEnum;
                newConfigurationValue.VarName = configurationValueEnum;
                newConfigurationValue.VarType = DataType.Date;
                newConfigurationValue.VarValue = value.ToString();

                this.primaryDataContext.ConfigurationValues.Add(newConfigurationValue);
                this.primaryDataContext.SaveChanges();
            }
            else
            {
                if (configurationValue.VarType == DataType.Date)
                {
                    configurationValue.VarValue = value.ToString();
                    this.primaryDataContext.SaveChanges();
                }
                else
                {
                    throw new DataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
                }
            }
        }

        /// <inheritdoc/>
        public void SetDateTimeRuntimeValue(RuntimeValueEnum runtimeValueEnum, DateTime value)
        {
            var runtimeValue = this.primaryDataContext.RuntimeValues.FirstOrDefault(s => s.VarName == runtimeValueEnum);

            if (runtimeValue == null)
            {
                var newRuntimeValue = new RuntimeValue();
                newRuntimeValue.VarName = runtimeValueEnum;
                newRuntimeValue.VarType = DataType.Date;
                newRuntimeValue.VarValue = value.ToString();

                this.primaryDataContext.RuntimeValues.Add(newRuntimeValue);
                this.primaryDataContext.SaveChanges();
            }
            else
            {
                if (runtimeValue.VarType == DataType.Date)
                {
                    runtimeValue.VarValue = value.ToString();
                    this.primaryDataContext.SaveChanges();
                }
                else
                {
                    throw new DataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
                }
            }
        }

        /// <inheritdoc/>
        public void SetDecimalConfigurationValue(long configurationValueEnum, long categoryValueEnum, decimal value)
        {
            var configurationValue = this.primaryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum && s.CategoryName == categoryValueEnum);

            if (configurationValue == null)
            {
                var newConfigurationValue = new ConfigurationValue();
                newConfigurationValue.CategoryName = categoryValueEnum;
                newConfigurationValue.VarName = configurationValueEnum;
                newConfigurationValue.VarType = DataType.Float;
                newConfigurationValue.VarValue = value.ToString();

                this.primaryDataContext.ConfigurationValues.Add(newConfigurationValue);
                this.primaryDataContext.SaveChanges();
            }
            else
            {
                if (configurationValue.VarType == DataType.Float)
                {
                    configurationValue.VarValue = value.ToString();
                    this.primaryDataContext.SaveChanges();
                }
                else
                {
                    throw new DataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
                }
            }
        }

        /// <inheritdoc/>
        public void SetDecimalRuntimeValue(RuntimeValueEnum runtimeValueEnum, decimal value)
        {
            var runtimeValue = this.primaryDataContext.RuntimeValues.FirstOrDefault(s => s.VarName == runtimeValueEnum);

            if (runtimeValue == null)
            {
                var newRuntimeValue = new RuntimeValue();
                newRuntimeValue.VarName = runtimeValueEnum;
                newRuntimeValue.VarType = DataType.Float;
                newRuntimeValue.VarValue = value.ToString();

                this.primaryDataContext.RuntimeValues.Add(newRuntimeValue);
                this.primaryDataContext.SaveChanges();
            }
            else
            {
                if (runtimeValue.VarType == DataType.Float)
                {
                    runtimeValue.VarValue = value.ToString();
                    this.primaryDataContext.SaveChanges();
                }
                else
                {
                    throw new DataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
                }
            }
        }

        /// <inheritdoc/>
        public void SetIntegerConfigurationValue(long configurationValueEnum, long categoryValueEnum, int value)
        {
            var configurationValue =
                this.primaryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum && s.CategoryName == categoryValueEnum);

            if (configurationValue == null)
            {
                var newConfigurationValue = new ConfigurationValue();
                newConfigurationValue.CategoryName = categoryValueEnum;
                newConfigurationValue.VarName = configurationValueEnum;
                newConfigurationValue.VarType = DataType.Integer;
                newConfigurationValue.VarValue = value.ToString();

                this.primaryDataContext.ConfigurationValues.Add(newConfigurationValue);
                this.primaryDataContext.SaveChanges();
            }
            else
            {
                if (configurationValue.VarType == DataType.Integer)
                {
                    configurationValue.VarValue = value.ToString();
                    this.primaryDataContext.SaveChanges();
                }
                else
                {
                    throw new DataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
                }
            }
        }

        /// <inheritdoc/>
        public void SetIntegerRuntimeValue(RuntimeValueEnum runtimeValueEnum, int value)
        {
            var runtimeValue =
                this.primaryDataContext.RuntimeValues.FirstOrDefault(s => s.VarName == runtimeValueEnum);

            if (runtimeValue == null)
            {
                var newRuntimeValue = new RuntimeValue();
                newRuntimeValue.VarName = runtimeValueEnum;
                newRuntimeValue.VarType = DataType.Integer;
                newRuntimeValue.VarValue = value.ToString();

                this.primaryDataContext.RuntimeValues.Add(newRuntimeValue);
                this.primaryDataContext.SaveChanges();
            }
            else
            {
                if (runtimeValue.VarType == DataType.Integer)
                {
                    runtimeValue.VarValue = value.ToString();
                    this.primaryDataContext.SaveChanges();
                }
                else
                {
                    throw new DataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
                }
            }
        }

        /// <inheritdoc/>
        public void SetIPAddressConfigurationValue(long configurationValueEnum, long categoryValueEnum, IPAddress value)
        {
            var configurationValue = this.primaryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum && s.CategoryName == categoryValueEnum);

            if (configurationValue == null)
            {
                var newConfigurationValue = new ConfigurationValue();
                newConfigurationValue.CategoryName = categoryValueEnum;
                newConfigurationValue.VarName = configurationValueEnum;
                newConfigurationValue.VarType = DataType.IPAddress;
                newConfigurationValue.VarValue = value.ToString();

                this.primaryDataContext.ConfigurationValues.Add(newConfigurationValue);
                this.primaryDataContext.SaveChanges();
            }
            else
            {
                if (configurationValue.VarType == DataType.Boolean)
                {
                    configurationValue.VarValue = value.ToString();
                    this.primaryDataContext.SaveChanges();
                }
                else
                {
                    throw new DataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
                }
            }
        }

        /// <inheritdoc/>
        public void SetStringConfigurationValue(long configurationValueEnum, long categoryValueEnum, string value)
        {
            var configurationValue = this.primaryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum && s.CategoryName == categoryValueEnum);

            if (configurationValue == null)
            {
                var newConfigurationValue = new ConfigurationValue();
                newConfigurationValue.CategoryName = categoryValueEnum;
                newConfigurationValue.VarName = configurationValueEnum;
                newConfigurationValue.VarType = DataType.String;
                newConfigurationValue.VarValue = value;

                this.primaryDataContext.ConfigurationValues.Add(newConfigurationValue);
                this.primaryDataContext.SaveChanges();
            }
            else
            {
                if (configurationValue.VarType == DataType.String)
                {
                    configurationValue.VarValue = value;
                    this.primaryDataContext.SaveChanges();
                }
                else
                {
                    throw new DataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
                }
            }
        }

        /// <inheritdoc/>
        public void SetStringRuntimeValue(RuntimeValueEnum runtimeValueEnum, string value)
        {
            var runtimeValue = this.primaryDataContext.RuntimeValues.FirstOrDefault(s => s.VarName == runtimeValueEnum);

            if (runtimeValue == null)
            {
                var newRuntimeValue = new RuntimeValue();
                newRuntimeValue.VarName = runtimeValueEnum;
                newRuntimeValue.VarType = DataType.String;
                newRuntimeValue.VarValue = value;

                this.primaryDataContext.RuntimeValues.Add(newRuntimeValue);
                this.primaryDataContext.SaveChanges();
            }
            else
            {
                if (runtimeValue.VarType == DataType.String)
                {
                    runtimeValue.VarValue = value;
                    this.primaryDataContext.SaveChanges();
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
