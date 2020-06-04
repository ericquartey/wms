using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Modules.Login
{
    public static class ScaffolderUserAccesLevel
    {
        #region Properties

        public static UserAccessLevel User { get; set; }

        public static bool IsLogged { get; set; }

        #endregion
    }
}
