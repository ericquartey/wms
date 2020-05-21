namespace Ferretto.VW.MAS.DataModels
{
    public sealed class Instruction : DataModel
    {
        #region Properties

        public InstructionDefinition Definition { get; set; }

        public bool IsDone { get; set; }

        public bool IsToDo { get; set; }

        #endregion
    }
}
