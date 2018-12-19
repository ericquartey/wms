using System;
using System.Collections.Generic;

namespace Ferretto.Common.Utils.Menu
{
    public class MainMenuItem
    {
        #region Constructors

        public MainMenuItem()
        {
            this.Children = new List<MainMenuItem>();
        }

        public MainMenuItem(String name, String backgroundColor, string image, String moduleName, String viewName, object data = null) : this()
        {
            this.Name = name;
            this.Image = image;
            this.BackgroundColor = backgroundColor;
            this.ModuleName = moduleName;
            this.ViewName = viewName;
            this.Data = data;
        }

        #endregion Constructors

        #region Properties

        public string BackgroundColor { get; set; }

        public List<MainMenuItem> Children { get; set; }

        public object Data { get; set; }

        public string Image { get; set; }

        public string ModuleName { get; set; }

        public string Name { get; set; }

        public string ViewName { get; set; }

        #endregion Properties
    }
}
