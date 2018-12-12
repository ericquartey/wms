using System;

namespace Ferretto.VW.MathLib
{
    public class Converter
    {
        decimal resolution;

        public Converter()
        {
            this.resolution = 1024;
        }

        public Converter(decimal resolution)
        {
            this.resolution = resolution;
        }

        // Conversion from mm to pulse
        // Conversion from mm distance to pulse distance
        public long FromMMToPulse(decimal mmDistance)
        {
            decimal pulseDistanceDecimal;
            long  pulseDistanceLong;
            decimal converted;

            pulseDistanceDecimal = mmDistance * resolution;
            converted = Math.Round(pulseDistanceDecimal, 0);

            pulseDistanceLong = (long)converted;

            return pulseDistanceLong;
        }

        // Conversion from mm/s speed to pulse/s speed
        public long FromMMSToPulseS(decimal mmsSpeed)
        {
            decimal pulseSpeedDecimal;
            long pulseSpeedLong;
            decimal converted;

            pulseSpeedDecimal = mmsSpeed * resolution;
            converted = Math.Round(pulseSpeedDecimal, 0);

            pulseSpeedLong = (long)converted;

            return pulseSpeedLong;
        }

        // Conversion from mm/s^2 acceleration to pulse/s^2 acceleration
        public long FromMMS2ToPulseS2(decimal mms2Acceleration)
        {
            decimal pulseAccelerationDecimal;
            long pulseAccelerationLong;
            decimal converted;

            pulseAccelerationDecimal = mms2Acceleration * resolution;
            converted = Math.Round(pulseAccelerationDecimal, 0);

            pulseAccelerationLong = (long)converted;

            return pulseAccelerationLong;
        }

        // Conversion from pulse to mm
        // Conversion from pulse distance to mm distance
        public decimal FromPulseToMM(long pulseDistance)
        {
            decimal mmDistanceDecimal;

            mmDistanceDecimal = pulseDistance / resolution;

            return Math.Round(mmDistanceDecimal, 4);
        }

        // Conversion from pulse/s speed to mm/s speed
        public decimal FromPulseSToMMS(long pulseSpeedS)
        {
            decimal mmSpeedDecimalS;

            mmSpeedDecimalS = pulseSpeedS / resolution;

            return Math.Round(mmSpeedDecimalS, 4);
        }

        // Conversion from pulse/s^2 acceleration to mm/s^2 acceleration
        public decimal FromPulseS2ToMMS2(long pulseSpeedS2)
        {
            decimal mmSpeedDecimalS2;

            mmSpeedDecimalS2 = pulseSpeedS2 / resolution;

            return Math.Round(mmSpeedDecimalS2, 4);
        }
    }
}
