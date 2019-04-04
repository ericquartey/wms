﻿namespace Ferretto.VW.Common_Utils.Enumerations
{
    public enum DataLayerExceptionCode
    {
        // INFO DataLayer constructor exceptions
        DATALAYER_CONTEXT_EXCEPTION = 001,

        EVENTAGGREGATOR_EXCEPTION = 002,

        // INFO SQLite exceptions
        PARSE_EXCEPTION = 100,

        DATATYPE_EXCEPTION = 101,

        CELL_NOT_FOUND_EXCEPTION = 102,

        NO_FREE_BLOCK_BOOKING_EXCEPTION = 103,

        NO_FREE_BLOCK_BOOKED_EXCEPTION = 104,

        UNDEFINED_TYPE_EXCEPTION = 105,

        UNKNOWN_INFO_FILE_EXCEPTION = 106,

        UPDATE_PARTITIONS_EXCEPTION = 107,

        SaveData = 1001
    }
}
