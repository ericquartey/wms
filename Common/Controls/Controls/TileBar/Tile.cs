namespace Ferretto.Common.Controls
{
    public class Tile : Prism.Mvvm.BindableBase
    {
        #region Fields

        private int? count;

        #endregion Fields

        #region Properties

        public int? Count
        {
            get => this.count;
            set => this.SetProperty(ref this.count, value);
        }

        public string Description { get; set; }

        public string Image => this.Key;

        public string Key { get; set; }

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
