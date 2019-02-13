using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.VW.Common_Utils.Messages
{
    public class InverterDriver_CommandMessage
    {
        public CommandType InverterCommand { get; set; }
    }

    public enum CommandType
    {
        HeartBeat,
        HorizontalHoming,
        HorizontalPosition,
        VerticalHoming,
        VerticalPosition,
        DrawerWeight
    }
}
