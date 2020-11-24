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

        public static bool IsLogged { get; set; }

        public static bool UseAccessories { get; set; }

        public static UserAccessLevel User { get; set; }

        #endregion
    }
}
