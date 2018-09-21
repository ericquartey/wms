namespace Ferretto.Common.Controls
{
    public class Tile
    {
        #region Properties

        public int Count { get; set; }

        public string Image => this.Name != null ? Resources.Icons.ResourceManager.GetString($"Filter{this.Name.Replace(" ", string.Empty)}") : null;

        public string Name { get; set; }

        #endregion Properties

        #region Methods

        public override System.String ToString()
        {
            return this.Name ?? base.ToString();
        }

        #endregion Methods
    }
}
