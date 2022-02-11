namespace Ferretto.VW.Devices.WeightingScale
{
    internal class WeightSample : IWeightSample
    {
        #region Fields

        private static readonly System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(
            @"(?<ScaleNumber>\d+),(?<Quality>UL|OL|ST|US),\s*(?<Weight>-?\d+.\d+),\s*(?<Tare>-?\d+.\d+),\s*(?<UnitsCount>\d+),\s*(?<AverageUnitWeight>-?\d+.\d+),\s*(?<UnitOfMeasure>\w)",
            System.Text.RegularExpressions.RegexOptions.Compiled);

        #endregion

        #region Properties

        public float? AverageUnitWeight { get; set; }

        public SampleQuality Quality { get; set; }

        public int ScaleNumber { get; set; }

        public float Tare { get; set; }

        public string UnitOfMeasure { get; set; }

        public int? UnitsCount { get; set; }

        public int? UnitsCountRef { get; set; }

        public float Weight { get; set; }

        #endregion

        #region Methods

        public static SampleQuality ParseQualityString(string quality)
        {
            switch (quality)
            {
                case "UL": return SampleQuality.Underload;
                case "OL": return SampleQuality.Overload;
                case "ST": return SampleQuality.Stable;
                case "US": return SampleQuality.Unstable;
                default: return SampleQuality.Unknown;
            };
        }

        public static bool TryParse(string message, out WeightSample sample)
        {
            var match = regex.Match(message);
            if (match.Success)
            {
                sample = new WeightSample
                {
                    AverageUnitWeight = float.Parse(match.Groups[nameof(AverageUnitWeight)].Value, System.Globalization.CultureInfo.InvariantCulture),
                    ScaleNumber = int.Parse(match.Groups[nameof(ScaleNumber)].Value),
                    Tare = float.Parse(match.Groups[nameof(Tare)].Value, System.Globalization.CultureInfo.InvariantCulture),
                    UnitOfMeasure = match.Groups[nameof(UnitOfMeasure)].Value,
                    UnitsCount = int.Parse(match.Groups[nameof(UnitsCount)].Value),
                    Weight = float.Parse(match.Groups[nameof(Weight)].Value, System.Globalization.CultureInfo.InvariantCulture),
                    Quality = ParseQualityString(match.Groups[nameof(Quality)].Value)
                };

                if (sample.AverageUnitWeight == 0)
                {
                    sample.AverageUnitWeight = null;
                    sample.UnitsCount = null;
                }

                return true;
            }

            sample = null;
            return false;
        }

        #endregion
    }
}
