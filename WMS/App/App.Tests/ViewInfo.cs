using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.WMS.App.Tests
{
    public class ViewInfo
    {
        #region Constructors

        public ViewInfo(string moduleName, string viewName)
        {
            this.ModuleName = moduleName;
            this.ViewName = viewName;
        }

        #endregion

        #region Properties

        public string ModuleName { get; set; }

        public string ViewName { get; set; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"{this.ModuleName}.{this.ViewName}";
        }

        #endregion
    }
}
