using System;
using Ferretto.VW.App.Installation.Resources;

namespace Ferretto.VW.App.Installation.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class ViewAttribute : Attribute
    {
        #region Fields

        private const string ArgumentCannotBeNullOrWhitespace = "Argument cannot be null or whitespace.";

        private readonly InstallatorMenuTypes installatorMenuType;

        private readonly string moduleName;

        private readonly string viewModelName;

        #endregion

        #region Constructors

        public ViewAttribute(string viewModelName, string moduleName, InstallatorMenuTypes menuType = InstallatorMenuTypes.None)
        {
            if (string.IsNullOrWhiteSpace(viewModelName))
            {
                throw new ArgumentException(ArgumentCannotBeNullOrWhitespace, nameof(viewModelName));
            }

            this.installatorMenuType = menuType;
            this.viewModelName = viewModelName;
            this.moduleName = moduleName;
        }

        #endregion

        #region Properties

        public InstallatorMenuTypes InstallatorMenuType => this.installatorMenuType;

        public string ModuleName => this.moduleName;

        public string ViewModelName => this.viewModelName;

        #endregion
    }
}
