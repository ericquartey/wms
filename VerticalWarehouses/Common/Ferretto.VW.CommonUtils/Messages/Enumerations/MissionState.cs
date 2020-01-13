namespace Ferretto.VW.CommonUtils.Messages.Enumerations
{
    // Warning: there names must match the class names, as they are used in GetStateByClassName
    public enum MissionState
    {
        NotDefined,

        New,

        Start,

        LoadElevator,

        ToTarget,

        DepositUnit,

        WaitPick,

        BayChain,

        CloseShutter,

        End,

        Error
    }
}
