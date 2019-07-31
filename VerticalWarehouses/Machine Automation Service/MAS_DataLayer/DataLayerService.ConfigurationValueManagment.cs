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
        public bool GetBoolConfigurationValue(long configurationValueEnum, long categoryValueEnum)
        {
            if (!this.CheckConfigurationDataType(configurationValueEnum, categoryValueEnum, ConfigurationDataType.Boolean))
            {
                this.logger.LogCritical($"1:Exception: get Boolean for {configurationValueEnum} variable - Exception Code: {DataLayerExceptionCode.DatatypeException}");
                throw new DataLayerException(DataLayerExceptionCode.DatatypeException);
            }

            if (configurationValueEnum == (long)SetupStatus.VerticalHomingDone)
            {
                return this.setupStatusVolatile.VerticalHomingDone;
            }

            bool returnBoolValue;

            var configurationValue = this.RetrieveConfigurationValue(configurationValueEnum, categoryValueEnum);
            if (configurationValue != null)
            {
                if (!bool.TryParse(configurationValue.VarValue, out returnBoolValue))
                {
                    this.logger.LogCritical($"3:Exception: Parse failed for {configurationValueEnum} in Primary partition - Error Code: {DataLayerPersistentExceptionCode.ParseValue}", DataLayerPersistentExceptionCode.ParseValue);
                    throw new DataLayerPersistentException(DataLayerPersistentExceptionCode.ParseValue);
                }
            }
            else
            {
                if (categoryValueEnum == (long)ConfigurationCategory.SetupStatus)
                {
                    returnBoolValue = false;
                }
                else
                {
                    this.logger.LogCritical($"4:Exception: value not found for {configurationValueEnum} - Exception Code: {DataLayerPersistentExceptionCode.ValueNotFound}");

                    throw new DataLayerPersistentException(DataLayerPersistentExceptionCode.ValueNotFound);
                }
            }

            return returnBoolValue;
        }

        /// <inheritdoc/>
        public DateTime GetDateTimeConfigurationValue(long configurationValueEnum, long categoryValueEnum)
        {
            if (!this.CheckConfigurationDataType(configurationValueEnum, categoryValueEnum, ConfigurationDataType.Date))
            {
                this.logger.LogCritical($"1:Exception: get DateTime for {configurationValueEnum} variable - Exception Code: {DataLayerExceptionCode.DatatypeException}");
                throw new DataLayerException(DataLayerExceptionCode.DatatypeException);
            }

            DateTime returnDateTimeValue;
            var configurationValue = this.RetrieveConfigurationValue(configurationValueEnum, categoryValueEnum);
            if (configurationValue != null)
            {
                if (!DateTime.TryParse(configurationValue.VarValue, out returnDateTimeValue))
                {
                    this.logger.LogCritical($"3:Exception: Parse failed for {configurationValueEnum} in Primary partition - Error Code: {DataLayerPersistentExceptionCode.ParseValue}", DataLayerPersistentExceptionCode.ParseValue);
                    throw new DataLayerPersistentException(DataLayerPersistentExceptionCode.ParseValue);
                }
            }
            else
            {
                this.logger.LogCritical($"4:Exception: value not found for {configurationValueEnum} - Exception Code: {DataLayerPersistentExceptionCode.ValueNotFound}");

                throw new DataLayerPersistentException(DataLayerPersistentExceptionCode.ValueNotFound);
            }

            return returnDateTimeValue;
        }

        /// <inheritdoc/>
        public decimal GetDecimalConfigurationValue(long configurationValueEnum, long categoryValueEnum)
        {
            if (!this.CheckConfigurationDataType(configurationValueEnum, categoryValueEnum, ConfigurationDataType.Float))
            {
                this.logger.LogCritical($"1:Exception: get Decimal for {configurationValueEnum} variable - Exception Code: {DataLayerExceptionCode.DatatypeException}");
                throw new DataLayerException(DataLayerExceptionCode.DatatypeException);
            }

            decimal returnDecimalValue;
            var configurationValue = this.RetrieveConfigurationValue(configurationValueEnum, categoryValueEnum);
            if (configurationValue != null)
            {
                if (!decimal.TryParse(configurationValue.VarValue, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture.NumberFormat, out returnDecimalValue))
                {
                    this.logger.LogCritical($"3:Exception: Parse failed for {configurationValueEnum} in Primary partition - Error Code: {DataLayerPersistentExceptionCode.ParseValue}", DataLayerPersistentExceptionCode.ParseValue);
                    throw new DataLayerPersistentException(DataLayerPersistentExceptionCode.ParseValue);
                }
            }
            else
            {
                this.logger.LogCritical($"4:Exception: value not found for {configurationValueEnum} - Exception Code: {DataLayerPersistentExceptionCode.ValueNotFound}");

                throw new DataLayerPersistentException(DataLayerPersistentExceptionCode.ValueNotFound);
            }

            return returnDecimalValue;
        }

        /// <inheritdoc/>
        public int GetIntegerConfigurationValue(long configurationValueEnum, long categoryValueEnum)
        {
            if (!this.CheckConfigurationDataType(configurationValueEnum, categoryValueEnum, ConfigurationDataType.Integer))
            {
                this.logger.LogCritical($"1:Exception: get Integer for {configurationValueEnum} variable - Exception Code: {DataLayerExceptionCode.DatatypeException}");
                throw new DataLayerException(DataLayerExceptionCode.DatatypeException);
            }

            int returnIntegerValue;

            var configurationValue = this.RetrieveConfigurationValue(configurationValueEnum, categoryValueEnum);
            if (configurationValue != null)
            {
                if (!int.TryParse(configurationValue.VarValue, out returnIntegerValue))
                {
                    this.logger.LogCritical($"3:Exception: Parse failed for {configurationValueEnum} in Primary partition - Error Code: {DataLayerPersistentExceptionCode.ParseValue}", DataLayerPersistentExceptionCode.ParseValue);
                    throw new DataLayerPersistentException(DataLayerPersistentExceptionCode.ParseValue);
                }
            }
            else
            {
                this.logger.LogCritical($"4:Exception: value not found for {configurationValueEnum} - Exception Code: {DataLayerPersistentExceptionCode.ValueNotFound}");

                throw new DataLayerPersistentException(DataLayerPersistentExceptionCode.ValueNotFound);
            }

            return returnIntegerValue;
        }

        /// <inheritdoc/>
        public IPAddress GetIpAddressConfigurationValue(long configurationValueEnum, long categoryValueEnum)
        {
            if (!this.CheckConfigurationDataType(configurationValueEnum, categoryValueEnum, ConfigurationDataType.IPAddress))
            {
                this.logger.LogCritical($"1:Exception: get IP Address for {configurationValueEnum} variable - Exception Code: {DataLayerExceptionCode.DatatypeException}");
                throw new DataLayerException(DataLayerExceptionCode.DatatypeException);
            }

            IPAddress returnIpAddressValue;

            var configurationValue = this.RetrieveConfigurationValue(configurationValueEnum, categoryValueEnum);
            if (configurationValue != null)
            {
                returnIpAddressValue = IPAddress.Parse(configurationValue.VarValue);
            }
            else
            {
                this.logger.LogCritical($"3:Exception: value not found for {configurationValueEnum} - Exception Code: {DataLayerPersistentExceptionCode.ValueNotFound}");

                throw new DataLayerPersistentException(DataLayerPersistentExceptionCode.ValueNotFound);
            }

            return returnIpAddressValue;
        }

        /// <inheritdoc/>
        public string GetStringConfigurationValue(long configurationValueEnum, long categoryValueEnum)
        {
            if (!this.CheckConfigurationDataType(configurationValueEnum, categoryValueEnum, ConfigurationDataType.String))
            {
                this.logger.LogCritical($"1:Exception: get string for {configurationValueEnum} variable - Exception Code: {DataLayerExceptionCode.DatatypeException}");
                throw new DataLayerException(DataLayerExceptionCode.DatatypeException);
            }

            string returnStringValue;

            var configurationValue = this.RetrieveConfigurationValue(configurationValueEnum, categoryValueEnum);
            if (configurationValue != null)
            {
                returnStringValue = configurationValue.VarValue;
            }
            else
            {
                this.logger.LogCritical($"3:Exception: value not found for {configurationValueEnum} - Exception Code: {DataLayerPersistentExceptionCode.ValueNotFound}");

                throw new DataLayerPersistentException(DataLayerPersistentExceptionCode.ValueNotFound);
            }

            return returnStringValue;
        }

        /// <inheritdoc/>
        public void SetBoolConfigurationValue(long configurationValueEnum, long categoryValueEnum, bool value)
        {
            if (!this.CheckConfigurationDataType(configurationValueEnum, categoryValueEnum, ConfigurationDataType.Boolean))
            {
                this.logger.LogCritical($"1:Exception: wrong datatype during set Boolean - Exception Code: {DataLayerExceptionCode.DatatypeException}");

                throw new DataLayerException(DataLayerExceptionCode.DatatypeException);
            }

            if (configurationValueEnum == (long)SetupStatus.VerticalHomingDone)
            {
                if (this.setupStatusVolatile != null)
                {
                    this.setupStatusVolatile.VerticalHomingDone = value;
                }

                return;
            }

            var newConfigurationValue = new ConfigurationValue
            {
                CategoryName = categoryValueEnum,
                VarName = configurationValueEnum,
                VarType = ConfigurationDataType.Boolean,
                VarValue = value.ToString(CultureInfo.InvariantCulture),
            };

            this.SetUpdateConfigurationValueCommon(newConfigurationValue);
        }

        /// <inheritdoc/>
        public void SetDateTimeConfigurationValue(long configurationValueEnum, long categoryValueEnum, DateTime value)
        {
            if (!this.CheckConfigurationDataType(configurationValueEnum, categoryValueEnum, ConfigurationDataType.Date))
            {
                this.logger.LogCritical($"1:Exception: wrong datatype during set Date - Exception Code: {DataLayerExceptionCode.DatatypeException}");

                throw new DataLayerException(DataLayerExceptionCode.DatatypeException);
            }

            var newConfigurationValue = new ConfigurationValue
            {
                CategoryName = categoryValueEnum,
                VarName = configurationValueEnum,
                VarType = ConfigurationDataType.Date,
                VarValue = value.ToString(CultureInfo.InvariantCulture),
            };

            this.SetUpdateConfigurationValueCommon(newConfigurationValue);
        }

        /// <inheritdoc/>
        public void SetDecimalConfigurationValue(long configurationValueEnum, long categoryValueEnum,
            decimal value)
        {
            if (!this.CheckConfigurationDataType(configurationValueEnum, categoryValueEnum, ConfigurationDataType.Float))
            {
                this.logger.LogCritical($"1:Exception: wrong datatype during set Decimal - Exception Code: {DataLayerExceptionCode.DatatypeException}");

                throw new DataLayerException(DataLayerExceptionCode.DatatypeException);
            }

            var newConfigurationValue = new ConfigurationValue
            {
                CategoryName = categoryValueEnum,
                VarName = configurationValueEnum,
                VarType = ConfigurationDataType.Float,
                VarValue = value.ToString(CultureInfo.InvariantCulture),
            };

            this.SetUpdateConfigurationValueCommon(newConfigurationValue);
        }

        /// <inheritdoc/>
        public void SetIntegerConfigurationValue(long configurationValueEnum, long categoryValueEnum, int value)
        {
            if (!this.CheckConfigurationDataType(configurationValueEnum, categoryValueEnum, ConfigurationDataType.Integer))
            {
                this.logger.LogCritical($"1:Exception: wrong datatype during set Integer - Exception Code: {DataLayerExceptionCode.DatatypeException}");

                throw new DataLayerException(DataLayerExceptionCode.DatatypeException);
            }

            var newConfigurationValue = new ConfigurationValue
            {
                CategoryName = categoryValueEnum,
                VarName = configurationValueEnum,
                VarType = ConfigurationDataType.Integer,
                VarValue = value.ToString(CultureInfo.InvariantCulture),
            };

            this.SetUpdateConfigurationValueCommon(newConfigurationValue);
        }

        public void SetIpAddressConfigurationValue(long configurationValueEnum, long categoryValueEnum, IPAddress value)
        {
            if (!this.CheckConfigurationDataType(configurationValueEnum, categoryValueEnum, ConfigurationDataType.IPAddress))
            {
                this.logger.LogCritical($"1:Exception: wrong datatype during set IP Address - Exception Code: {DataLayerExceptionCode.DatatypeException}");

                throw new DataLayerException(DataLayerExceptionCode.DatatypeException);
            }

            var newConfigurationValue = new ConfigurationValue
            {
                CategoryName = categoryValueEnum,
                VarName = configurationValueEnum,
                VarType = ConfigurationDataType.IPAddress,
                VarValue = value.ToString(),
            };

            this.SetUpdateConfigurationValueCommon(newConfigurationValue);
        }

        /// <inheritdoc/>
        public void SetStringConfigurationValue(long configurationValueEnum, long categoryValueEnum, string value)
        {
            if (!this.CheckConfigurationDataType(configurationValueEnum, categoryValueEnum, ConfigurationDataType.String))
            {
                this.logger.LogCritical($"1:Exception: wrong datatype during set String - Exception Code: {DataLayerExceptionCode.DatatypeException}");

                throw new DataLayerException(DataLayerExceptionCode.DatatypeException);
            }

            var newConfigurationValue = new ConfigurationValue
            {
                CategoryName = categoryValueEnum,
                VarName = configurationValueEnum,
                VarType = ConfigurationDataType.String,
                VarValue = value
            };

            this.SetUpdateConfigurationValueCommon(newConfigurationValue);
        }

        private ConfigurationValue RetrieveConfigurationValue(long configurationValueEnum, long categoryValueEnum)
        {
            ConfigurationValue configurationValue;

            try
            {
                using (var scope = this.serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<DataLayerContext>();

                    configurationValue = dbContext.ConfigurationValues
                        .FirstOrDefault(s =>
                            s.VarName == configurationValueEnum
                            &&
                            s.CategoryName == categoryValueEnum);
                }
            }
            catch
            {
                this.logger.LogCritical($"2:Exception: Parse failed for {configurationValueEnum} in Primary partition - Error Code: {DataLayerPersistentExceptionCode.DataContextNotValid}", DataLayerPersistentExceptionCode.DataContextNotValid);

                throw new DataLayerPersistentException(DataLayerPersistentExceptionCode.DataContextNotValid);
            }

            return configurationValue;
        }

        private void SetUpdateConfigurationValueCommon(ConfigurationValue newConfigurationValue)
        {
            using (var scope = this.serviceScopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<DataLayerContext>();

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
                    this.logger.LogCritical($"4:Exception: failed to write application log entry into database.");

                    throw;
                }
            }
        }

        #endregion
    }
}
