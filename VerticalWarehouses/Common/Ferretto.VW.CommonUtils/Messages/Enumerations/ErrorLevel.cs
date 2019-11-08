namespace Ferretto.VW.CommonUtils.Messages.Enumerations
{
    public enum ErrorLevel
    {
        None,

        /// <summary>
        /// Something unexpected happened. Execution will continue.
        /// </summary>
        Warning,

        /// <summary>
        /// An error occurred. The error shall be handled and execution will continue.
        /// </summary>
        Error,

        /// <summary>
        /// An unrecoverable error occurred. The application shall be terminated.
        /// </summary>
        Fatal,
    }
}
