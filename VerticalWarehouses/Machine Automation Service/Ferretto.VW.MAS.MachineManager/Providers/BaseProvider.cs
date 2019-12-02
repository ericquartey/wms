using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Prism.Events;

namespace Ferretto.VW.MAS.MachineManager.Providers
{
    internal class BaseProvider
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        #endregion

        #region Constructors

        protected BaseProvider(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
        }

        #endregion

        #region Methods

        protected void SendCommandToMachineManager(
            IMessageData messageData,
            string description,
            MessageActor sender,
            MessageType messageType,
            BayNumber requestingBay,
            BayNumber targetBay = BayNumber.None)
        {
            this.eventAggregator
                .GetEvent<CommandEvent>()
                .Publish(
                    new CommandMessage(
                        messageData,
                        description,
                        MessageActor.MachineManager,
                        sender,
                        messageType,
                        requestingBay,
                        targetBay));
        }

        #endregion
    }
}
