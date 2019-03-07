using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.VW.InstallationApp.Resources.Enumerables
{
    public enum ActionType
    {
        None = 0,

        VerticalPositioning = 1,

        HorizontalPositiong = 2,

        ShutterPositioning = 3,

        Homing = 4,

        Stop = 99,
    }
}
