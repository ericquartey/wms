namespace Ferretto.VW.MAS.DataModels
{
    public sealed class SetupProceduresSet : DataModel
    {
        #region Properties

        public PositioningProcedure BayHeightCheck { get; set; }

        public RepeatedTestProcedure BeltBurnishingTest { get; set; }

        public SetupProcedure CarouselManualMovements { get; set; }

        public SetupProcedure CellPanelsCheck { get; set; }

        public PositioningProcedure CellsHeightCheck { get; set; }

        public RepeatedTestProcedure DepositAndPickUpTest { get; set; }

        public HorizontalManualMovementsProcedure HorizontalManualMovements { get; set; }

        public SetupProcedure LoadFirstDrawerTest { get; set; }

        public OffsetCalibrationProcedure OffsetCalibration { get; set; }

        public SetupProcedure ShutterHeightCheck { get; set; }

        public ShutterManualMovementsProcedure ShutterManualMovements { get; set; }

        public RepeatedTestProcedure ShutterTest { get; set; }

        public VerticalManualMovementsProcedure VerticalManualMovements { get; set; }

        public VerticalResolutionCalibrationProcedure VerticalResolutionCalibration { get; set; }

        public SetupProcedure WeightCheck { get; set; }

        #endregion
    }
}
