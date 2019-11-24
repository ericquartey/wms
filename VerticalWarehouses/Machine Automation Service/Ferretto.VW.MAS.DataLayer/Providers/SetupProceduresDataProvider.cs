using System;
using System.Linq;
using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

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

        public SetupProceduresSet GetAll()
        {
            lock (this.dataContext)
            {
                return this.dataContext.SetupProceduresSets
                    .Include(s => s.BayHeightCheck)
                    .Include(s => s.BeltBurnishingTest)
                    .Include(s => s.CellPanelsCheck)
                    .Include(s => s.CellsHeightCheck)
                    .Include(s => s.DepositAndPickUpTest)
                    .Include(s => s.LoadFirstDrawerTest)
                    .Include(s => s.ShutterHeightCheck)
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
                try
                {
                    this.dataContext.SetupProcedures.Remove(this.dataContext.SetupProcedures.Find(setupProceduresSet?.BayHeightCheck?.Id));
                    this.dataContext.SetupProcedures.Remove(this.dataContext.SetupProcedures.Find(setupProceduresSet?.BeltBurnishingTest?.Id));
                    this.dataContext.SetupProcedures.Remove(this.dataContext.SetupProcedures.Find(setupProceduresSet?.CellPanelsCheck?.Id));
                    this.dataContext.SetupProcedures.Remove(this.dataContext.SetupProcedures.Find(setupProceduresSet?.CellsHeightCheck?.Id));
                    this.dataContext.SetupProcedures.Remove(this.dataContext.SetupProcedures.Find(setupProceduresSet?.DepositAndPickUpTest?.Id));
                    this.dataContext.SetupProcedures.Remove(this.dataContext.SetupProcedures.Find(setupProceduresSet?.LoadFirstDrawerTest?.Id));
                    this.dataContext.SetupProcedures.Remove(this.dataContext.SetupProcedures.Find(setupProceduresSet?.ShutterHeightCheck?.Id));
                    this.dataContext.SetupProcedures.Remove(this.dataContext.SetupProcedures.Find(setupProceduresSet?.ShutterTest?.Id));
                    this.dataContext.SetupProcedures.Remove(this.dataContext.SetupProcedures.Find(setupProceduresSet?.VerticalOffsetCalibration?.Id));
                    this.dataContext.SetupProcedures.Remove(this.dataContext.SetupProcedures.Find(setupProceduresSet?.VerticalResolutionCalibration?.Id));

                    this.dataContext.SetupProceduresSets.Remove(setupProceduresSet);

                    this.dataContext.SaveChanges();

                    this.Update(setupProceduresSet);

                    this.logger.LogDebug($"SetupProceduresSet import");
                }
                catch (Exception e)
                {
                    this.logger.LogError(e, $"SetupProceduresSet import exception");
                    throw;
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
                this.dataContext.AddOrUpdate(setupProceduresSet, (e) => e.Id);
                this.dataContext.AddOrUpdate(setupProceduresSet?.BayHeightCheck, (e) => e.Id);
                this.dataContext.AddOrUpdate(setupProceduresSet?.BeltBurnishingTest, (e) => e.Id);
                this.dataContext.AddOrUpdate(setupProceduresSet?.CellPanelsCheck, (e) => e.Id);
                this.dataContext.AddOrUpdate(setupProceduresSet?.CellsHeightCheck, (e) => e.Id);
                this.dataContext.AddOrUpdate(setupProceduresSet?.DepositAndPickUpTest, (e) => e.Id);
                this.dataContext.AddOrUpdate(setupProceduresSet?.LoadFirstDrawerTest, (e) => e.Id);
                this.dataContext.AddOrUpdate(setupProceduresSet?.ShutterHeightCheck, (e) => e.Id);
                this.dataContext.AddOrUpdate(setupProceduresSet?.ShutterTest, (e) => e.Id);
                this.dataContext.AddOrUpdate(setupProceduresSet?.VerticalOffsetCalibration, (e) => e.Id);
                this.dataContext.AddOrUpdate(setupProceduresSet?.VerticalResolutionCalibration, (e) => e.Id);

                this.dataContext.SaveChanges();
            }
        }

        #endregion
    }
}
