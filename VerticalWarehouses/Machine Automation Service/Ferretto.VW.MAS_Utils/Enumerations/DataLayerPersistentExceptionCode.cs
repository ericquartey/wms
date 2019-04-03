namespace Ferretto.VW.Common_Utils.Enumerations
{
    public enum DataLayerPersistentExceptionCode
    {
        VALUE_NOT_FOUND = 001,

        DATA_CONTEXT_NOT_VALID = 002,

        PARSE_VALUE = 003,

        PRIMARY_PARTITION_FAILURE = 010,

        PRIMARY_AND_SECONDARY_PARTITION_FAILURE = 011,

        SECONDARY_PARTITION_FAILURE = 012,
    }
}
