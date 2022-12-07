using System.Collections.Generic;

namespace Ferretto.VW.MAS.DataModels
{
    public sealed class MachineStatistics : DataModel
    {
        #region Properties

        public double? AreaFillPercentage { get; set; }

        public double AutomaticTimePercentage =>
            this.TotalPowerOnTime.TotalHours > 0
            ? this.TotalAutomaticTime.TotalHours * 100 / this.TotalPowerOnTime.TotalHours
            : 0;

        public List<InverterStatistics> InverterStatistics { get; set; }

        public System.TimeSpan TotalAutomaticTime { get; set; }

        public double TotalBayChainKilometers1 { get; set; }

        public double TotalBayChainKilometers2 { get; set; }

        public double TotalBayChainKilometers3 { get; set; }

        public int TotalHorizontalAxisCycles { get; set; }

        public double TotalHorizontalAxisKilometers { get; set; }

        public double TotalInverterMissionTime { get; set; }

        public double TotalInverterPowerOnTime { get; set; }

        public int TotalLoadUnitsInBay1 { get; set; }

        public int TotalLoadUnitsInBay2 { get; set; }

        public int TotalLoadUnitsInBay3 { get; set; }

        public int TotalMissions => this.TotalLoadUnitsInBay1 + this.TotalLoadUnitsInBay2 + this.TotalLoadUnitsInBay3;

        public System.TimeSpan TotalMissionTime { get; set; }

        public System.TimeSpan TotalPowerOnTime { get; set; }

        public int TotalVerticalAxisCycles { get; set; }

        public double TotalVerticalAxisKilometers { get; set; }

        public double TotalWeightBack { get; set; }

        public double TotalWeightFront { get; set; }

        public double UsageTimePercentage => this.TotalAutomaticTime.TotalHours > 0
                            ? this.TotalMissionTime.TotalHours * 100 / this.TotalAutomaticTime.TotalHours
            : 0;

        public double WeightCapacityPercentage { get; set; }

        #endregion
    }
}
