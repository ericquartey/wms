using Microsoft.AspNetCore.Mvc;
using Prism.Events;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.EventParameters;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.Common_Utils.Messages.Interfaces;

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
            missionData.Priority = 1;
            missionData.MissionType = MissionType.CellToBayMission;
            var missionMessage = new Event_Message(missionData,
                "Test Mission",
                MessageActor.AutomationService,
                MessageActor.WebAPI,
                MessageStatus.Start,
                MessageType.AddMission,
                MessageVerbosity.Debug);
            this.eventAggregator.GetEvent<MachineAutomationService_Event>().Publish(missionMessage);
        }

        [HttpPost("CreateMissionTest")]
        public void CreateMission([FromBody] int bayID, int drawerID)
        {
            var missionData = new MissionData();
            missionData.BayID = bayID;
            missionData.DrawerID = drawerID;

            var message = new Event_Message(missionData,
                "Create Mission",
                MessageActor.MissionsManager,
                MessageActor.WebAPI,
                MessageStatus.Start,
                MessageType.CreateMission,
                MessageVerbosity.Debug);
            this.eventAggregator.GetEvent<MachineAutomationService_Event>().Publish(message);
        }

        [HttpGet("HomingTest")]
        public string ExecuteHoming()
        {
            this.eventAggregator.GetEvent<WebAPI_CommandEvent>().Publish(new Command_EventParameter(CommandType.ExecuteHoming));
            return "Execute Homing Done!";
        }

        [HttpGet("HomingStop")]
        public void ExecuteStopHoming()
        {
            this.eventAggregator.GetEvent<WebAPI_CommandEvent>().Publish(new Command_EventParameter(CommandType.ExecuteStopHoming));
        }

        [HttpGet("MissionExecutedTest")]
        public void MissionExecuted()
        {
            var message = new Event_Message(
                null,
                "Mission Executed",
                MessageActor.MissionsManager,
                MessageActor.FiniteStateMachines,
                MessageStatus.End,
                MessageType.EndAction,
                MessageVerbosity.Debug);
            this.eventAggregator.GetEvent<MachineAutomationService_Event>().Publish(message);
        }

        [HttpGet("StopFSM")]
        public void StopFiniteStateMachine()
        {
            this.eventAggregator.GetEvent<WebAPI_CommandEvent>().Publish(new Command_EventParameter(CommandType.StopAction));
        }

        #endregion
    }
}
