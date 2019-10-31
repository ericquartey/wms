namespace Ferretto.VW.App.Controls
{
    [System.Flags]
    public enum EnableMask
    {
        Any = MachinePoweredOff | MachinePoweredOn | MachineManualMode | MachineAutomaticMode,

        MachinePoweredOff = 0x1,

        MachinePoweredOn = 0x2,

        MachineManualMode = MachinePoweredOn | 0x4,

        MachineAutomaticMode = MachinePoweredOn | 0x8,
    }
}
