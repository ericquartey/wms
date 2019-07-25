using System;
using System.Linq;
using Ferretto.VW.MAS.DataLayer.DatabaseContext;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.EntityFrameworkCore;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.DataLayer
{
    internal class ServicingProvider : IServicingProvider
    {
        #region Fields

        private readonly DataLayerConfiguration dataLayerConfiguration;

        private readonly DbContextOptions<DataLayerContext> primaryContextOptions;

        #endregion

        #region Constructors

        public ServicingProvider(DataLayerConfiguration dataLayerConfiguration)
        {
            if (dataLayerConfiguration == null)
            {
                throw new ArgumentNullException(nameof(dataLayerConfiguration));
            }

            this.dataLayerConfiguration = dataLayerConfiguration;

            this.primaryContextOptions = new DbContextOptionsBuilder<DataLayerContext>().UseSqlite(this.dataLayerConfiguration.PrimaryConnectionString).Options;
        }

        #endregion

        #region Methods

        public ServicingInfo GetInfo()
        {
            using (var primaryDataContext = new DataLayerContext(this.primaryContextOptions))
            {
                return primaryDataContext.ServicingInfo.FirstOrDefault();
            }
        }

        #endregion
    }
}
