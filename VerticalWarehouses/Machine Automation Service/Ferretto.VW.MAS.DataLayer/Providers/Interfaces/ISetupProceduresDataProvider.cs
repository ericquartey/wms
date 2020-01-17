using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore.Storage;

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

        SetupProcedure GetVerticalOriginCalibration();

        VerticalResolutionCalibrationProcedure GetVerticalResolutionCalibration();

        void Import(SetupProceduresSet setupProceduresSet, DataLayerContext context);

        RepeatedTestProcedure IncreasePerformedCycles(RepeatedTestProcedure depositAndPickUpTest);

        SetupProcedure MarkAsCompleted(SetupProcedure procedureParameters);

        RepeatedTestProcedure ResetPerformedCycles(RepeatedTestProcedure procedure);

        void Update(SetupProceduresSet setupProceduresSet, DataLayerContext dataContext);

        #endregion
    }
}
