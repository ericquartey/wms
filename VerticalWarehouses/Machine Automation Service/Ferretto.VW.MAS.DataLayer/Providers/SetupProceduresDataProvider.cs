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

        private static readonly Func<DataLayerContext, SetupProceduresSet> GetAllCompile =
            EF.CompileQuery((DataLayerContext context) =>
                context.SetupProceduresSets.AsNoTracking()

                    .Include(s => s.Bay1CarouselCalibration)
                    .Include(s => s.Bay1ExternalCalibration)
                    .Include(s => s.Bay1HeightCheck)
                    .Include(s => s.Bay1Laser)
                    .Include(s => s.Bay1ProfileCheck)
                    .Include(s => s.Bay1ShutterTest)
                    .Include(s => s.Bay1FullTest)

                    .Include(s => s.Bay2CarouselCalibration)
                    .Include(s => s.Bay2ExternalCalibration)
                    .Include(s => s.Bay2HeightCheck)
                    .Include(s => s.Bay2Laser)
                    .Include(s => s.Bay2ProfileCheck)
                    .Include(s => s.Bay2ShutterTest)
                    .Include(s => s.Bay2FullTest)

                    .Include(s => s.Bay3CarouselCalibration)
                    .Include(s => s.Bay3ExternalCalibration)
                    .Include(s => s.Bay3HeightCheck)
                    .Include(s => s.Bay3Laser)
                    .Include(s => s.Bay3ProfileCheck)
                    .Include(s => s.Bay3ShutterTest)
                    .Include(s => s.Bay3FullTest)

                    .Include(s => s.BeltBurnishingTest)
                    .Include(s => s.CellPanelsCheck)
                    .Include(s => s.CellsHeightCheck)
                    .Include(s => s.DepositAndPickUpTest)

                    .Include(s => s.LoadFirstDrawerTest)
                    .Include(s => s.ShutterHeightCheck)
                    .Include(s => s.VerticalOffsetCalibration)
                    .Include(s => s.VerticalOriginCalibration)
                    .Include(s => s.VerticalResolutionCalibration)

                    .Include(s => s.WeightMeasurement)

                    .Include(s => s.HorizontalChainCalibration)
                    .Include(s => s.HorizontalResolutionCalibration)

                    .Single());

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
                return GetAllCompile(this.dataContext);
            }
        }

        public RepeatedTestProcedure GetBayCarouselCalibration(BayNumber bayNumber)
        {
            lock (this.dataContext)
            {
                return this.dataContext.SetupProceduresSets.AsNoTracking()
                    .Select(s => bayNumber == BayNumber.BayOne ? s.Bay1CarouselCalibration : bayNumber == BayNumber.BayTwo ? s.Bay2CarouselCalibration : s.Bay3CarouselCalibration)
                    .Single();
            }
        }

        public RepeatedTestProcedure GetBayExternalCalibration(BayNumber bayNumber)
        {
            lock (this.dataContext)
            {
                return this.dataContext.SetupProceduresSets.AsNoTracking()
                    .Select(s => bayNumber == BayNumber.BayOne ? s.Bay1ExternalCalibration : bayNumber == BayNumber.BayTwo ? s.Bay2ExternalCalibration : s.Bay3ExternalCalibration)
                    .Single();
            }
        }

        public SetupProcedure GetBayHeightCheck(BayNumber bayNumber)
        {
            lock (this.dataContext)
            {
                return this.dataContext.SetupProceduresSets.AsNoTracking()
                    .Select(s => bayNumber == BayNumber.BayOne ? s.Bay1HeightCheck : bayNumber == BayNumber.BayTwo ? s.Bay2HeightCheck : s.Bay3HeightCheck)
                    .Single();
            }
        }

        public SetupProcedure GetBayLaser(BayNumber bayNumber)
        {
            lock (this.dataContext)
            {
                return this.dataContext.SetupProceduresSets.AsNoTracking()
                    .Select(s => bayNumber == BayNumber.BayOne ? s.Bay1Laser : bayNumber == BayNumber.BayTwo ? s.Bay2Laser : s.Bay3Laser)
                    .Single();
            }
        }

        public BayProfileCheckProcedure GetBayProfileCheck(BayNumber bayNumber)
        {
            lock (this.dataContext)
            {
                return this.dataContext.SetupProceduresSets.AsNoTracking()
                    .Select(s => bayNumber == BayNumber.BayOne ? s.Bay1ProfileCheck : bayNumber == BayNumber.BayTwo ? s.Bay2ProfileCheck : s.Bay3ProfileCheck)
                    .Single();
            }
        }

        public RepeatedTestProcedure GetBayShutterTest(BayNumber bayNumber)
        {
            lock (this.dataContext)
            {
                return this.dataContext.SetupProceduresSets.AsNoTracking()
                    .Select(s => bayNumber == BayNumber.BayOne ? s.Bay1ShutterTest : bayNumber == BayNumber.BayTwo ? s.Bay2ShutterTest : s.Bay3ShutterTest)
                    .Single();
            }
        }

        public RepeatedTestProcedure GetBeltBurnishingTest()
        {
            lock (this.dataContext)
            {
                return this.dataContext.SetupProceduresSets.AsNoTracking()
                    .Select(s => s.BeltBurnishingTest)
                    .Single();
            }
        }

        public PositioningProcedure GetCellPanelsCheck()
        {
            lock (this.dataContext)
            {
                return this.dataContext.SetupProceduresSets.AsNoTracking()
                    .Select(s => s.CellPanelsCheck)
                    .Single();
            }
        }

        public PositioningProcedure GetCellsHeightCheck()
        {
            lock (this.dataContext)
            {
                return this.dataContext.SetupProceduresSets.AsNoTracking()
                    .Select(s => s.CellsHeightCheck)
                    .Single();
            }
        }

        public RepeatedTestProcedure GetDepositAndPickUpTest()
        {
            lock (this.dataContext)
            {
                return this.dataContext.SetupProceduresSets.AsNoTracking()
                    .Select(s => s.DepositAndPickUpTest)
                    .Single();
            }
        }

        public RepeatedTestProcedure GetFullTest(BayNumber bayNumber)
        {
            lock (this.dataContext)
            {
                return this.dataContext.SetupProceduresSets.AsNoTracking()
                    .Select(s => bayNumber == BayNumber.BayOne ? s.Bay1FullTest : bayNumber == BayNumber.BayTwo ? s.Bay2FullTest : s.Bay3FullTest)
                    .Single();
            }
        }

        public SetupProcedure GetHorizontalChainCalibration()
        {
            lock (this.dataContext)
            {
                return this.dataContext.SetupProceduresSets.AsNoTracking()
                    .Select(s => s.HorizontalChainCalibration)
                    .Single();
            }
        }

        public SetupProcedure GetHorizontalResolutionCalibration()
        {
            lock (this.dataContext)
            {
                return this.dataContext.SetupProceduresSets.AsNoTracking()
                    .Select(s => s.HorizontalResolutionCalibration)
                    .Single();
            }
        }

        public PositioningProcedure GetLoadFirstDrawerTest()
        {
            lock (this.dataContext)
            {
                return this.dataContext.SetupProceduresSets.AsNoTracking()
                    .Select(s => s.LoadFirstDrawerTest)
                    .Single();
            }
        }

        public SetupProcedure GetShutterHeightCheck()
        {
            lock (this.dataContext)
            {
                return this.dataContext.SetupProceduresSets.AsNoTracking()
                    .Select(s => s.ShutterHeightCheck)
                    .Single();
            }
        }

        public OffsetCalibrationProcedure GetVerticalOffsetCalibration()
        {
            lock (this.dataContext)
            {
                return this.dataContext.SetupProceduresSets.AsNoTracking()
                    .Select(s => s.VerticalOffsetCalibration)
                    .Single();
            }
        }

        public SetupProcedure GetVerticalOriginCalibration()
        {
            lock (this.dataContext)
            {
                return this.dataContext.SetupProceduresSets.AsNoTracking()
                    .Select(s => s.VerticalOriginCalibration)
                    .Single();
            }
        }

        public VerticalResolutionCalibrationProcedure GetVerticalResolutionCalibration()
        {
            lock (this.dataContext)
            {
                return this.dataContext.SetupProceduresSets.AsNoTracking()
                    .Select(s => s.VerticalResolutionCalibration)
                    .Single();
            }
        }

        public void Import(SetupProceduresSet setupProceduresSet, DataLayerContext context)
        {
            _ = setupProceduresSet ?? throw new System.ArgumentNullException(nameof(setupProceduresSet));

            context.AddOrUpdate(setupProceduresSet, (e) => e.Id);

            context.AddOrUpdate(setupProceduresSet?.Bay1CarouselCalibration, (e) => e.Id);
            context.AddOrUpdate(setupProceduresSet?.Bay1ExternalCalibration, (e) => e.Id);
            context.AddOrUpdate(setupProceduresSet?.Bay1HeightCheck, (e) => e.Id);
            context.AddOrUpdate(setupProceduresSet?.Bay1Laser, (e) => e.Id);
            context.AddOrUpdate(setupProceduresSet?.Bay1ProfileCheck, (e) => e.Id);
            context.AddOrUpdate(setupProceduresSet?.Bay1ShutterTest, (e) => e.Id);
            context.AddOrUpdate(setupProceduresSet?.Bay1FullTest, (e) => e.Id);

            context.AddOrUpdate(setupProceduresSet?.Bay2CarouselCalibration, (e) => e.Id);
            context.AddOrUpdate(setupProceduresSet?.Bay2ExternalCalibration, (e) => e.Id);
            context.AddOrUpdate(setupProceduresSet?.Bay2HeightCheck, (e) => e.Id);
            context.AddOrUpdate(setupProceduresSet?.Bay2Laser, (e) => e.Id);
            context.AddOrUpdate(setupProceduresSet?.Bay2ProfileCheck, (e) => e.Id);
            context.AddOrUpdate(setupProceduresSet?.Bay2ShutterTest, (e) => e.Id);
            context.AddOrUpdate(setupProceduresSet?.Bay2FullTest, (e) => e.Id);

            context.AddOrUpdate(setupProceduresSet?.Bay3CarouselCalibration, (e) => e.Id);
            context.AddOrUpdate(setupProceduresSet?.Bay3ExternalCalibration, (e) => e.Id);
            context.AddOrUpdate(setupProceduresSet?.Bay3HeightCheck, (e) => e.Id);
            context.AddOrUpdate(setupProceduresSet?.Bay3Laser, (e) => e.Id);
            context.AddOrUpdate(setupProceduresSet?.Bay3ProfileCheck, (e) => e.Id);
            context.AddOrUpdate(setupProceduresSet?.Bay3ShutterTest, (e) => e.Id);
            context.AddOrUpdate(setupProceduresSet?.Bay3FullTest, (e) => e.Id);

            context.AddOrUpdate(setupProceduresSet?.BeltBurnishingTest, (e) => e.Id);
            context.AddOrUpdate(setupProceduresSet?.CellPanelsCheck, (e) => e.Id);
            context.AddOrUpdate(setupProceduresSet?.CellsHeightCheck, (e) => e.Id);
            context.AddOrUpdate(setupProceduresSet?.DepositAndPickUpTest, (e) => e.Id);

            context.AddOrUpdate(setupProceduresSet?.LoadFirstDrawerTest, (e) => e.Id);
            context.AddOrUpdate(setupProceduresSet?.ShutterHeightCheck, (e) => e.Id);
            context.AddOrUpdate(setupProceduresSet?.VerticalOffsetCalibration, (e) => e.Id);
            context.AddOrUpdate(setupProceduresSet?.VerticalOriginCalibration, (e) => e.Id);
            context.AddOrUpdate(setupProceduresSet?.VerticalResolutionCalibration, (e) => e.Id);

            context.AddOrUpdate(setupProceduresSet?.HorizontalChainCalibration, (e) => e.Id);
            context.AddOrUpdate(setupProceduresSet?.HorizontalResolutionCalibration, (e) => e.Id);
        }

        public RepeatedTestProcedure IncreasePerformedCycles(RepeatedTestProcedure procedure, int? requiredCycles = null)
        {
            lock (this.dataContext)
            {
                var existingProcedure = this.dataContext.SetupProcedures.SingleOrDefault(p => p.Id == procedure.Id);

                if (existingProcedure is RepeatedTestProcedure repeatedTestProcedure)
                {
                    repeatedTestProcedure.PerformedCycles++;
                    repeatedTestProcedure.IsCompleted = repeatedTestProcedure.PerformedCycles >= repeatedTestProcedure.RequiredCycles;
                    repeatedTestProcedure.InProgress = !repeatedTestProcedure.IsCompleted;
                    if (repeatedTestProcedure.IsCompleted)
                    {
                        repeatedTestProcedure.IsBypassed = false;
                    }
                    if (requiredCycles.HasValue)
                    {
                        repeatedTestProcedure.RequiredCycles = requiredCycles.Value;
                    }
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

        public SetupProcedure MarkAsCompleted(SetupProcedure procedure, bool bypassed = false)
        {
            lock (this.dataContext)
            {
                var existingProcedure = this.dataContext.SetupProcedures.SingleOrDefault(p => p.Id == procedure.Id);

                if (existingProcedure is null)
                {
                    throw new EntityNotFoundException(procedure.Id);
                }

                existingProcedure.IsCompleted = true;
                if (bypassed)
                {
                    existingProcedure.IsBypassed = true;
                }
                else if (!bypassed && existingProcedure.IsBypassed)
                {
                    existingProcedure.IsBypassed = false;
                }

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

            dataContext.AddOrUpdate(setupProceduresSet?.Bay1CarouselCalibration, (e) => e.Id);
            dataContext.AddOrUpdate(setupProceduresSet?.Bay1ExternalCalibration, (e) => e.Id);
            dataContext.AddOrUpdate(setupProceduresSet?.Bay1HeightCheck, (e) => e.Id);
            dataContext.AddOrUpdate(setupProceduresSet?.Bay1Laser, (e) => e.Id);
            dataContext.AddOrUpdate(setupProceduresSet?.Bay1ProfileCheck, (e) => e.Id);
            dataContext.AddOrUpdate(setupProceduresSet?.Bay1ShutterTest, (e) => e.Id);
            dataContext.AddOrUpdate(setupProceduresSet?.Bay1FullTest, (e) => e.Id);

            dataContext.AddOrUpdate(setupProceduresSet?.Bay2CarouselCalibration, (e) => e.Id);
            dataContext.AddOrUpdate(setupProceduresSet?.Bay2ExternalCalibration, (e) => e.Id);
            dataContext.AddOrUpdate(setupProceduresSet?.Bay2HeightCheck, (e) => e.Id);
            dataContext.AddOrUpdate(setupProceduresSet?.Bay2Laser, (e) => e.Id);
            dataContext.AddOrUpdate(setupProceduresSet?.Bay2ProfileCheck, (e) => e.Id);
            dataContext.AddOrUpdate(setupProceduresSet?.Bay2ShutterTest, (e) => e.Id);
            dataContext.AddOrUpdate(setupProceduresSet?.Bay2FullTest, (e) => e.Id);

            dataContext.AddOrUpdate(setupProceduresSet?.Bay3CarouselCalibration, (e) => e.Id);
            dataContext.AddOrUpdate(setupProceduresSet?.Bay3ExternalCalibration, (e) => e.Id);
            dataContext.AddOrUpdate(setupProceduresSet?.Bay3HeightCheck, (e) => e.Id);
            dataContext.AddOrUpdate(setupProceduresSet?.Bay3Laser, (e) => e.Id);
            dataContext.AddOrUpdate(setupProceduresSet?.Bay3ProfileCheck, (e) => e.Id);
            dataContext.AddOrUpdate(setupProceduresSet?.Bay3ShutterTest, (e) => e.Id);
            dataContext.AddOrUpdate(setupProceduresSet?.Bay3FullTest, (e) => e.Id);

            dataContext.AddOrUpdate(setupProceduresSet?.BeltBurnishingTest, (e) => e.Id);
            dataContext.AddOrUpdate(setupProceduresSet?.CellPanelsCheck, (e) => e.Id);
            dataContext.AddOrUpdate(setupProceduresSet?.CellsHeightCheck, (e) => e.Id);
            dataContext.AddOrUpdate(setupProceduresSet?.DepositAndPickUpTest, (e) => e.Id);

            dataContext.AddOrUpdate(setupProceduresSet?.LoadFirstDrawerTest, (e) => e.Id);
            dataContext.AddOrUpdate(setupProceduresSet?.ShutterHeightCheck, (e) => e.Id);
            dataContext.AddOrUpdate(setupProceduresSet?.VerticalOffsetCalibration, (e) => e.Id);
            dataContext.AddOrUpdate(setupProceduresSet?.VerticalOriginCalibration, (e) => e.Id);
            dataContext.AddOrUpdate(setupProceduresSet?.VerticalResolutionCalibration, (e) => e.Id);

            dataContext.AddOrUpdate(setupProceduresSet?.HorizontalChainCalibration, (e) => e.Id);
            dataContext.AddOrUpdate(setupProceduresSet?.HorizontalResolutionCalibration, (e) => e.Id);

            dataContext.SaveChanges();
        }

        #endregion
    }
}
