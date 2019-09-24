using System.Linq;
using Ferretto.VW.MAS.DataLayer.DatabaseContext;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Exceptions;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : ICellManagmentDataLayer
    {
        #region Methods

        //// INFO The method returns to the machine manager the position to take a drawer for mission from the WMS
        public LoadingUnitPosition GetLoadingUnitPosition(int cellId)
        {
            using (var scope = this.serviceScopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<DataLayerContext>();

                var cell = dbContext.Cells.FirstOrDefault(s => s.Id == cellId);
                if (cell == null)
                {
                    throw new DataLayerException(DataLayerExceptionCode.CellNotFoundException);
                }

                return new LoadingUnitPosition
                {
                    LoadingUnitCoord = cell.Position,
                    LoadingUnitSide = cell.Panel.Side,
                };
            }
        }

        #endregion
    }
}
