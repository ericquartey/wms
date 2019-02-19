namespace Ferretto.VW.Common_Utils
{
    public enum DataLayerExceptionEnum
    {
        // DataLayer constructor exceptions
        DATALAYER_CONTEXT_EXCEPTION = 001,

        EVENTAGGREGATOR_EXCEPTION = 002,

        // InMemory exceptions
        PARSE_EXCEPTION = 100, // Exception for a wrong data parse

        DATATYPE_EXCEPTION = 101, // Exception for wrong data type to parse

        CELL_NOT_FOUND_EXCEPTION = 102, // Exception for a cell not found
    }
}
