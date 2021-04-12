namespace Ferretto.VW.CommonUtils.Messages.Enumerations
{
    // Warning: these names must match the class names, as they are used in GetStateByClassName
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

        End,

        Error = 101,

        ErrorLoad,

        ErrorDeposit
    }
}
