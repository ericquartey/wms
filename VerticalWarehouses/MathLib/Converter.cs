using System;

namespace Ferretto.VW.MathLib
{
    public class Converter
    {
        decimal resolution;

        public decimal ManageResolution
        {
            set => this.resolution = value;
            get => this.resolution;
        }

        // Conversion from mm to pulse
        // Conversion from mm distance to pulse distance
        public int FromMMToPulse(decimal mmDistance)
        {
            decimal pulseDistanceDecimal;
            int pulseDistanceInt;
            decimal converted;

            pulseDistanceDecimal = mmDistance * resolution;
            converted = Math.Round(pulseDistanceDecimal, 0);

            pulseDistanceInt = (int)converted;

            return pulseDistanceInt;
        }

        // Conversion from mm/s speed to pulse/s speed
        public int FromMMSToPulseS(decimal mmsSpeed)
        {
            decimal pulseSpeedDecimal;
            int pulseSpeedInt;
            decimal converted;

            pulseSpeedDecimal = mmsSpeed * resolution;
            converted = Math.Round(pulseSpeedDecimal, 0);

            pulseSpeedInt = (int)converted;

            return pulseSpeedInt;
        }

        // Conversion from mm/s^2 acceleration to pulse/s^2 acceleration
        public int FromMMS2ToPulseS2(decimal mms2Acceleration)
        {
            decimal pulseAccelerationDecimal;
            int pulseAccelerationInt;
            decimal converted;

            pulseAccelerationDecimal = mms2Acceleration * resolution;
            converted = Math.Round(pulseAccelerationDecimal, 0);

            pulseAccelerationInt = (int)converted;

            return pulseAccelerationInt;
        }

        // Conversion from pulse to mm
        // Conversion from pulse distance to mm distance
        public decimal FromPulseToMM(int pulseDistance)
        {
            decimal mmDistanceDecimal;

            mmDistanceDecimal = pulseDistance / resolution;

            return Math.Round(mmDistanceDecimal, 4);
        }

        // Conversion from pulse/s speed to mm/s speed
        public decimal FromPulseSToMMS(int pulseSpeedS)
        {
            decimal mmSpeedDecimalS;

            mmSpeedDecimalS = pulseSpeedS / resolution;

            return Math.Round(mmSpeedDecimalS, 4);
        }

        // Conversion from pulse/s^2 acceleration to mm/s^2 acceleration
        public decimal FromPulseS2ToMMS2(int pulseSpeedS2)
        {
            decimal mmSpeedDecimalS2;

            mmSpeedDecimalS2 = pulseSpeedS2 / resolution;

            return Math.Round(mmSpeedDecimalS2, 4);
        }
    }
}
