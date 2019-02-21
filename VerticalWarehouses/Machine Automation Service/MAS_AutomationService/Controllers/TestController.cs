using Ferretto.VW.Common_Utils.EventParameters;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
using Microsoft.AspNetCore.Mvc;
using Prism.Events;

namespace Ferretto.VW.MAS_AutomationService
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        #endregion

        #region Constructors

        public TestController(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }

        #endregion

        #region Methods

        [HttpGet("AddMissionTest")]
        public void AddMission()
        {
            var missionData = new MissionData();

            var missionMessage = new Event_Message(missionData,
                "Test Mission",
                MessageActor.AutomationService,
                MessageActor.WebAPI,
                MessageStatus.Start,
                MessageType.AddMission,
                MessageVerbosity.Debug);
            this.eventAggregator.GetEvent<MachineAutomationService_Event>().Publish(missionMessage);
        }

        [HttpGet("HomingTest")]
        public void ExecuteHoming()
        {
            this.eventAggregator.GetEvent<WebAPI_CommandEvent>().Publish(new Command_EventParameter(CommandType.ExecuteHoming));
        }

        [HttpGet("HomingStop")]
        public void ExecuteStopHoming()
        {
            this.eventAggregator.GetEvent<WebAPI_CommandEvent>().Publish(new Command_EventParameter(CommandType.ExecuteStopHoming));
        }

        [HttpGet("PositioningStop")]
        public void ExecuteStopPositioning()
        {
            this.eventAggregator.GetEvent<WebAPI_CommandEvent>().Publish(new Command_EventParameter(CommandType.ExecuteStopVerticalPositioning));
        }

        [HttpGet("PositioningTest")]
        public void PositioningTest()
        {
            this.eventAggregator.GetEvent<WebAPI_CommandEvent>().Publish(new Command_EventParameter(CommandType.ExecuteVerticalPositioning));
        }

        [HttpGet("StopFSM")]
        public void StopFiniteStateMachine()
        {
            this.eventAggregator.GetEvent<WebAPI_CommandEvent>().Publish(new Command_EventParameter(CommandType.StopAction));
        }

        #endregion
    }
}
