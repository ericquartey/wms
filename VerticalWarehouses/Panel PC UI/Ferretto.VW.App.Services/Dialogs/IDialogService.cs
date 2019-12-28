namespace Ferretto.VW.App.Services
{
    public interface IDialogService
    {
        #region Methods

        void Show(string moduleName, string viewModelName);

        void ShowCustomMessagePopup(string title, string message);

        /// <summary>
        /// Shows a message dialog and returns the button selected by user.
        /// </summary>
        /// <param name="message">The message to show.</param>
        /// <param name="title">The title (caption) to show in the head of the dialog.</param>
        /// <param name="type">The type of dialog to show.</param>
        /// <param name="buttons">The dialog available buttons.</param>
        /// <returns>The dialog selected by user</returns>
        DialogResult ShowMessage(
            string message,
            string title,
            DialogType type,
            DialogButtons buttons);

        /// <summary>
        /// Shows an informational message dialog with an OK button.
        /// </summary>
        /// <param name="message">The message to show.</param>
        /// <param name="title">The title (caption) to show in the head of the dialog.</param>
        /// <returns>The action selected by user.</returns>
        DialogResult ShowMessage(string message, string title);

        #endregion
    }
}
