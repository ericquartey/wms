using System;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels.Enumerations;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Exceptions;
using Microsoft.Extensions.Logging;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : IResolutionConversionDataLayer
    {
        #region Methods

        public int MeterSUToPulsesConversion(decimal milliMeters, ConfigurationCategory configurationCategory)
        {
            if (milliMeters == 0)
            {
                var errorMessage = "Displacement to convert zero";
                this.logger.LogDebug(errorMessage);
                throw new ArgumentException(errorMessage);
            }

            decimal resolution;
            int pulses;

            switch (configurationCategory)
            {
                case ConfigurationCategory.VerticalAxis:
                    resolution = this.GetDecimalConfigurationValue((long)VerticalAxis.Resolution, (long)ConfigurationCategory.VerticalAxis);
                    break;

                case ConfigurationCategory.HorizontalAxis:
                    resolution = this.GetDecimalConfigurationValue((long)HorizontalAxis.Resolution, (long)ConfigurationCategory.HorizontalAxis);
                    break;

                default:
                    this.logger.LogCritical($"Wrong selected axis {configurationCategory} to get resolution");

                    throw new DataLayerException(DataLayerExceptionCode.WrongAxisException);
            }

            pulses = decimal.ToInt32(resolution * milliMeters);

            return pulses;
        }

        public decimal PulsesToMeterSUConversion(int pulses, ConfigurationCategory configurationCategory)
        {
            if (pulses == 0)
            {
                var errorMessage = "Zero pulses to convert";
                this.logger.LogDebug(errorMessage);
                throw new ArgumentException(errorMessage);
            }

            decimal resolution;
            decimal milliMeters;

            switch (configurationCategory)
            {
                case ConfigurationCategory.VerticalAxis:
                    resolution = this.GetDecimalConfigurationValue((long)VerticalAxis.Resolution, (long)ConfigurationCategory.VerticalAxis);
                    break;

                case ConfigurationCategory.HorizontalAxis:
                    resolution = this.GetDecimalConfigurationValue((long)HorizontalAxis.Resolution, (long)ConfigurationCategory.HorizontalAxis);
                    break;

                default:
                    this.logger.LogDebug($"Wrong selected axis {configurationCategory} to get resolution");

                    throw new DataLayerException(DataLayerExceptionCode.WrongAxisException);
            }

            try
            {
                milliMeters = pulses / resolution;
            }
            catch (DivideByZeroException ex)
            {
                var resolutionAxis = configurationCategory == ConfigurationCategory.VerticalAxis ? "Vertical" : "Horizontal";
                var errorMessage = $"Conversion impossible, resolution for {resolutionAxis} Axis zero";

                this.logger.LogDebug(errorMessage);

                throw new DataLayerException(errorMessage, DataLayerExceptionCode.DivideByZeroException, ex);
            }

            return milliMeters;
        }

        #endregion
    }
}
