using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.Utils.Events;
using Microsoft.Extensions.DependencyInjection;
using Prism.Events;

namespace Ferretto.VW.MAS.DataLayer.Providers
{
    public class BayChainVolatileDataProvider : IBayChainVolatileDataProvider
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly Dictionary<BayNumber, double> positions = new Dictionary<BayNumber, double>();

        private readonly IServiceScopeFactory serviceScopeFactory;

        #endregion

        #region Constructors

        public BayChainVolatileDataProvider(
            IEventAggregator eventAggregator,
            IServiceScopeFactory serviceScopeFactory)
        {
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));

            this.eventAggregator
                .GetEvent<NotificationEvent>()
                .Subscribe(
                    this.OnDataLayerReady,
                    ThreadOption.PublisherThread,
                    false,
                    m => m.Type == MessageType.DataLayerReady);

            this.positions.Add(BayNumber.BayOne, 0);
            this.positions.Add(BayNumber.BayTwo, 0);
            this.positions.Add(BayNumber.BayThree, 0);

            try
            {
                this.UpdateLastKnownPositions();
            }
            catch
            {
                // do nothing.
                // when data layer is ready, the database will be queryed again
            }
        }

        #endregion

        #region Methods

        public double GetPositionByBayNumber(BayNumber bayNumber)
        {
            if (!this.positions.ContainsKey(bayNumber))
            {
                throw new ArgumentOutOfRangeException(nameof(bayNumber));
            }

            return this.positions[bayNumber];
        }

        public void SetPosition(BayNumber bayNumber, double position)
        {
            if (!this.positions.ContainsKey(bayNumber))
            {
                throw new ArgumentOutOfRangeException(nameof(bayNumber));
            }

            if (this.positions[bayNumber] != position)
            {
                this.positions[bayNumber] = position;

                this.eventAggregator
                    .GetEvent<NotificationEvent>()
                    .Publish(
                        new NotificationMessage
                        {
                            Data = new BayChainPositionMessageData(bayNumber, position),
                            Destination = MessageActor.Any,
                            Source = MessageActor.DataLayer,
                            Type = MessageType.BayChainPosition,
                        });
            }
        }

        private void OnDataLayerReady(NotificationMessage message)
        {
            this.UpdateLastKnownPositions();
        }

        private void UpdateLastKnownPositions()
        {
            using (var scope = this.serviceScopeFactory.CreateScope())
            {
                var dataContext = scope.ServiceProvider.GetRequiredService<DataLayerContext>();

                var lastKnownPositions = dataContext.Bays.ToDictionary(b => b.Number, b => b.LastKnownChainPosition);

                foreach (var bayNumber in this.positions.Keys)
                {
                    if (lastKnownPositions.ContainsKey(bayNumber) && lastKnownPositions[bayNumber].HasValue)
                    {
                        this.SetPosition(bayNumber, lastKnownPositions[bayNumber].Value);
                    }
                }
            }
        }

        #endregion
    }
}
