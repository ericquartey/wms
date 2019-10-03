using System;
using System.Globalization;
using System.Linq;
using System.Net;
using Ferretto.VW.MAS.DataLayer.DatabaseContext;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DataModels.Enumerations;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : IConfigurationValueManagmentDataLayer
    {
        #region Methods

        /// <inheritdoc/>
        public bool GetBoolConfigurationValue<TEnum>(TEnum configurationValueEnum, ConfigurationCategory category)
            where TEnum : Enum
        {
            if (!this.IsReady)
            {
                var message = "Data layer is not yet ready.";

                this.Logger.LogError(message);
                throw new DataLayerException(message);
            }

            if (!this.CheckConfigurationDataType((long)(object)configurationValueEnum, category, ConfigurationDataType.Boolean))
            {
                this.Logger.LogCritical($"1:Exception: get Boolean for '{category}.{configurationValueEnum}' variable - Exception Code: {DataLayerExceptionCode.DatatypeException}");
                throw new DataLayerException(DataLayerExceptionCode.DatatypeException);
            }

            bool returnBoolValue;

            var configurationValue = this.RetrieveConfigurationValue((long)(object)configurationValueEnum, category);
            if (configurationValue != null)
            {
                if (!bool.TryParse(configurationValue.VarValue, out returnBoolValue))
                {
                    this.Logger.LogCritical(
                        $"Unable to parse value '{configurationValue.VarValue}' as boolean for field '{category}.{configurationValueEnum}'.", DataLayerPersistentExceptionCode.ParseValue);
                    throw new DataLayerPersistentException(DataLayerPersistentExceptionCode.ParseValue);
                }
            }
            else
            {
                this.Logger.LogTrace(
                    $"No value is available in database for '{category}.{configurationValueEnum}'.");

                throw new DataLayerPersistentException(DataLayerPersistentExceptionCode.ValueNotFound);
            }

            return returnBoolValue;
        }

        /// <inheritdoc/>
        public DateTime GetDateTimeConfigurationValue<TEnum>(TEnum configurationValueEnum, ConfigurationCategory category)
            where TEnum : Enum
        {
            if (!this.CheckConfigurationDataType((long)(object)configurationValueEnum, category, ConfigurationDataType.Date))
            {
                this.Logger.LogCritical($"1:Exception: get DateTime for '{category}.{configurationValueEnum}' variable - Exception Code: {DataLayerExceptionCode.DatatypeException}");
                throw new DataLayerException(DataLayerExceptionCode.DatatypeException);
            }

            DateTime returnDateTimeValue;
            var configurationValue = this.RetrieveConfigurationValue((long)(object)configurationValueEnum, category);
            if (configurationValue != null)
            {
                if (!DateTime.TryParse(configurationValue.VarValue, out returnDateTimeValue))
                {
                    this.Logger.LogCritical($"3:Exception: Parse failed for '{category}.{configurationValueEnum}' in Primary partition - Error Code: {DataLayerPersistentExceptionCode.ParseValue}", DataLayerPersistentExceptionCode.ParseValue);
                    throw new DataLayerPersistentException(DataLayerPersistentExceptionCode.ParseValue);
                }
            }
            else
            {
                this.Logger.LogTrace(
                    $"No value is available in database for '{configurationValueEnum}' as DateTime for field '{category}.{configurationValueEnum}'.", DataLayerPersistentExceptionCode.ParseValue);

                throw new DataLayerPersistentException(DataLayerPersistentExceptionCode.ValueNotFound);
            }

            return returnDateTimeValue;
        }

        /// <inheritdoc/>
        public decimal GetDecimalConfigurationValue<TEnum>(TEnum configurationValueEnum, ConfigurationCategory category)
            where TEnum : Enum
        {
            if (!this.CheckConfigurationDataType((long)(object)configurationValueEnum, category, ConfigurationDataType.Float))
            {
                this.Logger.LogCritical($"1:Exception: get Decimal for '{category}.{configurationValueEnum}' variable - Exception Code: {DataLayerExceptionCode.DatatypeException}");
                throw new DataLayerException(DataLayerExceptionCode.DatatypeException);
            }

            decimal returnDecimalValue;
            var configurationValue = this.RetrieveConfigurationValue((long)(object)configurationValueEnum, category);
            if (configurationValue != null)
            {
                if (!decimal.TryParse(configurationValue.VarValue, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture.NumberFormat, out returnDecimalValue))
                {
                    this.Logger.LogCritical($"3:Exception: Parse failed for '{category}.{configurationValueEnum}' in Primary partition - Error Code: {DataLayerPersistentExceptionCode.ParseValue}", DataLayerPersistentExceptionCode.ParseValue);
                    throw new DataLayerPersistentException(DataLayerPersistentExceptionCode.ParseValue);
                }
            }
            else
            {
                this.Logger.LogTrace(
                    $"No value is available in database for '{configurationValueEnum}' as Decimal for field '{category}.{configurationValueEnum}'.", DataLayerPersistentExceptionCode.ParseValue);
                throw new DataLayerPersistentException(DataLayerPersistentExceptionCode.ValueNotFound);
            }

            return returnDecimalValue;
        }

        /// <inheritdoc/>
        public int GetIntegerConfigurationValue<TEnum>(TEnum configurationValueEnum, ConfigurationCategory category)
            where TEnum : Enum
        {
            if (!this.CheckConfigurationDataType((long)(object)configurationValueEnum, category, ConfigurationDataType.Integer))
            {
                this.Logger.LogCritical($"1:Exception: get Integer for '{category}.{configurationValueEnum}' variable - Exception Code: {DataLayerExceptionCode.DatatypeException}");
                throw new DataLayerException(DataLayerExceptionCode.DatatypeException);
            }

            int returnIntegerValue;

            var configurationValue = this.RetrieveConfigurationValue((long)(object)configurationValueEnum, category);
            if (configurationValue != null)
            {
                if (!int.TryParse(configurationValue.VarValue, out returnIntegerValue))
                {
                    this.Logger.LogCritical($"3:Exception: Parse failed for '{category}.{configurationValueEnum}' in Primary partition - Error Code: {DataLayerPersistentExceptionCode.ParseValue}", DataLayerPersistentExceptionCode.ParseValue);
                    throw new DataLayerPersistentException(DataLayerPersistentExceptionCode.ParseValue);
                }
            }
            else
            {
                this.Logger.LogTrace(
                    $"No value is available in database for'{configurationValueEnum}' as Integer for field '{category}.{configurationValueEnum}'.", DataLayerPersistentExceptionCode.ParseValue);
                throw new DataLayerPersistentException(DataLayerPersistentExceptionCode.ValueNotFound);
            }

            return returnIntegerValue;
        }

        /// <inheritdoc/>
        public IPAddress GetIpAddressConfigurationValue<TEnum>(TEnum configurationValueEnum, ConfigurationCategory category)
            where TEnum : Enum
        {
            if (!this.CheckConfigurationDataType((long)(object)configurationValueEnum, category, ConfigurationDataType.IPAddress))
            {
                this.Logger.LogCritical($"1:Exception: get IP Address for '{category}.{configurationValueEnum}' variable - Exception Code: {DataLayerExceptionCode.DatatypeException}");
                throw new DataLayerException(DataLayerExceptionCode.DatatypeException);
            }

            IPAddress returnIpAddressValue;

            var configurationValue = this.RetrieveConfigurationValue((long)(object)configurationValueEnum, category);
            if (configurationValue != null)
            {
                returnIpAddressValue = IPAddress.Parse(configurationValue.VarValue);
            }
            else
            {
                this.Logger.LogTrace(
                    $"No value is available in database for '{category}.{configurationValueEnum}'.");

                throw new DataLayerPersistentException(DataLayerPersistentExceptionCode.ValueNotFound);
            }

            return returnIpAddressValue;
        }

        /// <inheritdoc/>
        public string GetStringConfigurationValue<TEnum>(TEnum configurationValueEnum, ConfigurationCategory category)
            where TEnum : Enum
        {
            if (!this.CheckConfigurationDataType((long)(object)configurationValueEnum, category, ConfigurationDataType.String))
            {
                this.Logger.LogCritical($"1:Exception: get string for '{category}.{configurationValueEnum}' variable - Exception Code: {DataLayerExceptionCode.DatatypeException}");
                throw new DataLayerException(DataLayerExceptionCode.DatatypeException);
            }

            string returnStringValue;

            var configurationValue = this.RetrieveConfigurationValue((long)(object)configurationValueEnum, category);
            if (configurationValue != null)
            {
                returnStringValue = configurationValue.VarValue;
            }
            else
            {
                this.Logger.LogTrace($"No value is available in database for '{category}.{configurationValueEnum}'.");
                throw new DataLayerPersistentException(DataLayerPersistentExceptionCode.ValueNotFound);
            }

            return returnStringValue;
        }

        /// <inheritdoc/>
        public void SetBoolConfigurationValue(long configurationValueEnum, ConfigurationCategory category, bool value)
        {
            if (!this.CheckConfigurationDataType(configurationValueEnum, category, ConfigurationDataType.Boolean))
            {
                this.Logger.LogCritical($"1:Exception: wrong datatype during set Boolean - Exception Code: {DataLayerExceptionCode.DatatypeException}");

                throw new DataLayerException(DataLayerExceptionCode.DatatypeException);
            }

            var newConfigurationValue = new ConfigurationValue
            {
                CategoryName = (long)category,
                VarName = configurationValueEnum,
                VarType = ConfigurationDataType.Boolean,
                VarValue = value.ToString(CultureInfo.InvariantCulture),
            };

            this.SetUpdateConfigurationValueCommon(newConfigurationValue);
        }

        /// <inheritdoc/>
        public void SetDateTimeConfigurationValue(
            long configurationValueEnum,
            ConfigurationCategory category,
            DateTime value)
        {
            if (!this.CheckConfigurationDataType(configurationValueEnum, category, ConfigurationDataType.Date))
            {
                this.Logger.LogCritical($"1:Exception: wrong datatype during set Date - Exception Code: {DataLayerExceptionCode.DatatypeException}");

                throw new DataLayerException(DataLayerExceptionCode.DatatypeException);
            }

            var newConfigurationValue = new ConfigurationValue
            {
                CategoryName = (long)category,
                VarName = configurationValueEnum,
                VarType = ConfigurationDataType.Date,
                VarValue = value.ToString(CultureInfo.InvariantCulture),
            };

            this.SetUpdateConfigurationValueCommon(newConfigurationValue);
        }

        /// <inheritdoc/>
        public void SetDecimalConfigurationValue(
            long configurationValueEnum,
            ConfigurationCategory category,
            decimal value)
        {
            if (!this.CheckConfigurationDataType(configurationValueEnum, category, ConfigurationDataType.Float))
            {
                this.Logger.LogCritical($"1:Exception: wrong datatype during set Decimal - Exception Code: {DataLayerExceptionCode.DatatypeException}");

                throw new DataLayerException(DataLayerExceptionCode.DatatypeException);
            }

            var newConfigurationValue = new ConfigurationValue
            {
                CategoryName = (long)category,
                VarName = configurationValueEnum,
                VarType = ConfigurationDataType.Float,
                VarValue = value.ToString(CultureInfo.InvariantCulture),
            };

            this.SetUpdateConfigurationValueCommon(newConfigurationValue);
        }

        /// <inheritdoc/>
        public void SetIntegerConfigurationValue(
            long configurationValueEnum,
            ConfigurationCategory category,
            int value)
        {
            if (!this.CheckConfigurationDataType(configurationValueEnum, category, ConfigurationDataType.Integer))
            {
                this.Logger.LogCritical($"1:Exception: wrong datatype during set Integer - Exception Code: {DataLayerExceptionCode.DatatypeException}");

                throw new DataLayerException(DataLayerExceptionCode.DatatypeException);
            }

            var newConfigurationValue = new ConfigurationValue
            {
                CategoryName = (long)category,
                VarName = configurationValueEnum,
                VarType = ConfigurationDataType.Integer,
                VarValue = value.ToString(CultureInfo.InvariantCulture),
            };

            this.SetUpdateConfigurationValueCommon(newConfigurationValue);
        }

        public void SetIpAddressConfigurationValue(
            long configurationValueEnum,
            ConfigurationCategory category,
            IPAddress value)
        {
            if (!this.CheckConfigurationDataType(configurationValueEnum, category, ConfigurationDataType.IPAddress))
            {
                this.Logger.LogCritical($"1:Exception: wrong datatype during set IP Address - Exception Code: {DataLayerExceptionCode.DatatypeException}");

                throw new DataLayerException(DataLayerExceptionCode.DatatypeException);
            }

            var newConfigurationValue = new ConfigurationValue
            {
                CategoryName = (long)category,
                VarName = configurationValueEnum,
                VarType = ConfigurationDataType.IPAddress,
                VarValue = value.ToString(),
            };

            this.SetUpdateConfigurationValueCommon(newConfigurationValue);
        }

        /// <inheritdoc/>
        public void SetStringConfigurationValue(
            long configurationValueEnum,
            ConfigurationCategory category,
            string value)
        {
            if (!this.CheckConfigurationDataType(configurationValueEnum, category, ConfigurationDataType.String))
            {
                this.Logger.LogCritical($"1:Exception: wrong datatype during set String - Exception Code: {DataLayerExceptionCode.DatatypeException}");

                throw new DataLayerException(DataLayerExceptionCode.DatatypeException);
            }

            var newConfigurationValue = new ConfigurationValue
            {
                CategoryName = (long)category,
                VarName = configurationValueEnum,
                VarType = ConfigurationDataType.String,
                VarValue = value,
            };

            this.SetUpdateConfigurationValueCommon(newConfigurationValue);
        }

        private ConfigurationValue RetrieveConfigurationValue(long configurationValueEnum, ConfigurationCategory category)
        {
            ConfigurationValue configurationValue;

            try
            {
                var dbContext = this.scope.ServiceProvider.GetRequiredService<DataLayerContext>();

                    configurationValue = dbContext.ConfigurationValues
                        .FirstOrDefault(s =>
                            s.VarName == configurationValueEnum
                            &&
                            s.CategoryName == (long)category);

            }
            catch
            {
                this.Logger.LogCritical($"2:Exception: Parse failed for '{category}.{configurationValueEnum}' in Primary partition - Error Code: {DataLayerPersistentExceptionCode.DataContextNotValid}", DataLayerPersistentExceptionCode.DataContextNotValid);

                throw new DataLayerPersistentException(DataLayerPersistentExceptionCode.DataContextNotValid);
            }

            return configurationValue;
        }

        private void SetUpdateConfigurationValueCommon(ConfigurationValue newConfigurationValue)
        {
            var dbContext = this.scope.ServiceProvider.GetRequiredService<DataLayerContext>();

                var configurationValue = dbContext.ConfigurationValues
                    .FirstOrDefault(s =>
                        s.VarName == newConfigurationValue.VarName
                        &&
                        s.CategoryName == newConfigurationValue.CategoryName);

                if (configurationValue == null)
                {
                    dbContext.ConfigurationValues.Add(newConfigurationValue);
                }
                else
                {
                    configurationValue.VarValue = newConfigurationValue.VarValue;
                }

                try
                {
                    dbContext.SaveChanges();
                }
                catch (Exception)
                {
                    this.Logger.LogCritical($"4:Exception: failed to write application log entry into database.");

                    throw;
                }

        }

        #endregion
    }
}
