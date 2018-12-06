using System.Collections.Generic;

namespace Ferretto.Common.DataModels
{
    // Stato di Lista Articoli
    public enum ItemListStatus
    {
        Waiting = 'W',
        Executing = 'E',
        Completed = 'C',
        Incomplete = 'I',
        Suspended = 'S'
    }
}
