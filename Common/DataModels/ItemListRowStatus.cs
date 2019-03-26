namespace Ferretto.Common.DataModels
{
    // Stato di Riga di Lista Articoli
    public enum ItemListRowStatus
    {
        New = 'N',

        Waiting = 'W',

        Executing = 'X',

        Completed = 'C',

        Error = 'E',

        Incomplete = 'I',

        Suspended = 'S'
    }
}
