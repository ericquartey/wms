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
        #region Properties

        private decimal HorizontalResolution { get; set; }

        private decimal VerticalResolution { get; set; }

        #endregion

        #region Methods

        public int MeterSUToPulsesConversion(decimal milliMeters, ConfigurationCategory configurationCategory)
        {
            //if (milliMeters == 0)
            //{
            //    var errorMessage = "Displacement to convert zero";
            //    this.Logger.LogDebug(errorMessage);
            //    throw new ArgumentException(errorMessage);
            //}

            decimal resolution;
            int pulses;

            switch (configurationCategory)
            {
                case ConfigurationCategory.VerticalAxis:
                    this.VerticalResolution = this.GetDecimalConfigurationValue((long)VerticalAxis.Resolution, ConfigurationCategory.VerticalAxis);
                    resolution = this.VerticalResolution;
                    break;

                case ConfigurationCategory.HorizontalAxis:
                    this.HorizontalResolution = this.GetDecimalConfigurationValue((long)HorizontalAxis.Resolution, ConfigurationCategory.HorizontalAxis);
                    resolution = this.HorizontalResolution;
                    break;

                default:
                    this.Logger.LogCritical($"Wrong selected axis {configurationCategory} to get resolution");

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
                this.Logger.LogDebug(errorMessage);
                throw new ArgumentException(errorMessage);
            }

            decimal resolution;
            decimal milliMeters;

            switch (configurationCategory)
            {
                case ConfigurationCategory.VerticalAxis:
                    this.VerticalResolution = this.GetDecimalConfigurationValue((long)VerticalAxis.Resolution, ConfigurationCategory.VerticalAxis);
                    resolution = this.VerticalResolution;
                    break;

                case ConfigurationCategory.HorizontalAxis:
                    this.HorizontalResolution = this.GetDecimalConfigurationValue((long)HorizontalAxis.Resolution, ConfigurationCategory.HorizontalAxis);
                    resolution = this.HorizontalResolution;
                    break;

                default:
                    var message = $"Wrong selected axis {configurationCategory} to get resolution";
                    this.Logger.LogDebug(message);

                    throw new DataLayerException(message, DataLayerExceptionCode.WrongAxisException, null);
            }

            try
            {
                milliMeters = pulses / resolution;
            }
            catch (DivideByZeroException ex)
            {
                var resolutionAxis = configurationCategory == ConfigurationCategory.VerticalAxis ? "Vertical" : "Horizontal";
                var errorMessage = $"Conversion impossible, resolution for {resolutionAxis} Axis zero";

                this.Logger.LogDebug(errorMessage);

                throw new DataLayerException(errorMessage, DataLayerExceptionCode.DivideByZeroException, ex);
            }

            return milliMeters;
        }

        #endregion
    }
}
