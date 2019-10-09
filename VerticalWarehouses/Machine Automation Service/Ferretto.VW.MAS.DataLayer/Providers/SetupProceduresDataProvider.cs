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
