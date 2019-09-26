using System;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.AutomationService.StateMachines.Interface
{
    public interface IStateMachine : IDisposable
    {


        #region Properties

        IEventAggregator EventAggregator { get; }

        ILogger<AutomationService> Logger { get; }

        BayNumber RequestingBay { get; }

        IServiceScopeFactory ServiceScopeFactory { get; }

        #endregion



        #region Methods

        /// <summary>
        /// Change state.
        /// </summary>
        /// <param name="newState">The new state</param>
        /// <param name="message">A message to be published</param>
        void ChangeState(IState newState, CommandMessage message = null);

        /// <summary>
        /// Process the command message incoming to the Finite State Machines.
        /// </summary>
        /// <param name="message">A <see cref="CommandMessage"/> command message to be parsed.</param>
        void ProcessCommandMessage(CommandMessage message);

        /// <summary>
        /// Process the notification message incoming to the Finite State Machines.
        /// </summary>
        /// <param name="message">A <see cref="NotificationMessage"/> notification message to be parsed.</param>
        void ProcessNotificationMessage(NotificationMessage message);

        /// <summary>
        /// Publish a given Command message via EventAggregator.
        /// </summary>
        /// <param name="message">A <see cref="CommandMessage"/> command message to be sent.</param>
        void PublishCommandMessage(CommandMessage message);

        /// <summary>
        /// Publish a given Notification message via EventAggregator.
        /// </summary>
        /// <param name="message">A <see cref="NotificationMessage"/> notification message to be sent.</param>
        void PublishNotificationMessage(NotificationMessage message);

        /// <summary>
        /// Start the states machine.
        /// </summary>
        void Start();

        /// <summary>
        /// Stop states machine.
        /// </summary>
        /// <param name="reason"></param>
        void Stop(StopRequestReason reason);

        #endregion
    }
}
