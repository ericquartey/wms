namespace Ferretto.VW.CommonUtils.Messages.Enumerations
{
    public enum DrawerOperationStep
    {
        None = 0,

        LoadingDrawerFromBay,

        MovingElevatorUp,

        StoringDrawerToCell,

        LoadingDrawerFromCell,

        MovingElevatorDown,

        StoringDrawerToBay,
    }
}
