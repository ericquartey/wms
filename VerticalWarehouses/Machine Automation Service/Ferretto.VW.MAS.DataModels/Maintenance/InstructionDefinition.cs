using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.DataModels
{
    public sealed class InstructionDefinition : DataModel
    {
        #region Properties

        public Axis Axis { get; set; }

        public BayNumber BayNumber { get; set; }

        public string CounterName { get; set; }

        public string Description { get; set; }

        public InstructionType InstructionType { get; set; }

        public bool IsCarousel { get; set; }

        public bool IsShutter { get; set; }

        public bool IsSystem { get; set; }

        public int? MaxDays { get; set; }

        public int? MaxRelativeCount { get; set; }

        public int? MaxTotalCount { get; set; }

        #endregion

        #region Methods

        public void SetCounterName(BayNumber bayNumber)
        {
            switch (bayNumber)
            {
                case BayNumber.BayOne:
                    this.CounterName = nameof(MachineStatistics.TotalLoadUnitsInBay1);
                    break;

                case BayNumber.BayTwo:
                    this.CounterName = nameof(MachineStatistics.TotalLoadUnitsInBay2);
                    break;

                case BayNumber.BayThree:
                    this.CounterName = nameof(MachineStatistics.TotalLoadUnitsInBay3);
                    break;
            }
        }

        #endregion
    }
}
