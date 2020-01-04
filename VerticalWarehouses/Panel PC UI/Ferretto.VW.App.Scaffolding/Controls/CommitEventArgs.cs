namespace Ferretto.VW.App.Scaffolding.Controls
{
    public class CommitEventArgs
    {
        internal CommitEventArgs(object chosenValue)
        {
            this.Value = chosenValue;
        }

        public object Value { get; }
    }
}
