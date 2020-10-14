using Ferretto.Common.Controls.WPF;

namespace Ferretto.VW.App.Services
{
    public class TrayControlCompartment : IDrawableCompartment
    {
        #region Properties

        public string Barcode { get; set; }

        public double? Depth { get; set; }

        public int Id { get; set; }

        public int? LoadingUnitId { get; set; }

        public double? Width { get; set; }

        public double? XPosition { get; set; }

        public double? YPosition { get; set; }

        #endregion
    }
}
