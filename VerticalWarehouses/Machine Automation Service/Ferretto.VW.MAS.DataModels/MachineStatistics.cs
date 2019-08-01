namespace Ferretto.VW.MAS.DataModels
{
    public sealed class MachineStatistics
    {
        public int Id { get; set; }

        public int TotalVerticalAxisCycles { get; set; }

        public double TotalVerticalAxisKilometers { get; set; }

        public int TotalBeltCycles { get; set; }

        public int TotalShutter1Cycles { get; set; }

        public int TotalShutter2Cycles { get; set; }

        public int TotalShutter3Cycles { get; set; }

        public int TotalMovedTraysInBay1 { get; set; }

        public int TotalMovedTraysInBay2 { get; set; }

        public int TotalMovedTraysInBay3 { get; set; }

        public int TotalMovedTrays { get; set; }

        public System.TimeSpan TotalPowerOnTime { get; set; }

        public System.TimeSpan TotalAutomaticTime { get; set; }

        public System.TimeSpan TotalMissionTime { get; set; }

        public double AutomaticTimePercentage =>
            this.TotalPowerOnTime.TotalHours > 0
            ? this.TotalAutomaticTime.TotalHours * 100 / this.TotalPowerOnTime.TotalHours
            : 0;

        public double? AreaFillPercentage { get; set; }

        public double WeightCapacityPercentage { get; set; }

        public double UsageTimePercentage => this.TotalAutomaticTime.TotalHours > 0
            ? this.TotalMissionTime.TotalHours * 100 / this.TotalAutomaticTime.TotalHours
            : 0;
    }
}
