using System;
using System.Linq;

namespace Ferretto.VW.MAS.DataLayer
{
    public sealed class ElevatorWeightCheckProcedureProvider : IElevatorWeightCheckProcedureProvider
    {
        #region Fields

        private readonly DataLayerContext dataContext;

        #endregion

        #region Constructors

        public ElevatorWeightCheckProcedureProvider(DataLayerContext dataContext)
        {
            this.dataContext = dataContext ?? throw new System.ArgumentNullException(nameof(dataContext));
        }

        #endregion

        #region Methods

        public void Start(int loadingUnitId, double displacement, double weight)
        {
            var loadingUnit = this.dataContext.LoadingUnits.FirstOrDefault(l => l.Id == loadingUnitId);
            if (loadingUnit is null)
            {
                throw new EntityNotFoundException(loadingUnitId);
            }

            if (displacement < 0)
            {
                throw new ArgumentOutOfRangeException($"LoadingUnit {loadingUnitId}, displacement must be positive.");
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
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
