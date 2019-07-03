using System;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using Ferretto.VW.MAS_DataLayer.Enumerations;
using Ferretto.VW.MAS_DataLayer.Interfaces;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS_DataLayer
{
    public partial class DataLayer : IDataLayerConfigurationValueManagment
    {
        #region Methods

        /// <inheritdoc/>
        public async Task<bool> GetBoolConfigurationValueAsync(long configurationValueEnum, long categoryValueEnum)
        {
            var returnBoolValue = false;
            ConfigurationValue primaryConfigurationValue;

            if (!this.CheckConfigurationDataType(configurationValueEnum, categoryValueEnum, ConfigurationDataType.Boolean))
            {
                this.logger.LogCritical($"1:Exception: get Boolean for {configurationValueEnum} variable - Exception Code: {DataLayerExceptionCode.DatatypeException}");
                throw new DataLayerException(DataLayerExceptionCode.DatatypeException);
            }

            if (configurationValueEnum == (long)SetupStatus.VerticalHomingDone)
            {
                return this.setupStatusVolatile?.VerticalHomingDone ?? false;
            }

            try
            {
                primaryConfigurationValue =
                    await this.primaryDataContext.ConfigurationValues.FirstOrDefaultAsync(s => s.VarName == configurationValueEnum && s.CategoryName == categoryValueEnum, cancellationToken: this.stoppingToken);
            }
            catch
            {
                this.logger.LogCritical($"2:Exception: Parse failed for {configurationValueEnum} in Primary partition - Error Code: {DataLayerPersistentExceptionCode.DataContextNotValid}", DataLayerPersistentExceptionCode.DataContextNotValid);

                throw new DataLayerPersistentException(DataLayerPersistentExceptionCode.DataContextNotValid);
            }

            if (primaryConfigurationValue != null)
            {
                if (!bool.TryParse(primaryConfigurationValue.VarValue, out returnBoolValue))
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
        public async Task<DateTime> GetDateTimeConfigurationValueAsync(long configurationValueEnum, long categoryValueEnum)
        {
            DateTime returnDateTimeValue;
            ConfigurationValue primaryConfigurationValue;

            if (!this.CheckConfigurationDataType(configurationValueEnum, categoryValueEnum, ConfigurationDataType.Date))
            {
                this.logger.LogCritical($"1:Exception: get DateTime for {configurationValueEnum} variable - Exception Code: {DataLayerExceptionCode.DatatypeException}");
                throw new DataLayerException(DataLayerExceptionCode.DatatypeException);
            }

            try
            {
                primaryConfigurationValue =
                    await this.primaryDataContext.ConfigurationValues.FirstOrDefaultAsync(s => s.VarName == configurationValueEnum && s.CategoryName == categoryValueEnum, cancellationToken: this.stoppingToken);
            }
            catch
            {
                this.logger.LogCritical($"2:Exception: Parse failed for {configurationValueEnum} in Primary partition - Error Code: {DataLayerPersistentExceptionCode.DataContextNotValid}", DataLayerPersistentExceptionCode.DataContextNotValid);

                throw new DataLayerPersistentException(DataLayerPersistentExceptionCode.DataContextNotValid);
            }

            if (primaryConfigurationValue != null)
            {
                if (!DateTime.TryParse(primaryConfigurationValue.VarValue, out returnDateTimeValue))
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
        public async Task<decimal> GetDecimalConfigurationValueAsync(long configurationValueEnum, long categoryValueEnum)
        {
            decimal returnDecimalValue = 0;
            ConfigurationValue primaryConfigurationValue;

            if (!this.CheckConfigurationDataType(configurationValueEnum, categoryValueEnum, ConfigurationDataType.Float))
            {
                this.logger.LogCritical($"1:Exception: get Decimal for {configurationValueEnum} variable - Exception Code: {DataLayerExceptionCode.DatatypeException}");
                throw new DataLayerException(DataLayerExceptionCode.DatatypeException);
            }

            try
            {
                primaryConfigurationValue =
                    await this.primaryDataContext.ConfigurationValues.FirstOrDefaultAsync(s => s.VarName == configurationValueEnum && s.CategoryName == categoryValueEnum, cancellationToken: this.stoppingToken);
            }
            catch
            {
                this.logger.LogCritical($"2:Exception: Parse failed for {configurationValueEnum} in Primary partition - Error Code: {DataLayerPersistentExceptionCode.DataContextNotValid}", DataLayerPersistentExceptionCode.DataContextNotValid);

                throw new DataLayerPersistentException(DataLayerPersistentExceptionCode.DataContextNotValid);
            }

            if (primaryConfigurationValue != null)
            {
                if (!decimal.TryParse(primaryConfigurationValue.VarValue, out returnDecimalValue))
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
        public async Task<int> GetIntegerConfigurationValueAsync(long configurationValueEnum, long categoryValueEnum)
        {
            var returnIntegerValue = 0;
            ConfigurationValue primaryConfigurationValue;

            if (!this.CheckConfigurationDataType(configurationValueEnum, categoryValueEnum, ConfigurationDataType.Integer))
            {
                this.logger.LogCritical($"1:Exception: get Integer for {configurationValueEnum} variable - Exception Code: {DataLayerExceptionCode.DatatypeException}");
                throw new DataLayerException(DataLayerExceptionCode.DatatypeException);
            }

            try
            {
                primaryConfigurationValue =
                    await this.primaryDataContext.ConfigurationValues.FirstOrDefaultAsync(s => s.VarName == configurationValueEnum && s.CategoryName == categoryValueEnum, cancellationToken: this.stoppingToken);
            }
            catch
            {
                this.logger.LogCritical($"2:Exception: Parse failed for {configurationValueEnum} in Primary partition - Error Code: {DataLayerPersistentExceptionCode.DataContextNotValid}", DataLayerPersistentExceptionCode.DataContextNotValid);

                throw new DataLayerPersistentException(DataLayerPersistentExceptionCode.DataContextNotValid);
            }

            if (primaryConfigurationValue != null)
            {
                if (!int.TryParse(primaryConfigurationValue.VarValue, out returnIntegerValue))
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
        public async Task<IPAddress> GetIPAddressConfigurationValueAsync(long configurationValueEnum, long categoryValueEnum)
        {
            IPAddress returnIPAddressValue = null;
            ConfigurationValue primaryConfigurationValue;

            if (!this.CheckConfigurationDataType(configurationValueEnum, categoryValueEnum, ConfigurationDataType.IPAddress))
            {
                this.logger.LogCritical($"1:Exception: get IP Address for {configurationValueEnum} variable - Exception Code: {DataLayerExceptionCode.DatatypeException}");
                throw new DataLayerException(DataLayerExceptionCode.DatatypeException);
            }

            try
            {
                primaryConfigurationValue =
                    await this.primaryDataContext.ConfigurationValues.FirstOrDefaultAsync(s => s.VarName == configurationValueEnum && s.CategoryName == categoryValueEnum, cancellationToken: this.stoppingToken);
            }
            catch
            {
                this.logger.LogCritical($"2:Exception: Parse failed for {configurationValueEnum} in Primary partition - Error Code: {DataLayerPersistentExceptionCode.DataContextNotValid}", DataLayerPersistentExceptionCode.DataContextNotValid);

                throw new DataLayerPersistentException(DataLayerPersistentExceptionCode.DataContextNotValid);
            }

            if (primaryConfigurationValue != null)
            {
                returnIPAddressValue = IPAddress.Parse(primaryConfigurationValue.VarValue);
            }
            else
            {
                this.logger.LogCritical($"3:Exception: value not found for {configurationValueEnum} - Exception Code: {DataLayerPersistentExceptionCode.ValueNotFound}");

                throw new DataLayerPersistentException(DataLayerPersistentExceptionCode.ValueNotFound);
            }

            return returnIPAddressValue;
        }

        /// <inheritdoc/>
        public async Task<string> GetStringConfigurationValueAsync(long configurationValueEnum, long categoryValueEnum)
        {
            var returnStringValue = string.Empty;
            ConfigurationValue primaryConfigurationValue;

            if (!this.CheckConfigurationDataType(configurationValueEnum, categoryValueEnum, ConfigurationDataType.String))
            {
                this.logger.LogCritical($"1:Exception: get string for {configurationValueEnum} variable - Exception Code: {DataLayerExceptionCode.DatatypeException}");
                throw new DataLayerException(DataLayerExceptionCode.DatatypeException);
            }
            try
            {
                primaryConfigurationValue =
                    await this.primaryDataContext.ConfigurationValues.FirstOrDefaultAsync(s => s.VarName == configurationValueEnum && s.CategoryName == categoryValueEnum, cancellationToken: this.stoppingToken);
            }
            catch
            {
                this.logger.LogCritical($"2:Exception: Parse failed for {configurationValueEnum} in Primary partition - Error Code: {DataLayerPersistentExceptionCode.DataContextNotValid}", DataLayerPersistentExceptionCode.DataContextNotValid);

                throw new DataLayerPersistentException(DataLayerPersistentExceptionCode.DataContextNotValid);
            }

            if (primaryConfigurationValue != null)
            {
                returnStringValue = primaryConfigurationValue.VarValue;
            }
            else
            {
                this.logger.LogCritical($"3:Exception: value not found for {configurationValueEnum} - Exception Code: {DataLayerPersistentExceptionCode.ValueNotFound}");

                throw new DataLayerPersistentException(DataLayerPersistentExceptionCode.ValueNotFound);
            }

            return returnStringValue;
        }

        /// <inheritdoc/>
        public async Task SetBoolConfigurationValueAsync(long configurationValueEnum, long categoryValueEnum, bool value)
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

            var newConfigurationValue = new ConfigurationValue();
            newConfigurationValue.CategoryName = categoryValueEnum;
            newConfigurationValue.VarName = configurationValueEnum;
            newConfigurationValue.VarType = ConfigurationDataType.Boolean;
            newConfigurationValue.VarValue = value.ToString();

            await this.SetConfigurationValueCommonAsync(newConfigurationValue, ConfigurationDataType.Boolean);
        }

        /// <inheritdoc/>
        public async Task SetDateTimeConfigurationValueAsync(long configurationValueEnum, long categoryValueEnum, DateTime value)
        {
            if (!this.CheckConfigurationDataType(configurationValueEnum, categoryValueEnum, ConfigurationDataType.Date))
            {
                this.logger.LogCritical($"1:Exception: wrong datatype during set Date - Exception Code: {DataLayerExceptionCode.DatatypeException}");

                throw new DataLayerException(DataLayerExceptionCode.DatatypeException);
            }

            var newConfigurationValue = new ConfigurationValue();
            newConfigurationValue.CategoryName = categoryValueEnum;
            newConfigurationValue.VarName = configurationValueEnum;
            newConfigurationValue.VarType = ConfigurationDataType.Date;
            newConfigurationValue.VarValue = value.ToString();

            await this.SetConfigurationValueCommonAsync(newConfigurationValue, ConfigurationDataType.Date);
        }

        /// <inheritdoc/>
        public async Task SetDecimalConfigurationValueAsync(long configurationValueEnum, long categoryValueEnum, decimal value)
        {
            if (!this.CheckConfigurationDataType(configurationValueEnum, categoryValueEnum, ConfigurationDataType.Float))
            {
                this.logger.LogCritical($"1:Exception: wrong datatype during set Decimal - Exception Code: {DataLayerExceptionCode.DatatypeException}");

                throw new DataLayerException(DataLayerExceptionCode.DatatypeException);
            }

            var newConfigurationValue = new ConfigurationValue();
            newConfigurationValue.CategoryName = categoryValueEnum;
            newConfigurationValue.VarName = configurationValueEnum;
            newConfigurationValue.VarType = ConfigurationDataType.Float;
            newConfigurationValue.VarValue = value.ToString();

            await this.SetConfigurationValueCommonAsync(newConfigurationValue, ConfigurationDataType.Float);
        }

        /// <inheritdoc/>
        public async Task SetIntegerConfigurationValueAsync(long configurationValueEnum, long categoryValueEnum, int value)
        {
            if (!this.CheckConfigurationDataType(configurationValueEnum, categoryValueEnum, ConfigurationDataType.Integer))
            {
                this.logger.LogCritical($"1:Exception: wrong datatype during set Integer - Exception Code: {DataLayerExceptionCode.DatatypeException}");

                throw new DataLayerException(DataLayerExceptionCode.DatatypeException);
            }

            var newConfigurationValue = new ConfigurationValue();
            newConfigurationValue.CategoryName = categoryValueEnum;
            newConfigurationValue.VarName = configurationValueEnum;
            newConfigurationValue.VarType = ConfigurationDataType.Integer;
            newConfigurationValue.VarValue = value.ToString();

            await this.SetConfigurationValueCommonAsync(newConfigurationValue, ConfigurationDataType.Integer);
        }

        /// <inheritdoc/>
        public async Task SetIPAddressConfigurationValueAsync(long configurationValueEnum, long categoryValueEnum, IPAddress value)
        {
            if (!this.CheckConfigurationDataType(configurationValueEnum, categoryValueEnum, ConfigurationDataType.IPAddress))
            {
                this.logger.LogCritical($"1:Exception: wrong datatype during set IP Address - Exception Code: {DataLayerExceptionCode.DatatypeException}");

                throw new DataLayerException(DataLayerExceptionCode.DatatypeException);
            }

            var newConfigurationValue = new ConfigurationValue();
            newConfigurationValue.CategoryName = categoryValueEnum;
            newConfigurationValue.VarName = configurationValueEnum;
            newConfigurationValue.VarType = ConfigurationDataType.IPAddress;
            newConfigurationValue.VarValue = value.ToString();

            await this.SetConfigurationValueCommonAsync(newConfigurationValue, ConfigurationDataType.IPAddress);
        }

        /// <inheritdoc/>
        public async Task SetStringConfigurationValueAsync(long configurationValueEnum, long categoryValueEnum, string value)
        {
            if (!this.CheckConfigurationDataType(configurationValueEnum, categoryValueEnum, ConfigurationDataType.String))
            {
                this.logger.LogCritical($"1:Exception: wrong datatype during set String - Exception Code: {DataLayerExceptionCode.DatatypeException}");

                throw new DataLayerException(DataLayerExceptionCode.DatatypeException);
            }

            var newConfigurationValue = new ConfigurationValue();
            newConfigurationValue.CategoryName = categoryValueEnum;
            newConfigurationValue.VarName = configurationValueEnum;
            newConfigurationValue.VarType = ConfigurationDataType.String;
            newConfigurationValue.VarValue = value;

            await this.SetConfigurationValueCommonAsync(newConfigurationValue, ConfigurationDataType.String);
        }

        private async Task SetConfigurationValueCommonAsync(ConfigurationValue newConfigurationValue, ConfigurationDataType configurationDataType)
        {
            var primaryPartitionError = false;
            var secondaryPartitionError = false;

            Expression<Func<ConfigurationValue, bool>> queryString = s => s.VarName == newConfigurationValue.VarName && s.CategoryName == newConfigurationValue.CategoryName;
            var primaryConfigurationValue = await this.primaryDataContext.ConfigurationValues.FirstOrDefaultAsync(queryString, cancellationToken: this.stoppingToken);

            try
            {
                if (primaryConfigurationValue == null)
                {
                    this.primaryDataContext.ConfigurationValues.Add(newConfigurationValue);
                }
                else
                {
                    primaryConfigurationValue.VarValue = newConfigurationValue.VarValue;
                }
                await this.primaryDataContext.SaveChangesAsync(this.stoppingToken);
            }
            catch
            {
                primaryPartitionError = true;
            }

            // INFO It is true if the secondary DB is not suppressed
            if (!this.suppressSecondary)
            {
                try
                {
                    // INFO the secondaryPartition must be aligned at the primary
                    if (primaryConfigurationValue == null)
                    {
                        this.secondaryDataContext.ConfigurationValues.Add(newConfigurationValue);
                    }
                    else
                    {
                        var secondaryConfigurationValue = await this.secondaryDataContext.ConfigurationValues.FirstOrDefaultAsync(queryString, cancellationToken: this.stoppingToken);

                        secondaryConfigurationValue.VarValue = newConfigurationValue.VarValue;
                    }

                    await this.secondaryDataContext.SaveChangesAsync(this.stoppingToken);
                }
                catch
                {
                    secondaryPartitionError = true;
                }
            }

            if (primaryPartitionError && secondaryPartitionError)
            {
                this.logger.LogCritical($"1:Exception: impossible writing {newConfigurationValue.VarName} in the primary and secondary partition - Exception Code: {DataLayerPersistentExceptionCode.PrimaryAndSecondaryPartitionFailure}");

                throw new DataLayerPersistentException(DataLayerPersistentExceptionCode.PrimaryAndSecondaryPartitionFailure);
            }

            if (primaryPartitionError && !secondaryPartitionError)
            {
                this.logger.LogCritical($"2:Exception: impossible writing {newConfigurationValue.VarName} in the primary partition - Exception Code: {DataLayerPersistentExceptionCode.PrimaryPartitionFailure}");

                throw new DataLayerPersistentException(DataLayerPersistentExceptionCode.PrimaryPartitionFailure);
            }

            if (!primaryPartitionError && secondaryPartitionError)
            {
                this.logger.LogCritical($"3:Exception: impossible writing {newConfigurationValue.VarName} in the secondary partition - Exception Code: {DataLayerPersistentExceptionCode.SecondaryPartitionFailure}");

                throw new DataLayerPersistentException(DataLayerPersistentExceptionCode.SecondaryPartitionFailure);
            }
        }

        #endregion
    }
}
