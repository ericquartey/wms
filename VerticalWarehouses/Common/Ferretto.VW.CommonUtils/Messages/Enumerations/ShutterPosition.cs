namespace Ferretto.VW.CommonUtils.Messages.Enumerations
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Minor Code Smell",
        "S4022:Enumerations should have \"Int32\" storage",
        Justification = "Review the code to understand if the ushort type specification is really necessary.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Design",
        "CA1028:Enum Storage should be Int32",
        Justification = "Review the code to understand if the ushort type specification is really necessary.")]
    public enum ShutterPosition : ushort
    {
        None = 0,

        Opened = 1,

        Half = 2,

        Closed = 3,

        Intermediate = 4
    }
}
