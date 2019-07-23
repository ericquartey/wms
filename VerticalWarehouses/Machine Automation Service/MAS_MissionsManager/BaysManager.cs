using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Events;
using Microsoft.Extensions.Hosting;
using Prism.Events;

namespace Ferretto.VW.MAS.MissionsManager
{
    public class BaysManager : IBaysManager
    {
        #region Fields

        private readonly IList<Bay> bays = new List<Bay>();

        private readonly IEventAggregator eventAggregator;

        private readonly IGeneralInfoConfigurationDataLayer generalInfoConfiguration;

        private readonly ISetupNetworkDataLayer networkConfiguration;

        #endregion

        #region Constructors

        public BaysManager(
            IEventAggregator eventAggregator,
            ISetupNetworkDataLayer networkConfiguration,
            IGeneralInfoConfigurationDataLayer generalInfoConfiguration)
        {
            if (eventAggregator == null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            if (networkConfiguration == null)
            {
                throw new ArgumentNullException(nameof(networkConfiguration));
            }

            if (generalInfoConfiguration == null)
            {
                throw new ArgumentNullException(nameof(generalInfoConfiguration));
            }

            this.eventAggregator = eventAggregator;
            this.networkConfiguration = networkConfiguration;
            this.generalInfoConfiguration = generalInfoConfiguration;

            this.eventAggregator
                .GetEvent<ClientConnectionChangedPubSubEvent>()
                .Subscribe(e => this.OnClientConnectionChanged(e));
        }

        #endregion

        #region Properties

        public IEnumerable<Bay> Bays => this.bays;

        #endregion

        #region Methods

        public async Task SetupBaysAsync()
        {
            var baysCount = await this.generalInfoConfiguration.BaysQuantity;
            var ipAddresses = new List<System.Net.IPAddress>();
            var bayTypes = new List<BayType>();

            switch (baysCount)
            {
                case 1:
                    ipAddresses.Add(await this.networkConfiguration.PPC1MasterIPAddress);

                    bayTypes.Add((BayType)await this.generalInfoConfiguration.Bay1Type);
                    break;

                case 2:
                    ipAddresses.Add(await this.networkConfiguration.PPC1MasterIPAddress);
                    ipAddresses.Add(await this.networkConfiguration.PPC2SlaveIPAddress);

                    bayTypes.Add((BayType)await this.generalInfoConfiguration.Bay1Type);
                    bayTypes.Add((BayType)await this.generalInfoConfiguration.Bay2Type);

                    break;

                case 3:
                    ipAddresses.Add(await this.networkConfiguration.PPC1MasterIPAddress);
                    ipAddresses.Add(await this.networkConfiguration.PPC2SlaveIPAddress);
                    ipAddresses.Add(await this.networkConfiguration.PPC3SlaveIPAddress);

                    bayTypes.Add((BayType)await this.generalInfoConfiguration.Bay1Type);
                    bayTypes.Add((BayType)await this.generalInfoConfiguration.Bay2Type);
                    bayTypes.Add((BayType)await this.generalInfoConfiguration.Bay3Type);

                    break;

                default:
                    throw new Exception($"Unexpected amount of bays in configuration ({baysCount})");
            }

            this.bays.Clear();
            for (var i = 0; i < baysCount; i++)
            {
                this.bays.Add(new Bay
                {
                    Id = i == 0 ? 2 : 3,  // TODO get actual bay ID from WMS
                    Status = BayStatus.Unavailable,
                    IpAddress = ipAddresses[i],
                    Type = bayTypes[i]
                });
            }
        }

        private void OnClientConnectionChanged(ClientConnectionChangedPayload e)
        {
            if (e == null)
            {
                throw new ArgumentNullException(nameof(e));
            }

            var bay = this.Bays.SingleOrDefault(b => b.IpAddress.Equals(e.ClientIpAddress));
            if (bay == null)
            {
                return;
            }

            if (e.IsConnected)
            {
                bay.ConnectionId = e.ConnectionId;
                bay.Status = BayStatus.Idle;

                this.eventAggregator
                    .GetEvent<NotificationEvent>()
                    .Publish(new NotificationMessage(
                        new BayConnectedMessageData
                        {
                            BayId = bay.Id,
                            BayType = (int)bay.Type,
                            PendingMissionsCount = bay.PendingMissions.Count(),
                            ConnectionId = bay.ConnectionId
                        },
                        "Bay Connected",
                        MessageActor.Any,
                        MessageActor.WebApi,
                        MessageType.BayConnected,
                        MessageStatus.NoStatus));
            }
            else
            {
                bay.ConnectionId = null;
                bay.Status = BayStatus.Unavailable;
            }
        }

        #endregion
    }
}
