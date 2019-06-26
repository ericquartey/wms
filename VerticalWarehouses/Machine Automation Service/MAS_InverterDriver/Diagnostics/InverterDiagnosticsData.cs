// ReSharper disable ArrangeThisQualifier

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

        #endregion

        #region Constructors

        public InverterDiagnosticsData()
        {
            this.nanosecondsPerTick = (1000L * 1000L * 1000L) / Stopwatch.Frequency;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets average value in microseconds
        /// </summary>
        public float AverageValue => (this.averageValue * (float)this.nanosecondsPerTick) / 1000000.0f;

        /// <summary>
        /// Gets average value in microseconds
        /// </summary>
        public float MaxValue => (this.maxValue * (float)this.nanosecondsPerTick) / 1000000.0f;

        /// <summary>
        /// Gets average value in microseconds
        /// </summary>
        public float MinValue => (this.minValue * (float)this.nanosecondsPerTick) / 1000000.0f;

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
