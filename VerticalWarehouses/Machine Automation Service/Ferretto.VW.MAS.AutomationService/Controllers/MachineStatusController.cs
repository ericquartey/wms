using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Microsoft.AspNetCore.Mvc;
using Prism.Events;
using Ferretto.VW.MAS.MissionsManager.Providers;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MachineStatusController : BaseAutomationController
    {

        #region Fields

        private readonly IRunningStateProvider runningStateProvider;

        #endregion

        #region Constructors

        public MachineStatusController(
            IRunningStateProvider runningStateProvider,
            IEventAggregator eventAggregator)
            : base(eventAggregator)
        {
            this.runningStateProvider = runningStateProvider ?? throw new ArgumentNullException(nameof(runningStateProvider));
        }

        #endregion



        #region Methods

        [HttpPost("power-off")]
        public void PowerOff()
        {
            this.runningStateProvider.SetRunningState(false, this.BayNumber, MessageActor.AutomationService);

            //var powerEnableMessageData = new PowerEnableMessageData(false);

            //this.PublishCommand(
            //    powerEnableMessageData,
            //    "Power Disable Command",
            //    MessageActor.MissionsManager,
            //    MessageType.PowerEnable);
        }

        [HttpPost("power-on")]
        public void PowerOn()
        {
            this.runningStateProvider.SetRunningState(true, this.BayNumber, MessageActor.AutomationService);

            //var powerEnableMessageData = new PowerEnableMessageData(true);

            //this.PublishCommand(
            //    powerEnableMessageData,
            //    "Power Enable Command",
            //    MessageActor.MissionsManager,
            //    MessageType.PowerEnable);
        }

        [HttpPost("stop")]
        public void Stop()
        {
            this.runningStateProvider.Stop(this.BayNumber, MessageActor.AutomationService);
        }

        #endregion
    }
}
