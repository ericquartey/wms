using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.RemoteIODriver.Source;

namespace Ferretto.VW.ActionBlocks
{
    public interface ISwitchMotors
    {
        RemoteIO SetRemoteIOInterface { set; }
        InverterDriver.InverterDriver SetInverterDriverInterface { set; }
        void Initialize();
        void SwitchHorizToVert();
        void SwitchVertToHoriz();
        void Terminate();
    }
}
