using System;
using System.Linq;
using Ferretto.VW.MAS.DataModels;


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

        public void SetInstallationDate()
        {
            lock (this.dataContext)
            {
                var s = new ServicingInfo();
                s.InstallationDate = DateTime.Now;
                this.dataContext.ServicingInfo.Add(s);
                this.dataContext.SaveChanges();
            }
        }

        #endregion
    }
}
