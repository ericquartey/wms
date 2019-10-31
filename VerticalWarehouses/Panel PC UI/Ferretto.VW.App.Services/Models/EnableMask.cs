namespace Ferretto.VW.App.Controls
{
    [System.Flags]
    public enum EnableMask
    {
        None = 0x0,

        MachineAutomaticMode = 0x1,

        MachineManualMode = 0x2,

        MachinePoweredOff = 0x3,
    }
}
