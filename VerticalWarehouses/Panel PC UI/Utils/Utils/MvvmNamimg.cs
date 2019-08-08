using System;
using System.Text.RegularExpressions;

namespace Ferretto.VW.Utils
{
    public static class MvvmNaming
    {
        #region Fields

        private static readonly Regex ViewModelSuffixRegEx =
                    new Regex($"{Common.VIEWMODEL_SUFFIX}$", RegexOptions.Compiled);

        private static readonly Regex ViewRegEx = new Regex(
                    $@"^{VW.Utils.Common.ASSEMBLY_QUALIFIEDNAME_PREFIX}\.(?<moduleName>[^\.]+)\.Views\.(?<viewName>.+)",
            RegexOptions.Compiled);

        #endregion

        #region Methods

        public static string GetViewModelName(string viewFullname)
        {
            // TO DO remove when Installation modules moved into modules
            string modulePresent = string.Empty;
            if (viewFullname.Contains("Modules"))
            {
                modulePresent = ".Modules";
                viewFullname = viewFullname.Replace(".Modules", "");
            }
            var elems = GetViewNameSplit(viewFullname);
            return $"{Common.ASSEMBLY_QUALIFIEDNAME_PREFIX}{modulePresent}.{elems.moduleName}.{Common.VIEWMODELS_SUFFIX}.{elems.viewName}{Common.MODEL_SUFFIX}";
        }

        public static string GetViewNameFromViewModelName(string viewModelName)
        {
            return ViewModelSuffixRegEx.Replace(viewModelName, Common.VIEW_SUFFIX);
        }

        public static (string moduleName, string viewName) GetViewNameSplit(string viewName)
        {
            var vmMatch = ViewRegEx.Match(viewName);
            return (vmMatch.Groups[1].Value, vmMatch.Groups[2].Value);
        }

        public static bool IsViewModelNameValid(string viewModelName)
        {
            return !string.IsNullOrEmpty(viewModelName) &&
                   viewModelName.EndsWith(Common.VIEWMODEL_SUFFIX, StringComparison.InvariantCulture);
        }

        #endregion
    }
}
