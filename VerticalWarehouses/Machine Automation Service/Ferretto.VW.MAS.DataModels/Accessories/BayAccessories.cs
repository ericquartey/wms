namespace Ferretto.VW.MAS.DataModels
{
    public class BayAccessories : DataModel
    {
        #region Properties

        public AlphaNumericBar AlphaNumericBar { get; set; }

        public BarcodeReader BarcodeReader { get; set; }

        public CardReader CardReader { get; set; }

        public LabelPrinter LabelPrinter { get; set; }

        public LaserPointer LaserPointer { get; set; }

        public TokenReader TokenReader { get; set; }

        public WeightingScale WeightingScale { get; set; }

        #endregion
    }
}
