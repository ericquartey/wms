using System;
using System.Collections.Generic;

namespace Ferretto.Common.Utils.Menu
{
    public class MainMenuItem
    {
        #region Properties

        public string Name { get; set; }
        public string Image { get; set; }
        public string ModuleName { get; set; }
        public string ViewName { get; set; }
        public string BackGroundColor { get; set; }
        public List<MainMenuItem> Children { get; set; }

        #endregion

        #region Ctor

        public MainMenuItem()
        {
            this.Children = new List<MainMenuItem>();
        }

        public MainMenuItem(String name, String backGroundColor, string image, String moduleName,
            String viewName) : this()
        {
            this.Name = name;
            this.Image = image;
            this.BackGroundColor = backGroundColor;
            this.ModuleName = moduleName;
            this.ViewName = viewName;
        }

        #endregion
    }
}
