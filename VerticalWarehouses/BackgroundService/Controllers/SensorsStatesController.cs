using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Ferretto.VW.RemoteIODriver.Source;
using Ferretto.VW.Utils.Source;

namespace BackgroundService
{
    [Route("api/[controller]")]
    [ApiController]
    public class SensorsStatesController
    {
        #region Fields

        private readonly IHubContext<SensorsStatesHub, ISensorsStatesHub> hub;

        #endregion Fields

        #region Constructors

        public SensorsStatesController(IHubContext<SensorsStatesHub, ISensorsStatesHub> hub)
        {
            this.hub = hub;
            RemoteIOManager.Current = new RemoteIOManager();
            //Console.WriteLine("RemoteIOManager created.");
        }

        #endregion Constructors

        #region Properties

        public static SensorsStates LastSensorsStates { get; set; } = new SensorsStates();

        #endregion Properties

        #region Methods

        [HttpGet("get-sensors")]
        public async Task<ActionResult<SensorsStates>> Get()
        {
            var remoteIO = new RemoteIO();
            this.DoThings(remoteIO);
            return null;
        }

        [HttpGet("get-random")]
        public async Task<ActionResult<SensorsStates>> GetRandom()
        {
            return new SensorsStates();
        }

        private async Task DoThings(RemoteIO rm)
        {
            while (true)
            {
                var tmp = new SensorsStates(rm.ReadData().ToArray());
                if (LastSensorsStates.Sensor1 == tmp.Sensor1 &&
                LastSensorsStates.Sensor2 == tmp.Sensor2 &&
                LastSensorsStates.Sensor3 == tmp.Sensor3 &&
                LastSensorsStates.Sensor4 == tmp.Sensor4 &&
                LastSensorsStates.Sensor5 == tmp.Sensor5 &&
                LastSensorsStates.Sensor6 == tmp.Sensor6 &&
                LastSensorsStates.Sensor7 == tmp.Sensor7 &&
                LastSensorsStates.Sensor8 == tmp.Sensor8)
                {
                    //Console.WriteLine("Sensors DIDN'T Change.");
                }
                else
                {
                    //Console.WriteLine("Sensors changed: TMP: " + tmp.Sensor1 + ", " + tmp.Sensor2 + ", " + tmp.Sensor3 + ", " + tmp.Sensor4 + ", " + tmp.Sensor5 + ", " + tmp.Sensor6 + ", " + tmp.Sensor7 + ", " + tmp.Sensor8);
                    //Console.WriteLine("   LastSensorsStates: " + LastSensorsStates.Sensor1 + ", " + LastSensorsStates.Sensor2 + ", " + LastSensorsStates.Sensor3 + ", " + LastSensorsStates.Sensor4 + ", " + LastSensorsStates.Sensor5 + ", " + LastSensorsStates.Sensor6 + ", " + LastSensorsStates.Sensor7 + ", " + LastSensorsStates.Sensor8);
                    LastSensorsStates = tmp;
                    await this.hub.Clients.All.SensorsChanged(tmp);
                }
                await Task.Delay(20);
            }
        }

        #endregion Methods
    }
}
