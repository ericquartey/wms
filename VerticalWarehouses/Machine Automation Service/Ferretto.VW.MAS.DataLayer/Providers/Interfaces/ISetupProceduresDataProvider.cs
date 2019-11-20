using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer
{
    public interface ISetupProceduresDataProvider
    {
        #region Methods

        void Add(SetupProceduresSet setupProceduresSet);

        SetupProceduresSet GetAll();

        PositioningProcedure GetBayHeightCheck();

        RepeatedTestProcedure GetBeltBurnishingTest();

        SetupProcedure GetCarouselManualMovements();

        PositioningProcedure GetCellPanelsCheck();

        PositioningProcedure GetCellsHeightCheck();

        RepeatedTestProcedure GetDepositAndPickUpTest();

        HorizontalManualMovementsProcedure GetHorizontalManualMovements();

        SetupProcedure GetLoadFirstDrawerTest();

        SetupProcedure GetShutterHeightCheck();

        ShutterManualMovementsProcedure GetShutterManualMovements();

        RepeatedTestProcedure GetShutterTest();

        VerticalManualMovementsProcedure GetVerticalManualMovements();

        OffsetCalibrationProcedure GetVerticalOffsetCalibration();

        VerticalResolutionCalibrationProcedure GetVerticalResolutionCalibration();

        SetupProcedure GetWeightCheck();

        RepeatedTestProcedure IncreasePerformedCycles(RepeatedTestProcedure depositAndPickUpTest);

        SetupProcedure MarkAsCompleted(SetupProcedure procedureParameters);

        #endregion
    }
}
