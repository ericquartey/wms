using System;
using System.Text.RegularExpressions;

namespace Ferretto.Common.Utils
{
    public static class MvvmNaming
    {
        private static readonly Regex ViewModelSuffixRegEx =
            new Regex($"{Common.VIEWMODEL_SUFFIX}$", RegexOptions.Compiled);

        private static readonly Regex ViewModelNameRegEx = new Regex(
            $"^{Common.ASSEMBLY_QUALIFIEDNAME_PREFIX.Replace(".", "\\.")}\\.(?<moduleName>[^\\.]+)\\.(?<viewModelName>.+)",
            RegexOptions.Compiled);

        public static string GetModelNameFromViewModelName(string viewModelName)
        {
            return ViewModelSuffixRegEx.Replace(viewModelName, string.Empty);
        }

        public static string GetViewNameFromViewModelName(string viewModelName)
        {
            return ViewModelSuffixRegEx.Replace(viewModelName, Common.VIEW_SUFFIX);
        }

        public static (string moduleName, string viewModelName) GetViewModelNameSplitted(string viewModelName)
        {
            var vmMatch = ViewModelNameRegEx.Match(viewModelName);
            return ( vmMatch.Groups[0].Value, vmMatch.Groups[1].Value );
        }

        public static (string moduleName, string viewModelName) GetViewModelNames(string viewModelName)
        {
            return GetViewModelNameSplitted(viewModelName);
        }

        public static (string moduleName, string viewModelName) GetViewModelNames<TViewModel>()
        {
            var type = typeof(TViewModel);
            var viewModelName = type.ToString();
            return GetViewModelNameSplitted(viewModelName);
        }

        public static string GetViewName(string moduleName, string regionName)
        {
            return $"{Common.ASSEMBLY_QUALIFIEDNAME_PREFIX}.{moduleName}.{regionName}{Common.VIEW_SUFFIX}";
        }

        public static bool IsViewModelNameValid(string viewModelName)
        {
            return !string.IsNullOrEmpty(viewModelName) &&
                   viewModelName.EndsWith(Common.VIEWMODEL_SUFFIX, StringComparison.InvariantCulture);
        }
    }
}
