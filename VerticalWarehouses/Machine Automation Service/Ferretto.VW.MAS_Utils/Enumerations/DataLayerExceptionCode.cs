namespace Ferretto.VW.MAS_Utils.Enumerations
{
    public enum DataLayerExceptionCode
    {
        // INFO DataLayer constructor exceptions
        DatalayerContextException = 001,

        EventaggregatorException = 002,

        // INFO InMemory exceptions
        ParseException = 100,

        DatatypeException = 101,

        CellNotFoundException = 102,

        NoFreeBlockBookingException = 103,

        NoFreeBlockBookedException = 104,

        UndefinedTypeException = 105,

        UnknownInfoFileException = 106,

        SaveData = 1001
    }
}
