using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer
{
    public interface ISetupProceduresDataProvider
    {
        #region Methods

        SetupProceduresSet GetAll();

        PositioningProcedure GetBayHeightCheck();

        RepeatedTestProcedure GetBeltBurnishingTest();

        SetupProcedure GetCarouselManualMovements();

        PositioningProcedure GetCellPanelsCheck();

        PositioningProcedure GetCellsHeightCheck();

        RepeatedTestProcedure GetDepositAndPickUpTest();

        HorizontalManualMovementsProcedure GetHorizontalManualMovements();

        SetupProcedure GetLoadFirstDrawerTest();

        OffsetCalibrationProcedure GetOffsetCalibration();

        SetupProcedure GetShutterHeightCheck();

        ShutterManualMovementsProcedure GetShutterManualMovements();

        RepeatedTestProcedure GetShutterTest();

        VerticalManualMovementsProcedure GetVerticalManualMovements();

        VerticalResolutionCalibrationProcedure GetVerticalResolutionCalibration();

        SetupProcedure GetWeightCheck();

        RepeatedTestProcedure IncreasePerformedCycles(RepeatedTestProcedure depositAndPickUpTest);

        SetupProcedure MarkAsCompleted(SetupProcedure procedureParameters);

        void Update(SetupProceduresSet setupProceduresSet);

        #endregion
    }
}
