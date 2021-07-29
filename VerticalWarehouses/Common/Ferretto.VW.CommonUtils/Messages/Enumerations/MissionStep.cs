namespace Ferretto.VW.CommonUtils.Messages.Enumerations
{
    /// <summary>
    /// Warning: these names must match the class names, as they are used in GetStateByClassName
    /// </summary>
    public enum MissionStep
    {
        NotDefined = 0,

        New,

        Start,

        LoadElevator,

        ToTarget,

        DepositUnit,

        WaitPick,

        BayChain,

        CloseShutter,

        BackToBay,

        WaitChain,

        WaitDepositCell,   // change in WaitDepositInCell

        WaitDepositExternalBay,

        WaitDepositInternalBay,

        WaitDepositBay,

        DoubleExtBay,

        ExtBay,

        ElevatorBayUp,

        EnableRobot,

        End,

        Error = 101,

        ErrorLoad,

        ErrorDeposit,
    }
}
