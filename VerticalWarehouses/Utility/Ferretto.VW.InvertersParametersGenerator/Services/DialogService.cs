using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Ferretto.VW.InvertersParametersGenerator.Service
{
    public static class DialogService
    {
        #region Fields

        private const string DefaultFilter = "All Files|*.*";

        #endregion Fields

        #region Methods

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
            try
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
            catch (Exception ex)
            {
                return null;
            }
        }

        public static string BrowseFolder(string title, string path = null)
        {
            var dlg = new CommonOpenFileDialog();
            dlg.Title = title;
            dlg.IsFolderPicker = true;
            dlg.InitialDirectory = path;

            dlg.AddToMostRecentlyUsedList = false;
            dlg.AllowNonFileSystemItems = false;
            dlg.DefaultDirectory = path;
            dlg.EnsureFileExists = true;
            dlg.EnsurePathExists = true;
            dlg.EnsureReadOnly = false;
            dlg.EnsureValidNames = true;
            dlg.Multiselect = false;
            dlg.ShowPlacesList = true;

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                return dlg.FileName;
            }

            return null;
        }

        public static string SaveFile(string title, string fineName, string defaultExtension, string extDescription, string initialDirectory = null)
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

            var dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.Title = title;
            dialog.InitialDirectory = initialDirectory;

            SetDialogProperties(dialog, defaultExtension, fineName, extFilters);

            var result = dialog.ShowDialog();
            if (result == true)
            {
                return dialog.FileName;
            }

            return null;
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

        #endregion Methods
    }

    public class Filters<T1, T2> : List<Tuple<T1, T2>>
    {
        #region Methods

        public void Add(T1 item1, T2 item2)
        {
            base.Add(new Tuple<T1, T2>(item1, item2));
        }

        #endregion Methods
    }
}
