using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.VW.App.Controls.Services
{
    public class NavigationTrack
    {
        #region Constructors

        public NavigationTrack(string moduleName, string viewName)
        {
            this.ModuleName = moduleName;
            this.ViewName = viewName;
        }

        #endregion

        #region Properties

        public string ModuleName { get; set; }

        public string ViewName { get; set; }

        #endregion
    }
}
