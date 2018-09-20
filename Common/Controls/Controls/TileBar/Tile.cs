namespace Ferretto.Common.Controls
{
    public class Tile
    {
        #region Properties

        public int Count { get; set; }
        public string Id { get; set; }

        public string Image => Resources.Icons.ResourceManager.GetString($"Filter{this.Name.Replace(" ", string.Empty)}");

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
