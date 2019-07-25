using System;
using System.Linq;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer
{
    internal class ServicingProvider : IServicingProvider
    {
        #region Fields

        private readonly DataLayerContext primaryDataContext;

        #endregion

        #region Constructors

        public ServicingProvider(DataLayerContext primaryDataContext)
        {
            if (primaryDataContext == null)
            {
                throw new ArgumentNullException(nameof(primaryDataContext));
            }

            this.primaryDataContext = primaryDataContext;
        }

        #endregion

        #region Methods

        public ServicingInfo GetInfo()
        {
            return this.primaryDataContext.ServicingInfo.FirstOrDefault();
        }

        #endregion
    }
}
