namespace Ferretto.VW.MAS.DataModels.Enumerations
{
    public enum LoadingUnitStatus
    {
        Undefined,

        InBay,

        OnMovementToLocation,

        OnMovementToBay,

        InLocation,

        InElevator, // Gestito solo dal PPC per aggiornamento dati

        Blocked
    }
}
