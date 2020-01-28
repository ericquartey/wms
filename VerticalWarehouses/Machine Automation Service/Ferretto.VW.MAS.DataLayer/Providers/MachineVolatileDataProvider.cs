using System;
using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Events;
using Prism.Events;

namespace Ferretto.VW.MAS.DataLayer
{
    internal sealed class MachineVolatileDataProvider : IMachineVolatileDataProvider
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private MachineMode mode;

        private readonly Dictionary<BayNumber, double> positions = new Dictionary<BayNumber, double>();

        #endregion

        #region Constructors

        public MachineVolatileDataProvider(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator ?? throw new System.ArgumentNullException(nameof(eventAggregator));

            this.IsBayLightOn = new Dictionary<BayNumber, bool>();

            this.positions.Add(BayNumber.BayOne, 0);
            this.positions.Add(BayNumber.BayTwo, 0);
            this.positions.Add(BayNumber.BayThree, 0);
        }

        #endregion

        #region Properties

        public MachineMode Mode
        {
            get => this.mode;
            set
            {
                if (this.mode != value)
                {
                    this.mode = value;

                    this.eventAggregator
                        .GetEvent<NotificationEvent>()
                        .Publish(
                            new NotificationMessage
                            {
                                Data = new MachineModeMessageData(this.mode),
                                Destination = MessageActor.Any,
                                Source = MessageActor.DataLayer,
                                Type = MessageType.MachineMode,
                            });
                }
            }
        }

        public Dictionary<BayNumber, bool> IsBayLightOn { get; set; }

        public bool IsHomingExecuted { get; set; }

        public bool IsMachineRunning { get; set; }

        public double ElevatorHorizontalPosition { get; set; }

        public double ElevatorVerticalPosition { get; set; }

        public Dictionary<BayNumber, bool> IsBayHomingExecuted { get; set; }

        #endregion

        #region Methods

        public double GetBayEncoderPosition(BayNumber bayNumber)
        {
            if (!this.positions.ContainsKey(bayNumber))
            {
                throw new ArgumentOutOfRangeException(nameof(bayNumber));
            }

            return this.positions[bayNumber];
        }

        public void SetBayEncoderPosition(BayNumber bayNumber, double position)
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

        #endregion
    }
}
