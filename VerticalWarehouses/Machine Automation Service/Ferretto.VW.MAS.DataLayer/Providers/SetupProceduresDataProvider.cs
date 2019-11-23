using System;
using System.Linq;
using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.DataLayer
{
    internal class SetupProceduresDataProvider : ISetupProceduresDataProvider
    {
        #region Fields

        private readonly DataLayerContext dataContext;

        private readonly ILogger<DataLayerContext> logger;

        #endregion

        #region Constructors

        public SetupProceduresDataProvider(
            DataLayerContext dataContext,
            ILogger<DataLayerContext> logger)
        {
            this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Methods

        public void Add(SetupProceduresSet setupProceduresSet)
        {
            if (setupProceduresSet is null)
            {
                throw new ArgumentNullException(nameof(setupProceduresSet));
            }

            lock (this.dataContext)
            {
                this.dataContext.SetupProceduresSets.Add(setupProceduresSet);
                this.dataContext.SaveChanges();
            }
        }

        public void ClearAll()
        {
            lock (this.dataContext)
            {
                this.dataContext.SetupProceduresSets.RemoveRange(this.dataContext.SetupProceduresSets);
                this.dataContext.SaveChanges();
            }
        }

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
                    .Include(s => s.LoadFirstDrawerTest)
                    .Include(s => s.ShutterHeightCheck)
                    .Include(s => s.ShutterManualMovements)
                    .Include(s => s.ShutterTest)
                    .Include(s => s.VerticalResolutionCalibration)
                    .Include(s => s.VerticalOffsetCalibration)
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

        public void Import(SetupProceduresSet setupProceduresSet)
        {
            lock (this.dataContext)
            {
                using (var transaction = this.dataContext.Database.BeginTransaction())
                {
                    try
                    {
                        this.dataContext.SetupProceduresSets.RemoveRange(this.dataContext.SetupProceduresSets);
                        this.dataContext.SetupProcedures.RemoveRange(this.dataContext.SetupProcedures);

                        this.dataContext.SetupProceduresSets.AddRange(setupProceduresSet);

                        this.dataContext.SaveChanges();

                        transaction.Commit();

                        this.logger.LogDebug($"SetupProceduresSet import");
                    }
                    catch (Exception e)
                    {
                        this.logger.LogError(e, $"SetupProceduresSet import exception");
                        transaction.Rollback();
                        throw;
                    }
                }
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
                this.dataContext.SetupProceduresSets.Attach(setupProceduresSet);
                this.dataContext.Entry(setupProceduresSet).State = EntityState.Modified;
                this.dataContext.SetupProceduresSets.Update(setupProceduresSet);

                this.dataContext.SetupProcedures.Attach(setupProceduresSet.BayHeightCheck);
                this.dataContext.Entry(setupProceduresSet.BayHeightCheck).State = EntityState.Modified;
                this.dataContext.SetupProcedures.Update(setupProceduresSet.BayHeightCheck);

                this.dataContext.SetupProcedures.Attach(setupProceduresSet.BeltBurnishingTest);
                this.dataContext.Entry(setupProceduresSet.BeltBurnishingTest).State = EntityState.Modified;
                this.dataContext.SetupProcedures.Update(setupProceduresSet.BeltBurnishingTest);

                this.dataContext.SetupProcedures.Attach(setupProceduresSet.CarouselManualMovements);
                this.dataContext.Entry(setupProceduresSet.CarouselManualMovements).State = EntityState.Modified;
                this.dataContext.SetupProcedures.Update(setupProceduresSet.CarouselManualMovements);

                this.dataContext.SetupProcedures.Attach(setupProceduresSet.CellPanelsCheck);
                this.dataContext.Entry(setupProceduresSet.CellPanelsCheck).State = EntityState.Modified;
                this.dataContext.SetupProcedures.Update(setupProceduresSet.CellPanelsCheck);

                this.dataContext.SetupProcedures.Attach(setupProceduresSet.CellsHeightCheck);
                this.dataContext.Entry(setupProceduresSet.CellPanelsCheck).State = EntityState.Modified;
                this.dataContext.SetupProcedures.Update(setupProceduresSet.CellsHeightCheck);

                this.dataContext.SetupProcedures.Attach(setupProceduresSet.DepositAndPickUpTest);
                this.dataContext.Entry(setupProceduresSet.DepositAndPickUpTest).State = EntityState.Modified;
                this.dataContext.SetupProcedures.Update(setupProceduresSet.DepositAndPickUpTest);

                this.dataContext.SetupProcedures.Attach(setupProceduresSet.LoadFirstDrawerTest);
                this.dataContext.Entry(setupProceduresSet.LoadFirstDrawerTest).State = EntityState.Modified;
                this.dataContext.SetupProcedures.Update(setupProceduresSet.LoadFirstDrawerTest);

                this.dataContext.SetupProcedures.Attach(setupProceduresSet.ShutterHeightCheck);
                this.dataContext.Entry(setupProceduresSet.ShutterHeightCheck).State = EntityState.Modified;
                this.dataContext.SetupProcedures.Update(setupProceduresSet.ShutterHeightCheck);

                this.dataContext.SetupProcedures.Attach(setupProceduresSet.ShutterManualMovements);
                this.dataContext.Entry(setupProceduresSet.ShutterManualMovements).State = EntityState.Modified;
                this.dataContext.SetupProcedures.Update(setupProceduresSet.ShutterManualMovements);

                this.dataContext.SetupProcedures.Attach(setupProceduresSet.ShutterTest);
                this.dataContext.Entry(setupProceduresSet.ShutterTest).State = EntityState.Modified;
                this.dataContext.SetupProcedures.Update(setupProceduresSet.ShutterTest);

                this.dataContext.SetupProcedures.Attach(setupProceduresSet.VerticalOffsetCalibration);
                this.dataContext.Entry(setupProceduresSet.VerticalOffsetCalibration).State = EntityState.Modified;
                this.dataContext.SetupProcedures.Update(setupProceduresSet.VerticalOffsetCalibration);

                this.dataContext.SetupProcedures.Attach(setupProceduresSet.VerticalResolutionCalibration);
                this.dataContext.Entry(setupProceduresSet.VerticalResolutionCalibration).State = EntityState.Modified;
                this.dataContext.SetupProcedures.Update(setupProceduresSet.VerticalResolutionCalibration);

                this.dataContext.SaveChanges();
            }
        }

        #endregion
    }
}
