using Ferretto.Common.Controls.WPF;

namespace Ferretto.VW.WmsCommunication.Source
{
    public class TrayControlCompartment : IDrawableCompartment
    {
        #region Properties

        public double? Height { get; set; }

        public int Id { get; set; }

        public int? LoadingUnitId { get; set; }

        public double? Width { get; set; }

        public double? XPosition { get; set; }

        public double? YPosition { get; set; }

        #endregion
    }
}
