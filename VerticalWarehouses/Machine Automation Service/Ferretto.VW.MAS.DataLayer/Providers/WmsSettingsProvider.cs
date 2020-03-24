﻿using System;
using System.Linq;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.Utils.Events;
using Prism.Events;

namespace Ferretto.VW.MAS.DataLayer.Providers
{
    internal class WmsSettingsProvider : IWmsSettingsProvider
    {
        #region Fields

        private static readonly int DefaultTimeSyncIntervalMilliseconds = 10 * 1000;

        private static readonly Uri DefaultUri = new Uri("http://localhost:80");

        private readonly DataLayerContext dataContext;

        private readonly IDataLayerService dataLayerService;

        private readonly IEventAggregator eventAggregator;

        #endregion

        #region Constructors

        public WmsSettingsProvider(DataLayerContext dataContext, IDataLayerService dataLayerService, IEventAggregator eventAggregator)
        {
            this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
            this.dataLayerService = dataLayerService ?? throw new ArgumentNullException(nameof(dataContext));
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
        }

        #endregion

        #region Properties

        public bool IsEnabled
        {
            get
            {
                if (this.dataLayerService.IsReady)
                {
                    lock (this.dataContext)
                    {
                        return this.dataContext.WmsSettings.Single().IsEnabled;
                    }
                }

                return false;
            }
            set
            {
                if (this.dataLayerService.IsReady)
                {
                    lock (this.dataContext)
                    {
                        var settings = this.dataContext.WmsSettings.Single();

                        settings.IsEnabled = value;
                        if (this.dataContext.SaveChanges() > 0)
                        {
                            this.eventAggregator
                                .GetEvent<NotificationEvent>()
                                .Publish(
                                    new CommonUtils.Messages.NotificationMessage
                                    {
                                        Type = CommonUtils.Messages.Enumerations.MessageType.WmsEnableChanged
                                    });
                        }
                    }
                }
            }
        }

        public bool IsTimeSyncEnabled
        {
            get
            {
                if (this.dataLayerService.IsReady)
                {
                    lock (this.dataContext)
                    {
                        return this.dataContext.WmsSettings.Single().IsTimeSyncEnabled;
                    }
                }

                return false;
            }
            set
            {
                if (this.dataLayerService.IsReady)
                {
                    lock (this.dataContext)
                    {
                        this.dataContext.WmsSettings.Single().IsTimeSyncEnabled = value;
                        this.dataContext.SaveChanges();
                    }
                }
            }
        }

        public DateTimeOffset LastWmsSyncTime
        {
            get
            {
                if (this.dataLayerService.IsReady)
                {
                    lock (this.dataContext)
                    {
                        return this.dataContext.WmsSettings.Single().LastWmsTimeSync;
                    }
                }

                return DateTimeOffset.Now;
            }
            set
            {
                if (this.dataLayerService.IsReady)
                {
                    lock (this.dataContext)
                    {
                        this.dataContext.WmsSettings.Single().LastWmsTimeSync = value;
                        this.dataContext.SaveChanges();
                    }
                }
            }
        }

        public Uri ServiceUrl
        {
            get
            {
                if (this.dataLayerService.IsReady)
                {
                    return this.dataContext.WmsSettings.Single().ServiceUrl ?? DefaultUri;
                }

                return DefaultUri;
            }
            set
            {
                if (this.dataLayerService.IsReady)
                {
                    lock (this.dataContext)
                    {
                        this.dataContext.WmsSettings.Single().ServiceUrl = value;
                        this.dataContext.SaveChanges();
                    }
                }
            }
        }

        public int TimeSyncIntervalMilliseconds
        {
            get
            {
                if (this.dataLayerService.IsReady)
                {
                    lock (this.dataContext)
                    {
                        return this.dataContext.WmsSettings.Single().TimeSyncIntervalMilliseconds;
                    }
                }

                return DefaultTimeSyncIntervalMilliseconds;
            }
        }

        #endregion

        #region Methods

        public WmsSettings GetAll()
        {
            lock (this.dataContext)
            {
                return this.dataContext.WmsSettings.Single();
            }
        }

        #endregion
    }
}
