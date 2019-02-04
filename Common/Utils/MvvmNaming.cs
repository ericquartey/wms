using System;
using System.Text.RegularExpressions;

namespace Ferretto.Common.Utils
{
    public static class MvvmNaming
    {
        #region Fields

        private static readonly Regex ViewModelNameRegEx = new Regex(
            $@"^{Common.ASSEMBLY_QUALIFIEDNAME_PREFIX}\.(?<moduleName>[^\.]+)\.(?<viewModelName>.+)",
            RegexOptions.Compiled);

        private static readonly Regex ViewModelSuffixRegEx =
                    new Regex($"{Common.VIEWMODEL_SUFFIX}$", RegexOptions.Compiled);

        #endregion

        #region Methods

        public static string GetModelNameFromViewModelName(string viewModelName)
        {
            return ViewModelSuffixRegEx.Replace(viewModelName, string.Empty);
        }

        public static string GetViewModelName(string moduleName, string viewModel)
        {
            return $"{Common.ASSEMBLY_QUALIFIEDNAME_PREFIX}.{moduleName}.{viewModel}";
        }

        public static(string moduleName, string viewModelName) GetViewModelNames(string viewModelName)
        {
            return GetViewModelNameSplit(viewModelName);
        }

        public static(string moduleName, string viewModelName) GetViewModelNames<TViewModel>()
        {
            var type = typeof(TViewModel);
            var viewModelName = type.ToString();
            return GetViewModelNameSplit(viewModelName);
        }

        public static(string moduleName, string viewModelName) GetViewModelNameSplit(string viewModelName)
        {
            var vmMatch = ViewModelNameRegEx.Match(viewModelName);
            return (vmMatch.Groups[0].Value, vmMatch.Groups[1].Value);
        }

        public static string GetViewName(string moduleName, string viewModel)
        {
            return $"{Common.ASSEMBLY_QUALIFIEDNAME_PREFIX}.{moduleName}.{viewModel}{Common.VIEW_SUFFIX}";
        }

        public static string GetViewNameFromViewModelName(string viewModelName)
        {
            return ViewModelSuffixRegEx.Replace(viewModelName, Common.VIEW_SUFFIX);
        }

        public static bool IsViewModelNameValid(string viewModelName)
        {
            return !string.IsNullOrEmpty(viewModelName) &&
                   viewModelName.EndsWith(Common.VIEWMODEL_SUFFIX, StringComparison.InvariantCulture);
        }

        #endregion
    }
}
