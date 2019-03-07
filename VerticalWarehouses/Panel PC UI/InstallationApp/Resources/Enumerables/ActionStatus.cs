using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.VW.InstallationApp.Resources.Enumerables
{
    public enum ActionStatus
    {
        None = 0,

        Start = 1,

        Executing = 2,

        Completed = 99,

        Error = 999,
    }
}
