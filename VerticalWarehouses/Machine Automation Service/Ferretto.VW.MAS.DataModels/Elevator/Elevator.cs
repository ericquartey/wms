namespace Ferretto.VW.MAS.DataModels
{
    public sealed class Elevator : DataModel
    {
        #region Properties

        public MovementParameters EmptyLoadMovementParameters { get; set; }

        public Axis HorizontalAxis { get; set; }

        public int? LoadingUnitOnBoard { get; set; }

        public MovementParameters MaximumLoadMovementParameters { get; set; }

        public decimal MaximumLoadOnBoard { get; set; }

        public ElevatorStructuralProperties StructuralProperties { get; set; }

        public Axis VerticalAxis { get; set; }

        #endregion
    }
}
