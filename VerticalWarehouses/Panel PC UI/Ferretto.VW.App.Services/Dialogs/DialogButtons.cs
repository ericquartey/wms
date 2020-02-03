namespace Ferretto.VW.App.Services
{
    /// <summary>
    /// Specifies the buttons that are displayed on a dialog.
    /// </summary>
    public enum DialogButtons
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
}
