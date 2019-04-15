using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ferretto.VW.MAS_DataLayer.Enumerations;
using Ferretto.VW.MAS_DataLayer.Interfaces;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS_DataLayer
{
    public partial class DataLayer : IDataLayerRuntimeValueManagment
    {
        #region Methods

        /// <inheritdoc/>
        public async Task<bool> GetBoolRuntimeValueAsync(long runtimeValueEnum, long categoryValueEnum)
        {
            this.logger.LogDebug("1:Method Start");

            var returnBoolValue = false;
            RuntimeValue primaryRuntimeValue;

            if (!this.CheckConfigurationDataType(runtimeValueEnum, categoryValueEnum, ConfigurationDataType.Boolean))
            {
                this.logger.LogCritical($"2:Exception: get Boolean for {runtimeValueEnum} variable - Exception Code: {DataLayerExceptionCode.DatatypeException}");
                throw new DataLayerException(DataLayerExceptionCode.DatatypeException);
            }

            try
            {
                primaryRuntimeValue =
                    await this.primaryDataContext.RuntimeValues.FirstOrDefaultAsync(s => s.VarName == runtimeValueEnum && s.CategoryName == categoryValueEnum, cancellationToken: this.stoppingToken);
            }
            catch
            {
                this.logger.LogCritical($"3:Exception: Parse failed for {runtimeValueEnum} in Primary partition - Error Code: {DataLayerPersistentExceptionCode.DataContextNotValid}", DataLayerPersistentExceptionCode.DataContextNotValid);
                throw new DataLayerPersistentException(DataLayerPersistentExceptionCode.DataContextNotValid);
            }

            if (primaryRuntimeValue != null)
            {
                if (!bool.TryParse(primaryRuntimeValue.VarValue, out returnBoolValue))
                {
                    this.logger.LogCritical($"4:Exception: Parse failed for {runtimeValueEnum} in Primary partition - Error Code: {DataLayerPersistentExceptionCode.ParseValue}", DataLayerPersistentExceptionCode.ParseValue);
                    throw new DataLayerPersistentException(DataLayerPersistentExceptionCode.ParseValue);
                }
            }
            else
            {
                this.logger.LogCritical($"5:Exception: value not found for {runtimeValueEnum} - Exception Code: {DataLayerPersistentExceptionCode.ValueNotFound}");

                throw new DataLayerPersistentException(DataLayerPersistentExceptionCode.ValueNotFound);
            }

            return returnBoolValue;
        }

        /// <inheritdoc/>
        public async Task<DateTime> GetDateTimeRuntimeValueAsync(long runtimeValueEnum, long categoryValueEnum)
        {
            this.logger.LogDebug("1:Method Start");

            DateTime returnDateTimeValue;
            RuntimeValue primaryRuntimeValue;

            if (!this.CheckConfigurationDataType(runtimeValueEnum, categoryValueEnum, ConfigurationDataType.Date))
            {
                this.logger.LogCritical($"2:Exception: get DateTime for {runtimeValueEnum} variable - Exception Code: {DataLayerExceptionCode.DatatypeException}");
                throw new DataLayerException(DataLayerExceptionCode.DatatypeException);
            }

            try
            {
                primaryRuntimeValue =
                    await this.primaryDataContext.RuntimeValues.FirstOrDefaultAsync(s => s.VarName == runtimeValueEnum && s.CategoryName == categoryValueEnum, cancellationToken: this.stoppingToken);
            }
            catch
            {
                this.logger.LogCritical($"3:Exception: Parse failed for {runtimeValueEnum} in Primary partition - Error Code: {DataLayerPersistentExceptionCode.DataContextNotValid}", DataLayerPersistentExceptionCode.DataContextNotValid);
                throw new DataLayerPersistentException(DataLayerPersistentExceptionCode.DataContextNotValid);
            }

            if (primaryRuntimeValue != null)
            {
                if (!DateTime.TryParse(primaryRuntimeValue.VarValue, out returnDateTimeValue))
                {
                    this.logger.LogCritical($"4:Exception: Parse failed for {runtimeValueEnum} in Primary partition - Error Code: {DataLayerPersistentExceptionCode.ParseValue}", DataLayerPersistentExceptionCode.ParseValue);
                    throw new DataLayerPersistentException(DataLayerPersistentExceptionCode.ParseValue);
                }
            }
            else
            {
                this.logger.LogCritical($"5:Exception: value not found for {runtimeValueEnum} - Exception Code: {DataLayerPersistentExceptionCode.ValueNotFound}");

                throw new DataLayerPersistentException(DataLayerPersistentExceptionCode.ValueNotFound);
            }

            return returnDateTimeValue;
        }

        /// <inheritdoc/>
        public async Task<decimal> GetDecimalRuntimeValueAsync(long runtimeValueEnum, long categoryValueEnum)
        {
            this.logger.LogDebug("1:Method Start");

            decimal returnDecimalValue = 0;
            RuntimeValue primaryRuntimeValue;

            if (!this.CheckConfigurationDataType(runtimeValueEnum, categoryValueEnum, ConfigurationDataType.Float))
            {
                this.logger.LogCritical($"2:Exception: get Decimal for {runtimeValueEnum} variable - Exception Code: {DataLayerExceptionCode.DatatypeException}");
                throw new DataLayerException(DataLayerExceptionCode.DatatypeException);
            }

            try
            {
                primaryRuntimeValue =
                    await this.primaryDataContext.RuntimeValues.FirstOrDefaultAsync(s => s.VarName == runtimeValueEnum && s.CategoryName == categoryValueEnum, cancellationToken: this.stoppingToken);
            }
            catch
            {
                this.logger.LogCritical($"3:Exception: Parse failed for {runtimeValueEnum} in Primary partition - Error Code: {DataLayerPersistentExceptionCode.DataContextNotValid}", DataLayerPersistentExceptionCode.DataContextNotValid);
                throw new DataLayerPersistentException(DataLayerPersistentExceptionCode.DataContextNotValid);
            }

            if (primaryRuntimeValue != null)
            {
                if (!decimal.TryParse(primaryRuntimeValue.VarValue, out returnDecimalValue))
                {
                    this.logger.LogCritical($"4:Exception: Parse failed for {runtimeValueEnum} in Primary partition - Error Code: {DataLayerPersistentExceptionCode.ParseValue}", DataLayerPersistentExceptionCode.ParseValue);
                    throw new DataLayerPersistentException(DataLayerPersistentExceptionCode.ParseValue);
                }
            }
            else
            {
                this.logger.LogCritical($"5:Exception: value not found for {runtimeValueEnum} - Exception Code: {DataLayerPersistentExceptionCode.ValueNotFound}");

                throw new DataLayerPersistentException(DataLayerPersistentExceptionCode.ValueNotFound);
            }

            return returnDecimalValue;
        }

        /// <inheritdoc/>
        public async Task<int> GetIntegerRuntimeValueAsync(long runtimeValueEnum, long categoryValueEnum)
        {
            this.logger.LogDebug("1:Method Start");

            var returnIntegerValue = 0;
            RuntimeValue primaryRuntimeValue;

            if (!this.CheckConfigurationDataType(runtimeValueEnum, categoryValueEnum, ConfigurationDataType.Integer))
            {
                this.logger.LogCritical($"2:Exception: get Intger for {runtimeValueEnum} variable - Exception Code: {DataLayerExceptionCode.DatatypeException}");
                throw new DataLayerException(DataLayerExceptionCode.DatatypeException);
            }

            try
            {
                primaryRuntimeValue =
                    await this.primaryDataContext.RuntimeValues.FirstOrDefaultAsync(s => s.VarName == runtimeValueEnum && s.CategoryName == categoryValueEnum, cancellationToken: this.stoppingToken);
            }
            catch
            {
                this.logger.LogCritical($"3:Exception: Parse failed for {runtimeValueEnum} in Primary partition - Error Code: {DataLayerPersistentExceptionCode.DataContextNotValid}", DataLayerPersistentExceptionCode.DataContextNotValid);
                throw new DataLayerPersistentException(DataLayerPersistentExceptionCode.DataContextNotValid);
            }

            if (primaryRuntimeValue != null)
            {
                if (!int.TryParse(primaryRuntimeValue.VarValue, out returnIntegerValue))
                {
                    this.logger.LogCritical($"4:Exception: Parse failed for {runtimeValueEnum} in Primary partition - Error Code: {DataLayerPersistentExceptionCode.ParseValue}", DataLayerPersistentExceptionCode.ParseValue);
                    throw new DataLayerPersistentException(DataLayerPersistentExceptionCode.ParseValue);
                }
            }
            else
            {
                this.logger.LogCritical($"5:Exception: value not found for {runtimeValueEnum} - Exception Code: {DataLayerPersistentExceptionCode.ValueNotFound}");

                throw new DataLayerPersistentException(DataLayerPersistentExceptionCode.ValueNotFound);
            }

            return returnIntegerValue;
        }

        /// <inheritdoc/>
        public async Task<string> GetStringRuntimeValueAsync(long runtimeValueEnum, long categoryValueEnum)
        {
            this.logger.LogDebug("1:Method Start");

            var returnStringValue = "";
            RuntimeValue primaryRuntimeValue;

            if (!this.CheckConfigurationDataType(runtimeValueEnum, categoryValueEnum, ConfigurationDataType.String))
            {
                this.logger.LogCritical($"2:Exception: get string for {runtimeValueEnum} variable - Exception Code: {DataLayerExceptionCode.DatatypeException}");
                throw new DataLayerException(DataLayerExceptionCode.DatatypeException);
            }
            try
            {
                primaryRuntimeValue =
                    await this.primaryDataContext.RuntimeValues.FirstOrDefaultAsync(s => s.VarName == runtimeValueEnum && s.CategoryName == categoryValueEnum, cancellationToken: this.stoppingToken);
            }
            catch
            {
                this.logger.LogCritical($"3:Exception: Parse failed for {runtimeValueEnum} in Primary partition - Error Code: {DataLayerPersistentExceptionCode.DataContextNotValid}", DataLayerPersistentExceptionCode.DataContextNotValid);

                throw new DataLayerPersistentException(DataLayerPersistentExceptionCode.DataContextNotValid);
            }

            if (primaryRuntimeValue != null)
            {
                returnStringValue = primaryRuntimeValue.VarValue;
            }
            else
            {
                this.logger.LogCritical($"4:Exception: value not found for {runtimeValueEnum} - Exception Code: {DataLayerPersistentExceptionCode.ValueNotFound}");

                throw new DataLayerPersistentException(DataLayerPersistentExceptionCode.ValueNotFound);
            }

            return returnStringValue;
        }

        /// <inheritdoc/>
        public async Task SetBoolRuntimeValueAsync(long runtimeValueEnum, long categoryValueEnum, bool value)
        {
            this.logger.LogDebug("1:Method Start");

            if (!this.CheckConfigurationDataType(runtimeValueEnum, categoryValueEnum, ConfigurationDataType.Boolean))
            {
                this.logger.LogCritical($"2:Exception: wrong datatype during set Boolean - Exception Code: {DataLayerExceptionCode.DatatypeException}");

                throw new DataLayerException(DataLayerExceptionCode.DatatypeException);
            }

            var newRuntimeValue = new RuntimeValue();
            newRuntimeValue.CategoryName = categoryValueEnum;
            newRuntimeValue.VarName = runtimeValueEnum;
            newRuntimeValue.VarType = ConfigurationDataType.Boolean;
            newRuntimeValue.VarValue = value.ToString();

            await this.SetRuntimeValueCommonAsync(newRuntimeValue, ConfigurationDataType.Boolean);
        }

        /// <inheritdoc/>
        public async Task SetDateTimeRuntimeValueAsync(long runtimeValueEnum, long categoryValueEnum, DateTime value)
        {
            this.logger.LogDebug("1:Method Start");

            if (!this.CheckConfigurationDataType(runtimeValueEnum, categoryValueEnum, ConfigurationDataType.Date))
            {
                this.logger.LogCritical($"2:Exception: wrong datatype during set Date - Exception Code: {DataLayerExceptionCode.DatatypeException}");

                throw new DataLayerException(DataLayerExceptionCode.DatatypeException);
            }

            var newRuntimeValue = new RuntimeValue();
            newRuntimeValue.CategoryName = categoryValueEnum;
            newRuntimeValue.VarName = runtimeValueEnum;
            newRuntimeValue.VarType = ConfigurationDataType.Date;
            newRuntimeValue.VarValue = value.ToString();

            await this.SetRuntimeValueCommonAsync(newRuntimeValue, ConfigurationDataType.Date);
        }

        /// <inheritdoc/>
        public async Task SetDecimalRuntimeValueAsync(long runtimeValueEnum, long categoryValueEnum, decimal value)
        {
            this.logger.LogDebug("1:Method Start");

            if (!this.CheckConfigurationDataType(runtimeValueEnum, categoryValueEnum, ConfigurationDataType.Float))
            {
                this.logger.LogCritical($"2:Exception: wrong datatype during set Decimal - Exception Code: {DataLayerExceptionCode.DatatypeException}");

                throw new DataLayerException(DataLayerExceptionCode.DatatypeException);
            }

            var newRuntimeValue = new RuntimeValue();
            newRuntimeValue.CategoryName = categoryValueEnum;
            newRuntimeValue.VarName = runtimeValueEnum;
            newRuntimeValue.VarType = ConfigurationDataType.Float;
            newRuntimeValue.VarValue = value.ToString();

            await this.SetRuntimeValueCommonAsync(newRuntimeValue, ConfigurationDataType.Float);
        }

        /// <inheritdoc/>
        public async Task SetIntegerRuntimeValueAsync(long runtimeValueEnum, long categoryValueEnum, int value)
        {
            this.logger.LogDebug("1:Method Start");

            if (!this.CheckConfigurationDataType(runtimeValueEnum, categoryValueEnum, ConfigurationDataType.Integer))
            {
                this.logger.LogCritical($"2:Exception: wrong datatype during set Integer - Exception Code: {DataLayerExceptionCode.DatatypeException}");

                throw new DataLayerException(DataLayerExceptionCode.DatatypeException);
            }

            var newRuntimeValue = new RuntimeValue();
            newRuntimeValue.CategoryName = categoryValueEnum;
            newRuntimeValue.VarName = runtimeValueEnum;
            newRuntimeValue.VarType = ConfigurationDataType.Integer;
            newRuntimeValue.VarValue = value.ToString();

            await this.SetRuntimeValueCommonAsync(newRuntimeValue, ConfigurationDataType.Integer);
        }

        /// <inheritdoc/>
        public async Task SetStringRuntimeValueAsync(long runtimeValueEnum, long categoryValueEnum, string value)
        {
            this.logger.LogDebug("1:Method Start");

            if (!this.CheckConfigurationDataType(runtimeValueEnum, categoryValueEnum, ConfigurationDataType.String))
            {
                this.logger.LogCritical($"2:Exception: wrong datatype during set String - Exception Code: {DataLayerExceptionCode.DatatypeException}");

                throw new DataLayerException(DataLayerExceptionCode.DatatypeException);
            }

            var newRuntimeValue = new RuntimeValue();
            newRuntimeValue.CategoryName = categoryValueEnum;
            newRuntimeValue.VarName = runtimeValueEnum;
            newRuntimeValue.VarType = ConfigurationDataType.String;
            newRuntimeValue.VarValue = value.ToString();

            await this.SetRuntimeValueCommonAsync(newRuntimeValue, ConfigurationDataType.String);
        }

        private async Task SetRuntimeValueCommonAsync(RuntimeValue newRuntimeValue, ConfigurationDataType configurationDataType)
        {
            var primaryPartitionError = false;
            var secondaryPartitionError = false;

            this.logger.LogDebug("1:Method Start");

            Expression<Func<RuntimeValue, bool>> queryString = s => s.VarName == newRuntimeValue.VarName && s.CategoryName == newRuntimeValue.CategoryName;
            var primaryRuntimeValue = await this.primaryDataContext.RuntimeValues.FirstOrDefaultAsync(queryString, cancellationToken: this.stoppingToken);

            try
            {
                if (primaryRuntimeValue == null)
                {
                    this.primaryDataContext.RuntimeValues.Add(newRuntimeValue);
                }
                else
                {
                    primaryRuntimeValue.VarValue = newRuntimeValue.VarValue;
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
                    if (primaryRuntimeValue == null)
                    {
                        this.secondaryDataContext.RuntimeValues.Add(newRuntimeValue);
                    }
                    else
                    {
                        var secondaryRuntimeValue = await this.secondaryDataContext.RuntimeValues.FirstOrDefaultAsync(queryString, cancellationToken: this.stoppingToken);
                        secondaryRuntimeValue.VarValue = newRuntimeValue.VarValue;
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
                this.logger.LogCritical($"2:Exception: impossible writing {newRuntimeValue.VarName} in primary and secondary partition - Exception Code: {DataLayerPersistentExceptionCode.PrimaryAndSecondaryPartitionFailure}");

                throw new DataLayerPersistentException(DataLayerPersistentExceptionCode.PrimaryAndSecondaryPartitionFailure);
            }

            if (primaryPartitionError && !secondaryPartitionError)
            {
                this.logger.LogCritical($"3:Exception: impossible writing {newRuntimeValue.VarName} in primary partition - Exception Code: {DataLayerPersistentExceptionCode.PrimaryPartitionFailure}");

                throw new DataLayerPersistentException(DataLayerPersistentExceptionCode.PrimaryPartitionFailure);
            }

            if (!primaryPartitionError && secondaryPartitionError)
            {
                this.logger.LogCritical($"4:Exception: impossible writing {newRuntimeValue.VarName} in primary and secondary partition - Exception Code: {DataLayerPersistentExceptionCode.SecondaryPartitionFailure}");

                throw new DataLayerPersistentException(DataLayerPersistentExceptionCode.SecondaryPartitionFailure);
            }

            this.logger.LogDebug("5:Method End");
        }

        #endregion
    }
}
