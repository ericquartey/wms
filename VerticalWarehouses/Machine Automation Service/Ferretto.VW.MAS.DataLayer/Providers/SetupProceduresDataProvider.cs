using System;
using System.Linq;
using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage;
using Ferretto.VW.CommonUtils.Messages.Enumerations;

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
                    .Include(s => s.Bay1ProfileCheck)
                    .Include(s => s.Bay2ProfileCheck)
                    .Include(s => s.Bay3ProfileCheck)
                    .Include(s => s.BeltBurnishingTest)
                    .Include(s => s.CellPanelsCheck)
                    .Include(s => s.CellsHeightCheck)
                    .Include(s => s.DepositAndPickUpTest)
                    .Include(s => s.LoadFirstDrawerTest)
                    .Include(s => s.ShutterHeightCheck)
                    .Include(s => s.Bay1ShutterTest)
                    .Include(s => s.Bay2ShutterTest)
                    .Include(s => s.Bay3ShutterTest)
                    .Include(s => s.Bay1CarouselCalibration)
                    .Include(s => s.Bay2CarouselCalibration)
                    .Include(s => s.Bay3CarouselCalibration)
                    .Include(s => s.VerticalResolutionCalibration)
                    .Include(s => s.VerticalOffsetCalibration)
                    .Include(s => s.VerticalOriginCalibration)
                    .Single();
            }
        }

        public RepeatedTestProcedure GetBayCarouselCalibration(BayNumber bayNumber)
        {
            lock (this.dataContext)
            {
                return this.dataContext.SetupProceduresSets
                    .Select(s => bayNumber == BayNumber.BayOne ? s.Bay1CarouselCalibration : bayNumber == BayNumber.BayTwo ? s.Bay2CarouselCalibration : s.Bay3CarouselCalibration)
                    .Single();
            }
        }

        public BayProfileCheckProcedure GetBayProfileCheck(BayNumber bayNumber)
        {
            lock (this.dataContext)
            {
                return this.dataContext.SetupProceduresSets
                    .Select(s => bayNumber == BayNumber.BayOne ? s.Bay1ProfileCheck : bayNumber == BayNumber.BayTwo ? s.Bay2ProfileCheck : s.Bay3ProfileCheck)
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

        public RepeatedTestProcedure GetShutterTest(BayNumber bayNumber)
        {
            lock (this.dataContext)
            {
                return this.dataContext.SetupProceduresSets
                    .Select(s => bayNumber == BayNumber.BayOne ? s.Bay1ShutterTest : bayNumber == BayNumber.BayTwo ? s.Bay2ShutterTest : s.Bay3ShutterTest)
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

        public SetupProcedure GetVerticalOriginCalibration()
        {
            lock (this.dataContext)
            {
                return this.dataContext.SetupProceduresSets
                    .Select(s => s.VerticalOriginCalibration)
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

            //context.SetupProcedures.Remove(context.SetupProcedures.Find(setupProceduresSet?.Bay1ProfileCheck?.Id));
            //context.SetupProcedures.Remove(context.SetupProcedures.Find(setupProceduresSet?.Bay2ProfileCheck?.Id));
            //context.SetupProcedures.Remove(context.SetupProcedures.Find(setupProceduresSet?.Bay3ProfileCheck?.Id));
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
            context.AddOrUpdate(setupProceduresSet?.Bay1ProfileCheck, (e) => e.Id);
            context.AddOrUpdate(setupProceduresSet?.Bay2ProfileCheck, (e) => e.Id);
            context.AddOrUpdate(setupProceduresSet?.Bay3ProfileCheck, (e) => e.Id);
            context.AddOrUpdate(setupProceduresSet?.BeltBurnishingTest, (e) => e.Id);
            context.AddOrUpdate(setupProceduresSet?.CellPanelsCheck, (e) => e.Id);
            context.AddOrUpdate(setupProceduresSet?.CellsHeightCheck, (e) => e.Id);
            context.AddOrUpdate(setupProceduresSet?.DepositAndPickUpTest, (e) => e.Id);
            context.AddOrUpdate(setupProceduresSet?.LoadFirstDrawerTest, (e) => e.Id);
            context.AddOrUpdate(setupProceduresSet?.ShutterHeightCheck, (e) => e.Id);
            context.AddOrUpdate(setupProceduresSet?.Bay1ShutterTest, (e) => e.Id);
            context.AddOrUpdate(setupProceduresSet?.Bay2ShutterTest, (e) => e.Id);
            context.AddOrUpdate(setupProceduresSet?.Bay3ShutterTest, (e) => e.Id);
            context.AddOrUpdate(setupProceduresSet?.VerticalOffsetCalibration, (e) => e.Id);
            context.AddOrUpdate(setupProceduresSet?.VerticalResolutionCalibration, (e) => e.Id);
            context.AddOrUpdate(setupProceduresSet?.VerticalOriginCalibration, (e) => e.Id);
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
                    repeatedTestProcedure.InProgress = !repeatedTestProcedure.IsCompleted;

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

        public PositioningProcedure InProgressProcedure(PositioningProcedure procedure)
        {
            lock (this.dataContext)
            {
                var existingProcedure = this.dataContext.SetupProcedures.SingleOrDefault(p => p.Id == procedure.Id);

                if (existingProcedure is PositioningProcedure positioningProcedure)
                {
                    positioningProcedure.InProgress = true;

                    this.dataContext.SetupProcedures.Update(positioningProcedure);
                    this.dataContext.SaveChanges();

                    return positioningProcedure;
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

                if (existingProcedure is RepeatedTestProcedure repeatedTestProcedure)
                {
                    repeatedTestProcedure.InProgress = false;
                }

                if (existingProcedure is PositioningProcedure positioningProcedure)
                {
                    positioningProcedure.InProgress = false;
                }

                this.dataContext.SetupProcedures.Update(existingProcedure);
                this.dataContext.SaveChanges();

                return existingProcedure;
            }
        }

        public RepeatedTestProcedure ResetPerformedCycles(RepeatedTestProcedure procedure)
        {
            lock (this.dataContext)
            {
                var existingProcedure = this.dataContext.SetupProcedures.SingleOrDefault(p => p.Id == procedure.Id);

                if (existingProcedure is RepeatedTestProcedure repeatedTestProcedure)
                {
                    repeatedTestProcedure.PerformedCycles = 0;

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

        public void Update(SetupProceduresSet setupProceduresSet, DataLayerContext dataContext)
        {
            _ = setupProceduresSet ?? throw new System.ArgumentNullException(nameof(setupProceduresSet));

            if (dataContext is null)
            {
                dataContext = this.dataContext;
            }

            dataContext.AddOrUpdate(setupProceduresSet, (e) => e.Id);
            dataContext.AddOrUpdate(setupProceduresSet?.Bay1ProfileCheck, (e) => e.Id);
            dataContext.AddOrUpdate(setupProceduresSet?.Bay2ProfileCheck, (e) => e.Id);
            dataContext.AddOrUpdate(setupProceduresSet?.Bay3ProfileCheck, (e) => e.Id);
            dataContext.AddOrUpdate(setupProceduresSet?.BeltBurnishingTest, (e) => e.Id);
            dataContext.AddOrUpdate(setupProceduresSet?.CellPanelsCheck, (e) => e.Id);
            dataContext.AddOrUpdate(setupProceduresSet?.CellsHeightCheck, (e) => e.Id);
            dataContext.AddOrUpdate(setupProceduresSet?.DepositAndPickUpTest, (e) => e.Id);
            dataContext.AddOrUpdate(setupProceduresSet?.LoadFirstDrawerTest, (e) => e.Id);
            dataContext.AddOrUpdate(setupProceduresSet?.ShutterHeightCheck, (e) => e.Id);
            dataContext.AddOrUpdate(setupProceduresSet?.Bay1ShutterTest, (e) => e.Id);
            dataContext.AddOrUpdate(setupProceduresSet?.Bay2ShutterTest, (e) => e.Id);
            dataContext.AddOrUpdate(setupProceduresSet?.Bay3ShutterTest, (e) => e.Id);
            dataContext.AddOrUpdate(setupProceduresSet?.VerticalOffsetCalibration, (e) => e.Id);
            dataContext.AddOrUpdate(setupProceduresSet?.VerticalResolutionCalibration, (e) => e.Id);
            dataContext.AddOrUpdate(setupProceduresSet?.VerticalOriginCalibration, (e) => e.Id);

            dataContext.SaveChanges();
        }

        #endregion
    }
}
