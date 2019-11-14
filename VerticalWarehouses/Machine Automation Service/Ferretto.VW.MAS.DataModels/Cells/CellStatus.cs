namespace Ferretto.VW.MAS.DataModels
{
    public enum CellStatus
    {
        /// <summary>
        /// A loading unit can be layed on the cell.
        /// </summary>
        Free,

        /// <summary>
        /// A loading unit or its content is occupying the cell.
        /// </summary>
        Occupied,
    }
}
