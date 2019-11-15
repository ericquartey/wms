namespace Ferretto.VW.CommonUtils.Messages.Enumerations
{
    [System.Flags]
    public enum MachineMovementMode
    {
        NotMovement = 0x1,

        ElevatorMovement = 0x2,

        BayMovement = 0x4,

        ShutterMovement = 0x8,
    }
}
