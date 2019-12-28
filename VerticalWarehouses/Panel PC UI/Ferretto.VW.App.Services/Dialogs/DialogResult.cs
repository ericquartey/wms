namespace Ferretto.VW.App.Services
{
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
}
