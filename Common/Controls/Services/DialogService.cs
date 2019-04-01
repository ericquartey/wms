using System;
using System.Windows;
using System.Windows.Threading;
using DevExpress.Xpf.WindowsUI;
using Ferretto.Common.Controls.Interfaces;

namespace Ferretto.Common.Controls.Services
{
    public class DialogService : IDialogService
    {
        #region Fields

        private WmsMessagePopup wmsMessagePopup;

        #endregion

        #region Constructors

        public DialogService(INavigationService navigationService)
        {
            navigationService?.Register<WmsMessagePopup, WmsMessagePopupViewModel>();
        }

        #endregion

        #region Methods

        public void ShowErrorDialog(string title, string message, bool isError)
        {
            Application.Current.Dispatcher.BeginInvoke(
                       DispatcherPriority.Normal,
                       new Action(() =>
                       {
                           this.ShowMessagePopupError(title, message, isError);
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
                ConvertDialogIcon(type)));
        }

        private void ShowMessagePopupError(string title, string message, bool isError)
        {
            if (Application.Current.MainWindow.IsVisible == false)
            {
                return;
            }

            if (this.wmsMessagePopup == null &&
                isError == false)
            {
                return;
            }

            if (this.wmsMessagePopup != null &&
                this.wmsMessagePopup.IsVisible == false)
            {
                this.wmsMessagePopup.Disappear();
                this.wmsMessagePopup = null;
            }

            if (this.wmsMessagePopup == null)
            {
                this.wmsMessagePopup = new WmsMessagePopup();
                WmsMessagePopup.ShowDialog(this.wmsMessagePopup as INavigableView, true);
            }

            Application.Current.Dispatcher.BeginInvoke(
                     DispatcherPriority.Loaded,
                     new Action(() =>
                     ((WmsMessagePopupViewModel)this.wmsMessagePopup.DataContext).Update(title, message, isError)));
        }

        #endregion
    }
}
