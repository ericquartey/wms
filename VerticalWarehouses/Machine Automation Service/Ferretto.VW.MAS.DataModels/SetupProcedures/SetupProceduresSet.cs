namespace Ferretto.VW.MAS.DataModels
{
    public sealed class SetupProceduresSet : DataModel
    {
        #region Properties

        public PositioningProcedure BayHeightCheck { get; set; }

        public RepeatedTestProcedure BeltBurnishingTest { get; set; }

        public SetupProcedure CellPanelsCheck { get; set; }

        public PositioningProcedure CellsHeightCheck { get; set; }

        public RepeatedTestProcedure ShutterTest { get; set; }

        public VerticalResolutionCalibrationProcedure VerticalResolutionCalibration { get; set; }

        #endregion
    }
}
