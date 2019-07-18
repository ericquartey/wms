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


    }
}
