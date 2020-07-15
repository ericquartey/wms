using System;
using System.Windows;
using System.Windows.Threading;
using CommonServiceLocator;
using DevExpress.Xpf.WindowsUI;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.Utils;

namespace Ferretto.VW.App.Controls
{
    internal class DialogService : IDialogService
    {
        #region Fields

        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #endregion

        #region Constructors

        public DialogService()
        {
            DevExpress.Xpf.Core.DXMessageBoxLocalizer.Active = new MesageBoxLocalizer();
        }

        #endregion

        #region Methods

        public string BrowseFolder(string description, string path = null)
        {
            using (var dlg = new System.Windows.Forms.FolderBrowserDialog())
            {
                dlg.Description = description;
                dlg.SelectedPath = path;

                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    return dlg.SelectedPath;
                }
            }
            return null;
        }

        public void Show(string moduleName, string viewModelName)
        {
            if (!MvvmNaming.IsViewModelNameValid(viewModelName))
            {
                this.logger.Warn($"Unable to show to view '{moduleName}.{viewModelName}' because name is invalid.");
                return;
            }

            this.logger.Trace($"Show view '{moduleName}.{viewModelName}'.");
            try
            {
                var navigationService = ServiceLocator.Current.GetInstance<INavigationService>();
                navigationService.LoadModule(moduleName);

                var viewName = MvvmNaming.GetViewNameFromViewModelName(viewModelName);

                var winView = ServiceLocator.Current.GetInstance<INavigableView>(viewName);

                PpcMessagePopup.ShowDialog(winView, true, false);
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, $"Cannot show view '{moduleName}.{viewModelName}'.");
            }
        }

        public void ShowCustomMessagePopup(string title, string message)
        {
            Application.Current.Dispatcher.BeginInvoke(
                       DispatcherPriority.Normal,
                       new Action(() =>
                       {
                           this.ShowCustomMessage(title, message);
                       }));
        }

        public DialogResult ShowMessage(string message, string title)
        {
            return this.ShowMessage(message, title, DialogType.Information, DialogButtons.OK);
        }

        public DialogResult ShowMessage(
            string message,
            string title,
            DialogType type,
            DialogButtons buttons)
        {
            return ShowMessageDialog(message, title, type, buttons);
        }

        private static MessageBoxButton ConvertDialogButtons(DialogButtons buttons)
        {
            switch (buttons)
            {
                case DialogButtons.OK:
                    return MessageBoxButton.OK;

                case DialogButtons.OKCancel:
                    return MessageBoxButton.OKCancel;

                case DialogButtons.YesNo:
                    return MessageBoxButton.YesNo;

                case DialogButtons.YesNoCancel:
                    return MessageBoxButton.YesNoCancel;

                default:
                    return MessageBoxButton.OK;
            }
        }

        private static MessageBoxImage ConvertDialogIcon(DialogType type)
        {
            switch (type)
            {
                case DialogType.Error:
                    return MessageBoxImage.Error;

                case DialogType.Exclamation:
                    return MessageBoxImage.Exclamation;

                case DialogType.Information:
                    return MessageBoxImage.Information;

                case DialogType.Question:
                    return MessageBoxImage.Question;

                case DialogType.Warning:
                    return MessageBoxImage.Warning;

                default:
                    return MessageBoxImage.None;
            }
        }

        private static DialogResult ConvertDialogResult(MessageBoxResult messageBoxResult)
        {
            switch (messageBoxResult)
            {
                case MessageBoxResult.Cancel:
                    return DialogResult.Cancel;

                case MessageBoxResult.No:
                    return DialogResult.No;

                case MessageBoxResult.None:
                    return DialogResult.None;

                case MessageBoxResult.OK:
                    return DialogResult.OK;

                case MessageBoxResult.Yes:
                    return DialogResult.Yes;

                default:
                    return DialogResult.None;
            }
        }

        private static DialogResult ShowMessageDialog(
            string message,
            string title,
            DialogType type,
            DialogButtons buttons)
        {
            return ConvertDialogResult(WinUIMessageBox.Show(
                message,
                title,
                ConvertDialogButtons(buttons),
                ConvertDialogIcon(type),
                MessageBoxResult.Cancel));
        }

        private void ShowCustomMessage(string title, string message)
        {
            if (Application.Current.MainWindow.IsVisible == false)
            {
                return;
            }

            var ppcMessagePopup = new PpcMessagePopup();
            var vm = new PpcMessagePopupViewModel();
            ppcMessagePopup.DataContext = vm;
            vm.Update(title, message);
            ppcMessagePopup.Topmost = false;
            ppcMessagePopup.ShowInTaskbar = false;
            PpcMessagePopup.ShowDialog(ppcMessagePopup);
        }

        #endregion

        #region Classes

        public class MesageBoxLocalizer : DevExpress.Xpf.Core.DXMessageBoxLocalizer
        {
            #region Methods

            protected override void PopulateStringTable()
            {
                base.PopulateStringTable();
                this.AddString(DevExpress.Xpf.Core.DXMessageBoxStringId.Yes, Localized.Get("General.Yes"));
                this.AddString(DevExpress.Xpf.Core.DXMessageBoxStringId.No, Localized.Get("General.No"));
                this.AddString(DevExpress.Xpf.Core.DXMessageBoxStringId.Cancel, Localized.Get("General.Cancel"));
            }

            #endregion
        }

        #endregion
    }
}
