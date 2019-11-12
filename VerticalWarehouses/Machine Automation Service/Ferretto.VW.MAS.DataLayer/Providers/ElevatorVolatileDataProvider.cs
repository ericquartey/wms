using System;
using System.Collections.Generic;
using System.Text;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.Utils.Events;
using Prism.Events;

namespace Ferretto.VW.MAS.DataLayer.Providers
{
    public class ElevatorVolatileDataProvider : IElevatorVolatileDataProvider
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private double horizontalPosition;

        private double verticalPosition;

        #endregion

        #region Constructors

        public ElevatorVolatileDataProvider(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
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
    }
}
