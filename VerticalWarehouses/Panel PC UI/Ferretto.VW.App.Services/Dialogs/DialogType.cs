namespace Ferretto.VW.App.Services
{
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
