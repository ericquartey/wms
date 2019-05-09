namespace Ferretto.Common.Controls.WPF
{
    public interface IDrawableCompartment
    {
        #region Properties

        double? Height { get; set; }

        int Id { get; set; }

        int? LoadingUnitId { get; set; }

        double? Width { get; set; }

        double? XPosition { get; set; }

        double? YPosition { get; set; }

        #endregion
    }
}
