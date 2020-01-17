namespace Ferretto.VW.MAS.DataModels
{
    public sealed class SetupProceduresSet : DataModel
    {
        #region Properties

        public PositioningProcedure Bay1HeightCheck { get; set; }

        public RepeatedTestProcedure Bay1ShutterTest { get; set; }

        public PositioningProcedure Bay2HeightCheck { get; set; }

        public RepeatedTestProcedure Bay2ShutterTest { get; set; }

        public PositioningProcedure Bay3HeightCheck { get; set; }

        public RepeatedTestProcedure Bay3ShutterTest { get; set; }

        public RepeatedTestProcedure BeltBurnishingTest { get; set; }

        public PositioningProcedure CellPanelsCheck { get; set; }

        public PositioningProcedure CellsHeightCheck { get; set; }

        public RepeatedTestProcedure DepositAndPickUpTest { get; set; }

        public SetupProcedure LoadFirstDrawerTest { get; set; }

        public SetupProcedure ShutterHeightCheck { get; set; }

        public OffsetCalibrationProcedure VerticalOffsetCalibration { get; set; }

        public SetupProcedure VerticalOriginCalibration { get; set; }

        public VerticalResolutionCalibrationProcedure VerticalResolutionCalibration { get; set; }

        #endregion
    }
}
