using System;
using System.Linq;
using Ferretto.VW.MAS.DataModels;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.DataLayer
{
    internal sealed class ServicingProvider : IServicingProvider
    {
        #region Fields

        private readonly DataLayerContext dataContext;

        #endregion

        #region Constructors

        public ServicingProvider(DataLayerContext dataContext)
        {
            this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
        }

        #endregion

        #region Methods

        public ServicingInfo GetInfo()
        {
            lock (this.dataContext)
            {
                return this.dataContext.ServicingInfo.FirstOrDefault();
            }
        }

        #endregion
    }
}
