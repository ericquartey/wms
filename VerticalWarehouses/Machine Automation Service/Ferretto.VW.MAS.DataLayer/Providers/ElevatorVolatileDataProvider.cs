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
    public class ElevatorVolatileDataProvider : IElevatorVolatileDataProvider
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly IServiceScopeFactory serviceScopeFactory;

        private double horizontalPosition;

        private double verticalPosition;

        #endregion

        #region Constructors

        public ElevatorVolatileDataProvider(
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
            try
            {
                this.UpdateLastKnownPosition();
            }
            catch
            {
                // do nothing.
                // when data layer is ready, the database will be queryed again
            }
        }

        #endregion

        #region Properties

        public double HorizontalPosition
        {
            get => this.horizontalPosition;
            set
            {
                if (this.horizontalPosition != value)
                {
                    this.horizontalPosition = value;

                    this.eventAggregator
                        .GetEvent<NotificationEvent>()
                        .Publish(
                            new NotificationMessage
                            {
                                Data = new ElevatorPositionMessageData(this.verticalPosition, value),
                                Destination = MessageActor.Any,
                                Source = MessageActor.DataLayer,
                                Type = MessageType.ElevatorPosition,
                            });
                }
            }
        }

        public double VerticalPosition
        {
            get => this.verticalPosition;
            set
            {
                if (this.verticalPosition != value)
                {
                    this.verticalPosition = value;

                    this.eventAggregator
                        .GetEvent<NotificationEvent>()
                        .Publish(
                            new NotificationMessage
                            {
                                Data = new ElevatorPositionMessageData(value, this.horizontalPosition),
                                Destination = MessageActor.Any,
                                Source = MessageActor.DataLayer,
                                Type = MessageType.ElevatorPosition,
                            });
                }
            }
        }

        #endregion

        #region Methods

        private void OnDataLayerReady(NotificationMessage message)
        {
            this.UpdateLastKnownPosition();
        }

        private void UpdateLastKnownPosition()
        {
            using (var scope = this.serviceScopeFactory.CreateScope())
            {
                var dataContext = scope.ServiceProvider.GetRequiredService<DataLayerContext>();

                var lastKnownPositions = dataContext.ElevatorAxes.ToDictionary(a => a.Orientation, a => a.LastKnownPosition);

                if (lastKnownPositions.ContainsKey(Orientation.Horizontal)
                    &&
                    lastKnownPositions[Orientation.Horizontal].HasValue)
                {
                    this.HorizontalPosition = lastKnownPositions[Orientation.Horizontal].Value;
                }

                if (lastKnownPositions.ContainsKey(Orientation.Vertical)
                    &&
                    lastKnownPositions[Orientation.Vertical].HasValue)
                {
                    this.VerticalPosition = lastKnownPositions[Orientation.Vertical].Value;
                }
            }
        }

        #endregion
    }
}
