using System;
using System.Linq;
using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Ferretto.VW.MAS.DataLayer
{
    internal class SetupProceduresDataProvider : ISetupProceduresDataProvider
    {
        #region Fields

        private readonly DataLayerContext dataContext;

        private readonly ILogger<SetupProceduresDataProvider> logger;

        #endregion

        #region Constructors

        public SetupProceduresDataProvider(
            DataLayerContext dataContext,
            ILogger<SetupProceduresDataProvider> logger)
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

        public void Import(SetupProceduresSet setupProceduresSet, DataLayerContext context)
        {
            _ = setupProceduresSet ?? throw new System.ArgumentNullException(nameof(setupProceduresSet));

            //context.SetupProcedures.Remove(context.SetupProcedures.Find(setupProceduresSet?.BayHeightCheck?.Id));
            //context.SetupProcedures.Remove(context.SetupProcedures.Find(setupProceduresSet?.BeltBurnishingTest?.Id));
            //context.SetupProcedures.Remove(context.SetupProcedures.Find(setupProceduresSet?.CellPanelsCheck?.Id));
            //context.SetupProcedures.Remove(context.SetupProcedures.Find(setupProceduresSet?.CellsHeightCheck?.Id));
            //context.SetupProcedures.Remove(context.SetupProcedures.Find(setupProceduresSet?.DepositAndPickUpTest?.Id));
            //context.SetupProcedures.Remove(context.SetupProcedures.Find(setupProceduresSet?.LoadFirstDrawerTest?.Id));
            //context.SetupProcedures.Remove(context.SetupProcedures.Find(setupProceduresSet?.ShutterHeightCheck?.Id));
            //context.SetupProcedures.Remove(context.SetupProcedures.Find(setupProceduresSet?.ShutterTest?.Id));
            //context.SetupProcedures.Remove(context.SetupProcedures.Find(setupProceduresSet?.VerticalOffsetCalibration?.Id));
            //context.SetupProcedures.Remove(context.SetupProcedures.Find(setupProceduresSet?.VerticalResolutionCalibration?.Id));
            //context.SetupProceduresSets.Remove(setupProceduresSet);

            context.AddOrUpdate(setupProceduresSet, (e) => e.Id);
            context.AddOrUpdate(setupProceduresSet?.BayHeightCheck, (e) => e.Id);
            context.AddOrUpdate(setupProceduresSet?.BeltBurnishingTest, (e) => e.Id);
            context.AddOrUpdate(setupProceduresSet?.CellPanelsCheck, (e) => e.Id);
            context.AddOrUpdate(setupProceduresSet?.CellsHeightCheck, (e) => e.Id);
            context.AddOrUpdate(setupProceduresSet?.DepositAndPickUpTest, (e) => e.Id);
            context.AddOrUpdate(setupProceduresSet?.LoadFirstDrawerTest, (e) => e.Id);
            context.AddOrUpdate(setupProceduresSet?.ShutterHeightCheck, (e) => e.Id);
            context.AddOrUpdate(setupProceduresSet?.ShutterTest, (e) => e.Id);
            context.AddOrUpdate(setupProceduresSet?.VerticalOffsetCalibration, (e) => e.Id);
            context.AddOrUpdate(setupProceduresSet?.VerticalResolutionCalibration, (e) => e.Id);
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

        public void Update(SetupProceduresSet setupProceduresSet, DataLayerContext dataContext)
        {
            _ = setupProceduresSet ?? throw new System.ArgumentNullException(nameof(setupProceduresSet));

            dataContext ??= this.dataContext;

            dataContext.AddOrUpdate(setupProceduresSet, (e) => e.Id);
            dataContext.AddOrUpdate(setupProceduresSet?.BayHeightCheck, (e) => e.Id);
            dataContext.AddOrUpdate(setupProceduresSet?.BeltBurnishingTest, (e) => e.Id);
            dataContext.AddOrUpdate(setupProceduresSet?.CellPanelsCheck, (e) => e.Id);
            dataContext.AddOrUpdate(setupProceduresSet?.CellsHeightCheck, (e) => e.Id);
            dataContext.AddOrUpdate(setupProceduresSet?.DepositAndPickUpTest, (e) => e.Id);
            dataContext.AddOrUpdate(setupProceduresSet?.LoadFirstDrawerTest, (e) => e.Id);
            dataContext.AddOrUpdate(setupProceduresSet?.ShutterHeightCheck, (e) => e.Id);
            dataContext.AddOrUpdate(setupProceduresSet?.ShutterTest, (e) => e.Id);
            dataContext.AddOrUpdate(setupProceduresSet?.VerticalOffsetCalibration, (e) => e.Id);
            dataContext.AddOrUpdate(setupProceduresSet?.VerticalResolutionCalibration, (e) => e.Id);

            dataContext.SaveChanges();
        }

        #endregion
    }
}
