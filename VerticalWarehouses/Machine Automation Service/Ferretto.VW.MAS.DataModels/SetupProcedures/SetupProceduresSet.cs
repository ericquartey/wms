namespace Ferretto.VW.MAS.DataModels
{
    public sealed class SetupProceduresSet : DataModel
    {
        #region Properties

        public PositioningProcedure BayHeightCheck { get; set; }

        public RepeatedTestProcedure BeltBurnishingTest { get; set; }

        public PositioningProcedure CellPanelsCheck { get; set; }

        public PositioningProcedure CellsHeightCheck { get; set; }

        public RepeatedTestProcedure DepositAndPickUpTest { get; set; }

        public SetupProcedure LoadFirstDrawerTest { get; set; }

        public SetupProcedure ShutterHeightCheck { get; set; }

        public RepeatedTestProcedure ShutterTest { get; set; }

        public OffsetCalibrationProcedure VerticalOffsetCalibration { get; set; }

        public SetupProcedure VerticalOriginCalibration { get; set; }

        public VerticalResolutionCalibrationProcedure VerticalResolutionCalibration { get; set; }

        #endregion
    }
}
