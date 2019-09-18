using System;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.MAS.DataLayer.DatabaseContext;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;

namespace Ferretto.VW.MAS.DataLayer.Providers
{
    public class ElevatorWeightCheckProcedureProvider : IElevatorWeightCheckProcedureProvider
    {
        #region Fields

        private readonly DataLayerContext dataContext;

        #endregion

        #region Constructors

        public ElevatorWeightCheckProcedureProvider(DataLayerContext dataContext)
        {
            if (dataContext == null)
            {
                throw new System.ArgumentNullException(nameof(dataContext));
            }

            this.dataContext = dataContext;
        }

        #endregion

        #region Methods

        public void Start(int loadingUnitId, decimal runToTest, decimal weight)
        {
            var loadingUnit = this.dataContext.LoadingUnits.FirstOrDefault(l => l.Id == loadingUnitId);
            if (loadingUnit is null)
            {
                throw new Exceptions.EntityNotFoundException(loadingUnitId);
            }

            if (runToTest < 0)
            {
                throw new ArgumentOutOfRangeException($"LoadingUnit {loadingUnitId}, runToTest must be positive.");
            }

            if (weight <= 0)
            {
                throw new ArgumentOutOfRangeException($"LoadingUnit {loadingUnitId}, weight must be greater than zero.");
            }

            if (loadingUnit.MaxNetWeight < (loadingUnit.Tare + weight))
            {
                throw new ArgumentOutOfRangeException($"LoadingUnit {loadingUnitId}, weight ({weight}) must be less than ({loadingUnit.MaxNetWeight - loadingUnit.Tare}).");
            }

            // TO DO execute operations.
            Task.Delay(5000);
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
