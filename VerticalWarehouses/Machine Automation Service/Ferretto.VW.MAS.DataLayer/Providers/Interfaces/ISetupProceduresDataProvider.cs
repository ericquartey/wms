﻿using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore.Storage;

namespace Ferretto.VW.MAS.DataLayer
{
    public interface ISetupProceduresDataProvider
    {
        #region Methods

        SetupProceduresSet GetAll();

        RepeatedTestProcedure GetBayCarouselCalibration(BayNumber bayNumber);

        SetupProcedure GetBayFirstLoadingUnit(BayNumber bayNumber);

        SetupProcedure GetBayHeightCheck(BayNumber bayNumber);

        SetupProcedure GetBayLaser(BayNumber bayNumber);

        BayProfileCheckProcedure GetBayProfileCheck(BayNumber bayNumber);

        RepeatedTestProcedure GetBayShutterTest(BayNumber bayNumber);

        RepeatedTestProcedure GetBeltBurnishingTest();

        PositioningProcedure GetCellPanelsCheck();

        PositioningProcedure GetCellsHeightCheck();

        RepeatedTestProcedure GetDepositAndPickUpTest();

        SetupProcedure GetLoadFirstDrawerTest();

        SetupProcedure GetShutterHeightCheck();

        OffsetCalibrationProcedure GetVerticalOffsetCalibration();

        SetupProcedure GetVerticalOriginCalibration();

        VerticalResolutionCalibrationProcedure GetVerticalResolutionCalibration();

        void Import(SetupProceduresSet setupProceduresSet, DataLayerContext context);

        RepeatedTestProcedure IncreasePerformedCycles(RepeatedTestProcedure depositAndPickUpTest);

        PositioningProcedure InProgressProcedure(PositioningProcedure procedure);

        SetupProcedure MarkAsCompleted(SetupProcedure procedure, bool bypassed = false);

        RepeatedTestProcedure ResetPerformedCycles(RepeatedTestProcedure procedure);

        void Update(SetupProceduresSet setupProceduresSet, DataLayerContext dataContext);

        #endregion
    }
}
