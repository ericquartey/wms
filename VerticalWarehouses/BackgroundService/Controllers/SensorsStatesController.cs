using System.Threading.Tasks;
using Ferretto.VW.RemoteIODriver;
using Ferretto.VW.Utils.Source;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace BackgroundService
{
    [Route("api/[controller]")]
    [ApiController]
    public class SensorsStatesController
    {
        #region Fields

        private readonly IHubContext<SensorsStatesHub, ISensorsStatesHub> hub;

        #endregion

        #region Constructors

        public SensorsStatesController(IHubContext<SensorsStatesHub, ISensorsStatesHub> hub)
        {
            this.hub = hub;
        }

        #endregion

        #region Properties

        public SensorsStates LastSensorsStates { get; set; } = new SensorsStates();

        #endregion

        #region Methods

        [HttpGet]
        public async Task<ActionResult<SensorsStates>> Get()
        {
            var remoteIO = new RemoteIO();
            this.ReadSensors(remoteIO);
            return null;
        }

        [HttpGet("get-random")]
        public async Task<ActionResult<SensorsStates>> GetRandom()
        {
            return new SensorsStates();
        }

        private async Task ReadSensors(RemoteIO rm)
        {
            while (true)
            {
                var tmp = new SensorsStates(rm.Inputs.ToArray());
                if (this.LastSensorsStates.Sensor1 == tmp.Sensor1 &&
                this.LastSensorsStates.Sensor2 == tmp.Sensor2 &&
                this.LastSensorsStates.Sensor3 == tmp.Sensor3 &&
                this.LastSensorsStates.Sensor4 == tmp.Sensor4 &&
                this.LastSensorsStates.Sensor5 == tmp.Sensor5 &&
                this.LastSensorsStates.Sensor6 == tmp.Sensor6 &&
                this.LastSensorsStates.Sensor7 == tmp.Sensor7 &&
                this.LastSensorsStates.Sensor8 == tmp.Sensor8)
                {
                }
                else
                {
                    this.LastSensorsStates = tmp;
                    await this.hub.Clients.All.OnSensorsChanged(tmp);
                }

                await Task.Delay(20);
            }
        }

        #endregion
    }
}
