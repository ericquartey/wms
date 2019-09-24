namespace Ferretto.VW.MAS.Utils.Enumerations
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Design",
        "CA1028:Enum Storage should be Int32",
        Justification = "We need to see if it is really necessary to specify the type here.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Minor Code Smell",
        "S4022:Enumerations should have \"Int32\" storage",
        Justification = "We need to see if it is really necessary to specify the type here.")]
    public enum IoIndex : byte
    {
        IoDevice1 = 0x00,

        IoDevice2 = 0x01,

        IoDevice3 = 0x02,

        All = 0x10,

        None = 0xFF,
    }
}
