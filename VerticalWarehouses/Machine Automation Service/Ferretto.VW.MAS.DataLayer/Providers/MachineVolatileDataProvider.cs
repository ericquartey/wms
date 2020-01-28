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

        #endregion

        #region Constructors

        public MachineVolatileDataProvider(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator ?? throw new System.ArgumentNullException(nameof(eventAggregator));

            this.IsBayLightOn = new Dictionary<BayNumber, bool>();
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

        #endregion
    }
}
