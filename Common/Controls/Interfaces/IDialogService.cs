namespace Ferretto.Common.Controls.Interfaces
{
  public interface IDialogService
  {
    /// <summary>
    /// Shows a message dialog and returns the button selected by user.
    /// </summary>
    /// <param name="message">The message to show</param>
    /// <param name="title">The title (caption) to show in the head of the dialog</param>
    /// <param name="type">The type of dialog to show</param>
    /// <param name="buttons">The dialog available buttons</param>
    /// <returns>The dialog selected by user</returns>
    DialogResult ShowMessage(
        string message,
        string title = null,
        DialogType type = DialogType.Information,
        DialogButton buttons = DialogButton.OK);
  }

  /// <summary>
  /// Specifies the buttons that are displayed on a dialog.
  /// </summary>
  public enum DialogButton
  {
    /// <summary>
    /// The dialog displays no buttons.
    /// </summary>
    None,

    /// <summary>
    /// The dialog displays an OK button.
    /// </summary>
    OK,

    /// <summary>
    /// The dialog displays OK and Cancel buttons.
    /// </summary>
    OKCancel,

    /// <summary>
    /// The dialog displays Yes, No, and Cancel buttons.
    /// </summary>
    YesNoCancel,

    /// <summary>
    /// The dialog displays Yes and No buttons.
    /// </summary>
    YesNo,

    /// <summary>
    /// The dialog displays Yes, YesAll, No and NoAll buttons.
    /// </summary>
    YesNoAll,
  }

  /// <summary>
  /// Specifies which dialog button that a user choice.
  /// </summary>
  public enum DialogResult
  {
    /// <summary>
    /// The dialog returns no result.
    /// </summary>
    None,

    /// <summary>
    /// The result value of the dialog is OK.
    /// </summary>
    OK,

    /// <summary>
    /// The result value of the dialog is Cancel.
    /// </summary>
    Cancel,

    /// <summary>
    /// The result value of the dialog is Yes.
    /// </summary>
    Yes,

    /// <summary>
    /// The result value of the dialog is YesAll.
    /// </summary>
    YesAll,

    /// <summary>
    /// The result value of the dialog is No.
    /// </summary>
    No,

    /// <summary>
    /// The result value of the dialog is NoAll.
    /// </summary>
    NoAll,
  }

  /// <summary>
  /// Specifies the type of dialog.
  /// </summary>
  public enum DialogType
  {
    /// <summary>
    /// No icon is displayed.
    /// </summary>
    None,

    /// <summary>
    /// The dialog contains a symbol consisting of white X in a circle with
    /// a red background.
    /// </summary>
    Error,

    /// <summary>
    /// The dialog contains a symbol consisting of a question mark in a circle.
    /// </summary>
    Question,

    /// <summary>
    /// The dialog contains a symbol consisting of an exclamation point in a
    /// triangle with a yellow background.
    /// </summary>
    Exclamation,

    /// <summary>
    /// The dialog contains a symbol consisting of an exclamation point in a
    /// triangle with a yellow background.
    /// </summary>
    Warning,

    /// <summary>
    /// The dialog contains a symbol consisting of a lowercase letter i in a circle.
    /// </summary>
    Information,
  }
}
