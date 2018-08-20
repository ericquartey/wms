using System;
using System.Windows;
using DevExpress.Xpf.WindowsUI;
using Ferretto.Common.Controls.Interfaces;

namespace Ferretto.Common.Controls.Services
{
  public class DialogService : IDialogService
  {
    #region Methods
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

    static private DialogResult ConvertDialogResult(MessageBoxResult messageBoxResult)
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

    static private MessageBoxImage ConvertDialogIcon(DialogType type)
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

    static private MessageBoxButton ConvertDialogButtons(DialogButtons buttons)
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
    #endregion
  }
}
