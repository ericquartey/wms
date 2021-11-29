using System;

namespace Ferretto.VW.App.Controls
{
    [Flags]
    public enum EnableMask
    {
        Undefined = 0,

        MachinePoweredOff = 0x1,

        MachinePoweredOn = 0x2,

        MachineManualMode = 0x4,

        MachineAutomaticMode = 0x8,

        Any = MachinePoweredOff | MachinePoweredOn | MachineManualMode | MachineAutomaticMode,
    }
}
