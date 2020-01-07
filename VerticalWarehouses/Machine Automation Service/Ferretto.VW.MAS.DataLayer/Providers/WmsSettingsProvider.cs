using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ferretto.VW.MAS.DataLayer.Providers
{
    internal class WmsSettingsProvider : IWmsSettingsProvider
    {
        #region Fields

        private readonly DataLayerContext dataContext;

        #endregion

        #region Constructors

        public WmsSettingsProvider(DataLayerContext dataContext)
        {
            this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
        }

        #endregion

        #region Properties

        public bool IsWmsTimeSyncEnabled
        {
            get
            {
                lock (this.dataContext)
                {
                    return this.dataContext.WmsSettings.Single().IsWmsTimeSyncEnabled;
                }
            }
            set
            {
                lock (this.dataContext)
                {
                    this.dataContext.WmsSettings.Single().IsWmsTimeSyncEnabled = value;
                    this.dataContext.SaveChanges();
                }
            }
        }

        public DateTimeOffset LastWmsSyncTime
        {
            get
            {
                lock (this.dataContext)
                {
                    return this.dataContext.WmsSettings.Single().LastWmsTimeSync;
                }
            }
            set
            {
                lock (this.dataContext)
                {
                    this.dataContext.WmsSettings.Single().LastWmsTimeSync = value;
                    this.dataContext.SaveChanges();
                }
            }
        }

        public int TimeSyncIntervalMilliseconds
        {
            get
            {
                lock (this.dataContext)
                {
                    return this.dataContext.WmsSettings.Single().TimeSyncIntervalMilliseconds;
                }
            }
        }

        #endregion
    }
}
