namespace Ferretto.VW.MAS.DataModels
{
    public enum CellStatus
    {
        /// <summary>
        /// A loading unit can be layed on the cell.
        /// </summary>
        Free,

        /// <summary>
        /// No loading unit or its content shall be placed in front of the cell.
        /// </summary>
        Disabled,

        /// <summary>
        /// A loading unit or its content is occupying the cell.
        /// </summary>
        Occupied,

        /// <summary>
        /// No loading unit can be layed on the cell, but a loading unit content can be placed in front of the cell.
        /// </summary>
        /// <remarks>
        /// It shall be used for e.g. broken cells.
        /// </remarks>
        ///

        // TODO please rename to Blocked
        Unusable,
    }
}
