using System;
using System.Linq;
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
            lock (this.dataContext)
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
                    .Include(s => s.ShutterHeightCheck)
                    .Include(s => s.ShutterManualMovements)
                    .Include(s => s.ShutterTest)
                    .Include(s => s.VerticalManualMovements)
                    .Include(s => s.VerticalResolutionCalibration)
                    .Include(s => s.VerticalOffsetCalibration)
                    .Include(s => s.WeightCheck)
                    .Single();
            }
        }

        public PositioningProcedure GetBayHeightCheck()
        {
            lock (this.dataContext)
            {
                return this.dataContext.SetupProceduresSets
                    .Select(s => s.BayHeightCheck)
                    .Single();
            }
        }

        public RepeatedTestProcedure GetBeltBurnishingTest()
        {
            lock (this.dataContext)
            {
                return this.dataContext.SetupProceduresSets
                    .Select(s => s.BeltBurnishingTest)
                    .Single();
            }
        }

        public SetupProcedure GetCarouselManualMovements()
        {
            lock (this.dataContext)
            {
                return this.dataContext.SetupProceduresSets
                    .Select(s => s.CarouselManualMovements)
                    .Single();
            }
        }

        public PositioningProcedure GetCellPanelsCheck()
        {
            lock (this.dataContext)
            {
                return this.dataContext.SetupProceduresSets
                    .Select(s => s.CellPanelsCheck)
                    .Single();
            }
        }

        public PositioningProcedure GetCellsHeightCheck()
        {
            lock (this.dataContext)
            {
                return this.dataContext.SetupProceduresSets
                    .Select(s => s.CellsHeightCheck)
                    .Single();
            }
        }

        public RepeatedTestProcedure GetDepositAndPickUpTest()
        {
            lock (this.dataContext)
            {
                return this.dataContext.SetupProceduresSets
                    .Select(s => s.DepositAndPickUpTest)
                    .Single();
            }
        }

        public HorizontalManualMovementsProcedure GetHorizontalManualMovements()
        {
            lock (this.dataContext)
            {
                return this.dataContext.SetupProceduresSets
                    .Select(s => s.HorizontalManualMovements)
                    .Single();
            }
        }

        public SetupProcedure GetLoadFirstDrawerTest()
        {
            lock (this.dataContext)
            {
                return this.dataContext.SetupProceduresSets
                    .Select(s => s.LoadFirstDrawerTest)
                    .Single();
            }
        }

        public SetupProcedure GetShutterHeightCheck()
        {
            lock (this.dataContext)
            {
                return this.dataContext.SetupProceduresSets
                    .Select(s => s.ShutterHeightCheck)
                    .Single();
            }
        }

        public ShutterManualMovementsProcedure GetShutterManualMovements()
        {
            lock (this.dataContext)
            {
                return this.dataContext.SetupProceduresSets
                    .Select(s => s.ShutterManualMovements)
                    .Single();
            }
        }

        public RepeatedTestProcedure GetShutterTest()
        {
            lock (this.dataContext)
            {
                return this.dataContext.SetupProceduresSets
                    .Select(s => s.ShutterTest)
                    .Single();
            }
        }

        public VerticalManualMovementsProcedure GetVerticalManualMovements()
        {
            lock (this.dataContext)
            {
                return this.dataContext.SetupProceduresSets
                    .Select(s => s.VerticalManualMovements)
                    .Single();
            }
        }

        public OffsetCalibrationProcedure GetVerticalOffsetCalibration()
        {
            lock (this.dataContext)
            {
                return this.dataContext.SetupProceduresSets
                    .Select(s => s.VerticalOffsetCalibration)
                    .Single();
            }
        }

        public VerticalResolutionCalibrationProcedure GetVerticalResolutionCalibration()
        {
            lock (this.dataContext)
            {
                return this.dataContext.SetupProceduresSets
                    .Select(s => s.VerticalResolutionCalibration)
                    .Single();
            }
        }

        public SetupProcedure GetWeightCheck()
        {
            lock (this.dataContext)
            {
                return this.dataContext.SetupProceduresSets
                    .Select(s => s.WeightCheck)
                    .Single();
            }
        }

        public RepeatedTestProcedure IncreasePerformedCycles(RepeatedTestProcedure procedure)
        {
            lock (this.dataContext)
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
        }

        public SetupProcedure MarkAsCompleted(SetupProcedure procedure)
        {
            lock (this.dataContext)
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
        }

        public void Update(SetupProceduresSet setupProceduresSet)
        {
            if (setupProceduresSet is null)
            {
                throw new ArgumentNullException(nameof(setupProceduresSet));
            }

            lock (this.dataContext)
            {
                this.dataContext.SetupProceduresSets.Update(setupProceduresSet);
                this.dataContext.SaveChanges();
            }
        }

        #endregion
    }
}
