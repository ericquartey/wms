using System;
using System.Windows;
using System.Windows.Threading;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.WindowsUI;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Controls.Interfaces;

namespace Ferretto.VW.App.Services
{
    public class DialogService : IDialogService
    {
        #region Constructors

        public DialogService()
        {
            DXMessageBoxLocalizer.Active = new MesageBoxLocalizer();
        }

        #endregion

        #region Methods

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
            return this.ShowMessage(message, title, DialogType.Information, Controls.Interfaces.DialogButtons.OK);
        }

        public DialogResult ShowMessage(
            string message,
            string title,
            DialogType type,
            Controls.Interfaces.DialogButtons buttons)
        {
            return ShowMessageDialog(message, title, type, buttons);
        }

        private static MessageBoxButton ConvertDialogButtons(Controls.Interfaces.DialogButtons buttons)
        {
            switch (buttons)
            {
                case Controls.Interfaces.DialogButtons.OK:
                    return MessageBoxButton.OK;

                case Controls.Interfaces.DialogButtons.OKCancel:
                    return MessageBoxButton.OKCancel;

                case Controls.Interfaces.DialogButtons.YesNo:
                    return MessageBoxButton.YesNo;

                case Controls.Interfaces.DialogButtons.YesNoCancel:
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
                                    Controls.Interfaces.DialogButtons buttons)
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

        public class MesageBoxLocalizer : DXMessageBoxLocalizer
        {
            #region Methods

            protected override void PopulateStringTable()
            {
                base.PopulateStringTable();
                this.AddString(DXMessageBoxStringId.Yes, "Si");
                this.AddString(DXMessageBoxStringId.No, "No");
                this.AddString(DXMessageBoxStringId.Cancel, "Annulla");
            }

            #endregion
        }

        #endregion
    }
}
