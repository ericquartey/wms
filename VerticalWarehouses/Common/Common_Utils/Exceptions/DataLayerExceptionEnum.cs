namespace Ferretto.VW.Common_Utils
{
    public enum DataLayerExceptionEnum
    {
        // INFO DataLayer constructor exceptions
        DATALAYER_CONTEXT_EXCEPTION = 001,

        EVENTAGGREGATOR_EXCEPTION = 002,

        // INFO InMemory exceptions
        PARSE_EXCEPTION = 100,

        DATATYPE_EXCEPTION = 101,

        CELL_NOT_FOUND_EXCEPTION = 102,

        NO_FREE_BLOCK_EXCEPTION = 103,
    }
}
