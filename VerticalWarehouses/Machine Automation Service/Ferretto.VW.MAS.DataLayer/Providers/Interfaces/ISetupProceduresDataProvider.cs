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

        SetupProcedure GetLoadFirstDrawerTest();

        SetupProcedure GetShutterHeightCheck();

        //ShutterManualMovementsProcedure GetShutterManualMovements();

        RepeatedTestProcedure GetShutterTest();

        OffsetCalibrationProcedure GetVerticalOffsetCalibration();

        VerticalResolutionCalibrationProcedure GetVerticalResolutionCalibration();

        void Import(SetupProceduresSet setupProceduresSet);

        RepeatedTestProcedure IncreasePerformedCycles(RepeatedTestProcedure depositAndPickUpTest);

        SetupProcedure MarkAsCompleted(SetupProcedure procedureParameters);

        void Update(SetupProceduresSet setupProceduresSet);

        #endregion
    }
}
