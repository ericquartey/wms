using System;
using System.Linq;
using Ferretto.VW.MAS.DataLayer.DatabaseContext;
using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.VW.MAS.DataLayer
{
    internal class SetupProceduresDataProvider : ISetupProceduresDataProvider
    {
        #region Fields

        private readonly DataLayerContext dataContext;

        #endregion

        #region Constructors

        public SetupProceduresDataProvider(DataLayerContext dataContext)
        {
            this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
        }

        #endregion

        #region Methods

        public SetupProceduresSet GetAll()
        {
            return this.dataContext.SetupProceduresSets
                .Include(s => s.BayHeightCheck)
                .Include(s => s.BeltBurnishingTest)
                .Include(s => s.CarouselManualMovements)
                .Include(s => s.CellPanelsCheck)
                .Include(s => s.CellsHeightCheck)
                .Include(s => s.DepositAndPickUpTest)
                .Include(s => s.HorizontalManualMovements)
                .Include(s => s.LoadFirstDrawerTest)
                .Include(s => s.OffsetCalibration)
                .Include(s => s.ShutterHeightCheck)
                .Include(s => s.ShutterManualMovements)
                .Include(s => s.ShutterTest)
                .Include(s => s.VerticalManualMovements)
                .Include(s => s.VerticalResolutionCalibration)
                .Include(s => s.WeightCheck)
                .Single();
        }

        public PositioningProcedure GetBayHeightCheck()
        {
            return this.dataContext.SetupProceduresSets
                .Select(s => s.BayHeightCheck)
                .Single();
        }

        public RepeatedTestProcedure GetBeltBurnishingTest()
        {
            return this.dataContext.SetupProceduresSets
                .Select(s => s.BeltBurnishingTest)
                .Single();
        }

        public SetupProcedure GetCarouselManualMovements()
        {
            return this.dataContext.SetupProceduresSets
                .Select(s => s.CarouselManualMovements)
                .Single();
        }

        public PositioningProcedure GetCellPanelsCheck()
        {
            return this.dataContext.SetupProceduresSets
                .Select(s => s.CellPanelsCheck)
                .Single();
        }

        public PositioningProcedure GetCellsHeightCheck()
        {
            return this.dataContext.SetupProceduresSets
                .Select(s => s.CellsHeightCheck)
                .Single();
        }

        public RepeatedTestProcedure GetDepositAndPickUpTest()
        {
            return this.dataContext.SetupProceduresSets
                .Select(s => s.DepositAndPickUpTest)
                .Single();
        }

        public HorizontalManualMovementsProcedure GetHorizontalManualMovements()
        {
            return this.dataContext.SetupProceduresSets
                .Select(s => s.HorizontalManualMovements)
                .Single();
        }

        public SetupProcedure GetLoadFirstDrawerTest()
        {
            return this.dataContext.SetupProceduresSets
                .Select(s => s.LoadFirstDrawerTest)
                .Single();
        }

        public OffsetCalibrationProcedure GetOffsetCalibration()
        {
            return this.dataContext.SetupProceduresSets
                .Select(s => s.OffsetCalibration)
                .Single();
        }

        public SetupProcedure GetShutterHeightCheck()
        {
            return this.dataContext.SetupProceduresSets
                .Select(s => s.ShutterHeightCheck)
                .Single();
        }

        public ShutterManualMovementsProcedure GetShutterManualMovements()
        {
            return this.dataContext.SetupProceduresSets
                .Select(s => s.ShutterManualMovements)
                .Single();
        }

        public RepeatedTestProcedure GetShutterTest()
        {
            return this.dataContext.SetupProceduresSets
                .Select(s => s.ShutterTest)
                .Single();
        }

        public VerticalManualMovementsProcedure GetVerticalManualMovements()
        {
            return this.dataContext.SetupProceduresSets
                .Select(s => s.VerticalManualMovements)
                .Single();
        }

        public VerticalResolutionCalibrationProcedure GetVerticalResolutionCalibration()
        {
            return this.dataContext.SetupProceduresSets
                .Select(s => s.VerticalResolutionCalibration)
                .Single();
        }

        public SetupProcedure GetWeightCheck()
        {
            return this.dataContext.SetupProceduresSets
                .Select(s => s.WeightCheck)
                .Single();
        }

        public RepeatedTestProcedure IncreasePerformedCycles(RepeatedTestProcedure procedure)
        {
            var existingProcedure = this.dataContext.SetupProcedures.SingleOrDefault(p => p.Id == procedure.Id);

            if (existingProcedure is RepeatedTestProcedure repeatedTestProcedure)
            {
                repeatedTestProcedure.PerformedCycles++;
                repeatedTestProcedure.IsCompleted = repeatedTestProcedure.PerformedCycles >= repeatedTestProcedure.RequiredCycles;

                this.dataContext.SetupProcedures.Update(repeatedTestProcedure);
                this.dataContext.SaveChanges();

                return repeatedTestProcedure;
            }
            else
            {
                throw new EntityNotFoundException(procedure.Id);
            }
        }

        public SetupProcedure MarkAsCompleted(SetupProcedure procedure)
        {
            var existingProcedure = this.dataContext.SetupProcedures.SingleOrDefault(p => p.Id == procedure.Id);

            if (existingProcedure is null)
            {
                throw new EntityNotFoundException(procedure.Id);
            }

            existingProcedure.IsCompleted = true;

            this.dataContext.SetupProcedures.Update(existingProcedure);
            this.dataContext.SaveChanges();

            return existingProcedure;
        }

        #endregion
    }
}
