using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace Ferretto.VW.Installer.Service
{
    public static class DialogService
    {

        private const string DefaultFilter = "All Files|*.*";

        public static string[] BrowseFile(
            string title = null,
            string defaultFileName = null,
            string defaultExtension = null,
            string extDescription = null,
            string initialDirectory = null,
            bool multiselect = false)
        {
            Filters<string, string> extFilters = null;

            if (string.IsNullOrEmpty(defaultExtension))
            {
                defaultExtension = "*";
            }

            if (extDescription != null)
            {
                extFilters = new Filters<string, string>();
                extFilters.Add(defaultExtension, extDescription);
            }

            return BrowseFile(title, defaultFileName, defaultExtension, extFilters, initialDirectory, multiselect);
        }

        public static string[] BrowseFile(
            string title,
            string defaultFileName,
            string defaultExtension,
            Filters<string, string> extFilters,
            string initialDirectory,
            bool multiselect = false)
        {

            var dialog = new OpenFileDialog()
            {
                Title = title,
                CheckFileExists = true,
                CheckPathExists = true,
                InitialDirectory = initialDirectory,
                Multiselect = multiselect,
            };

            SetDialogProperties(dialog, defaultExtension, defaultFileName, extFilters);

            if (dialog.ShowDialog().GetValueOrDefault() == true)
            {
                return dialog.FileNames;
            }

            return null;
        }

        private static void SetDialogProperties(
            FileDialog dialog,
            string defaultFileExtension,
            string defaultFileName,
            Filters<string, string> filters)
        {
            if (string.IsNullOrEmpty(defaultFileName) == false)
            {
                dialog.FileName = defaultFileName;
            }

            if (string.IsNullOrEmpty(defaultFileExtension) == false)
            {
                dialog.DefaultExt = GetExtension(defaultFileExtension);
            }

            if (filters == null)
            {
                dialog.Filter = DefaultFilter;
            }
            else
            {
                var filterBuilder = new StringBuilder();

                foreach (var filter in filters)
                {
                    var description = filter.Item2;
                    var extensions = filter.Item1.Split(';')
                        .Select(item => GetExtension(item)).ToArray();

                    if (filterBuilder.Length > 0)
                    {
                        filterBuilder.Append("|");
                    }

                    filterBuilder.Append(description);

                    filterBuilder.Append(" (");
                    filterBuilder.Append(string.Join(", ", extensions));
                    filterBuilder.Append(")|");
                    filterBuilder.Append(string.Join(";", extensions));
                }

                dialog.Filter = filterBuilder.ToString();
            }
        }

        private static string GetExtension(string extension)
        {

            extension = extension.Trim();

            if (extension.StartsWith("*.") == false)
            {
                extension = "*." + extension;
            }

            if (extension.StartsWith("."))
            {
                extension = "*" + extension;
            }

            return extension;
        }
    }

    public class Filters<T1, T2> : List<Tuple<T1, T2>>
    {
        public void Add(T1 item1, T2 item2)
        {
            base.Add(new Tuple<T1, T2>(item1, item2));
        }
    }
}
