using System;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.MAS.DataLayer.DatabaseContext;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace Ferretto.VW.MAS.DataLayer.Providers
{
    public class ElevatorWeightCheckProvider : IElevatorProvider
    {
        #region Fields

        private readonly DataLayerContext dataContext;

        #endregion

        #region Constructors

        public ElevatorWeightCheckProvider(DataLayerContext dataContext)
        {
            if (dataContext == null)
            {
                throw new System.ArgumentNullException(nameof(dataContext));
            }

            this.dataContext = dataContext;
        }

        #endregion

        #region Methods

        public void Start(int id, decimal runToTest, decimal weight)
        {
            var loadingUnit = this.dataContext.LoadingUnits.FirstOrDefault(l => l.Id == id);
            if (loadingUnit is null)
            {
                throw new Exceptions.EntityNotFoundException(id);
            }

            if (runToTest < 0)
            {
                throw new ArgumentOutOfRangeException($"LoadingUnit {id}, runToTest must be positive.");
            }

            if (weight <= 0)
            {
                throw new ArgumentOutOfRangeException($"LoadingUnit {id}, weight must be greater than zero.");
            }

            if (loadingUnit.MaxNetWeight < (loadingUnit.Tare + weight))
            {
                throw new ArgumentOutOfRangeException($"LoadingUnit {id}, weight ({weight}) must be less than ({loadingUnit.MaxNetWeight - loadingUnit.Tare}).");
            }

            // TO DO execute operations.
            Task.Delay(5000);
        }

        #endregion
    }
}
