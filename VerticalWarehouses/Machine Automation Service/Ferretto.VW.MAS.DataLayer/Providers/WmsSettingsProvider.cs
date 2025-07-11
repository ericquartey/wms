﻿using System;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.Utils.Events;
using Microsoft.EntityFrameworkCore;
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

        private readonly IMachineVolatileDataProvider machineVolatileDataProvider;

        #endregion

        #region Constructors

        public WmsSettingsProvider(
            DataLayerContext dataContext,
            IDataLayerService dataLayerService,
            IMachineVolatileDataProvider machineVolatileDataProvider,
            IEventAggregator eventAggregator)
        {
            this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
            this.dataLayerService = dataLayerService ?? throw new ArgumentNullException(nameof(dataContext));
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.machineVolatileDataProvider = machineVolatileDataProvider ?? throw new ArgumentNullException(nameof(machineVolatileDataProvider));
        }

        #endregion

        #region Properties

        public int ConnectionTimeout
        {
            get
            {
                if (!this.dataLayerService.IsReady)
                {
                    return 0;
                }

                if (this.machineVolatileDataProvider.WMSConnectionTimeout is null)
                {
                    lock (this.dataContext)
                    {
                        this.machineVolatileDataProvider.WMSConnectionTimeout = this.dataContext.WmsSettings.AsNoTracking().Select(w => w.ConnectionTimeout).Single();
                    }
                }
                return this.machineVolatileDataProvider.WMSConnectionTimeout.Value;
            }
            set
            {
                if (!this.dataLayerService.IsReady)
                {
                    return;
                }

                if (this.machineVolatileDataProvider.WMSConnectionTimeout is null || value != this.machineVolatileDataProvider.WMSConnectionTimeout.Value)
                {
                    lock (this.dataContext)
                    {
                        this.machineVolatileDataProvider.WMSConnectionTimeout = value;
                        this.dataContext.WmsSettings.Single().ConnectionTimeout = value;
                        this.dataContext.SaveChanges();
                    }
                }
            }
        }

        public bool AlarmsToWmsOn
        {
            get
            {
                lock (this.dataContext)
                {
                    var isEnabled = this.dataContext.WmsSettings.AsNoTracking().Select(w => w.AlarmsToWmsOn).Single();
                    return isEnabled;
                }
            }
            set
            {
                lock (this.dataContext)
                {
                    this.dataContext.WmsSettings.Single().AlarmsToWmsOn = value;
                    this.dataContext.SaveChanges();
                }
            }
        }

        public int DelayTimeout
        {
            get
            {
                lock (this.dataContext)
                {
                    var delay = this.dataContext.WmsSettings.AsNoTracking().Select(w => w.DelayTimeout).Single();
                    if (delay == 0)
                    {
                        this.dataContext.WmsSettings.Single().DelayTimeout = 3000;
                        this.dataContext.SaveChanges();
                    }
                    return delay;
                }
            }
            set
            {
                lock (this.dataContext)
                {
                    this.dataContext.WmsSettings.Single().DelayTimeout = value;
                    this.dataContext.SaveChanges();
                }
            }
        }

        public bool IsConnected
        {
            get
            {
                if (!this.dataLayerService.IsReady)
                {
                    return false;
                }
                if (this.machineVolatileDataProvider.WMSIsConnected is null)
                {
                    lock (this.dataContext)
                    {
                        this.machineVolatileDataProvider.WMSIsConnected = this.dataContext.WmsSettings.AsNoTracking().Select(w => w.IsConnected).Single();
                    }
                }
                return this.machineVolatileDataProvider.WMSIsConnected.Value;
            }
            set
            {
                if (!this.dataLayerService.IsReady)
                {
                    return;
                }
                if (this.machineVolatileDataProvider.WMSIsConnected is null || value != this.machineVolatileDataProvider.WMSIsConnected.Value)
                {
                    lock (this.dataContext)
                    {
                        this.machineVolatileDataProvider.WMSIsConnected = value;
                        this.dataContext.WmsSettings.Single().IsConnected = value;
                        this.dataContext.SaveChanges();
                    }
                }
            }
        }

        public bool IsEnabled
        {
            get
            {
                if (!this.dataLayerService.IsReady)
                {
                    return false;
                }

                if (this.machineVolatileDataProvider.WmsIsEnabled is null)
                {
                    lock (this.dataContext)
                    {
                        this.machineVolatileDataProvider.WmsIsEnabled = this.dataContext.WmsSettings.AsNoTracking().Select(w => w.IsEnabled).Single();
                    }
                }
                return this.machineVolatileDataProvider.WmsIsEnabled.Value;
            }
            set
            {
                if (!this.dataLayerService.IsReady)
                {
                    return;
                }
                if (this.machineVolatileDataProvider.WmsIsEnabled is null || value != this.machineVolatileDataProvider.WmsIsEnabled.Value)
                {
                    lock (this.dataContext)
                    {
                        this.machineVolatileDataProvider.WmsIsEnabled = value;
                        var settings = this.dataContext.WmsSettings.Single();

                        settings.IsEnabled = value;
                        if (this.dataContext.SaveChanges() > 0)
                        {
                            this.eventAggregator
                                .GetEvent<NotificationEvent>()
                                .Publish(
                                    new CommonUtils.Messages.NotificationMessage
                                    {
                                        Destination = MessageActor.AutomationService,
                                        Source = MessageActor.DataLayer,
                                        Type = MessageType.WmsEnableChanged
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
                        return this.dataContext.WmsSettings.AsNoTracking().Select(w => w.IsTimeSyncEnabled).Single();
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
                        return this.dataContext.WmsSettings.AsNoTracking().Select(w => w.LastWmsTimeSync).Single();
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
                    if (this.machineVolatileDataProvider.ServiceUrl is null)
                    {
                        lock (this.dataContext)
                        {
                            this.machineVolatileDataProvider.ServiceUrl = this.dataContext.WmsSettings.AsNoTracking().Select(w => w.ServiceUrl).Single() ?? DefaultUri;
                        }
                    }
                    return this.machineVolatileDataProvider.ServiceUrl;
                }
                return DefaultUri;
            }
            set
            {
                if (!this.dataLayerService.IsReady)
                {
                    return;
                }
                if (this.machineVolatileDataProvider.ServiceUrl is null || value != this.machineVolatileDataProvider.ServiceUrl)
                {
                    lock (this.dataContext)
                    {
                        this.machineVolatileDataProvider.ServiceUrl = value;
                        this.dataContext.WmsSettings.Single().ServiceUrl = value;
                        this.dataContext.SaveChanges();
                        this.eventAggregator
                            .GetEvent<NotificationEvent>()
                            .Publish(
                                new CommonUtils.Messages.NotificationMessage
                                {
                                    Destination = MessageActor.AutomationService,
                                    Source = MessageActor.DataLayer,
                                    Type = MessageType.WmsEnableChanged
                                });
                    }
                }
            }
        }

        public bool SocketLinkEndOfLine
        {
            get
            {
                if (!this.dataLayerService.IsReady)
                {
                    return false;
                }
                lock (this.dataContext)
                {
                    return this.dataContext.WmsSettings.AsNoTracking().Select(w => w.SocketLinkEndOfLine).Single();
                }
            }
            set
            {
                if (!this.dataLayerService.IsReady)
                {
                    return;
                }

                lock (this.dataContext)
                {
                    this.dataContext.WmsSettings.Single().SocketLinkEndOfLine = value;
                    this.dataContext.SaveChanges();
                }
            }
        }

        public bool SocketLinkIsEnabled
        {
            get
            {
                if (!this.dataLayerService.IsReady)
                {
                    return false;
                }

                if (this.machineVolatileDataProvider.SocketLinkIsEnabled is null)
                {
                    lock (this.dataContext)
                    {
                        this.machineVolatileDataProvider.SocketLinkIsEnabled = this.dataContext.WmsSettings.AsNoTracking().Select(w => w.SocketLinkIsEnabled).Single();
                    }
                }
                return this.machineVolatileDataProvider.SocketLinkIsEnabled.Value;
            }
            set
            {
                if (!this.dataLayerService.IsReady)
                {
                    return;
                }

                lock (this.dataContext)
                {
                    this.machineVolatileDataProvider.SocketLinkIsEnabled = value;
                    this.dataContext.WmsSettings.Single().SocketLinkIsEnabled = value;
                    this.dataContext.SaveChanges();
                }
                this.eventAggregator
                    .GetEvent<NotificationEvent>()
                    .Publish(
                        new CommonUtils.Messages.NotificationMessage
                        {
                            Destination = MessageActor.AutomationService,
                            Source = MessageActor.DataLayer,
                            Type = MessageType.SocketLinkEnableChanged
                        });
            }
        }

        public int SocketLinkPolling
        {
            get
            {
                if (!this.dataLayerService.IsReady)
                {
                    return 0;
                }

                lock (this.dataContext)
                {
                    return this.dataContext.WmsSettings.AsNoTracking().Select(w => w.SocketLinkPolling).Single();
                }
            }
            set
            {
                if (!this.dataLayerService.IsReady)
                {
                    return;
                }

                lock (this.dataContext)
                {
                    this.dataContext.WmsSettings.Single().SocketLinkPolling = value;
                    this.dataContext.SaveChanges();
                }
            }
        }

        public int SocketLinkPort
        {
            get
            {
                if (!this.dataLayerService.IsReady)
                {
                    return 0;
                }

                lock (this.dataContext)
                {
                    return this.dataContext.WmsSettings.AsNoTracking().Select(w => w.SocketLinkPort).Single();
                }
            }
            set
            {
                if (!this.dataLayerService.IsReady)
                {
                    return;
                }

                lock (this.dataContext)
                {
                    this.dataContext.WmsSettings.Single().SocketLinkPort = value;
                    this.dataContext.SaveChanges();
                }
            }
        }

        public int SocketLinkTimeout
        {
            get
            {
                if (!this.dataLayerService.IsReady)
                {
                    return 0;
                }

                lock (this.dataContext)
                {
                    return this.dataContext.WmsSettings.AsNoTracking().Select(w => w.SocketLinkTimeout).Single();
                }
            }
            set
            {
                if (!this.dataLayerService.IsReady)
                {
                    return;
                }

                lock (this.dataContext)
                {
                    this.dataContext.WmsSettings.Single().SocketLinkTimeout = value;
                    this.dataContext.SaveChanges();
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
                        return this.dataContext.WmsSettings.AsNoTracking().Select(w => w.TimeSyncIntervalMilliseconds).Single();
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
                return this.dataContext.WmsSettings.AsNoTracking().Single();
            }
        }

        public void TimeSyncIntervalMillisecondsUpdate(int seconds)
        {
            lock (this.dataContext)
            {
                var wms = this.dataContext.WmsSettings.Single();
                wms.TimeSyncIntervalMilliseconds = seconds;
                this.dataContext.WmsSettings.Update(wms);
                this.dataContext.SaveChanges();
            }
        }

        #endregion
    }
}
