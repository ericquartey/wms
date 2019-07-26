using System;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using Ferretto.VW.MAS.DataLayer.DatabaseContext;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DataModels.Enumerations;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Exceptions;
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
            bool returnBoolValue;
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
                using (var primaryDataContext = new DataLayerContext(this.primaryContextOptions))
                {
                    primaryConfigurationValue = primaryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum && s.CategoryName == categoryValueEnum);
                }
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
        public DateTime GetDateTimeConfigurationValue(long configurationValueEnum, long categoryValueEnum)
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
                using (var primaryDataContext = new DataLayerContext(this.primaryContextOptions))
                {
                    primaryConfigurationValue = primaryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum && s.CategoryName == categoryValueEnum);
                }
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
        public decimal GetDecimalConfigurationValue(long configurationValueEnum, long categoryValueEnum)
        {
            decimal returnDecimalValue;
            ConfigurationValue primaryConfigurationValue;

            if (!this.CheckConfigurationDataType(configurationValueEnum, categoryValueEnum, ConfigurationDataType.Float))
            {
                this.logger.LogCritical($"1:Exception: get Decimal for {configurationValueEnum} variable - Exception Code: {DataLayerExceptionCode.DatatypeException}");
                throw new DataLayerException(DataLayerExceptionCode.DatatypeException);
            }

            try
            {
                using (var primaryDataContext = new DataLayerContext(this.primaryContextOptions))
                {
                    primaryConfigurationValue = primaryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum && s.CategoryName == categoryValueEnum);
                }
            }
            catch
            {
                this.logger.LogCritical($"2:Exception: Parse failed for {configurationValueEnum} in Primary partition - Error Code: {DataLayerPersistentExceptionCode.DataContextNotValid}", DataLayerPersistentExceptionCode.DataContextNotValid);

                throw new DataLayerPersistentException(DataLayerPersistentExceptionCode.DataContextNotValid);
            }

            if (primaryConfigurationValue != null)
            {
                if (!decimal.TryParse(primaryConfigurationValue.VarValue, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture.NumberFormat, out returnDecimalValue))
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
            int returnIntegerValue;
            ConfigurationValue primaryConfigurationValue;

            if (!this.CheckConfigurationDataType(configurationValueEnum, categoryValueEnum, ConfigurationDataType.Integer))
            {
                this.logger.LogCritical($"1:Exception: get Integer for {configurationValueEnum} variable - Exception Code: {DataLayerExceptionCode.DatatypeException}");
                throw new DataLayerException(DataLayerExceptionCode.DatatypeException);
            }

            try
            {
                using (var primaryDataContext = new DataLayerContext(this.primaryContextOptions))
                {
                    primaryConfigurationValue = primaryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum && s.CategoryName == categoryValueEnum);
                }
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
        public IPAddress GetIpAddressConfigurationValue(long configurationValueEnum, long categoryValueEnum)
        {
            IPAddress returnIpAddressValue;
            ConfigurationValue primaryConfigurationValue;

            if (!this.CheckConfigurationDataType(configurationValueEnum, categoryValueEnum, ConfigurationDataType.IPAddress))
            {
                this.logger.LogCritical($"1:Exception: get IP Address for {configurationValueEnum} variable - Exception Code: {DataLayerExceptionCode.DatatypeException}");
                throw new DataLayerException(DataLayerExceptionCode.DatatypeException);
            }

            try
            {
                using (var primaryDataContext = new DataLayerContext(this.primaryContextOptions))
                {
                    primaryConfigurationValue = primaryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum && s.CategoryName == categoryValueEnum);
                }
            }
            catch
            {
                this.logger.LogCritical($"2:Exception: Parse failed for {configurationValueEnum} in Primary partition - Error Code: {DataLayerPersistentExceptionCode.DataContextNotValid}", DataLayerPersistentExceptionCode.DataContextNotValid);

                throw new DataLayerPersistentException(DataLayerPersistentExceptionCode.DataContextNotValid);
            }

            if (primaryConfigurationValue != null)
            {
                returnIpAddressValue = IPAddress.Parse(primaryConfigurationValue.VarValue);
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
            string returnStringValue;
            ConfigurationValue primaryConfigurationValue;

            if (!this.CheckConfigurationDataType(configurationValueEnum, categoryValueEnum, ConfigurationDataType.String))
            {
                this.logger.LogCritical($"1:Exception: get string for {configurationValueEnum} variable - Exception Code: {DataLayerExceptionCode.DatatypeException}");
                throw new DataLayerException(DataLayerExceptionCode.DatatypeException);
            }
            try
            {
                using (var primaryDataContext = new DataLayerContext(this.primaryContextOptions))
                {
                    primaryConfigurationValue = primaryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum && s.CategoryName == categoryValueEnum);
                }
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

            var newConfigurationValue = new ConfigurationValue();
            newConfigurationValue.CategoryName = categoryValueEnum;
            newConfigurationValue.VarName = configurationValueEnum;
            newConfigurationValue.VarType = ConfigurationDataType.Boolean;
            newConfigurationValue.VarValue = value.ToString(CultureInfo.InvariantCulture);

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

            var newConfigurationValue = new ConfigurationValue();
            newConfigurationValue.CategoryName = categoryValueEnum;
            newConfigurationValue.VarName = configurationValueEnum;
            newConfigurationValue.VarType = ConfigurationDataType.Date;
            newConfigurationValue.VarValue = value.ToString(CultureInfo.InvariantCulture);

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

            var newConfigurationValue = new ConfigurationValue();
            newConfigurationValue.CategoryName = categoryValueEnum;
            newConfigurationValue.VarName = configurationValueEnum;
            newConfigurationValue.VarType = ConfigurationDataType.Float;
            newConfigurationValue.VarValue = value.ToString(CultureInfo.InvariantCulture);

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

            var newConfigurationValue = new ConfigurationValue();
            newConfigurationValue.CategoryName = categoryValueEnum;
            newConfigurationValue.VarName = configurationValueEnum;
            newConfigurationValue.VarType = ConfigurationDataType.Integer;
            newConfigurationValue.VarValue = value.ToString(CultureInfo.InvariantCulture);

            this.SetUpdateConfigurationValueCommon(newConfigurationValue);
        }

        public void SetIpAddressConfigurationValue(long configurationValueEnum, long categoryValueEnum, IPAddress value)
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

            var newConfigurationValue = new ConfigurationValue();
            newConfigurationValue.CategoryName = categoryValueEnum;
            newConfigurationValue.VarName = configurationValueEnum;
            newConfigurationValue.VarType = ConfigurationDataType.String;
            newConfigurationValue.VarValue = value;

            this.SetUpdateConfigurationValueCommon(newConfigurationValue);
        }

        private void SetUpdateConfigurationValueCommon(ConfigurationValue newConfigurationValue)
        {
            var primaryPartitionError = false;
            var secondaryPartitionError = false;

            Expression<Func<ConfigurationValue, bool>> queryString = s => s.VarName == newConfigurationValue.VarName && s.CategoryName == newConfigurationValue.CategoryName;

            using (var primaryDataContext = new DataLayerContext(this.primaryContextOptions))
            {
                var primaryConfigurationValue = primaryDataContext.ConfigurationValues.FirstOrDefault(queryString);

                try
                {
                    if (primaryConfigurationValue == null)
                    {
                        primaryDataContext.ConfigurationValues.Add(newConfigurationValue);
                    }
                    else
                    {
                        primaryConfigurationValue.VarValue = newConfigurationValue.VarValue;
                    }

                    primaryDataContext.SaveChanges();
                }
                catch
                {
                    primaryPartitionError = true;
                }
            }

            if (!this.suppressSecondary)
            {
                using (var secondaryDataContext = new DataLayerContext(this.secondaryContextOptions))
                {
                    var secondaryConfigurationValue = secondaryDataContext.ConfigurationValues.FirstOrDefault(queryString);

                    try
                    {
                        if (secondaryConfigurationValue == null)
                        {
                            secondaryDataContext.ConfigurationValues.Add(newConfigurationValue);
                        }
                        else
                        {
                            secondaryConfigurationValue.VarValue = newConfigurationValue.VarValue;
                        }

                        secondaryDataContext.SaveChanges();
                    }
                    catch
                    {
                        secondaryPartitionError = true;
                    }
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
