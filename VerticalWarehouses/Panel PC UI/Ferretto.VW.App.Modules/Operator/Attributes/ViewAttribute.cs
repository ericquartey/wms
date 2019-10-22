using System;
using Ferretto.VW.App.Operator.Resources;

namespace Ferretto.VW.App.Operator.Attributes
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
            OperatorMenuTypes menuType = OperatorMenuTypes.None,
            bool trackCurrentView = true)
        {
            if (string.IsNullOrWhiteSpace(viewModelName))
            {
                throw new ArgumentException(ArgumentCannotBeNullOrWhitespace, nameof(viewModelName));
            }

            this.OperatorMenuType = menuType;
            this.ViewModelName = viewModelName;
            this.ModuleName = moduleName;
        }

        #endregion

        #region Properties

        public string ModuleName { get; }

        public OperatorMenuTypes OperatorMenuType { get; }

        public string ViewModelName { get; }

        #endregion
    }
}
