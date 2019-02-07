using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.VW.Common_Utils.EventParameters
{
    public class Command_EventParameter
    {
        public CommandType CommandType { get; set; }

        public Command_EventParameter(CommandType commandType)
        {
            this.CommandType = commandType;
        }
    }

    public enum CommandType
    {
        ExecuteHoming
    }
}
