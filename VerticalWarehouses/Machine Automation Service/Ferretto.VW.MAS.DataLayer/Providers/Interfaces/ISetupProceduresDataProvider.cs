using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer
{
    public interface ISetupProceduresDataProvider
    {
        #region Methods

        SetupProceduresSet GetAll();

        PositioningProcedure GetBayHeightCheck();

        RepeatedTestProcedure GetBeltBurnishingTest();

        PositioningProcedure GetCellPanelsCheck();

        PositioningProcedure GetCellsHeightCheck();

        RepeatedTestProcedure GetDepositAndPickUpTest();

        SetupProcedure GetLoadFirstDrawerTest();

        SetupProcedure GetShutterHeightCheck();

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
