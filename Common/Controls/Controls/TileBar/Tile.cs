using Prism.Mvvm;

namespace Ferretto.Common.Controls
{
    public class Tile : BindableBase
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

        public string Image => this.Name != null ?
            $"Filter{this.Name.Replace(" ", string.Empty)}"
            : null;

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
