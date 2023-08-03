using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer
{
    public interface ISetupProceduresDataProvider
    {
        #region Methods

        SetupProceduresSet GetAll();

        RepeatedTestProcedure GetBayCarouselCalibration(BayNumber bayNumber);

        RepeatedTestProcedure GetBayExternalCalibration(BayNumber bayNumber);

        SetupProcedure GetBayHeightCheck(BayNumber bayNumber);

        SetupProcedure GetBayLaser(BayNumber bayNumber);

        BayProfileCheckProcedure GetBayProfileCheck(BayNumber bayNumber);

        RepeatedTestProcedure GetBayShutterTest(BayNumber bayNumber);

        RepeatedTestProcedure GetBeltBurnishingTest();

        PositioningProcedure GetCellPanelsCheck();

        PositioningProcedure GetCellsHeightCheck();

        RepeatedTestProcedure GetDepositAndPickUpTest();

        RepeatedTestProcedure GetFullTest(BayNumber bayNumber);

        SetupProcedure GetHorizontalChainCalibration();

        RepeatedTestProcedure GetHorizontalResolutionCalibration();

        PositioningProcedure GetLoadFirstDrawerTest();

        SetupProcedure GetShutterHeightCheck();

        OffsetCalibrationProcedure GetVerticalOffsetCalibration();

        SetupProcedure GetVerticalOriginCalibration();

        VerticalResolutionCalibrationProcedure GetVerticalResolutionCalibration();

        SetupProcedure GetWeightMeasurement();

        void Import(SetupProceduresSet setupProceduresSet, DataLayerContext context);

        RepeatedTestProcedure IncreasePerformedCycles(RepeatedTestProcedure depositAndPickUpTest, int? requiredCycles = null);

        PositioningProcedure InProgressProcedure(PositioningProcedure procedure);

        SetupProcedure MarkAsCompleted(SetupProcedure procedure, bool bypassed = false);

        RepeatedTestProcedure ResetPerformedCycles(RepeatedTestProcedure procedure);

        void SetBayShutterRequiredCycles(BayNumber bayNumber, int value);

        void Update(SetupProceduresSet setupProceduresSet, DataLayerContext dataContext);

        #endregion
    }
}
