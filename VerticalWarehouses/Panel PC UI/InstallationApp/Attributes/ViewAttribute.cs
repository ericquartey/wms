using System;
using Ferretto.VW.App.Installation.Resources;

namespace Ferretto.VW.App.Installation.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class ViewAttribute : Attribute
    {
        #region Fields

        private const string ArgumentCannotBeNullOrWhitespace = "Argument cannot be null or whitespace.";

        #endregion

        #region Constructors

        public ViewAttribute(
            string viewModelName,
            string moduleName,
            InstallatorMenuTypes menuType = InstallatorMenuTypes.None,
            bool isTrackable = true)
        {
            if (string.IsNullOrWhiteSpace(viewModelName))
            {
                throw new ArgumentException(ArgumentCannotBeNullOrWhitespace, nameof(viewModelName));
            }

            this.InstallatorMenuType = menuType;
            this.ViewModelName = viewModelName;
            this.ModuleName = moduleName;
            this.IsTrackable = isTrackable;
        }

        #endregion

        #region Properties

        public bool IsTrackable { get; }

        public InstallatorMenuTypes InstallatorMenuType { get; }

        public string ModuleName { get; }

        public string ViewModelName { get; }

        #endregion
    }
}
