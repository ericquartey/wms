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
                .Include(s => s.CellPanelsCheck)
                .Include(s => s.CellsHeightCheck)
                .Include(s => s.ShutterTest)
                .Include(s => s.VerticalResolutionCalibration)
                .Single();
        }

        #endregion
    }
}
