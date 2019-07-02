// ReSharper disable ArrangeThisQualifier

using System;
using System.Diagnostics;

namespace Ferretto.VW.MAS_InverterDriver.Diagnostics
{
    public class InverterDiagnosticsData
    {
        #region Fields

        private readonly long nanosecondsPerTick;

        private long averageValue;

        private long maxValue;

        private long minValue;

        private long totalValues;

        private double totalValuesSquared;

        #endregion

        #region Constructors

        public InverterDiagnosticsData()
        {
            this.nanosecondsPerTick = (1000L * 1000L * 1000L) / Stopwatch.Frequency;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets average value in milliseconds
        /// </summary>
        public double AverageValue => (this.averageValue * (double)this.nanosecondsPerTick) / 1000000.0d;

        /// <summary>
        /// Gets average value in milliseconds
        /// </summary>
        public double MaxValue => (this.maxValue * (double)this.nanosecondsPerTick) / 1000000.0d;

        /// <summary>
        /// Gets average value in milliseconds
        /// </summary>
        public double MinValue => (this.minValue * (double)this.nanosecondsPerTick) / 1000000.0d;

        /// <summary>
        /// Gets Standard Deviation value in milliseconds
        /// </summary>
        public double StandardDeviation => Math.Sqrt((this.totalValuesSquared - (this.TotalSamples * Math.Pow(this.averageValue, 2))) / this.TotalSamples) / 1000000.0d;

        public long TotalSamples { get; private set; }

        #endregion

        #region Methods

        public void AddValue(long newValue)
        {
            if (newValue < this.minValue || this.minValue == 0)
            {
                this.minValue = newValue;
            }

            if (newValue > this.maxValue)
            {
                this.maxValue = newValue;
            }

            this.totalValues += newValue;
            this.totalValuesSquared += Math.Pow(newValue, 2);
            this.TotalSamples++;

            this.averageValue = this.totalValues / this.TotalSamples;
        }

        public void ResetDiagnostics()
        {
            this.minValue = 0;
            this.maxValue = 0;
            this.averageValue = 0;
            this.TotalSamples = 0;
        }

        #endregion
    }
}
