namespace Ferretto.VW.Devices.WeightingScale
{
    internal class WeightSampleMinebea : IWeightSample
    {
        #region Fields

        private static readonly System.Text.RegularExpressions.Regex regexLong = new System.Text.RegularExpressions.Regex(
            @"(?<Type>[a-zA-Z\*].{5})(?<Weight>[\+\- ]?\s*\d+.\d*)\s*(?<UnitOfMeasure>\w)",
            System.Text.RegularExpressions.RegexOptions.Compiled);

        private static readonly System.Text.RegularExpressions.Regex regexShort = new System.Text.RegularExpressions.Regex(
            @"(?<Weight>[\+\- ]?\s*\d+.\d*)\s*(?<UnitOfMeasure>\w)",
            System.Text.RegularExpressions.RegexOptions.Compiled);

        #endregion

        #region Properties

        public float? AverageUnitWeight { get; set; }

        public SampleQuality Quality { get; set; }

        public int ScaleNumber { get; set; }

        public float Tare { get; set; }

        public string Type { get; set; }

        public string UnitOfMeasure { get; set; }

        public int? UnitsCount { get; set; }

        public float Weight { get; set; }

        #endregion

        #region Methods

        public static bool TryParse(string message, out WeightSampleMinebea sample)
        {
            var match = regexLong.Match(message);
            if (match.Success)
            {
                var type = match.Groups[nameof(Type)].Value;
                // remove spaces from weight string
                var weight = match.Groups[nameof(Weight)].Value.Replace(" ", "");
                sample = new WeightSampleMinebea()
                {
                    UnitOfMeasure = match.Groups[nameof(UnitOfMeasure)].Value,
                    Weight = float.Parse(weight, System.Globalization.CultureInfo.InvariantCulture),
                    Quality = SampleQuality.Stable
                };

                if (type.Contains("Qnt"))
                {
                    sample.UnitsCount = (int)sample.Weight;
                    sample.Weight = 0;
                }

                return true;
            }
            else
            {
                match = regexShort.Match(message);
                if (match.Success)
                {
                    // remove spaces from weight string
                    var weight = match.Groups[nameof(Weight)].Value.Replace(" ", "");
                    sample = new WeightSampleMinebea()
                    {
                        UnitOfMeasure = match.Groups[nameof(UnitOfMeasure)].Value,
                        Weight = float.Parse(weight, System.Globalization.CultureInfo.InvariantCulture),
                        Quality = SampleQuality.Stable
                    };
                    return true;
                }
            }

            sample = null;
            return false;
        }

        #endregion
    }
}
