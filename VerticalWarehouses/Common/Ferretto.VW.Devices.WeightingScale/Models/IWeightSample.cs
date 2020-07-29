namespace Ferretto.VW.Devices.WeightingScale
{
    public interface IWeightSample
    {
        #region Properties

        float? AverageUnitWeight { get; }

        SampleQuality Quality { get; }

        int ScaleNumber { get; }

        float Tare { get; }

        string UnitOfMeasure { get; }

        int? UnitsCount { get; }

        float Weight { get; }

        #endregion
    }
}
